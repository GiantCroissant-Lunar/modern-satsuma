# Plate.ModernSatsuma.Drawing.SystemDrawing

System.Drawing-based implementation of Modern Satsuma graph visualization.

## Overview

This package provides a concrete implementation of the `Plate.ModernSatsuma.Abstractions` drawing interfaces using `System.Drawing.Common`. It enables graph visualization using the GDI+ graphics API.

## Platform Support

⚠️ **Important**: `System.Drawing.Common` is primarily designed for Windows. While it can work on Linux/macOS with libgdiplus, it has limitations and is officially deprecated for cross-platform use in .NET Core 3.0+.

**Recommended for:**
- Windows-only applications
- .NET Framework applications
- Legacy system compatibility

**For cross-platform applications**, consider using:
- `Plate.ModernSatsuma.Drawing.SkiaSharp` (when available) - hardware-accelerated, truly cross-platform
- `Plate.ModernSatsuma.Drawing.ImageSharp` (when available) - pure C#, cross-platform

## Installation

```bash
dotnet add package Plate.ModernSatsuma.Drawing.SystemDrawing
```

## Usage

### Basic Example

```csharp
using Plate.ModernSatsuma;
using Plate.ModernSatsuma.Drawing.SystemDrawing;
using Plate.ModernSatsuma.Abstractions;

// Create a graph
var graph = new CompleteGraph(7);

// Compute layout
var layout = new ForceDirectedLayout(graph);
layout.Run();

// Create drawing components
var factory = new SystemDrawingRenderSurfaceFactory();
var nodeShape = new NodeShape(NodeShapeKind.Diamond, new Size2D(40, 40));
var nodeStyle = new NodeStyle(factory.GraphicsFactory)
{
    Brush = factory.GraphicsFactory.CreateBrush(Color.Yellow),
    Shape = nodeShape
};

// Create drawer
var drawer = new GraphDrawer(graph, factory.GraphicsFactory)
{
    NodePosition = node => new Point2D(
        layout.NodePositions[node].X,
        layout.NodePositions[node].Y
    ),
    NodeCaption = node => graph.GetNodeIndex(node).ToString(),
    NodeStyle = node => nodeStyle
};

// Draw and save
using var surface = drawer.Draw(factory, 300, 300, Color.White);
surface.Save("graph.png");
```

### Customizing Styles

```csharp
// Create custom pens and brushes
var redPen = factory.GraphicsFactory.CreatePen(Color.Red, width: 2.0);
var blueBrush = factory.GraphicsFactory.CreateBrush(Color.Blue);

// Use different styles per node
var drawer = new GraphDrawer(graph, factory.GraphicsFactory)
{
    NodeStyle = node =>
    {
        var index = graph.GetNodeIndex(node);
        var style = new NodeStyle(factory.GraphicsFactory);
        
        if (index % 2 == 0)
        {
            style.Brush = factory.GraphicsFactory.CreateBrush(Color.Red);
            style.Shape = new NodeShape(NodeShapeKind.Rectangle, new Size2D(30, 30));
        }
        else
        {
            style.Brush = factory.GraphicsFactory.CreateBrush(Color.Blue);
            style.Shape = new NodeShape(NodeShapeKind.Ellipse, new Size2D(30, 30));
        }
        
        return style;
    }
};
```

## Components

### SystemDrawingRenderSurfaceFactory
Factory for creating render surfaces and graphics objects.

### SystemDrawingGraphicsContext
Graphics context implementation wrapping `System.Drawing.Graphics`.

### NodeShape
Standard node shape implementation supporting:
- Diamond
- Ellipse
- Rectangle
- Triangle
- UpsideDownTriangle

### NodeStyle
Visual style configuration for nodes including:
- Pen (outline)
- Brush (fill)
- Shape
- Text font and brush

### GraphDrawer
High-level API for drawing entire graphs with automatic layout scaling.

## Migration from Original Satsuma

If you were using the original Satsuma library's `Drawing.cs`:

**Before:**
```csharp
using Satsuma.Drawing;
using System.Drawing;

var drawer = new GraphDrawer
{
    Graph = graph,
    NodePositions = node => (PointF)layout.NodePositions[node]
    // ...
};

var bitmap = drawer.Draw(300, 300, Color.White);
bitmap.Save("graph.png");
```

**After:**
```csharp
using Plate.ModernSatsuma;
using Plate.ModernSatsuma.Drawing.SystemDrawing;
using Plate.ModernSatsuma.Abstractions;

var factory = new SystemDrawingRenderSurfaceFactory();
var drawer = new GraphDrawer(graph, factory.GraphicsFactory)
{
    NodePosition = node => new Point2D(
        layout.NodePositions[node].X,
        layout.NodePositions[node].Y
    )
    // ...
};

using var surface = drawer.Draw(factory, 300, 300, Color.White);
surface.Save("graph.png");
```

## Troubleshooting

### Linux/macOS Issues

If you encounter issues on Linux/macOS, you need to install libgdiplus:

**Ubuntu/Debian:**
```bash
sudo apt-get install libgdiplus
```

**macOS:**
```bash
brew install mono-libgdiplus
```

**Note**: Even with libgdiplus, some features may not work correctly. For better cross-platform support, consider using a SkiaSharp-based implementation instead.

## License

Zlib License - See LICENSE file in repository root.

## See Also

- [Plate.ModernSatsuma](../Plate.ModernSatsuma/) - Core graph library
- [Plate.ModernSatsuma.Abstractions](../Plate.ModernSatsuma.Abstractions/) - Drawing abstractions
