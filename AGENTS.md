# ModernSatsuma - Agent Instructions

**Project**: Plate.ModernSatsuma  
**Stack**: .NET  
**Type**: Graph Library  
**Date**: 2025-10-14

---

## Quick Start for AI Agents

### Rule Priority (Read in Order)

1. **Workspace Rules** (highest priority) → `/Users/apprenticegc/Work/lunar-horse/.agent/local/overrides.md`
2. **Project Rules** → `.agent/local/overrides.md` (when created)
3. **Stack Rules** (cached) → `~/.cache/lunar-rules/v1.0.0/stacks/dotnet/`
4. **Base Rules** (cached) → `~/.cache/lunar-rules/v1.0.0/base/`

### Where to Find Rules

```
Workspace Rules (ALL projects in lunar-horse)
  Absolute: /Users/apprenticegc/Work/lunar-horse/.agent/local/overrides.md
  Relative: ../../../.agent/local/overrides.md
  
Project Rules (this project only)
  Location: .agent/local/overrides.md (create if needed)
```

---

## Project Overview

**Purpose**: Modernized version of the Satsuma Graph Library for .NET Standard 2.1+

**Key Features**:
- Graph algorithms (Dijkstra, A*, BFS, DFS, Bellman-Ford)
- Network flow (Preflow, Network Simplex)
- Matching algorithms (bipartite, maximum, minimum cost)
- Graph transformations and I/O (GraphML, Lemon format)
- Linear programming framework

**Status**: 🚧 Under active development - core projects build successfully; historical docs still reference earlier build issues

---

## Important Context

### ✅ Historical Build Blockers (Now Resolved)

1. **Duplicate IClearable Interface**
   - Originally: `IClearable` was defined in both `Graph.cs` and `Utils.cs`, causing CS0101.
   - Current: Definition is kept only in `Graph.cs`; `Utils.cs` no longer declares `IClearable`.

2. **System.Drawing Dependencies in Drawing.cs**
   - Originally: `Drawing.cs` referenced `System.Drawing` types not available on .NET Standard.
   - Current: Core project excludes `Drawing.cs` and instead uses a pluggable architecture:
     - `Plate.ModernSatsuma.Abstractions` for drawing interfaces
     - `Plate.ModernSatsuma.Drawing.SystemDrawing` for System.Drawing-based rendering
     - `Plate.ModernSatsuma.Drawing.SkiaSharp` for cross-platform SkiaSharp rendering

See `docs/FIX_ACTION_PLAN.md` and `docs/analysis/MODERNIZATION_ANALYSIS.md` for historical details.

### 📋 Key Documentation

- **MODERNIZATION_ANALYSIS.md** - Gap analysis vs original Satsuma
- **FIX_ACTION_PLAN.md** - Step-by-step fix guide  
- **STRUCTURE_ALIGNMENT.md** - Recent folder structure changes
- **CONTRIBUTING.md** - Development workflow

---

## Project Structure

```
modern-satsuma/
├── dotnet/framework/
│   ├── Plate.ModernSatsuma.sln                 # Solution file
│   ├── Directory.Packages.props                # Central package management
│   ├── src/Plate.ModernSatsuma/                # Core graph library (netstandard2.1)
│   │   ├── Graph.cs                            # Core graph types
│   │   ├── Dijsktra.cs, BellmanFord.cs, ...    # Algorithms
│   │   ├── Drawing.cs                          # Retained for reference, excluded from core build
│   │   └── Utils.cs                            # Utilities (no IClearable definition)
│   ├── src/Plate.ModernSatsuma.Abstractions/   # Drawing/layout abstractions
│   ├── src/Plate.ModernSatsuma.Drawing.SystemDrawing/  # System.Drawing renderer
│   ├── src/Plate.ModernSatsuma.Drawing.SkiaSharp/      # SkiaSharp renderer
│   └── tests/                                 # Unit tests for core and renderers
└── docs/                                      # Documentation
```

---

## Build Commands

### Current Status: ✅ Core Projects Build

```bash
cd dotnet/framework
dotnet restore
dotnet build  # Core and renderer projects should build successfully
dotnet test   # Run tests
```

---

## Development Guidelines

### Code Style

- **Namespace**: `Plate.ModernSatsuma` (migrated from `Satsuma`)
- **Target**: .NET Standard 2.0 (broad compatibility)
- **Language**: C# with nullable reference types enabled
- **Formatting**: Use `dotnet format`

### Pre-commit Hooks

This project uses pre-commit hooks:

```bash
# Install (one time)
pip install pre-commit
pre-commit install

# Run manually
pre-commit run --all-files
```

Hooks will:
- Format C# code automatically
- Check for debug statements
- Validate YAML/JSON/Markdown
- Detect secrets and large files

### Testing

- Unit tests use xUnit
- Target .NET 8.0 for tests
- Tests reference the .NET Standard 2.0 library

---

## Validation Configuration

### .plate-validators.yaml

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

### GitVersion.yml

- **Mode**: ContinuousDelivery
- **main**: Patch increment (e.g., 1.0.x)
- **develop**: Minor increment with alpha label (e.g., 1.x.0-alpha)
- **feature**: Uses branch name as label

---

## Common Tasks

### Fix Build Issues

1. **Remove duplicate IClearable**
   ```bash
   # Edit dotnet/framework/src/Plate.ModernSatsuma/Utils.cs
   # Delete lines 12-17 (IClearable interface)
   ```

2. **Handle Drawing.cs**
   ```bash
   # Option A: Rename to disable
   mv dotnet/framework/src/Plate.ModernSatsuma/Drawing.cs \
      dotnet/framework/src/Plate.ModernSatsuma/Drawing.cs.DISABLED
   ```

3. **Rebuild**
   ```bash
   cd dotnet/framework
   dotnet clean
   dotnet build
   ```

### Add New Algorithm

1. Create new `.cs` file in `dotnet/framework/src/Plate.ModernSatsuma/`
2. Use namespace `Plate.ModernSatsuma`
3. Follow existing patterns (e.g., see `Dijsktra.cs`)
4. Add unit tests in `dotnet/framework/tests/Plate.ModernSatsuma.Tests/`
5. Update documentation

### Update Documentation

1. Add/update XML comments in code
2. Update README.md if public API changes
3. Add examples if introducing new features
4. Update CHANGELOG (when created)

---

## Supported AI Systems

### Claude Code
- **File**: `CLAUDE.md` (optional, agent-specific)
- **Rules**: Reads `.agent/local/` automatically
- **This file**: Uses AGENTS.md for quick reference

### Windsurf/Cascade
- **File**: This file (AGENTS.md)
- **Rules**: Reads `.agent/local/` automatically
- **Context**: Full project context aware

### GitHub Copilot
- **File**: `.github/copilot-instructions.md` (create if needed)
- **Rules**: Needs explicit pointers to `.agent/` files

---

## Next Steps

1. **Fix Build Issues** (Priority 1)
   - Remove duplicate IClearable
   - Handle Drawing.cs dependencies
   - Verify build succeeds

2. **Verify Functionality** (Priority 2)
   - Run existing tests
   - Compare with original Satsuma
   - Document any differences

3. **Quality Improvements** (Priority 3)
   - Address nullable reference warnings
   - Fix XML documentation
   - Add missing tests

---

## Related Projects

- **Original**: `winged-bean/ref-projects/satsumagraph-code` (reference)
- **Cross-Milo**: Service abstraction framework (uses graph algorithms)
- **Plugin-Manoi**: Plugin system (may use graph dependency resolution)

---

## License

Based on original Satsuma Graph Library by Balázs Szalkai  
License: zlib/libpng (permissive)  
See LICENSE file for details

---

## Quick References

- Solution: `dotnet/framework/Plate.ModernSatsuma.sln`
- Source: `dotnet/framework/src/Plate.ModernSatsuma/`
- Tests: `dotnet/framework/tests/Plate.ModernSatsuma.Tests/`
- Docs: `docs/`
- Config: `.plate-validators.yaml`, `GitVersion.yml`, `.pre-commit-config.yaml`
