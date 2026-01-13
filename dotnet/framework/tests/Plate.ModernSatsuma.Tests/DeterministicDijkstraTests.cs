using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace Plate.ModernSatsuma.Test;

/// <summary>
/// Tests for DeterministicDijkstra with emphasis on reproducible tie-breaking.
/// </summary>
public class DeterministicDijkstraTests
{
    [Fact]
    public void DeterministicDijkstra_BasicShortestPath_ShouldFindOptimalRoute()
    {
        // Arrange: Simple linear graph 1 -> 2 -> 3
        var graph = new CustomGraph();
        var n1 = graph.AddNode();
        var n2 = graph.AddNode();
        var n3 = graph.AddNode();
        graph.AddArc(n1, n2, Directedness.Directed);
        graph.AddArc(n2, n3, Directedness.Directed);
        
        var costs = new Dictionary<Arc, double>();
        foreach (var arc in graph.Arcs())
        {
            costs[arc] = 1.0; // All edges cost 1
        }
        
        var dijkstra = new DeterministicDijkstra(graph, arc => costs[arc]);
        
        // Act
        dijkstra.AddSource(n1);
        dijkstra.Run();
        
        // Assert
        dijkstra.Reached(n3).Should().BeTrue();
        dijkstra.GetDistance(n3).Should().Be(2.0);
    }

    [Fact]
    public void DeterministicDijkstra_TieBreaking_ShouldSelectLowerNodeIdFirst()
    {
        // Arrange: Graph where multiple paths have same cost but different node IDs
        //
        //        ┌─ N2 ─┐
        //   N1 ──┤      ├── N4
        //        └─ N3 ─┘
        //
        // All edges have cost 1, so N1 -> N2 -> N4 and N1 -> N3 -> N4 both cost 2.
        // N2.Id < N3.Id, so N2 should be fixed before N3.
        
        var graph = new CustomGraph();
        var n1 = graph.AddNode(); // Id = 1
        var n2 = graph.AddNode(); // Id = 2
        var n3 = graph.AddNode(); // Id = 3
        var n4 = graph.AddNode(); // Id = 4
        
        graph.AddArc(n1, n2, Directedness.Directed); // N1 -> N2, cost 1
        graph.AddArc(n1, n3, Directedness.Directed); // N1 -> N3, cost 1
        graph.AddArc(n2, n4, Directedness.Directed); // N2 -> N4, cost 1
        graph.AddArc(n3, n4, Directedness.Directed); // N3 -> N4, cost 1
        
        var dijkstra = new DeterministicDijkstra(graph, _ => 1.0);
        
        // Act
        dijkstra.AddSource(n1);
        
        var fixOrder = new List<Node>();
        Node fixedNode;
        while ((fixedNode = dijkstra.Step()) != Node.Invalid)
        {
            fixOrder.Add(fixedNode);
        }
        
        // Assert
        fixOrder.Should().HaveCount(4);
        fixOrder[0].Should().Be(n1); // Source first
        fixOrder[1].Should().Be(n2); // N2.Id (2) < N3.Id (3), same cost
        fixOrder[2].Should().Be(n3); // N3 next
        fixOrder[3].Should().Be(n4); // N4 last
    }

    [Fact]
    public void DeterministicDijkstra_MultipleRuns_ShouldProduceSameResults()
    {
        // Arrange: Same graph, run multiple times
        var graph = CreateDiamondGraph();
        
        // Act: Run 10 times and collect paths to final node
        var paths = new List<List<long>>();
        for (int i = 0; i < 10; i++)
        {
            var dijkstra = new DeterministicDijkstra(graph, _ => 1.0);
            dijkstra.AddSource(new Node(1));
            dijkstra.Run();
            
            var path = dijkstra.GetPath(new Node(4));
            path.Should().NotBeNull();
            
            var nodeIds = new List<long>();
            foreach (var node in path!.Nodes())
            {
                nodeIds.Add(node.Id);
            }
            paths.Add(nodeIds);
        }
        
        // Assert: All paths should be identical
        for (int i = 1; i < paths.Count; i++)
        {
            paths[i].Should().BeEquivalentTo(paths[0], options => options.WithStrictOrdering());
        }
    }

    [Fact]
    public void DeterministicDijkstra_WithVaryingCosts_ShouldSelectCheapestPath()
    {
        // Arrange: Diamond graph with different costs
        //
        //        ┌─ N2 (cost 1) ─┐
        //   N1 ──┤               ├── N4
        //        └─ N3 (cost 5) ─┘
        //
        var graph = new CustomGraph();
        var n1 = graph.AddNode();
        var n2 = graph.AddNode();
        var n3 = graph.AddNode();
        var n4 = graph.AddNode();
        
        var a12 = graph.AddArc(n1, n2, Directedness.Directed);
        var a13 = graph.AddArc(n1, n3, Directedness.Directed);
        var a24 = graph.AddArc(n2, n4, Directedness.Directed);
        var a34 = graph.AddArc(n3, n4, Directedness.Directed);
        
        var costs = new Dictionary<Arc, double>
        {
            [a12] = 1.0, // Cheap
            [a13] = 5.0, // Expensive
            [a24] = 1.0,
            [a34] = 1.0
        };
        
        var dijkstra = new DeterministicDijkstra(graph, arc => costs[arc]);
        
        // Act
        dijkstra.AddSource(n1);
        dijkstra.Run();
        
        // Assert
        dijkstra.GetDistance(n4).Should().Be(2.0); // Via N2: 1 + 1 = 2, not via N3: 5 + 1 = 6
        
        var path = dijkstra.GetPath(n4);
        path.Should().NotBeNull();
        
        var nodeIds = path!.Nodes().Select(n => n.Id).ToList();
        nodeIds.Should().BeEquivalentTo(new long[] { 1, 2, 4 }, options => options.WithStrictOrdering());
    }

    [Fact]
    public void DeterministicDijkstra_ImpassableArc_ShouldNotTraverse()
    {
        // Arrange
        var graph = new CustomGraph();
        var n1 = graph.AddNode();
        var n2 = graph.AddNode();
        var n3 = graph.AddNode();
        
        var a12 = graph.AddArc(n1, n2, Directedness.Directed);
        var a23 = graph.AddArc(n2, n3, Directedness.Directed);
        
        var costs = new Dictionary<Arc, double>
        {
            [a12] = double.PositiveInfinity, // Impassable
            [a23] = 1.0
        };
        
        var dijkstra = new DeterministicDijkstra(graph, arc => costs[arc]);
        
        // Act
        dijkstra.AddSource(n1);
        dijkstra.Run();
        
        // Assert
        dijkstra.Fixed(n2).Should().BeFalse();
        dijkstra.Fixed(n3).Should().BeFalse();
        dijkstra.GetDistance(n2).Should().Be(double.PositiveInfinity);
    }

    [Fact]
    public void DeterministicDijkstra_MaximumMode_ShouldUseMaxCostAlongPath()
    {
        // Arrange: Path with varying costs, maximize instead of sum
        var graph = new CustomGraph();
        var n1 = graph.AddNode();
        var n2 = graph.AddNode();
        var n3 = graph.AddNode();
        
        var a12 = graph.AddArc(n1, n2, Directedness.Directed);
        var a23 = graph.AddArc(n2, n3, Directedness.Directed);
        
        var costs = new Dictionary<Arc, double>
        {
            [a12] = 3.0,
            [a23] = 2.0
        };
        
        var dijkstra = new DeterministicDijkstra(graph, arc => costs[arc], DijkstraMode.Maximum);
        
        // Act
        dijkstra.AddSource(n1);
        dijkstra.Run();
        
        // Assert: Maximum mode takes max(3, 2) = 3, not sum(3+2) = 5
        dijkstra.GetDistance(n3).Should().Be(3.0);
    }

    [Fact]
    public void DeterministicDijkstra_RunUntilFixed_ShouldStopEarly()
    {
        // Arrange: Larger graph, but we only need path to middle node
        var graph = new CustomGraph();
        var nodes = new Node[10];
        for (int i = 0; i < 10; i++)
        {
            nodes[i] = graph.AddNode();
        }
        
        // Linear chain: 0 -> 1 -> 2 -> ... -> 9
        for (int i = 0; i < 9; i++)
        {
            graph.AddArc(nodes[i], nodes[i + 1], Directedness.Directed);
        }
        
        var dijkstra = new DeterministicDijkstra(graph, _ => 1.0);
        dijkstra.AddSource(nodes[0]);
        
        // Act
        var result = dijkstra.RunUntilFixed(nodes[5]);
        
        // Assert
        result.Should().Be(nodes[5]);
        dijkstra.GetDistance(nodes[5]).Should().Be(5.0);
        
        // Nodes beyond target should not be fixed
        dijkstra.Fixed(nodes[6]).Should().BeFalse();
    }

    [Fact]
    public void DeterministicDijkstra_Clear_ShouldResetState()
    {
        // Arrange
        var graph = CreateDiamondGraph();
        var dijkstra = new DeterministicDijkstra(graph, _ => 1.0);
        
        dijkstra.AddSource(new Node(1));
        dijkstra.Run();
        dijkstra.Fixed(new Node(4)).Should().BeTrue();
        
        // Act
        dijkstra.Clear();
        
        // Assert
        dijkstra.Fixed(new Node(4)).Should().BeFalse();
        dijkstra.Reached(new Node(4)).Should().BeFalse();
    }

    [Fact]
    public void DeterministicDijkstra_LargeGraph_ShouldRemainDeterministic()
    {
        // Arrange: Larger graph with many equal-cost paths
        var graph = new CustomGraph();
        var size = 100;
        var nodes = new Node[size];
        
        for (int i = 0; i < size; i++)
        {
            nodes[i] = graph.AddNode();
        }
        
        // Create grid-like connections (each node connects to next 5)
        for (int i = 0; i < size; i++)
        {
            for (int j = 1; j <= 5 && i + j < size; j++)
            {
                graph.AddArc(nodes[i], nodes[i + j], Directedness.Directed);
            }
        }
        
        // Act: Run twice
        var paths1 = RunAndCollectFixOrder(graph, nodes[0]);
        var paths2 = RunAndCollectFixOrder(graph, nodes[0]);
        
        // Assert: Same fix order
        paths1.Should().BeEquivalentTo(paths2, options => options.WithStrictOrdering());
    }

    #region Helper Methods

    private static CustomGraph CreateDiamondGraph()
    {
        var graph = new CustomGraph();
        var n1 = graph.AddNode(); // Id = 1
        var n2 = graph.AddNode(); // Id = 2
        var n3 = graph.AddNode(); // Id = 3
        var n4 = graph.AddNode(); // Id = 4
        
        graph.AddArc(n1, n2, Directedness.Directed);
        graph.AddArc(n1, n3, Directedness.Directed);
        graph.AddArc(n2, n4, Directedness.Directed);
        graph.AddArc(n3, n4, Directedness.Directed);
        
        return graph;
    }

    private static List<long> RunAndCollectFixOrder(IGraph graph, Node source)
    {
        var dijkstra = new DeterministicDijkstra(graph, _ => 1.0);
        dijkstra.AddSource(source);
        
        var fixOrder = new List<long>();
        Node fixedNode;
        while ((fixedNode = dijkstra.Step()) != Node.Invalid)
        {
            fixOrder.Add(fixedNode.Id);
        }
        
        return fixOrder;
    }

    #endregion
}
