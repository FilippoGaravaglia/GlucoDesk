#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
INPUT_ICON="$ROOT_DIR/src/GlucoDesk.Desktop/Assets/MenuBar/glucodesk-menubar-icon.png"
OUTPUT_ICON="$INPUT_ICON"
PREVIEW_ICON="$ROOT_DIR/artifacts/icon-generation/macos-menubar-icon-preview.png"

CANVAS_SIZE="${1:-64}"
CONTENT_SCALE="${2:-0.92}"

info() {
  echo "==> $*"
}

fail() {
  echo "error: $*" >&2
  exit 1
}

if [[ ! -f "$INPUT_ICON" ]]; then
  fail "input icon not found: $INPUT_ICON"
fi

if ! command -v python3 >/dev/null 2>&1; then
  fail "python3 not found"
fi

TEMP_VENV="$ROOT_DIR/artifacts/icon-generation/.venv-menubar-icon"

if ! python3 - <<'PY' >/dev/null 2>&1
import PIL
PY
then
  info "Pillow not found, creating local virtual environment"
  python3 -m venv "$TEMP_VENV"
  # shellcheck disable=SC1091
  source "$TEMP_VENV/bin/activate"
  pip install --quiet Pillow
else
  if [[ -d "$TEMP_VENV" ]]; then
    # shellcheck disable=SC1091
    source "$TEMP_VENV/bin/activate"
  fi
fi

info "resizing macOS menu bar icon"
python3 - <<PY
from pathlib import Path
from PIL import Image

input_icon = Path(r"$INPUT_ICON")
output_icon = Path(r"$OUTPUT_ICON")
preview_icon = Path(r"$PREVIEW_ICON")

canvas_size = int("$CANVAS_SIZE")
content_scale = float("$CONTENT_SCALE")

image = Image.open(input_icon).convert("RGBA")
alpha = image.getchannel("A")
bbox = alpha.getbbox()

if bbox is None:
    raise SystemExit(f"Image has no visible alpha content: {input_icon}")

cropped = image.crop(bbox)

target_side = max(1, int(round(canvas_size * content_scale)))
ratio = min(target_side / cropped.width, target_side / cropped.height)

new_width = max(1, int(round(cropped.width * ratio)))
new_height = max(1, int(round(cropped.height * ratio)))

resized = cropped.resize((new_width, new_height), Image.LANCZOS)

canvas = Image.new("RGBA", (canvas_size, canvas_size), (0, 0, 0, 0))
offset_x = (canvas_size - new_width) // 2
offset_y = (canvas_size - new_height) // 2
canvas.paste(resized, (offset_x, offset_y), resized)

output_icon.parent.mkdir(parents=True, exist_ok=True)
preview_icon.parent.mkdir(parents=True, exist_ok=True)

canvas.save(output_icon, format="PNG")
canvas.save(preview_icon, format="PNG")

print(f"updated: {output_icon}")
print(f"preview: {preview_icon}")
print(f"original size: {image.size}")
print(f"content bbox: {bbox}")
print(f"canvas size: {canvas_size}x{canvas_size}")
print(f"content scale: {content_scale}")
print(f"final logo size: {new_width}x{new_height}")
PY

info "done"
echo "Icon:"
echo "  $OUTPUT_ICON"
echo "Preview:"
echo "  $PREVIEW_ICON"
