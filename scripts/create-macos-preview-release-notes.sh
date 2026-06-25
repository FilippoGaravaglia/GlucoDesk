#!/usr/bin/env bash

set -euo pipefail

APP_NAME="GlucoDesk"
VERSION="${1:-0.2.1-preview}"

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
OUTPUT_DIR="$ROOT_DIR/artifacts/macos/$VERSION"
OUTPUT_PATH="$OUTPUT_DIR/${APP_NAME}-${VERSION}-github-release-notes.md"
MANIFEST_PATH="$OUTPUT_DIR/${APP_NAME}-${VERSION}-macos-release-assets.txt"

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

resolve_commit_sha() {
  if git -C "$ROOT_DIR" rev-parse --short HEAD >/dev/null 2>&1; then
    git -C "$ROOT_DIR" rev-parse --short HEAD
  else
    printf "unknown"
  fi
}

resolve_branch_name() {
  if git -C "$ROOT_DIR" rev-parse --abbrev-ref HEAD >/dev/null 2>&1; then
    git -C "$ROOT_DIR" rev-parse --abbrev-ref HEAD
  else
    printf "unknown"
  fi
}

write_asset_section() {
  if [[ -f "$MANIFEST_PATH" ]]; then
    sed "s/^/> /" "$MANIFEST_PATH"
    return 0
  fi

  cat <<EOF
> Asset manifest not found.
>
> Generate release assets first with:
>
> \`\`\`bash
> bash scripts/create-macos-preview-release-assets.sh ${VERSION} osx-arm64
> \`\`\`
EOF
}

require_command git

mkdir -p "$OUTPUT_DIR"

COMMIT_SHA="$(resolve_commit_sha)"
BRANCH_NAME="$(resolve_branch_name)"
GENERATED_AT="$(date -u +"%Y-%m-%dT%H:%M:%SZ")"

cat > "$OUTPUT_PATH" <<EOF
# ${APP_NAME} ${VERSION}

${APP_NAME} ${VERSION} is a macOS preview build of GlucoDesk.

GlucoDesk is a local-first desktop companion for glucose awareness while working at a computer.

> [!IMPORTANT]
> GlucoDesk is not a medical device.
>
> It does not provide medical advice, treatment decisions, insulin dosing guidance, alarms, or emergency notifications.
>
> Always rely on approved CGM/mobile apps, pump systems, and healthcare professionals for medical decisions.

---

## Preview scope

This preview focuses on the current macOS desktop product loop:

- desktop glucose dashboard;
- menu bar presence;
- local glucose history;
- data freshness and provider status;
- history continuity and gap awareness;
- PDF glycemic diary export;
- Excel glycemic diary export;
- data completeness reporting;
- export metadata;
- safety notice in generated exports.

This release is intended for awareness, personal review, and desktop convenience only.

---

## macOS downloads

Attach the generated macOS assets from the local release artifacts folder to this GitHub Release.

Expected assets include:

- \`${APP_NAME}-${VERSION}-osx-arm64.zip\`
- \`${APP_NAME}-${VERSION}-osx-arm64.dmg\`
- \`${APP_NAME}-${VERSION}-osx-arm64-checksums.sha256\`

If Intel macOS assets are generated, also attach:

- \`${APP_NAME}-${VERSION}-osx-x64.zip\`
- \`${APP_NAME}-${VERSION}-osx-x64.dmg\`
- \`${APP_NAME}-${VERSION}-osx-x64-checksums.sha256\`

Choose:

- \`osx-arm64\` for Apple Silicon Macs, such as M1, M2, M3, M4 or newer;
- \`osx-x64\` for Intel Macs.

---

## Installation notes

Download the correct package for your Mac.

For the zip package:

1. download the zip;
2. unzip it;
3. move \`GlucoDesk.app\` to the Applications folder;
4. open the app.

For the dmg package:

1. download the dmg;
2. open the dmg;
3. move \`GlucoDesk.app\` to the Applications folder;
4. open the app.

---

## macOS Gatekeeper note

This preview may be unsigned or not notarized depending on how it was built.

Because of this, macOS may block the first launch.

First try:

\`\`\`text
Right click GlucoDesk.app -> Open -> Open
\`\`\`

If macOS reports that the app is damaged or cannot be opened, remove the quarantine attribute:

\`\`\`bash
xattr -dr com.apple.quarantine /Applications/GlucoDesk.app
open /Applications/GlucoDesk.app
\`\`\`

This is expected for the current preview packaging flow.

A future release goal is to provide signed and notarized macOS packages.

---

## Verify downloads

The release includes SHA256 checksum files.

After downloading the assets, you can verify them from Terminal:

\`\`\`bash
shasum -a 256 -c ${APP_NAME}-${VERSION}-osx-arm64-checksums.sha256
\`\`\`

Expected output:

\`\`\`text
${APP_NAME}-${VERSION}-osx-arm64.zip: OK
${APP_NAME}-${VERSION}-osx-arm64.dmg: OK
\`\`\`

---

## Generated asset manifest

EOF

write_asset_section >> "$OUTPUT_PATH"

cat >> "$OUTPUT_PATH" <<EOF

---

## Known preview limitations

- GlucoDesk is not a medical device.
- No emergency alerts are provided.
- No insulin dosing suggestions are provided.
- Local history may be incomplete.
- Export quality depends on locally available readings.
- Provider behavior depends on account configuration and external service availability.
- macOS signing and notarization may still require additional release work.

---

## Validation checklist before publishing

Before publishing this release, verify that:

- [ ] \`dotnet build -c Release\` passes;
- [ ] \`dotnet test -c Release\` passes;
- [ ] macOS release assets were generated;
- [ ] macOS release assets were verified;
- [ ] checksum verification passes;
- [ ] the generated \`.app\` launches locally;
- [ ] dashboard opens;
- [ ] menu bar presence works;
- [ ] PDF export works;
- [ ] Excel export works;
- [ ] README is up to date;
- [ ] safety wording is present in README and generated exports.

---

## Build metadata

- Version: \`${VERSION}\`
- Branch: \`${BRANCH_NAME}\`
- Commit: \`${COMMIT_SHA}\`
- Generated at: \`${GENERATED_AT}\`

---

## Disclaimer

GlucoDesk is an independent software project.

It is not affiliated with, endorsed by, approved by, or sponsored by Dexcom, Insulet, Omnipod, or any other medical device manufacturer.

GlucoDesk is not a medical device.

Do not use GlucoDesk for treatment decisions, insulin dosing, emergency alerts, or as a replacement for approved diabetes applications.

For therapy decisions, always use approved medical devices and official medical apps.
EOF

info "GitHub release notes created"
echo
echo "$OUTPUT_PATH"
