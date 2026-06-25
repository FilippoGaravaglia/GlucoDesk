#!/usr/bin/env bash

set -euo pipefail

APP_NAME="GlucoDesk"
VERSION="${1:-0.2.1-preview}"

if [[ "$#" -gt 0 ]]; then
  shift
fi

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PACKAGE_SCRIPT="$ROOT_DIR/scripts/package-macos-preview.sh"
VERIFY_SCRIPT="$ROOT_DIR/scripts/verify-macos-preview-artifacts.sh"
MANIFEST_PATH="$ROOT_DIR/artifacts/macos/$VERSION/${APP_NAME}-${VERSION}-macos-release-assets.txt"

fail() {
  echo "error: $*" >&2
  exit 1
}

info() {
  echo "==> $*"
}

require_script() {
  local path="$1"

  if [[ ! -f "$path" ]]; then
    fail "required script not found: $path"
  fi

  if [[ ! -x "$path" ]]; then
    fail "required script is not executable: $path"
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
      fail "unsupported macOS architecture $machine. Pass RID explicitly, e.g. osx-arm64 or osx-x64."
      ;;
  esac
}

write_requested_rids() {
  local rid

  if [[ "$#" -eq 0 ]]; then
    detect_runtime_identifier
    return 0
  fi

  for rid in "$@"; do
    case "$rid" in
      all)
        printf "%s\n" "osx-arm64"
        printf "%s\n" "osx-x64"
        ;;
      osx-arm64|osx-x64)
        printf "%s\n" "$rid"
        ;;
      *)
        fail "unsupported runtime identifier $rid. Use osx-arm64, osx-x64, or all."
        ;;
    esac
  done
}

append_manifest_entry() {
  local rid="$1"
  local artifact_root="$ROOT_DIR/artifacts/macos/$VERSION/$rid"
  local zip_name="${APP_NAME}-${VERSION}-${rid}.zip"
  local dmg_name="${APP_NAME}-${VERSION}-${rid}.dmg"
  local checksums_name="${APP_NAME}-${VERSION}-${rid}-checksums.sha256"
  local checksums_path="$artifact_root/$checksums_name"

  if [[ ! -f "$checksums_path" ]]; then
    fail "checksums file not found while writing manifest: $checksums_path"
  fi

  {
    echo "- Runtime: $rid"
    echo "  App bundle folder: ${APP_NAME}-${VERSION}-${rid}/${APP_NAME}.app"
    echo "  ZIP: $zip_name"
    echo "  DMG: $dmg_name"
    echo "  SHA256: $checksums_name"
    echo "  Checksums:"
    sed "s/^/    /" "$checksums_path"
    echo
  } >> "$MANIFEST_PATH"
}

require_script "$PACKAGE_SCRIPT"
require_script "$VERIFY_SCRIPT"

if [[ "$(uname -s)" != "Darwin" ]]; then
  fail "macOS release asset creation must be run on macOS"
fi

TMP_RIDS="$(mktemp)"
write_requested_rids "$@" > "$TMP_RIDS"

mkdir -p "$(dirname "$MANIFEST_PATH")"

{
  echo "GlucoDesk macOS release assets"
  echo
  echo "Version: $VERSION"
  echo "Generated at: $(date -u +"%Y-%m-%dT%H:%M:%SZ")"
  echo
  echo "Attach the generated ZIP, DMG and SHA256 files to the GitHub Release."
  echo
  echo "Assets:"
  echo
} > "$MANIFEST_PATH"

while IFS= read -r rid; do
  if [[ -z "$rid" ]]; then
    continue
  fi

  info "creating release assets for $rid"
  bash "$PACKAGE_SCRIPT" "$VERSION" "$rid"

  info "verifying release assets for $rid"
  bash "$VERIFY_SCRIPT" "$VERSION" "$rid"

  append_manifest_entry "$rid"
done < "$TMP_RIDS"

rm -f "$TMP_RIDS"

info "macOS release assets created successfully"
echo
echo "Manifest:"
echo "  $MANIFEST_PATH"
echo
cat "$MANIFEST_PATH"
