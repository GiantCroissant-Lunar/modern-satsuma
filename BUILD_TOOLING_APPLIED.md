# Build Tooling & Validation Applied to ModernSatsuma

**Date**: 2025-10-14  
**Status**: ✅ Complete  
**Based On**: build-arcane + hook-validator patterns

---

## Summary

Applied standard Plate Framework build tooling and validation infrastructure from **build-arcane** and **hook-validator** projects to modern-satsuma.

---

## Files Added

### Pre-commit Hooks & Validation

#### `.pre-commit-config.yaml` ✅
**Source**: Adapted from build-arcane and cross-milo patterns  
**Purpose**: Automated code quality checks on commit

**Hooks Configured**:
1. **Standard Checks**
   - Trailing whitespace removal
   - End-of-file fixer
   - JSON/YAML/XML validation
   - Large file detection
   - Merge conflict detection
   - Secret detection (GitLeaks)

2. **.NET Specific** (from hook-validator)
   - Auto-format C# code with `dotnet format`
   - Check for debug statements (Debugger.Break)

3. **Python Scripts**
   - Ruff linting and formatting

4. **Documentation**
   - YAML linting (yamllint)
   - Markdown linting (markdownlint)

**Usage**:
```bash
# Install pre-commit
pip install pre-commit

# Install hooks
pre-commit install

# Run manually
pre-commit run --all-files
```

#### `.plate-validators.yaml` ✅
**Source**: Adapted from cross-milo configuration  
**Purpose**: Configure Plate Framework validators

**Configuration**:
```yaml
dotnet:
  layer_name: "Plate"
  company_prefix: "GiantCroissant"
  project_name: "ModernSatsuma"
  
  blacklist:
    - "Satsuma"  # Old namespace
    
  whitelist:
    - "Plate"
    - "System"
    - "Microsoft"
```

**Enforces**:
- Namespace conventions
- No circular dependencies
- Package ID conventions
- Central package management
- Required project properties

---

### Version Control & Semantic Versioning

#### `GitVersion.yml` ✅
**Source**: Copied from build-arcane pattern  
**Purpose**: Semantic versioning based on git branches

**Configuration**:
- **main**: Stable releases (patch increment)
- **develop**: Alpha releases (minor increment)
- **feature/**: Feature branches (uses branch name)
- **hotfix/**: Beta releases (patch increment)

**Mode**: ContinuousDelivery

**Example versions**:
- `main` → `1.0.0`, `1.0.1`, `1.0.2`
- `develop` → `1.1.0-alpha.1`, `1.1.0-alpha.2`
- `feature/add-algorithm` → `1.1.0-add-algorithm.1`

---

### Linting Configuration

#### `.yamllint` ✅
**Source**: Copied from build-arcane  
**Purpose**: YAML file validation and style checking

**Rules**:
- Line length: 120 chars (warning)
- Indentation: 2 spaces
- Document start: disabled
- Truthy values: true/false/on/off

#### `.markdownlint.json` ✅
**Source**: Copied from build-arcane  
**Purpose**: Markdown file validation

**Rules**:
- Line length: 120 chars (flexible for code blocks and tables)
- Allow inline HTML (MD033: false)
- No enforce first heading (MD041: false)

---

### Documentation Files

#### `CONTRIBUTING.md` ✅
**Source**: New, adapted from build-arcane pattern  
**Purpose**: Development workflow and contribution guidelines

**Sections**:
- Getting started
- Development workflow
- Code style guidelines
- Testing requirements
- Pre-commit hooks usage
- Commit message conventions
- Known issues
- Pull request process

#### `AGENTS.md` ✅
**Source**: New, adapted from cross-milo pattern  
**Purpose**: AI agent instructions (Windsurf, Cascade, general)

**Content**:
- Rule priority and locations
- Project overview and status
- Known build issues (critical)
- Project structure
- Build commands
- Development guidelines
- Validation configuration
- Common tasks
- Quick references

#### `CLAUDE.md` ✅
**Source**: New, specific for Claude Code  
**Purpose**: Claude-specific instructions with critical warnings

**Content**:
- Build blocker warnings (prominent)
- Quick start guide
- Key principles with rule IDs
- Important documentation links
- Project context
- Task-specific instructions
- Rules to follow (DO/DON'T)
- File locations

---

## Integration with Existing Files

### Already Present (No Changes)
- ✅ `.editorconfig` - Editor configuration
- ✅ `.gitignore` - Git ignore patterns
- ✅ `README.md` - Project overview
- ✅ `LICENSE` - zlib/libpng license
- ✅ `build-config.json` - Build metadata

### Updated/Enhanced
- ✅ Documentation now references pre-commit hooks
- ✅ README mentions validation tools
- ✅ CONTRIBUTING.md explains hook installation

---

## Comparison with Source Projects

### build-arcane Pattern Applied ✅

| Feature | build-arcane | modern-satsuma | Status |
|---------|-------------|----------------|---------|
| `.pre-commit-config.yaml` | ✅ | ✅ | Adapted |
| `GitVersion.yml` | ✅ | ✅ | Copied |
| `.yamllint` | ✅ | ✅ | Copied |
| `.markdownlint.json` | ✅ | ✅ | Copied |
| `CONTRIBUTING.md` | ✅ | ✅ | Adapted |

### hook-validator Integration ✅

| Feature | hook-validator | modern-satsuma | Status |
|---------|---------------|----------------|---------|
| `.plate-validators.yaml` | ✅ | ✅ | Configured |
| dotnet format hook | ✅ | ✅ | Integrated |
| debug statement check | ✅ | ✅ | Integrated |
| TODO format validation | ✅ | ⏳ | Ready (needs hook-validator installed) |

### cross-milo Pattern Applied ✅

| Feature | cross-milo | modern-satsuma | Status |
|---------|-----------|----------------|---------|
| `.plate-validators.yaml` | ✅ | ✅ | Similar config |
| GitLeaks integration | ✅ | ✅ | Enabled |
| Ruff for Python | ✅ | ✅ | Configured |
| `AGENTS.md` | ✅ | ✅ | Adapted |

---

## Pre-commit Hook Details

### Hook Categories

1. **File Hygiene** (pre-commit-hooks)
   - Fixes trailing whitespace
   - Ensures newline at end of file
   - Normalizes line endings to LF

2. **Security** (gitleaks)
   - Detects secrets and API keys
   - Prevents committing sensitive data
   - Scans commit history

3. **Code Quality** (.NET)
   - Auto-formats C# with `dotnet format`
   - Checks for `Debugger.Break()` statements
   - Validates project structure

4. **Script Quality** (Python)
   - Lints with Ruff
   - Auto-formats with Ruff
   - Applies to `scripts/*.py`

5. **Documentation Quality**
   - YAML validation and linting
   - Markdown linting and auto-fix
   - JSON/XML validation

### Exclusions

Pre-commit hooks skip:
- Build artifacts (`bin/`, `obj/`)
- Generated files (`*.Designer.cs`, `*.g.cs`)
- Minified files (`*.min.*`)
- Node modules
- Git internal files

---

## Validation Rules

### .NET Namespace Validation

**Enforced**:
- Must use `Plate.ModernSatsuma` namespace
- Cannot use old `Satsuma` namespace (blacklisted)
- Only approved namespaces allowed

**Allowed Namespaces**:
- `Plate.*` (project namespace)
- `System.*` (BCL)
- `Microsoft.*` (framework)

**Forbidden Namespaces**:
- `Satsuma` (old namespace from original library)

### Project Structure Validation

**Required Properties**:
- `TargetFramework` (must be present)
- `LangVersion` (must be specified)

**Central Package Management**:
- Enabled via `Directory.Packages.props`
- Package versions centralized
- No versions in project files

---

## Usage Instructions

### First-Time Setup

```bash
cd /Users/apprenticegc/Work/lunar-horse/personal-work/plate-projects/modern-satsuma

# Install pre-commit
pip install pre-commit

# Install git hooks
pre-commit install

# Test all hooks (optional)
pre-commit run --all-files
```

### Daily Workflow

```bash
# Make changes
vim dotnet/framework/src/Plate.ModernSatsuma/SomeFile.cs

# Stage changes
git add .

# Commit (hooks run automatically)
git commit -m "feat: add new algorithm"

# Hooks will:
# 1. Format C# code automatically
# 2. Check for issues
# 3. Validate files
# 4. Fix common problems
# 5. Commit if all pass
```

### Manual Hook Execution

```bash
# Run all hooks
pre-commit run --all-files

# Run specific hook
pre-commit run format-dotnet --all-files

# Update hooks to latest versions
pre-commit autoupdate
```

---

## GitVersion Workflow

### Version Calculation

```bash
# Install GitVersion (if not already)
dotnet tool install --global GitVersion.Tool

# Calculate current version
dotnet-gitversion

# Show version info
dotnet-gitversion /showvariable SemVer
```

### Branch-Based Versioning

**On main branch**:
```bash
git checkout main
dotnet-gitversion  # → 1.0.0, 1.0.1, 1.0.2...
```

**On develop branch**:
```bash
git checkout develop
dotnet-gitversion  # → 1.1.0-alpha.1, 1.1.0-alpha.2...
```

**On feature branch**:
```bash
git checkout -b feature/new-algorithm
dotnet-gitversion  # → 1.1.0-new-algorithm.1
```

---

## Integration with CI/CD (Future)

When CI/CD is added, these tools provide:

### Pre-commit Integration
- Run hooks in CI to verify contributors installed them
- Fail build if code formatting is incorrect
- Enforce all validation rules

### GitVersion Integration
- Automatically version releases
- Generate changelogs
- Tag releases appropriately

### Validation Integration
- Run plate-validators in CI
- Check for circular dependencies
- Enforce namespace conventions
- Validate TODO format

---

## Benefits

### Code Quality ✅
- Consistent formatting (automatic)
- No debug statements in production code
- Valid YAML/JSON/XML files
- Clean markdown documentation

### Security ✅
- Secret detection prevents leaks
- Private key detection
- No large binary files committed

### Compliance ✅
- Namespace conventions enforced
- Package management centralized
- Project structure validated

### Developer Experience ✅
- Automatic code formatting (no manual work)
- Fast feedback (catches issues before push)
- Consistent across team
- Easy setup (one command)

---

## Troubleshooting

### Hook Fails on Commit

```bash
# See what failed
git commit -m "message"  # Will show hook output

# Run hooks manually to debug
pre-commit run --all-files

# Skip hooks temporarily (NOT recommended)
git commit --no-verify -m "message"
```

### dotnet format Not Found

```bash
# Install .NET SDK (includes dotnet format)
# Or install as tool
dotnet tool install -g dotnet-format
```

### Python Scripts Fail

```bash
# Install Python dependencies
pip install ruff

# Or use hook-validator virtual environment
cd ../hook-validator
python -m venv .venv
source .venv/bin/activate  # or .venv\Scripts\activate on Windows
pip install -r requirements.txt
```

### Hook-Validator Not Found

The format-dotnet hook expects hook-validator to be at:
`../hook-validator/python/src/validators/dotnet/format_code.py`

Ensure hook-validator is cloned as a sibling project.

---

## Next Steps

1. **Test Hooks** ✅ Ready
   ```bash
   pre-commit run --all-files
   ```

2. **Fix Build Issues** ⏳ Pending
   - Remove duplicate IClearable
   - Handle Drawing.cs
   - See `docs/FIX_ACTION_PLAN.md`

3. **First Commit** ⏳ After build fixed
   ```bash
   git add .
   git commit -m "feat: add build tooling and validation"
   ```

4. **CI/CD Integration** ⏳ Future
   - Add GitHub Actions workflow
   - Run pre-commit in CI
   - Use GitVersion for releases

---

## Files Summary

**Added** (10 files):
1. `.pre-commit-config.yaml` - Pre-commit hook configuration
2. `.plate-validators.yaml` - Plate validator configuration
3. `GitVersion.yml` - Semantic versioning configuration
4. `.yamllint` - YAML linting rules
5. `.markdownlint.json` - Markdown linting rules
6. `CONTRIBUTING.md` - Contribution guidelines
7. `AGENTS.md` - AI agent instructions (general)
8. `CLAUDE.md` - Claude Code specific instructions
9. `BUILD_TOOLING_APPLIED.md` - This file

**Total Files in Root**: 14 configuration/documentation files

**Status**: ✅ All tooling applied and ready for use
