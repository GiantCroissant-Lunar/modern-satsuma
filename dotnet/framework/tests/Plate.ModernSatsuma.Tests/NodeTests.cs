using System;
using FluentAssertions;
using NSubstitute;
using Plate.ModernSatsuma;
using Xunit;

namespace Plate.ModernSatsuma.Test
{
    /// <summary>
    /// Tests for Node struct
    /// </summary>
    public class NodeTests
    {
        [Fact]
        public void Node_WithValidId_ShouldCreateSuccessfully()
        {
            // Arrange
            var id = 42L;

            // Act
            var node = new Node(id);

            // Assert
            node.Id.Should().Be(id);
        }

        [Fact]
        public void Node_Invalid_ShouldHaveIdZero()
        {
            // Act
            var invalidNode = Node.Invalid;

            // Assert
            invalidNode.Id.Should().Be(0);
        }

        [Fact]
        public void Node_Equals_ShouldWorkCorrectly()
        {
            // Arrange
            var node1 = new Node(1);
            var node2 = new Node(1);
            var node3 = new Node(2);

            // Assert
            node1.Should().Be(node2);
            node1.Should().NotBe(node3);
            node1.Equals(node2).Should().BeTrue();
            node1.Equals(node3).Should().BeFalse();
        }

        [Fact]
        public void Node_ToString_ShouldReturnFormattedString()
        {
            // Arrange
            var node = new Node(123);

            // Act
            var result = node.ToString();

            // Assert
            result.Should().Be("#123");
        }
    }
}
