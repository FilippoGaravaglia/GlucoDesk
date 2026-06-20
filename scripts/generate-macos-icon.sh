#!/usr/bin/env bash
set -euo pipefail

PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

SOURCE_ICON="${1:-$PROJECT_ROOT/src/GlucoDesk.Desktop/Assets/Brand/glucodesk-wordmark-removebg-preview.png}"
OUTPUT_ICNS="${2:-$PROJECT_ROOT/artifacts/branding/GlucoDesk.icns}"

ICONSET_DIR="$PROJECT_ROOT/artifacts/branding/GlucoDesk.iconset"
NORMALIZED_ICON="$PROJECT_ROOT/artifacts/branding/GlucoDesk-source-1024.png"

if [[ ! -f "$SOURCE_ICON" ]]; then
  echo "Source icon not found: $SOURCE_ICON"
  echo "Provide a PNG source icon, preferably 1024x1024."
  exit 1
fi

mkdir -p "$(dirname "$OUTPUT_ICNS")"
rm -rf "$ICONSET_DIR"
mkdir -p "$ICONSET_DIR"

sips \
  --resampleHeightWidthMax 900 \
  --padToHeightWidth 1024 1024 \
  --padColor FFFFFF \
  "$SOURCE_ICON" \
  --out "$NORMALIZED_ICON" >/dev/null

sips -z 16 16 "$NORMALIZED_ICON" --out "$ICONSET_DIR/icon_16x16.png" >/dev/null
sips -z 32 32 "$NORMALIZED_ICON" --out "$ICONSET_DIR/icon_16x16@2x.png" >/dev/null
sips -z 32 32 "$NORMALIZED_ICON" --out "$ICONSET_DIR/icon_32x32.png" >/dev/null
sips -z 64 64 "$NORMALIZED_ICON" --out "$ICONSET_DIR/icon_32x32@2x.png" >/dev/null
sips -z 128 128 "$NORMALIZED_ICON" --out "$ICONSET_DIR/icon_128x128.png" >/dev/null
sips -z 256 256 "$NORMALIZED_ICON" --out "$ICONSET_DIR/icon_128x128@2x.png" >/dev/null
sips -z 256 256 "$NORMALIZED_ICON" --out "$ICONSET_DIR/icon_256x256.png" >/dev/null
sips -z 512 512 "$NORMALIZED_ICON" --out "$ICONSET_DIR/icon_256x256@2x.png" >/dev/null
sips -z 512 512 "$NORMALIZED_ICON" --out "$ICONSET_DIR/icon_512x512.png" >/dev/null
sips -z 1024 1024 "$NORMALIZED_ICON" --out "$ICONSET_DIR/icon_512x512@2x.png" >/dev/null

iconutil -c icns "$ICONSET_DIR" -o "$OUTPUT_ICNS"

echo "macOS icon generated:"
echo "$OUTPUT_ICNS"
