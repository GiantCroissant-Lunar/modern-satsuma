#!/usr/bin/env python
"""
Validate that agent pointer files are in sync using the generator script.

Runs `.agent/scripts/generate_pointers.py --check` when any relevant files
are staged: `.agent/**`, `CLAUDE.md`, `AGENTS.md`, `.github/copilot-instructions.md`, `.windsurf/rules.md`.
"""

from __future__ import annotations

import re
import subprocess
import sys
from pathlib import Path

RELEVANT_PATTERN = re.compile(
    r"^(?:\.agent/|CLAUDE\.md|AGENTS\.md|\.github/copilot-instructions\.md|\.windsurf/rules\.md)"
)


def run(cmd: list[str]) -> subprocess.CompletedProcess:
    return subprocess.run(cmd, capture_output=True, text=True)


def main() -> int:
    repo_root = Path.cwd()

    # Gather staged files
    diff = run(
        ["git", "diff", "--cached", "--name-only"]
    )  # if not a git repo, pre-commit wouldn't run
    staged = [line.strip() for line in diff.stdout.splitlines() if line.strip()]

    if any(RELEVANT_PATTERN.match(p) for p in staged):
        sys.stdout.write("Detected changes to agent files or pointer files\n")

        generator = repo_root / ".agent" / "scripts" / "generate_pointers.py"
        if not generator.exists():
            sys.stderr.write("Error: .agent/scripts/generate_pointers.py not found.\n")
            return 1

        # Invoke the generator in check mode using the current Python interpreter
        proc = run([sys.executable, str(generator), "--check"])
        if proc.returncode == 0:
            sys.stdout.write("Agent pointer files are up to date\n")
            return 0

        sys.stdout.write("\nAgent pointer files are out of sync!\n\n")
        sys.stdout.write("To fix, run:\n")
        sys.stdout.write(f"  {Path(sys.executable).name} {generator}\n")
        sys.stdout.write(
            "  git add CLAUDE.md AGENTS.md .github/copilot-instructions.md .windsurf/rules.md\n\n"
        )
        sys.stdout.write(proc.stdout)
        sys.stderr.write(proc.stderr)
        return 1

    sys.stdout.write("No agent files modified, skipping validation\n")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
