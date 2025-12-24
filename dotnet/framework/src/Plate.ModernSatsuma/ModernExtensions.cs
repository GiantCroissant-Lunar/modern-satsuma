using System;
using System.Collections.Generic;
using System.Linq;

namespace Plate.ModernSatsuma;

/// <summary>
/// Modern .NET API extensions for Satsuma graph library.
/// Provides contemporary patterns like IReadOnlyCollection, fluent APIs, and Try* methods.
/// </summary>
public static class ModernExtensions
{
    /// <summary>
    /// Gets the nodes as a read-only collection.
    /// </summary>
    /// <param name="graph">The graph.</param>
    /// <returns>A read-only collection of nodes.</returns>
    public static IReadOnlyCollection<Node> GetNodesAsReadOnly(this IGraph graph)
    {
        if (graph == null) throw new ArgumentNullException(nameof(graph));
        return graph.Nodes().ToList().AsReadOnly();
    }

    /// <summary>
    /// Gets the arcs as a read-only collection.
    /// </summary>
    /// <param name="graph">The graph.</param>
    /// <param name="filter">Optional arc filter.</param>
    /// <returns>A read-only collection of arcs.</returns>
    public static IReadOnlyCollection<Arc> GetArcsAsReadOnly(this IGraph graph, ArcFilter filter = ArcFilter.All)
    {
        if (graph == null) throw new ArgumentNullException(nameof(graph));
        return graph.Arcs(filter).ToList().AsReadOnly();
    }

    /// <summary>
    /// Attempts to get a node by checking if it exists in the graph.
    /// </summary>
    /// <param name="graph">The graph.</param>
    /// <param name="node">The node to check.</param>
    /// <param name="validNode">The node if it exists, Node.Invalid otherwise.</param>
    /// <returns>True if the node exists in the graph, false otherwise.</returns>
    public static bool TryGetNode(this IGraph graph, Node node, out Node validNode)
    {
        if (graph == null) throw new ArgumentNullException(nameof(graph));
        if (graph.HasNode(node))
        {
            validNode = node;
            return true;
        }
        validNode = Node.Invalid;
        return false;
    }

    /// <summary>
    /// Attempts to get an arc by checking if it exists in the graph.
    /// </summary>
    /// <param name="graph">The graph.</param>
    /// <param name="arc">The arc to check.</param>
    /// <param name="validArc">The arc if it exists, Arc.Invalid otherwise.</param>
    /// <returns>True if the arc exists in the graph, false otherwise.</returns>
    public static bool TryGetArc(this IGraph graph, Arc arc, out Arc validArc)
    {
        if (graph == null) throw new ArgumentNullException(nameof(graph));
        if (graph.HasArc(arc))
        {
            validArc = arc;
            return true;
        }
        validArc = Arc.Invalid;
        return false;
    }

    /// <summary>
    /// Gets the node count as an int (convenience method).
    /// </summary>
    /// <param name="graph">The graph.</param>
    /// <returns>The number of nodes.</returns>
    public static int CountNodes(this IGraph graph)
    {
        if (graph == null) throw new ArgumentNullException(nameof(graph));
        return graph.NodeCount();
    }

    /// <summary>
    /// Gets the arc count as an int (convenience method).
    /// </summary>
    /// <param name="graph">The graph.</param>
    /// <param name="filter">Optional arc filter.</param>
    /// <returns>The number of arcs matching the filter.</returns>
    public static int CountArcs(this IGraph graph, ArcFilter filter = ArcFilter.All)
    {
        if (graph == null) throw new ArgumentNullException(nameof(graph));
        return graph.ArcCount(filter);
    }

    /// <summary>
    /// Checks if a graph is empty (has no nodes).
    /// </summary>
    /// <param name="graph">The graph.</param>
    /// <returns>True if the graph has no nodes, false otherwise.</returns>
    public static bool IsEmpty(this IGraph graph)
    {
        if (graph == null) throw new ArgumentNullException(nameof(graph));
        return graph.NodeCount() == 0;
    }

    /// <summary>
    /// Attempts to get the level of a node in a BFS traversal.
    /// </summary>
    /// <param name="bfs">The BFS instance.</param>
    /// <param name="node">The node.</param>
    /// <param name="level">The level if the node has been reached, -1 otherwise.</param>
    /// <returns>True if the node has been reached, false otherwise.</returns>
    public static bool TryGetLevel(this Bfs bfs, Node node, out int level)
    {
        if (bfs == null) throw new ArgumentNullException(nameof(bfs));
        level = bfs.GetLevel(node);
        return level >= 0;
    }

    /// <summary>
    /// Attempts to get the distance of a node from the sources in Dijkstra's algorithm.
    /// </summary>
    /// <param name="dijkstra">The Dijkstra instance.</param>
    /// <param name="node">The node.</param>
    /// <param name="distance">The distance if the node has been reached, PositiveInfinity otherwise.</param>
    /// <returns>True if the node has been reached (distance is finite), false otherwise.</returns>
    public static bool TryGetDistance(this Dijkstra dijkstra, Node node, out double distance)
    {
        if (dijkstra == null) throw new ArgumentNullException(nameof(dijkstra));
        distance = dijkstra.GetDistance(node);
        return !double.IsPositiveInfinity(distance);
    }

    /// <summary>
    /// Attempts to get the distance of a node from the sources in Bellman-Ford algorithm.
    /// </summary>
    /// <param name="bellmanFord">The BellmanFord instance.</param>
    /// <param name="node">The node.</param>
    /// <param name="distance">The distance if the node has been reached, PositiveInfinity otherwise.</param>
    /// <returns>True if the node has been reached (distance is finite), false otherwise.</returns>
    /// <exception cref="InvalidOperationException">A negative cycle has been detected.</exception>
    public static bool TryGetDistance(this BellmanFord bellmanFord, Node node, out double distance)
    {
        if (bellmanFord == null) throw new ArgumentNullException(nameof(bellmanFord));
        distance = bellmanFord.GetDistance(node);
        return !double.IsPositiveInfinity(distance);
    }

    /// <summary>
    /// Attempts to get the parent arc of a node in a pathfinding algorithm.
    /// </summary>
    /// <param name="dijkstra">The Dijkstra instance.</param>
    /// <param name="node">The node.</param>
    /// <param name="parentArc">The parent arc if it exists, Arc.Invalid otherwise.</param>
    /// <returns>True if the node has a parent arc, false otherwise.</returns>
    public static bool TryGetParentArc(this Dijkstra dijkstra, Node node, out Arc parentArc)
    {
        if (dijkstra == null) throw new ArgumentNullException(nameof(dijkstra));
        parentArc = dijkstra.GetParentArc(node);
        return parentArc != Arc.Invalid;
    }

    /// <summary>
    /// Attempts to get the parent arc of a node in a BFS traversal.
    /// </summary>
    /// <param name="bfs">The BFS instance.</param>
    /// <param name="node">The node.</param>
    /// <param name="parentArc">The parent arc if it exists, Arc.Invalid otherwise.</param>
    /// <returns>True if the node has a parent arc, false otherwise.</returns>
    public static bool TryGetParentArc(this Bfs bfs, Node node, out Arc parentArc)
    {
        if (bfs == null) throw new ArgumentNullException(nameof(bfs));
        parentArc = bfs.GetParentArc(node);
        return parentArc != Arc.Invalid;
    }

    /// <summary>
    /// Attempts to get the parent arc of a node in Bellman-Ford algorithm.
    /// </summary>
    /// <param name="bellmanFord">The BellmanFord instance.</param>
    /// <param name="node">The node.</param>
    /// <param name="parentArc">The parent arc if it exists, Arc.Invalid otherwise.</param>
    /// <returns>True if the node has a parent arc, false otherwise.</returns>
    /// <exception cref="InvalidOperationException">A negative cycle has been detected.</exception>
    public static bool TryGetParentArc(this BellmanFord bellmanFord, Node node, out Arc parentArc)
    {
        if (bellmanFord == null) throw new ArgumentNullException(nameof(bellmanFord));
        parentArc = bellmanFord.GetParentArc(node);
        return parentArc != Arc.Invalid;
    }

    public static bool TryFindTwoEdgeDisjointShortestPathsOptimal(
        this IGraph graph,
        Node source,
        Node target,
        Func<Arc, double> cost,
        out IReadOnlyList<IPath>? paths,
        DijkstraMode mode = DijkstraMode.Sum)
    {
        if (graph == null) throw new ArgumentNullException(nameof(graph));
        if (cost == null) throw new ArgumentNullException(nameof(cost));

        var result = DisjointPaths.FindTwoEdgeDisjointShortestPathsOptimal(graph, source, target, cost, mode);
        if (result.Count == 0)
        {
            paths = null;
            return false;
        }

        paths = result;
        return true;
    }

    public static bool TryFindTwoEdgeDisjointShortestPathsOptimal(
        this IGraph graph,
        Node source,
        Node target,
        out IReadOnlyList<IPath>? paths)
    {
        if (graph == null) throw new ArgumentNullException(nameof(graph));

        var result = DisjointPaths.FindTwoEdgeDisjointShortestPathsOptimal(graph, source, target);
        if (result.Count == 0)
        {
            paths = null;
            return false;
        }

        paths = result;
        return true;
    }
}
