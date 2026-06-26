#!/usr/bin/env bash

set -euo pipefail

APP_NAME="GlucoDesk"

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
SERVICE_PATH="$ROOT_DIR/src/GlucoDesk.Desktop/DesktopPresence/Services/AvaloniaDesktopPresenceLifecycleService.cs"
OUTPUT_ICON="$ROOT_DIR/src/GlucoDesk.Desktop/Assets/AppIcon/glucodesk-windows-tray-icon.png"

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

require_command python3

if [[ ! -f "$SERVICE_PATH" ]]; then
  fail "desktop presence lifecycle service not found: $SERVICE_PATH"
fi

info "creating Windows tray icon from existing macOS menu bar icon"

python3 - "$ROOT_DIR" "$SERVICE_PATH" "$OUTPUT_ICON" <<'PY'
from pathlib import Path
import re
import shutil
import sys

root_dir = Path(sys.argv[1])
service_path = Path(sys.argv[2])
output_icon = Path(sys.argv[3])

content = service_path.read_text(encoding="utf-8")

uri_constants = dict(
    re.findall(
        r'private\s+static\s+readonly\s+Uri\s+(\w+)\s*=\s*new\("avares://GlucoDesk\.Desktop/([^"]+)"\);',
        content,
    )
)

mac_variable_match = re.search(
    r'OperatingSystem\.IsMacOS\(\)\s*\?\s*(\w+)\s*:',
    content,
)

if not mac_variable_match:
    raise SystemExit("Could not detect the macOS tray/menu-bar icon variable from GetTrayIconUri.")

mac_variable = mac_variable_match.group(1)

if mac_variable not in uri_constants:
    raise SystemExit(f"Could not resolve macOS tray icon URI variable: {mac_variable}")

mac_asset_relative_path = uri_constants[mac_variable]
mac_asset_path = root_dir / "src/GlucoDesk.Desktop" / mac_asset_relative_path

if not mac_asset_path.exists():
    raise SystemExit(f"macOS tray icon asset not found: {mac_asset_path}")

output_icon.parent.mkdir(parents=True, exist_ok=True)
shutil.copyfile(mac_asset_path, output_icon)

print(f"macOS tray icon variable: {mac_variable}")
print(f"macOS tray icon asset: {mac_asset_path}")
print(f"Windows tray icon asset: {output_icon}")
PY

info "Windows tray icon created successfully"
echo "Icon:"
echo "  $OUTPUT_ICON"
