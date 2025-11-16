using System.Linq;
using FluentAssertions;
using Plate.ModernSatsuma;
using Xunit;

namespace Plate.ModernSatsuma.Test
{
    public class KShortestPathsTests
    {
        [Fact]
        public void FindKShortestSimplePaths_OnSmallGraph_ShouldReturnPathsInCostOrder()
        {
            // Arrange: create a small graph with two simple paths between source and target
            var graph = new CustomGraph();
            var n1 = graph.AddNode(); // Node(1)
            var n2 = graph.AddNode(); // Node(2)
            var n3 = graph.AddNode(); // Node(3)
            var n4 = graph.AddNode(); // Node(4)

            // Arcs are directed for simplicity
            var a12 = graph.AddArc(n1, n2, Directedness.Directed); // id 1
            var a24 = graph.AddArc(n2, n4, Directedness.Directed); // id 2
            var a13 = graph.AddArc(n1, n3, Directedness.Directed); // id 3
            var a34 = graph.AddArc(n3, n4, Directedness.Directed); // id 4

            // Act
            var paths = KShortestPaths.FindKShortestSimplePaths(
                graph,
                n1,
                n4,
                k: 3,
                cost: arc => arc.Id switch
                {
                    1 => 1.0, // 1 -> 2
                    2 => 1.0, // 2 -> 4
                    3 => 1.0, // 1 -> 3
                    4 => 2.0, // 3 -> 4 (longer)
                    _ => 1.0
                });

            // Assert
            paths.Count.Should().Be(2); // only two simple paths exist

            var p1Nodes = paths[0].Nodes().ToList();
            p1Nodes.Should().Equal(new[] { n1, n2, n4 });

            var p2Nodes = paths[1].Nodes().ToList();
            p2Nodes.Should().Equal(new[] { n1, n3, n4 });
        }

        [Fact]
        public void FindKShortestSimplePaths_WhenNoPathExists_ShouldReturnEmpty()
        {
            var graph = new CustomGraph();
            var n1 = graph.AddNode();
            var n2 = graph.AddNode();

            var paths = KShortestPaths.FindKShortestSimplePaths(
                graph,
                n1,
                n2,
                k: 3,
                cost: _ => 1.0);

            paths.Should().BeEmpty();
        }

        [Fact]
        public void FindKShortestSimplePaths_WithKEqualsOne_ShouldReturnSingleShortestPath()
        {
            var graph = new CustomGraph();
            var n1 = graph.AddNode();
            var n2 = graph.AddNode();
            var n3 = graph.AddNode();

            graph.AddArc(n1, n2, Directedness.Directed); // id 1
            graph.AddArc(n2, n3, Directedness.Directed); // id 2

            var paths = KShortestPaths.FindKShortestSimplePaths(
                graph,
                n1,
                n3,
                k: 1);

            paths.Count.Should().Be(1);
            paths[0].Nodes().ToList().Should().Equal(new[] { n1, n2, n3 });
        }

        [Fact]
        public void FindKShortestSimplePaths_WithMaximumMode_ShouldOrderByMaxEdgeCost()
        {
            var graph = new CustomGraph();
            var n1 = graph.AddNode();
            var n2 = graph.AddNode();
            var n3 = graph.AddNode();
            var n4 = graph.AddNode();

            graph.AddArc(n1, n2, Directedness.Directed); // id 1
            graph.AddArc(n2, n4, Directedness.Directed); // id 2
            graph.AddArc(n1, n3, Directedness.Directed); // id 3
            graph.AddArc(n3, n4, Directedness.Directed); // id 4

            double Cost(Arc arc) => arc.Id switch
            {
                1 => 1.0,
                2 => 5.0, // path 1-2-4 has max cost 5
                3 => 2.0,
                4 => 3.0, // path 1-3-4 has max cost 3 (preferred)
                _ => 1.0
            };

            var paths = KShortestPaths.FindKShortestSimplePaths(
                graph,
                n1,
                n4,
                k: 2,
                cost: Cost,
                mode: DijkstraMode.Maximum);

            paths.Count.Should().Be(2);

            var first = paths[0].Nodes().ToList();
            first.Should().Equal(new[] { n1, n3, n4 });

            var second = paths[1].Nodes().ToList();
            second.Should().Equal(new[] { n1, n2, n4 });
        }

        [Fact]
        public void FindKShortestSimplePaths_WithKGreaterThanPathCount_ShouldReturnAllExistingPaths()
        {
            var graph = new CustomGraph();
            var n1 = graph.AddNode();
            var n2 = graph.AddNode();
            var n3 = graph.AddNode();

            graph.AddArc(n1, n2, Directedness.Directed);
            graph.AddArc(n2, n3, Directedness.Directed);

            var paths = KShortestPaths.FindKShortestSimplePaths(
                graph,
                n1,
                n3,
                k: 10);

            paths.Count.Should().Be(1);
        }
    }
}
