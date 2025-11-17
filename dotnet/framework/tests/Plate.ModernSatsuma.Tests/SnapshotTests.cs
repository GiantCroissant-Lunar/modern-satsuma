using System;
using System.Threading.Tasks;
using VerifyTests;
using VerifyXunit;
using Xunit;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Plate.ModernSatsuma;

namespace Plate.ModernSatsuma.Test
{
    public class SnapshotTests
    {
        [Fact]
        public Task VerifySimpleTextSnapshot()
        {
            const string text = "Snapshot test for Plate.ModernSatsuma";
            return Verifier.Verify(text);
        }

        [Fact]
        public Task VerifyCustomGraphStructure()
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

            var description = DescribeGraph(graph);
            return Verifier.Verify(description);
        }

        private static string DescribeGraph(IGraph graph)
        {
            var sb = new StringBuilder();
            var nodes = graph.Nodes().OrderBy(n => n.Id).ToList();
            sb.AppendLine("Nodes:");
            foreach (var node in nodes)
            {
                sb.AppendLine(node.Id.ToString());
            }

            var arcs = graph.Arcs(ArcFilter.All)
                .OrderBy(a => graph.U(a).Id)
                .ThenBy(a => graph.V(a).Id)
                .ThenBy(a => a.Id)
                .ToList();

            sb.AppendLine();
            sb.AppendLine("Arcs:");
            foreach (var arc in arcs)
            {
                var u = graph.U(arc);
                var v = graph.V(arc);
                var kind = graph.IsEdge(arc) ? "Edge" : "Arc";
                sb.AppendLine($"{arc.Id}: {u.Id} -> {v.Id} ({kind})");
            }

            return sb.ToString();
        }

        [Fact]
        public Task VerifyDisjointPathsStructureAndPaths()
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

            var paths = graph
                .BuildDisjointPaths()
                .From(s)
                .To(t)
                .EdgeDisjoint()
                .WithK(2)
                .WithCost(_ => 1.0)
                .WithMode(DijkstraMode.Sum)
                .WithStrategy(DisjointPathsStrategy.OptimalWhenAvailable)
                .Run();

            var sb = new StringBuilder();
            sb.AppendLine(DescribeGraph(graph));
            sb.AppendLine();
            sb.AppendLine("Paths:");
            sb.Append(DescribePaths(graph, paths));

            return Verifier.Verify(sb.ToString());
        }

        private static string DescribePaths(IGraph graph, IReadOnlyList<IPath> paths)
        {
            var pathStrings = new List<string>(paths.Count);
            foreach (var path in paths)
            {
                var nodes = path.Nodes().Select(n => n.Id).ToList();
                var repr = string.Join(" -> ", nodes);
                pathStrings.Add(repr);
            }

            pathStrings.Sort(StringComparer.Ordinal);

            var sb = new StringBuilder();
            for (int i = 0; i < pathStrings.Count; i++)
            {
                sb.AppendLine($"Path {i + 1}: {pathStrings[i]}");
            }

            return sb.ToString();
        }
    }
}
