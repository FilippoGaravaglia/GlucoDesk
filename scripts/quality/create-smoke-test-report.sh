#!/usr/bin/env bash

set -euo pipefail

version=""
template_path="docs/smoke-tests/preview-template.md"
output_dir="docs/smoke-tests"
force=false

print_usage() {
  cat <<'USAGE'
Usage:
  scripts/quality/create-smoke-test-report.sh --version VERSION [options]

Options:
  --version VERSION      Release version, for example v0.4.0-preview.
  --template FILE        Template path. Default: docs/smoke-tests/preview-template.md.
  --output-dir DIR       Output directory. Default: docs/smoke-tests.
  --force                Overwrite an existing smoke test report.
  -h, --help             Show this help.

Examples:
  scripts/quality/create-smoke-test-report.sh --version v0.4.0-preview
  scripts/quality/create-smoke-test-report.sh --version v0.4.0-preview --force
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
    --template)
      if [ "$#" -lt 2 ]; then
        echo "ERROR: --template requires a value" >&2
        exit 1
      fi

      template_path="$2"
      shift 2
      ;;
    --output-dir)
      if [ "$#" -lt 2 ]; then
        echo "ERROR: --output-dir requires a value" >&2
        exit 1
      fi

      output_dir="$2"
      shift 2
      ;;
    --force)
      force=true
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

if [ ! -f "$template_path" ]; then
  echo "ERROR: smoke test template not found: $template_path" >&2
  exit 1
fi

mkdir -p "$output_dir"

output_path="$output_dir/${version}.md"

if [ -f "$output_path" ] && [ "$force" != true ]; then
  echo "ERROR: smoke test report already exists: $output_path" >&2
  echo "Use --force to overwrite." >&2
  exit 1
fi

python3 - "$template_path" "$output_path" "$version" <<'PY'
from pathlib import Path
import sys
from datetime import datetime, timezone

template_path = Path(sys.argv[1])
output_path = Path(sys.argv[2])
version = sys.argv[3]

template = template_path.read_text()
content = template.replace("VERSION", version)

stamp = datetime.now(timezone.utc).strftime("%Y-%m-%dT%H:%M:%SZ")

header = f"""<!--
Generated from {template_path}
Release version: {version}
Generated at UTC: {stamp}

Before publishing:
- Replace TODO values.
- Mark each manual test as PASS, FAIL, or N/A.
- Keep this report committed with the release branch when possible.
-->

"""

output_path.write_text(header + content)
PY

echo "Smoke test report created: $output_path"
echo ""
echo "Next steps:"
echo "  1. Edit $output_path"
echo "  2. Replace TODO values"
echo "  3. Mark manual checks as PASS, FAIL, or N/A"
echo "  4. Run: scripts/quality/release-readiness-check.sh --release-version $version --require-smoke-test"
