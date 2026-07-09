#!/usr/bin/env bash

set -euo pipefail

version=""
dry_run=false

print_usage() {
  cat <<'USAGE'
Usage:
  scripts/quality/set-preview-version.sh --version VERSION [options]

Options:
  --version VERSION   Preview release version, for example v0.4.0-preview or 0.4.0-preview.
  --dry-run           Print the normalized version without writing VERSION.
  -h, --help          Show this help.

Examples:
  scripts/quality/set-preview-version.sh --version v0.4.0-preview
  scripts/quality/set-preview-version.sh --version 0.4.0-preview
  scripts/quality/set-preview-version.sh --version v0.4.0-preview --dry-run
USAGE
}

while [ "$#" -gt 0 ]; do
  case "$1" in
    --version)
      if [ "$#" -lt 2 ]; then
        echo "ERROR: --version requires a value" >&2
        exit 1
      fi

      version="$2"
      shift 2
      ;;
    --dry-run)
      dry_run=true
      shift
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

if [ -z "$version" ]; then
  echo "ERROR: --version is required." >&2
  print_usage
  exit 1
fi

repo_root="$(git rev-parse --show-toplevel 2>/dev/null || true)"

if [ -z "${repo_root:-}" ]; then
  echo "ERROR: current directory is not inside a Git repository." >&2
  exit 1
fi

cd "$repo_root"

normalized_version="${version#v}"

if ! printf "%s" "$normalized_version" | grep -Eq '^[0-9]+\.[0-9]+\.[0-9]+(-[0-9A-Za-z.-]+)?$'; then
  echo "ERROR: invalid preview version: $version" >&2
  echo "Expected something like v0.4.0-preview or 0.4.0-preview." >&2
  exit 1
fi

echo "Input version: $version"
echo "Normalized VERSION value: $normalized_version"
echo "Git tag/release version: v$normalized_version"

if [ "$dry_run" = true ]; then
  echo "Dry run completed. VERSION was not changed."
  exit 0
fi

printf "%s\n" "$normalized_version" > VERSION

echo "Updated VERSION"
echo ""
echo "Next steps:"
echo "  1. Review VERSION"
echo "  2. Run: scripts/quality/prepare-preview-release.sh --version v$normalized_version"
echo "  3. Run: scripts/quality/release-readiness-check.sh --release-version v$normalized_version"
