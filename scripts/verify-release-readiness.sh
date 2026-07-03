#!/usr/bin/env bash
set -euo pipefail

ALLOW_DIRTY=false

for arg in "$@"; do
  case "$arg" in
    --allow-dirty)
      ALLOW_DIRTY=true
      ;;
    *)
      echo "Unknown argument: $arg"
      echo "Usage: scripts/verify-release-readiness.sh [--allow-dirty]"
      exit 2
      ;;
  esac
done

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$REPO_ROOT"

echo "== GlucoDesk release readiness verification =="
echo ""

echo "== Checking required files =="
required_files=(
  "README.md"
  "CHANGELOG.md"
  "docs/features/glucose-awareness-notifications.md"
  "docs/qa/glucose-notifications-checklist.md"
  "docs/qa/native-notification-packaged-app-checklist.md"
  "docs/qa/release-readiness-checklist.md"
  "docs/release-notes/glucose-awareness-notifications-preview.md"
)

for file in "${required_files[@]}"; do
  if [[ ! -f "$file" ]]; then
    echo "Missing required file: $file"
    exit 1
  fi

  echo "OK: $file"
done

echo ""
echo "== Checking release notes are not ignored =="
if git check-ignore -q docs/release-notes/glucose-awareness-notifications-preview.md; then
  echo "Release notes file is ignored by .gitignore."
  exit 1
fi

echo "OK: release notes file is trackable"

echo ""
echo "== Checking documentation links =="
grep -q "docs/qa/release-readiness-checklist.md" README.md
grep -q "docs/release-notes/glucose-awareness-notifications-preview.md" README.md
grep -q "../qa/release-readiness-checklist.md" docs/features/glucose-awareness-notifications.md
grep -q "../release-notes/glucose-awareness-notifications-preview.md" docs/features/glucose-awareness-notifications.md
grep -q "release-readiness-checklist.md" docs/qa/native-notification-packaged-app-checklist.md

echo "OK: release readiness links found"

echo ""
echo "== Building Release configuration =="
dotnet build -c Release

echo ""
echo "== Running Release tests =="
dotnet test -c Release

echo ""
echo "== Checking whitespace =="
git diff --check

echo ""
echo "== Checking working tree =="
if [[ "$ALLOW_DIRTY" == "true" ]]; then
  echo "Skipped clean working tree requirement because --allow-dirty was provided."
  git status -sb
else
  if [[ -n "$(git status --porcelain)" ]]; then
    echo "Working tree is not clean:"
    git status -sb
    exit 1
  fi

  echo "OK: working tree is clean"
fi

echo ""
echo "Release readiness verification completed successfully."
