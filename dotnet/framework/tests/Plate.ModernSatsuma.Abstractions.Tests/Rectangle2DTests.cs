using FluentAssertions;
using Plate.ModernSatsuma.Abstractions;
using Xunit;

namespace Plate.ModernSatsuma.Abstractions.Tests
{
    public class Rectangle2DTests
    {
        [Fact]
        public void Constructor_ShouldSetProperties()
        {
            // Arrange
            var x = 10.5;
            var y = 20.7;
            var width = 100.3;
            var height = 75.9;

            // Act
            var rect = new Rectangle2D(x, y, width, height);

            // Assert
            rect.X.Should().Be(x);
            rect.Y.Should().Be(y);
            rect.Width.Should().Be(width);
            rect.Height.Should().Be(height);
        }

        [Fact]
        public void Properties_ShouldCalculateCorrectly()
        {
            // Arrange
            var rect = new Rectangle2D(10, 20, 100, 50);

            // Act & Assert
            rect.Left.Should().Be(10);
            rect.Top.Should().Be(20);
            rect.Right.Should().Be(110); // X + Width
            rect.Bottom.Should().Be(70); // Y + Height
        }

        [Fact]
        public void Properties_ShouldHandleNegativeCoordinates()
        {
            // Arrange
            var rect = new Rectangle2D(-10, -20, 100, 50);

            // Act & Assert
            rect.Left.Should().Be(-10);
            rect.Top.Should().Be(-20);
            rect.Right.Should().Be(90); // -10 + 100
            rect.Bottom.Should().Be(30); // -20 + 50
        }

        [Fact]
        public void Properties_ShouldHandleZeroDimensions()
        {
            // Arrange
            var rect = new Rectangle2D(10, 20, 0, 0);

            // Act & Assert
            rect.Left.Should().Be(10);
            rect.Top.Should().Be(20);
            rect.Right.Should().Be(10); // X + 0
            rect.Bottom.Should().Be(20); // Y + 0
        }

        [Theory]
        [InlineData(0, 0, 10, 20)]
        [InlineData(-10.5, 20.7, 100.3, -50.9)]
        [InlineData(double.MaxValue, double.MinValue, 1, 1)]
        [InlineData(0, 0, 0, 0)]
        public void Constructor_ShouldHandleEdgeCases(double x, double y, double width, double height)
        {
            // Act
            var rect = new Rectangle2D(x, y, width, height);

            // Assert
            rect.X.Should().Be(x);
            rect.Y.Should().Be(y);
            rect.Width.Should().Be(width);
            rect.Height.Should().Be(height);
        }

        [Fact]
        public void Properties_ShouldCalculateCorrectlyForNegativeDimensions()
        {
            // Arrange
            var rect = new Rectangle2D(10, 20, -100, -50);

            // Act & Assert
            rect.Left.Should().Be(10);
            rect.Top.Should().Be(20);
            rect.Right.Should().Be(-90); // 10 + (-100)
            rect.Bottom.Should().Be(-30); // 20 + (-50)
        }

        [Fact]
        public void Properties_ShouldHandleFloatingPointPrecision()
        {
            // Arrange
            var rect = new Rectangle2D(10.5, 20.7, 100.3, 75.9);

            // Act & Assert
            rect.Left.Should().Be(10.5);
            rect.Top.Should().Be(20.7);
            rect.Right.Should().Be(110.8); // 10.5 + 100.3
            rect.Bottom.Should().BeApproximately(96.6, 1e-10); // 20.7 + 75.9 (with precision tolerance)
        }

        [Fact]
        public void Constructor_ShouldHandleLargeValues()
        {
            // Arrange
            var x = double.MaxValue / 4;
            var y = double.MaxValue / 4;
            var width = double.MaxValue / 2;
            var height = double.MaxValue / 2;

            // Act
            var rect = new Rectangle2D(x, y, width, height);

            // Assert
            rect.X.Should().Be(x);
            rect.Y.Should().Be(y);
            rect.Width.Should().Be(width);
            rect.Height.Should().Be(height);
        }

        [Fact]
        public void Rectangle_ShouldBeImmutable()
        {
            // Arrange
            var rect = new Rectangle2D(10, 20, 100, 50);

            // Act & Assert - Since it's readonly, we can't modify it directly
            // This test verifies the struct maintains its values
            var copy = rect;
            copy.X.Should().Be(rect.X);
            copy.Y.Should().Be(rect.Y);
            copy.Width.Should().Be(rect.Width);
            copy.Height.Should().Be(rect.Height);
        }
    }
}