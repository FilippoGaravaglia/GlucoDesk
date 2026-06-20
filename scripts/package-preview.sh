#!/usr/bin/env bash
set -euo pipefail

PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
VERSION_FILE="$PROJECT_ROOT/VERSION"
SOLUTION_FILE="$PROJECT_ROOT/GlucoDesk.slnx"
DESKTOP_PROJECT="$PROJECT_ROOT/src/GlucoDesk.Desktop/GlucoDesk.Desktop.csproj"

if [[ ! -f "$VERSION_FILE" ]]; then
  echo "VERSION file not found at $VERSION_FILE"
  exit 1
fi

VERSION="$(tr -d '[:space:]' < "$VERSION_FILE")"

if [[ -z "$VERSION" ]]; then
  echo "VERSION file is empty"
  exit 1
fi

RID="${1:-osx-arm64}"
CONFIGURATION="${CONFIGURATION:-Release}"

BASE_VERSION="${VERSION%%-*}"
BUNDLE_VERSION="1"

ARTIFACTS_ROOT="$PROJECT_ROOT/artifacts"
RAW_PUBLISH_DIR="$ARTIFACTS_ROOT/publish/GlucoDesk-$VERSION-$RID/raw"
APP_PACKAGE_DIR="$ARTIFACTS_ROOT/package/GlucoDesk-$VERSION-$RID"
APP_BUNDLE_DIR="$APP_PACKAGE_DIR/GlucoDesk.app"
APP_CONTENTS_DIR="$APP_BUNDLE_DIR/Contents"
APP_MACOS_DIR="$APP_CONTENTS_DIR/MacOS"
APP_RESOURCES_DIR="$APP_CONTENTS_DIR/Resources"
RELEASES_DIR="$ARTIFACTS_ROOT/releases"
ARCHIVE_PATH="$RELEASES_DIR/GlucoDesk-$VERSION-$RID.zip"
ICON_PATH="$ARTIFACTS_ROOT/branding/GlucoDesk.icns"

echo "Packaging GlucoDesk $VERSION for $RID"
echo "Project root: $PROJECT_ROOT"

rm -rf "$RAW_PUBLISH_DIR"
rm -rf "$APP_PACKAGE_DIR"
mkdir -p "$RAW_PUBLISH_DIR"
mkdir -p "$APP_MACOS_DIR"
mkdir -p "$APP_RESOURCES_DIR"
mkdir -p "$RELEASES_DIR"

"$PROJECT_ROOT/scripts/generate-macos-icon.sh" \
  "$PROJECT_ROOT/src/GlucoDesk.Desktop/Assets/Brand/glucodesk-wordmark-removebg-preview.png" \
  "$ICON_PATH"

dotnet clean "$SOLUTION_FILE"
dotnet restore "$SOLUTION_FILE"
dotnet build "$SOLUTION_FILE" -c "$CONFIGURATION" --no-restore
dotnet test "$SOLUTION_FILE" -c "$CONFIGURATION" --no-build

dotnet publish "$DESKTOP_PROJECT" \
  -c "$CONFIGURATION" \
  -r "$RID" \
  --self-contained true \
  -p:PublishSingleFile=false \
  -p:Version="$VERSION" \
  -p:AssemblyVersion="$BASE_VERSION.0" \
  -p:FileVersion="$BASE_VERSION.0" \
  -o "$RAW_PUBLISH_DIR"

cp -R "$RAW_PUBLISH_DIR/"* "$APP_MACOS_DIR/"
cp "$ICON_PATH" "$APP_RESOURCES_DIR/GlucoDesk.icns"

if [[ ! -f "$APP_MACOS_DIR/GlucoDesk.Desktop" ]]; then
  echo "Expected executable not found: $APP_MACOS_DIR/GlucoDesk.Desktop"
  echo "Published files:"
  ls -la "$APP_MACOS_DIR"
  exit 1
fi

chmod +x "$APP_MACOS_DIR/GlucoDesk.Desktop"

cat > "$APP_CONTENTS_DIR/Info.plist" <<PLIST
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "https://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
  <dict>
    <key>CFBundleDevelopmentRegion</key>
    <string>en</string>

    <key>CFBundleDisplayName</key>
    <string>GlucoDesk</string>

    <key>CFBundleExecutable</key>
    <string>GlucoDesk.Desktop</string>

    <key>CFBundleIconFile</key>
    <string>GlucoDesk</string>

    <key>CFBundleIdentifier</key>
    <string>io.github.filippogaravaglia.glucodesk</string>

    <key>CFBundleInfoDictionaryVersion</key>
    <string>6.0</string>

    <key>CFBundleName</key>
    <string>GlucoDesk</string>

    <key>CFBundlePackageType</key>
    <string>APPL</string>

    <key>CFBundleShortVersionString</key>
    <string>$BASE_VERSION</string>

    <key>CFBundleVersion</key>
    <string>$BUNDLE_VERSION</string>

    <key>LSApplicationCategoryType</key>
    <string>public.app-category.healthcare-fitness</string>

    <key>NSHighResolutionCapable</key>
    <true/>

    <key>NSHumanReadableCopyright</key>
    <string>Copyright © Filippo Garavaglia. All rights reserved.</string>
  </dict>
</plist>
PLIST

cat > "$APP_CONTENTS_DIR/PkgInfo" <<'PKGINFO'
APPL????
PKGINFO

rm -f "$ARCHIVE_PATH"

(
  cd "$APP_PACKAGE_DIR"
  zip -r "$ARCHIVE_PATH" "GlucoDesk.app"
)

echo
echo "Preview app bundle created:"
echo "$APP_BUNDLE_DIR"
echo
echo "Preview release archive created:"
echo "$ARCHIVE_PATH"
echo
echo "To test locally:"
echo "open \"$APP_BUNDLE_DIR\""
echo
echo "If macOS blocks the unsigned preview app, use right click > Open."
