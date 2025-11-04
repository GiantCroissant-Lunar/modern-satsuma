---
doc_id: DOC-2025-00020
title: Pre-commit Hooks Organization
doc_type: reference
status: active
canonical: true
created: 2025-10-30
tags: [precommit, organization, structure, best-practices]
summary: Organization and structure guide for pre-commit hooks pack in lunar-snake-hub
related: [DOC-2025-00019]
source:
  author: agent
  agent: claude
  model: sonnet-4.5
---

# Pre-commit Hooks Organization

This document explains how the pre-commit hooks pack is organized and the rationale behind the structure.

## Directory Structure

```text
precommit/
├── README.md                   # Main documentation (DOC-2025-00019)
├── ORGANIZATION.md            # This file
├── hooks/                     # Git hook entry points
│   ├── pre-commit             # Runs before git commit
│   ├── commit-msg             # Validates commit messages
│   └── pre-push               # Runs before git push
├── checks/                    # Individual check scripts
│   └── general/               # Language-agnostic checks
│       ├── gitleaks-check     # Secrets detection
│       ├── prevent_nul_file.py # Prevents null file commits
│       └── validate_agent_pointers.py # Agent pointer sync
├── utils/                     # Shared utilities
│   └── common.sh             # Common bash functions
└── examples/                  # Example configurations
    ├── pre-commit-config.yaml # Example pre-commit config
    └── setup-hooks.sh         # Automated setup script
```

## Design Principles

### 1. Universal vs. Project-Specific

**Hub provides:**

- ✅ Universal checks (gitleaks, agent pointers, prevent nul files)
- ✅ Example hooks (pre-commit, commit-msg, pre-push)
- ✅ Shared utilities (common.sh)

**Satellites add:**

- 🔧 Language-specific checks (.NET, Python, Go, Rust, etc.)
- 🔧 Project-specific validations
- 🔧 Custom business logic checks

**Rationale:** The hub should not impose language constraints. A Python project doesn't need .NET checks, and vice versa.

### 2. Separation of Concerns

**hooks/** - Git hook entry points

- Orchestrate multiple checks
- Handle error aggregation
- Provide user-friendly output
- Should be thin wrappers

**checks/** - Individual check implementations

- Single responsibility
- Independently testable
- Reusable across hooks
- Clear success/failure semantics

**utils/** - Shared code

- Avoid duplication
- Consistent output formatting
- Color coding for terminals
- Error handling patterns

### 3. Discoverability

**Clear naming:**

- `gitleaks-check` (not `check-secrets` or `secrets`)
- `validate_agent_pointers.py` (describes what it validates)
- `prevent_nul_file.py` (describes what it prevents)

**Categorization:**

- `checks/general/` - Works for any project
- `checks/dotnet/` (satellite-only) - .NET specific
- `checks/python/` (satellite-only) - Python specific

### 4. Composability

Hooks can be used:

1. **Via pre-commit framework** (recommended) - `.pre-commit-config.yaml`
2. **Directly in .git/hooks/** - Copy hooks/ to .git/hooks/
3. **Via task runner** - `task pre-commit:run`
4. **Mixed approach** - Pre-commit + custom local hooks

## File Organization Rules

### Executable Scripts

All scripts in `hooks/` and `checks/` must be executable:

```bash
chmod +x precommit/hooks/*
chmod +x precommit/checks/**/*
```

### Shebang Lines

**Bash scripts:**

```bash
#!/bin/bash
```

**Python scripts:**

```python
#!/usr/bin/env python3
```

### Dependencies

**Document all dependencies:**

- In README.md prerequisites section
- At top of script as comments
- With installation instructions

### Self-Contained

Each check should:

- Run independently
- Not depend on other checks
- Exit with 0 (success) or 1 (failure)
- Print clear error messages

## Integration Patterns

### Pattern 1: Pre-commit Framework (Recommended)

Hub provides `.pre-commit-config.yaml` at root level. Satellites just install:

```bash
task hub:sync
pre-commit install
```

**Benefits:**

- Automatic updates when hub syncs
- Language-specific virtual environments
- Parallel execution
- Skip functionality
- CI integration

### Pattern 2: Direct Git Hooks

For simple projects or when pre-commit framework is overkill:

```bash
cp .hub-cache/precommit/hooks/* .git/hooks/
```

**Benefits:**

- No external dependencies
- Faster execution (no framework overhead)
- Full control over hook behavior

### Pattern 3: Hybrid

Use pre-commit for universal checks, custom scripts for project-specific:

```yaml
repos:
  - repo: https://github.com/pre-commit/pre-commit-hooks
    rev: v4.5.0
    hooks:
      - id: trailing-whitespace

  - repo: local
    hooks:
      - id: my-custom-check
        entry: ./git-hooks/checks/custom/my-check.sh
        language: system
```

## Adding New Checks

### Universal Check Criteria

To add a check to the hub's `checks/general/`, it must be:

1. **Language-agnostic** - Works for any project type
2. **Broadly applicable** - Useful for most satellites
3. **Well-documented** - Clear README and inline comments
4. **Tested** - Works on Windows, macOS, Linux
5. **Fast** - Runs in < 2 seconds for typical projects

### Examples of Universal Checks

✅ **Good candidates:**

- Secret detection (gitleaks)
- File encoding validation
- License header checks
- Conventional commit validation
- Agent pointer synchronization
- Prevent accidental commits (TODO, console.log)

❌ **Not universal (satellite-specific):**

- Language formatting (.NET, Python, Go)
- Type checking (mypy, TypeScript)
- Linting (ESLint, Ruff, golangci-lint)
- Build verification
- Test execution

### Adding a New Universal Check

1. Create script in `checks/general/new-check`
2. Make executable: `chmod +x checks/general/new-check`
3. Add documentation to README.md
4. Add to example `.pre-commit-config.yaml`
5. Test on multiple project types
6. Update version number
7. Tag release: `packs-precommit-v0.2.0`

## Maintenance

### Versioning Strategy

Follow semantic versioning:

- **Patch** (0.1.1): Bug fixes, documentation updates
- **Minor** (0.2.0): New checks, non-breaking changes
- **Major** (1.0.0): Breaking changes, major restructuring

### Backward Compatibility

- Keep old check names as symlinks when renaming
- Deprecate gracefully with warnings
- Maintain compatibility for 2 minor versions
- Document breaking changes in CHANGELOG

### Testing

Before releasing new version:

1. Test on fresh satellite clone
2. Test with pre-commit framework
3. Test with direct git hooks
4. Test on Windows, macOS, Linux
5. Verify all checks pass on hub itself (dogfooding)

## Philosophy

> "The hub provides the foundation, satellites build on it."

The pre-commit pack embodies this:

- **Hub:** Universal quality checks everyone needs
- **Satellites:** Project-specific validation that makes sense for their stack

This keeps the hub lean, focused, and widely applicable while allowing satellites full customization.

## See Also

- [README.md](README.md) - Main documentation (DOC-2025-00019)
- [Hub Dogfooding](../../DOGFOODING.md) - How hub uses its own hooks
- [Taskfile.yml](../../Taskfile.yml) - Hub's task automation (includes pre-commit tasks)

---

**Version:** 0.1.0
**Last Updated:** 2025-10-30
**Next Doc ID:** DOC-2025-00021
