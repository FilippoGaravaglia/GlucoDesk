#!/usr/bin/env bash

set -euo pipefail

APP_NAME="GlucoDesk"

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
SOURCE_ICON="$ROOT_DIR/src/GlucoDesk.Desktop/Assets/AppIcon/glucodesk-app-icon.png"
OUTPUT_ICON="$ROOT_DIR/src/GlucoDesk.Desktop/Assets/AppIcon/glucodesk-app-icon.ico"
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

"$PYTHON_BIN" - "$SOURCE_ICON" "$OUTPUT_ICON" <<'PY'
from collections import deque
from pathlib import Path
from PIL import Image

import sys

source_icon = Path(sys.argv[1])
output_icon = Path(sys.argv[2])

sizes = [16, 24, 32, 48, 64, 128, 256]

# Keep the real logo large, but leave a small safe margin for Windows taskbar rendering.
target_fill_ratio = 0.92

# Only near-white pixels connected to the image border are treated as background.
# This avoids deleting intentional white parts inside the GlucoDesk "G" mark.
background_threshold = 245
alpha_threshold = 12

image = Image.open(source_icon).convert("RGBA")
width, height = image.size
pixels = image.load()

def is_background_candidate(x: int, y: int) -> bool:
    r, g, b, a = pixels[x, y]

    if a <= alpha_threshold:
        return True

    return r >= background_threshold and g >= background_threshold and b >= background_threshold

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

canvas.save(
    output_icon,
    format="ICO",
    sizes=[(size, size) for size in sizes],
)

print(f"created {output_icon}")
print(f"source size: {image.size}")
print(f"removed border/background pixels: {len(visited)}")
print(f"content bbox: {bbox}")
print(f"optimized logo size: {resized.size}")
PY

info "Windows .ico generated successfully"
echo "Icon:"
echo "  $OUTPUT_ICON"
