using FluentAssertions;
using Plate.ModernSatsuma.Abstractions;
using Plate.ModernSatsuma.Drawing.SystemDrawing;
using System;
using Xunit;

namespace Plate.ModernSatsuma.Drawing.SystemDrawing.Tests
{
    public class NodeShapeTests : IDisposable
    {
        private readonly SystemDrawingRenderSurfaceFactory _surfaceFactory;
        private readonly IRenderSurface _surface;
        private readonly IGraphicsContext _context;

        public NodeShapeTests()
        {
            _surfaceFactory = new SystemDrawingRenderSurfaceFactory();
            _surface = _surfaceFactory.CreateSurface(400, 400);
            _context = _surface.GetGraphicsContext();
        }

        public void Dispose()
        {
            _context?.Dispose();
            _surface?.Dispose();
        }

        [Theory]
        [InlineData(NodeShapeKind.Diamond)]
        [InlineData(NodeShapeKind.Ellipse)]
        [InlineData(NodeShapeKind.Rectangle)]
        [InlineData(NodeShapeKind.Triangle)]
        [InlineData(NodeShapeKind.UpsideDownTriangle)]
        public void Constructor_ShouldCreateValidShape(NodeShapeKind kind)
        {
            // Arrange
            var size = new Size2D(50, 50);

            // Act
            var shape = new NodeShape(kind, size);

            // Assert
            shape.Should().NotBeNull();
            shape.Kind.Should().Be(kind);
            shape.Size.Should().Be(size);
        }

        [Fact]
        public void Constructor_WithVariousSizes_ShouldSetSize()
        {
            // Arrange & Act
            var shape1 = new NodeShape(NodeShapeKind.Rectangle, new Size2D(10, 10));
            var shape2 = new NodeShape(NodeShapeKind.Rectangle, new Size2D(100, 50));
            var shape3 = new NodeShape(NodeShapeKind.Rectangle, new Size2D(200, 300));

            // Assert
            shape1.Size.Should().Be(new Size2D(10, 10));
            shape2.Size.Should().Be(new Size2D(100, 50));
            shape3.Size.Should().Be(new Size2D(200, 300));
        }

        [Theory]
        [InlineData(NodeShapeKind.Diamond)]
        [InlineData(NodeShapeKind.Ellipse)]
        [InlineData(NodeShapeKind.Rectangle)]
        [InlineData(NodeShapeKind.Triangle)]
        [InlineData(NodeShapeKind.UpsideDownTriangle)]
        public void Draw_ShouldNotThrow(NodeShapeKind kind)
        {
            // Arrange
            var shape = new NodeShape(kind, new Size2D(40, 40));
            var pen = _surfaceFactory.GraphicsFactory.CreatePen(Color.Black);
            var brush = _surfaceFactory.GraphicsFactory.CreateBrush(Color.White);

            // Act & Assert
            shape.Invoking(s => s.Draw(_context, pen, brush)).Should().NotThrow();
        }

        [Theory]
        [InlineData(0)]      // Right
        [InlineData(Math.PI / 4)]  // Northeast
        [InlineData(Math.PI / 2)]  // Top
        [InlineData(Math.PI)]      // Left
        [InlineData(3 * Math.PI / 2)] // Bottom
        [InlineData(2 * Math.PI)]  // Right again
        public void GetBoundary_WithVariousAngles_ShouldReturnReasonablePoints(double angle)
        {
            // Arrange
            var size = new Size2D(50, 50);
            var shape = new NodeShape(NodeShapeKind.Rectangle, size);

            // Act
            var boundary = shape.GetBoundary(angle);

            // Assert
            boundary.Should().NotBeNull();
            // The boundary point should be within the bounding box of the shape
            Math.Abs(boundary.X).Should().BeLessOrEqualTo(size.Width / 2 + 1); // +1 for floating point tolerance
            Math.Abs(boundary.Y).Should().BeLessOrEqualTo(size.Height / 2 + 1);
        }

        [Fact]
        public void GetBoundary_ForCircle_ShouldReturnPointsOnCircumference()
        {
            // Arrange
            var size = new Size2D(100, 100);
            var shape = new NodeShape(NodeShapeKind.Ellipse, size);
            var radius = size.Width / 2;

            // Act & Assert - Test at cardinal directions
            var right = shape.GetBoundary(0);
            var top = shape.GetBoundary(Math.PI / 2);
            var left = shape.GetBoundary(Math.PI);
            var bottom = shape.GetBoundary(3 * Math.PI / 2);

            // For a circle, boundary should be at radius distance
            right.X.Should().BeApproximately(radius, 0.1);
            right.Y.Should().BeApproximately(0, 0.1);

            top.X.Should().BeApproximately(0, 0.1);
            top.Y.Should().BeApproximately(-radius, 0.1);

            left.X.Should().BeApproximately(-radius, 0.1);
            left.Y.Should().BeApproximately(0, 0.1);

            bottom.X.Should().BeApproximately(0, 0.1);
            bottom.Y.Should().BeApproximately(radius, 0.1);
        }

        [Fact]
        public void Size_ShouldMatchConstructorValue()
        {
            // Arrange
            var expectedSize = new Size2D(75, 60);
            var shape = new NodeShape(NodeShapeKind.Diamond, expectedSize);

            // Act
            var actualSize = shape.Size;

            // Assert
            actualSize.Should().Be(expectedSize);
        }

        [Fact]
        public void Draw_WithDifferentColorsAndStyles_ShouldNotThrow()
        {
            // Arrange
            var shape = new NodeShape(NodeShapeKind.Ellipse, new Size2D(50, 50));
            var factory = _surfaceFactory.GraphicsFactory;

            var pen1 = factory.CreatePen(Color.Red, 1.0);
            var pen2 = factory.CreatePen(Color.Blue, 2.5);
            var pen3 = factory.CreatePen(new Color(128, 128, 128), 0.5);

            var brush1 = factory.CreateBrush(Color.Yellow);
            var brush2 = factory.CreateBrush(Color.Green);
            var brush3 = factory.CreateBrush(new Color(255, 200, 150));

            // Act & Assert
            shape.Invoking(s => s.Draw(_context, pen1, brush1)).Should().NotThrow();
            shape.Invoking(s => s.Draw(_context, pen2, brush2)).Should().NotThrow();
            shape.Invoking(s => s.Draw(_context, pen3, brush3)).Should().NotThrow();
        }

        [Fact]
        public void Draw_MultipleShapesOnSameSurface_ShouldNotInterfere()
        {
            // Arrange
            var factory = _surfaceFactory.GraphicsFactory;
            var pen = factory.CreatePen(Color.Black);
            var brush = factory.CreateBrush(Color.White);

            var shape1 = new NodeShape(NodeShapeKind.Rectangle, new Size2D(30, 30));
            var shape2 = new NodeShape(NodeShapeKind.Ellipse, new Size2D(40, 40));
            var shape3 = new NodeShape(NodeShapeKind.Triangle, new Size2D(35, 35));

            // Act & Assert
            shape1.Invoking(s => s.Draw(_context, pen, brush)).Should().NotThrow();
            shape2.Invoking(s => s.Draw(_context, pen, brush)).Should().NotThrow();
            shape3.Invoking(s => s.Draw(_context, pen, brush)).Should().NotThrow();
        }
    }
}
