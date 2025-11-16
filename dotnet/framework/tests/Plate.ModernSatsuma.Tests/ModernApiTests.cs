using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Plate.ModernSatsuma;
using Xunit;

namespace Plate.ModernSatsuma.Test
{
    /// <summary>
    /// Tests for modern .NET API patterns (TryGet, Builders, Async, Span)
    /// </summary>
    public class ModernApiTests
    {
        #region TryGet Pattern Tests

        [Fact]
        public void TryGetPath_WithValidPath_ShouldReturnTrueAndPath()
        {
            // Arrange
            var graph = CreateConnectedGraph();
            var dijkstra = new Dijkstra(graph, arc => 1.0, DijkstraMode.Sum);
            dijkstra.AddSource(new Node(1));
            dijkstra.Run();

            // Act
            var result = dijkstra.TryGetPath(new Node(3), out var path);

            // Assert
            result.Should().BeTrue();
            path.Should().NotBeNull();
            path!.NodeCount().Should().BeGreaterThan(1);
        }

        [Fact]
        public void TryGetPath_WithUnreachableNode_ShouldReturnFalseAndNull()
        {
            // Arrange
            var graph = CreateDisconnectedGraph();
            var dijkstra = new Dijkstra(graph, arc => 1.0, DijkstraMode.Sum);
            dijkstra.AddSource(new Node(1));
            dijkstra.Run();

            // Act
            var result = dijkstra.TryGetPath(new Node(4), out var path);

            // Assert
            result.Should().BeFalse();
            path.Should().BeNull();
        }

        [Fact]
        public void TryGetDistance_WithReachableNode_ShouldReturnDistance()
        {
            // Arrange
            var graph = CreateConnectedGraph();
            var dijkstra = new Dijkstra(graph, arc => 2.0, DijkstraMode.Sum);
            dijkstra.AddSource(new Node(1));
            dijkstra.Run();

            // Act
            var result = dijkstra.TryGetDistance(new Node(2), out var distance);

            // Assert
            result.Should().BeTrue();
            distance.Should().Be(2.0);
        }

        [Fact]
        public void TryGetDistance_WithUnreachableNode_ShouldReturnFalse()
        {
            // Arrange
            var graph = CreateDisconnectedGraph();
            var dijkstra = new Dijkstra(graph, arc => 1.0, DijkstraMode.Sum);
            dijkstra.AddSource(new Node(1));
            dijkstra.Run();

            // Act
            var result = dijkstra.TryGetDistance(new Node(4), out var distance);

            // Assert
            result.Should().BeFalse();
            distance.Should().Be(double.PositiveInfinity);
        }

        #endregion

        #region Builder Pattern Tests

        [Fact]
        public void DijkstraBuilder_FluentConfiguration_ShouldWork()
        {
            // Arrange
            var graph = CreateConnectedGraph();

            // Act
            var result = DijkstraBuilder
                .Create(graph)
                .WithCost(arc => 1.0)
                .WithMode(DijkstraMode.Sum)
                .AddSource(new Node(1))
                .Run();

            // Assert
            result.Should().NotBeNull();
            result.TryGetDistance(new Node(3), out var distance).Should().BeTrue();
        }

        [Fact]
        public void DijkstraBuilder_MultipleSourcesAndTargets_ShouldWork()
        {
            // Arrange
            var graph = CreateConnectedGraph();

            // Act
            var result = DijkstraBuilder
                .Create(graph)
                .WithCost(arc => 1.0)
                .AddSource(new Node(1))
                .AddSource(new Node(2))
                .RunUntilFixed(new Node(3));

            // Assert
            result.Should().NotBeNull();
            result.Fixed(new Node(3)).Should().BeTrue();
        }

        [Fact]
        public void BfsBuilder_BasicConfiguration_ShouldWork()
        {
            // Arrange
            var graph = CreateConnectedGraph();

            // Act
            var result = BfsBuilder
                .Create(graph)
                .AddSource(new Node(1))
                .Run();

            // Assert
            result.Should().NotBeNull();
            result.Reached(new Node(3)).Should().BeTrue();
        }

        [Fact]
        public void AStarBuilder_WithHeuristic_ShouldWork()
        {
            // Arrange
            var graph = CreateGridGraph();

            // Act
            var result = AStarBuilder
                .Create(graph)
                .WithCost(arc => 1.0)
                .WithHeuristic(node => ManhattanDistance(node, new Node(9))) // Target bottom-right
                .AddSource(new Node(1)) // Start top-left
                .RunUntilReached(new Node(9));

            // Assert
            result.Should().NotBeNull();
            result.GetDistance(new Node(9)).Should().NotBe(double.PositiveInfinity);
        }

        #endregion

        #region Async API Tests

        [Fact]
        public async Task DijkstraBuilder_RunAsync_ShouldCompleteSuccessfully()
        {
            // Arrange
            var graph = CreateConnectedGraph();
            using var cts = new CancellationTokenSource();

            // Act
            var result = await DijkstraBuilder
                .Create(graph)
                .WithCost(arc => 1.0)
                .AddSource(new Node(1))
                .RunAsync(cts.Token);

            // Assert
            result.Should().NotBeNull();
            result.Reached(new Node(3)).Should().BeTrue();
        }

        [Fact]
        public async Task DijkstraBuilder_RunAsyncWithCancellation_ShouldRespectCancellation()
        {
            // Arrange
            var graph = CreateLargeGraph();
            using var cts = new CancellationTokenSource();
            // Cancel immediately so cancellation is guaranteed before the async run.
            cts.Cancel();

            // Act & Assert
            var act = async () => await DijkstraBuilder
                .Create(graph)
                .WithCost(arc => 1.0)
                .AddSource(new Node(1))
                .RunAsync(cts.Token);

            await act.Should().ThrowAsync<OperationCanceledException>();
        }

        [Fact]
        public async Task BfsBuilder_RunAsync_ShouldWork()
        {
            // Arrange
            var graph = CreateConnectedGraph();

            // Act
            var result = await BfsBuilder
                .Create(graph)
                .AddSource(new Node(1))
                .RunAsync(CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Reached(new Node(3)).Should().BeTrue();
        }

        [Fact]
        public async Task AStarBuilder_RunUntilReachedAsync_ShouldCompleteSuccessfully()
        {
            var graph = CreateGridGraph();
            using var cts = new CancellationTokenSource();

            var result = await AStarBuilder
                .Create(graph)
                .WithCost(arc => 1.0)
                .WithHeuristic(node => ManhattanDistance(node, new Node(9)))
                .AddSource(new Node(1))
                .RunUntilReachedAsync(new Node(9), cts.Token);

            result.Should().NotBeNull();
            result.GetDistance(new Node(9)).Should().NotBe(double.PositiveInfinity);
        }

        [Fact]
        public async Task AStarBuilder_RunUntilReachedAsync_WithCancellation_ShouldRespectCancellation()
        {
            var graph = CreateGridGraph();
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            var act = async () => await AStarBuilder
                .Create(graph)
                .WithCost(arc => 1.0)
                .WithHeuristic(node => ManhattanDistance(node, new Node(9)))
                .AddSource(new Node(1))
                .RunUntilReachedAsync(new Node(9), cts.Token);

            await act.Should().ThrowAsync<OperationCanceledException>();
        }

        #endregion

        #region Span API Tests

        [Fact]
        public void GetPathSpan_WithValidPath_ShouldFillSpanAndReturnLength()
        {
            // Arrange
            var graph = CreateConnectedGraph();
            var dijkstra = new Dijkstra(graph, arc => 1.0, DijkstraMode.Sum);
            dijkstra.AddSource(new Node(1));
            dijkstra.Run();

            Span<Node> pathBuffer = stackalloc Node[10];

            // Act
            var pathLength = dijkstra.GetPathSpan(new Node(3), pathBuffer);

            // Assert
            pathLength.Should().BeGreaterThan(0);
            pathLength.Should().BeLessOrEqualTo(pathBuffer.Length);
            
            // Verify path starts with source and ends with target (same as IPath.Nodes() enumeration)
            pathBuffer[0].Should().Be(new Node(1));
            pathBuffer[pathLength - 1].Should().Be(new Node(3));
        }

        [Fact]
        public void GetPathSpan_WithUnreachableNode_ShouldReturnZero()
        {
            // Arrange
            var graph = CreateDisconnectedGraph();
            var dijkstra = new Dijkstra(graph, arc => 1.0, DijkstraMode.Sum);
            dijkstra.AddSource(new Node(1));
            dijkstra.Run();

            Span<Node> pathBuffer = stackalloc Node[10];

            // Act
            var pathLength = dijkstra.GetPathSpan(new Node(4), pathBuffer);

            // Assert
            pathLength.Should().Be(0);
        }

        [Fact]
        public void GetPathSpan_WithInsufficientBuffer_ShouldReturnNegativeLength()
        {
            // Arrange
            var graph = CreateLongPathGraph();
            var dijkstra = new Dijkstra(graph, arc => 1.0, DijkstraMode.Sum);
            dijkstra.AddSource(new Node(1));
            dijkstra.Run();

            Span<Node> smallBuffer = stackalloc Node[2]; // Too small for the path

            // Act
            var pathLength = dijkstra.GetPathSpan(new Node(5), smallBuffer);

            // Assert
            pathLength.Should().BeLessThan(0); // Indicates buffer too small
        }

        #endregion

        #region Extension Method Tests

        [Fact]
        public void Reached_WithReachableNode_ShouldReturnTrue()
        {
            // Arrange
            var graph = CreateConnectedGraph();
            var dijkstra = new Dijkstra(graph, arc => 1.0, DijkstraMode.Sum);
            dijkstra.AddSource(new Node(1));
            dijkstra.Run();

            // Act & Assert
            dijkstra.Reached(new Node(3)).Should().BeTrue();
        }

        [Fact]
        public void Reached_WithUnreachableNode_ShouldReturnFalse()
        {
            // Arrange
            var graph = CreateDisconnectedGraph();
            var dijkstra = new Dijkstra(graph, arc => 1.0, DijkstraMode.Sum);
            dijkstra.AddSource(new Node(1));
            dijkstra.Run();

            // Act & Assert
            dijkstra.Reached(new Node(4)).Should().BeFalse();
        }

        #endregion

        #region Test Graph Creation Helpers

        private static CustomGraph CreateConnectedGraph()
        {
            var graph = new CustomGraph();
            var node1 = graph.AddNode(); // Node(1)
            var node2 = graph.AddNode(); // Node(2)
            var node3 = graph.AddNode(); // Node(3)

            graph.AddArc(node1, node2, Directedness.Directed);
            graph.AddArc(node2, node3, Directedness.Directed);

            return graph;
        }

        private static CustomGraph CreateDisconnectedGraph()
        {
            var graph = new CustomGraph();
            var node1 = graph.AddNode(); // Node(1)
            var node2 = graph.AddNode(); // Node(2)
            var node3 = graph.AddNode(); // Node(3)
            var node4 = graph.AddNode(); // Node(4) - disconnected

            graph.AddArc(node1, node2, Directedness.Directed);
            graph.AddArc(node2, node3, Directedness.Directed);
            // Node 4 is intentionally disconnected

            return graph;
        }

        private static CustomGraph CreateGridGraph()
        {
            // Create a 3x3 grid graph for A* testing
            var graph = new CustomGraph();
            var nodes = new Node[9];

            // Create 9 nodes (3x3 grid)
            for (int i = 0; i < 9; i++)
            {
                nodes[i] = graph.AddNode();
            }

            // Connect adjacent nodes (4-connectivity)
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    int current = row * 3 + col;

                    // Connect to right neighbor
                    if (col < 2)
                    {
                        graph.AddArc(nodes[current], nodes[current + 1], Directedness.Undirected);
                    }

                    // Connect to bottom neighbor
                    if (row < 2)
                    {
                        graph.AddArc(nodes[current], nodes[current + 3], Directedness.Undirected);
                    }
                }
            }

            return graph;
        }

        private static CustomGraph CreateLongPathGraph()
        {
            var graph = new CustomGraph();
            var nodes = new List<Node>();

            // Create a chain of 10 nodes
            for (int i = 0; i < 10; i++)
            {
                nodes.Add(graph.AddNode());
            }

            // Connect them in a chain
            for (int i = 0; i < 9; i++)
            {
                graph.AddArc(nodes[i], nodes[i + 1], Directedness.Directed);
            }

            return graph;
        }

        private static CustomGraph CreateLargeGraph()
        {
            var graph = new CustomGraph();
            var nodes = new List<Node>();

            // Create 1000 nodes for cancellation testing
            for (int i = 0; i < 1000; i++)
            {
                nodes.Add(graph.AddNode());
            }

            // Create many edges to make algorithm work harder
            var random = new Random(42);
            for (int i = 0; i < nodes.Count; i++)
            {
                for (int j = 0; j < 5; j++) // 5 random connections per node
                {
                    var target = random.Next(nodes.Count);
                    if (target != i)
                    {
                        graph.AddArc(nodes[i], nodes[target], Directedness.Directed);
                    }
                }
            }

            return graph;
        }

        private static double ManhattanDistance(Node from, Node to)
        {
            // Simple Manhattan distance for 3x3 grid
            // Assumes nodes are numbered 1-9 in row-major order
            int fromIndex = (int)from.Id - 1;
            int toIndex = (int)to.Id - 1;

            int fromRow = fromIndex / 3;
            int fromCol = fromIndex % 3;
            int toRow = toIndex / 3;
            int toCol = toIndex % 3;

            return Math.Abs(fromRow - toRow) + Math.Abs(fromCol - toCol);
        }

        #endregion
    }
}