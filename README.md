# ModernSatsuma

![.NET Standard](https://img.shields.io/badge/.NET%20Standard-2.0-blue)
![C#](https://img.shields.io/badge/C%23-latest-blue)
![License](https://img.shields.io/badge/license-zlib%2Flibpng-blue)
![Status](https://img.shields.io/badge/status-development-yellow)
![NuGet](https://img.shields.io/badge/NuGet-ready-green)

A modernized version of the Satsuma Graph Library for .NET Standard 2.0+.

## Overview

ModernSatsuma is a comprehensive graph library providing efficient implementations of graph algorithms, data structures, and utilities for .NET applications.

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
├── dotnet/framework/
│   ├── src/ModernSatsuma/    # Main library
│   ├── tests/                      # Unit tests
│   └── ModernSatsuma.sln
├── build/                          # Build artifacts
├── docs/                           # Documentation
└── scripts/                        # Build/utility scripts
```

### Source Code Organization

The library source code is organized into logical folders while keeping all types in the flat `ModernSatsuma` namespace:

```
src/ModernSatsuma/
├── Core/           # Graph, Path, Node, Arc, PriorityQueue, DisjointSet
├── Adaptors/       # Subgraph, Supergraph, ReverseGraph, UndirectedGraph, etc.
├── Generators/     # CompleteGraph, CompleteBipartiteGraph
├── Traversal/      # Bfs, Dfs, Connectivity, SpanningForest
├── Algorithms/
│   ├── ShortestPaths/  # Dijkstra, A*, BellmanFord, BidirectionalDijkstra
│   ├── Flows/          # Preflow, NetworkSimplex
│   ├── Matching/       # Matching, BipartiteMaximumMatching
│   ├── LinearProgramming/  # LP, OptimalSubgraph, OptimalVertexSet
│   └── Tsp/            # Traveling salesman algorithms
├── IO/             # GraphML, Lemon format support
├── Layout/         # Graph layout algorithms
└── Extensions/     # Modern C# extensions (async, Span<T>, builders)
```

See [docs/STRUCTURE.md](./docs/STRUCTURE.md) for detailed documentation.

## Getting Started

### Build

#### Using Nuke Build System (Recommended)
```bash
cd build/nuke
.\build.cmd Pack    # Build and create NuGet package
.\build.cmd --help  # Show available targets
```

#### Using .NET CLI Directly
```bash
cd dotnet/framework
dotnet build
```

### Test

```bash
cd dotnet/framework
dotnet test
```

### NuGet Package Creation

The project uses [Nuke](https://nuke.build/) with GitVersion for automated package creation:
- **Packages Output**: `build/_artifacts/{version}/`
- **Semantic Versioning**: Automatic version generation based on Git history
- **Build Targets**: Clean, Restore, Compile, Pack, Publish

See [Nuke Build Documentation](./docs/NUKE_BUILD_SETUP_COMPLETE.md) for complete setup details.

### Using the Library

```csharp
using ModernSatsuma;

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

See the [docs](./docs/) folder for comprehensive documentation:

### 📋 Quick Links
- **[Documentation Index](./docs/README.md)** - Complete documentation overview
- **[Project Status](./docs/status/)** - Current status and completion reports
- **[Implementation Guides](./docs/guides/)** - Step-by-step guides and plans
- **[Performance Guide](./docs/PERFORMANCE_GUIDE.md)** - API optimization guide
- **[Nuke Build System](./docs/NUKE_BUILD_SETUP_COMPLETE.md)** - Complete build system setup and GitVersion integration
- **[Documentation Organization Report](./docs/DOCUMENTATION_ORGANIZATION_COMPLETE.md)** - Complete documentation restructuring summary

### 🚀 Getting Started
- **New Users:** Start with [Project Status](./docs/status/README.md)
- **Developers:** See [Implementation Guides](./docs/guides/README.md)
- **Performance:** Read [Performance Guide](./docs/PERFORMANCE_GUIDE.md)

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
