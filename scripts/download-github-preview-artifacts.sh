#!/usr/bin/env bash
set -euo pipefail

readonly SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
readonly ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"
readonly WORKFLOW_FILE="preview-release-artifacts.yml"

version="${1:-}"
run_id="${2:-}"

info() {
  printf '[INFO] %s\n' "$1"
}

error() {
  printf '[ERROR] %s\n' "$1" >&2
}

usage() {
  cat <<'USAGE'
Usage:
  ./scripts/download-github-preview-artifacts.sh <version> [run-id]

Examples:
  ./scripts/download-github-preview-artifacts.sh 0.2.1-preview
  ./scripts/download-github-preview-artifacts.sh 0.2.1-preview 12345678901

Notes:
  - Requires GitHub CLI: gh
  - If run-id is omitted, the latest successful run for preview-release-artifacts.yml on main is used.
  - Downloaded files are written under artifacts/github-actions/<version>/run-<run-id>.
USAGE
}

if [[ -z "$version" ]]; then
  usage
  exit 1
fi

cd "$ROOT_DIR"

if [[ ! -f ".github/workflows/$WORKFLOW_FILE" ]]; then
  error "Workflow file not found: .github/workflows/$WORKFLOW_FILE"
  exit 1
fi

if ! command -v gh >/dev/null 2>&1; then
  error "GitHub CLI 'gh' is required but was not found."
  error "Install it, authenticate with 'gh auth login', then retry."
  exit 1
fi

if ! gh auth status >/dev/null 2>&1; then
  error "GitHub CLI is not authenticated."
  error "Run 'gh auth login' and retry."
  exit 1
fi

if [[ -z "$run_id" ]]; then
  info "No run id provided. Looking for latest successful '$WORKFLOW_FILE' run on main..."

  run_id="$(
    gh run list \
      --workflow "$WORKFLOW_FILE" \
      --branch main \
      --status success \
      --limit 1 \
      --json databaseId \
      --jq '.[0].databaseId'
  )"

  if [[ -z "$run_id" || "$run_id" == "null" ]]; then
    error "No successful GitHub Actions run found for workflow '$WORKFLOW_FILE' on main."
    error "Run the workflow first, then retry."
    exit 1
  fi
fi

readonly download_dir="$ROOT_DIR/artifacts/github-actions/$version/run-$run_id"
readonly manifest_file="$download_dir/downloaded-files.txt"
readonly checksum_file="$download_dir/downloaded-files.sha256"

info "Version: $version"
info "Workflow: $WORKFLOW_FILE"
info "Run id: $run_id"
info "Download directory: $download_dir"

rm -rf "$download_dir"
mkdir -p "$download_dir"

info "Downloading GitHub Actions artifacts..."
gh run download "$run_id" --dir "$download_dir"

file_count="$(find "$download_dir" -type f | wc -l | tr -d ' ')"

if [[ "$file_count" == "0" ]]; then
  error "No files were downloaded from GitHub Actions run $run_id."
  exit 1
fi

info "Downloaded $file_count file(s)."

find "$download_dir" -type f | sort > "$manifest_file"

info "Downloaded files:"
sed "s|$ROOT_DIR/||" "$manifest_file"

info "Generating local SHA256 checksum file..."
(
  cd "$download_dir"
  find . -type f \
    ! -name 'downloaded-files.sha256' \
    -print0 \
    | sort -z \
    | xargs -0 shasum -a 256
) > "$checksum_file"

info "Checksum file created:"
info "$checksum_file"

info "Done."
