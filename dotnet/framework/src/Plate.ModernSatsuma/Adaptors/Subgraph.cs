

using System;
using System.Collections.Generic;
using System.Linq;

namespace Plate.ModernSatsuma;

/// Adaptor for hiding/showing nodes/arcs of an underlying graph.
/// Node and Arc objects are interchangeable between the adaptor and the original graph.
///
/// The underlying graph can be modified while using this adaptor,
/// as long as no nodes/arcs are deleted; and newly added nodes/arcs are explicitly enabled/disabled,
/// since enabledness of newly added nodes/arcs is undefined.
///
/// By default, all nodes and arcs are enabled.
/// \sa Supergraph
public sealed class Subgraph : IGraph
{
    private IGraph graph;

    private bool defaultNodeEnabled;
    private HashSet<Node> nodeExceptions = new();
    private bool defaultArcEnabled;
    private HashSet<Arc> arcExceptions = new();

    /// The underlying graph.
    public IGraph Graph => graph;

    public Subgraph(IGraph graph)
    {
        this.graph = graph;

        EnableAllNodes(true);
        EnableAllArcs(true);
    }

    /// Creates a subgraph induced by the given nodes.
    /// The induced subgraph contains only the specified nodes and all arcs between them.
    /// <param name="graph">The underlying graph.</param>
    /// <param name="nodes">The nodes to include in the subgraph.</param>
    /// <returns>A Subgraph containing only the specified nodes and arcs between them.</returns>
    public static Subgraph InducedByNodes(IGraph graph, IEnumerable<Node> nodes)
    {
        var subgraph = new Subgraph(graph);
        subgraph.EnableAllNodes(false);
        foreach (var node in nodes)
        {
            subgraph.Enable(node, true);
        }
        // Arcs are automatically filtered by Subgraph to only include those where both endpoints are enabled
        return subgraph;
    }

    /// Creates a subgraph induced by the given arcs.
    /// The induced subgraph contains only the specified arcs and their endpoint nodes.
    /// <param name="graph">The underlying graph.</param>
    /// <param name="arcs">The arcs to include in the subgraph.</param>
    /// <returns>A Subgraph containing only the specified arcs and their endpoint nodes.</returns>
    public static Subgraph InducedByArcs(IGraph graph, IEnumerable<Arc> arcs)
    {
        var subgraph = new Subgraph(graph);
        subgraph.EnableAllNodes(false);
        subgraph.EnableAllArcs(false);

        foreach (var arc in arcs)
        {
            subgraph.Enable(arc, true);
            subgraph.Enable(graph.U(arc), true);
            subgraph.Enable(graph.V(arc), true);
        }
        return subgraph;
    }

    /// Creates a subgraph by applying filter predicates.
    /// <param name="graph">The underlying graph.</param>
    /// <param name="nodeFilter">A predicate that returns true for nodes to keep. If null, all nodes are kept.</param>
    /// <param name="arcFilter">A predicate that returns true for arcs to keep. If null, all arcs are kept (subject to node filtering).</param>
    /// <returns>A Subgraph containing only nodes and arcs that pass the filters.</returns>
    public static Subgraph Filter(IGraph graph, Func<Node, bool>? nodeFilter = null, Func<Arc, bool>? arcFilter = null)
    {
        var subgraph = new Subgraph(graph);

        if (nodeFilter != null)
        {
            subgraph.EnableAllNodes(false);
            foreach (var node in graph.Nodes())
            {
                if (nodeFilter(node))
                    subgraph.Enable(node, true);
            }
        }

        if (arcFilter != null)
        {
            subgraph.EnableAllArcs(false);
            foreach (var arc in graph.Arcs())
            {
                if (arcFilter(arc))
                    subgraph.Enable(arc, true);
            }
        }

        return subgraph;
    }

    /// Enables/disables all nodes at once.
    /// \param enabled \c true if all nodes should be enabled, \c false if all nodes should be disabled.
    public void EnableAllNodes(bool enabled)
    {
        defaultNodeEnabled = enabled;
        nodeExceptions.Clear();
    }

    /// Enables/disables all arcs at once.
    /// \param enabled \c true if all arcs should be enabled, \c false if all arcs should be disabled.
    public void EnableAllArcs(bool enabled)
    {
        defaultArcEnabled = enabled;
        arcExceptions.Clear();
    }

    /// Enables/disables a single node.
    /// \param enabled \c true if the node should be enabled, \c false if the node should be disabled.
    public void Enable(Node node, bool enabled)
    {
        bool exception = (defaultNodeEnabled != enabled);
        if (exception)
            nodeExceptions.Add(node);
        else nodeExceptions.Remove(node);
    }

    /// Enables/disables a single arc.
    /// \param enabled \c true if the arc should be enabled, \c false if the arc should be disabled.
    public void Enable(Arc arc, bool enabled)
    {
        bool exception = (defaultArcEnabled != enabled);
        if (exception)
            arcExceptions.Add(arc);
        else arcExceptions.Remove(arc);
    }

    /// Queries the enabledness of a node.
    public bool IsEnabled(Node node)
    {
        return defaultNodeEnabled ^ nodeExceptions.Contains(node);
    }

    /// Queries the enabledness of an arc.
    public bool IsEnabled(Arc arc)
    {
        return defaultArcEnabled ^ arcExceptions.Contains(arc);
    }

    public Node U(Arc arc)
    {
        return graph.U(arc);
    }

    public Node V(Arc arc)
    {
        return graph.V(arc);
    }

    public bool IsEdge(Arc arc)
    {
        return graph.IsEdge(arc);
    }

    private IEnumerable<Node> NodesInternal()
    {
        foreach (var node in graph.Nodes())
            if (IsEnabled(node)) yield return node;
    }

    public IEnumerable<Node> Nodes()
    {
        if (nodeExceptions.Count == 0)
        {
            if (defaultNodeEnabled) return graph.Nodes();
            return Enumerable.Empty<Node>();
        }
        return NodesInternal();
    }

    public IEnumerable<Arc> Arcs(ArcFilter filter = ArcFilter.All)
    {
        foreach (var arc in graph.Arcs(filter))
            if (IsEnabled(arc) && IsEnabled(graph.U(arc)) && IsEnabled(graph.V(arc))) yield return arc;
    }

    public IEnumerable<Arc> Arcs(Node u, ArcFilter filter = ArcFilter.All)
    {
        if (!IsEnabled(u)) yield break;
        foreach (var arc in graph.Arcs(u, filter))
            if (IsEnabled(arc) && IsEnabled(graph.Other(arc, u))) yield return arc;
    }

    public IEnumerable<Arc> Arcs(Node u, Node v, ArcFilter filter = ArcFilter.All)
    {
        if (!IsEnabled(u) || !IsEnabled(v)) yield break;
        foreach (var arc in graph.Arcs(u, v, filter))
            if (IsEnabled(arc)) yield return arc;
    }

    public int NodeCount()
    {
        return defaultNodeEnabled ? graph.NodeCount() - nodeExceptions.Count : nodeExceptions.Count;
    }

    public int ArcCount(ArcFilter filter = ArcFilter.All)
    {
        if (nodeExceptions.Count == 0 && filter == ArcFilter.All)
            return defaultNodeEnabled ?
                (defaultArcEnabled ? graph.ArcCount() - arcExceptions.Count : arcExceptions.Count)
                : 0;

        return Arcs(filter).Count();
    }

    public int ArcCount(Node u, ArcFilter filter = ArcFilter.All)
    {
        return Arcs(u, filter).Count();
    }

    public int ArcCount(Node u, Node v, ArcFilter filter = ArcFilter.All)
    {
        return Arcs(u, v, filter).Count();
    }

    public bool HasNode(Node node)
    {
        return graph.HasNode(node) && IsEnabled(node);
    }

    public bool HasArc(Arc arc)
    {
        return graph.HasArc(arc) && IsEnabled(arc);
    }
}
