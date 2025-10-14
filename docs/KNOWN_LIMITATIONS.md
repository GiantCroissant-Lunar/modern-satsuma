# Known Limitations

## Drawing Functionality Excluded

The `Drawing.cs` module from the original Satsuma library is currently excluded from compilation.

### Reason

System.Drawing is not available in .NET Standard 2.0 without additional packages, and:
- `System.Drawing.Common` requires explicit NuGet package reference
- System.Drawing.Common is deprecated for cross-platform use in .NET Core 3.0+
- Modern alternatives (SkiaSharp, ImageSharp) require significant API changes

### Affected APIs

The following types and methods are **not available** in the current build:

- `INodeShape` interface
- `NodeShapeKind` enum
- `StandardShape` class
- `GraphDrawing` class
- `GraphDrawingExtensions` methods
- Bitmap/SVG/PNG rendering of graphs
- Graph visualization and layout drawing functionality

### Impact

You can still use all core graph algorithms (Dijkstra, BFS, DFS, network flow, matching, etc.) and graph data structures. Only the visual rendering capabilities are unavailable.

### Workarounds

If you need to visualize graphs:

1. **Use GraphML export** (still available):
   ```csharp
   using (var writer = new StreamWriter("graph.graphml"))
   {
       graph.SaveGraphML(writer);
   }
   // Open in yEd, Gephi, or other GraphML-compatible viewer
   ```

2. **Export to DOT format** (if available) and use Graphviz

3. **Implement custom rendering** using your preferred graphics library

### Future Work

See [RFC-004: API Modernization](./rfcs/RFC-004-api-modernization.md) for plans to restore drawing functionality using modern cross-platform graphics libraries:

- Option A: SkiaSharp (cross-platform, hardware-accelerated)
- Option B: ImageSharp (pure C#, cross-platform)
- Option C: System.Drawing.Common with multi-targeting (Windows-focused)

### Version Information

- **Since**: v1.0.0-alpha.1
- **Status**: Temporarily excluded
- **Target for restoration**: v2.0 or later (after RFC-004 implementation)

---

For questions or to contribute a fix, see [CONTRIBUTING.md](../CONTRIBUTING.md) or open an issue in the repository.
