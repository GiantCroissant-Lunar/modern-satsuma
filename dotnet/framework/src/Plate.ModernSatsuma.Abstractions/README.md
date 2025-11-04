# Plate.ModernSatsuma.Abstractions

Platform-agnostic drawing abstractions for Modern Satsuma graph visualization.

## Overview

This package provides interfaces and abstractions for graph drawing functionality that can be implemented by different rendering engines. It allows the core Modern Satsuma library to remain platform-independent while enabling graph visualization through pluggable rendering backends.

## Architecture

The abstraction layer separates the graph drawing logic from the actual rendering implementation:

```
┌─────────────────────────────────────┐
│  Plate.ModernSatsuma               │
│  (Core graph algorithms & data)    │
└──────────────┬──────────────────────┘
               │
               ▼
┌─────────────────────────────────────┐
│  Plate.ModernSatsuma.Abstractions  │
│  (Drawing interfaces)              │
└──────────────┬──────────────────────┘
               │
               ▼
┌─────────────────────────────────────┐
│  Rendering Implementation          │
│  - SystemDrawing (Windows-focused) │
│  - SkiaSharp (cross-platform)      │
│  - ImageSharp (pure C#)            │
│  - Custom implementations          │
└─────────────────────────────────────┘
```

## Key Interfaces

### IGraphicsContext
Core drawing interface providing methods for rendering primitives:
- Lines, ellipses, polygons
- Text rendering
- Coordinate transformations
- State management (save/restore)

### INodeShape
Defines how graph nodes are visually represented:
- Size and boundary calculation
- Drawing logic independent of rendering backend

### IGraphDrawer
High-level interface for drawing entire graphs:
- Node positioning
- Arc rendering
- Style application

### IRenderSurface
Represents a drawable surface that can be saved to various image formats.

## Usage

This package is typically not used directly. Instead, install a rendering implementation:

```bash
# For Windows-focused applications
dotnet add package Plate.ModernSatsuma.Drawing.SystemDrawing

# For cross-platform applications (coming soon)
dotnet add package Plate.ModernSatsuma.Drawing.SkiaSharp
```

## Creating Custom Implementations

To create your own rendering backend:

1. Implement `IRenderSurfaceFactory`
2. Implement `IGraphicsContext` for your rendering engine
3. Implement `IGraphicsFactory` for creating pens, brushes, and fonts
4. Optionally implement `INodeShape` and `INodeStyle` for custom node rendering

See `Plate.ModernSatsuma.Drawing.SystemDrawing` for a complete reference implementation.

## Benefits

- **Platform Independence**: Core library doesn't depend on platform-specific drawing APIs
- **Flexibility**: Swap rendering engines without changing graph code
- **Testability**: Mock drawing operations for unit tests
- **Future-Proof**: Easy to adopt new rendering technologies

## License

Zlib License - See LICENSE file in repository root.
