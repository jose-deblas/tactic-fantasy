#!/bin/bash
# run-tests.sh - convenience script to run domain tests
set -euo pipefail
cd "$(dirname "$0")"
# Run only the DomainTests project
dotnet test ./Tests/DomainTests/DomainTests.csproj --no-build --verbosity minimal
