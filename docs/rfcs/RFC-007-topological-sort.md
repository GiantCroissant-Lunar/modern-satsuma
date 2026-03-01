# RFC-007: Topological Sort Algorithm

**Status**: 🔴 Proposed
**Priority**: P1 - High
**Created**: 2026-03-01
**Authors**: Claude (AI Agent)
**Depends On**: None

---

## Summary

Add a `TopologicalSort` algorithm to `Plate.ModernSatsuma` that produces a linear ordering
of nodes in a directed acyclic graph (DAG) such that for every arc `u → v`, node `u`
appears before `v` in the ordering. Include cycle detection and optional layer grouping
for parallel execution.

---

## Problem Statement

### Missing Algorithm

ModernSatsuma provides `Dfs` (depth-first search) and `Bfs` (breadth-first search) as
traversal primitives, plus `Connectivity` for connected components. However, there is no
dedicated topological sort algorithm despite this being one of the most fundamental
directed graph operations.

### Current Workarounds

Users can implement topological sort by subclassing `Dfs` and collecting nodes in
`NodeExit` (reverse post-order). This works but:

1. Requires understanding `Dfs` internals and subclassing an abstract class.
2. Does not detect cycles — `BackArc` is called but the user must wire up the detection.
3. Does not provide layer grouping (Kahn's algorithm naturally produces layers; DFS does
   not).
4. Every consumer reimplements the same boilerplate.

### Motivating Use Case

`Plate.ModernSatsuma` is being adopted as the graph backbone for task DAG scheduling in
the giant-isopod agent swarm framework. The scheduler needs:

1. **Linear ordering** — dispatch tasks in dependency order.
2. **Cycle detection** — reject graphs with circular dependencies at submission time.
3. **Layer grouping** — identify independent tasks that can execute in parallel (same
   topological layer = no dependencies between them).
4. **Critical path** — already available via `Dijkstra`, but requires topo sort as a
   prerequisite for some formulations.

---

## Proposed Solution

### Phase 1: Core TopologicalSort (Kahn's Algorithm)

Add `TopologicalSort` class in `Traversal/TopologicalSort.cs` implementing Kahn's
algorithm (BFS-based, iterative, naturally produces layers).

**Why Kahn's over DFS post-order:**

| Concern | Kahn's (BFS) | DFS post-order |
|---------|-------------|----------------|
| Cycle detection | Built-in (remaining nodes = cycle) | Requires separate `BackArc` check |
| Layer grouping | Natural (each BFS wave = one layer) | Requires separate pass |
| Determinism | Stable (process in node-id order within each layer) | Stack-dependent |
| Implementation | Iterative (no recursion, no stack overflow on deep graphs) | Recursive (may overflow) |
| Performance | O(V + E) | O(V + E) |

### API Design

```csharp
namespace Plate.ModernSatsuma;

/// <summary>
/// Computes a topological ordering of a directed acyclic graph.
/// Uses Kahn's algorithm (BFS-based) for iterative, layer-aware sorting.
/// </summary>
public sealed class TopologicalSort
{
    /// <summary>The input graph.</summary>
    public IGraph Graph { get; }

    /// <summary>
    /// The sorted nodes in topological order.
    /// Empty if the graph contains a cycle.
    /// </summary>
    public IReadOnlyList<Node> Order { get; }

    /// <summary>
    /// Nodes grouped by topological layer (depth from roots).
    /// Layer 0 = nodes with no incoming arcs.
    /// Layer 1 = nodes whose predecessors are all in layer 0. Etc.
    /// Empty if the graph contains a cycle.
    /// </summary>
    public IReadOnlyList<IReadOnlyList<Node>> Layers { get; }

    /// <summary>
    /// True if the graph is a valid DAG (no cycles).
    /// False if cycles were detected.
    /// </summary>
    public bool IsAcyclic { get; }

    /// <summary>
    /// If the graph contains a cycle, these are the nodes involved in cycles
    /// (nodes remaining after Kahn's algorithm exhausts all zero-in-degree nodes).
    /// Empty if the graph is acyclic.
    /// </summary>
    public IReadOnlySet<Node> CyclicNodes { get; }

    /// <summary>
    /// Computes the topological sort of the given directed graph.
    /// </summary>
    /// <param name="graph">The directed graph to sort.</param>
    /// <param name="arcFilter">
    /// Filter for which arcs to consider. Default is <see cref="ArcFilter.Forward"/>
    /// (respects arc direction). Use <see cref="ArcFilter.All"/> to treat as undirected
    /// (though topological sort is only meaningful for directed graphs).
    /// </param>
    public TopologicalSort(IGraph graph, ArcFilter arcFilter = ArcFilter.Forward)
    {
        // Implementation
    }
}
```

### Algorithm Implementation

```csharp
// Kahn's algorithm (sketch)
public TopologicalSort(IGraph graph, ArcFilter arcFilter = ArcFilter.Forward)
{
    Graph = graph;
    var order = new List<Node>();
    var layers = new List<List<Node>>();

    // 1. Compute in-degree for each node
    var inDegree = new Dictionary<long, int>();
    foreach (var node in graph.Nodes())
        inDegree[node.Id] = 0;

    foreach (var arc in graph.Arcs(arcFilter))
    {
        var target = graph.V(arc);  // head of the arc
        inDegree[target.Id]++;
    }

    // 2. Seed queue with zero-in-degree nodes (roots)
    var currentLayer = new List<Node>();
    foreach (var node in graph.Nodes())
    {
        if (inDegree[node.Id] == 0)
            currentLayer.Add(node);
    }

    // 3. Process layers (BFS waves)
    while (currentLayer.Count > 0)
    {
        currentLayer.Sort((a, b) => a.Id.CompareTo(b.Id));  // deterministic order
        layers.Add(currentLayer);
        order.AddRange(currentLayer);

        var nextLayer = new List<Node>();
        foreach (var node in currentLayer)
        {
            foreach (var arc in graph.Arcs(node, ArcFilter.Forward))
            {
                var target = graph.Other(arc, node);
                inDegree[target.Id]--;
                if (inDegree[target.Id] == 0)
                    nextLayer.Add(target);
            }
        }
        currentLayer = nextLayer;
    }

    // 4. Cycle detection: any remaining nodes with in-degree > 0
    var cyclic = new HashSet<Node>();
    foreach (var node in graph.Nodes())
    {
        if (inDegree[node.Id] > 0)
            cyclic.Add(node);
    }

    Order = order;
    Layers = layers.Select(l => (IReadOnlyList<Node>)l).ToList();
    IsAcyclic = cyclic.Count == 0;
    CyclicNodes = cyclic;
}
```

### Phase 2: Builder Extension

Add a fluent builder in `Extensions/Builders.cs`:

```csharp
public sealed class TopologicalSortBuilder
{
    private IGraph _graph;
    private ArcFilter _arcFilter = ArcFilter.Forward;

    public TopologicalSortBuilder(IGraph graph) => _graph = graph;

    public TopologicalSortBuilder WithArcFilter(ArcFilter filter)
    {
        _arcFilter = filter;
        return this;
    }

    public TopologicalSort Build() => new TopologicalSort(_graph, _arcFilter);
}

// Extension method on IGraph
public static TopologicalSortBuilder TopologicalSort(this IGraph graph)
    => new TopologicalSortBuilder(graph);
```

Usage:
```csharp
var result = graph.TopologicalSort().Build();
if (result.IsAcyclic)
{
    foreach (var layer in result.Layers)
    {
        // All nodes in this layer can execute in parallel
        foreach (var node in layer)
            Console.WriteLine($"  {node.Id}");
    }
}
```

---

## Implementation Plan

1. Create `Traversal/TopologicalSort.cs` with Kahn's algorithm.
2. Add `TopologicalSortBuilder` to `Extensions/Builders.cs`.
3. Write unit tests in `Plate.ModernSatsuma.Tests/Traversal/TopologicalSortTests.cs`.
4. Update `pages.dox` with algorithm documentation.

---

## Success Criteria

- [ ] `TopologicalSort` produces correct linear ordering for DAGs
- [ ] Cycle detection correctly identifies cyclic graphs and reports `CyclicNodes`
- [ ] Layer grouping correctly groups independent nodes
- [ ] Deterministic output (sorted by `Node.Id` within each layer)
- [ ] Works with all `IGraph` implementations (CustomGraph, Subgraph, ReverseGraph, etc.)
- [ ] O(V + E) time complexity
- [ ] Builder extension provides fluent API consistent with `DijkstraBuilder`, `BfsBuilder`
- [ ] All existing tests continue to pass

---

## Testing Strategy

### Unit Tests

```csharp
// 1. Linear chain: A → B → C → D
//    Expected order: [A, B, C, D], 4 layers of 1 node each

// 2. Diamond: A → B, A → C, B → D, C → D
//    Expected order: [A, B, C, D] (B/C interchangeable), 3 layers: [A], [B,C], [D]

// 3. Parallel chains: A → B, C → D (no cross-dependencies)
//    Expected: 2 layers: [A,C], [B,D]

// 4. Single node: A
//    Expected: [A], 1 layer

// 5. Empty graph
//    Expected: [], 0 layers, IsAcyclic = true

// 6. Cycle: A → B → C → A
//    Expected: Order empty, IsAcyclic = false, CyclicNodes = {A, B, C}

// 7. Partial cycle: A → B → C → B, A → D
//    Expected: Order = [A, D], IsAcyclic = false, CyclicNodes = {B, C}

// 8. Large fan-out: A → B1, A → B2, ..., A → Bn
//    Expected: 2 layers: [A], [B1..Bn]

// 9. Deep chain: 1000 nodes in sequence
//    Expected: 1000 layers, no stack overflow (iterative algorithm)
```

---

## Alternatives Considered

### DFS Post-Order

Classic approach: subclass `Dfs`, collect nodes in `NodeExit`, reverse the list.

- **Pro**: Reuses existing `Dfs` infrastructure.
- **Con**: Recursive — risks stack overflow on deep graphs. No natural layer grouping.
  Cycle detection requires additional `BackArc` tracking. Users already have this option
  via `Dfs` subclassing; a dedicated class should offer more.

**Decision**: Kahn's algorithm is strictly better for this library's needs.

### External Library

Use an existing .NET topological sort package.

- **Con**: ModernSatsuma's value proposition is zero external dependencies with a
  consistent interface-based API. Adding an external dependency contradicts this.

**Decision**: Implement natively.

---

## Related RFCs

- RFC-001: Critical Build Fixes (prerequisite — library must build)
- RFC-004: API Surface Modernization (builder pattern, async extensions)
- Giant-isopod ADR-002: Task Graph (DAG) via ModernSatsuma (primary consumer)

---

## Timeline

- **Phase 1** (core algorithm + tests): ~2 hours
- **Phase 2** (builder extension): ~30 minutes

---

## Approval

- [ ] Library maintainer
- [ ] Downstream consumer (giant-isopod)

---

## Implementation Checklist

- [ ] Create `Traversal/TopologicalSort.cs`
- [ ] Implement Kahn's algorithm with layer grouping
- [ ] Implement cycle detection with `CyclicNodes` reporting
- [ ] Add `TopologicalSortBuilder` to `Extensions/Builders.cs`
- [ ] Add `TopologicalSort()` extension method on `IGraph`
- [ ] Write unit tests (9+ test cases)
- [ ] Update `pages.dox` documentation
- [ ] Build with `dotnet tool run unify-build -- Compile`
- [ ] Run tests
- [ ] Pack and sync to local NuGet feed
