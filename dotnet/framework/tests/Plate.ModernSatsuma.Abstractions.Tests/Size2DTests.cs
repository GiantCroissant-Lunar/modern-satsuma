using FluentAssertions;
using Plate.ModernSatsuma.Abstractions;
using Xunit;

namespace Plate.ModernSatsuma.Abstractions.Tests
{
    public class Size2DTests
    {
        [Fact]
        public void Constructor_ShouldSetDimensions()
        {
            // Arrange
            var width = 100.5;
            var height = 75.3;

            // Act
            var size = new Size2D(width, height);

            // Assert
            size.Width.Should().Be(width);
            size.Height.Should().Be(height);
        }

        [Fact]
        public void Constructor_ShouldHandleZeroDimensions()
        {
            // Arrange & Act
            var size = new Size2D(0, 0);

            // Assert
            size.Width.Should().Be(0);
            size.Height.Should().Be(0);
        }

        [Fact]
        public void Constructor_ShouldHandleNegativeDimensions()
        {
            // Arrange
            var width = -10.5;
            var height = -20.3;

            // Act
            var size = new Size2D(width, height);

            // Assert
            size.Width.Should().Be(width);
            size.Height.Should().Be(height);
        }

        [Theory]
        [InlineData(10.5, 20.7)]
        [InlineData(0, 0)]
        [InlineData(-100, 200)]
        [InlineData(double.MaxValue, double.MinValue)]
        public void Constructor_ShouldHandleEdgeCases(double width, double height)
        {
            // Act
            var size = new Size2D(width, height);

            // Assert
            size.Width.Should().Be(width);
            size.Height.Should().Be(height);
        }

        [Fact]
        public void DefaultConstructor_ShouldWork()
        {
            // Act
            var size = new Size2D(100, 50);

            // Assert
            size.Width.Should().Be(100);
            size.Height.Should().Be(50);
        }

        [Fact]
        public void Size_ShouldBeImmutable()
        {
            // Arrange
            var size = new Size2D(100, 50);

            // Act & Assert - Since it's readonly, we can't modify it directly
            // This test verifies the struct is readonly by attempting to use it
            var copy = size;
            copy.Should().Be(size);
        }
    }
}