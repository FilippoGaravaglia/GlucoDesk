#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
SITE="$ROOT/site"

required_files=(
  "$SITE/index.html"
  "$SITE/404.html"
  "$SITE/.nojekyll"
  "$SITE/robots.txt"
  "$SITE/sitemap.xml"
  "$SITE/manifest.webmanifest"
  "$SITE/assets/css/styles.css"
  "$SITE/assets/js/app.js"
  "$SITE/assets/icons/glucodesk-app-icon.png"
  "$SITE/assets/images/screenshots/dashboard.png"
  "$SITE/assets/images/screenshots/diary.png"
  "$SITE/assets/images/screenshots/settings.png"
  "$SITE/assets/images/screenshots/account.png"
  "$SITE/assets/images/demo/menu-bar-states.gif"
  "$SITE/assets/images/demo/language-onboarding.png"
)

echo "==> Validating required website files"

for file in "${required_files[@]}"; do
  if [[ ! -f "$file" ]]; then
    echo "Missing file: $file"
    exit 1
  fi
done

echo "==> Checking for root-relative asset references"

if grep -RInE \
  'href="/|src="/' \
  "$SITE" \
  --include='*.html' \
  --include='*.css' \
  --include='*.js'; then
  echo "Root-relative references found."
  exit 1
fi

echo "==> Checking official release downloads"

grep -q 'data-download-platform="macos-arm64"' "$SITE/index.html"
grep -q 'data-download-platform="macos-x64"' "$SITE/index.html"
grep -q 'data-download-platform="windows-x64"' "$SITE/index.html"

grep -q   'GlucoDesk-0.3.0-preview-macos-arm64-installable.zip'   "$SITE/index.html"

grep -q   'GlucoDesk-0.3.0-preview-macos-x64-installable.zip'   "$SITE/index.html"

grep -q   'GlucoDesk-0.3.0-preview-windows-x64-installable.zip'   "$SITE/index.html"

if grep -RInE   'downloadComingSoon|Link coming in final step|Link disponibile nello step finale'   "$SITE"; then
  echo "Download placeholder ancora presente."
  exit 1
fi

echo "==> Checking bilingual content"

grep -q 'data-language-button="en"' "$SITE/index.html"
grep -q 'data-language-button="it"' "$SITE/index.html"
grep -q 'const translations' "$SITE/assets/js/app.js"

echo "==> Checking safety notice"

grep -q 'not a medical device' "$SITE/index.html"
grep -q 'non dispositivo medico' "$SITE/assets/js/app.js"

echo "==> Website validation passed"
