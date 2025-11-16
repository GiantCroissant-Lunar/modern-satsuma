using System;
using System.Collections.Generic;

namespace Plate.ModernSatsuma;

/// <summary>
/// Provides helpers for computing shortest paths using a bidirectional variant of Dijkstra's algorithm.
/// </summary>
public static class BidirectionalDijkstra
{
    /// <summary>
    /// Finds a shortest path between <paramref name="source"/> and <paramref name="target"/>.
    /// For <see cref="DijkstraMode.Sum"/>, a bidirectional search is used when possible; for other
    /// modes the method falls back to the standard <see cref="Dijkstra"/> implementation.
    /// </summary>
    public static IPath? FindShortestPath(
        IGraph graph,
        Node source,
        Node target,
        Func<Arc, double> cost,
        DijkstraMode mode = DijkstraMode.Sum)
    {
        if (graph == null) throw new ArgumentNullException(nameof(graph));
        if (cost == null) throw new ArgumentNullException(nameof(cost));

        if (source == target)
        {
            var trivial = new Path(graph);
            trivial.Begin(source);
            return trivial;
        }

        // For non-Sum modes, fall back to standard Dijkstra to preserve semantics.
        if (mode != DijkstraMode.Sum)
        {
            var dijkstra = new Dijkstra(graph, cost, mode);
            dijkstra.AddSource(source);
            var fixedTarget = dijkstra.RunUntilFixed(target);
            if (fixedTarget == Node.Invalid)
            {
                return null;
            }

            return dijkstra.GetPath(target);
        }

        var forwardDist = new Dictionary<Node, double>();
        var backwardDist = new Dictionary<Node, double>();
        var forwardParent = new Dictionary<Node, Arc>();
        var backwardParent = new Dictionary<Node, Arc>();

        var forwardQueue = new PriorityQueue<Node, double>();
        var backwardQueue = new PriorityQueue<Node, double>();

        forwardDist[source] = 0.0;
        forwardParent[source] = Arc.Invalid;
        forwardQueue[source] = 0.0;

        backwardDist[target] = 0.0;
        backwardParent[target] = Arc.Invalid;
        backwardQueue[target] = 0.0;

        double bestCost = double.PositiveInfinity;
        var meetingNode = Node.Invalid;

        while (forwardQueue.Count > 0 && backwardQueue.Count > 0)
        {
            double forwardMinDist;
            double backwardMinDist;
            var forwardMinNode = forwardQueue.Peek(out forwardMinDist);
            var backwardMinNode = backwardQueue.Peek(out backwardMinDist);

            if (forwardMinDist + backwardMinDist >= bestCost)
            {
                break;
            }

            if (forwardMinDist <= backwardMinDist)
            {
                // Expand from the forward search
                forwardQueue.Pop();
                if (!forwardDist.TryGetValue(forwardMinNode, out var storedDist) || storedDist < forwardMinDist)
                {
                    continue; // outdated entry
                }

                foreach (var arc in graph.Arcs(forwardMinNode, ArcFilter.Forward))
                {
                    var v = graph.Other(arc, forwardMinNode);
                    var arcCost = cost(arc);
                    if (arcCost < 0)
                    {
                        throw new InvalidOperationException("Invalid cost: " + arcCost);
                    }

                    var newDist = forwardMinDist + arcCost;
                    double oldDist;
                    if (!forwardDist.TryGetValue(v, out oldDist) || newDist < oldDist)
                    {
                        forwardDist[v] = newDist;
                        forwardParent[v] = arc;
                        forwardQueue[v] = newDist;

                        if (backwardDist.TryGetValue(v, out var backDist))
                        {
                            var total = newDist + backDist;
                            if (total < bestCost)
                            {
                                bestCost = total;
                                meetingNode = v;
                            }
                        }
                    }
                }
            }
            else
            {
                // Expand from the backward search
                backwardQueue.Pop();
                if (!backwardDist.TryGetValue(backwardMinNode, out var storedDist) || storedDist < backwardMinDist)
                {
                    continue; // outdated entry
                }

                foreach (var arc in graph.Arcs(backwardMinNode, ArcFilter.Backward))
                {
                    var v = graph.Other(arc, backwardMinNode);
                    var arcCost = cost(arc);
                    if (arcCost < 0)
                    {
                        throw new InvalidOperationException("Invalid cost: " + arcCost);
                    }

                    var newDist = backwardMinDist + arcCost;
                    double oldDist;
                    if (!backwardDist.TryGetValue(v, out oldDist) || newDist < oldDist)
                    {
                        backwardDist[v] = newDist;
                        backwardParent[v] = arc;
                        backwardQueue[v] = newDist;

                        if (forwardDist.TryGetValue(v, out var fDist))
                        {
                            var total = newDist + fDist;
                            if (total < bestCost)
                            {
                                bestCost = total;
                                meetingNode = v;
                            }
                        }
                    }
                }
            }
        }

        if (meetingNode == Node.Invalid)
        {
            return null;
        }

        // Reconstruct prefix: source -> meetingNode
        var path = new Path(graph);
        path.Begin(meetingNode);

        var current = meetingNode;
        while (forwardParent.TryGetValue(current, out var forwardArc) && forwardArc != Arc.Invalid)
        {
            path.AddFirst(forwardArc);
            current = graph.Other(forwardArc, current);
        }

        // Reconstruct suffix: meetingNode -> target
        current = meetingNode;
        while (backwardParent.TryGetValue(current, out var backwardArc) && backwardArc != Arc.Invalid)
        {
            path.AddLast(backwardArc);
            current = graph.Other(backwardArc, current);
            if (current == target)
            {
                break;
            }
        }

        return path;
    }
}
