using System;
using System.Collections.Generic;
using System.Linq;

namespace Plate.ModernSatsuma;

/// Represents a node in a UnionGraph, wrapping the original node with its source graph index.
public readonly struct UnionNode : IEquatable<UnionNode>
{
    /// The index of the source graph (0-based).
    public int GraphIndex { get; }
    /// The original node from the source graph.
    public Node OriginalNode { get; }

    public UnionNode(int graphIndex, Node originalNode)
    {
        GraphIndex = graphIndex;
        OriginalNode = originalNode;
    }

    public bool Equals(UnionNode other) => GraphIndex == other.GraphIndex && OriginalNode == other.OriginalNode;
    public override bool Equals(object? obj) => obj is UnionNode other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(GraphIndex, OriginalNode);
    public override string ToString() => $"G{GraphIndex}:{OriginalNode}";

    public static bool operator ==(UnionNode a, UnionNode b) => a.Equals(b);
    public static bool operator !=(UnionNode a, UnionNode b) => !a.Equals(b);
}

/// Represents an arc in a UnionGraph, wrapping the original arc with its source graph index.
public readonly struct UnionArc : IEquatable<UnionArc>
{
    /// The index of the source graph (0-based).
    public int GraphIndex { get; }
    /// The original arc from the source graph.
    public Arc OriginalArc { get; }

    public UnionArc(int graphIndex, Arc originalArc)
    {
        GraphIndex = graphIndex;
        OriginalArc = originalArc;
    }

    public bool Equals(UnionArc other) => GraphIndex == other.GraphIndex && OriginalArc == other.OriginalArc;
    public override bool Equals(object? obj) => obj is UnionArc other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(GraphIndex, OriginalArc);
    public override string ToString() => $"G{GraphIndex}:{OriginalArc}";

    public static bool operator ==(UnionArc a, UnionArc b) => a.Equals(b);
    public static bool operator !=(UnionArc a, UnionArc b) => !a.Equals(b);
}

/// Adaptor for treating multiple graphs as a single logical graph (union/overlay).
/// Node and Arc objects are wrapped with their source graph index to avoid ID collisions.
/// The underlying graphs must NOT be modified while using this adaptor.
///
/// This adaptor provides a read-only view over the combined graphs without copying data.
/// Memory usage: O(k) where k is the number of source graphs (nodes/arcs are not copied).
///
/// Example:
/// <code>
/// var g1 = new CustomGraph();
/// var a = g1.AddNode();
/// var b = g1.AddNode();
/// g1.AddArc(a, b, Directedness.Directed);
///
/// var g2 = new CustomGraph();
/// var c = g2.AddNode();
/// var d = g2.AddNode();
/// g2.AddArc(c, d, Directedness.Undirected);
///
/// var union = new UnionGraph(g1, g2);
/// Console.WriteLine($"Union has {union.NodeCount()} nodes and {union.ArcCount()} arcs");
/// // Output: Union has 4 nodes and 2 arcs
///
/// // Get wrapped node identities
/// var wrappedA = union.WrapNode(0, a); // (graph 0, node a)
/// var wrappedC = union.WrapNode(1, c); // (graph 1, node c)
/// </code>
/// <seealso cref="JoinGraph"/>
/// <seealso cref="Subgraph"/>
public class UnionGraph : IGraph
{
    private readonly IReadOnlyList<IGraph> graphs;

    // Bit layout for composite IDs:
    // Upper 16 bits: graph index (supports up to 65536 graphs)
    // Lower 48 bits: original ID
    private const int GraphIndexBits = 16;
    private const long OriginalIdMask = (1L << 48) - 1;
    private const int MaxGraphIndex = (1 << GraphIndexBits) - 1;

    /// Creates a UnionGraph from multiple source graphs.
    /// <param name="graphs">The source graphs to combine.</param>
    public UnionGraph(params IGraph[] graphs)
        : this((IReadOnlyList<IGraph>)graphs)
    {
    }

    /// Creates a UnionGraph from a collection of source graphs.
    /// <param name="graphs">The source graphs to combine.</param>
    public UnionGraph(IReadOnlyList<IGraph> graphs)
    {
        if (graphs.Count > MaxGraphIndex)
            throw new ArgumentException($"Too many graphs. Maximum supported is {MaxGraphIndex}.", nameof(graphs));

        this.graphs = graphs;
    }

    /// The number of source graphs.
    public int GraphCount => graphs.Count;

    /// Gets a source graph by index.
    public IGraph GetGraph(int index) => graphs[index];

    #region Node/Arc Wrapping

    /// Wraps a node from a source graph into a UnionGraph node.
    /// <param name="graphIndex">The index of the source graph (0-based).</param>
    /// <param name="node">The original node from the source graph.</param>
    /// <returns>A Node with a composite ID usable in this UnionGraph.</returns>
    public Node WrapNode(int graphIndex, Node node)
    {
        if (graphIndex < 0 || graphIndex >= graphs.Count)
            throw new ArgumentOutOfRangeException(nameof(graphIndex));
        return new Node(((long)graphIndex << 48) | (node.Id & OriginalIdMask));
    }

    /// Wraps an arc from a source graph into a UnionGraph arc.
    /// <param name="graphIndex">The index of the source graph (0-based).</param>
    /// <param name="arc">The original arc from the source graph.</param>
    /// <returns>An Arc with a composite ID usable in this UnionGraph.</returns>
    public Arc WrapArc(int graphIndex, Arc arc)
    {
        if (graphIndex < 0 || graphIndex >= graphs.Count)
            throw new ArgumentOutOfRangeException(nameof(graphIndex));
        return new Arc(((long)graphIndex << 48) | (arc.Id & OriginalIdMask));
    }

    /// Unwraps a UnionGraph node to get its source graph index and original node.
    /// <param name="node">A node from this UnionGraph.</param>
    /// <returns>A UnionNode containing the graph index and original node.</returns>
    public UnionNode UnwrapNode(Node node)
    {
        int graphIndex = (int)(node.Id >> 48);
        var originalNode = new Node(node.Id & OriginalIdMask);
        return new UnionNode(graphIndex, originalNode);
    }

    /// Unwraps a UnionGraph arc to get its source graph index and original arc.
    /// <param name="arc">An arc from this UnionGraph.</param>
    /// <returns>A UnionArc containing the graph index and original arc.</returns>
    public UnionArc UnwrapArc(Arc arc)
    {
        int graphIndex = (int)(arc.Id >> 48);
        var originalArc = new Arc(arc.Id & OriginalIdMask);
        return new UnionArc(graphIndex, originalArc);
    }

    #endregion

    #region IGraph Implementation

    public Node U(Arc arc)
    {
        var unwrapped = UnwrapArc(arc);
        var originalU = graphs[unwrapped.GraphIndex].U(unwrapped.OriginalArc);
        return WrapNode(unwrapped.GraphIndex, originalU);
    }

    public Node V(Arc arc)
    {
        var unwrapped = UnwrapArc(arc);
        var originalV = graphs[unwrapped.GraphIndex].V(unwrapped.OriginalArc);
        return WrapNode(unwrapped.GraphIndex, originalV);
    }

    public bool IsEdge(Arc arc)
    {
        var unwrapped = UnwrapArc(arc);
        return graphs[unwrapped.GraphIndex].IsEdge(unwrapped.OriginalArc);
    }

    public IEnumerable<Node> Nodes()
    {
        for (int i = 0; i < graphs.Count; i++)
        {
            foreach (var node in graphs[i].Nodes())
            {
                yield return WrapNode(i, node);
            }
        }
    }

    public IEnumerable<Arc> Arcs(ArcFilter filter = ArcFilter.All)
    {
        for (int i = 0; i < graphs.Count; i++)
        {
            foreach (var arc in graphs[i].Arcs(filter))
            {
                yield return WrapArc(i, arc);
            }
        }
    }

    public IEnumerable<Arc> Arcs(Node u, ArcFilter filter = ArcFilter.All)
    {
        var unwrapped = UnwrapNode(u);
        foreach (var arc in graphs[unwrapped.GraphIndex].Arcs(unwrapped.OriginalNode, filter))
        {
            yield return WrapArc(unwrapped.GraphIndex, arc);
        }
    }

    public IEnumerable<Arc> Arcs(Node u, Node v, ArcFilter filter = ArcFilter.All)
    {
        var unwrappedU = UnwrapNode(u);
        var unwrappedV = UnwrapNode(v);

        // Nodes must be from the same graph to have arcs between them in a basic union
        if (unwrappedU.GraphIndex != unwrappedV.GraphIndex)
            yield break;

        foreach (var arc in graphs[unwrappedU.GraphIndex].Arcs(unwrappedU.OriginalNode, unwrappedV.OriginalNode, filter))
        {
            yield return WrapArc(unwrappedU.GraphIndex, arc);
        }
    }

    public int NodeCount()
    {
        int count = 0;
        for (int i = 0; i < graphs.Count; i++)
            count += graphs[i].NodeCount();
        return count;
    }

    public int ArcCount(ArcFilter filter = ArcFilter.All)
    {
        int count = 0;
        for (int i = 0; i < graphs.Count; i++)
            count += graphs[i].ArcCount(filter);
        return count;
    }

    public int ArcCount(Node u, ArcFilter filter = ArcFilter.All)
    {
        var unwrapped = UnwrapNode(u);
        return graphs[unwrapped.GraphIndex].ArcCount(unwrapped.OriginalNode, filter);
    }

    public int ArcCount(Node u, Node v, ArcFilter filter = ArcFilter.All)
    {
        var unwrappedU = UnwrapNode(u);
        var unwrappedV = UnwrapNode(v);

        if (unwrappedU.GraphIndex != unwrappedV.GraphIndex)
            return 0;

        return graphs[unwrappedU.GraphIndex].ArcCount(unwrappedU.OriginalNode, unwrappedV.OriginalNode, filter);
    }

    public bool HasNode(Node node)
    {
        var unwrapped = UnwrapNode(node);
        if (unwrapped.GraphIndex < 0 || unwrapped.GraphIndex >= graphs.Count)
            return false;
        return graphs[unwrapped.GraphIndex].HasNode(unwrapped.OriginalNode);
    }

    public bool HasArc(Arc arc)
    {
        var unwrapped = UnwrapArc(arc);
        if (unwrapped.GraphIndex < 0 || unwrapped.GraphIndex >= graphs.Count)
            return false;
        return graphs[unwrapped.GraphIndex].HasArc(unwrapped.OriginalArc);
    }

    #endregion

    #region Materialization

    /// Materializes the union into a mutable CustomGraph.
    /// This copies all nodes and arcs into a new graph.
    /// <returns>A new CustomGraph containing all nodes and arcs from the union.</returns>
    public CustomGraph ToCustomGraph()
    {
        var result = new CustomGraph();
        var nodeMapping = new Dictionary<Node, Node>();

        // Copy all nodes
        foreach (var node in Nodes())
        {
            nodeMapping[node] = result.AddNode();
        }

        // Copy all arcs
        foreach (var arc in Arcs())
        {
            var u = nodeMapping[U(arc)];
            var v = nodeMapping[V(arc)];
            var directedness = IsEdge(arc) ? Directedness.Undirected : Directedness.Directed;
            result.AddArc(u, v, directedness);
        }

        return result;
    }

    #endregion
}
