using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Plate.ModernSatsuma;
using Xunit;

namespace Plate.ModernSatsuma.Test
{
    public class TopologicalSortTests
    {
        [Fact]
        public void LinearChain_ShouldProduceCorrectOrder()
        {
            // A -> B -> C -> D
            var graph = new CustomGraph();
            var a = graph.AddNode();
            var b = graph.AddNode();
            var c = graph.AddNode();
            var d = graph.AddNode();

            graph.AddArc(a, b, Directedness.Directed);
            graph.AddArc(b, c, Directedness.Directed);
            graph.AddArc(c, d, Directedness.Directed);

            var result = new TopologicalSort(graph);

            result.IsAcyclic.Should().BeTrue();
            result.CyclicNodes.Should().BeEmpty();
            result.Order.Should().HaveCount(4);
            result.Order.Should().ContainInOrder(a, b, c, d);
            result.Layers.Should().HaveCount(4);
            result.Layers[0].Should().Equal(a);
            result.Layers[1].Should().Equal(b);
            result.Layers[2].Should().Equal(c);
            result.Layers[3].Should().Equal(d);
        }

        [Fact]
        public void Diamond_ShouldGroupIndependentNodesInSameLayer()
        {
            // A -> B, A -> C, B -> D, C -> D
            var graph = new CustomGraph();
            var a = graph.AddNode();
            var b = graph.AddNode();
            var c = graph.AddNode();
            var d = graph.AddNode();

            graph.AddArc(a, b, Directedness.Directed);
            graph.AddArc(a, c, Directedness.Directed);
            graph.AddArc(b, d, Directedness.Directed);
            graph.AddArc(c, d, Directedness.Directed);

            var result = new TopologicalSort(graph);

            result.IsAcyclic.Should().BeTrue();
            result.CyclicNodes.Should().BeEmpty();
            result.Order.Should().HaveCount(4);
            result.Layers.Should().HaveCount(3);
            result.Layers[0].Should().Equal(a);
            result.Layers[1].Should().HaveCount(2);
            result.Layers[1].Should().Contain(b);
            result.Layers[1].Should().Contain(c);
            result.Layers[2].Should().Equal(d);

            // Verify topological property: for every arc u->v, u appears before v
            var indexOf = new Dictionary<long, int>();
            for (int i = 0; i < result.Order.Count; i++)
                indexOf[result.Order[i].Id] = i;

            indexOf[a.Id].Should().BeLessThan(indexOf[b.Id]);
            indexOf[a.Id].Should().BeLessThan(indexOf[c.Id]);
            indexOf[b.Id].Should().BeLessThan(indexOf[d.Id]);
            indexOf[c.Id].Should().BeLessThan(indexOf[d.Id]);
        }

        [Fact]
        public void ParallelChains_ShouldGroupRootsAndLeavesTogether()
        {
            // A -> B, C -> D (no cross-dependencies)
            var graph = new CustomGraph();
            var a = graph.AddNode();
            var b = graph.AddNode();
            var c = graph.AddNode();
            var d = graph.AddNode();

            graph.AddArc(a, b, Directedness.Directed);
            graph.AddArc(c, d, Directedness.Directed);

            var result = new TopologicalSort(graph);

            result.IsAcyclic.Should().BeTrue();
            result.CyclicNodes.Should().BeEmpty();
            result.Order.Should().HaveCount(4);
            result.Layers.Should().HaveCount(2);
            result.Layers[0].Should().HaveCount(2);
            result.Layers[0].Should().Contain(a);
            result.Layers[0].Should().Contain(c);
            result.Layers[1].Should().HaveCount(2);
            result.Layers[1].Should().Contain(b);
            result.Layers[1].Should().Contain(d);
        }

        [Fact]
        public void SingleNode_ShouldReturnOneLayerWithOneNode()
        {
            var graph = new CustomGraph();
            var a = graph.AddNode();

            var result = new TopologicalSort(graph);

            result.IsAcyclic.Should().BeTrue();
            result.CyclicNodes.Should().BeEmpty();
            result.Order.Should().Equal(a);
            result.Layers.Should().HaveCount(1);
            result.Layers[0].Should().Equal(a);
        }

        [Fact]
        public void EmptyGraph_ShouldReturnEmptyResults()
        {
            var graph = new CustomGraph();

            var result = new TopologicalSort(graph);

            result.IsAcyclic.Should().BeTrue();
            result.CyclicNodes.Should().BeEmpty();
            result.Order.Should().BeEmpty();
            result.Layers.Should().BeEmpty();
        }

        [Fact]
        public void SimpleCycle_ShouldDetectAllNodesAsCyclic()
        {
            // A -> B -> C -> A
            var graph = new CustomGraph();
            var a = graph.AddNode();
            var b = graph.AddNode();
            var c = graph.AddNode();

            graph.AddArc(a, b, Directedness.Directed);
            graph.AddArc(b, c, Directedness.Directed);
            graph.AddArc(c, a, Directedness.Directed);

            var result = new TopologicalSort(graph);

            result.IsAcyclic.Should().BeFalse();
            result.CyclicNodes.Should().HaveCount(3);
            result.CyclicNodes.Should().Contain(a);
            result.CyclicNodes.Should().Contain(b);
            result.CyclicNodes.Should().Contain(c);
            result.Order.Should().BeEmpty();
        }

        [Fact]
        public void PartialCycle_ShouldReportCyclicAndNonCyclicNodes()
        {
            // A -> B -> C -> B (cycle on B,C), A -> D (D reachable)
            var graph = new CustomGraph();
            var a = graph.AddNode();
            var b = graph.AddNode();
            var c = graph.AddNode();
            var d = graph.AddNode();

            graph.AddArc(a, b, Directedness.Directed);
            graph.AddArc(b, c, Directedness.Directed);
            graph.AddArc(c, b, Directedness.Directed);
            graph.AddArc(a, d, Directedness.Directed);

            var result = new TopologicalSort(graph);

            result.IsAcyclic.Should().BeFalse();
            result.CyclicNodes.Should().HaveCount(2);
            result.CyclicNodes.Should().Contain(b);
            result.CyclicNodes.Should().Contain(c);

            // Non-cyclic nodes A and D should still be in the order
            result.Order.Should().Contain(a);
            result.Order.Should().Contain(d);
            result.Order.Should().NotContain(b);
            result.Order.Should().NotContain(c);
        }

        [Fact]
        public void LargeFanOut_ShouldProduceTwoLayers()
        {
            // A -> B1, A -> B2, ..., A -> B50
            var graph = new CustomGraph();
            var root = graph.AddNode();
            var children = new List<Node>();

            for (int i = 0; i < 50; i++)
            {
                var child = graph.AddNode();
                children.Add(child);
                graph.AddArc(root, child, Directedness.Directed);
            }

            var result = new TopologicalSort(graph);

            result.IsAcyclic.Should().BeTrue();
            result.Order.Should().HaveCount(51);
            result.Layers.Should().HaveCount(2);
            result.Layers[0].Should().Equal(root);
            result.Layers[1].Should().HaveCount(50);
            foreach (var child in children)
                result.Layers[1].Should().Contain(child);
        }

        [Fact]
        public void DeepChain_ShouldNotStackOverflow()
        {
            // 1000-node linear chain: n0 -> n1 -> ... -> n999
            var graph = new CustomGraph();
            var nodes = new List<Node>();

            for (int i = 0; i < 1000; i++)
                nodes.Add(graph.AddNode());

            for (int i = 0; i < 999; i++)
                graph.AddArc(nodes[i], nodes[i + 1], Directedness.Directed);

            var result = new TopologicalSort(graph);

            result.IsAcyclic.Should().BeTrue();
            result.Order.Should().HaveCount(1000);
            result.Layers.Should().HaveCount(1000);

            // Verify ordering matches the chain
            for (int i = 0; i < 1000; i++)
                result.Order[i].Should().Be(nodes[i]);
        }

        [Fact]
        public void Builder_ShouldProduceSameResultAsDirectConstruction()
        {
            var graph = new CustomGraph();
            var a = graph.AddNode();
            var b = graph.AddNode();
            var c = graph.AddNode();

            graph.AddArc(a, b, Directedness.Directed);
            graph.AddArc(b, c, Directedness.Directed);

            var direct = new TopologicalSort(graph);
            var viaBuilder = graph.TopologicalSort().Build();

            viaBuilder.IsAcyclic.Should().Be(direct.IsAcyclic);
            viaBuilder.Order.Should().Equal(direct.Order);
            viaBuilder.Layers.Should().HaveCount(direct.Layers.Count);
            for (int i = 0; i < direct.Layers.Count; i++)
                viaBuilder.Layers[i].Should().Equal(direct.Layers[i]);
        }
    }
}
