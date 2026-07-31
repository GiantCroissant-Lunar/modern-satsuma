#!/bin/bash
# Wrapper to run dotnet-gitversion via cmd.exe (required because .NET tools
# launched directly from Git Bash cannot resolve the .git directory).
# Uses `dotnet tool run` so the version pinned in .config/dotnet-tools.json wins;
# a bare `dotnet-gitversion` would silently resolve a globally-installed GitVersion.
REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd -W)"
cmd //c "cd /d ${REPO_ROOT} && dotnet tool run dotnet-gitversion /output json" 2>/dev/null
