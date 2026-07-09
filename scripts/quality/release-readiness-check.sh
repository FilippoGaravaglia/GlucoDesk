#!/usr/bin/env bash

set -u
set -o pipefail

configuration="Release"
strict=false
skip_dotnet=false
release_version=""
artifacts_dir=""

failures=0
warnings=0

print_usage() {
  cat <<'USAGE'
Usage:
  scripts/quality/release-readiness-check.sh [options]

Options:
  --strict                   Require main branch and clean working tree.
  --configuration NAME       Build/test configuration. Default: Release.
  --release-version VERSION  Require release notes for the given version.
  --artifacts-dir DIR        Validate release artifacts and generate checksums.
  --skip-dotnet              Skip restore/build/test. Useful while editing the script.
  -h, --help                 Show this help.

Examples:
  scripts/quality/release-readiness-check.sh
  scripts/quality/release-readiness-check.sh --strict
  scripts/quality/release-readiness-check.sh --strict --release-version v0.4.0-preview
  scripts/quality/release-readiness-check.sh --strict --artifacts-dir artifacts/release
USAGE
}

pass() {
  printf "✅ %s\n" "$1"
}

warn() {
  warnings=$((warnings + 1))
  printf "⚠️  %s\n" "$1"
}

fail() {
  failures=$((failures + 1))
  printf "❌ %s\n" "$1"
}

info() {
  printf "ℹ️  %s\n" "$1"
}

run_step() {
  local label="$1"
  shift

  info "$label"

  if "$@"; then
    pass "$label"
  else
    fail "$label"
  fi
}

check_required_file() {
  local path="$1"
  local label="$2"

  if [ -f "$path" ]; then
    pass "$label"
  else
    fail "$label missing: $path"
  fi
}

check_required_directory() {
  local path="$1"
  local label="$2"

  if [ -d "$path" ]; then
    pass "$label"
  else
    fail "$label missing: $path"
  fi
}

check_file_contains() {
  local path="$1"
  local pattern="$2"
  local label="$3"

  if [ ! -f "$path" ]; then
    fail "$label missing file: $path"
    return
  fi

  if grep -Fq "$pattern" "$path"; then
    pass "$label"
  else
    fail "$label missing pattern: $pattern"
  fi
}

while [ "$#" -gt 0 ]; do
  case "$1" in
    --strict)
      strict=true
      shift
      ;;
    --configuration)
      if [ "$#" -lt 2 ]; then
        fail "--configuration requires a value"
        break
      fi

      configuration="$2"
      shift 2
      ;;
    --release-version)
      if [ "$#" -lt 2 ]; then
        fail "--release-version requires a value"
        break
      fi

      release_version="$2"
      shift 2
      ;;
    --artifacts-dir)
      if [ "$#" -lt 2 ]; then
        fail "--artifacts-dir requires a value"
        break
      fi

      artifacts_dir="$2"
      shift 2
      ;;
    --skip-dotnet)
      skip_dotnet=true
      shift
      ;;
    -h|--help)
      print_usage
      exit 0
      ;;
    *)
      fail "Unknown option: $1"
      print_usage
      break
      ;;
  esac
done

printf "\nGlucoDesk release readiness check\n"
printf "Configuration: %s\n" "$configuration"
printf "Strict mode: %s\n" "$strict"
printf "Release version: %s\n" "${release_version:-none}"
printf "Artifacts dir: %s\n" "${artifacts_dir:-none}"
printf "Skip .NET: %s\n\n" "$skip_dotnet"

repo_root="$(git rev-parse --show-toplevel 2>/dev/null || true)"

if [ -z "${repo_root:-}" ]; then
  fail "Current directory is not inside a Git repository."
else
  cd "$repo_root" || exit 1
  pass "Git repository detected: $repo_root"
fi

branch_name="$(git rev-parse --abbrev-ref HEAD 2>/dev/null || true)"

if [ -n "$branch_name" ]; then
  info "Current branch: $branch_name"

  if [ "$strict" = true ] && [ "$branch_name" != "main" ]; then
    fail "Strict mode requires branch main. Current branch: $branch_name"
  else
    pass "Branch check completed"
  fi
else
  fail "Unable to detect current Git branch."
fi

working_tree_status="$(git status --porcelain 2>/dev/null || true)"

if [ -n "$working_tree_status" ]; then
  if [ "$strict" = true ]; then
    fail "Strict mode requires a clean working tree."
  else
    warn "Working tree has local changes. This is allowed outside strict mode."
  fi
else
  pass "Working tree is clean"
fi

solution_file=""

for candidate in *.slnx *.sln; do
  if [ -f "$candidate" ]; then
    solution_file="$candidate"
    break
  fi
done

if [ -z "$solution_file" ]; then
  fail "No .slnx or .sln file found in repository root."
else
  pass "Solution found: $solution_file"
fi

check_required_directory "src/GlucoDesk.Desktop" "Desktop project directory"
check_required_directory "tests/GlucoDesk.Desktop.Tests" "Desktop tests directory"

check_required_file "README.md" "README"
check_required_file "LICENSE" "License"

check_required_file "src/GlucoDesk.Desktop/Assets/MenuBar/glucodesk-menubar-icon-high.png" "High menu bar icon"
check_required_file "src/GlucoDesk.Desktop/Assets/MenuBar/glucodesk-menubar-icon-low.png" "Low menu bar icon"
check_required_file "src/GlucoDesk.Desktop/Assets/MenuBar/glucodesk-menubar-icon-in-range.png" "In-range menu bar icon"
check_required_file "src/GlucoDesk.Desktop/Assets/MenuBar/glucodesk-menubar-icon-privacy.png" "Privacy menu bar icon"

if find scripts -maxdepth 3 -type f \( -iname "*mac*" -o -iname "*package*" -o -iname "*publish*" \) | grep -q .; then
  pass "Packaging/publish script candidate found"
else
  warn "No packaging/publish script candidate found under scripts/"
fi

if [ -f "CHANGELOG.md" ]; then
  pass "CHANGELOG found"
else
  warn "CHANGELOG.md not found. Release notes can still be written on GitHub, but a changelog is recommended."
fi

check_required_file "docs/release-notes/preview-template.md" "Preview release notes template"
check_required_file "scripts/quality/release-artifacts-manifest.sh" "Release artifacts manifest script"

if [ -n "$release_version" ]; then
  release_notes_path="docs/release-notes/${release_version}.md"

  check_required_file "$release_notes_path" "Release notes for $release_version"
  check_file_contains "$release_notes_path" "## Supported platform" "Release notes supported platform section"
  check_file_contains "$release_notes_path" "## Installation" "Release notes installation section"
  check_file_contains "$release_notes_path" "## What's new" "Release notes what's new section"
  check_file_contains "$release_notes_path" "## Known limitations" "Release notes known limitations section"
  check_file_contains "$release_notes_path" "## Safety disclaimer" "Release notes safety disclaimer section"
  check_file_contains "$release_notes_path" "## Manual smoke test" "Release notes manual smoke test section"
fi

if [ -n "$artifacts_dir" ]; then
  check_required_directory "$artifacts_dir" "Release artifacts directory"

  if [ -d "$artifacts_dir" ]; then
    if [ -n "$release_version" ]; then
      run_step "release artifacts manifest" \
        scripts/quality/release-artifacts-manifest.sh \
          --artifacts-dir "$artifacts_dir" \
          --release-version "$release_version" \
          --output "$artifacts_dir/release-artifacts-manifest.md"
    else
      run_step "release artifacts manifest" \
        scripts/quality/release-artifacts-manifest.sh \
          --artifacts-dir "$artifacts_dir" \
          --output "$artifacts_dir/release-artifacts-manifest.md"
    fi
  fi
fi

if [ "$skip_dotnet" = false ]; then
  if command -v dotnet >/dev/null 2>&1; then
    pass ".NET SDK available"
  else
    fail ".NET SDK not found in PATH"
  fi

  if [ -n "$solution_file" ]; then
    run_step "dotnet restore" dotnet restore "$solution_file"
    run_step "dotnet build $configuration" dotnet build "$solution_file" -c "$configuration" --no-restore
    run_step "dotnet test $configuration" dotnet test "$solution_file" -c "$configuration" --no-build
  fi
else
  warn ".NET restore/build/test skipped by --skip-dotnet"
fi

run_step "git diff --check" git diff --check

printf "\nRelease readiness summary\n"
printf "Failures: %s\n" "$failures"
printf "Warnings: %s\n" "$warnings"

if [ "$failures" -gt 0 ]; then
  printf "\nResult: NOT READY\n"
  exit 1
fi

if [ "$warnings" -gt 0 ]; then
  printf "\nResult: READY WITH WARNINGS\n"
  exit 0
fi

printf "\nResult: READY\n"
exit 0
