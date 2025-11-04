# Drawing Functionality Extraction

## Overview

The drawing functionality from the original Satsuma library has been extracted into a separate, pluggable architecture. This document explains the extraction process, architecture, and benefits.

## Problem Statement

The original `Drawing.cs` file had a Windows-only dependency on `System.Drawing`, which:
- Made the core library platform-dependent
- Required System.Drawing.Common package (deprecated for cross-platform use)
- Prevented the library from being truly cross-platform
- Mixed graph algorithms with rendering concerns

## Solution

We've extracted drawing into a layered architecture:

### Layer 1: Core Library (Plate.ModernSatsuma)
**Purpose**: Platform-independent graph algorithms and data structures

**Contents**:
- All graph algorithms (Dijkstra, BFS, DFS, flow, matching, etc.)
- Graph data structures (CompleteGraph, Path, CustomGraph, etc.)
- `ForceDirectedLayout` - layout algorithm (moved from Layout.cs)
- `PointD` struct - coordinate representation
- GraphML I/O and other utilities

**Dependencies**: None (except standard library)

### Layer 2: Abstractions (Plate.ModernSatsuma.Abstractions)
**Purpose**: Platform-agnostic drawing interfaces

**Contents**:
- `IGraphicsContext` - Drawing primitives interface
- `INodeShape` - Node shape abstraction
- `IGraphDrawer` - Graph drawing interface
- `IRenderSurface` - Drawable surface interface
- `IGraphicsFactory` - Factory for creating drawing objects
- Geometric primitives: `Point2D`, `Size2D`, `Rectangle2D`, `Color`

**Dependencies**: Plate.ModernSatsuma (for Node and Arc types)

### Layer 3: Implementations
**Purpose**: Concrete rendering backends

**Current Implementations**:

#### Plate.ModernSatsuma.Drawing.SystemDrawing
- System.Drawing.Common-based implementation
- Windows-focused (works on Linux/macOS with libgdiplus but not recommended)
- Complete implementation of all abstractions
- Supports PNG, JPEG, BMP, GIF output
- **Status**: ✅ Available

#### Plate.ModernSatsuma.Drawing.SkiaSharp
- SkiaSharp-based implementation
- Truly cross-platform (Windows, Linux, macOS, mobile, WebAssembly)
- Hardware-accelerated rendering with GPU support
- High-quality anti-aliasing
- Supports PNG, JPEG, BMP, GIF output
- **Status**: ✅ Available

**Planned Implementations**:

#### Plate.ModernSatsuma.Drawing.ImageSharp (Future)
- ImageSharp-based implementation
- Pure C#, no native dependencies
- Cross-platform
- **Status**: 📋 Planned

## Architecture Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                    Application Code                         │
│  (Uses IGraphDrawer, IGraphicsFactory, etc.)               │
└────────────────────────────┬────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────┐
│            Plate.ModernSatsuma.Abstractions                 │
│  - IGraphicsContext, INodeShape, IGraphDrawer               │
│  - Point2D, Size2D, Rectangle2D, Color                      │
│  - Platform-agnostic interfaces                             │
└────────────────────────────┬────────────────────────────────┘
                             │
                ┌────────────┴────────────┐
                ▼                         ▼
┌──────────────────────────┐  ┌──────────────────────────┐
│  SystemDrawing Package   │  │  Future Implementations  │
│  (Windows-focused)       │  │  - SkiaSharp             │
│                          │  │  - ImageSharp            │
│  Uses:                   │  │  - Custom renderers      │
│  - System.Drawing.Common │  │                          │
│  - GDI+ on Windows       │  │                          │
└──────────────────────────┘  └──────────────────────────┘
                             ▲
                             │
                    ┌────────┴────────┐
                    │                 │
┌───────────────────▼─────┐  ┌────────▼─────────────────┐
│ Plate.ModernSatsuma     │  │  Other Libraries        │
│ (Core graph library)    │  │  - xterm.js (if needed) │
│ - Algorithms            │  │  - Visualization tools  │
│ - Data structures       │  │                         │
│ - ForceDirectedLayout   │  │                         │
└─────────────────────────┘  └─────────────────────────┘
```

## File Changes

### New Files Created

#### Abstractions Package
- `Plate.ModernSatsuma.Abstractions/Plate.ModernSatsuma.Abstractions.csproj`
- `Plate.ModernSatsuma.Abstractions/IGraphicsContext.cs` - Graphics primitives
- `Plate.ModernSatsuma.Abstractions/INodeShape.cs` - Node shape abstraction
- `Plate.ModernSatsuma.Abstractions/IGraphDrawer.cs` - Graph drawer interface
- `Plate.ModernSatsuma.Abstractions/README.md` - Documentation

#### SystemDrawing Package
- `Plate.ModernSatsuma.Drawing.SystemDrawing/Plate.ModernSatsuma.Drawing.SystemDrawing.csproj`
- `Plate.ModernSatsuma.Drawing.SystemDrawing/SystemDrawingAdapter.cs` - Graphics context impl
- `Plate.ModernSatsuma.Drawing.SystemDrawing/NodeShape.cs` - Node shape impl
- `Plate.ModernSatsuma.Drawing.SystemDrawing/NodeStyle.cs` - Node style impl
- `Plate.ModernSatsuma.Drawing.SystemDrawing/GraphDrawer.cs` - Graph drawer impl
- `Plate.ModernSatsuma.Drawing.SystemDrawing/README.md` - Documentation

#### SkiaSharp Package
- `Plate.ModernSatsuma.Drawing.SkiaSharp/Plate.ModernSatsuma.Drawing.SkiaSharp.csproj`
- `Plate.ModernSatsuma.Drawing.SkiaSharp/SkiaSharpAdapter.cs` - Graphics context impl
- `Plate.ModernSatsuma.Drawing.SkiaSharp/NodeShape.cs` - Node shape impl
- `Plate.ModernSatsuma.Drawing.SkiaSharp/NodeStyle.cs` - Node style impl
- `Plate.ModernSatsuma.Drawing.SkiaSharp/GraphDrawer.cs` - Graph drawer impl
- `Plate.ModernSatsuma.Drawing.SkiaSharp/README.md` - Documentation

#### Core Library
- `Plate.ModernSatsuma/GraphLayout.cs` - Layout algorithms (extracted from Layout.cs)

### Modified Files

#### Core Library
- `Plate.ModernSatsuma/Plate.ModernSatsuma.csproj` - Updated to exclude Drawing.cs and Layout.cs
- Original `Drawing.cs` and `Layout.cs` retained for reference but excluded from build

#### Documentation
- `docs/KNOWN_LIMITATIONS.md` - Updated to explain new architecture
- `docs/DRAWING_EXTRACTION.md` - This file

#### Build Configuration
- `dotnet/framework/Directory.Packages.props` - Added System.Drawing.Common and SkiaSharp versions

## Benefits

### 1. Platform Independence
The core library no longer depends on System.Drawing, making it truly cross-platform for .NET Standard 2.1+.

### 2. Separation of Concerns
- Graph algorithms are separate from visualization
- Rendering logic is separate from graph logic
- Each layer has a single, well-defined responsibility

### 3. Testability
- Can mock drawing operations for unit tests
- Can test graph algorithms without rendering
- Can test rendering independently

### 4. Flexibility
- Users can choose their preferred rendering backend
- Easy to add new rendering implementations
- Can swap rendering engines without changing graph code

### 5. Future-Proof
- Easy to adopt new rendering technologies (e.g., WebAssembly, GPU acceleration)
- Can support multiple rendering backends simultaneously
- Architecture supports custom rendering implementations

### 6. Backward Compatibility Path
While the API has changed, migration is straightforward with clear examples provided.

## Usage Examples

### Basic Graph Drawing (SkiaSharp - Recommended)

```csharp
using Plate.ModernSatsuma;
using Plate.ModernSatsuma.Drawing.SkiaSharp;
using Plate.ModernSatsuma.Abstractions;

// Create and layout graph
var graph = new CompleteGraph(7);
var layout = new ForceDirectedLayout(graph);
layout.Run();

// Create SkiaSharp rendering factory
var factory = new SkiaSharpRenderSurfaceFactory();

// Create and configure drawer
var drawer = new GraphDrawer(graph, factory.GraphicsFactory)
{
    NodePosition = node => new Point2D(
        layout.NodePositions[node].X,
        layout.NodePositions[node].Y
    ),
    NodeCaption = node => $"N{graph.GetNodeIndex(node)}"
};

// Render and save
using var surface = drawer.Draw(factory, 800, 600, Color.White);
surface.Save("graph.png");
```

```csharp
using Plate.ModernSatsuma;
using Plate.ModernSatsuma.Drawing.SystemDrawing;
using Plate.ModernSatsuma.Abstractions;

// Create and layout graph
var graph = new CompleteGraph(7);
var layout = new ForceDirectedLayout(graph);
layout.Run();

// Create rendering factory
var factory = new SystemDrawingRenderSurfaceFactory();

// Create and configure drawer
var drawer = new GraphDrawer(graph, factory.GraphicsFactory)
{
    NodePosition = node => new Point2D(
        layout.NodePositions[node].X,
        layout.NodePositions[node].Y
    ),
    NodeCaption = node => $"N{graph.GetNodeIndex(node)}"
};

// Render and save
using var surface = drawer.Draw(factory, 800, 600, Color.White);
surface.Save("graph.png");
```

### Custom Node Styles

```csharp
var nodeStyles = new Dictionary<Node, INodeStyle>();
foreach (var node in graph.Nodes())
{
    var style = new NodeStyle(factory.GraphicsFactory);
    
    // Color by degree
    int degree = graph.Arcs(node).Count();
    if (degree > 5)
        style.Brush = factory.GraphicsFactory.CreateBrush(Color.Red);
    else if (degree > 3)
        style.Brush = factory.GraphicsFactory.CreateBrush(Color.Yellow);
    else
        style.Brush = factory.GraphicsFactory.CreateBrush(Color.Green);
    
    nodeStyles[node] = style;
}

var drawer = new GraphDrawer(graph, factory.GraphicsFactory)
{
    NodePosition = /* ... */,
    NodeStyle = node => nodeStyles[node]
};
```

## Migration Guide

For code using the original Satsuma drawing API:

**Before:**
```csharp
using Satsuma.Drawing;
var drawer = new GraphDrawer { Graph = graph, /* ... */ };
var bitmap = drawer.Draw(300, 300, Color.White);
```

**After:**
```csharp
using Plate.ModernSatsuma.Drawing.SystemDrawing;
using Plate.ModernSatsuma.Abstractions;

var factory = new SystemDrawingRenderSurfaceFactory();
var drawer = new GraphDrawer(graph, factory.GraphicsFactory) { /* ... */ };
using var surface = drawer.Draw(factory, 300, 300, Color.White);
```

See package README files for detailed migration examples.

## Implementation Notes

### Why Interfaces Instead of Abstract Classes?
- More flexible for implementers
- Allows composition over inheritance
- Better for dependency injection and testing
- C# doesn't support multiple inheritance

### Why Separate Factory Interfaces?
- Allows different backends to create their own drawing objects
- Ensures type safety at compile time
- Makes it explicit when you're working with backend-specific objects

### Why Point2D in Abstractions vs PointD in Core?
- `PointD` is for layout coordinates (graph algorithms)
- `Point2D` is for drawing coordinates (rendering)
- Separation allows independent evolution of each type
- Conversion between them is simple and explicit

## Future Enhancements

### Short Term
- ✅ Complete SystemDrawing implementation
- ✅ Complete SkiaSharp implementation
- 📋 Add XML documentation
- 📋 Add usage examples
- 📋 Add unit tests for abstractions

### Medium Term
- 📋 Implement ImageSharp backend
- 📋 Add SVG export capability
- 📋 Add more node shape options
- 📋 Add arc styling options

### Long Term
- 📋 WebGL/WebGPU backend for browser rendering
- 📋 GPU-accelerated layout algorithms
- 📋 Interactive visualization support
- 📋 Animation support

## Contributing

To add a new rendering backend:

1. Create a new project: `Plate.ModernSatsuma.Drawing.{YourBackend}`
2. Reference `Plate.ModernSatsuma.Abstractions`
3. Implement:
   - `IRenderSurfaceFactory`
   - `IRenderSurface`
   - `IGraphicsContext`
   - `IGraphicsFactory`
   - Optionally: `INodeShape`, `INodeStyle`
4. Add documentation and examples
5. Submit a pull request

See `Plate.ModernSatsuma.Drawing.SystemDrawing` as a reference implementation.

## Questions?

- Check package README files
- See updated [KNOWN_LIMITATIONS.md](KNOWN_LIMITATIONS.md)
- Open an issue on GitHub
- Review [CONTRIBUTING.md](../CONTRIBUTING.md)

---

**Version**: 1.1.0-dev  
**Date**: 2025-11-02  
**Status**: Implementation Complete  
**Available**: SystemDrawing ✅, SkiaSharp ✅
