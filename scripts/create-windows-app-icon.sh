#!/usr/bin/env bash

set -euo pipefail

APP_NAME="GlucoDesk"

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
SOURCE_ICON="$ROOT_DIR/src/GlucoDesk.Desktop/Assets/AppIcon/glucodesk-app-icon.png"
OUTPUT_ICON="$ROOT_DIR/src/GlucoDesk.Desktop/Assets/AppIcon/glucodesk-app-icon.ico"
TEMP_DIR="$ROOT_DIR/artifacts/icon-generation/windows-ico"

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
  fail "Windows icon generation currently uses macOS sips and must be run on macOS"
fi

require_command sips
require_command python3

if [[ ! -f "$SOURCE_ICON" ]]; then
  fail "source icon not found: $SOURCE_ICON"
fi

info "generating Windows .ico for $APP_NAME"

rm -rf "$TEMP_DIR"
mkdir -p "$TEMP_DIR"

for size in 16 24 32 48 64 128 256; do
  sips -z "$size" "$size" "$SOURCE_ICON" --out "$TEMP_DIR/icon_${size}x${size}.png" >/dev/null
done

python3 - "$TEMP_DIR" "$OUTPUT_ICON" <<'PY'
from pathlib import Path
import struct
import sys

temp_dir = Path(sys.argv[1])
output_icon = Path(sys.argv[2])

sizes = [16, 24, 32, 48, 64, 128, 256]
entries = []

for size in sizes:
    png_path = temp_dir / f"icon_{size}x{size}.png"

    if not png_path.exists():
        raise SystemExit(f"missing PNG icon size: {png_path}")

    data = png_path.read_bytes()
    width_byte = 0 if size >= 256 else size
    height_byte = 0 if size >= 256 else size

    entries.append((size, width_byte, height_byte, data))

header_size = 6
directory_entry_size = 16
image_offset = header_size + directory_entry_size * len(entries)

ico = bytearray()

ico += struct.pack("<HHH", 0, 1, len(entries))

for _size, width_byte, height_byte, data in entries:
    ico += struct.pack(
        "<BBBBHHII",
        width_byte,
        height_byte,
        0,
        0,
        1,
        32,
        len(data),
        image_offset,
    )

    image_offset += len(data)

for _size, _width_byte, _height_byte, data in entries:
    ico += data

output_icon.parent.mkdir(parents=True, exist_ok=True)
output_icon.write_bytes(ico)

print(f"created {output_icon}")
PY

rm -rf "$TEMP_DIR"

info "Windows .ico generated successfully"
echo "Icon:"
echo "  $OUTPUT_ICON"
