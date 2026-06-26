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
  fail "Windows icon generation currently runs from macOS because it optimizes the existing macOS artwork source"
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
from pathlib import Path
from PIL import Image

import sys

source_icon = Path(sys.argv[1])
output_icon = Path(sys.argv[2])

sizes = [16, 24, 32, 48, 64, 128, 256]
target_fill_ratio = 0.88
white_threshold = 245
alpha_threshold = 12

image = Image.open(source_icon).convert("RGBA")
pixels = image.load()

width, height = image.size

# Convert white / near-white background pixels to transparent.
# This removes the white square that looks bad on the Windows taskbar.
for y in range(height):
    for x in range(width):
        r, g, b, a = pixels[x, y]

        if a <= alpha_threshold:
            pixels[x, y] = (r, g, b, 0)
            continue

        if r >= white_threshold and g >= white_threshold and b >= white_threshold:
            pixels[x, y] = (r, g, b, 0)

bbox = image.getbbox()

if bbox is None:
    raise SystemExit("source icon appears empty after background cleanup")

cropped = image.crop(bbox)

# Add a tiny transparent padding before resizing, so the logo does not touch edges.
padding = max(2, round(max(cropped.size) * 0.04))
padded = Image.new(
    "RGBA",
    (cropped.width + padding * 2, cropped.height + padding * 2),
    (0, 0, 0, 0),
)
padded.alpha_composite(cropped, (padding, padding))

base_size = 256
max_logo_size = round(base_size * target_fill_ratio)
scale = max_logo_size / max(padded.width, padded.height)

resized = padded.resize(
    (
        max(1, round(padded.width * scale)),
        max(1, round(padded.height * scale)),
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
print(f"content bbox: {bbox}")
print(f"optimized logo size: {resized.size}")
PY

info "Windows .ico generated successfully"
echo "Icon:"
echo "  $OUTPUT_ICON"
