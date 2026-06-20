#!/usr/bin/env bash
set -euo pipefail

echo "Cleaning solution..."
dotnet clean

echo "Restoring packages..."
dotnet restore

echo "Building Release..."
dotnet build -c Release --no-restore

echo "Running tests..."
dotnet test -c Release --no-build

echo "Verification completed successfully."