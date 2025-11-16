using System.Collections.Generic;
using FluentAssertions;
using Plate.ModernSatsuma;
using Xunit;

namespace Plate.ModernSatsuma.Test
{
    public class DfsTests
    {
        private sealed class RecordingDfs : Dfs
        {
            public readonly List<Node> EnterOrder = new();
            public readonly List<int> Levels = new();

            protected override void Start(out Direction direction)
            {
                direction = Direction.Forward;
            }

            protected override bool NodeEnter(Node node, Arc arc)
            {
                EnterOrder.Add(node);
                Levels.Add(Level);
                return true;
            }
        }

        [Fact]
        public void Dfs_ShouldVisitAllReachableNodesAndTrackLevels()
        {
            var graph = new CustomGraph();
            var n1 = graph.AddNode();
            var n2 = graph.AddNode();
            var n3 = graph.AddNode();

            graph.AddArc(n1, n2, Directedness.Directed);
            graph.AddArc(n2, n3, Directedness.Directed);

            var dfs = new RecordingDfs();
            dfs.Run(graph, new[] { n1 });

            dfs.EnterOrder.Should().Contain(new[] { n1, n2, n3 });
            dfs.Levels.Should().HaveCount(3);
            dfs.Levels[0].Should().Be(0);
            dfs.Levels[1].Should().Be(1);
            dfs.Levels[2].Should().Be(2);
        }
    }
}
