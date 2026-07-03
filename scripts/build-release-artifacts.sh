#!/usr/bin/env bash
set -euo pipefail

CONFIGURATION="Release"
VERSION_SUFFIX=""
SKIP_TESTS=false
OUTPUT_ROOT="artifacts/release"

usage() {
  echo "Usage: scripts/build-release-artifacts.sh [--version-suffix <suffix>] [--skip-tests]"
  echo ""
  echo "Examples:"
  echo "  scripts/build-release-artifacts.sh"
  echo "  scripts/build-release-artifacts.sh --version-suffix preview"
  echo "  scripts/build-release-artifacts.sh --skip-tests"
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    --version-suffix)
      if [[ $# -lt 2 ]]; then
        echo "Missing value for --version-suffix"
        usage
        exit 2
      fi

      VERSION_SUFFIX="$2"
      shift 2
      ;;
    --skip-tests)
      SKIP_TESTS=true
      shift
      ;;
    -h|--help)
      usage
      exit 0
      ;;
    *)
      echo "Unknown argument: $1"
      usage
      exit 2
      ;;
  esac
done

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$REPO_ROOT"

DESKTOP_PROJECT="src/GlucoDesk.Desktop/GlucoDesk.Desktop.csproj"

if [[ ! -f "$DESKTOP_PROJECT" ]]; then
  echo "Missing desktop project: $DESKTOP_PROJECT"
  exit 1
fi

echo "== GlucoDesk release artifact build =="
echo ""

echo "== Cleaning output directory =="
rm -rf "$OUTPUT_ROOT"
mkdir -p "$OUTPUT_ROOT"

echo ""
echo "== Building solution =="
dotnet build -c "$CONFIGURATION"

if [[ "$SKIP_TESTS" == "false" ]]; then
  echo ""
  echo "== Running tests =="
  dotnet test -c "$CONFIGURATION"
else
  echo ""
  echo "== Skipping tests because --skip-tests was provided =="
fi

echo ""
echo "== Checking whitespace =="
git diff --check

publish_artifact() {
  local runtime_identifier="$1"
  local artifact_name="$2"

  local publish_dir="$OUTPUT_ROOT/$runtime_identifier/publish"
  local archive_dir="$OUTPUT_ROOT/$runtime_identifier"
  local archive_path="$OUTPUT_ROOT/$artifact_name.zip"

  echo ""
  echo "== Publishing $runtime_identifier =="
  dotnet publish "$DESKTOP_PROJECT" \
    -c "$CONFIGURATION" \
    -r "$runtime_identifier" \
    --self-contained true \
    -p:PublishSingleFile=false \
    -p:DebugType=None \
    -p:DebugSymbols=false \
    -o "$publish_dir"

  echo ""
  echo "== Creating archive $archive_path =="
  (
    cd "$archive_dir"
    zip -r "../$artifact_name.zip" publish >/dev/null
  )

  if [[ ! -f "$archive_path" ]]; then
    echo "Archive was not created: $archive_path"
    exit 1
  fi

  echo "Created: $archive_path"
}

suffix=""
if [[ -n "$VERSION_SUFFIX" ]]; then
  suffix="-$VERSION_SUFFIX"
fi

publish_artifact "osx-x64" "GlucoDesk-osx-x64$suffix"
publish_artifact "osx-arm64" "GlucoDesk-osx-arm64$suffix"
publish_artifact "win-x64" "GlucoDesk-win-x64$suffix"

echo ""
echo "== Release artifacts =="
find "$OUTPUT_ROOT" -maxdepth 1 -type f -name "*.zip" -print | sort

echo ""
echo "Release artifact build completed successfully."
