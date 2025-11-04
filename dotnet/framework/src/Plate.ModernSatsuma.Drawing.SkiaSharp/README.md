# Plate.ModernSatsuma.Drawing.SkiaSharp

SkiaSharp-based implementation of Modern Satsuma graph visualization - truly cross-platform and hardware-accelerated.

## Overview

This package provides a concrete implementation of the `Plate.ModernSatsuma.Abstractions` drawing interfaces using SkiaSharp, Google's 2D graphics library. It enables high-quality, hardware-accelerated graph visualization across all platforms.

## Platform Support

✅ **Truly Cross-Platform**: Works everywhere SkiaSharp is supported:

- **Desktop**: Windows, Linux, macOS
- **Mobile**: iOS, Android, Tizen
- **Web**: WebAssembly (Blazor)
- **Server**: Headless rendering on any platform
- **IoT**: Raspberry Pi and other embedded devices

### Why SkiaSharp?

- 🚀 **Hardware-Accelerated** - Uses GPU when available for better performance
- 🌍 **Cross-Platform** - Single codebase works on all platforms
- 🎨 **High Quality** - Advanced anti-aliasing and rendering features
- ⚡ **Fast** - Optimized C++ engine with .NET bindings
- 🔧 **Well-Maintained** - Actively developed by Microsoft and community
- 📦 **Production-Ready** - Used by Xamarin.Forms, SkiaSharp.Views, and many apps

## Installation

```bash
dotnet add package Plate.ModernSatsuma.Drawing.SkiaSharp
```

## Usage

### Basic Example

```csharp
using Plate.ModernSatsuma;
using Plate.ModernSatsuma.Drawing.SkiaSharp;
using Plate.ModernSatsuma.Abstractions;

// Create a graph
var graph = new CompleteGraph(7);

// Compute layout
var layout = new ForceDirectedLayout(graph);
layout.Run();

// Create SkiaSharp rendering factory
var factory = new SkiaSharpRenderSurfaceFactory();

// Create node style
var nodeShape = new NodeShape(NodeShapeKind.Diamond, new Size2D(40, 40));
var nodeStyle = new NodeStyle(factory.GraphicsFactory)
{
    Brush = factory.GraphicsFactory.CreateBrush(Color.Yellow),
    Shape = nodeShape
};

// Create and configure drawer
var drawer = new GraphDrawer(graph, factory.GraphicsFactory)
{
    NodePosition = node => new Point2D(
        layout.NodePositions[node].X,
        layout.NodePositions[node].Y
    ),
    NodeCaption = node => $"N{graph.GetNodeIndex(node)}",
    NodeStyle = node => nodeStyle
};

// Render and save
using var surface = drawer.Draw(factory, 800, 600, Color.White);
surface.Save("graph.png");
```

### Advanced Styling

```csharp
var factory = new SkiaSharpRenderSurfaceFactory();

// Create gradient-like effect using different node colors
var colorPalette = new[]
{
    new Color(255, 0, 0),     // Red
    new Color(255, 127, 0),   // Orange
    new Color(255, 255, 0),   // Yellow
    new Color(0, 255, 0),     // Green
    new Color(0, 0, 255),     // Blue
    new Color(75, 0, 130),    // Indigo
    new Color(148, 0, 211)    // Violet
};

var nodeStyles = new Dictionary<Node, INodeStyle>();
var nodes = graph.Nodes().ToArray();

for (int i = 0; i < nodes.Length; i++)
{
    var style = new NodeStyle(factory.GraphicsFactory);
    style.Brush = factory.GraphicsFactory.CreateBrush(
        colorPalette[i % colorPalette.Length]
    );
    style.Shape = new NodeShape(
        NodeShapeKind.Ellipse, 
        new Size2D(30 + i * 2, 30 + i * 2)  // Growing size
    );
    nodeStyles[nodes[i]] = style;
}

var drawer = new GraphDrawer(graph, factory.GraphicsFactory)
{
    NodePosition = node => new Point2D(
        layout.NodePositions[node].X,
        layout.NodePositions[node].Y
    ),
    NodeStyle = node => nodeStyles[node],
    NodeCaption = node => $"Node {graph.GetNodeIndex(node)}"
};
```

### Custom Arc Styling

```csharp
// Style arcs based on graph properties
var drawer = new GraphDrawer(graph, factory.GraphicsFactory)
{
    NodePosition = /* ... */,
    ArcPen = arc =>
    {
        // Make edges thicker than directed arcs
        if (graph.IsEdge(arc))
            return factory.GraphicsFactory.CreatePen(Color.Blue, width: 2.0);
        else
            return factory.GraphicsFactory.CreatePen(Color.Red, width: 1.0, arrowCap: true);
    }
};
```

### Different Image Formats

```csharp
using var surface = drawer.Draw(factory, 800, 600, Color.White);

// PNG (default, lossless)
surface.Save("graph.png", ImageFormat.Png);

// JPEG (smaller file, lossy)
surface.Save("graph.jpg", ImageFormat.Jpeg);

// BMP (uncompressed)
surface.Save("graph.bmp", ImageFormat.Bmp);

// Save to stream
using var memoryStream = new MemoryStream();
surface.Save(memoryStream, ImageFormat.Png);
byte[] imageData = memoryStream.ToArray();
```

## Components

### SkiaSharpRenderSurfaceFactory
Factory for creating SkiaSharp render surfaces and graphics objects.

### SkiaSharpGraphicsContext
Hardware-accelerated graphics context implementation using SkiaSharp's SKCanvas.

Features:
- Hardware-accelerated rendering
- High-quality anti-aliasing
- Accurate text measurement and rendering
- Custom arrow cap rendering for directed graphs

### NodeShape
Standard node shape implementation supporting:
- **Diamond** - Four-pointed diamond shape
- **Ellipse** - Circular/oval shape
- **Rectangle** - Rectangular/square shape
- **Triangle** - Upward-pointing triangle
- **UpsideDownTriangle** - Downward-pointing triangle

### NodeStyle
Visual style configuration for nodes including:
- **Pen** - Outline color and width
- **Brush** - Fill color
- **Shape** - Node geometry
- **TextFont** - Caption font (family, size, bold, italic)
- **TextBrush** - Caption color

### GraphDrawer
High-level API for drawing entire graphs with:
- Automatic layout scaling to fit bounding box
- Customizable node positions, styles, and captions
- Customizable arc styling and arrow caps
- Support for both directed and undirected graphs

## Performance Tips

### For Large Graphs (100+ nodes)

```csharp
// 1. Pre-create and reuse styles
var defaultStyle = new NodeStyle(factory.GraphicsFactory);
var drawer = new GraphDrawer(graph, factory.GraphicsFactory)
{
    NodeStyle = _ => defaultStyle  // Don't create new style per call
};

// 2. Simplify shapes for dense graphs
var simpleShape = new NodeShape(NodeShapeKind.Ellipse, new Size2D(10, 10));

// 3. Disable captions for very large graphs
var drawer = new GraphDrawer(graph, factory.GraphicsFactory)
{
    NodeCaption = _ => ""  // No text rendering overhead
};
```

### Hardware Acceleration

SkiaSharp automatically uses hardware acceleration when available. On systems with GPU support, rendering will be significantly faster than software-only solutions.

## Comparison with System.Drawing

| Feature | SkiaSharp | System.Drawing |
|---------|-----------|----------------|
| **Cross-Platform** | ✅ True (all platforms) | ⚠️ Windows-focused |
| **Performance** | ✅ Hardware-accelerated | ⚠️ Software-only on non-Windows |
| **Quality** | ✅ Excellent anti-aliasing | ✅ Good |
| **Mobile Support** | ✅ iOS, Android | ❌ No |
| **WebAssembly** | ✅ Yes (Blazor) | ❌ No |
| **Maintenance** | ✅ Actively developed | ⚠️ Deprecated for cross-platform |
| **Installation** | Simple NuGet | Requires libgdiplus on Linux/macOS |

**Recommendation**: Use SkiaSharp for new projects, especially if cross-platform support is needed.

## Platform-Specific Notes

### Windows
Works out of the box, no additional setup needed. Uses GPU acceleration automatically.

### Linux
Works out of the box, no dependencies needed (unlike System.Drawing which requires libgdiplus).

```bash
# No installation required - SkiaSharp includes native libraries!
```

### macOS
Works out of the box on both Intel and Apple Silicon Macs.

### Docker/Headless
Perfect for server-side graph generation:

```dockerfile
FROM mcr.microsoft.com/dotnet/runtime:6.0
# No additional dependencies needed for SkiaSharp!
COPY --from=build /app .
ENTRYPOINT ["dotnet", "YourApp.dll"]
```

### Blazor WebAssembly
Can be used for client-side graph visualization in web browsers.

## Migration from System.Drawing

The API is identical! Just change the factory:

**Before:**
```csharp
using Plate.ModernSatsuma.Drawing.SystemDrawing;
var factory = new SystemDrawingRenderSurfaceFactory();
```

**After:**
```csharp
using Plate.ModernSatsuma.Drawing.SkiaSharp;
var factory = new SkiaSharpRenderSurfaceFactory();
```

Everything else remains the same!

## Troubleshooting

### "Unable to load DLL 'libSkiaSharp'"

This usually means the native SkiaSharp libraries weren't copied to the output directory.

**Solution**:
```xml
<ItemGroup>
  <PackageReference Include="SkiaSharp" />
  <PackageReference Include="SkiaSharp.NativeAssets.Linux" Condition="'$(OS)' == 'Unix'" />
</ItemGroup>
```

### Performance Issues

If rendering is slow:
1. Check if you're recreating styles on every call (use caching)
2. Verify hardware acceleration is working (GPU should be used)
3. Consider reducing node count or simplifying shapes

### Font Not Found

SkiaSharp uses system fonts. If a font isn't available:

```csharp
// Use a common fallback font
var font = factory.GraphicsFactory.CreateFont("Arial", 12);
// or
var font = factory.GraphicsFactory.GetDefaultFont();
```

## Examples

See the [examples directory](../../examples/SkiaSharp/) for complete working examples:

- `BasicGraph.cs` - Simple graph rendering
- `StyledGraph.cs` - Custom colors and shapes
- `LargeGraph.cs` - Performance optimization techniques
- `AnimatedGraph.cs` - Creating animated visualizations

## License

Zlib License - See LICENSE file in repository root.

## See Also

- [Plate.ModernSatsuma](../Plate.ModernSatsuma/) - Core graph library
- [Plate.ModernSatsuma.Abstractions](../Plate.ModernSatsuma.Abstractions/) - Drawing abstractions
- [SkiaSharp Documentation](https://docs.microsoft.com/en-us/xamarin/xamarin-forms/user-interface/graphics/skiasharp/)
- [SkiaSharp GitHub](https://github.com/mono/SkiaSharp)
