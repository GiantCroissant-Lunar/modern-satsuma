# Known Limitations

## Drawing Functionality - Extracted to Separate Packages

The drawing functionality from the original Satsuma library has been **extracted and modernized** into a pluggable architecture.

### Architecture

Drawing capabilities are now available through separate packages:

```
Plate.ModernSatsuma (Core)
    ↓
Plate.ModernSatsuma.Abstractions (Interfaces)
    ↓
Implementation packages:
  - Plate.ModernSatsuma.Drawing.SystemDrawing (Available - Windows-focused)
  - Plate.ModernSatsuma.Drawing.SkiaSharp (Available - Cross-platform)
  - Plate.ModernSatsuma.Drawing.ImageSharp (Planned - Pure C#)
```

### Core Library Changes

The core `Plate.ModernSatsuma` library remains platform-independent:
- ✅ Layout algorithms (`ForceDirectedLayout`) moved to `GraphLayout.cs`
- ✅ Core `PointD` struct for layout coordinates
- ✅ All graph algorithms and data structures remain unchanged
- ⚠️ Original `Drawing.cs` and `Layout.cs` retained for reference but excluded from build

### Using Drawing Functionality

To use graph visualization:

1. **Install a rendering package:**
   ```bash
   # For cross-platform applications (RECOMMENDED)
   dotnet add package Plate.ModernSatsuma.Drawing.SkiaSharp
   
   # For Windows applications
   dotnet add package Plate.ModernSatsuma.Drawing.SystemDrawing
   
   # For pure C# (coming soon)
   dotnet add package Plate.ModernSatsuma.Drawing.ImageSharp
   ```

2. **Use the abstraction-based API:**
   ```csharp
   using Plate.ModernSatsuma;
   using Plate.ModernSatsuma.Drawing.SkiaSharp;  // or SystemDrawing
   using Plate.ModernSatsuma.Abstractions;
   
   var graph = new CompleteGraph(7);
   var layout = new ForceDirectedLayout(graph);
   layout.Run();
   
   var factory = new SkiaSharpRenderSurfaceFactory();  // or SystemDrawingRenderSurfaceFactory
   var drawer = new GraphDrawer(graph, factory.GraphicsFactory)
   {
       NodePosition = node => new Point2D(
           layout.NodePositions[node].X,
           layout.NodePositions[node].Y
       )
   };
   
   using var surface = drawer.Draw(factory, 300, 300, Color.White);
   surface.Save("graph.png");
   ```

### Available APIs

#### Core Library (Plate.ModernSatsuma)
- ✅ `ForceDirectedLayout` - Force-directed graph layout algorithm
- ✅ `PointD` - 2D point with double precision
- ✅ All graph algorithms and data structures

#### Abstractions (Plate.ModernSatsuma.Abstractions)
- ✅ `IGraphicsContext` - Platform-agnostic drawing interface
- ✅ `INodeShape` - Node shape interface
- ✅ `IGraphDrawer` - Graph drawing interface
- ✅ `IRenderSurface` - Drawable surface interface
- ✅ `Point2D`, `Size2D`, `Rectangle2D`, `Color` - Geometric primitives

#### SkiaSharp Implementation (Plate.ModernSatsuma.Drawing.SkiaSharp)
- ✅ `NodeShape` - Standard node shapes (Diamond, Ellipse, Rectangle, Triangle)
- ✅ `NodeStyle` - Node styling (pen, brush, font)
- ✅ `GraphDrawer` - Complete graph rendering
- ✅ Bitmap/PNG/JPEG/GIF/BMP export
- ✅ Truly cross-platform (Windows, Linux, macOS, mobile, WebAssembly)
- ✅ Hardware-accelerated rendering
- ✅ High-quality anti-aliasing

#### SystemDrawing Implementation (Plate.ModernSatsuma.Drawing.SystemDrawing)
- ✅ `NodeShape` - Standard node shapes (Diamond, Ellipse, Rectangle, Triangle)
- ✅ `NodeStyle` - Node styling (pen, brush, font)
- ✅ `GraphDrawer` - Complete graph rendering
- ✅ Bitmap/PNG/JPEG/GIF/BMP export
- ⚠️ Windows-focused (System.Drawing.Common)

### Alternative Visualization Methods

If you don't want to use the drawing packages:

1. **GraphML Export** (built into core library):
   ```csharp
   using (var writer = new StreamWriter("graph.graphml"))
   {
       graph.SaveGraphML(writer);
   }
   // Open in yEd, Gephi, or other GraphML-compatible viewer
   ```

2. **Export layout coordinates and visualize elsewhere**:
   ```csharp
   var layout = new ForceDirectedLayout(graph);
   layout.Run();
   foreach (var node in graph.Nodes())
   {
       var pos = layout.NodePositions[node];
       Console.WriteLine($"Node {node}: ({pos.X}, {pos.Y})");
   }
   ```

### Benefits of New Architecture

- ✅ **Platform Independence**: Core library has no System.Drawing dependency
- ✅ **Flexibility**: Swap rendering engines without changing graph code
- ✅ **Future-Proof**: Easy to add SkiaSharp, ImageSharp, or custom renderers
- ✅ **Testability**: Mock drawing operations for unit tests
- ✅ **Maintainability**: Separation of concerns between algorithms and visualization

### Migration from Original Satsuma

The API has changed slightly. See the README files in each package for migration examples:
- [Abstractions README](../dotnet/framework/src/Plate.ModernSatsuma.Abstractions/README.md)
- [SystemDrawing README](../dotnet/framework/src/Plate.ModernSatsuma.Drawing.SystemDrawing/README.md)

### Future Work

Planned rendering implementations:
- **ImageSharp**: Pure C#, cross-platform, no native dependencies

### Version Information

- **Since**: v1.1.0 (current development)
- **Status**: Architecture implemented
- **Available**: SystemDrawing ✅, SkiaSharp ✅
- **Original files**: Retained in source for reference but excluded from build

---

For questions or to contribute implementations for other rendering engines, see [CONTRIBUTING.md](../CONTRIBUTING.md) or open an issue in the repository.
