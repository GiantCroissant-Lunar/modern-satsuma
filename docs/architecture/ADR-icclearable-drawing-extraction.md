# ADR: IClearable Location and Drawing Extraction

**Status**: Accepted  
**Date**: 2025-10-14 (documented retroactively)

## Context

The ModernSatsuma project is a modernization of the original Satsuma Graph Library. During early modernization passes, two changes introduced critical build issues:

1. **Duplicate `IClearable` interface**  
   - `IClearable` was defined in both `Graph.cs` and `Utils.cs` under the `Plate.ModernSatsuma` namespace.  
   - This caused a `CS0101` compilation error (type defined multiple times).

2. **System.Drawing dependency in `Drawing.cs`**  
   - `Drawing.cs` referenced `System.Drawing`, `System.Drawing.Drawing2D`, and `System.Drawing.Imaging` types.  
   - The core library targets .NET Standard (now `netstandard2.1`), where `System.Drawing` is either unavailable or deprecated for cross‑platform use.  
   - This made the core package non-portable and blocked builds on non-Windows platforms without additional, deprecated packages.

At the same time, the project needed to:

- Preserve the original Satsuma public API surface as much as possible.
- Keep the core library platform-independent and suitable as a reusable graph kernel.
- Provide a path for modern, cross-platform rendering (SkiaSharp, etc.).

## Decision

1. **Single source of truth for `IClearable`**

   - Keep `IClearable` defined **only** in `Graph.cs` in the `Plate.ModernSatsuma` namespace.
   - Remove the duplicate definition from `Utils.cs`.
   - Treat `Graph.cs` as the canonical home for graph-related interfaces (`IClearable`, `IGraph`, `IArcLookup`, etc.).

2. **Extract drawing into separate packages; exclude `Drawing.cs` from core**

   - The core library project `Plate.ModernSatsuma` (targeting `netstandard2.1`) **does not compile `Drawing.cs`**.  
     - `Drawing.cs` is kept in the repository only as a historical reference to the original Satsuma drawing implementation.
   - Drawing and layout rendering are moved to a pluggable architecture:
     - `Plate.ModernSatsuma.Abstractions` – platform-agnostic drawing/layout interfaces and primitives.  
     - `Plate.ModernSatsuma.Drawing.SystemDrawing` – implementation using `System.Drawing` for Windows-centric scenarios.  
     - `Plate.ModernSatsuma.Drawing.SkiaSharp` – cross-platform implementation using SkiaSharp.
   - The core library remains responsible for:
     - Graph data structures and algorithms (Dijkstra, A*, BFS/DFS, flows, matching, LP, etc.).
     - Layout algorithms (e.g., force-directed layout) expressed in terms of numeric coordinates and simple geometry types.

3. **Document the change and treat old analysis as historical**

   - `MODERNIZATION_ANALYSIS.md`, `FIX_ACTION_PLAN.md`, `STRUCTURE_ALIGNMENT.md`, and `AGENTS.md` are updated to:
     - Mark the duplicate `IClearable` and `Drawing.cs` issues as **historical**.
     - State that `IClearable` now lives only in `Graph.cs`.
     - Explain that drawing was extracted into dedicated renderer packages, and `Drawing.cs` is excluded from the core project.

## Consequences

### Positive

- **Build stability**  
  - The core `Plate.ModernSatsuma` project now builds cleanly targeting `netstandard2.1` without requiring `System.Drawing.Common` or other platform-specific packages.

- **Platform independence**  
  - Consumers can use core graph algorithms on any platform supported by .NET Standard 2.1.  
  - Rendering concerns are isolated to optional packages (SystemDrawing, SkiaSharp), which can choose their own target frameworks and dependencies.

- **Cleaner architecture**  
  - Graph kernel (data structures + algorithms) is separated from visualization concerns.  
  - Drawing implementations can evolve independently (e.g., add ImageSharp-based renderer) without impacting the core API.

- **Reduced ambiguity**  
  - `IClearable` has a single, well-defined location (`Graph.cs`), reducing confusion for contributors and tools.

### Negative / Trade-offs

- **Breaking change for existing consumers relying on `Drawing.cs` in core**  
  - Code that previously referenced `Plate.ModernSatsuma.Drawing` types from the core assembly must now reference one of the renderer packages.
  - Some APIs in the extracted renderers may not be 1:1 with the original `Drawing.cs` signatures (due to modernization and abstraction).

- **Slightly more complex dependency graph**  
  - Consumers that need visualization must add an extra package reference (e.g., `Plate.ModernSatsuma.Drawing.SkiaSharp`).

### Risks and Mitigations

- **Risk: Incomplete parity with original drawing API**  
  - Mitigation: Keep `Drawing.cs` in the repo as reference; design abstractions to cover the common scenarios and document migration paths.

- **Risk: Confusion from historical docs**  
  - Mitigation: Clearly label old analysis documents as historical and note that the build issues have been resolved.

## Alternatives Considered

1. **Keep `Drawing.cs` in core and reference System.Drawing.Common**
   - Rejected due to:
     - Cross-platform limitations and deprecation of System.Drawing.Common outside Windows.
     - Unnecessary coupling between core algorithms and a specific rendering technology.

2. **Remove drawing functionality entirely**
   - Rejected because visualization is valuable for many users and examples; better to provide optional packages.

3. **Keep duplicate `IClearable` and suppress the compiler error**
   - Rejected; this would be fragile, confusing, and contrary to good design.

## Implementation Notes

- Core project:
  - `Plate.ModernSatsuma` targets **`netstandard2.1`** and excludes `Drawing.cs` from compilation.
  - `IClearable` is defined only in `Graph.cs`.

- Renderer projects:
  - `Plate.ModernSatsuma.Abstractions` defines drawing/layout abstractions (interfaces, geometry types).
  - `Plate.ModernSatsuma.Drawing.SystemDrawing` and `Plate.ModernSatsuma.Drawing.SkiaSharp` implement these abstractions using platform-specific graphics libraries.

- Documentation updates:
  - AGENTS.md and related docs now describe the new architecture and mark the original issues as resolved.
