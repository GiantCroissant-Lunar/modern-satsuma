

using System;
using System.Collections.Generic;
using System.Linq;

namespace Plate.ModernSatsuma;

/// Finds a maximum matching in a bipartite graph using the alternating path algorithm.
/// \sa BipartiteMinimumCostMatching
public sealed class BipartiteMaximumMatching : IClearable
{
    public IGraph Graph { get; private set; }
    /// Describes a bipartition of the input graph by dividing its nodes into red and blue ones.
    public Func<Node, bool> IsRed { get; private set; }

    private readonly Matching matching;

    /// The current matching.
    public IMatching Matching => matching;

    private readonly HashSet<Node> unmatchedRedNodes;

    public BipartiteMaximumMatching(IGraph graph, Func<Node, bool> isRed)
    {
        Graph = graph;
        IsRed = isRed;
        matching = new Matching(Graph);
        unmatchedRedNodes = new();

        Clear();
    }

    /// Removes all arcs from the matching.
    public void Clear()
    {
        matching.Clear();
        unmatchedRedNodes.Clear();
        foreach (var n in Graph.Nodes())
            if (IsRed(n)) unmatchedRedNodes.Add(n);

    }

    /// Grows the current matching greedily.
    /// Can be used to speed up optimization by finding a reasonable initial matching.
    /// \param maxImprovements The maximum number of arcs to grow the current matching with.
    /// \return The number of arcs added to the matching.
    public int GreedyGrow(int maxImprovements = int.MaxValue)
    {
        int result = 0;
        List<Node> matchedRedNodes = new();
        foreach (var x in unmatchedRedNodes)
            foreach (var arc in Graph.Arcs(x))
            {
                Node y = Graph.Other(arc, x);
                if (!matching.HasNode(y))
                {
                    matching.Enable(arc, true);
                    matchedRedNodes.Add(x);
                    result++;
                    if (result >= maxImprovements) goto BreakAll;
                    break;
                }
            }
        BreakAll:
        foreach (var n in matchedRedNodes) unmatchedRedNodes.Remove(n);
        return result;
    }

    /// Tries to add a specific arc to the current matching.
    /// If the arc is already present, does nothing.
    /// \param arc An arc of #Graph.
    /// \exception ArgumentException Trying to add an illegal arc.
    public void Add(Arc arc)
    {
        if (matching.HasArc(arc)) return;
        matching.Enable(arc, true);
        Node u = Graph.U(arc);
        unmatchedRedNodes.Remove(IsRed(u) ? u : Graph.V(arc));
    }

    private Dictionary<Node, Arc> parentArc;
    private Node Traverse(Node node)
    {
        Arc matchedArc = matching.MatchedArc(node);

        if (IsRed(node))
        {
            foreach (var arc in Graph.Arcs(node))
                if (arc != matchedArc)
                {
                    Node y = Graph.Other(arc, node);
                    if (!parentArc.ContainsKey(y))
                    {
                        parentArc[y] = arc;
                        if (!matching.HasNode(y)) return y;
                        Node result = Traverse(y);
                        if (result != Node.Invalid) return result;
                    }
                }
        }
        else
        {
            Node y = Graph.Other(matchedArc, node);
            if (!parentArc.ContainsKey(y))
            {
                parentArc[y] = matchedArc;
                Node result = Traverse(y);
                if (result != Node.Invalid) return result;
            }
        }

        return Node.Invalid;
    }

    /// Grows the current matching to a maximum matching by running the whole alternating path algorithm.
    /// \note Calling #GreedyGrow before #Run may speed up operation.
    public void Run()
    {
        List<Node> matchedRedNodes = new();
        parentArc = new();
        foreach (var x in unmatchedRedNodes)
        {
            parentArc.Clear();
            parentArc[x] = Arc.Invalid;

            // find an alternating path
            Node y = Traverse(x);
            if (y == Node.Invalid) continue;

            // modify matching along the alternating path
            while (true)
            {
                // y ----arc---- z (====arc2===)
                Arc arc = parentArc[y];
                Node z = Graph.Other(arc, y);
                Arc arc2 = (z == x ? Arc.Invalid : parentArc[z]);
                if (arc2 != Arc.Invalid) matching.Enable(arc2, false);
                matching.Enable(arc, true);
                if (arc2 == Arc.Invalid) break;
                y = Graph.Other(arc2, z);
            }

            matchedRedNodes.Add(x);
        }
        parentArc = null;

        foreach (var n in matchedRedNodes) unmatchedRedNodes.Remove(n);
    }
}
