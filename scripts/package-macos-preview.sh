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
NOTARIZE="${GLUCODESK_NOTARIZE:-false}"
NOTARY_KEYCHAIN_PROFILE="${GLUCODESK_NOTARY_KEYCHAIN_PROFILE:-}"
NOTARY_APPLE_ID="${GLUCODESK_NOTARY_APPLE_ID:-}"
NOTARY_TEAM_ID="${GLUCODESK_NOTARY_TEAM_ID:-}"
NOTARY_PASSWORD="${GLUCODESK_NOTARY_PASSWORD:-}"

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
  local resources_dir="$1"
  local source_icon="$ROOT_DIR/src/GlucoDesk.Desktop/Assets/AppIcon/glucodesk-app-icon.icns"
  local output_icon="$resources_dir/glucodesk-app-icon.icns"

  if [[ ! -f "$source_icon" ]]; then
    fail "optimized macOS app icon not found: $source_icon. Generate it with scripts/create-macos-app-icon.sh"
  fi

  mkdir -p "$resources_dir"
  cp "$source_icon" "$output_icon"

  info "using optimized macOS .icns app icon"
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

write_installation_guides() {
  local destination_dir="$1"
  local rid="$2"
  local architecture_label

  case "$rid" in
    osx-arm64)
      architecture_label="Apple Silicon (arm64)"
      ;;
    osx-x64)
      architecture_label="Intel (x64)"
      ;;
    *)
      architecture_label="$rid"
      ;;
  esac

  cat > "$destination_dir/GUIDA-INSTALLAZIONE-IT.txt" <<GUIDE_IT
GLUCODESK — GUIDA ALL'INSTALLAZIONE SU macOS
============================================

Versione: ${VERSION}
Architettura: ${architecture_label}

GlucoDesk è attualmente distribuito come versione preview e non è ancora
firmato o notarizzato da Apple.

Per questo motivo macOS potrebbe bloccare l'applicazione al primo avvio.
Questo comportamento è previsto per questa versione preview.

INSTALLAZIONE
-------------

1. Trascina GlucoDesk.app nella cartella Applicazioni utilizzando il
   collegamento Applications presente in questa finestra.

2. Apri la cartella Applicazioni.

3. Avvia GlucoDesk.

PRIMO AVVIO — SE macOS BLOCCA L'APP
-----------------------------------

macOS potrebbe mostrare un messaggio indicando che Apple non può verificare
GlucoDesk oppure che l'applicazione proviene da uno sviluppatore non
identificato.

Se questo accade:

1. Chiudi il messaggio di avviso.

2. Apri Impostazioni di Sistema.

3. Vai su Privacy e sicurezza.

4. Scorri fino alla sezione Sicurezza.

5. Cerca il messaggio relativo a GlucoDesk.

6. Fai clic su "Apri comunque".

7. Conferma utilizzando la password del Mac o Touch ID, se richiesto.

8. Apri nuovamente GlucoDesk dalla cartella Applicazioni.

Questa autorizzazione è normalmente necessaria soltanto al primo avvio.

DOWNLOAD UFFICIALE
------------------

Scarica GlucoDesk esclusivamente dal sito ufficiale:

https://glucodesk.com/

oppure dalla pagina GitHub ufficiale:

https://github.com/FilippoGaravaglia/GlucoDesk/releases

SICUREZZA
---------

GlucoDesk non è un dispositivo medico e non deve essere utilizzato per
decisioni relative al dosaggio dell'insulina, trattamento, diagnosi,
emergenze o altre decisioni mediche critiche.
GUIDE_IT

  cat > "$destination_dir/INSTALLATION-GUIDE-EN.txt" <<GUIDE_EN
GLUCODESK — macOS INSTALLATION GUIDE
====================================

Version: ${VERSION}
Architecture: ${architecture_label}

GlucoDesk is currently distributed as a preview build and is not yet
signed or notarized by Apple.

Because of this, macOS may block the application the first time it is opened.
This behavior is expected for this preview version.

INSTALLATION
------------

1. Drag GlucoDesk.app into the Applications folder using the Applications
   shortcut shown in this window.

2. Open the Applications folder.

3. Launch GlucoDesk.

FIRST LAUNCH — IF macOS BLOCKS THE APP
--------------------------------------

macOS may display a message saying that Apple cannot verify GlucoDesk
or that the application is from an unidentified developer.

If this happens:

1. Close the warning dialog.

2. Open System Settings.

3. Go to Privacy & Security.

4. Scroll down to the Security section.

5. Find the message referring to GlucoDesk.

6. Click "Open Anyway".

7. Confirm using your Mac password or Touch ID, if requested.

8. Launch GlucoDesk again from Applications.

This approval is normally required only the first time the application
is opened.

OFFICIAL DOWNLOAD
-----------------

Only download GlucoDesk from the official website:

https://glucodesk.com/

or from the official GitHub Releases page:

https://github.com/FilippoGaravaglia/GlucoDesk/releases

SAFETY
------

GlucoDesk is not a medical device and must not be used for insulin dosing,
treatment, diagnosis, emergency, or other safety-critical medical decisions.
GUIDE_EN
}

write_safety_notice() {
  local safety_notice_path="$1"

  cat > "$safety_notice_path" <<SAFETY
GlucoDesk safety notice

GlucoDesk is not a medical device.

It does not provide medical advice, treatment decisions, insulin dosing guidance, alarms, or emergency notifications.

Do not use GlucoDesk to make insulin dosing, treatment, diagnosis, emergency, or safety-critical decisions.

Always rely on approved CGM apps, pump systems, glucose meters, and healthcare professionals for medical decisions.
SAFETY
}


notarize_dmg_if_configured() {
  local dmg_path="$1"

  if [[ "$NOTARIZE" != "true" ]]; then
    info "skipping macOS notarization because GLUCODESK_NOTARIZE=${NOTARIZE}"
    return 0
  fi

  if [[ -z "$SIGNING_IDENTITY" ]]; then
    fail "macOS notarization requires GLUCODESK_CODESIGN_IDENTITY to be set to a valid Developer ID Application certificate"
  fi

  require_command xcrun

  info "submitting dmg for Apple notarization"

  if [[ -n "$NOTARY_KEYCHAIN_PROFILE" ]]; then
    xcrun notarytool submit "$dmg_path" \
      --keychain-profile "$NOTARY_KEYCHAIN_PROFILE" \
      --wait
  else
    if [[ -z "$NOTARY_APPLE_ID" || -z "$NOTARY_TEAM_ID" || -z "$NOTARY_PASSWORD" ]]; then
      fail "macOS notarization requires either GLUCODESK_NOTARY_KEYCHAIN_PROFILE or GLUCODESK_NOTARY_APPLE_ID, GLUCODESK_NOTARY_TEAM_ID and GLUCODESK_NOTARY_PASSWORD"
    fi

    xcrun notarytool submit "$dmg_path" \
      --apple-id "$NOTARY_APPLE_ID" \
      --team-id "$NOTARY_TEAM_ID" \
      --password "$NOTARY_PASSWORD" \
      --wait
  fi

  info "stapling notarization ticket to dmg"
  xcrun stapler staple "$dmg_path"

  info "validating stapled notarization ticket"
  xcrun stapler validate "$dmg_path"
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
DMG_STAGING_DIR="$ARTIFACT_ROOT/dmg-staging"
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

info "building macOS native notification helper"
"$ROOT_DIR/scripts/build-macos-notification-helper.sh" "$CONTENTS_DIR/Helpers"
write_installation_guides "$STAGING_DIR" "$RID"
write_safety_notice "$STAGING_DIR/SAFETY-NOTICE.txt"

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

info "creating dmg install staging directory"
rm -rf "$DMG_STAGING_DIR"
mkdir -p "$DMG_STAGING_DIR"

ditto "$APP_BUNDLE" "$DMG_STAGING_DIR/${APP_NAME}.app"
ln -s /Applications "$DMG_STAGING_DIR/Applications"
cp "$STAGING_DIR/GUIDA-INSTALLAZIONE-IT.txt"   "$DMG_STAGING_DIR/GUIDA-INSTALLAZIONE-IT.txt"

cp "$STAGING_DIR/INSTALLATION-GUIDE-EN.txt"   "$DMG_STAGING_DIR/INSTALLATION-GUIDE-EN.txt"

cp "$STAGING_DIR/SAFETY-NOTICE.txt"   "$DMG_STAGING_DIR/SAFETY-NOTICE.txt"

info "creating dmg archive"
rm -f "$DMG_PATH"
hdiutil create \
  -volname "${APP_NAME} ${VERSION}" \
  -srcfolder "$DMG_STAGING_DIR" \
  -ov \
  -format UDZO \
  "$DMG_PATH" >/dev/null

notarize_dmg_if_configured "$DMG_PATH"

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
