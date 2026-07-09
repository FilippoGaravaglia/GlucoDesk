#!/usr/bin/env bash

set -euo pipefail

version=""
force=false
skip_readiness=false

print_usage() {
  cat <<'USAGE'
Usage:
  scripts/quality/prepare-preview-release.sh --version VERSION [options]

Options:
  --version VERSION   Preview release version, for example v0.4.0-preview.
  --force             Overwrite existing generated release notes and smoke test report.
  --skip-readiness    Skip the final release readiness check.
  -h, --help          Show this help.

Examples:
  scripts/quality/prepare-preview-release.sh --version v0.4.0-preview
  scripts/quality/prepare-preview-release.sh --version v0.4.0-preview --force
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
    --force)
      force=true
      shift
      ;;
    --skip-readiness)
      skip_readiness=true
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

release_notes_path="docs/release-notes/${version}.md"
smoke_test_path="docs/smoke-tests/${version}.md"

echo ""
echo "Preparing GlucoDesk preview release: $version"
echo "Repository: $repo_root"
echo ""

if [ ! -x "scripts/quality/create-release-notes.sh" ]; then
  echo "ERROR: missing executable script scripts/quality/create-release-notes.sh" >&2
  exit 1
fi

if [ ! -x "scripts/quality/create-smoke-test-report.sh" ]; then
  echo "ERROR: missing executable script scripts/quality/create-smoke-test-report.sh" >&2
  exit 1
fi

if [ ! -x "scripts/quality/release-readiness-check.sh" ]; then
  echo "ERROR: missing executable script scripts/quality/release-readiness-check.sh" >&2
  exit 1
fi

create_args=(--version "$version")

if [ "$force" = true ]; then
  create_args+=(--force)
fi

echo "Step 1/3 - Creating release notes"
if [ -f "$release_notes_path" ] && [ "$force" != true ]; then
  echo "Release notes already exist: $release_notes_path"
else
  scripts/quality/create-release-notes.sh "${create_args[@]}"
fi

echo ""
echo "Step 2/3 - Creating smoke test report"
if [ -f "$smoke_test_path" ] && [ "$force" != true ]; then
  echo "Smoke test report already exists: $smoke_test_path"
else
  scripts/quality/create-smoke-test-report.sh "${create_args[@]}"
fi

echo ""
echo "Generated release files:"
echo "  - $release_notes_path"
echo "  - $smoke_test_path"

if [ "$skip_readiness" = true ]; then
  echo ""
  echo "Readiness check skipped by --skip-readiness."
else
  echo ""
  echo "Step 3/3 - Running release readiness check"
  scripts/quality/release-readiness-check.sh \
    --release-version "$version" \
    --require-smoke-test
fi

echo ""
echo "Next manual steps:"
echo "  1. Edit $release_notes_path"
echo "  2. Edit $smoke_test_path"
echo "  3. Replace TODO values"
echo "  4. Run: scripts/quality/release-readiness-check.sh --release-version $version --require-smoke-test"
echo "  5. On main, run: scripts/quality/release-readiness-check.sh --strict --release-version $version --require-smoke-test"
