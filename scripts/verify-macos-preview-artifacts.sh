#!/usr/bin/env bash

set -euo pipefail

APP_NAME="GlucoDesk"
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
    fail "required command $1 was not found"
  fi
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
      fail "unsupported architecture $machine. Pass RID explicitly, e.g. osx-arm64 or osx-x64."
      ;;
  esac
}

verify_file() {
  local path="$1"
  local label="$2"

  if [[ ! -f "$path" ]]; then
    fail "$label not found: $path"
  fi

  if [[ ! -s "$path" ]]; then
    fail "$label is empty: $path"
  fi

  info "$label found"
}


verify_dmg_install_layout() {
  local dmg_path="$1"
  local mount_dir="$2"

  rm -rf "$mount_dir"
  mkdir -p "$mount_dir"

  hdiutil attach "$dmg_path" \
    -mountpoint "$mount_dir" \
    -nobrowse \
    -readonly \
    -quiet

  local error_message=""

  if [[ ! -d "$mount_dir/${APP_NAME}.app" ]]; then
    error_message="DMG does not contain ${APP_NAME}.app"
  elif [[ ! -L "$mount_dir/Applications" ]]; then
    error_message="DMG does not contain Applications symlink"
  elif [[ "$(readlink "$mount_dir/Applications")" != "/Applications" ]]; then
    error_message="DMG Applications symlink does not point to /Applications"
  elif [[ ! -f "$mount_dir/README.txt" ]]; then
    error_message="DMG does not contain README.txt"
  elif [[ ! -f "$mount_dir/SAFETY-NOTICE.txt" ]]; then
    error_message="DMG does not contain SAFETY-NOTICE.txt"
  fi

  hdiutil detach "$mount_dir" -quiet
  rm -rf "$mount_dir"

  if [[ -n "$error_message" ]]; then
    fail "$error_message"
  fi

  info "dmg install layout verified"
}

if [[ -z "$RID" ]]; then
  RID="$(detect_runtime_identifier)"
fi

ARTIFACT_ROOT="$ROOT_DIR/artifacts/macos/$VERSION/$RID"
STAGING_DIR="$ARTIFACT_ROOT/${APP_NAME}-${VERSION}-${RID}"
APP_BUNDLE="$STAGING_DIR/${APP_NAME}.app"
INFO_PLIST="$APP_BUNDLE/Contents/Info.plist"
README_PATH="$STAGING_DIR/README.txt"
SAFETY_NOTICE_PATH="$STAGING_DIR/SAFETY-NOTICE.txt"
ZIP_PATH="$ARTIFACT_ROOT/${APP_NAME}-${VERSION}-${RID}.zip"
DMG_PATH="$ARTIFACT_ROOT/${APP_NAME}-${VERSION}-${RID}.dmg"
CHECKSUMS_PATH="$ARTIFACT_ROOT/${APP_NAME}-${VERSION}-${RID}-checksums.sha256"
DMG_MOUNT_DIR="$ARTIFACT_ROOT/dmg-verify-mount"

require_command shasum
require_command unzip

info "verifying macOS preview artifacts for ${APP_NAME} ${VERSION} ${RID}"

if [[ ! -d "$ARTIFACT_ROOT" ]]; then
  fail "artifact directory not found: $ARTIFACT_ROOT"
fi

if [[ ! -d "$APP_BUNDLE" ]]; then
  fail ".app bundle not found: $APP_BUNDLE"
fi

if [[ ! -f "$INFO_PLIST" ]]; then
  fail "Info.plist not found: $INFO_PLIST"
fi

verify_file "$ZIP_PATH" "zip archive"
verify_file "$DMG_PATH" "dmg archive"
verify_file "$CHECKSUMS_PATH" "checksums file"
verify_file "$README_PATH" "release README"
verify_file "$SAFETY_NOTICE_PATH" "safety notice"

grep -q "$(basename "$ZIP_PATH")" "$CHECKSUMS_PATH" || fail "checksums file does not reference zip archive"
grep -q "$(basename "$DMG_PATH")" "$CHECKSUMS_PATH" || fail "checksums file does not reference dmg archive"

info "testing zip archive integrity"
unzip -tq "$ZIP_PATH" >/dev/null

if [[ "$(uname -s)" == "Darwin" ]]; then
  require_command hdiutil

  info "verifying dmg archive"
  hdiutil verify "$DMG_PATH" >/dev/null

  info "verifying dmg install layout"
  verify_dmg_install_layout "$DMG_PATH" "$DMG_MOUNT_DIR"

  if command -v plutil >/dev/null 2>&1; then
    info "validating Info.plist"
    plutil -lint "$INFO_PLIST" >/dev/null
  fi

  if command -v codesign >/dev/null 2>&1; then
    info "verifying app bundle code signature"
    codesign --verify --deep "$APP_BUNDLE" >/dev/null
  fi
fi

info "verifying SHA256 checksums"
(
  cd "$ARTIFACT_ROOT"
  shasum -a 256 -c "$(basename "$CHECKSUMS_PATH")"
)

info "release artifacts verified successfully"
