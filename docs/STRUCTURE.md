---
title: Project Structure
description: Organization of the Plate.ModernSatsuma codebase
---

# Project Structure

This document describes the organization of the Plate.ModernSatsuma source code.

## Overview

The codebase is organized into logical folders by functionality. All classes remain in the flat `Plate.ModernSatsuma` namespace for API compatibility - the folders are purely organizational.

## Directory Layout

```
dotnet/framework/src/Plate.ModernSatsuma/
├── Core/                    # Fundamental types and data structures
│   ├── Graph.cs             # Node, Arc, IGraph, IArcLookup, etc.
│   ├── Path.cs              # IPath interface and Path class
│   ├── PriorityQueue.cs     # Priority queue implementation
│   ├── DisjointSet.cs       # Union-find data structure
│   └── Utils.cs             # Utility interfaces (IClearable)
│
├── Adaptors/                # Graph wrapper/adaptor patterns
│   ├── Subgraph.cs          # View of a subset of a graph
│   ├── Supergraph.cs        # Extension of a graph
│   ├── ContractedGraph.cs   # Graph with contracted nodes
│   ├── ReverseGraph.cs      # Graph with reversed arcs
│   ├── RedirectedGraph.cs   # Graph with redirected arcs
│   ├── UndirectedGraph.cs   # Undirected view of directed graph
│   ├── UnionGraph.cs        # Union of multiple graphs
│   └── JoinGraph.cs         # Join of multiple graphs
│
├── Generators/              # Graph construction utilities
│   ├── CompleteGraph.cs     # Complete graph (K_n)
│   └── CompleteBipartiteGraph.cs  # Complete bipartite graph (K_{m,n})
│
├── Traversal/               # Graph traversal algorithms
│   ├── Bfs.cs               # Breadth-first search
│   ├── Dfs.cs               # Depth-first search
│   ├── Connectivity.cs      # Connected components
│   └── SpanningForest.cs    # Spanning forest construction
│
├── Algorithms/              # Graph algorithms organized by category
│   ├── ShortestPaths/       # Shortest path algorithms
│   │   ├── Dijkstra.cs      # Dijkstra's algorithm
│   │   ├── DijkstraLegacy.cs    # Backward compat shim (deprecated)
│   │   ├── DeterministicDijkstra.cs  # Deterministic variant
│   │   ├── BidirectionalDijkstra.cs  # Bidirectional search
│   │   ├── BellmanFord.cs   # Bellman-Ford algorithm
│   │   ├── AStar.cs         # A* search algorithm
│   │   └── KShortestPaths.cs    # K shortest paths
│   │
│   ├── Flows/               # Network flow algorithms
│   │   ├── Preflow.cs       # Push-relabel algorithm
│   │   └── NetworkSimplex.cs    # Network simplex method
│   │
│   ├── Matching/            # Matching algorithms
│   │   ├── Matching.cs      # General matching
│   │   ├── BipartiteMaximumMatching.cs  # Maximum bipartite matching
│   │   └── BipartiteMinimumCostMatching.cs  # Min-cost matching
│   │
│   ├── Disjointness/        # Disjoint paths
│   │   └── DisjointPaths.cs # Node/arc-disjoint paths
│   │
│   ├── Isomorphism/         # Graph isomorphism
│   │   └── Isomorphism.cs   # Isomorphism testing
│   │
│   ├── LinearProgramming/   # LP-based algorithms
│   │   ├── LP.cs            # Linear programming core
│   │   ├── LP.OptimalSubgraph.cs    # Optimal subgraph selection
│   │   └── LP.OptimalVertexSet.cs   # Optimal vertex set selection
│   │
│   └── Tsp/                 # Traveling salesman
│       └── Tsp.cs           # TSP approximation algorithms
│
├── IO/                      # Input/output functionality
│   ├── IO.cs                # Core I/O utilities
│   └── GraphML.cs           # GraphML format support
│
├── Layout/                  # Graph layout algorithms
│   ├── Layout.cs            # Layout interfaces
│   └── GraphLayout.cs       # Layout implementations
│
└── Extensions/              # Modern C# extension methods
    ├── ModernExtensions.cs  # General extensions
    ├── AsyncExtensions.cs   # Async/await support
    ├── SpanExtensions.cs    # Span<T> support
    └── Builders.cs          # Fluent builder patterns
```

## Separate Packages

Drawing/rendering functionality is provided by separate packages:

```
dotnet/framework/src/
├── Plate.ModernSatsuma.Abstractions/     # Drawing interfaces
│   ├── IGraphDrawer.cs
│   ├── IGraphicsContext.cs
│   └── INodeShape.cs
│
├── Plate.ModernSatsuma.Drawing.SkiaSharp/    # SkiaSharp renderer
│   ├── GraphDrawer.cs
│   ├── NodeShape.cs
│   ├── NodeStyle.cs
│   └── SkiaSharpAdapter.cs
│
└── Plate.ModernSatsuma.Drawing.SystemDrawing/ # System.Drawing renderer
    ├── GraphDrawer.cs
    ├── NodeShape.cs
    ├── NodeStyle.cs
    └── SystemDrawingAdapter.cs
```

This separation allows:
- **Platform flexibility**: Choose the renderer appropriate for your platform
- **Minimal dependencies**: Core library has no graphics dependencies
- **Package size**: Only include what you need

## Design Principles

### Flat Namespace

All public types remain in the `Plate.ModernSatsuma` namespace regardless of their folder location. This ensures:

- **API Stability**: Existing code continues to work without changes
- **Simple Imports**: One `using Plate.ModernSatsuma;` imports everything
- **Discoverability**: All types visible together in IDE autocomplete

### Folder Organization

Folders group related functionality:

- **Core**: Types used throughout the library
- **Adaptors**: Decorator pattern implementations for graphs
- **Generators**: Factory methods for common graph types
- **Traversal**: Basic graph exploration algorithms
- **Algorithms**: Advanced algorithms by category
- **IO**: Serialization and file format support
- **Layout**: Visualization support
- **Extensions**: Modern C# idioms

### Backward Compatibility

The `Dijsktra` class (typo) is preserved as `DijkstraLegacy.cs` with an `[Obsolete]` attribute pointing users to the correctly-spelled `Dijkstra` class.

## Adding New Files

When adding new functionality:

1. Place the file in the appropriate folder
2. Keep the namespace as `Plate.ModernSatsuma`
3. Add XML documentation comments
4. Add corresponding unit tests

Example:
```csharp
// File: Algorithms/ShortestPaths/FloydWarshall.cs
namespace Plate.ModernSatsuma;

/// <summary>
/// Floyd-Warshall all-pairs shortest path algorithm.
/// </summary>
public class FloydWarshall
{
    // Implementation
}
```
