using System;
using FluentAssertions;
using NSubstitute;
using Plate.ModernSatsuma;
using Xunit;

namespace Plate.ModernSatsuma.Test
{
    /// <summary>
    /// Tests for ArcLookupExtensions
    /// </summary>
    public class ArcLookupExtensionsTests
    {
        [Fact]
        public void ArcToString_WithValidArc_ShouldReturnFormattedString()
        {
            // Arrange
            var mockGraph = Substitute.For<IArcLookup>();
            var arc = new Arc(123);
            var nodeU = new Node(1);
            var nodeV = new Node(2);

            mockGraph.U(arc).Returns(nodeU);
            mockGraph.V(arc).Returns(nodeV);
            mockGraph.IsEdge(arc).Returns(true);

            // Act
            var result = mockGraph.ArcToString(arc);

            // Assert
            result.Should().Be("#1<-->#2");
        }

        [Fact]
        public void ArcToString_WithInvalidArc_ShouldReturnInvalidString()
        {
            // Arrange
            var mockGraph = Substitute.For<IArcLookup>();

            // Act
            var result = mockGraph.ArcToString(Arc.Invalid);

            // Assert
            result.Should().Be("Arc.Invalid");
        }

        [Fact]
        public void ArcToString_WithDirectedArc_ShouldUseArrows()
        {
            // Arrange
            var mockGraph = Substitute.For<IArcLookup>();
            var arc = new Arc(456);
            var nodeU = new Node(3);
            var nodeV = new Node(4);

            mockGraph.U(arc).Returns(nodeU);
            mockGraph.V(arc).Returns(nodeV);
            mockGraph.IsEdge(arc).Returns(false);

            // Act
            var result = mockGraph.ArcToString(arc);

            // Assert
            result.Should().Be("#3--->#4");
        }

        [Fact]
        public void Other_WithArc_ShouldReturnCorrectOppositeNode()
        {
            // Arrange
            var mockGraph = Substitute.For<IArcLookup>();
            var arc = new Arc(789);
            var nodeU = new Node(5);
            var nodeV = new Node(6);

            mockGraph.U(arc).Returns(nodeU);
            mockGraph.V(arc).Returns(nodeV);

            // Act & Assert
            mockGraph.Other(arc, nodeU).Should().Be(nodeV);
            mockGraph.Other(arc, nodeV).Should().Be(nodeU);
            mockGraph.Other(arc, new Node(99)).Should().Be(nodeU); // Default behavior
        }

        [Fact]
        public void Nodes_WithAllowDuplicatesTrue_ShouldReturnBothNodes()
        {
            // Arrange
            var mockGraph = Substitute.For<IArcLookup>();
            var arc = new Arc(111);
            var nodeU = new Node(7);
            var nodeV = new Node(8);

            mockGraph.U(arc).Returns(nodeU);
            mockGraph.V(arc).Returns(nodeV);

            // Act
            var result = mockGraph.Nodes(arc, allowDuplicates: true);

            // Assert
            result.Should().BeEquivalentTo(new[] { nodeU, nodeV });
        }

        [Fact]
        public void Nodes_WithAllowDuplicatesFalseAndDifferentNodes_ShouldReturnBothNodes()
        {
            // Arrange
            var mockGraph = Substitute.For<IArcLookup>();
            var arc = new Arc(222);
            var nodeU = new Node(9);
            var nodeV = new Node(10);

            mockGraph.U(arc).Returns(nodeU);
            mockGraph.V(arc).Returns(nodeV);

            // Act
            var result = mockGraph.Nodes(arc, allowDuplicates: false);

            // Assert
            result.Should().BeEquivalentTo(new[] { nodeU, nodeV });
        }

        [Fact]
        public void Nodes_WithAllowDuplicatesFalseAndSameNodes_ShouldReturnSingleNode()
        {
            // Arrange
            var mockGraph = Substitute.For<IArcLookup>();
            var arc = new Arc(333);
            var node = new Node(11);

            mockGraph.U(arc).Returns(node);
            mockGraph.V(arc).Returns(node);

            // Act
            var result = mockGraph.Nodes(arc, allowDuplicates: false);

            // Assert
            result.Should().BeEquivalentTo(new[] { node });
        }
    }
}
