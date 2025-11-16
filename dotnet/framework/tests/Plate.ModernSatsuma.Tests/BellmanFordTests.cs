using System;
using System.Collections.Generic;
using FluentAssertions;
using Plate.ModernSatsuma;
using Xunit;

namespace Plate.ModernSatsuma.Test
{
    public class BellmanFordTests
    {
        [Fact]
        public void BellmanFord_BasicShortestPath_ShouldFindOptimalRoute()
        {
            var graph = CreateSimpleGraph();
            var sources = new[] { graph.Item1 };

            var bellmanFord = new BellmanFord(graph.Item3, _ => 1.0, sources);

            var start = graph.Item1;
            var middle = graph.Item2;
            var target = graph.Item4;

            bellmanFord.Reached(target).Should().BeTrue();
            bellmanFord.GetDistance(target).Should().Be(2.0);

            bellmanFord.TryGetDistance(target, out var distance).Should().BeTrue();
            distance.Should().Be(2.0);

            bellmanFord.TryGetPath(target, out var path).Should().BeTrue();
            path.Should().NotBeNull();
            path!.NodeCount().Should().Be(3);
            path.FirstNode.Should().Be(start);
            path.LastNode.Should().Be(target);
        }

        [Fact]
        public void BellmanFord_UnreachableNode_ShouldReportInfinityAndNoPath()
        {
            var graph = CreateSimpleGraph();
            var sources = new[] { graph.Item1 };

            var bellmanFord = new BellmanFord(graph.Item3, _ => 1.0, sources);

            var unreachable = graph.Item5;

            bellmanFord.Reached(unreachable).Should().BeFalse();
            bellmanFord.GetDistance(unreachable).Should().Be(double.PositiveInfinity);

            bellmanFord.TryGetDistance(unreachable, out var distance).Should().BeFalse();
            distance.Should().Be(double.PositiveInfinity);

            bellmanFord.TryGetPath(unreachable, out var path).Should().BeFalse();
            path.Should().BeNull();

            Span<Node> buffer = stackalloc Node[8];
            var length = bellmanFord.GetPathSpan(unreachable, buffer);
            length.Should().Be(0);
        }

        [Fact]
        public void BellmanFord_GetPathSpan_ShouldMatchGetPath()
        {
            var graph = CreateSimpleGraph();
            var sources = new[] { graph.Item1 };

            var bellmanFord = new BellmanFord(graph.Item3, _ => 1.0, sources);

            var start = graph.Item1;
            var target = graph.Item4;

            Span<Node> buffer = stackalloc Node[8];
            var length = bellmanFord.GetPathSpan(target, buffer);

            length.Should().Be(3);
            buffer[0].Should().Be(start);
            buffer[length - 1].Should().Be(target);
        }

        [Fact]
        public void BellmanFord_NegativeCycle_ShouldSetNegativeCycleAndThrowOnQueries()
        {
            var graph = CreateNegativeCycleGraph();
            var sources = new[] { graph.Item1 };

            var bellmanFord = new BellmanFord(graph.Item3, _ => -1.0, sources);

            bellmanFord.NegativeCycle.Should().NotBeNull();

            var node = graph.Item1;

            Action distanceAction = () => bellmanFord.GetDistance(node);
            distanceAction.Should().Throw<InvalidOperationException>();

            Action parentArcAction = () => bellmanFord.GetParentArc(node);
            parentArcAction.Should().Throw<InvalidOperationException>();

            Action pathAction = () => bellmanFord.GetPath(node);
            pathAction.Should().Throw<InvalidOperationException>();

            Action tryGetDistanceAction = () => bellmanFord.TryGetDistance(node, out _);
            tryGetDistanceAction.Should().Throw<InvalidOperationException>();

            Action tryGetParentArcAction = () => bellmanFord.TryGetParentArc(node, out _);
            tryGetParentArcAction.Should().Throw<InvalidOperationException>();

            Action spanAction = () =>
            {
                var localBuffer = new Node[4];
                var localSpan = new Span<Node>(localBuffer);
                bellmanFord.GetPathSpan(node, localSpan);
            };
            spanAction.Should().Throw<InvalidOperationException>();
        }

        private static Tuple<Node, Node, CustomGraph, Node, Node> CreateSimpleGraph()
        {
            var graph = new CustomGraph();
            var n1 = graph.AddNode();
            var n2 = graph.AddNode();
            var n3 = graph.AddNode();
            var n4 = graph.AddNode();

            graph.AddArc(n1, n2, Directedness.Directed);
            graph.AddArc(n2, n3, Directedness.Directed);

            return Tuple.Create(n1, n2, graph, n3, n4);
        }

        private static Tuple<Node, Node, CustomGraph> CreateNegativeCycleGraph()
        {
            var graph = new CustomGraph();
            var n1 = graph.AddNode();
            var n2 = graph.AddNode();

            graph.AddArc(n1, n2, Directedness.Undirected);

            return Tuple.Create(n1, n2, graph);
        }
    }
}
