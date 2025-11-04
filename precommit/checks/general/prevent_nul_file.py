#!/usr/bin/env python
"""
Pre-commit hook to prevent committing a Windows 'nul' file and clean it up.

Behavior:
- If 'nul' is staged, unstage and delete it, print guidance, and fail the hook.
- If 'nul' exists in the working directory (even if not staged), delete it and warn.
"""

from __future__ import annotations

import subprocess
import sys
from pathlib import Path


def run(cmd: list[str]) -> subprocess.CompletedProcess:
    return subprocess.run(cmd, capture_output=True, text=True)


def main() -> int:
    repo_root = Path.cwd()
    nul_path = repo_root / "nul"

    # Get staged files
    diff = run(
        ["git", "diff", "--cached", "--name-only"]
    )  # ignore errors; empty output if none
    staged = {line.strip() for line in diff.stdout.splitlines() if line.strip()}

    if "nul" in staged:
        sys.stdout.write("\nERROR: Attempting to commit Windows 'nul' file\n\n")
        sys.stdout.write(
            "The file 'nul' is created by Windows command redirections like:\n"
        )
        sys.stdout.write("  dir /s /b *.json 2>nul\n\n")
        sys.stdout.write("Solutions:\n")
        sys.stdout.write("  1. Use PowerShell: Get-ChildItem -Recurse -Filter *.json\n")
        sys.stdout.write("  2. Use bash/Git Bash: find . -name '*.json' 2>/dev/null\n")
        sys.stdout.write("  3. Don't redirect stderr: dir /s /b *.json\n\n")
        sys.stdout.write("Auto-cleanup: Removing 'nul' file and unstaging...\n")

        try:
            if nul_path.exists() and nul_path.is_file():
                nul_path.unlink(missing_ok=True)  # type: ignore[arg-type]
        except Exception:
            # Ignore deletion errors; still try to unstage
            pass

        run(["git", "reset", "HEAD", "nul"])  # unstage if present
        sys.stdout.write("Fixed! Please commit again.\n")
        return 1

    # If the file exists in the working directory, clean it up
    if nul_path.exists() and nul_path.is_file():
        sys.stdout.write("\nWARNING: Found 'nul' file in working directory\n")
        sys.stdout.write("Auto-cleanup: Removing it...\n")
        try:
            nul_path.unlink(missing_ok=True)  # type: ignore[arg-type]
            sys.stdout.write("Cleaned up.\n")
        except Exception:
            # Non-fatal; allow commit to continue
            sys.stdout.write("Could not remove 'nul' file; please remove manually.\n")

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
