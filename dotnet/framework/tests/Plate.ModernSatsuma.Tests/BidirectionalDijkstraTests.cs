using System.Linq;
using FluentAssertions;
using Plate.ModernSatsuma;
using Xunit;

namespace Plate.ModernSatsuma.Test
{
    public class BidirectionalDijkstraTests
    {
        [Fact]
        public void FindShortestPath_BasicLineGraph_ShouldMatchDijkstra()
        {
            var graph = new CustomGraph();
            var n1 = graph.AddNode();
            var n2 = graph.AddNode();
            var n3 = graph.AddNode();

            graph.AddArc(n1, n2, Directedness.Directed); // id 1
            graph.AddArc(n2, n3, Directedness.Directed); // id 2

            double Cost(Arc _) => 1.0;

            var bidiPath = BidirectionalDijkstra.FindShortestPath(graph, n1, n3, Cost);
            bidiPath.Should().NotBeNull();

            var nodes = bidiPath!.Nodes().ToList();
            nodes.Should().Equal(new[] { n1, n2, n3 });

            var dijkstra = new Dijkstra(graph, Cost, DijkstraMode.Sum);
            dijkstra.AddSource(n1);
            dijkstra.RunUntilFixed(n3);
            dijkstra.GetDistance(n3).Should().Be(2.0);
        }

        [Fact]
        public void FindShortestPath_WithTwoPaths_ShouldChooseCheapest()
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
                2 => 1.0, // path 1-2-4 has total cost 2
                3 => 2.0,
                4 => 2.0, // path 1-3-4 has total cost 4
                _ => 1.0
            };

            var path = BidirectionalDijkstra.FindShortestPath(graph, n1, n4, Cost);
            path.Should().NotBeNull();

            var nodes = path!.Nodes().ToList();
            nodes.Should().Equal(new[] { n1, n2, n4 });
        }

        [Fact]
        public void FindShortestPath_WhenNoPathExists_ShouldReturnNull()
        {
            var graph = new CustomGraph();
            var n1 = graph.AddNode();
            var n2 = graph.AddNode();

            var path = BidirectionalDijkstra.FindShortestPath(graph, n1, n2, _ => 1.0);

            path.Should().BeNull();
        }

        [Fact]
        public void FindShortestPath_WithMaximumMode_ShouldFallbackToDijkstra()
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

            var path = BidirectionalDijkstra.FindShortestPath(
                graph,
                n1,
                n4,
                Cost,
                DijkstraMode.Maximum);

            path.Should().NotBeNull();
            var nodes = path!.Nodes().ToList();
            nodes.Should().Equal(new[] { n1, n3, n4 });
        }

        [Fact]
        public void FindShortestPath_SourceEqualsTarget_ShouldReturnTrivialPath()
        {
            var graph = new CustomGraph();
            var n1 = graph.AddNode();

            var path = BidirectionalDijkstra.FindShortestPath(graph, n1, n1, _ => 1.0);

            path.Should().NotBeNull();
            var nodes = path!.Nodes().ToList();
            nodes.Should().Equal(new[] { n1 });
        }
    }
}
