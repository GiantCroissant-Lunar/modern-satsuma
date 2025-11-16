using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Plate.ModernSatsuma;
using Xunit;

namespace Plate.ModernSatsuma.Test
{
    public class DisjointPathsTests
    {
        [Fact]
        public void FindEdgeDisjointShortestPaths_OnSmallGraph_ShouldReturnTwoDisjointPaths()
        {
            var graph = new CustomGraph();
            var n1 = graph.AddNode();
            var n2 = graph.AddNode();
            var n3 = graph.AddNode();
            var n4 = graph.AddNode();

            var a12 = graph.AddArc(n1, n2, Directedness.Directed); // id 1
            var a24 = graph.AddArc(n2, n4, Directedness.Directed); // id 2
            var a13 = graph.AddArc(n1, n3, Directedness.Directed); // id 3
            var a34 = graph.AddArc(n3, n4, Directedness.Directed); // id 4

            double Cost(Arc arc) => arc.Id switch
            {
                1 => 1.0,
                2 => 1.0,
                3 => 1.0,
                4 => 2.0, // second path is slightly longer
                _ => 1.0
            };

            var paths = DisjointPaths.FindEdgeDisjointShortestPaths(
                graph,
                n1,
                n4,
                k: 2,
                cost: Cost);

            paths.Count.Should().Be(2);

            var p1Nodes = paths[0].Nodes().ToList();
            p1Nodes.Should().Equal(new[] { n1, n2, n4 });

            var p2Nodes = paths[1].Nodes().ToList();
            p2Nodes.Should().Equal(new[] { n1, n3, n4 });

            var p1Arcs = paths[0].Arcs().ToHashSet();
            var p2Arcs = paths[1].Arcs().ToHashSet();
            p1Arcs.Intersect(p2Arcs).Should().BeEmpty();
        }

        [Fact]
        public void FindEdgeDisjointShortestPaths_WhenOnlyOnePathExists_ShouldReturnSinglePath()
        {
            var graph = new CustomGraph();
            var n1 = graph.AddNode();
            var n2 = graph.AddNode();
            var n3 = graph.AddNode();

            graph.AddArc(n1, n2, Directedness.Directed);
            graph.AddArc(n2, n3, Directedness.Directed);

            var paths = DisjointPaths.FindEdgeDisjointShortestPaths(
                graph,
                n1,
                n3,
                k: 3,
                cost: _ => 1.0);

            paths.Count.Should().Be(1);
            paths[0].Nodes().ToList().Should().Equal(new[] { n1, n2, n3 });
        }

        [Fact]
        public void FindEdgeDisjointShortestPaths_WhenNoPathExists_ShouldReturnEmpty()
        {
            var graph = new CustomGraph();
            var n1 = graph.AddNode();
            var n2 = graph.AddNode();

            var paths = DisjointPaths.FindEdgeDisjointShortestPaths(
                graph,
                n1,
                n2,
                k: 2);

            paths.Should().BeEmpty();
        }

        [Fact]
        public void FindNodeDisjointShortestPaths_OnSmallGraph_ShouldReturnTwoNodeDisjointPaths()
        {
            var graph = new CustomGraph();
            var s = graph.AddNode();
            var a = graph.AddNode();
            var b = graph.AddNode();
            var t = graph.AddNode();

            graph.AddArc(s, a, Directedness.Directed);
            graph.AddArc(a, t, Directedness.Directed);
            graph.AddArc(s, b, Directedness.Directed);
            graph.AddArc(b, t, Directedness.Directed);

            var paths = DisjointPaths.FindNodeDisjointShortestPaths(graph, s, t, k: 2);

            paths.Count.Should().Be(2);

            var p1Nodes = paths[0].Nodes().ToList();
            var p2Nodes = paths[1].Nodes().ToList();

            p1Nodes.First().Should().Be(s);
            p1Nodes.Last().Should().Be(t);
            p2Nodes.First().Should().Be(s);
            p2Nodes.Last().Should().Be(t);

            var p1Internal = p1Nodes.Skip(1).Take(p1Nodes.Count - 2).ToHashSet();
            var p2Internal = p2Nodes.Skip(1).Take(p2Nodes.Count - 2).ToHashSet();
            p1Internal.Intersect(p2Internal).Should().BeEmpty();
        }

        [Fact]
        public void FindNodeDisjointShortestPaths_WhenOnlyOnePathExists_ShouldReturnSinglePath()
        {
            var graph = new CustomGraph();
            var n1 = graph.AddNode();
            var n2 = graph.AddNode();
            var n3 = graph.AddNode();
            var n4 = graph.AddNode();

            graph.AddArc(n1, n2, Directedness.Directed);
            graph.AddArc(n2, n3, Directedness.Directed);
            graph.AddArc(n3, n4, Directedness.Directed);

            var paths = DisjointPaths.FindNodeDisjointShortestPaths(graph, n1, n4, k: 3);

            paths.Count.Should().Be(1);
            paths[0].Nodes().ToList().Should().Equal(new[] { n1, n2, n3, n4 });
        }

        [Fact]
        public void FindNodeDisjointShortestPaths_WhenNoPathExists_ShouldReturnEmpty()
        {
            var graph = new CustomGraph();
            var n1 = graph.AddNode();
            var n2 = graph.AddNode();

            var paths = DisjointPaths.FindNodeDisjointShortestPaths(graph, n1, n2, k: 2);

            paths.Should().BeEmpty();
        }

        [Fact]
        public void FindTwoEdgeDisjointShortestPathsOptimal_ShouldMatchBruteForceAndBeatGreedyOnCounterexample()
        {
            // Construct a small directed graph where the greedy successive approach is suboptimal
            // compared to the Suurballe-style optimal algorithm.
            var graph = new CustomGraph();
            var s = graph.AddNode();
            var a = graph.AddNode();
            var b = graph.AddNode();
            var c = graph.AddNode();
            var d = graph.AddNode();
            var t = graph.AddNode();

            var e1 = graph.AddArc(s, a, Directedness.Directed);
            var e2 = graph.AddArc(s, b, Directedness.Directed);
            var e3 = graph.AddArc(a, c, Directedness.Directed);
            var e4 = graph.AddArc(b, d, Directedness.Directed);
            var e5 = graph.AddArc(c, t, Directedness.Directed);
            var e6 = graph.AddArc(d, t, Directedness.Directed);
            var e7 = graph.AddArc(a, d, Directedness.Directed);
            var e8 = graph.AddArc(b, c, Directedness.Directed);

            var weights = new Dictionary<Arc, double>
            {
                [e1] = 1.0,
                [e2] = 1.0,
                [e3] = 1.0,
                [e4] = 1.0,
                [e5] = 1.0,
                [e6] = 1.0,
                [e7] = 0.9,
                [e8] = 5.0,
            };

            double Cost(Arc arc) => weights[arc];

            var greedyPaths = DisjointPaths.FindEdgeDisjointShortestPaths(
                graph,
                s,
                t,
                k: 2,
                cost: Cost,
                mode: DijkstraMode.Sum);

            var optimalPaths = DisjointPaths.FindTwoEdgeDisjointShortestPathsOptimal(
                graph,
                s,
                t,
                cost: Cost,
                mode: DijkstraMode.Sum);

            greedyPaths.Count.Should().Be(2);
            optimalPaths.Count.Should().Be(2);

            double TotalCost(IReadOnlyList<IPath> paths)
            {
                return paths.Sum(p => p.Arcs().Sum(Cost));
            }

            var greedyCost = TotalCost(greedyPaths);
            var optimalCost = TotalCost(optimalPaths);

            // On this graph, the optimal algorithm should strictly beat the greedy successive approach.
            optimalCost.Should().BeLessThan(greedyCost);

            // Brute-force all simple s->t paths and compute the true minimum total cost over all
            // pairs of edge-disjoint paths, then assert the optimal algorithm matches it.
            var allPaths = new List<List<Arc>>();
            var current = new List<Arc>();
            var visited = new HashSet<Node>();

            void Dfs(Node u)
            {
                if (u.Equals(t))
                {
                    allPaths.Add(new List<Arc>(current));
                    return;
                }

                visited.Add(u);
                foreach (var arc in graph.Arcs(u, ArcFilter.Forward))
                {
                    var v = graph.V(arc);
                    if (visited.Contains(v))
                    {
                        continue;
                    }

                    current.Add(arc);
                    Dfs(v);
                    current.RemoveAt(current.Count - 1);
                }
                visited.Remove(u);
            }

            Dfs(s);

            allPaths.Count.Should().BeGreaterOrEqualTo(2);

            double bestTotal = double.PositiveInfinity;
            for (int i = 0; i < allPaths.Count; i++)
            {
                for (int j = i + 1; j < allPaths.Count; j++)
                {
                    if (!AreEdgeDisjoint(allPaths[i], allPaths[j]))
                    {
                        continue;
                    }

                    var cost1 = allPaths[i].Sum(Cost);
                    var cost2 = allPaths[j].Sum(Cost);
                    var total = cost1 + cost2;
                    if (total < bestTotal)
                    {
                        bestTotal = total;
                    }
                }
            }

            bestTotal.Should().NotBe(double.PositiveInfinity);
            optimalCost.Should().BeApproximately(bestTotal, 1e-6);
        }

        private static bool AreEdgeDisjoint(IReadOnlyList<Arc> first, IReadOnlyList<Arc> second)
        {
            for (int i = 0; i < first.Count; i++)
            {
                for (int j = 0; j < second.Count; j++)
                {
                    if (first[i].Equals(second[j]))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
