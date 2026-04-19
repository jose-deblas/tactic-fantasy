#!/bin/bash
# run-tests.sh - convenience script to run domain tests
# Usage: ./run-tests.sh [--filter "FullyQualifiedName~TestName"]
set -euo pipefail
cd "$(dirname "$0")"
PROJECT=./Tests/DomainTests/DomainTests.csproj
# If dotnet isn't installed, fail with a helpful message
if ! command -v dotnet >/dev/null 2>&1; then
  echo "dotnet CLI not found. Install .NET SDK to run tests: https://dotnet.microsoft.com/download" >&2
  exit 2
fi
# If no build artifacts exist, run a build first
if [ ! -d "./Tests/DomainTests/bin" ]; then
  echo "No build artifacts found. Running dotnet build..."
  dotnet build "$PROJECT" --verbosity minimal
fi
# Support optional filter argument
FILTER=""
if [ "$#" -ge 1 ]; then
  FILTER="--filter $*"
fi
echo "Running domain tests..."
dotnet test "$PROJECT" --verbosity minimal $FILTER
