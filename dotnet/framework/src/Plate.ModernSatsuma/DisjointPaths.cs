using System;
using System.Collections.Generic;
using System.Linq;

namespace Plate.ModernSatsuma;

/// <summary>
/// Provides helpers for computing edge-disjoint shortest paths.
/// </summary>
public static class DisjointPaths
{
    /// <summary>
    /// Finds up to <paramref name="k"/> edge-disjoint shortest paths between
    /// <paramref name="source"/> and <paramref name="target"/> assuming unit edge costs
    /// and using <see cref="DijkstraMode.Sum"/>.
    /// </summary>
    public static IReadOnlyList<IPath> FindEdgeDisjointShortestPaths(
        IGraph graph,
        Node source,
        Node target,
        int k)
    {
        return FindEdgeDisjointShortestPaths(graph, source, target, k, _ => 1.0, DijkstraMode.Sum);
    }

    /// <summary>
    /// Finds up to <paramref name="k"/> edge-disjoint shortest paths between
    /// <paramref name="source"/> and <paramref name="target"/> by repeatedly running
    /// Dijkstra and removing the arcs of previously found paths.
    /// </summary>
    public static IReadOnlyList<IPath> FindEdgeDisjointShortestPaths(
        IGraph graph,
        Node source,
        Node target,
        int k,
        Func<Arc, double> cost,
        DijkstraMode mode = DijkstraMode.Sum)
    {
        if (graph == null) throw new ArgumentNullException(nameof(graph));
        if (cost == null) throw new ArgumentNullException(nameof(cost));
        if (k <= 0) throw new ArgumentOutOfRangeException(nameof(k), "k must be positive.");

        var result = new List<IPath>();
        var bannedArcs = new HashSet<Arc>();

        for (int i = 0; i < k; i++)
        {
            var path = ComputeShortestPathWithBannedArcs(graph, source, target, cost, mode, bannedArcs);
            if (path == null)
            {
                break;
            }

            result.Add(path);

            foreach (var arc in path.Arcs())
            {
                bannedArcs.Add(arc);
            }
        }

        return result;
    }

    public static IReadOnlyList<IPath> FindTwoEdgeDisjointShortestPathsOptimal(
        IGraph graph,
        Node source,
        Node target)
    {
        return FindTwoEdgeDisjointShortestPathsOptimal(graph, source, target, _ => 1.0, DijkstraMode.Sum);
    }

    public static IReadOnlyList<IPath> FindTwoEdgeDisjointShortestPathsOptimal(
        IGraph graph,
        Node source,
        Node target,
        Func<Arc, double> cost,
        DijkstraMode mode = DijkstraMode.Sum)
    {
        if (graph == null) throw new ArgumentNullException(nameof(graph));
        if (cost == null) throw new ArgumentNullException(nameof(cost));

        if (mode != DijkstraMode.Sum)
        {
            return FindEdgeDisjointShortestPaths(graph, source, target, 2, cost, mode);
        }

        var optimal = FindTwoEdgeDisjointShortestPathsOptimalCore(graph, source, target, cost);
        if (optimal != null)
        {
            return optimal;
        }

        return FindEdgeDisjointShortestPaths(graph, source, target, 2, cost, mode);
    }

    private static IReadOnlyList<IPath>? FindTwoEdgeDisjointShortestPathsOptimalCore(
        IGraph graph,
        Node source,
        Node target,
        Func<Arc, double> cost)
    {
        Dijkstra dijkstra;
        try
        {
            dijkstra = new Dijkstra(graph, cost, DijkstraMode.Sum);
            dijkstra.AddSource(source);
            dijkstra.Run();
        }
        catch (InvalidOperationException)
        {
            // Negative cost or other invalid configuration for Dijkstra in Sum mode.
            return null;
        }

        var firstPath = dijkstra.GetPath(target);
        if (firstPath == null)
        {
            // No path at all.
            return Array.Empty<IPath>();
        }

        var firstPathArcs = firstPath.Arcs().ToList();
        if (firstPathArcs.Count == 0)
        {
            // Trivial path (source == target).
            return new IPath[] { firstPath };
        }

        // Only handle directed graphs in the optimal routine.
        if (firstPathArcs.Any(arc => graph.IsEdge(arc)))
        {
            return null;
        }

        // Record distances from source.
        var distances = new Dictionary<Node, double>();
        foreach (var node in graph.Nodes())
        {
            var d = dijkstra.GetDistance(node);
            if (!double.IsPositiveInfinity(d))
            {
                distances[node] = d;
            }
        }

        var onFirstPath = new HashSet<Arc>(firstPathArcs);
        var residual = new Dictionary<Node, List<ResidualEdge>>();

        // Build residual graph with reweighted edges.
        foreach (var arc in graph.Arcs(ArcFilter.All))
        {
            var u = graph.U(arc);
            var v = graph.V(arc);

            if (!distances.TryGetValue(u, out var du) || !distances.TryGetValue(v, out var dv))
            {
                continue; // ignore arcs incident to unreached nodes
            }

            if (onFirstPath.Contains(arc))
            {
                // Reverse edges on the first path with zero cost.
                AddResidualEdge(residual, v, u, arc, 0.0, isReversed: true);
            }
            else
            {
                var w = cost(arc);
                if (w < 0)
                {
                    // We only handle non-negative weights optimally.
                    return null;
                }

                var reweighted = w + du - dv;
                if (reweighted < 0)
                {
                    reweighted = 0;
                }

                AddResidualEdge(residual, u, v, arc, reweighted, isReversed: false);
            }
        }

        var residualPath = RunResidualDijkstra(residual, source, target);
        if (residualPath == null || residualPath.Count == 0)
        {
            // Only one path found.
            return new IPath[] { firstPath };
        }

        // Cancel edges used in opposite directions and build the union of remaining arcs.
        var cancelled = new HashSet<Arc>();
        var extraArcs = new List<Arc>();
        foreach (var edge in residualPath)
        {
            if (edge.IsReversed)
            {
                cancelled.Add(edge.Arc);
            }
            else
            {
                extraArcs.Add(edge.Arc);
            }
        }

        var unionArcs = new HashSet<Arc>();
        foreach (var arc in firstPathArcs)
        {
            if (!cancelled.Contains(arc))
            {
                unionArcs.Add(arc);
            }
        }

        foreach (var arc in extraArcs)
        {
            if (!cancelled.Contains(arc))
            {
                unionArcs.Add(arc);
            }
        }

        // Build directed adjacency from the union arcs.
        var adjacency = new Dictionary<Node, List<Arc>>();
        void AddAdj(Node u, Arc arc)
        {
            if (!adjacency.TryGetValue(u, out var list))
            {
                list = new List<Arc>();
                adjacency[u] = list;
            }
            list.Add(arc);
        }

        foreach (var arc in unionArcs)
        {
            var u = graph.U(arc);
            AddAdj(u, arc);
        }

        List<Arc>? FindPathArcs()
        {
            var queue = new Queue<Node>();
            var parentArc = new Dictionary<Node, Arc>();
            queue.Enqueue(source);
            parentArc[source] = Arc.Invalid;

            while (queue.Count > 0)
            {
                var u = queue.Dequeue();
                if (!adjacency.TryGetValue(u, out var edges)) continue;

                foreach (var arc in edges)
                {
                    var v = graph.V(arc);
                    if (parentArc.ContainsKey(v)) continue;
                    parentArc[v] = arc;
                    if (v == target)
                    {
                        var pathArcs = new List<Arc>();
                        var cur = target;
                        while (cur != source)
                        {
                            var a = parentArc[cur];
                            pathArcs.Add(a);
                            cur = graph.U(a);
                        }
                        pathArcs.Reverse();
                        return pathArcs;
                    }
                    queue.Enqueue(v);
                }
            }

            return null;
        }

        var firstOptimalArcs = FindPathArcs();
        if (firstOptimalArcs == null)
        {
            return null;
        }

        // Remove first path arcs from adjacency and search for the second path.
        foreach (var arc in firstOptimalArcs)
        {
            var u = graph.U(arc);
            if (adjacency.TryGetValue(u, out var edges))
            {
                edges.Remove(arc);
            }
        }

        var secondOptimalArcs = FindPathArcs();
        if (secondOptimalArcs == null || secondOptimalArcs.Count == 0)
        {
            // Only one path remains in the union; return that and the original first path.
            var single = BuildPathFromArcSequence(graph, source, firstOptimalArcs);
            return new IPath[] { single };
        }

        var firstOptimalPath = BuildPathFromArcSequence(graph, source, firstOptimalArcs);
        var secondOptimalPath = BuildPathFromArcSequence(graph, source, secondOptimalArcs);
        return new IPath[] { firstOptimalPath, secondOptimalPath };
    }

    public static IReadOnlyList<IPath> FindNodeDisjointShortestPaths(
        IGraph graph,
        Node source,
        Node target,
        int k)
    {
        return FindNodeDisjointShortestPaths(graph, source, target, k, _ => 1.0, DijkstraMode.Sum);
    }

    public static IReadOnlyList<IPath> FindNodeDisjointShortestPaths(
        IGraph graph,
        Node source,
        Node target,
        int k,
        Func<Arc, double> cost,
        DijkstraMode mode = DijkstraMode.Sum)
    {
        if (graph == null) throw new ArgumentNullException(nameof(graph));
        if (cost == null) throw new ArgumentNullException(nameof(cost));
        if (k <= 0) throw new ArgumentOutOfRangeException(nameof(k), "k must be positive.");

        var result = new List<IPath>();
        var bannedNodes = new HashSet<Node>();

        for (int i = 0; i < k; i++)
        {
            var path = ComputeShortestPathWithBannedNodes(graph, source, target, cost, mode, bannedNodes);
            if (path == null)
            {
                break;
            }

            result.Add(path);

            // Ban internal nodes of this path (exclude endpoints)
            var nodes = path.Nodes().ToList();
            for (int n = 1; n < nodes.Count - 1; n++)
            {
                bannedNodes.Add(nodes[n]);
            }
        }

        return result;
    }

    private static IPath? ComputeShortestPathWithBannedArcs(
        IGraph graph,
        Node source,
        Node target,
        Func<Arc, double> baseCost,
        DijkstraMode mode,
        HashSet<Arc> bannedArcs)
    {
        double Cost(Arc arc)
        {
            if (bannedArcs.Contains(arc))
            {
                return double.PositiveInfinity;
            }

            return baseCost(arc);
        }

        var dijkstra = new Dijkstra(graph, Cost, mode);
        dijkstra.AddSource(source);
        var fixedTarget = dijkstra.RunUntilFixed(target);
        if (fixedTarget == Node.Invalid)
        {
            return null;
        }

        return dijkstra.GetPath(target);
    }

    private static IPath? ComputeShortestPathWithBannedNodes(
        IGraph graph,
        Node source,
        Node target,
        Func<Arc, double> baseCost,
        DijkstraMode mode,
        HashSet<Node> bannedNodes)
    {
        double Cost(Arc arc)
        {
            var u = graph.U(arc);
            var v = graph.V(arc);
            if (bannedNodes.Contains(u) || bannedNodes.Contains(v))
            {
                return double.PositiveInfinity;
            }

            return baseCost(arc);
        }

        var dijkstra = new Dijkstra(graph, Cost, mode);
        dijkstra.AddSource(source);
        var fixedTarget = dijkstra.RunUntilFixed(target);
        if (fixedTarget == Node.Invalid)
        {
            return null;
        }

        return dijkstra.GetPath(target);
    }

    private static void AddResidualEdge(
        Dictionary<Node, List<ResidualEdge>> residual,
        Node from,
        Node to,
        Arc arc,
        double weight,
        bool isReversed)
    {
        if (!residual.TryGetValue(from, out var list))
        {
            list = new List<ResidualEdge>();
            residual[from] = list;
        }

        list.Add(new ResidualEdge(from, to, arc, weight, isReversed));
    }

    private static List<ResidualEdge>? RunResidualDijkstra(
        Dictionary<Node, List<ResidualEdge>> residual,
        Node source,
        Node target)
    {
        var dist = new Dictionary<Node, double>();
        var parent = new Dictionary<Node, ResidualEdge>();
        var queue = new PriorityQueue<Node, double>();

        dist[source] = 0.0;
        queue[source] = 0.0;

        while (queue.Count > 0)
        {
            var u = queue.Peek(out var d);
            queue.Pop();

            if (!dist.TryGetValue(u, out var current) || d > current)
            {
                continue;
            }

            if (u == target)
            {
                break;
            }

            if (!residual.TryGetValue(u, out var edges))
            {
                continue;
            }

            foreach (var edge in edges)
            {
                var v = edge.To;
                var nd = d + edge.Weight;
                if (!dist.TryGetValue(v, out var old) || nd < old)
                {
                    dist[v] = nd;
                    parent[v] = edge;
                    queue[v] = nd;
                }
            }
        }

        if (!dist.ContainsKey(target))
        {
            return null;
        }

        var path = new List<ResidualEdge>();
        var currentNode = target;
        while (!currentNode.Equals(source))
        {
            if (!parent.TryGetValue(currentNode, out var edge))
            {
                break;
            }
            path.Add(edge);
            currentNode = edge.From;
        }

        path.Reverse();
        return path;
    }

    private static Path BuildPathFromArcSequence(IGraph graph, Node source, List<Arc> arcs)
    {
        var path = new Path(graph);
        path.Begin(source);
        foreach (var arc in arcs)
        {
            path.AddLast(arc);
        }
        return path;
    }

    private readonly struct ResidualEdge
    {
        public Node From { get; }
        public Node To { get; }
        public Arc Arc { get; }
        public double Weight { get; }
        public bool IsReversed { get; }

        public ResidualEdge(Node from, Node to, Arc arc, double weight, bool isReversed)
        {
            From = from;
            To = to;
            Arc = arc;
            Weight = weight;
            IsReversed = isReversed;
        }
    }
}
