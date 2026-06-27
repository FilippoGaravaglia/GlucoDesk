#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

SOURCE_ICON="$ROOT_DIR/src/GlucoDesk.Desktop/Assets/MenuBar/glucodesk-menubar-icon.png"
OUTPUT_ICON="$ROOT_DIR/src/GlucoDesk.Desktop/Assets/AppIcon/glucodesk-windows-tray-icon.png"
PREVIEW_ICON="$ROOT_DIR/artifacts/icon-generation/windows-tray-icon-preview.png"

CANVAS_SIZE="${1:-64}"
CONTENT_SCALE="${2:-0.92}"

info() {
  echo "==> $*"
}

fail() {
  echo "error: $*" >&2
  exit 1
}

if [[ ! -f "$SOURCE_ICON" ]]; then
  fail "source macOS menu bar icon not found: $SOURCE_ICON"
fi

if ! command -v python3 >/dev/null 2>&1; then
  fail "python3 not found"
fi

VENV_DIR="$ROOT_DIR/artifacts/icon-generation/.venv-tray-icon"

if ! python3 - <<'PY' >/dev/null 2>&1
import PIL
PY
then
  info "Pillow not found, creating local virtual environment"
  python3 -m venv "$VENV_DIR"
  # shellcheck disable=SC1091
  source "$VENV_DIR/bin/activate"
  pip install --quiet Pillow
else
  if [[ -d "$VENV_DIR" ]]; then
    # shellcheck disable=SC1091
    source "$VENV_DIR/bin/activate"
  fi
fi

info "creating high-quality Windows tray icon from macOS menu bar icon"

python3 - <<PY
from pathlib import Path
from PIL import Image

source_icon = Path(r"$SOURCE_ICON")
output_icon = Path(r"$OUTPUT_ICON")
preview_icon = Path(r"$PREVIEW_ICON")

canvas_size = int("$CANVAS_SIZE")
content_scale = float("$CONTENT_SCALE")

if not 0.1 <= content_scale <= 1.0:
    raise SystemExit(f"content scale must be between 0.1 and 1.0, got {content_scale}")

image = Image.open(source_icon).convert("RGBA")
alpha = image.getchannel("A")
bbox = alpha.getbbox()

if bbox is None:
    raise SystemExit(f"source icon has no visible alpha content: {source_icon}")

cropped = image.crop(bbox)

target_side = max(1, int(round(canvas_size * content_scale)))
scale = min(target_side / cropped.width, target_side / cropped.height)

new_width = max(1, int(round(cropped.width * scale)))
new_height = max(1, int(round(cropped.height * scale)))

resized = cropped.resize((new_width, new_height), Image.LANCZOS)

canvas = Image.new("RGBA", (canvas_size, canvas_size), (0, 0, 0, 0))
offset_x = (canvas_size - new_width) // 2
offset_y = (canvas_size - new_height) // 2
canvas.paste(resized, (offset_x, offset_y), resized)

output_icon.parent.mkdir(parents=True, exist_ok=True)
preview_icon.parent.mkdir(parents=True, exist_ok=True)

canvas.save(output_icon, format="PNG")
canvas.save(preview_icon, format="PNG")

print(f"source: {source_icon}")
print(f"output: {output_icon}")
print(f"preview: {preview_icon}")
print(f"source size: {image.size}")
print(f"source content bbox: {bbox}")
print(f"canvas size: {canvas_size}x{canvas_size}")
print(f"content scale: {content_scale}")
print(f"final glyph size: {new_width}x{new_height}")
PY

info "Windows tray icon generated successfully"
echo
echo "Output:"
echo "  $OUTPUT_ICON"
echo
echo "Preview:"
echo "  $PREVIEW_ICON"
