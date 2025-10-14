# Claude Code Instructions - ModernSatsuma

**Project**: Plate.ModernSatsuma  
**Location**: `/Users/apprenticegc/Work/lunar-horse/personal-work/plate-projects/modern-satsuma`  
**Workspace**: lunar-horse

---

## ⚠️ CRITICAL: Build Currently Broken

**DO NOT** attempt to build until fixing these issues:

1. **Duplicate IClearable Interface** (CS0101)
   - File 1: `dotnet/framework/src/Plate.ModernSatsuma/Graph.cs` (lines 138-141)
   - File 2: `dotnet/framework/src/Plate.ModernSatsuma/Utils.cs` (lines 12-17)
   - **Fix**: Delete from Utils.cs, keep in Graph.cs

2. **Drawing.cs Dependencies** (CS0234/CS0246)
   - File: `dotnet/framework/src/Plate.ModernSatsuma/Drawing.cs`
   - Problem: System.Drawing not available in .NET Standard 2.0
   - **Quick Fix**: Rename to `Drawing.cs.DISABLED`
   - **Full Fix**: Add System.Drawing.Common package (deprecated) or rewrite with SkiaSharp

See `docs/FIX_ACTION_PLAN.md` for step-by-step resolution.

---

## Quick Start

### Workspace Rules
**Location**: `/Users/apprenticegc/Work/lunar-horse/.agent/local/overrides.md`  
**READ THIS FIRST** - Contains rules for ALL projects in lunar-horse workspace

### Project Structure

```
modern-satsuma/
├── dotnet/framework/                    # .NET solution (NOT in root)
│   ├── Plate.ModernSatsuma.sln
│   ├── src/Plate.ModernSatsuma/        # Main library
│   └── tests/Plate.ModernSatsuma.Tests/
├── build/                              # Build artifacts
├── docs/                               # Documentation
└── scripts/                            # Build scripts (future)
```

### Build Commands

```bash
# Navigate to solution
cd /Users/apprenticegc/Work/lunar-horse/personal-work/plate-projects/modern-satsuma/dotnet/framework

# Restore (works)
dotnet restore

# Build (FAILS - see issues above)
dotnet build

# Test (after build is fixed)
dotnet test
```

---

## Key Principles

### R-MODERNSATSUMA-010: Solution Location ⭐ CRITICAL
Solution is at `dotnet/framework/Plate.ModernSatsuma.sln`, NOT in root.  
Always `cd dotnet/framework` before running dotnet commands.

### R-MODERNSATSUMA-020: Namespace Convention
- **Current**: `Plate.ModernSatsuma`
- **Old**: `Satsuma` (original library)
- **Never** use the old `Satsuma` namespace

### R-MODERNSATSUMA-030: Target Framework
- **Library**: .NET Standard 2.0 (broad compatibility)
- **Tests**: .NET 8.0 (modern features)
- Do NOT change library target framework

### R-MODERNSATSUMA-040: Pre-commit Hooks
Uses hook-validator for code quality. Hooks auto-format C# code.
```bash
# Install once
pip install pre-commit
pre-commit install

# Run manually
pre-commit run --all-files
```

---

## Important Documentation

### Must Read (In Order)

1. **`docs/FIX_ACTION_PLAN.md`** - How to fix build (CRITICAL)
2. **`docs/MODERNIZATION_ANALYSIS.md`** - What changed from original Satsuma
3. **`STRUCTURE_ALIGNMENT.md`** - Why folders are organized this way
4. **`CONTRIBUTING.md`** - Development workflow

### Quick Reference

- **AGENTS.md** - AI agent instructions (general)
- **README.md** - Project overview
- **LICENSE** - zlib/libpng license from original Satsuma

---

## Project Context

### What is ModernSatsuma?

A .NET Standard 2.0 modernization of the Satsuma Graph Library.

**Original**: `winged-bean/ref-projects/satsumagraph-code` (reference implementation)

**Features**:
- Graph data structures (Node, Arc, directed/undirected)
- Path finding (Dijkstra, A*, BFS, DFS, Bellman-Ford)
- Network flow (Preflow, Network Simplex)
- Matching algorithms (maximum, bipartite, min-cost)
- Graph I/O (GraphML, Lemon format)
- Linear programming for optimization

### Why Modernize?

1. Target .NET Standard 2.0 for broad compatibility
2. Use modern C# features (nullable reference types)
3. Central package management
4. Standard Plate Framework structure
5. Modern tooling (pre-commit hooks, validation)

### What's Missing?

**Nothing major** - all 34 source files from original are present.  
Only issues are build-time problems (duplicate interface, drawing dependencies).

---

## Validation Configuration

### .plate-validators.yaml

```yaml
dotnet:
  layer_name: "Plate"
  project_name: "ModernSatsuma"
  blacklist: ["Satsuma"]  # Old namespace
  whitelist: ["Plate", "System", "Microsoft"]
```

Enforces:
- No use of old `Satsuma` namespace
- Only approved namespaces
- Package ID conventions
- Central package management

### GitVersion.yml

Semantic versioning:
- **main** → 1.0.x (stable)
- **develop** → 1.x.0-alpha (development)
- **feature/** → 1.x.0-feature-name (testing)

---

## Common Tasks

### Task: Fix Build Issues

See `docs/FIX_ACTION_PLAN.md` for detailed steps.

**Quick version**:
```bash
cd /Users/apprenticegc/Work/lunar-horse/personal-work/plate-projects/modern-satsuma

# Fix 1: Remove duplicate IClearable from Utils.cs
# Edit dotnet/framework/src/Plate.ModernSatsuma/Utils.cs
# Delete lines 12-17

# Fix 2: Disable Drawing.cs
mv dotnet/framework/src/Plate.ModernSatsuma/Drawing.cs \
   dotnet/framework/src/Plate.ModernSatsuma/Drawing.cs.DISABLED

# Test build
cd dotnet/framework
dotnet clean
dotnet build  # Should now succeed
```

### Task: Add New Algorithm

1. Create `dotnet/framework/src/Plate.ModernSatsuma/YourAlgorithm.cs`
2. Namespace: `Plate.ModernSatsuma`
3. Follow patterns in existing files (e.g., `Dijsktra.cs`)
4. Add tests: `dotnet/framework/tests/Plate.ModernSatsuma.Tests/YourAlgorithmTests.cs`
5. Build and test

### Task: Update Documentation

1. Add XML comments (triple-slash `///`) to public APIs
2. Update README.md for API changes
3. Add examples if complex
4. Run `dotnet build` to validate XML doc comments

### Task: Format Code

```bash
cd dotnet/framework
dotnet format
```

Or let pre-commit hooks handle it automatically.

---

## File Locations

### Source Code
- **Main library**: `dotnet/framework/src/Plate.ModernSatsuma/*.cs`
- **Core types**: `Graph.cs` (Node, Arc, interfaces)
- **Algorithms**: `Dijsktra.cs`, `BellmanFord.cs`, `Preflow.cs`, etc.
- **I/O**: `IO.cs`, `IO.GraphML.cs`
- **Broken**: `Drawing.cs` (System.Drawing dependency)

### Tests
- **Location**: `dotnet/framework/tests/Plate.ModernSatsuma.Tests/*.cs`
- **Framework**: xUnit
- **Target**: .NET 8.0

### Configuration
- **Solution**: `dotnet/framework/Plate.ModernSatsuma.sln`
- **Packages**: `dotnet/framework/Directory.Packages.props`
- **Project**: `dotnet/framework/src/Plate.ModernSatsuma/Plate.ModernSatsuma.csproj`

---

## Rules to Follow

### DO ✅

- Read workspace rules first: `/Users/apprenticegc/Work/lunar-horse/.agent/local/overrides.md`
- Use `Plate.ModernSatsuma` namespace
- Target .NET Standard 2.0 for library code
- Add XML documentation to public APIs
- Add unit tests for new features
- Run pre-commit hooks before committing
- Follow existing code patterns

### DON'T ❌

- Change target framework from netstandard2.0
- Use `Satsuma` namespace (old)
- Build from project root (solution is in `dotnet/framework/`)
- Add dependencies without updating `Directory.Packages.props`
- Commit without running tests
- Skip pre-commit hooks

---

## Related Projects

- **Original Satsuma**: `../../../yokan-projects/winged-bean/ref-projects/satsumagraph-code`
- **Cross-Milo**: `../cross-milo` (may use graph algorithms)
- **Plugin-Manoi**: `../plugin-manoi` (may use dependency graphs)
- **Build-Arcane**: `../build-arcane` (build tooling patterns)
- **Hook-Validator**: `../hook-validator` (validation tools we use)

---

## For Claude Code Specifically

When working in this project:

1. **ALWAYS** check if build is fixed before attempting operations
2. **ALWAYS** use full paths: `/Users/apprenticegc/Work/lunar-horse/personal-work/plate-projects/modern-satsuma/...`
3. **ALWAYS** cd to `dotnet/framework` before dotnet commands
4. **READ** `docs/FIX_ACTION_PLAN.md` before fixing build issues
5. **REFER** to original at `winged-bean/ref-projects/satsumagraph-code` when unsure
6. **USE** pre-commit hooks to auto-format code

---

## Version Information

**Current Status**: v0.1.0-alpha (pre-release, build broken)  
**Last Updated**: 2025-10-14  
**Next Milestone**: Fix build issues, reach v0.1.0-alpha (buildable)

---

**Read the workspace rules**: `/Users/apprenticegc/Work/lunar-horse/.agent/local/overrides.md`
