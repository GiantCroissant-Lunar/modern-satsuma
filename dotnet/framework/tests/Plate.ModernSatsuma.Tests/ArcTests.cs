using System;
using FluentAssertions;
using NSubstitute;
using Plate.ModernSatsuma;
using Xunit;

namespace Plate.ModernSatsuma.Test
{
    /// <summary>
    /// Tests for Arc struct
    /// </summary>
    public class ArcTests
    {
        [Fact]
        public void Arc_WithValidId_ShouldCreateSuccessfully()
        {
            // Arrange
            var id = 42L;

            // Act
            var arc = new Arc(id);

            // Assert
            arc.Id.Should().Be(id);
        }

        [Fact]
        public void Arc_Invalid_ShouldHaveIdZero()
        {
            // Act
            var invalidArc = Arc.Invalid;

            // Assert
            invalidArc.Id.Should().Be(0);
        }

        [Fact]
        public void Arc_Equals_ShouldWorkCorrectly()
        {
            // Arrange
            var arc1 = new Arc(1);
            var arc2 = new Arc(1);
            var arc3 = new Arc(2);

            // Assert
            arc1.Should().Be(arc2);
            arc1.Should().NotBe(arc3);
            arc1.Equals(arc2).Should().BeTrue();
            arc1.Equals(arc3).Should().BeFalse();
        }

        [Fact]
        public void Arc_ToString_ShouldReturnFormattedString()
        {
            // Arrange
            var arc = new Arc(456);

            // Act
            var result = arc.ToString();

            // Assert
            result.Should().Be("|456");
        }
    }
}
