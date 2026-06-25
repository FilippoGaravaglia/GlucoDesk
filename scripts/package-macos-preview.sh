#!/usr/bin/env bash

set -euo pipefail

APP_NAME="GlucoDesk"
PROJECT_PATH="${GLUCODESK_DESKTOP_PROJECT:-src/GlucoDesk.Desktop/GlucoDesk.Desktop.csproj}"
EXECUTABLE_NAME="${GLUCODESK_EXECUTABLE_NAME:-GlucoDesk.Desktop}"
BUNDLE_IDENTIFIER="${GLUCODESK_BUNDLE_ID:-io.github.filippogaravaglia.glucodesk}"
CONFIGURATION="${CONFIGURATION:-Release}"
RUN_TESTS="${RUN_TESTS:-true}"
ADHOC_SIGN="${GLUCODESK_ADHOC_SIGN:-true}"
SIGNING_IDENTITY="${GLUCODESK_CODESIGN_IDENTITY:-}"

VERSION="${1:-0.2.1-preview}"
RID="${2:-}"

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

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

sanitize_bundle_version() {
  local value="$1"
  value="${value%%-*}"
  value="$(printf "%s" "$value" | sed -E 's/[^0-9.]/./g; s/\.+/./g; s/^\.//; s/\.$//')"

  if [[ -z "$value" ]]; then
    value="0.0.0"
  fi

  printf "%s" "$value"
}

detect_runtime_identifier() {
  local machine
  machine="$(uname -m)"

  case "$machine" in
    arm64)
      printf "osx-arm64"
      ;;
    x86_64)
      printf "osx-x64"
      ;;
    *)
      fail "unsupported macOS architecture '$machine'. Pass RID explicitly, e.g. osx-arm64 or osx-x64."
      ;;
  esac
}

create_icon_if_possible() {
  local source_icon="$ROOT_DIR/src/GlucoDesk.Desktop/Assets/AppIcon/glucodesk-app-icon.png"
  local resources_dir="$1"

  if [[ ! -f "$source_icon" ]]; then
    info "app icon source not found, skipping .icns generation"
    return 0
  fi

  if ! command -v sips >/dev/null 2>&1 || ! command -v iconutil >/dev/null 2>&1; then
    info "sips/iconutil not available, skipping .icns generation"
    return 0
  fi

  local iconset_dir="$resources_dir/glucodesk-app-icon.iconset"
  rm -rf "$iconset_dir"
  mkdir -p "$iconset_dir"

  sips -z 16 16     "$source_icon" --out "$iconset_dir/icon_16x16.png" >/dev/null
  sips -z 32 32     "$source_icon" --out "$iconset_dir/icon_16x16@2x.png" >/dev/null
  sips -z 32 32     "$source_icon" --out "$iconset_dir/icon_32x32.png" >/dev/null
  sips -z 64 64     "$source_icon" --out "$iconset_dir/icon_32x32@2x.png" >/dev/null
  sips -z 128 128   "$source_icon" --out "$iconset_dir/icon_128x128.png" >/dev/null
  sips -z 256 256   "$source_icon" --out "$iconset_dir/icon_128x128@2x.png" >/dev/null
  sips -z 256 256   "$source_icon" --out "$iconset_dir/icon_256x256.png" >/dev/null
  sips -z 512 512   "$source_icon" --out "$iconset_dir/icon_256x256@2x.png" >/dev/null
  sips -z 512 512   "$source_icon" --out "$iconset_dir/icon_512x512.png" >/dev/null
  sips -z 1024 1024 "$source_icon" --out "$iconset_dir/icon_512x512@2x.png" >/dev/null

  iconutil -c icns "$iconset_dir" -o "$resources_dir/glucodesk-app-icon.icns"
  rm -rf "$iconset_dir"

  info "generated macOS .icns app icon"
}

write_info_plist() {
  local plist_path="$1"
  local short_version="$2"
  local bundle_version="$3"

  cat > "$plist_path" <<PLIST
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "https://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
  <dict>
    <key>CFBundleName</key>
    <string>${APP_NAME}</string>

    <key>CFBundleDisplayName</key>
    <string>${APP_NAME}</string>

    <key>CFBundleIdentifier</key>
    <string>${BUNDLE_IDENTIFIER}</string>

    <key>CFBundleExecutable</key>
    <string>${EXECUTABLE_NAME}</string>

    <key>CFBundlePackageType</key>
    <string>APPL</string>

    <key>CFBundleShortVersionString</key>
    <string>${short_version}</string>

    <key>CFBundleVersion</key>
    <string>${bundle_version}</string>

    <key>CFBundleIconFile</key>
    <string>glucodesk-app-icon</string>

    <key>LSMinimumSystemVersion</key>
    <string>12.0</string>

    <key>NSHighResolutionCapable</key>
    <true/>

    <key>LSApplicationCategoryType</key>
    <string>public.app-category.healthcare-fitness</string>
  </dict>
</plist>
PLIST
}

write_release_readme() {
  local readme_path="$1"
  local rid="$2"

  cat > "$readme_path" <<README
GlucoDesk ${VERSION} (${rid})

This is a macOS preview build of GlucoDesk.

Safety notice:
GlucoDesk is not a medical device. It does not provide medical advice, treatment decisions, insulin dosing guidance, alarms, or emergency notifications. Always rely on approved CGM apps, pump systems, and healthcare professionals for medical decisions.

Preview limitations:
- local history may be incomplete;
- export quality depends on available local readings;
- provider behavior depends on configuration and external service availability;
- this preview may be unsigned or not notarized depending on how it was built.

If macOS blocks the app because it is not signed/notarized, document the limitation clearly in the GitHub release notes before distributing it publicly.
README
}

if [[ "$(uname -s)" != "Darwin" ]]; then
  fail "macOS packaging must be run on macOS"
fi

require_command dotnet
require_command ditto
require_command hdiutil
require_command shasum

if [[ -z "$RID" ]]; then
  RID="$(detect_runtime_identifier)"
fi

SHORT_VERSION="$(sanitize_bundle_version "$VERSION")"
BUNDLE_VERSION="$SHORT_VERSION"

ARTIFACT_ROOT="$ROOT_DIR/artifacts/macos/$VERSION/$RID"
PUBLISH_DIR="$ARTIFACT_ROOT/publish"
STAGING_DIR="$ARTIFACT_ROOT/${APP_NAME}-${VERSION}-${RID}"
APP_BUNDLE="$STAGING_DIR/${APP_NAME}.app"
CONTENTS_DIR="$APP_BUNDLE/Contents"
MACOS_DIR="$CONTENTS_DIR/MacOS"
RESOURCES_DIR="$CONTENTS_DIR/Resources"
ZIP_PATH="$ARTIFACT_ROOT/${APP_NAME}-${VERSION}-${RID}.zip"
DMG_PATH="$ARTIFACT_ROOT/${APP_NAME}-${VERSION}-${RID}.dmg"
CHECKSUMS_PATH="$ARTIFACT_ROOT/${APP_NAME}-${VERSION}-${RID}-checksums.sha256"

info "packaging ${APP_NAME} ${VERSION} for ${RID}"

rm -rf "$ARTIFACT_ROOT"
mkdir -p "$PUBLISH_DIR" "$MACOS_DIR" "$RESOURCES_DIR"

pushd "$ROOT_DIR" >/dev/null

info "restoring solution"
dotnet restore

info "building solution"
dotnet build -c "$CONFIGURATION" --no-restore

if [[ "$RUN_TESTS" == "true" ]]; then
  info "running tests"
  dotnet test -c "$CONFIGURATION" --no-build
else
  info "skipping tests because RUN_TESTS=${RUN_TESTS}"
fi

info "publishing desktop project"
dotnet publish "$PROJECT_PATH" \
  -c "$CONFIGURATION" \
  -r "$RID" \
  --self-contained true \
  -o "$PUBLISH_DIR" \
  -p:PublishSingleFile=false \
  -p:DebugType=None \
  -p:DebugSymbols=false

popd >/dev/null

info "creating .app bundle"
rsync -a "$PUBLISH_DIR"/ "$MACOS_DIR"/

if [[ ! -f "$MACOS_DIR/$EXECUTABLE_NAME" ]]; then
  fail "expected executable not found: $MACOS_DIR/$EXECUTABLE_NAME"
fi

chmod +x "$MACOS_DIR/$EXECUTABLE_NAME"

create_icon_if_possible "$RESOURCES_DIR"
write_info_plist "$CONTENTS_DIR/Info.plist" "$SHORT_VERSION" "$BUNDLE_VERSION"
write_release_readme "$STAGING_DIR/README.txt" "$RID"

if command -v codesign >/dev/null 2>&1; then
  if [[ -n "$SIGNING_IDENTITY" ]]; then
    info "signing app bundle with configured identity"
    codesign --force --deep --options runtime --timestamp --sign "$SIGNING_IDENTITY" "$APP_BUNDLE"
  elif [[ "$ADHOC_SIGN" == "true" ]]; then
    info "applying ad-hoc code signature"
    codesign --force --deep --sign - "$APP_BUNDLE" || info "ad-hoc code signing failed; continuing with unsigned preview bundle"
  else
    info "skipping code signing"
  fi
else
  info "codesign not available, skipping code signing"
fi

info "creating zip archive"
rm -f "$ZIP_PATH"
ditto -c -k --sequesterRsrc --keepParent "$STAGING_DIR" "$ZIP_PATH"

info "creating dmg archive"
rm -f "$DMG_PATH"
hdiutil create \
  -volname "${APP_NAME} ${VERSION}" \
  -srcfolder "$STAGING_DIR" \
  -ov \
  -format UDZO \
  "$DMG_PATH" >/dev/null

info "creating SHA256 checksums"
rm -f "$CHECKSUMS_PATH"
(
  cd "$ARTIFACT_ROOT"
  shasum -a 256 "$(basename "$ZIP_PATH")" "$(basename "$DMG_PATH")" > "$(basename "$CHECKSUMS_PATH")"
)

info "package completed"
echo
echo "Artifacts:"
echo "  App: $APP_BUNDLE"
echo "  Zip: $ZIP_PATH"
echo "  DMG: $DMG_PATH"
echo "  Checksums: $CHECKSUMS_PATH"
echo
echo "Manual smoke test:"
echo "  open \"$APP_BUNDLE\""
