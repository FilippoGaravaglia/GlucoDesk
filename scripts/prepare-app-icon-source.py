#!/usr/bin/env python3
from __future__ import annotations

import argparse
import binascii
import struct
import sys
import zlib
from collections import deque
from pathlib import Path


PNG_SIGNATURE = b"\x89PNG\r\n\x1a\n"


def main() -> int:
    parser = argparse.ArgumentParser(
        description="Prepare a transparent 1024x1024 macOS app icon source image."
    )
    parser.add_argument("input", type=Path)
    parser.add_argument("output", type=Path)
    parser.add_argument("--canvas-size", type=int, default=1024)
    parser.add_argument("--icon-size", type=int, default=860)
    parser.add_argument("--background-threshold", type=int, default=225)

    args = parser.parse_args()

    width, height, pixels = read_png_rgba(args.input)

    pixels = remove_border_connected_light_background(
        pixels,
        width,
        height,
        args.background_threshold,
    )

    bbox = find_alpha_bbox(pixels, width, height)
    if bbox is None:
        print("Input image does not contain visible pixels.", file=sys.stderr)
        return 1

    prepared = fit_visible_pixels_on_transparent_canvas(
        pixels,
        width,
        height,
        bbox,
        args.canvas_size,
        args.icon_size,
    )

    args.output.parent.mkdir(parents=True, exist_ok=True)
    write_png_rgba(args.output, args.canvas_size, args.canvas_size, prepared)

    print("Prepared app icon source:")
    print(args.output)
    print(f"Canvas: {args.canvas_size}x{args.canvas_size}")
    print(f"Visible icon target size: {args.icon_size}px")

    return 0


def read_png_rgba(path: Path) -> tuple[int, int, bytearray]:
    data = path.read_bytes()

    if not data.startswith(PNG_SIGNATURE):
        raise ValueError(f"Not a PNG file: {path}")

    offset = len(PNG_SIGNATURE)
    width = 0
    height = 0
    bit_depth = 0
    color_type = 0
    idat = bytearray()

    while offset < len(data):
        length = struct.unpack(">I", data[offset:offset + 4])[0]
        offset += 4

        chunk_type = data[offset:offset + 4]
        offset += 4

        chunk_data = data[offset:offset + length]
        offset += length

        offset += 4

        if chunk_type == b"IHDR":
            width, height, bit_depth, color_type, compression, filter_method, interlace = struct.unpack(
                ">IIBBBBB",
                chunk_data,
            )

            if bit_depth != 8:
                raise ValueError("Only 8-bit PNG files are supported.")

            if color_type not in (2, 6):
                raise ValueError("Only RGB and RGBA PNG files are supported.")

            if compression != 0 or filter_method != 0 or interlace != 0:
                raise ValueError("Unsupported PNG encoding options.")

        elif chunk_type == b"IDAT":
            idat.extend(chunk_data)

        elif chunk_type == b"IEND":
            break

    if width <= 0 or height <= 0:
        raise ValueError("Invalid PNG dimensions.")

    channels = 4 if color_type == 6 else 3
    raw = zlib.decompress(bytes(idat))
    stride = width * channels

    rows: list[bytearray] = []
    cursor = 0

    for _ in range(height):
        filter_type = raw[cursor]
        cursor += 1

        row = bytearray(raw[cursor:cursor + stride])
        cursor += stride

        previous = rows[-1] if rows else bytearray(stride)
        unfilter_row(row, previous, channels, filter_type)

        rows.append(row)

    rgba = bytearray(width * height * 4)

    for y, row in enumerate(rows):
        for x in range(width):
            source_index = x * channels
            target_index = ((y * width) + x) * 4

            rgba[target_index] = row[source_index]
            rgba[target_index + 1] = row[source_index + 1]
            rgba[target_index + 2] = row[source_index + 2]
            rgba[target_index + 3] = row[source_index + 3] if channels == 4 else 255

    return width, height, rgba


def unfilter_row(
    row: bytearray,
    previous: bytearray,
    bytes_per_pixel: int,
    filter_type: int,
) -> None:
    for index in range(len(row)):
        left = row[index - bytes_per_pixel] if index >= bytes_per_pixel else 0
        up = previous[index]
        upper_left = previous[index - bytes_per_pixel] if index >= bytes_per_pixel else 0

        if filter_type == 0:
            value = row[index]
        elif filter_type == 1:
            value = row[index] + left
        elif filter_type == 2:
            value = row[index] + up
        elif filter_type == 3:
            value = row[index] + ((left + up) // 2)
        elif filter_type == 4:
            value = row[index] + paeth_predictor(left, up, upper_left)
        else:
            raise ValueError(f"Unsupported PNG filter type: {filter_type}")

        row[index] = value & 0xFF


def paeth_predictor(left: int, up: int, upper_left: int) -> int:
    estimate = left + up - upper_left
    left_distance = abs(estimate - left)
    up_distance = abs(estimate - up)
    upper_left_distance = abs(estimate - upper_left)

    if left_distance <= up_distance and left_distance <= upper_left_distance:
        return left

    if up_distance <= upper_left_distance:
        return up

    return upper_left


def remove_border_connected_light_background(
    pixels: bytearray,
    width: int,
    height: int,
    threshold: int,
) -> bytearray:
    result = bytearray(pixels)
    visited = bytearray(width * height)
    queue: deque[tuple[int, int]] = deque()

    for x in range(width):
        enqueue_if_background(result, visited, queue, width, x, 0, threshold)
        enqueue_if_background(result, visited, queue, width, x, height - 1, threshold)

    for y in range(height):
        enqueue_if_background(result, visited, queue, width, 0, y, threshold)
        enqueue_if_background(result, visited, queue, width, width - 1, y, threshold)

    while queue:
        x, y = queue.popleft()
        pixel_index = ((y * width) + x) * 4
        result[pixel_index + 3] = 0

        for next_x, next_y in (
            (x - 1, y),
            (x + 1, y),
            (x, y - 1),
            (x, y + 1),
        ):
            if 0 <= next_x < width and 0 <= next_y < height:
                enqueue_if_background(
                    result,
                    visited,
                    queue,
                    width,
                    next_x,
                    next_y,
                    threshold,
                )

    return result


def enqueue_if_background(
    pixels: bytearray,
    visited: bytearray,
    queue: deque[tuple[int, int]],
    width: int,
    x: int,
    y: int,
    threshold: int,
) -> None:
    visit_index = (y * width) + x
    if visited[visit_index]:
        return

    visited[visit_index] = 1

    if is_light_background_pixel(pixels, width, x, y, threshold):
        queue.append((x, y))


def is_light_background_pixel(
    pixels: bytearray,
    width: int,
    x: int,
    y: int,
    threshold: int,
) -> bool:
    index = ((y * width) + x) * 4

    red = pixels[index]
    green = pixels[index + 1]
    blue = pixels[index + 2]
    alpha = pixels[index + 3]

    if alpha <= 8:
        return True

    minimum = min(red, green, blue)
    maximum = max(red, green, blue)

    return minimum >= threshold and maximum - minimum <= 40


def find_alpha_bbox(
    pixels: bytearray,
    width: int,
    height: int,
) -> tuple[int, int, int, int] | None:
    min_x = width
    min_y = height
    max_x = -1
    max_y = -1

    for y in range(height):
        for x in range(width):
            alpha = pixels[((y * width) + x) * 4 + 3]
            if alpha <= 8:
                continue

            min_x = min(min_x, x)
            min_y = min(min_y, y)
            max_x = max(max_x, x)
            max_y = max(max_y, y)

    if max_x < min_x or max_y < min_y:
        return None

    return min_x, min_y, max_x, max_y


def fit_visible_pixels_on_transparent_canvas(
    pixels: bytearray,
    source_width: int,
    source_height: int,
    bbox: tuple[int, int, int, int],
    canvas_size: int,
    icon_size: int,
) -> bytearray:
    min_x, min_y, max_x, max_y = bbox

    crop_width = max_x - min_x + 1
    crop_height = max_y - min_y + 1

    scale = min(icon_size / crop_width, icon_size / crop_height)

    resized_width = max(1, round(crop_width * scale))
    resized_height = max(1, round(crop_height * scale))

    output = bytearray(canvas_size * canvas_size * 4)

    offset_x = (canvas_size - resized_width) // 2
    offset_y = (canvas_size - resized_height) // 2

    for y in range(resized_height):
        source_y = min_y + ((y + 0.5) / scale) - 0.5

        for x in range(resized_width):
            source_x = min_x + ((x + 0.5) / scale) - 0.5
            red, green, blue, alpha = sample_bilinear_rgba(
                pixels,
                source_width,
                source_height,
                source_x,
                source_y,
            )

            target_x = offset_x + x
            target_y = offset_y + y
            target_index = ((target_y * canvas_size) + target_x) * 4

            output[target_index] = red
            output[target_index + 1] = green
            output[target_index + 2] = blue
            output[target_index + 3] = alpha

    return output


def sample_bilinear_rgba(
    pixels: bytearray,
    width: int,
    height: int,
    x: float,
    y: float,
) -> tuple[int, int, int, int]:
    x0 = clamp_int(int(x), 0, width - 1)
    y0 = clamp_int(int(y), 0, height - 1)
    x1 = clamp_int(x0 + 1, 0, width - 1)
    y1 = clamp_int(y0 + 1, 0, height - 1)

    fx = x - int(x)
    fy = y - int(y)

    samples = [
        (x0, y0, (1 - fx) * (1 - fy)),
        (x1, y0, fx * (1 - fy)),
        (x0, y1, (1 - fx) * fy),
        (x1, y1, fx * fy),
    ]

    weighted_alpha = 0.0
    weighted_red = 0.0
    weighted_green = 0.0
    weighted_blue = 0.0

    for sample_x, sample_y, weight in samples:
        index = ((sample_y * width) + sample_x) * 4
        red = pixels[index]
        green = pixels[index + 1]
        blue = pixels[index + 2]
        alpha = pixels[index + 3] / 255.0

        weighted_alpha += alpha * weight
        weighted_red += red * alpha * weight
        weighted_green += green * alpha * weight
        weighted_blue += blue * alpha * weight

    if weighted_alpha <= 0:
        return 0, 0, 0, 0

    red = round(weighted_red / weighted_alpha)
    green = round(weighted_green / weighted_alpha)
    blue = round(weighted_blue / weighted_alpha)
    alpha = round(weighted_alpha * 255)

    return (
        clamp_int(red, 0, 255),
        clamp_int(green, 0, 255),
        clamp_int(blue, 0, 255),
        clamp_int(alpha, 0, 255),
    )


def clamp_int(value: int, minimum: int, maximum: int) -> int:
    return max(minimum, min(maximum, value))


def write_png_rgba(path: Path, width: int, height: int, pixels: bytearray) -> None:
    raw = bytearray()

    for y in range(height):
        raw.append(0)
        start = y * width * 4
        end = start + (width * 4)
        raw.extend(pixels[start:end])

    png = bytearray(PNG_SIGNATURE)
    png.extend(make_chunk(b"IHDR", struct.pack(">IIBBBBB", width, height, 8, 6, 0, 0, 0)))
    png.extend(make_chunk(b"IDAT", zlib.compress(bytes(raw), level=9)))
    png.extend(make_chunk(b"IEND", b""))

    path.write_bytes(bytes(png))


def make_chunk(chunk_type: bytes, data: bytes) -> bytes:
    checksum = binascii.crc32(chunk_type)
    checksum = binascii.crc32(data, checksum) & 0xFFFFFFFF

    return (
        struct.pack(">I", len(data))
        + chunk_type
        + data
        + struct.pack(">I", checksum)
    )


if __name__ == "__main__":
    raise SystemExit(main())
