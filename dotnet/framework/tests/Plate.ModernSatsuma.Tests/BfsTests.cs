using System;
using System.Collections.Generic;
using FluentAssertions;
using Plate.ModernSatsuma;
using Xunit;

namespace Plate.ModernSatsuma.Test
{
    public class BfsTests
    {
        [Fact]
        public void GetLevel_OnSimpleLineGraph_ShouldMatchDistanceFromSource()
        {
            var graph = new CustomGraph();
            var n1 = graph.AddNode();
            var n2 = graph.AddNode();
            var n3 = graph.AddNode();

            graph.AddArc(n1, n2, Directedness.Directed);
            graph.AddArc(n2, n3, Directedness.Directed);

            var bfs = new Bfs(graph);
            bfs.AddSource(n1);
            bfs.Run();

            bfs.GetLevel(n1).Should().Be(0);
            bfs.GetLevel(n2).Should().Be(1);
            bfs.GetLevel(n3).Should().Be(2);
        }

        [Fact]
        public void TryGetPath_ShouldReturnShortestPathOrNull()
        {
            var graph = new CustomGraph();
            var n1 = graph.AddNode();
            var n2 = graph.AddNode();
            var n3 = graph.AddNode();
            var n4 = graph.AddNode(); // unreachable

            graph.AddArc(n1, n2, Directedness.Directed);
            graph.AddArc(n2, n3, Directedness.Directed);

            var bfs = new Bfs(graph);
            bfs.AddSource(n1);
            bfs.Run();

            bfs.TryGetPath(n3, out var pathTo3).Should().BeTrue();
            pathTo3.Should().NotBeNull();
            pathTo3!.FirstNode.Should().Be(n1);
            pathTo3.LastNode.Should().Be(n3);

            bfs.TryGetPath(n4, out var pathTo4).Should().BeFalse();
            pathTo4.Should().BeNull();
        }
    }
}
