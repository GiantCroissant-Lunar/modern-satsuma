#!/usr/bin/env python3
"""Thin wrapper: sync this project's agent resources via the workspace script.

Walks up from this file looking for ``<lunar-horse>/.agent/scripts/sync-project.py``
and runs it with ``--project <this-project-root>`` plus any pass-through CLI
args. Mirrors the pattern established by muni-dungeon/tools/sync-agent-resources.py.
"""

from __future__ import annotations

import runpy
import sys
from pathlib import Path

PROJECT_ROOT = Path(__file__).resolve().parents[1]
SHARED_REL = Path(".agent") / "scripts" / "sync-project.py"


def find_workspace_script(start: Path) -> Path:
    cur = start
    while True:
        candidate = cur / SHARED_REL
        if candidate.is_file():
            return candidate
        if cur.parent == cur:
            raise SystemExit(
                f"ERROR: workspace sync script not found searching upward from "
                f"{start}; expected {SHARED_REL}"
            )
        cur = cur.parent


def main() -> int:
    script = find_workspace_script(PROJECT_ROOT)
    sys.argv = [str(script), "--project", str(PROJECT_ROOT), *sys.argv[1:]]
    runpy.run_path(str(script), run_name="__main__")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
