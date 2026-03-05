using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Plate.ModernSatsuma;
using Xunit;

namespace Plate.ModernSatsuma.Test
{
    public class GraphCompositionTests
    {
        #region SplitIntoComponents Tests

        [Fact]
        public void SplitIntoComponents_SingleComponent_ReturnsSingleSubgraph()
        {
            var graph = new CustomGraph();
            var n1 = graph.AddNode();
            var n2 = graph.AddNode();
            var n3 = graph.AddNode();
            graph.AddArc(n1, n2, Directedness.Undirected);
            graph.AddArc(n2, n3, Directedness.Undirected);

            var components = graph.SplitIntoComponents().ToList();

            components.Should().HaveCount(1);
            components[0].NodeCount().Should().Be(3);
            components[0].ArcCount().Should().Be(2);
        }

        [Fact]
        public void SplitIntoComponents_TwoDisconnectedComponents_ReturnsTwoSubgraphs()
        {
            var graph = new CustomGraph();
            var a = graph.AddNode();
            var b = graph.AddNode();
            var c = graph.AddNode();
            var d = graph.AddNode();
            graph.AddArc(a, b, Directedness.Undirected);
            graph.AddArc(c, d, Directedness.Undirected);

            var components = graph.SplitIntoComponents().ToList();

            components.Should().HaveCount(2);
            components.Sum(c => c.NodeCount()).Should().Be(4);
            components.Sum(c => c.ArcCount()).Should().Be(2);
        }

        [Fact]
        public void SplitIntoComponents_IsolatedNodes_ReturnsOneSubgraphPerNode()
        {
            var graph = new CustomGraph();
            graph.AddNode();
            graph.AddNode();
            graph.AddNode();

            var components = graph.SplitIntoComponents().ToList();

            components.Should().HaveCount(3);
            components.All(c => c.NodeCount() == 1).Should().BeTrue();
        }

        [Fact]
        public void IsConnected_ConnectedSubset_ReturnsTrue()
        {
            var graph = new CustomGraph();
            var a = graph.AddNode();
            var b = graph.AddNode();
            var c = graph.AddNode();
            graph.AddArc(a, b, Directedness.Undirected);
            graph.AddArc(b, c, Directedness.Undirected);

            graph.IsConnected(new[] { a, b, c }).Should().BeTrue();
            graph.IsConnected(new[] { a, b }).Should().BeTrue();
        }

        [Fact]
        public void IsConnected_DisconnectedSubset_ReturnsFalse()
        {
            var graph = new CustomGraph();
            var a = graph.AddNode();
            var b = graph.AddNode();
            var c = graph.AddNode();
            graph.AddArc(a, b, Directedness.Undirected);
            // c is isolated

            graph.IsConnected(new[] { a, c }).Should().BeFalse();
        }

        #endregion

        #region Subgraph Convenience Methods Tests

        [Fact]
        public void InducedByNodes_ReturnsSubgraphWithOnlySpecifiedNodes()
        {
            var graph = new CustomGraph();
            var a = graph.AddNode();
            var b = graph.AddNode();
            var c = graph.AddNode();
            graph.AddArc(a, b, Directedness.Undirected);
            graph.AddArc(b, c, Directedness.Undirected);
            graph.AddArc(a, c, Directedness.Undirected);

            var subgraph = Subgraph.InducedByNodes(graph, new[] { a, b });

            subgraph.NodeCount().Should().Be(2);
            subgraph.HasNode(a).Should().BeTrue();
            subgraph.HasNode(b).Should().BeTrue();
            subgraph.HasNode(c).Should().BeFalse();
            subgraph.ArcCount().Should().Be(1); // Only the a-b arc
        }

        [Fact]
        public void InducedByArcs_ReturnsSubgraphWithArcsAndEndpoints()
        {
            var graph = new CustomGraph();
            var a = graph.AddNode();
            var b = graph.AddNode();
            var c = graph.AddNode();
            var arc1 = graph.AddArc(a, b, Directedness.Undirected);
            var arc2 = graph.AddArc(b, c, Directedness.Undirected);

            var subgraph = Subgraph.InducedByArcs(graph, new[] { arc1 });

            subgraph.NodeCount().Should().Be(2);
            subgraph.HasNode(a).Should().BeTrue();
            subgraph.HasNode(b).Should().BeTrue();
            subgraph.HasNode(c).Should().BeFalse();
            subgraph.ArcCount().Should().Be(1);
            subgraph.HasArc(arc1).Should().BeTrue();
            subgraph.HasArc(arc2).Should().BeFalse();
        }

        [Fact]
        public void Filter_WithNodePredicate_FiltersNodes()
        {
            var graph = new CustomGraph();
            var n1 = graph.AddNode();
            var n2 = graph.AddNode();
            var n3 = graph.AddNode();
            graph.AddArc(n1, n2, Directedness.Undirected);
            graph.AddArc(n2, n3, Directedness.Undirected);

            var subgraph = Subgraph.Filter(graph, nodeFilter: n => n != n3);

            subgraph.NodeCount().Should().Be(2);
            subgraph.HasNode(n1).Should().BeTrue();
            subgraph.HasNode(n2).Should().BeTrue();
            subgraph.HasNode(n3).Should().BeFalse();
        }

        [Fact]
        public void Filter_WithArcPredicate_FiltersArcs()
        {
            var graph = new CustomGraph();
            var a = graph.AddNode();
            var b = graph.AddNode();
            var arc1 = graph.AddArc(a, b, Directedness.Directed);
            var arc2 = graph.AddArc(a, b, Directedness.Undirected);

            var subgraph = Subgraph.Filter(graph, arcFilter: arc => graph.IsEdge(arc));

            subgraph.HasArc(arc1).Should().BeFalse();
            subgraph.HasArc(arc2).Should().BeTrue();
        }

        #endregion

        #region UnionGraph Tests

        [Fact]
        public void UnionGraph_CombinesTwoGraphs_NodeCountIsSum()
        {
            var g1 = new CustomGraph();
            g1.AddNode();
            g1.AddNode();

            var g2 = new CustomGraph();
            g2.AddNode();
            g2.AddNode();
            g2.AddNode();

            var union = new UnionGraph(g1, g2);

            union.NodeCount().Should().Be(5);
            union.GraphCount.Should().Be(2);
        }

        [Fact]
        public void UnionGraph_CombinesTwoGraphs_ArcCountIsSum()
        {
            var g1 = new CustomGraph();
            var a1 = g1.AddNode();
            var b1 = g1.AddNode();
            g1.AddArc(a1, b1, Directedness.Directed);

            var g2 = new CustomGraph();
            var a2 = g2.AddNode();
            var b2 = g2.AddNode();
            g2.AddArc(a2, b2, Directedness.Undirected);
            g2.AddArc(b2, a2, Directedness.Directed);

            var union = new UnionGraph(g1, g2);

            union.ArcCount().Should().Be(3);
        }

        [Fact]
        public void UnionGraph_WrapUnwrap_PreservesIdentity()
        {
            var g1 = new CustomGraph();
            var originalNode = g1.AddNode();

            var union = new UnionGraph(g1);
            var wrappedNode = union.WrapNode(0, originalNode);
            var unwrapped = union.UnwrapNode(wrappedNode);

            unwrapped.GraphIndex.Should().Be(0);
            unwrapped.OriginalNode.Should().Be(originalNode);
        }

        [Fact]
        public void UnionGraph_NodesFromDifferentGraphs_HaveDistinctIds()
        {
            var g1 = new CustomGraph();
            var n1 = g1.AddNode();

            var g2 = new CustomGraph();
            var n2 = g2.AddNode();

            var union = new UnionGraph(g1, g2);
            var wrapped1 = union.WrapNode(0, n1);
            var wrapped2 = union.WrapNode(1, n2);

            // Even if original IDs are the same, wrapped IDs should differ
            wrapped1.Should().NotBe(wrapped2);
        }

        [Fact]
        public void UnionGraph_ArcEndpoints_AreCorrectlyMapped()
        {
            var g1 = new CustomGraph();
            var a = g1.AddNode();
            var b = g1.AddNode();
            var arc = g1.AddArc(a, b, Directedness.Directed);

            var union = new UnionGraph(g1);
            var wrappedArc = union.WrapArc(0, arc);

            var u = union.U(wrappedArc);
            var v = union.V(wrappedArc);

            union.UnwrapNode(u).OriginalNode.Should().Be(a);
            union.UnwrapNode(v).OriginalNode.Should().Be(b);
        }

        [Fact]
        public void UnionGraph_ArcsFromDifferentGraphs_NotConnected()
        {
            var g1 = new CustomGraph();
            var a = g1.AddNode();

            var g2 = new CustomGraph();
            var b = g2.AddNode();

            var union = new UnionGraph(g1, g2);
            var wrappedA = union.WrapNode(0, a);
            var wrappedB = union.WrapNode(1, b);

            // No arcs between nodes from different graphs in a plain union
            union.ArcCount(wrappedA, wrappedB).Should().Be(0);
        }

        [Fact]
        public void UnionGraph_ToCustomGraph_MaterializesCorrectly()
        {
            var g1 = new CustomGraph();
            var a = g1.AddNode();
            var b = g1.AddNode();
            g1.AddArc(a, b, Directedness.Directed);

            var g2 = new CustomGraph();
            var c = g2.AddNode();

            var union = new UnionGraph(g1, g2);
            var materialized = union.ToCustomGraph();

            materialized.NodeCount().Should().Be(3);
            materialized.ArcCount().Should().Be(1);
        }

        [Fact]
        public void UnionGraph_IterateNodes_ReturnsAllNodes()
        {
            var g1 = new CustomGraph();
            g1.AddNode();
            g1.AddNode();

            var g2 = new CustomGraph();
            g2.AddNode();

            var union = new UnionGraph(g1, g2);
            var nodes = union.Nodes().ToList();

            nodes.Should().HaveCount(3);
        }

        #endregion

        #region JoinGraph Tests

        [Fact]
        public void JoinGraph_WithoutConnectors_BehavesLikeUnion()
        {
            var g1 = new CustomGraph();
            var a = g1.AddNode();
            var b = g1.AddNode();
            g1.AddArc(a, b, Directedness.Directed);

            var g2 = new CustomGraph();
            var c = g2.AddNode();

            var join = new JoinGraph(g1, g2);

            join.NodeCount().Should().Be(3);
            join.ArcCount().Should().Be(1);
        }

        [Fact]
        public void JoinGraph_AddConnector_CreatesArcBetweenGraphs()
        {
            var g1 = new CustomGraph();
            var a = g1.AddNode();
            var b = g1.AddNode();
            g1.AddArc(a, b, Directedness.Directed);

            var g2 = new CustomGraph();
            var c = g2.AddNode();
            var d = g2.AddNode();
            g2.AddArc(c, d, Directedness.Directed);

            var join = new JoinGraph(g1, g2);

            // Add a connector from b (in g1) to c (in g2)
            join.AddConnector(0, b, 1, c, Directedness.Directed);

            join.ArcCount().Should().Be(3);
            join.Connectors.Should().HaveCount(1);
        }

        [Fact]
        public void JoinGraph_Connector_IsTraversable()
        {
            var g1 = new CustomGraph();
            var a = g1.AddNode();

            var g2 = new CustomGraph();
            var b = g2.AddNode();

            var join = new JoinGraph(g1, g2);
            var wrappedA = join.Union.WrapNode(0, a);
            var wrappedB = join.Union.WrapNode(1, b);

            join.AddConnector(Connector.Create(join.Union, 0, a, 1, b, Directedness.Directed));

            // Check that arcs from wrappedA include the connector
            var arcsFromA = join.Arcs(wrappedA, ArcFilter.Forward).ToList();
            arcsFromA.Should().HaveCount(1);

            // The arc should connect to wrappedB
            var arc = arcsFromA[0];
            join.U(arc).Should().Be(wrappedA);
            join.V(arc).Should().Be(wrappedB);
        }

        [Fact]
        public void JoinGraph_UndirectedConnector_IsTraversableBothWays()
        {
            var g1 = new CustomGraph();
            var a = g1.AddNode();

            var g2 = new CustomGraph();
            var b = g2.AddNode();

            var join = new JoinGraph(g1, g2);
            var wrappedA = join.Union.WrapNode(0, a);
            var wrappedB = join.Union.WrapNode(1, b);

            join.AddConnector(0, a, 1, b, Directedness.Undirected);

            join.Arcs(wrappedA, ArcFilter.All).Should().HaveCount(1);
            join.Arcs(wrappedB, ArcFilter.All).Should().HaveCount(1);
            join.IsEdge(join.Arcs(wrappedA).First()).Should().BeTrue();
        }

        [Fact]
        public void JoinGraph_RemoveConnector_RemovesArc()
        {
            var g1 = new CustomGraph();
            var a = g1.AddNode();

            var g2 = new CustomGraph();
            var b = g2.AddNode();

            var join = new JoinGraph(g1, g2);
            var connectorArc = join.AddConnector(0, a, 1, b);

            join.ArcCount().Should().Be(1);

            join.RemoveConnector(connectorArc).Should().BeTrue();

            join.ArcCount().Should().Be(0);
            join.Connectors.Should().BeEmpty();
        }

        [Fact]
        public void JoinGraph_IsConnectorArc_DistinguishesConnectors()
        {
            var g1 = new CustomGraph();
            var a = g1.AddNode();
            var b = g1.AddNode();
            var originalArc = g1.AddArc(a, b, Directedness.Directed);

            var g2 = new CustomGraph();
            var c = g2.AddNode();

            var join = new JoinGraph(g1, g2);
            var connectorArc = join.AddConnector(0, b, 1, c);
            var wrappedOriginalArc = join.Union.WrapArc(0, originalArc);

            join.IsConnectorArc(connectorArc).Should().BeTrue();
            join.IsConnectorArc(wrappedOriginalArc).Should().BeFalse();
        }

        [Fact]
        public void JoinGraph_ToCustomGraph_IncludesConnectors()
        {
            var g1 = new CustomGraph();
            var a = g1.AddNode();

            var g2 = new CustomGraph();
            var b = g2.AddNode();

            var join = new JoinGraph(g1, g2);
            join.AddConnector(0, a, 1, b);

            var materialized = join.ToCustomGraph();

            materialized.NodeCount().Should().Be(2);
            materialized.ArcCount().Should().Be(1);
        }

        [Fact]
        public void JoinGraph_PathAcrossGraphs_CanBeFound()
        {
            // Create two simple path graphs
            var g1 = new CustomGraph();
            var a = g1.AddNode();
            var b = g1.AddNode();
            g1.AddArc(a, b, Directedness.Directed);

            var g2 = new CustomGraph();
            var c = g2.AddNode();
            var d = g2.AddNode();
            g2.AddArc(c, d, Directedness.Directed);

            var join = new JoinGraph(g1, g2);
            join.AddConnector(0, b, 1, c, Directedness.Directed);

            var wrappedA = join.Union.WrapNode(0, a);
            var wrappedD = join.Union.WrapNode(1, d);

            // Use BFS to find path from a to d
            var bfs = new Bfs(join);
            bfs.AddSource(wrappedA);
            bfs.Run();

            bfs.Reached(wrappedD).Should().BeTrue();
            bfs.GetLevel(wrappedD).Should().Be(3); // a -> b -> c -> d
        }

        #endregion
    }
}
