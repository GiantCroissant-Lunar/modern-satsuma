#!/usr/bin/env python3
"""
Pre-commit hook wrapper for documentation organization.

This script detects scattered markdown files and either:
1. In dry-run mode: Warns about scattered docs and exits with code 1
2. In auto-fix mode: Automatically organizes the files

Usage:
    python git-hooks/checks/general/organize_scattered_docs.py [--auto-fix]
"""

import argparse
import subprocess
import sys
from pathlib import Path

# Get the root directory
SCRIPT_DIR = Path(__file__).resolve().parent
ROOT_DIR = SCRIPT_DIR.parent.parent.parent
ORGANIZE_SCRIPT = ROOT_DIR / "git-hooks" / "checks" / "python" / "organize_docs.py"


def main():
    parser = argparse.ArgumentParser(
        description="Pre-commit hook for scattered documentation detection"
    )
    parser.add_argument(
        "--auto-fix",
        action="store_true",
        help="Automatically organize scattered docs instead of just warning",
    )

    args = parser.parse_args()

    if args.auto_fix:
        # Run the organizer in auto-move mode
        print("Auto-organizing scattered documentation files...")
        result = subprocess.run(
            [sys.executable, str(ORGANIZE_SCRIPT), "--auto-move"], cwd=ROOT_DIR
        )

        if result.returncode == 0:
            print("SUCCESS: Documentation organization completed successfully")
        else:
            print("ERROR: Documentation organization failed")

        sys.exit(result.returncode)
    else:
        # Run in dry-run mode to detect issues
        result = subprocess.run(
            [sys.executable, str(ORGANIZE_SCRIPT), "--dry-run"],
            cwd=ROOT_DIR,
            capture_output=True,
            text=True,
        )

        if result.returncode == 0:
            # No files need to be moved
            print("SUCCESS: All documentation files are properly organized")
            sys.exit(0)
        else:
            # Files need to be moved
            print("WARNING: Scattered documentation files detected!")
            print("\nThe following files should be organized:")
            print(result.stdout)
            print("\nTo automatically organize these files, run:")
            print("  python git-hooks/checks/python/organize_docs.py --auto-move")
            print(
                "\nOr add --auto-fix to this hook to organize automatically on commit."
            )
            sys.exit(1)


if __name__ == "__main__":
    main()
