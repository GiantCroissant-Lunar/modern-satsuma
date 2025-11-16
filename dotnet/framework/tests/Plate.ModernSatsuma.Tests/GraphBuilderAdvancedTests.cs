using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Plate.ModernSatsuma;
using Plate.ModernSatsuma.Generators;
using Xunit;

namespace Plate.ModernSatsuma.Test;

[GraphBuilder]
public partial class UndirectedEdgeBuilder
{
    [Node]
    public Node A { get; private set; }

    [Node]
    public Node B { get; private set; }

    [Arc(Directedness = Directedness.Undirected)]
    public void A_to_B(Node from, Node to)
    {
    }
}

[GraphBuilder(GraphType = typeof(TestBuildableGraph))]
public partial class CustomGraphTypeBuilder
{
    [Node]
    public Node A { get; private set; }

    [Node]
    public Node B { get; private set; }

    [Arc]
    public void A_to_B(Node from, Node to)
    {
    }
}

[GraphBuilder(DefaultDirectedness = Directedness.Undirected)]
public partial class DefaultUndirectedBuilder
{
    [Node]
    public Node A { get; private set; }

    [Node]
    public Node B { get; private set; }

    [Arc]
    public void A_to_B(Node from, Node to)
    {
    }
}

[GraphBuilder]
public partial class LogicalNameNodeBuilder
{
    [Node(Name = "Start")]
    public Node S { get; private set; }

    [Node(Name = "End")]
    public Node E { get; private set; }

    [Arc]
    public void Start_to_End(Node from, Node to)
    {
    }
}

[GraphBuilder]
public partial class WeightedGraphBuilder
{
    [Node]
    public Node A { get; private set; }

    [Node]
    public Node B { get; private set; }

    [Arc(Cost = 2.0, WeightName = "distance")]
    public void A_to_B(Node from, Node to)
    {
    }
}

[GraphBuilder]
public partial class TaggedGraphBuilder
{
    [Node]
    public Node A { get; private set; }

    [Node]
    public Node B { get; private set; }

    [Arc(Tag = "road")]
    public void A_to_B(Node from, Node to)
    {
    }
}

// Intentionally misconfigured builder to exercise MSGB010 (name not in From_to_To format).
[GraphBuilder]
public partial class BadArcNameBuilder
{
    [Node]
    public Node One { get; private set; }

    [Node]
    public Node Two { get; private set; }

    [Arc]
    public void Connect(Node from, Node to)
    {
    }
}

// Intentionally misconfigured builder to exercise MSGB011 (unknown node name in arc).
[GraphBuilder]
public partial class BadArcUnknownNodeBuilder
{
    [Node]
    public Node One { get; private set; }

    [Arc]
    public void One_to_Two(Node from, Node to)
    {
    }
}

public sealed class TestBuildableGraph : IBuildableGraph, IGraph
{
    private readonly CustomGraph _inner = new();

    public Node AddNode() => _inner.AddNode();

    public Arc AddArc(Node u, Node v, Directedness directedness) => _inner.AddArc(u, v, directedness);

    public void Clear() => _inner.Clear();

    public Node U(Arc arc) => _inner.U(arc);

    public Node V(Arc arc) => _inner.V(arc);

    public bool IsEdge(Arc arc) => _inner.IsEdge(arc);

    public IEnumerable<Node> Nodes() => _inner.Nodes();

    public IEnumerable<Arc> Arcs(ArcFilter filter = ArcFilter.All) => _inner.Arcs(filter);

    public IEnumerable<Arc> Arcs(Node u, ArcFilter filter = ArcFilter.All) => _inner.Arcs(u, filter);

    public IEnumerable<Arc> Arcs(Node u, Node v, ArcFilter filter = ArcFilter.All) => _inner.Arcs(u, v, filter);

    public int NodeCount() => _inner.NodeCount();

    public int ArcCount(ArcFilter filter = ArcFilter.All) => _inner.ArcCount(filter);

    public int ArcCount(Node u, ArcFilter filter = ArcFilter.All) => _inner.ArcCount(u, filter);

    public int ArcCount(Node u, Node v, ArcFilter filter = ArcFilter.All) => _inner.ArcCount(u, v, filter);

    public bool HasNode(Node node) => _inner.HasNode(node);

    public bool HasArc(Arc arc) => _inner.HasArc(arc);
}

public class GraphBuilderAdvancedTests
{
    [Fact]
    public void UndirectedArc_ShouldProduceEdge()
    {
        var builder = new UndirectedEdgeBuilder();
        var graph = builder.BuildGraph();

        var arc = graph.Arcs(ArcFilter.Edge).Single();

        graph.IsEdge(arc).Should().BeTrue();
    }

    [Fact]
    public void GraphTypeOverride_ShouldUseCustomGraphType()
    {
        var builder = new CustomGraphTypeBuilder();
        var graph = builder.BuildGraph();

        graph.Should().BeOfType<TestBuildableGraph>();

        var iGraph = (IGraph)graph;
        iGraph.NodeCount().Should().Be(2);
        iGraph.ArcCount().Should().Be(1);
    }

    [Fact]
    public void DefaultDirectedness_ShouldApplyToArcs()
    {
        var builder = new DefaultUndirectedBuilder();
        var graph = builder.BuildGraph();

        var arc = graph.Arcs(ArcFilter.Edge).Single();

        graph.IsEdge(arc).Should().BeTrue();
    }

    [Fact]
    public void NodeLogicalNames_ShouldBeResolvedFromArcNames()
    {
        var builder = new LogicalNameNodeBuilder();
        var graph = builder.BuildGraph();

        var iGraph = (IGraph)graph;
        iGraph.NodeCount().Should().Be(2);
        iGraph.ArcCount().Should().Be(1);
    }

    [Fact]
    public void NamedWeights_ShouldPopulateNamedCostFunction()
    {
        var builder = new WeightedGraphBuilder();
        var graph = builder.BuildGraph();

        var iGraph = (IGraph)graph;
        var arc = iGraph.Arcs().Single();

        var distanceCost = builder.CreateCostFunction("distance");
        distanceCost(arc).Should().Be(2.0);
    }

    [Fact]
    public void Tags_ShouldBeAvailableViaTagLookup()
    {
        var builder = new TaggedGraphBuilder();
        var graph = builder.BuildGraph();

        var iGraph = (IGraph)graph;
        var arc = iGraph.Arcs().Single();

        var tagLookup = builder.CreateTagLookup();
        tagLookup(arc).Should().Be("road");
    }
}
