# Plate.ModernSatsuma

A modernized version of the Satsuma Graph Library for .NET Standard 2.0+.

## Overview

Plate.ModernSatsuma is a comprehensive graph library providing efficient implementations of graph algorithms, data structures, and utilities for .NET applications.

## Features

- **Graph Data Structures**: Flexible graph representations (directed, undirected, mixed)
- **Path Finding**: Dijkstra, A*, Bellman-Ford, BFS, DFS
- **Network Flow**: Preflow, Network Simplex algorithms
- **Matching**: Maximum matching, bipartite matching, minimum cost matching
- **Connectivity**: Strongly connected components, bridges, cut vertices
- **Graph Transformations**: Subgraphs, supergraphs, contracted graphs, reversed graphs
- **I/O Support**: GraphML, Lemon graph format, simple graph format
- **Linear Programming**: LP solver framework for graph optimization
- **Advanced Features**: Graph isomorphism, TSP solvers, layout algorithms

## Project Structure

```
modern-satsuma/
├── dotnet/
│   └── framework/
│       ├── src/
│       │   └── Plate.ModernSatsuma/     # Main library
│       ├── tests/
│       │   └── Plate.ModernSatsuma.Tests/  # Unit tests
│       └── Plate.ModernSatsuma.sln
├── build/                                # Build artifacts
├── docs/                                 # Documentation
└── scripts/                              # Build/utility scripts
```

## Getting Started

### Build

```bash
cd dotnet/framework
dotnet build
```

### Test

```bash
cd dotnet/framework
dotnet test
```

### Using the Library

```csharp
using Plate.ModernSatsuma;

// Create a custom graph
var graph = new CustomGraph();
var node1 = graph.AddNode();
var node2 = graph.AddNode();
var arc = graph.AddArc(node1, node2, Directedness.Directed);

// Find shortest path
var dijkstra = new Dijkstra(graph, arc => 1.0, DijkstraMode.Sum);
dijkstra.AddSource(node1);
dijkstra.RunUntilFixed();

if (dijkstra.Reached(node2))
{
    var distance = dijkstra.GetDistance(node2);
    var path = dijkstra.GetPath(node2);
}
```

## Documentation

See the [docs](./docs/) folder for detailed documentation:
- [Modernization Analysis](./docs/MODERNIZATION_ANALYSIS.md)
- [Fix Action Plan](./docs/FIX_ACTION_PLAN.md)

## Status

**Current Status**: Under active development

### Known Limitations

- Drawing/rendering functionality excluded from .NET Standard 2.0 build (System.Drawing dependency)
- Use external visualization tools or export to GraphML for visual representation

## License

Based on the original Satsuma Graph Library by Balázs Szalkai.

Original license: zlib/libpng License

Modernization and modifications: [Your License Here]

## Credits

- Original Satsuma Graph Library: Balázs Szalkai
- Modernization: Plate Framework Team
