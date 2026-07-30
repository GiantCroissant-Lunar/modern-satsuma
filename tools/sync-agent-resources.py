#!/usr/bin/env python3
"""Thin wrapper: sync this project's agent resources via the workspace script.

Walks up from this file looking for ``<lunar-horse>/.agent/scripts/sync-project.py``
and runs it for each nested adapter under ``.agent/adapters/targets`` and
``.agent/adapters/runners``. Additional CLI arguments are passed through.
"""

from __future__ import annotations

import subprocess
import sys
from pathlib import Path

PROJECT_ROOT = Path(__file__).resolve().parents[1]
SHARED_REL = Path(".agent") / "scripts" / "sync-project.py"
ADAPTERS_REL = Path(".agent") / "adapters"


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


def discover_providers(project_root: Path) -> list[str]:
    """Return adapter paths relative to ``.agent/adapters``."""
    adapters_root = project_root / ADAPTERS_REL
    return sorted(
        config.parent.relative_to(adapters_root).as_posix()
        for config in adapters_root.rglob("config.yaml")
    )


def requested_provider(args: list[str]) -> tuple[int, str] | None:
    """Find a ``--provider`` argument and return its index and value."""
    for index, arg in enumerate(args):
        if arg == "--provider":
            if index + 1 >= len(args):
                raise SystemExit("ERROR: --provider requires a value")
            return index, args[index + 1]
        if arg.startswith("--provider="):
            return index, arg.partition("=")[2]
    return None


def resolve_provider(requested: str, providers: list[str]) -> str:
    """Resolve a full adapter path or an unambiguous leaf name."""
    normalized = requested.replace("\\", "/")
    if normalized in providers:
        return normalized

    matches = [provider for provider in providers if Path(provider).name == requested]
    if len(matches) == 1:
        return matches[0]
    if not matches:
        choices = ", ".join(providers)
        raise SystemExit(
            f"ERROR: unknown provider '{requested}'; available providers: {choices}"
        )

    choices = ", ".join(matches)
    raise SystemExit(
        f"ERROR: provider '{requested}' is ambiguous; use one of: {choices}"
    )


def with_provider(args: list[str], provider: str) -> list[str]:
    """Insert or replace the provider argument for the shared script."""
    result = list(args)
    selected = requested_provider(result)
    if selected is None:
        return ["--provider", provider, *result]

    index, _ = selected
    if result[index] == "--provider":
        result[index + 1] = provider
    else:
        result[index] = f"--provider={provider}"
    return result


def main() -> int:
    script = find_workspace_script(PROJECT_ROOT)
    providers = discover_providers(PROJECT_ROOT)
    if not providers:
        raise SystemExit(
            f"ERROR: no adapter configs found under {PROJECT_ROOT / ADAPTERS_REL}"
        )

    passthrough = sys.argv[1:]
    selected = requested_provider(passthrough)
    if selected is not None:
        providers = [resolve_provider(selected[1], providers)]

    exit_code = 0
    for provider in providers:
        result = subprocess.run(
            [
                sys.executable,
                str(script),
                "--project",
                str(PROJECT_ROOT),
                *with_provider(passthrough, provider),
            ],
            check=False,
        )
        exit_code = max(exit_code, result.returncode)
    return exit_code


if __name__ == "__main__":
    raise SystemExit(main())
