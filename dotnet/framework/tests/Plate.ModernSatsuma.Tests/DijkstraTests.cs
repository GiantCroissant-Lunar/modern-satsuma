using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Plate.ModernSatsuma;
using Xunit;

namespace Plate.ModernSatsuma.Test
{
    /// <summary>
    /// Comprehensive tests for Dijkstra's shortest path algorithm
    /// </summary>
    public class DijkstraTests
    {
        [Fact]
        public void Dijkstra_BasicShortestPath_ShouldFindOptimalRoute()
        {
            // Arrange
            var graph = CreateSimpleTestGraph();
            var dijkstra = new Dijkstra(graph, GetEdgeWeight, DijkstraMode.Sum);
            
            // Act
            dijkstra.AddSource(new Node(1));
            dijkstra.Run();
            
            // Assert
            dijkstra.Reached(new Node(3)).Should().BeTrue();
            dijkstra.GetDistance(new Node(3)).Should().Be(3.0); // 1->2 (2) + 2->3 (1) = 3
            
            var path = dijkstra.GetPath(new Node(3));
            path.Should().NotBeNull();
            path!.NodeCount().Should().Be(3); // nodes 1, 2, 3
        }

        [Fact]
        public void Dijkstra_UnreachableNode_ShouldReturnInfiniteDistance()
        {
            // Arrange
            var graph = CreateDisconnectedGraph();
            var dijkstra = new Dijkstra(graph, GetEdgeWeight, DijkstraMode.Sum);
            
            // Act
            dijkstra.AddSource(new Node(1));
            dijkstra.Run();
            
            // Assert
            dijkstra.Reached(new Node(4)).Should().BeFalse();
            dijkstra.GetDistance(new Node(4)).Should().Be(double.PositiveInfinity);
            dijkstra.GetPath(new Node(4)).Should().BeNull();
        }

        [Fact]
        public void Dijkstra_SingleNodeGraph_ShouldHandleCorrectly()
        {
            // Arrange
            var graph = new CustomGraph();
            var node = graph.AddNode();
            var dijkstra = new Dijkstra(graph, GetEdgeWeight, DijkstraMode.Sum);
            
            // Act
            dijkstra.AddSource(node);
            dijkstra.Run();
            
            // Assert
            dijkstra.Reached(node).Should().BeTrue();
            dijkstra.GetDistance(node).Should().Be(0.0);
            
            var path = dijkstra.GetPath(node);
            path.Should().NotBeNull();
            path!.NodeCount().Should().Be(1);
        }

        [Fact]
        public void Dijkstra_MultipleSourceNodes_ShouldFindShortestFromAny()
        {
            // Arrange
            var graph = CreateMultiSourceTestGraph();
            var dijkstra = new Dijkstra(graph, GetEdgeWeight, DijkstraMode.Sum);
            
            // Act
            dijkstra.AddSource(new Node(1)); // Distance 5 to target
            dijkstra.AddSource(new Node(2)); // Distance 2 to target
            dijkstra.Run();
            
            // Assert
            var target = new Node(3);
            dijkstra.Reached(target).Should().BeTrue();
            dijkstra.GetDistance(target).Should().Be(2.0); // Should use shorter path from node 2
        }

        [Fact]
        public void Dijkstra_ZeroWeightEdges_ShouldHandleCorrectly()
        {
            // Arrange
            var graph = CreateZeroWeightGraph();
            var dijkstra = new Dijkstra(graph, GetZeroEdgeWeight, DijkstraMode.Sum);
            
            // Act
            dijkstra.AddSource(new Node(1));
            dijkstra.Run();
            
            // Assert
            dijkstra.GetDistance(new Node(3)).Should().Be(0.0);
        }

        [Fact]
        public void Dijkstra_MaximumMode_ShouldUseMaximumCost()
        {
            // Arrange
            var graph = CreateSimpleTestGraph();
            var dijkstra = new Dijkstra(graph, GetEdgeWeight, DijkstraMode.Maximum);
            
            // Act
            dijkstra.AddSource(new Node(1));
            dijkstra.Run();
            
            // Assert
            // In maximum mode, path cost is the maximum edge weight in the path
            dijkstra.GetDistance(new Node(3)).Should().Be(2.0); // Max of (2, 1) = 2
        }

        [Fact]
        public void Dijkstra_RunUntilFixed_ShouldStopWhenTargetReached()
        {
            // Arrange
            var graph = CreateLargeTestGraph();
            var dijkstra = new Dijkstra(graph, GetEdgeWeight, DijkstraMode.Sum);
            var target = new Node(5);
            
            // Act
            dijkstra.AddSource(new Node(1));
            dijkstra.RunUntilFixed(target);
            
            // Assert
            dijkstra.Fixed(target).Should().BeTrue();
            dijkstra.Reached(target).Should().BeTrue();
        }

        [Fact]
        public void Dijkstra_Step_ShouldAllowIncrementalExecution()
        {
            // Arrange
            var graph = CreateSimpleTestGraph();
            var dijkstra = new Dijkstra(graph, GetEdgeWeight, DijkstraMode.Sum);
            dijkstra.AddSource(new Node(1));
            
            // Act & Assert
            var stepResult = dijkstra.Step();
            stepResult.Should().NotBe(Node.Invalid); // Should have fixed a node
            
            // Continue stepping until completion
            Node fixedNode;
            do
            {
                fixedNode = dijkstra.Step();
            } while (fixedNode != Node.Invalid);
            
            dijkstra.Reached(new Node(3)).Should().BeTrue();
        }

        [Fact]
        public void Dijkstra_GetParentArc_ShouldReturnCorrectPath()
        {
            // Arrange
            var graph = CreateSimpleTestGraph();
            var dijkstra = new Dijkstra(graph, GetEdgeWeight, DijkstraMode.Sum);
            
            // Act
            dijkstra.AddSource(new Node(1));
            dijkstra.Run();
            
            // Assert
            var target = new Node(3);
            var parentArc = dijkstra.GetParentArc(target);
            parentArc.Should().NotBe(Arc.Invalid);
            
            // Verify the parent arc connects to the correct previous node
            var previousNode = graph.Other(parentArc, target);
            previousNode.Should().Be(new Node(2));
        }

        [Fact]
        public void Dijkstra_LargeGraph_ShouldPerformReasonably()
        {
            // Arrange
            var graph = CreateLargeTestGraph(1000); // 1000 nodes
            var dijkstra = new Dijkstra(graph, GetEdgeWeight, DijkstraMode.Sum);
            
            // Act
            var startTime = DateTime.UtcNow;
            dijkstra.AddSource(new Node(1));
            dijkstra.Run();
            var duration = DateTime.UtcNow - startTime;
            
            // Assert
            duration.Should().BeLessThan(TimeSpan.FromSeconds(1)); // Should complete within 1 second
            dijkstra.Reached(new Node(100)).Should().BeTrue(); // Should reach some nodes
        }

        #region Test Graph Creation Helpers

        private static CustomGraph CreateSimpleTestGraph()
        {
            var graph = new CustomGraph();
            var node1 = graph.AddNode(); // Node(1)
            var node2 = graph.AddNode(); // Node(2)  
            var node3 = graph.AddNode(); // Node(3)
            
            // Create path: 1 --(2)--> 2 --(1)--> 3
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
            
            // Connect 1-2-3, leave 4 disconnected
            graph.AddArc(node1, node2, Directedness.Directed);
            graph.AddArc(node2, node3, Directedness.Directed);
            
            return graph;
        }

        private static CustomGraph CreateMultiSourceTestGraph()
        {
            var graph = new CustomGraph();
            var node1 = graph.AddNode(); // Node(1)
            var node2 = graph.AddNode(); // Node(2)
            var node3 = graph.AddNode(); // Node(3)
            
            // Create two paths to node 3:
            // 1 --(5)--> 3 (longer)
            // 2 --(2)--> 3 (shorter)
            graph.AddArc(node1, node3, Directedness.Directed);
            graph.AddArc(node2, node3, Directedness.Directed);
            
            return graph;
        }

        private static CustomGraph CreateZeroWeightGraph()
        {
            var graph = new CustomGraph();
            var node1 = graph.AddNode(); // Node(1)
            var node2 = graph.AddNode(); // Node(2)
            var node3 = graph.AddNode(); // Node(3)
            
            graph.AddArc(node1, node2, Directedness.Directed);
            graph.AddArc(node2, node3, Directedness.Directed);
            
            return graph;
        }

        private static CustomGraph CreateLargeTestGraph(int nodeCount = 100)
        {
            var graph = new CustomGraph();
            var nodes = new List<Node>();
            
            // Create nodes
            for (int i = 0; i < nodeCount; i++)
            {
                nodes.Add(graph.AddNode());
            }
            
            // Create a connected graph with random edges
            var random = new Random(42); // Fixed seed for reproducible tests
            for (int i = 0; i < nodeCount - 1; i++)
            {
                // Ensure connectivity by creating a path
                graph.AddArc(nodes[i], nodes[i + 1], Directedness.Directed);
                
                // Add some random additional edges
                if (random.NextDouble() < 0.3) // 30% chance of additional edge
                {
                    var targetIndex = random.Next(nodeCount);
                    if (targetIndex != i)
                    {
                        graph.AddArc(nodes[i], nodes[targetIndex], Directedness.Directed);
                    }
                }
            }
            
            return graph;
        }

        #endregion

        #region Weight Functions

        private static double GetEdgeWeight(Arc arc)
        {
            // Simple weight function for testing
            // Arc with id 1 has weight 2, arc with id 2 has weight 1, etc.
            return arc.Id switch
            {
                1 => 2.0,
                2 => 1.0,
                3 => 5.0, // For multi-source test
                4 => 2.0, // For multi-source test
                _ => 1.0  // Default weight
            };
        }

        private static double GetZeroEdgeWeight(Arc arc)
        {
            return 0.0; // All edges have zero weight
        }

        #endregion
    }
}