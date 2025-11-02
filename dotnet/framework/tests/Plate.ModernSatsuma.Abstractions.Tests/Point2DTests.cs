using FluentAssertions;
using Plate.ModernSatsuma.Abstractions;
using Xunit;

namespace Plate.ModernSatsuma.Abstractions.Tests
{
    public class Point2DTests
    {
        [Fact]
        public void Constructor_ShouldSetCoordinates()
        {
            // Arrange
            var x = 10.5;
            var y = 20.7;

            // Act
            var point = new Point2D(x, y);

            // Assert
            point.X.Should().Be(x);
            point.Y.Should().Be(y);
        }

        [Fact]
        public void Equals_ShouldWorkCorrectly()
        {
            // Arrange
            var point1 = new Point2D(10, 20);
            var point2 = new Point2D(10, 20);
            var point3 = new Point2D(15, 20);
            var point4 = new Point2D(10, 25);

            // Act & Assert
            point1.Should().Be(point2);
            point1.Should().NotBe(point3);
            point1.Should().NotBe(point4);
            
            // Test IEquatable
            point1.Equals(point2).Should().BeTrue();
            point1.Equals(point3).Should().BeFalse();
            point1.Equals(null).Should().BeFalse();
        }

        [Fact]
        public void Distance_ShouldCalculateCorrectly()
        {
            // Arrange
            var point1 = new Point2D(0, 0);
            var point2 = new Point2D(3, 4);
            var point3 = new Point2D(-3, -4);

            // Act & Assert
            point1.Distance(point2).Should().Be(5.0);
            point1.Distance(point3).Should().Be(5.0);
            point2.Distance(point3).Should().Be(10.0);
        }

        [Fact]
        public void Distance_ShouldBeZeroForSamePoint()
        {
            // Arrange
            var point = new Point2D(10, 20);

            // Act & Assert
            point.Distance(point).Should().Be(0.0);
        }

        [Fact]
        public void Addition_ShouldWorkCorrectly()
        {
            // Arrange
            var point1 = new Point2D(10, 20);
            var point2 = new Point2D(5, 15);

            // Act
            var result = point1 + point2;

            // Assert
            result.Should().Be(new Point2D(15, 35));
        }

        [Fact]
        public void Subtraction_ShouldWorkCorrectly()
        {
            // Arrange
            var point1 = new Point2D(20, 30);
            var point2 = new Point2D(5, 10);

            // Act
            var result = point1 - point2;

            // Assert
            result.Should().Be(new Point2D(15, 20));
        }

        [Fact]
        public void Multiplication_ShouldWorkCorrectly()
        {
            // Arrange
            var point = new Point2D(10, 20);
            var scalar = 2.5;

            // Act
            var result = point * scalar;

            // Assert
            result.Should().Be(new Point2D(25, 50));
        }

        [Fact]
        public void Multiplication_ShouldHandleZeroScalar()
        {
            // Arrange
            var point = new Point2D(10, 20);

            // Act
            var result = point * 0;

            // Assert
            result.Should().Be(new Point2D(0, 0));
        }

        [Fact]
        public void Multiplication_ShouldHandleNegativeScalar()
        {
            // Arrange
            var point = new Point2D(10, 20);

            // Act
            var result = point * -2;

            // Assert
            result.Should().Be(new Point2D(-20, -40));
        }

        [Fact]
        public void GetHashCode_ShouldBeConsistent()
        {
            // Arrange
            var point1 = new Point2D(10, 20);
            var point2 = new Point2D(10, 20);
            var point3 = new Point2D(15, 20);

            // Act & Assert
            point1.GetHashCode().Should().Be(point2.GetHashCode());
            point1.GetHashCode().Should().NotBe(point3.GetHashCode());
        }

        [Fact]
        public void GetHashCode_ShouldBeConsistentAcrossCalls()
        {
            // Arrange
            var point = new Point2D(10, 20);

            // Act & Assert
            var hash1 = point.GetHashCode();
            var hash2 = point.GetHashCode();
            hash1.Should().Be(hash2);
        }

        [Fact]
        public void ToString_ShouldFormatCorrectly()
        {
            // Arrange
            var point = new Point2D(10.5, 20.7);

            // Act
            var result = point.ToString();

            // Assert
            result.Should().Be("(10.5, 20.7)");
        }

        [Fact]
        public void EqualityOperators_ShouldWorkCorrectly()
        {
            // Arrange
            var point1 = new Point2D(10, 20);
            var point2 = new Point2D(10, 20);
            var point3 = new Point2D(15, 20);

            // Act & Assert
            (point1 == point2).Should().BeTrue();
            (point1 == point3).Should().BeFalse();
            (point1 != point3).Should().BeTrue();
            (point1 != point2).Should().BeFalse();
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(-10.5, 20.7)]
        [InlineData(1000, -2000)]
        [InlineData(double.MaxValue, double.MinValue)]
        public void Constructor_ShouldHandleEdgeCases(double x, double y)
        {
            // Act
            var point = new Point2D(x, y);

            // Assert
            point.X.Should().Be(x);
            point.Y.Should().Be(y);
        }

        [Fact]
        public void EqualsObject_ShouldHandleDifferentTypes()
        {
            // Arrange
            var point = new Point2D(10, 20);
            var other = "not a point";

            // Act & Assert
            point.Equals(other).Should().BeFalse();
        }
    }
}