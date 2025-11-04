---
doc_id: DOC-2025-00019
title: Pre-commit Hooks Pack
doc_type: reference
status: active
canonical: true
created: 2025-10-30
tags: [precommit, git-hooks, quality, automation]
summary: Reference implementation of git hooks for code quality, security checks, and validation across lunar-snake projects
source:
  author: agent
  agent: claude
  model: sonnet-4.5
---

# Pre-commit Hooks Pack

**Version:** 0.1.0
**Source:** Extracted from lablab-bean
**Purpose:** Reusable git hooks for lunar-snake satellites

This pack provides a reference implementation of git hooks that can be used across lunar-snake projects.

## What's Included

### Universal Checks (checks/general/)

These checks work for any project type:

- **`gitleaks-check`** - Detects secrets, API keys, passwords, credentials
- **`prevent_nul_file.py`** - Prevents null file commits
- **`validate_agent_pointers.py`** - Ensures agent pointer files (CLAUDE.md, etc.) are in sync

### Hook Scripts (hooks/)

Example git hook implementations:

- **`pre-commit`** - Runs before commit (checks for TODO, console.log, debugging code)
- **`commit-msg`** - Validates commit messages (Conventional Commits format)
- **`pre-push`** - Runs before push (checks for sensitive data)

### Utilities (utils/)

- **`common.sh`** - Shared bash functions for consistent output, colors, error handling

### Examples (examples/)

- **`pre-commit-config.yaml`** - Example configuration for pre-commit framework
- **`setup-hooks.sh`** - Automated setup script

## For Satellites: How to Use

### Option 1: Use with pre-commit Framework (Recommended)

The hub already provides `.pre-commit-config.yaml` at the root level. Satellites can:

1. Sync from hub: `task hub:sync`
2. Install: `pre-commit install`
3. Done! Hooks run automatically

### Option 2: Copy Custom Hooks

If you need project-specific customizations:

```bash
# After hub:sync, copy hooks from cache
cp -r .hub-cache/precommit/hooks/* .git/hooks/
cp -r .hub-cache/precommit/checks ./git-hooks/checks
cp -r .hub-cache/precommit/utils ./git-hooks/utils
chmod +x .git/hooks/*
chmod +x ./git-hooks/checks/**/*
```

### Option 3: Hybrid (Pre-commit + Custom)

Use pre-commit for universal checks, add custom checks for project-specific needs:

```yaml
# .pre-commit-config.yaml
repos:
  # ... standard pre-commit hooks ...

  - repo: local
    hooks:
      - id: custom-check
        name: Custom Project Check
        entry: ./git-hooks/checks/custom/my-check
        language: system
```

## Language-Specific Checks

**The hub provides only universal checks.** For language-specific checks, satellites should add their own:

### .NET Projects

Copy from lablab-bean or create your own:

```
git-hooks/checks/dotnet/
├── dotnet-format-check       # .NET code formatting
└── one-type-per-file-check   # C# one type per file rule
```

### Python Projects

```
git-hooks/checks/python/
└── python-check              # Ruff linter + mypy type checking
```

### Go Projects

```
git-hooks/checks/golang/
├── gofmt-check
└── golangci-lint-check
```

## Prerequisites

Install tools for universal checks:

```bash
# gitleaks (secrets detection)
# Windows
winget install gitleaks

# macOS
brew install gitleaks

# Linux
# Download from https://github.com/gitleaks/gitleaks/releases
```

Python checks (`validate_agent_pointers.py`, `prevent_nul_file.py`) require Python 3.8+.

## Configuration

### gitleaks

Create `.gitleaks.toml` in your project root to customize:

```toml
[allowlist]
description = "Allowlist for false positives"
paths = [
  '''\.env\.example$''',
  '''test/fixtures/.*'''
]
```

### Agent Pointer Validation

The `validate_agent_pointers.py` check ensures that pointer files (CLAUDE.md, AGENTS.md, etc.) stay in sync with `.agent/` base rules.

To generate/update pointers:

```bash
python .agent/scripts/generate_pointers.py
```

## Directory Structure

```
precommit/
├── README.md                   # This file
├── hooks/                      # Git hook scripts
│   ├── pre-commit
│   ├── commit-msg
│   └── pre-push
├── checks/                     # Check scripts
│   └── general/                # Universal checks
│       ├── gitleaks-check
│       ├── prevent_nul_file.py
│       └── validate_agent_pointers.py
├── utils/                      # Shared utilities
│   └── common.sh
└── examples/                   # Setup examples
    ├── pre-commit-config.yaml
    └── setup-hooks.sh
```

## Customization for Satellites

### Adding Project-Specific Checks

1. Create `git-hooks/checks/custom/` in your satellite
2. Add your check scripts
3. Reference them in `.pre-commit-config.yaml` or call from main hooks

### Modifying Hook Behavior

Don't edit cached files from hub! Instead:

1. Copy hooks to your `git-hooks/` directory
2. Customize there
3. Update your `.git/hooks/` to point to your customized versions

## Versioning

This pack follows semantic versioning:

- **0.1.0** - Initial extraction from lablab-bean
- Satellites pin versions in `.hub-manifest.toml`:

```toml
[packs]
precommit = "0.1.0"
```

## Contributing

To add new universal checks to the hub:

1. Ensure check is language-agnostic
2. Add to `precommit/checks/general/`
3. Update this README
4. Test on multiple project types
5. Submit PR to hub

## See Also

- [Pre-commit Framework](https://pre-commit.com/) - Official documentation
- [Gitleaks](https://github.com/gitleaks/gitleaks) - Secrets detection
- [Conventional Commits](https://www.conventionalcommits.org/) - Commit message format
- [Hub Dogfooding Guide](../../DOGFOODING.md) - How hub uses its own infrastructure

---

**Next Pack ID:** DOC-2025-00020
