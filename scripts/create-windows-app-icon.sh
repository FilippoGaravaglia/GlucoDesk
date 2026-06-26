#!/usr/bin/env bash

set -euo pipefail

APP_NAME="GlucoDesk"

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
SOURCE_ICON="$ROOT_DIR/src/GlucoDesk.Desktop/Assets/AppIcon/glucodesk-app-icon.png"
OUTPUT_ICON="$ROOT_DIR/src/GlucoDesk.Desktop/Assets/AppIcon/glucodesk-app-icon.ico"
PREVIEW_ICON="$ROOT_DIR/artifacts/icon-generation/windows-icon-preview.png"
TOOLS_DIR="$ROOT_DIR/artifacts/icon-generation/windows-icon-tools"

fail() {
  echo "error: $*" >&2
  exit 1
}

info() {
  echo "==> $*"
}

require_command() {
  if ! command -v "$1" >/dev/null 2>&1; then
    fail "required command '$1' was not found"
  fi
}

if [[ "$(uname -s)" != "Darwin" ]]; then
  fail "Windows icon generation currently runs from macOS because it optimizes the existing artwork source"
fi

require_command python3

if [[ ! -f "$SOURCE_ICON" ]]; then
  fail "source icon not found: $SOURCE_ICON"
fi

PYTHON_BIN="python3"

if ! "$PYTHON_BIN" - <<'PY' >/dev/null 2>&1
from PIL import Image
PY
then
  info "Pillow not found, creating local icon-generation virtual environment"
  rm -rf "$TOOLS_DIR"
  python3 -m venv "$TOOLS_DIR"
  PYTHON_BIN="$TOOLS_DIR/bin/python"
  "$PYTHON_BIN" -m pip install --upgrade pip >/dev/null
  "$PYTHON_BIN" -m pip install pillow >/dev/null
fi

info "generating Windows-optimized .ico for $APP_NAME"

"$PYTHON_BIN" - "$SOURCE_ICON" "$OUTPUT_ICON" "$PREVIEW_ICON" <<'PY'
from collections import deque
from pathlib import Path
from PIL import Image

import colorsys
import math
import sys

source_icon = Path(sys.argv[1])
output_icon = Path(sys.argv[2])
preview_icon = Path(sys.argv[3])

sizes = [16, 24, 32, 48, 64, 128, 256]

# Large enough for Windows taskbar, but not so large that it touches edges.
target_fill_ratio = 0.96

alpha_threshold = 12
background_distance_threshold = 95
bright_background_threshold = 205
low_saturation_threshold = 0.30

image = Image.open(source_icon).convert("RGBA")
width, height = image.size
pixels = image.load()

sample_points = [
    (0, 0),
    (width - 1, 0),
    (0, height - 1),
    (width - 1, height - 1),
    (width // 2, 0),
    (width // 2, height - 1),
    (0, height // 2),
    (width - 1, height // 2),
]

background_samples = []

for x, y in sample_points:
    r, g, b, a = pixels[x, y]
    if a > alpha_threshold:
        background_samples.append((r, g, b))

if background_samples:
    background_color = tuple(
        sorted(channel_values)[len(channel_values) // 2]
        for channel_values in zip(*background_samples)
    )
else:
    background_color = (255, 255, 255)

def color_distance(c1, c2):
    return math.sqrt(
        (c1[0] - c2[0]) ** 2 +
        (c1[1] - c2[1]) ** 2 +
        (c1[2] - c2[2]) ** 2
    )

def is_bright_low_saturation(r, g, b):
    h, s, v = colorsys.rgb_to_hsv(r / 255, g / 255, b / 255)
    return v >= bright_background_threshold / 255 and s <= low_saturation_threshold

def is_background_candidate(x: int, y: int) -> bool:
    r, g, b, a = pixels[x, y]

    if a <= alpha_threshold:
        return True

    if color_distance((r, g, b), background_color) <= background_distance_threshold:
        return True

    if is_bright_low_saturation(r, g, b):
        return True

    return False

visited = set()
queue = deque()

for x in range(width):
    queue.append((x, 0))
    queue.append((x, height - 1))

for y in range(height):
    queue.append((0, y))
    queue.append((width - 1, y))

while queue:
    x, y = queue.popleft()

    if x < 0 or y < 0 or x >= width or y >= height:
        continue

    if (x, y) in visited:
        continue

    if not is_background_candidate(x, y):
        continue

    visited.add((x, y))

    r, g, b, _a = pixels[x, y]
    pixels[x, y] = (r, g, b, 0)

    queue.append((x - 1, y))
    queue.append((x + 1, y))
    queue.append((x, y - 1))
    queue.append((x, y + 1))

# Clean remaining antialias fringe directly touching transparent background.
for _ in range(3):
    to_clear = []

    for y in range(height):
        for x in range(width):
            r, g, b, a = pixels[x, y]

            if a <= alpha_threshold:
                continue

            if not is_background_candidate(x, y):
                continue

            has_transparent_neighbor = False

            for nx, ny in ((x - 1, y), (x + 1, y), (x, y - 1), (x, y + 1)):
                if 0 <= nx < width and 0 <= ny < height:
                    if pixels[nx, ny][3] <= alpha_threshold:
                        has_transparent_neighbor = True
                        break

            if has_transparent_neighbor:
                to_clear.append((x, y))

    if not to_clear:
        break

    for x, y in to_clear:
        r, g, b, _a = pixels[x, y]
        pixels[x, y] = (r, g, b, 0)

bbox = image.getbbox()

if bbox is None:
    raise SystemExit("source icon appears empty after background cleanup")

cropped = image.crop(bbox)

base_size = 256
max_logo_size = round(base_size * target_fill_ratio)
scale = max_logo_size / max(cropped.width, cropped.height)

resized = cropped.resize(
    (
        max(1, round(cropped.width * scale)),
        max(1, round(cropped.height * scale)),
    ),
    Image.Resampling.LANCZOS,
)

canvas = Image.new("RGBA", (base_size, base_size), (0, 0, 0, 0))
canvas.alpha_composite(
    resized,
    (
        (base_size - resized.width) // 2,
        (base_size - resized.height) // 2,
    ),
)

output_icon.parent.mkdir(parents=True, exist_ok=True)
preview_icon.parent.mkdir(parents=True, exist_ok=True)

canvas.save(preview_icon, format="PNG")

canvas.save(
    output_icon,
    format="ICO",
    sizes=[(size, size) for size in sizes],
)

print(f"created {output_icon}")
print(f"preview {preview_icon}")
print(f"source size: {image.size}")
print(f"background color: {background_color}")
print(f"removed border/background pixels: {len(visited)}")
print(f"content bbox: {bbox}")
print(f"optimized logo size: {resized.size}")
PY

info "Windows .ico generated successfully"
echo "Icon:"
echo "  $OUTPUT_ICON"
echo "Preview:"
echo "  $PREVIEW_ICON"
