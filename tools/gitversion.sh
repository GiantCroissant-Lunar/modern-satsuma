#!/bin/bash
# Wrapper to run dotnet-gitversion via cmd.exe (required because .NET tools
# launched directly from Git Bash cannot resolve the .git directory).
REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd -W)"
cmd //c "cd /d ${REPO_ROOT} && dotnet-gitversion /output json" 2>nul
