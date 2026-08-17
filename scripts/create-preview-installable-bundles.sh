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


write_macos_installation_guides() {
  local destination_dir="$1"
  local architecture_label="$2"

  cat > "$destination_dir/GUIDA-INSTALLAZIONE-IT.txt" <<README
GLUCODESK — GUIDA ALL'INSTALLAZIONE SU macOS
============================================

Versione: $VERSION
Architettura: $architecture_label

GlucoDesk è attualmente distribuito come versione preview e non è ancora
firmato o notarizzato da Apple.

Per questo motivo macOS potrebbe bloccare l'applicazione al primo avvio.
Questo comportamento è previsto per questa versione preview.

INSTALLAZIONE
-------------

1. Apri il file DMG di GlucoDesk.

2. Trascina GlucoDesk.app nella cartella Applicazioni.

3. Apri la cartella Applicazioni e avvia GlucoDesk.

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

oppure dalla pagina GitHub ufficiale del progetto:

https://github.com/FilippoGaravaglia/GlucoDesk/releases

SICUREZZA
---------

GlucoDesk non è un dispositivo medico e non deve essere utilizzato per
decisioni relative al dosaggio dell'insulina, trattamento, diagnosi,
emergenze o altre decisioni mediche critiche.
README

  cat > "$destination_dir/INSTALLATION-GUIDE-EN.txt" <<README
GLUCODESK — macOS INSTALLATION GUIDE
====================================

Version: $VERSION
Architecture: $architecture_label

GlucoDesk is currently distributed as a preview build and is not yet
signed or notarized by Apple.

Because of this, macOS may block the application the first time it is opened.
This behavior is expected for this preview version.

INSTALLATION
------------

1. Open the GlucoDesk DMG file.

2. Drag GlucoDesk.app into the Applications folder.

3. Open Applications and launch GlucoDesk.

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

or from the official GlucoDesk GitHub Releases page:

https://github.com/FilippoGaravaglia/GlucoDesk/releases

SAFETY
------

GlucoDesk is not a medical device and must not be used for insulin dosing,
treatment, diagnosis, emergency, or other safety-critical medical decisions.
README
}

write_macos_installation_guides \
  "$BUNDLE_DIR/macos-arm64" \
  "Apple Silicon (arm64)"

write_macos_installation_guides \
  "$BUNDLE_DIR/macos-x64" \
  "Intel (x64)"

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
