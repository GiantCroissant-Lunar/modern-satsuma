using System;
using System.Collections.Generic;
using System.Linq;

namespace Plate.ModernSatsuma;

/// Represents a connector arc that joins nodes from different graphs in a UnionGraph.
public readonly struct Connector
{
    /// The source node (must be a wrapped UnionGraph node).
    public Node From { get; }
    /// The target node (must be a wrapped UnionGraph node).
    public Node To { get; }
    /// Whether this connector is undirected (edge) or directed.
    public Directedness Directedness { get; }

    /// Creates a directed connector arc from one node to another.
    public Connector(Node from, Node to, Directedness directedness = Directedness.Directed)
    {
        From = from;
        To = to;
        Directedness = directedness;
    }

    /// Creates a connector arc from nodes in specific graphs.
    /// <param name="union">The UnionGraph containing the source graphs.</param>
    /// <param name="fromGraphIndex">The index of the graph containing the source node.</param>
    /// <param name="fromNode">The source node in its original graph.</param>
    /// <param name="toGraphIndex">The index of the graph containing the target node.</param>
    /// <param name="toNode">The target node in its original graph.</param>
    /// <param name="directedness">Whether the connector is directed or undirected.</param>
    public static Connector Create(UnionGraph union, int fromGraphIndex, Node fromNode, int toGraphIndex, Node toNode, Directedness directedness = Directedness.Directed)
    {
        return new Connector(
            union.WrapNode(fromGraphIndex, fromNode),
            union.WrapNode(toGraphIndex, toNode),
            directedness);
    }
}

/// Adaptor for joining multiple graphs with connector arcs between them.
/// Built on top of UnionGraph, this adaptor adds additional arcs that connect
/// nodes across different source graphs.
///
/// The underlying graphs must NOT be modified while using this adaptor.
/// Connector arcs are managed by this adaptor and can be added/removed.
///
/// Memory usage: O(k + c) where k is the number of source graphs and c is the number of connectors.
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
/// g2.AddArc(c, d, Directedness.Directed);
///
/// // Create a joined graph with a connector from g1.b to g2.c
/// var join = new JoinGraph(new[] { g1, g2 });
/// join.AddConnector(Connector.Create(join.Union, 0, b, 1, c, Directedness.Directed));
///
/// // Now there's a path from a through b to c to d across both graphs
/// Console.WriteLine($"Joined graph has {join.NodeCount()} nodes and {join.ArcCount()} arcs");
/// // Output: Joined graph has 4 nodes and 3 arcs
/// </code>
/// <seealso cref="UnionGraph"/>
/// <seealso cref="Connector"/>
public class JoinGraph : IGraph
{
    private readonly UnionGraph union;
    private readonly List<Connector> connectors;

    // Connector arc IDs start from a high value to avoid collisions
    private const long ConnectorArcIdBase = 1L << 62;
    private long nextConnectorId;

    // Maps connector arc IDs to their definitions
    private readonly Dictionary<Arc, Connector> connectorArcs;

    // Maps nodes to their connector arcs for efficient adjacency queries
    private readonly Dictionary<Node, List<Arc>> nodeConnectors;

    /// Creates a JoinGraph from multiple source graphs.
    /// <param name="graphs">The source graphs to join.</param>
    public JoinGraph(params IGraph[] graphs)
        : this((IReadOnlyList<IGraph>)graphs)
    {
    }

    /// Creates a JoinGraph from a collection of source graphs.
    /// <param name="graphs">The source graphs to join.</param>
    public JoinGraph(IReadOnlyList<IGraph> graphs)
    {
        union = new UnionGraph(graphs);
        connectors = new List<Connector>();
        connectorArcs = new Dictionary<Arc, Connector>();
        nodeConnectors = new Dictionary<Node, List<Arc>>();
        nextConnectorId = ConnectorArcIdBase;
    }

    /// The underlying UnionGraph.
    public UnionGraph Union => union;

    /// The number of source graphs.
    public int GraphCount => union.GraphCount;

    /// Gets a source graph by index.
    public IGraph GetGraph(int index) => union.GetGraph(index);

    /// The connector arcs added to this join.
    public IReadOnlyList<Connector> Connectors => connectors;

    #region Connector Management

    /// Adds a connector arc between nodes.
    /// <param name="connector">The connector to add.</param>
    /// <returns>The Arc representing this connector.</returns>
    public Arc AddConnector(Connector connector)
    {
        if (!union.HasNode(connector.From))
            throw new ArgumentException("From node is not in the union graph.", nameof(connector));
        if (!union.HasNode(connector.To))
            throw new ArgumentException("To node is not in the union graph.", nameof(connector));

        var arc = new Arc(nextConnectorId++);
        connectors.Add(connector);
        connectorArcs[arc] = connector;

        // Add to adjacency lists
        AddToNodeConnectors(connector.From, arc);
        if (connector.From != connector.To)
            AddToNodeConnectors(connector.To, arc);

        return arc;
    }

    /// Adds a connector arc between nodes from specific graphs.
    public Arc AddConnector(int fromGraphIndex, Node fromNode, int toGraphIndex, Node toNode, Directedness directedness = Directedness.Directed)
    {
        return AddConnector(Connector.Create(union, fromGraphIndex, fromNode, toGraphIndex, toNode, directedness));
    }

    /// Adds multiple connector arcs at once.
    public void AddConnectors(IEnumerable<Connector> connectorsToAdd)
    {
        foreach (var connector in connectorsToAdd)
            AddConnector(connector);
    }

    private void AddToNodeConnectors(Node node, Arc arc)
    {
        if (!nodeConnectors.TryGetValue(node, out var list))
        {
            list = new List<Arc>();
            nodeConnectors[node] = list;
        }
        list.Add(arc);
    }

    /// Removes a connector arc.
    /// <param name="arc">The arc to remove.</param>
    /// <returns>True if the arc was a connector and was removed.</returns>
    public bool RemoveConnector(Arc arc)
    {
        if (!connectorArcs.TryGetValue(arc, out var connector))
            return false;

        connectorArcs.Remove(arc);
        connectors.Remove(connector);

        // Remove from adjacency lists
        if (nodeConnectors.TryGetValue(connector.From, out var fromList))
            fromList.Remove(arc);
        if (connector.From != connector.To && nodeConnectors.TryGetValue(connector.To, out var toList))
            toList.Remove(arc);

        return true;
    }

    /// Checks if an arc is a connector arc (as opposed to an arc from the source graphs).
    public bool IsConnectorArc(Arc arc)
    {
        return connectorArcs.ContainsKey(arc);
    }

    /// Clears all connector arcs.
    public void ClearConnectors()
    {
        connectors.Clear();
        connectorArcs.Clear();
        nodeConnectors.Clear();
    }

    #endregion

    #region IGraph Implementation

    public Node U(Arc arc)
    {
        if (connectorArcs.TryGetValue(arc, out var connector))
            return connector.From;
        return union.U(arc);
    }

    public Node V(Arc arc)
    {
        if (connectorArcs.TryGetValue(arc, out var connector))
            return connector.To;
        return union.V(arc);
    }

    public bool IsEdge(Arc arc)
    {
        if (connectorArcs.TryGetValue(arc, out var connector))
            return connector.Directedness == Directedness.Undirected;
        return union.IsEdge(arc);
    }

    public IEnumerable<Node> Nodes()
    {
        return union.Nodes();
    }

    public IEnumerable<Arc> Arcs(ArcFilter filter = ArcFilter.All)
    {
        // First yield union arcs
        foreach (var arc in union.Arcs(filter))
            yield return arc;

        // Then yield connector arcs
        foreach (var kvp in connectorArcs)
        {
            if (MatchesFilter(kvp.Key, kvp.Value, filter))
                yield return kvp.Key;
        }
    }

    public IEnumerable<Arc> Arcs(Node u, ArcFilter filter = ArcFilter.All)
    {
        // Arcs from the union
        foreach (var arc in union.Arcs(u, filter))
            yield return arc;

        // Connector arcs
        if (nodeConnectors.TryGetValue(u, out var connectorList))
        {
            foreach (var arc in connectorList)
            {
                if (connectorArcs.TryGetValue(arc, out var connector) && MatchesFilter(arc, connector, filter, u))
                    yield return arc;
            }
        }
    }

    public IEnumerable<Arc> Arcs(Node u, Node v, ArcFilter filter = ArcFilter.All)
    {
        // Arcs from the union
        foreach (var arc in union.Arcs(u, v, filter))
            yield return arc;

        // Check connector arcs
        if (nodeConnectors.TryGetValue(u, out var connectorList))
        {
            foreach (var arc in connectorList)
            {
                if (connectorArcs.TryGetValue(arc, out var connector))
                {
                    var other = connector.From == u ? connector.To : connector.From;
                    if (other == v && MatchesFilter(arc, connector, filter, u))
                        yield return arc;
                }
            }
        }
    }

    private bool MatchesFilter(Arc arc, Connector connector, ArcFilter filter)
    {
        if (filter == ArcFilter.All) return true;
        if (filter == ArcFilter.Edge) return connector.Directedness == Directedness.Undirected;
        // For Forward/Backward, we need context of which node we're looking from
        return true;
    }

    private bool MatchesFilter(Arc arc, Connector connector, ArcFilter filter, Node fromNode)
    {
        switch (filter)
        {
            case ArcFilter.All:
                return true;
            case ArcFilter.Edge:
                return connector.Directedness == Directedness.Undirected;
            case ArcFilter.Forward:
                if (connector.Directedness == Directedness.Undirected) return true;
                return connector.From == fromNode;
            case ArcFilter.Backward:
                if (connector.Directedness == Directedness.Undirected) return true;
                return connector.To == fromNode;
            default:
                return true;
        }
    }

    public int NodeCount()
    {
        return union.NodeCount();
    }

    public int ArcCount(ArcFilter filter = ArcFilter.All)
    {
        int count = union.ArcCount(filter);
        foreach (var kvp in connectorArcs)
        {
            if (MatchesFilter(kvp.Key, kvp.Value, filter))
                count++;
        }
        return count;
    }

    public int ArcCount(Node u, ArcFilter filter = ArcFilter.All)
    {
        int count = union.ArcCount(u, filter);
        if (nodeConnectors.TryGetValue(u, out var connectorList))
        {
            foreach (var arc in connectorList)
            {
                if (connectorArcs.TryGetValue(arc, out var connector) && MatchesFilter(arc, connector, filter, u))
                    count++;
            }
        }
        return count;
    }

    public int ArcCount(Node u, Node v, ArcFilter filter = ArcFilter.All)
    {
        int count = union.ArcCount(u, v, filter);
        if (nodeConnectors.TryGetValue(u, out var connectorList))
        {
            foreach (var arc in connectorList)
            {
                if (connectorArcs.TryGetValue(arc, out var connector))
                {
                    var other = connector.From == u ? connector.To : connector.From;
                    if (other == v && MatchesFilter(arc, connector, filter, u))
                        count++;
                }
            }
        }
        return count;
    }

    public bool HasNode(Node node)
    {
        return union.HasNode(node);
    }

    public bool HasArc(Arc arc)
    {
        return connectorArcs.ContainsKey(arc) || union.HasArc(arc);
    }

    #endregion

    #region Materialization

    /// Materializes the joined graph into a mutable CustomGraph.
    /// This copies all nodes and arcs (including connectors) into a new graph.
    /// <returns>A new CustomGraph containing all nodes and arcs from the join.</returns>
    public CustomGraph ToCustomGraph()
    {
        var result = new CustomGraph();
        var nodeMapping = new Dictionary<Node, Node>();

        // Copy all nodes
        foreach (var node in Nodes())
        {
            nodeMapping[node] = result.AddNode();
        }

        // Copy all arcs (including connectors)
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
