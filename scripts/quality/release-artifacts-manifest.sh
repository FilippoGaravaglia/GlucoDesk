#!/usr/bin/env bash

set -euo pipefail

artifacts_dir=""
output_file=""
release_version=""

print_usage() {
  cat <<'USAGE'
Usage:
  scripts/quality/release-artifacts-manifest.sh --artifacts-dir DIR [options]

Options:
  --artifacts-dir DIR       Directory containing release artifacts.
  --output FILE             Manifest output path. Default: DIR/release-artifacts-manifest.md.
  --release-version VALUE   Release version to include in the manifest.
  -h, --help                Show this help.

Supported artifact extensions:
  .zip, .dmg, .pkg, .tar.gz
USAGE
}

while [ "$#" -gt 0 ]; do
  case "$1" in
    --artifacts-dir)
      if [ "$#" -lt 2 ]; then
        echo "ERROR: --artifacts-dir requires a value" >&2
        exit 1
      fi

      artifacts_dir="$2"
      shift 2
      ;;
    --output)
      if [ "$#" -lt 2 ]; then
        echo "ERROR: --output requires a value" >&2
        exit 1
      fi

      output_file="$2"
      shift 2
      ;;
    --release-version)
      if [ "$#" -lt 2 ]; then
        echo "ERROR: --release-version requires a value" >&2
        exit 1
      fi

      release_version="$2"
      shift 2
      ;;
    -h|--help)
      print_usage
      exit 0
      ;;
    *)
      echo "ERROR: unknown option: $1" >&2
      print_usage
      exit 1
      ;;
  esac
done

if [ -z "$artifacts_dir" ]; then
  echo "ERROR: --artifacts-dir is required." >&2
  print_usage
  exit 1
fi

if [ ! -d "$artifacts_dir" ]; then
  echo "ERROR: artifacts directory does not exist: $artifacts_dir" >&2
  exit 1
fi

if ! command -v shasum >/dev/null 2>&1; then
  echo "ERROR: shasum is required but was not found in PATH." >&2
  exit 1
fi

if [ -z "$output_file" ]; then
  output_file="$artifacts_dir/release-artifacts-manifest.md"
fi

manifest_dir="$(dirname "$output_file")"
mkdir -p "$manifest_dir"

artifact_list_file="$(mktemp)"
trap 'rm -f "$artifact_list_file"' EXIT

find "$artifacts_dir" \
  -maxdepth 1 \
  -type f \
  \( -name "*.zip" -o -name "*.dmg" -o -name "*.pkg" -o -name "*.tar.gz" \) \
  -print | sort > "$artifact_list_file"

if [ ! -s "$artifact_list_file" ]; then
  echo "ERROR: no release artifacts found in $artifacts_dir" >&2
  echo "Expected at least one of: .zip, .dmg, .pkg, .tar.gz" >&2
  exit 1
fi

sha_file="$artifacts_dir/SHA256SUMS.txt"

{
  echo "# GlucoDesk Release Artifacts Manifest"
  echo ""

  if [ -n "$release_version" ]; then
    echo "Release version: $release_version"
  else
    echo "Release version: unspecified"
  fi

  echo "Artifacts directory: $artifacts_dir"
  echo "Generated at UTC: $(date -u '+%Y-%m-%dT%H:%M:%SZ')"
  echo ""
  echo "| File | Size bytes | SHA256 |"
  echo "|---|---:|---|"
} > "$output_file"

: > "$sha_file"

while IFS= read -r artifact; do
  file_name="$(basename "$artifact")"
  size_bytes="$(wc -c < "$artifact" | tr -d ' ')"
  sha256="$(shasum -a 256 "$artifact" | awk '{print $1}')"

  echo "| $file_name | $size_bytes | $sha256 |" >> "$output_file"
  echo "$sha256  $file_name" >> "$sha_file"
done < "$artifact_list_file"

echo "Release artifact manifest created: $output_file"
echo "SHA256 sums created: $sha_file"
