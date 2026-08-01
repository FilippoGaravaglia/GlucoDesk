#!/usr/bin/env bash

set -euo pipefail

PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
ZIP_PATH="${HOME}/Downloads/glucodesk-carb-guide-legumes-images.zip"
TEMP_DIRECTORY="/tmp/glucodesk-carb-guide-legumes-images"
ASSET_DIRECTORY="${PROJECT_ROOT}/src/GlucoDesk.Desktop/Assets/CarbGuide"

if [[ ! -f "${ZIP_PATH}" ]]; then
  echo "Missing ZIP file:"
  echo "${ZIP_PATH}"
  exit 1
fi

rm -rf "${TEMP_DIRECTORY}"
mkdir -p "${TEMP_DIRECTORY}"
mkdir -p "${ASSET_DIRECTORY}"

unzip -o \
  "${ZIP_PATH}" \
  -d "${TEMP_DIRECTORY}"

required_files=(
  "fagioli-freschi.png"
  "ceci-secchi.png"
)

for file_name in "${required_files[@]}"; do
  source_path="${TEMP_DIRECTORY}/${file_name}"
  destination_path="${ASSET_DIRECTORY}/${file_name}"

  if [[ ! -s "${source_path}" ]]; then
    echo "Missing or empty image: ${source_path}"
    exit 1
  fi

  cp -f \
    "${source_path}" \
    "${destination_path}"

  echo "Installed: ${destination_path}"
done

echo
echo "Legume images installed successfully."
