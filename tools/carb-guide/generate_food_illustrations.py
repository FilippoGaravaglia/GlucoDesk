#!/usr/bin/env python3

from __future__ import annotations

import math
import struct
import zlib
from pathlib import Path
from typing import Iterable

WIDTH = 800
HEIGHT = 420

OUTPUT_DIRECTORY = Path(
    "src/GlucoDesk.Desktop/Assets/CarbGuide"
)

Color = tuple[int, int, int, int]
Point = tuple[float, float]


def create_canvas(color: Color) -> bytearray:
    pixels = bytearray(WIDTH * HEIGHT * 4)

    for y in range(HEIGHT):
        for x in range(WIDTH):
            index = (y * WIDTH + x) * 4
            pixels[index:index + 4] = bytes(color)

    return pixels


def blend_pixel(
    pixels: bytearray,
    x: int,
    y: int,
    color: Color,
) -> None:
    if x < 0 or y < 0 or x >= WIDTH or y >= HEIGHT:
        return

    index = (y * WIDTH + x) * 4
    source_alpha = color[3] / 255.0
    destination_alpha = pixels[index + 3] / 255.0

    output_alpha = (
        source_alpha
        + destination_alpha * (1.0 - source_alpha)
    )

    if output_alpha <= 0:
        pixels[index:index + 4] = b"\x00\x00\x00\x00"
        return

    for channel in range(3):
        source = color[channel]
        destination = pixels[index + channel]

        output = (
            source * source_alpha
            + destination
            * destination_alpha
            * (1.0 - source_alpha)
        ) / output_alpha

        pixels[index + channel] = max(
            0,
            min(255, round(output)),
        )

    pixels[index + 3] = max(
        0,
        min(255, round(output_alpha * 255)),
    )


def draw_circle(
    pixels: bytearray,
    center_x: float,
    center_y: float,
    radius: float,
    color: Color,
) -> None:
    minimum_x = max(0, int(center_x - radius))
    maximum_x = min(WIDTH - 1, int(center_x + radius))
    minimum_y = max(0, int(center_y - radius))
    maximum_y = min(HEIGHT - 1, int(center_y + radius))

    squared_radius = radius * radius

    for y in range(minimum_y, maximum_y + 1):
        for x in range(minimum_x, maximum_x + 1):
            dx = x - center_x
            dy = y - center_y

            if dx * dx + dy * dy <= squared_radius:
                blend_pixel(pixels, x, y, color)


def draw_ellipse(
    pixels: bytearray,
    center_x: float,
    center_y: float,
    radius_x: float,
    radius_y: float,
    color: Color,
) -> None:
    minimum_x = max(0, int(center_x - radius_x))
    maximum_x = min(WIDTH - 1, int(center_x + radius_x))
    minimum_y = max(0, int(center_y - radius_y))
    maximum_y = min(HEIGHT - 1, int(center_y + radius_y))

    for y in range(minimum_y, maximum_y + 1):
        for x in range(minimum_x, maximum_x + 1):
            normalized_x = (x - center_x) / radius_x
            normalized_y = (y - center_y) / radius_y

            if (
                normalized_x * normalized_x
                + normalized_y * normalized_y
                <= 1.0
            ):
                blend_pixel(pixels, x, y, color)


def point_inside_polygon(
    x: float,
    y: float,
    points: list[Point],
) -> bool:
    inside = False
    previous_index = len(points) - 1

    for current_index, current in enumerate(points):
        previous = points[previous_index]

        intersects = (
            (current[1] > y) != (previous[1] > y)
            and x
            < (
                (previous[0] - current[0])
                * (y - current[1])
                / (
                    previous[1]
                    - current[1]
                    + 0.000001
                )
                + current[0]
            )
        )

        if intersects:
            inside = not inside

        previous_index = current_index

    return inside


def draw_polygon(
    pixels: bytearray,
    points: Iterable[Point],
    color: Color,
) -> None:
    polygon = list(points)

    minimum_x = max(0, int(min(x for x, _ in polygon)))
    maximum_x = min(
        WIDTH - 1,
        int(max(x for x, _ in polygon)),
    )
    minimum_y = max(0, int(min(y for _, y in polygon)))
    maximum_y = min(
        HEIGHT - 1,
        int(max(y for _, y in polygon)),
    )

    for y in range(minimum_y, maximum_y + 1):
        for x in range(minimum_x, maximum_x + 1):
            if point_inside_polygon(x, y, polygon):
                blend_pixel(pixels, x, y, color)


def draw_thick_line(
    pixels: bytearray,
    start: Point,
    end: Point,
    thickness: float,
    color: Color,
) -> None:
    dx = end[0] - start[0]
    dy = end[1] - start[1]
    distance = max(1.0, math.sqrt(dx * dx + dy * dy))
    steps = max(1, int(distance))

    for step in range(steps + 1):
        ratio = step / steps
        x = start[0] + dx * ratio
        y = start[1] + dy * ratio

        draw_circle(
            pixels,
            x,
            y,
            thickness / 2.0,
            color,
        )


def draw_plate(
    pixels: bytearray,
    center_x: float = 400,
    center_y: float = 235,
    radius_x: float = 245,
    radius_y: float = 112,
) -> None:
    draw_ellipse(
        pixels,
        center_x,
        center_y + 14,
        radius_x + 14,
        radius_y + 10,
        (27, 79, 119, 24),
    )

    draw_ellipse(
        pixels,
        center_x,
        center_y,
        radius_x,
        radius_y,
        (255, 255, 255, 255),
    )

    draw_ellipse(
        pixels,
        center_x,
        center_y,
        radius_x - 24,
        radius_y - 18,
        (235, 246, 255, 255),
    )


def add_background_decoration(
    pixels: bytearray,
    accent: Color,
) -> None:
    draw_circle(
        pixels,
        80,
        70,
        84,
        (
            accent[0],
            accent[1],
            accent[2],
            38,
        ),
    )

    draw_circle(
        pixels,
        720,
        350,
        105,
        (
            accent[0],
            accent[1],
            accent[2],
            28,
        ),
    )


def save_png(
    path: Path,
    pixels: bytearray,
) -> None:
    scanlines = bytearray()

    stride = WIDTH * 4

    for y in range(HEIGHT):
        scanlines.append(0)
        row_start = y * stride
        scanlines.extend(
            pixels[row_start:row_start + stride]
        )

    def chunk(
        chunk_type: bytes,
        data: bytes,
    ) -> bytes:
        payload = chunk_type + data

        return (
            struct.pack(">I", len(data))
            + payload
            + struct.pack(
                ">I",
                zlib.crc32(payload) & 0xFFFFFFFF,
            )
        )

    png = bytearray(b"\x89PNG\r\n\x1a\n")

    png.extend(
        chunk(
            b"IHDR",
            struct.pack(
                ">IIBBBBB",
                WIDTH,
                HEIGHT,
                8,
                6,
                0,
                0,
                0,
            ),
        )
    )

    png.extend(
        chunk(
            b"IDAT",
            zlib.compress(bytes(scanlines), level=9),
        )
    )

    png.extend(chunk(b"IEND", b""))

    path.write_bytes(png)


def prosciutto_melone() -> bytearray:
    pixels = create_canvas((239, 248, 255, 255))
    add_background_decoration(
        pixels,
        (245, 165, 70, 255),
    )
    draw_plate(pixels)

    for offset in (-115, -45, 25, 95):
        draw_ellipse(
            pixels,
            420 + offset,
            258 + abs(offset) * 0.08,
            58,
            25,
            (207, 94, 93, 255),
        )

        draw_thick_line(
            pixels,
            (380 + offset, 255),
            (458 + offset, 268),
            5,
            (246, 188, 179, 235),
        )

    draw_polygon(
        pixels,
        [
            (275, 175),
            (365, 120),
            (455, 170),
            (366, 227),
        ],
        (250, 183, 62, 255),
    )

    draw_polygon(
        pixels,
        [
            (296, 174),
            (365, 137),
            (431, 172),
            (366, 209),
        ],
        (255, 220, 108, 255),
    )

    return pixels


def affettato_misto() -> bytearray:
    pixels = create_canvas((239, 248, 255, 255))
    add_background_decoration(
        pixels,
        (194, 77, 88, 255),
    )
    draw_plate(pixels)

    colors = [
        (198, 67, 72, 255),
        (224, 112, 102, 255),
        (171, 72, 65, 255),
        (232, 137, 122, 255),
    ]

    positions = [
        (300, 215, -0.18),
        (390, 190, 0.12),
        (485, 220, -0.10),
        (360, 275, 0.20),
        (465, 280, -0.16),
    ]

    for index, (x, y, rotation) in enumerate(positions):
        _ = rotation

        draw_ellipse(
            pixels,
            x,
            y,
            76,
            31,
            colors[index % len(colors)],
        )

        draw_thick_line(
            pixels,
            (x - 42, y - 5),
            (x + 42, y + 7),
            4,
            (248, 203, 193, 220),
        )

    return pixels


def crostini() -> bytearray:
    pixels = create_canvas((239, 248, 255, 255))
    add_background_decoration(
        pixels,
        (216, 147, 50, 255),
    )
    draw_plate(pixels)

    positions = [
        (320, 210),
        (430, 195),
        (380, 285),
        (500, 265),
    ]

    for x, y in positions:
        draw_polygon(
            pixels,
            [
                (x - 50, y - 30),
                (x + 43, y - 24),
                (x + 53, y + 30),
                (x - 43, y + 34),
            ],
            (224, 174, 96, 255),
        )

        draw_polygon(
            pixels,
            [
                (x - 38, y - 21),
                (x + 32, y - 17),
                (x + 39, y + 22),
                (x - 32, y + 25),
            ],
            (251, 218, 148, 255),
        )

        draw_circle(
            pixels,
            x + 5,
            y,
            21,
            (111, 72, 41, 255),
        )

        draw_circle(
            pixels,
            x - 17,
            y + 8,
            13,
            (137, 91, 46, 255),
        )

    return pixels


def riso_parboiled() -> bytearray:
    pixels = create_canvas((239, 248, 255, 255))
    add_background_decoration(
        pixels,
        (208, 176, 84, 255),
    )
    draw_plate(pixels)

    draw_ellipse(
        pixels,
        400,
        230,
        175,
        78,
        (245, 229, 176, 255),
    )

    for row in range(8):
        for column in range(16):
            x = (
                270
                + column * 17
                + (row % 2) * 7
            )
            y = 185 + row * 13

            draw_ellipse(
                pixels,
                x,
                y,
                8,
                3,
                (
                    224 + (column % 3) * 4,
                    204 + (row % 3) * 4,
                    143,
                    255,
                ),
            )

    return pixels


def spaghetti() -> bytearray:
    pixels = create_canvas((239, 248, 255, 255))
    add_background_decoration(
        pixels,
        (224, 171, 52, 255),
    )
    draw_plate(pixels)

    for index in range(46):
        angle = index * 0.43
        radius = 45 + (index % 9) * 10

        start_x = 400 + math.cos(angle) * radius
        start_y = 235 + math.sin(angle) * radius * 0.42

        end_x = 400 + math.cos(angle + 1.7) * radius
        end_y = 235 + math.sin(angle + 1.7) * radius * 0.42

        draw_thick_line(
            pixels,
            (start_x, start_y),
            (end_x, end_y),
            5,
            (231, 184, 68, 255),
        )

    draw_circle(
        pixels,
        400,
        232,
        42,
        (246, 210, 106, 190),
    )

    return pixels


def tortelloni() -> bytearray:
    pixels = create_canvas((239, 248, 255, 255))
    add_background_decoration(
        pixels,
        (103, 169, 111, 255),
    )
    draw_plate(pixels)

    positions = [
        (310, 205),
        (400, 185),
        (490, 210),
        (345, 275),
        (445, 275),
    ]

    for x, y in positions:
        draw_polygon(
            pixels,
            [
                (x - 50, y),
                (x - 18, y - 38),
                (x + 40, y - 20),
                (x + 48, y + 25),
                (x - 15, y + 38),
            ],
            (229, 204, 132, 255),
        )

        draw_circle(
            pixels,
            x,
            y,
            22,
            (242, 224, 167, 255),
        )

        draw_thick_line(
            pixels,
            (x - 39, y),
            (x + 37, y + 2),
            3,
            (176, 147, 82, 180),
        )

    return pixels


def gnocchi_pomodoro() -> bytearray:
    pixels = create_canvas((239, 248, 255, 255))
    add_background_decoration(
        pixels,
        (225, 86, 67, 255),
    )
    draw_plate(pixels)

    positions = [
        (300, 205),
        (355, 184),
        (415, 198),
        (475, 183),
        (520, 222),
        (325, 255),
        (385, 250),
        (450, 260),
        (500, 280),
        (365, 300),
        (430, 305),
    ]

    for index, (x, y) in enumerate(positions):
        draw_ellipse(
            pixels,
            x,
            y,
            34,
            23,
            (
                225 + index % 2 * 8,
                113,
                73,
                255,
            ),
        )

        draw_thick_line(
            pixels,
            (x - 18, y - 2),
            (x + 17, y + 3),
            3,
            (246, 166, 116, 210),
        )

    return pixels


def main() -> None:
    OUTPUT_DIRECTORY.mkdir(
        parents=True,
        exist_ok=True,
    )

    illustrations = {
        "prosciutto-melone.png": prosciutto_melone,
        "affettato-misto.png": affettato_misto,
        "crostini.png": crostini,
        "riso-parboiled.png": riso_parboiled,
        "spaghetti.png": spaghetti,
        "tortelloni-ricotta-spinaci.png": tortelloni,
        "gnocchi-pomodoro.png": gnocchi_pomodoro,
    }

    for file_name, generator in illustrations.items():
        output_path = OUTPUT_DIRECTORY / file_name
        save_png(output_path, generator())

        print(
            f"Generated {output_path} "
            f"({output_path.stat().st_size} bytes)"
        )


if __name__ == "__main__":
    main()
