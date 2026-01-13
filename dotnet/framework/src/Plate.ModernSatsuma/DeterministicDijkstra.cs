using System;
using System.Collections.Generic;
using System.Linq;

namespace Plate.ModernSatsuma;

/// <summary>
/// Dijkstra's algorithm with deterministic tie-breaking by node ID.
/// </summary>
/// <remarks>
/// When multiple nodes have the same cost, this implementation deterministically
/// selects the node with the smallest ID, ensuring reproducible results regardless
/// of iteration order or hash distribution.
/// </remarks>
public sealed class DeterministicDijkstra
{
    /// <summary>
    /// The input graph.
    /// </summary>
    public IGraph Graph { get; }

    /// <summary>
    /// The arc cost function.
    /// <c>double.PositiveInfinity</c> means that an arc is impassable.
    /// Cost must be non-negative.
    /// </summary>
    public Func<Arc, double> Cost { get; }

    /// <summary>
    /// The path cost calculation mode.
    /// </summary>
    public DijkstraMode Mode { get; }

    /// <summary>
    /// The lowest possible cost value.
    /// </summary>
    public double NullCost { get; }

    private readonly Dictionary<Node, double> _distance;
    private readonly Dictionary<Node, Arc> _parentArc;
    private readonly SortedSet<(double Cost, long NodeId, Node Node)> _priorityQueue;
    private readonly Dictionary<Node, double> _currentPriority;

    /// <summary>
    /// Custom comparer for priority queue entries.
    /// Orders by cost first, then by node ID for deterministic tie-breaking.
    /// </summary>
    private sealed class PriorityComparer : IComparer<(double Cost, long NodeId, Node Node)>
    {
        public static readonly PriorityComparer Instance = new();

        public int Compare((double Cost, long NodeId, Node Node) x, (double Cost, long NodeId, Node Node) y)
        {
            int costCmp = x.Cost.CompareTo(y.Cost);
            if (costCmp != 0) return costCmp;
            return x.NodeId.CompareTo(y.NodeId);
        }
    }

    /// <summary>
    /// Creates a new deterministic Dijkstra instance.
    /// </summary>
    /// <param name="graph">The graph to search.</param>
    /// <param name="cost">The arc cost function. Must be non-negative for Sum mode.</param>
    /// <param name="mode">The path cost calculation mode (default: Sum).</param>
    public DeterministicDijkstra(IGraph graph, Func<Arc, double> cost, DijkstraMode mode = DijkstraMode.Sum)
    {
        Graph = graph ?? throw new ArgumentNullException(nameof(graph));
        Cost = cost ?? throw new ArgumentNullException(nameof(cost));
        Mode = mode;
        NullCost = mode == DijkstraMode.Sum ? 0 : double.NegativeInfinity;

        _distance = new Dictionary<Node, double>();
        _parentArc = new Dictionary<Node, Arc>();
        _priorityQueue = new SortedSet<(double Cost, long NodeId, Node Node)>(PriorityComparer.Instance);
        _currentPriority = new Dictionary<Node, double>();
    }

    private void ValidateCost(double c)
    {
        if (Mode == DijkstraMode.Sum && c < 0)
            throw new InvalidOperationException($"Invalid cost: {c}. Cost must be non-negative in Sum mode.");
    }

    /// <summary>
    /// Adds a new source node with initial cost of 0.
    /// </summary>
    /// <exception cref="InvalidOperationException">The node has already been reached.</exception>
    public void AddSource(Node node)
    {
        AddSource(node, NullCost);
    }

    /// <summary>
    /// Adds a new source node with a specified initial cost.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The node has already been reached, or nodeCost is invalid.
    /// </exception>
    public void AddSource(Node node, double nodeCost)
    {
        if (Reached(node))
            throw new InvalidOperationException("Cannot add a reached node as a source.");
        ValidateCost(nodeCost);

        _parentArc[node] = Arc.Invalid;
        _priorityQueue.Add((nodeCost, node.Id, node));
        _currentPriority[node] = nodeCost;
    }

    /// <summary>
    /// Performs a step in the algorithm and fixes a node.
    /// </summary>
    /// <returns>The newly fixed node, or Node.Invalid if there was no reached but unfixed node.</returns>
    public Node Step()
    {
        if (_priorityQueue.Count == 0)
            return Node.Invalid;

        // Pop the minimum (cost, nodeId) tuple - deterministic due to nodeId tie-breaker
        var min = _priorityQueue.Min;
        _priorityQueue.Remove(min);
        _currentPriority.Remove(min.Node);

        var (minCost, _, minNode) = min;

        if (double.IsPositiveInfinity(minCost))
            return Node.Invalid;

        _distance[minNode] = minCost; // Fix the node

        // Update neighbors
        foreach (var arc in Graph.Arcs(minNode, ArcFilter.Forward))
        {
            Node other = Graph.Other(arc, minNode);
            if (Fixed(other))
                continue;

            double arcCost = Cost(arc);
            ValidateCost(arcCost);
            double newCost = Mode == DijkstraMode.Sum
                ? minCost + arcCost
                : Math.Max(minCost, arcCost);

            double oldCost = _currentPriority.TryGetValue(other, out var existing)
                ? existing
                : double.PositiveInfinity;

            if (newCost < oldCost)
            {
                // Remove old entry if exists
                if (_currentPriority.ContainsKey(other))
                {
                    _priorityQueue.Remove((oldCost, other.Id, other));
                }

                _priorityQueue.Add((newCost, other.Id, other));
                _currentPriority[other] = newCost;
                _parentArc[other] = arc;
            }
        }

        return minNode;
    }

    /// <summary>
    /// Runs the algorithm until all reachable nodes are fixed.
    /// </summary>
    public void Run()
    {
        while (Step() != Node.Invalid) { }
    }

    /// <summary>
    /// Runs the algorithm until a specific target node is fixed.
    /// </summary>
    /// <returns>The target if it was successfully fixed, or Node.Invalid if unreachable.</returns>
    public Node RunUntilFixed(Node target)
    {
        if (Fixed(target))
            return target;

        while (true)
        {
            Node fixedNode = Step();
            if (fixedNode == Node.Invalid || fixedNode == target)
                return fixedNode;
        }
    }

    /// <summary>
    /// Runs the algorithm until a node satisfying the given condition is fixed.
    /// </summary>
    /// <returns>A matching node if found, or Node.Invalid if all matching nodes are unreachable.</returns>
    public Node RunUntilFixed(Func<Node, bool> isTarget)
    {
        var already = FixedNodes.FirstOrDefault(isTarget);
        if (already != Node.Invalid)
            return already;

        while (true)
        {
            var fixedNode = Step();
            if (fixedNode == Node.Invalid || isTarget(fixedNode))
                return fixedNode;
        }
    }

    /// <summary>
    /// Returns whether a node has been reached (is in the search tree).
    /// </summary>
    public bool Reached(Node node) => _parentArc.ContainsKey(node);

    /// <summary>
    /// Returns all reached nodes.
    /// </summary>
    public IEnumerable<Node> ReachedNodes => _parentArc.Keys;

    /// <summary>
    /// Returns whether a node has been fixed (distance is finalized).
    /// </summary>
    public bool Fixed(Node node) => _distance.ContainsKey(node);

    /// <summary>
    /// Returns all fixed nodes.
    /// </summary>
    public IEnumerable<Node> FixedNodes => _distance.Keys;

    /// <summary>
    /// Gets the cost of the cheapest path from source to a given node.
    /// </summary>
    /// <returns>The distance, or PositiveInfinity if not yet reached.</returns>
    public double GetDistance(Node node)
    {
        return _distance.TryGetValue(node, out var result) ? result : double.PositiveInfinity;
    }

    /// <summary>
    /// Gets the arc connecting a node with its parent in the shortest path tree.
    /// </summary>
    /// <returns>The arc, or Arc.Invalid if node is source or not reached.</returns>
    public Arc GetParentArc(Node node)
    {
        return _parentArc.TryGetValue(node, out var result) ? result : Arc.Invalid;
    }

    /// <summary>
    /// Gets the cheapest path from source to a given node.
    /// </summary>
    /// <returns>The path, or null if not reached.</returns>
    public IPath? GetPath(Node node)
    {
        if (!Reached(node))
            return null;

        var result = new Path(Graph);
        result.Begin(node);

        while (true)
        {
            Arc arc = GetParentArc(node);
            if (arc == Arc.Invalid)
                break;
            result.AddFirst(arc);
            node = Graph.Other(arc, node);
        }

        return result;
    }

    /// <summary>
    /// Attempts to get the cheapest path from source to a given node.
    /// </summary>
    public bool TryGetPath(Node node, out IPath? path)
    {
        path = GetPath(node);
        return path != null;
    }

    /// <summary>
    /// Clears all state, allowing reuse with different sources.
    /// </summary>
    public void Clear()
    {
        _distance.Clear();
        _parentArc.Clear();
        _priorityQueue.Clear();
        _currentPriority.Clear();
    }
}
