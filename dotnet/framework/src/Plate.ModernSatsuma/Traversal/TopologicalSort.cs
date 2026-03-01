using System;
using System.Collections.Generic;
using System.Linq;

namespace Plate.ModernSatsuma;

/// <summary>
/// Computes a topological ordering of a directed acyclic graph using Kahn's algorithm.
/// The algorithm is iterative (BFS-based), naturally produces layer groupings for
/// parallel execution, and detects cycles.
/// </summary>
/// <remarks>
/// <para>
/// Time complexity: O(V + E) where V is the number of nodes and E is the number of arcs.
/// </para>
/// <para>
/// Nodes within each layer are sorted by <see cref="Node.Id"/> for deterministic output.
/// </para>
/// </remarks>
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
    /// Layer 1 = nodes whose predecessors are all in layer 0, etc.
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
    public IReadOnlyCollection<Node> CyclicNodes { get; }

    /// <summary>
    /// Computes the topological sort of the given directed graph.
    /// </summary>
    /// <param name="graph">The directed graph to sort.</param>
    /// <param name="arcFilter">
    /// Filter for which arcs to consider when determining successors.
    /// Default is <see cref="ArcFilter.Forward"/> (respects arc direction).
    /// </param>
    /// <exception cref="ArgumentNullException"><paramref name="graph"/> is null.</exception>
    public TopologicalSort(IGraph graph, ArcFilter arcFilter = ArcFilter.Forward)
    {
        Graph = graph ?? throw new ArgumentNullException(nameof(graph));

        var order = new List<Node>();
        var layers = new List<IReadOnlyList<Node>>();

        // 1. Compute in-degree for each node
        var inDegree = new Dictionary<long, int>();
        foreach (var node in graph.Nodes())
            inDegree[node.Id] = 0;

        foreach (var node in graph.Nodes())
        {
            foreach (var arc in graph.Arcs(node, arcFilter))
            {
                var target = graph.Other(arc, node);
                if (target != node && inDegree.ContainsKey(target.Id))
                    inDegree[target.Id]++;
            }
        }

        // 2. Seed with zero-in-degree nodes (roots)
        var currentLayer = new List<Node>();
        foreach (var node in graph.Nodes())
        {
            if (inDegree[node.Id] == 0)
                currentLayer.Add(node);
        }

        // 3. Process layers (BFS waves)
        while (currentLayer.Count > 0)
        {
            currentLayer.Sort((a, b) => a.Id.CompareTo(b.Id));
            layers.Add(currentLayer);
            order.AddRange(currentLayer);

            var nextLayer = new List<Node>();
            foreach (var node in currentLayer)
            {
                foreach (var arc in graph.Arcs(node, arcFilter))
                {
                    var target = graph.Other(arc, node);
                    if (target != node && inDegree.ContainsKey(target.Id))
                    {
                        inDegree[target.Id]--;
                        if (inDegree[target.Id] == 0)
                            nextLayer.Add(target);
                    }
                }
            }

            currentLayer = nextLayer;
        }

        // 4. Cycle detection: remaining nodes with in-degree > 0
        var cyclic = new HashSet<Node>();
        foreach (var node in graph.Nodes())
        {
            if (inDegree[node.Id] > 0)
                cyclic.Add(node);
        }

        Order = order;
        Layers = layers;
        IsAcyclic = cyclic.Count == 0;
        CyclicNodes = cyclic;
    }
}
