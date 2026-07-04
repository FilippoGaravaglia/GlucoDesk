#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
SOURCE_FILE="$ROOT_DIR/src/GlucoDesk.Desktop/Platform/MacOS/Notifications/GlucoDeskNotificationHelper.swift"
ICON_FILE="$ROOT_DIR/src/GlucoDesk.Desktop/Assets/AppIcon/glucodesk-app-icon.icns"

OUTPUT_ROOT="${1:-$ROOT_DIR/src/GlucoDesk.Desktop/bin/Release/net10.0}"
APP_DIR="$OUTPUT_ROOT/GlucoDeskNotificationHelper.app"
CONTENTS_DIR="$APP_DIR/Contents"
MACOS_DIR="$CONTENTS_DIR/MacOS"
RESOURCES_DIR="$CONTENTS_DIR/Resources"
EXECUTABLE="$MACOS_DIR/GlucoDeskNotificationHelper"

if [[ "$(uname -s)" != "Darwin" ]]; then
  echo "macOS notification helper can only be built on macOS."
  exit 0
fi

if [[ ! -f "$SOURCE_FILE" ]]; then
  echo "Missing helper source: $SOURCE_FILE"
  exit 1
fi

if [[ ! -f "$ICON_FILE" ]]; then
  echo "Missing helper icon: $ICON_FILE"
  exit 1
fi

rm -rf "$APP_DIR"
mkdir -p "$MACOS_DIR" "$RESOURCES_DIR"

cp "$ICON_FILE" "$RESOURCES_DIR/GlucoDesk.icns"

cat > "$CONTENTS_DIR/Info.plist" <<'PLIST'
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "https://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleName</key>
    <string>GlucoDesk Notifications</string>
    <key>CFBundleDisplayName</key>
    <string>GlucoDesk Notifications</string>
    <key>CFBundleIdentifier</key>
    <string>com.filippogaravaglia.glucodesk.notifications</string>
    <key>CFBundleExecutable</key>
    <string>GlucoDeskNotificationHelper</string>
    <key>CFBundleIconFile</key>
    <string>GlucoDesk</string>
    <key>CFBundlePackageType</key>
    <string>APPL</string>
    <key>CFBundleShortVersionString</key>
    <string>0.3.0</string>
    <key>CFBundleVersion</key>
    <string>1</string>
    <key>NSPrincipalClass</key>
    <string>NSApplication</string>
    <key>LSMinimumSystemVersion</key>
    <string>13.0</string>
    <key>LSUIElement</key>
    <true/>
</dict>
</plist>
PLIST

xcrun swiftc -swift-version 5 \
  "$SOURCE_FILE" \
  -o "$EXECUTABLE" \
  -framework Cocoa \
  -framework UserNotifications

chmod +x "$EXECUTABLE"

xattr -cr "$APP_DIR"
codesign --force --deep --sign - "$APP_DIR"
codesign --verify --deep --strict --verbose=2 "$APP_DIR"

echo "Built macOS notification helper:"
echo "$APP_DIR"
