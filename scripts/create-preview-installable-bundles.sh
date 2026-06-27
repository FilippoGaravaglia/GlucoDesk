#!/usr/bin/env bash
set -euo pipefail

APP_NAME="GlucoDesk"
VERSION="${1:-0.2.1-preview}"
RUN_ID="${2:-}"
RELEASE_TAG="${3:-v${VERSION}-rc1}"

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
SOURCE_DIR="$ROOT_DIR/artifacts/github-actions/$VERSION/run-$RUN_ID"
BUNDLE_DIR="$ROOT_DIR/artifacts/release-candidate/$RELEASE_TAG"

fail() {
  echo "error: $*" >&2
  exit 1
}

info() {
  echo "==> $*"
}

copy_file() {
  local source="$1"
  local destination_dir="$2"

  if [[ ! -f "$source" ]]; then
    fail "required file not found: $source"
  fi

  cp "$source" "$destination_dir/"
}

if [[ -z "$RUN_ID" ]]; then
  fail "run id is required. Usage: ./scripts/create-preview-installable-bundles.sh 0.2.1-preview <RUN_ID> v0.2.1-preview-rc1"
fi

if [[ ! -d "$SOURCE_DIR" ]]; then
  fail "GitHub Actions artifact directory not found: $SOURCE_DIR"
fi

info "Version: $VERSION"
info "Run id: $RUN_ID"
info "Release tag: $RELEASE_TAG"
info "Source dir: $SOURCE_DIR"
info "Bundle dir: $BUNDLE_DIR"

rm -rf "$BUNDLE_DIR"
mkdir -p \
  "$BUNDLE_DIR/macos-arm64" \
  "$BUNDLE_DIR/macos-x64" \
  "$BUNDLE_DIR/windows-x64"

MACOS_ARTIFACT_DIR="$SOURCE_DIR/glucodesk-$VERSION-macos-preview-artifacts"
WINDOWS_ARTIFACT_DIR="$SOURCE_DIR/glucodesk-$VERSION-windows-preview-artifacts"

info "Copying macOS Apple Silicon assets"
copy_file "$MACOS_ARTIFACT_DIR/GlucoDesk-$VERSION-macos-release-assets.txt" "$BUNDLE_DIR/macos-arm64"
copy_file "$MACOS_ARTIFACT_DIR/osx-arm64/GlucoDesk-$VERSION-osx-arm64.dmg" "$BUNDLE_DIR/macos-arm64"
copy_file "$MACOS_ARTIFACT_DIR/osx-arm64/GlucoDesk-$VERSION-osx-arm64.zip" "$BUNDLE_DIR/macos-arm64"
copy_file "$MACOS_ARTIFACT_DIR/osx-arm64/GlucoDesk-$VERSION-osx-arm64-checksums.sha256" "$BUNDLE_DIR/macos-arm64"

info "Copying macOS Intel assets"
copy_file "$MACOS_ARTIFACT_DIR/GlucoDesk-$VERSION-macos-release-assets.txt" "$BUNDLE_DIR/macos-x64"
copy_file "$MACOS_ARTIFACT_DIR/osx-x64/GlucoDesk-$VERSION-osx-x64.dmg" "$BUNDLE_DIR/macos-x64"
copy_file "$MACOS_ARTIFACT_DIR/osx-x64/GlucoDesk-$VERSION-osx-x64.zip" "$BUNDLE_DIR/macos-x64"
copy_file "$MACOS_ARTIFACT_DIR/osx-x64/GlucoDesk-$VERSION-osx-x64-checksums.sha256" "$BUNDLE_DIR/macos-x64"

info "Copying Windows x64 assets"
copy_file "$WINDOWS_ARTIFACT_DIR/GlucoDesk-$VERSION-windows-release-assets.txt" "$BUNDLE_DIR/windows-x64"
copy_file "$WINDOWS_ARTIFACT_DIR/win-x64/GlucoDesk-$VERSION-win-x64-setup.exe" "$BUNDLE_DIR/windows-x64"
copy_file "$WINDOWS_ARTIFACT_DIR/win-x64/GlucoDesk-$VERSION-win-x64-portable.zip" "$BUNDLE_DIR/windows-x64"
copy_file "$WINDOWS_ARTIFACT_DIR/win-x64/GlucoDesk-$VERSION-win-x64-checksums.sha256" "$BUNDLE_DIR/windows-x64"

cat > "$BUNDLE_DIR/macos-arm64/README.txt" <<README
GlucoDesk macOS Apple Silicon preview package.

Install:
1. Open the DMG.
2. Drag GlucoDesk.app into Applications.
3. Launch GlucoDesk from Applications.

First launch on macOS:
This preview build may be unsigned or not notarized. On first launch, macOS may show a message saying that Apple cannot verify whether GlucoDesk contains malware.

If that happens:
1. Click Done or close the warning dialog.
2. Open System Settings.
3. Go to Privacy & Security.
4. Scroll to the Security section.
5. Find the GlucoDesk warning.
6. Click Open Anyway.
7. Confirm with your password or Touch ID.
8. Launch GlucoDesk again from Applications.

This approval is normally required only the first time the app is opened.

Safety:
GlucoDesk is not a medical device and must not be used for insulin dosing, treatment, diagnosis, emergency, or safety-critical decisions.
README

cat > "$BUNDLE_DIR/macos-x64/README.txt" <<README
GlucoDesk macOS Intel preview package.

Install:
1. Open the DMG.
2. Drag GlucoDesk.app into Applications.
3. Launch GlucoDesk from Applications.

First launch on macOS:
This preview build may be unsigned or not notarized. On first launch, macOS may show a message saying that Apple cannot verify whether GlucoDesk contains malware.

If that happens:
1. Click Done or close the warning dialog.
2. Open System Settings.
3. Go to Privacy & Security.
4. Scroll to the Security section.
5. Find the GlucoDesk warning.
6. Click Open Anyway.
7. Confirm with your password or Touch ID.
8. Launch GlucoDesk again from Applications.

This approval is normally required only the first time the app is opened.

Safety:
GlucoDesk is not a medical device and must not be used for insulin dosing, treatment, diagnosis, emergency, or safety-critical decisions.
README

cat > "$BUNDLE_DIR/windows-x64/README.txt" <<README
GlucoDesk Windows x64 preview package.

Install:
1. Run GlucoDesk-$VERSION-win-x64-setup.exe.
2. Follow the installation wizard.
3. Launch GlucoDesk from the Start Menu.

Portable mode:
You can also extract GlucoDesk-$VERSION-win-x64-portable.zip and run GlucoDesk.Desktop.exe.

First launch on Windows:
This preview build is not code-signed. Microsoft Defender SmartScreen may show a warning such as "Windows protected your PC" because the app is not yet recognized.

If that happens:
1. Click More info.
2. Verify that the app name is GlucoDesk-$VERSION-win-x64-setup.exe.
3. Click Run anyway.

On Italian Windows, the buttons may appear as:
1. Ulteriori informazioni.
2. Esegui comunque.

Only continue if you downloaded GlucoDesk from the official GitHub Releases page.

Safety:
GlucoDesk is not a medical device and must not be used for insulin dosing, treatment, diagnosis, emergency, or safety-critical decisions.
README

info "Creating installable ZIP bundles"
(
  cd "$BUNDLE_DIR/macos-arm64"
  zip -qry "../GlucoDesk-$VERSION-macos-arm64-installable.zip" .
)

(
  cd "$BUNDLE_DIR/macos-x64"
  zip -qry "../GlucoDesk-$VERSION-macos-x64-installable.zip" .
)

(
  cd "$BUNDLE_DIR/windows-x64"
  zip -qry "../GlucoDesk-$VERSION-windows-x64-installable.zip" .
)

info "Generating bundle checksums"
(
  cd "$BUNDLE_DIR"
  shasum -a 256 \
    "GlucoDesk-$VERSION-macos-arm64-installable.zip" \
    "GlucoDesk-$VERSION-macos-x64-installable.zip" \
    "GlucoDesk-$VERSION-windows-x64-installable.zip" \
    > "GlucoDesk-$VERSION-release-candidate-bundles.sha256"
)

info "Created bundles:"
ls -lh "$BUNDLE_DIR"/*.zip "$BUNDLE_DIR"/*.sha256

info "Verifying bundle checksums"
(
  cd "$BUNDLE_DIR"
  shasum -a 256 -c "GlucoDesk-$VERSION-release-candidate-bundles.sha256"
)

info "Done."
