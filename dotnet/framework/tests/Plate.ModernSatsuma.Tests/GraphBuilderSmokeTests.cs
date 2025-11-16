using FluentAssertions;
using Plate.ModernSatsuma;
using Plate.ModernSatsuma.Generators;
using Xunit;

namespace Plate.ModernSatsuma.Test;

[GraphBuilder(GraphType = typeof(CustomGraph))]
public partial class SmokeGraph
{
    [NodeAttribute]
    public Node A { get; private set; }

    [NodeAttribute]
    public Node B { get; private set; }

    [ArcAttribute]
    public void A_to_B(Node from, Node to)
    {
    }
}

/// <summary>
/// Smoke tests to ensure the GraphBuilder source generator is wired correctly
/// and that the attributes can be consumed from a test project.
///
/// NOTE: This test currently does not depend on any generated methods. The
/// generator only emits a stub file per [GraphBuilder] type, which is sufficient
/// to verify that the analyzer is running and compilation succeeds.
/// </summary>
public class GraphBuilderSmokeTests
{
    [Fact]
    public void GraphBuilderAttributes_ShouldBeUsableFromTests()
    {
        // Arrange & Act
        var builder = new SmokeGraph();

        // Assert
        builder.Should().NotBeNull();
    }
}
