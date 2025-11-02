using FluentAssertions;
using Plate.ModernSatsuma.Abstractions;
using Plate.ModernSatsuma.Drawing.SystemDrawing;
using System;
using Xunit;

namespace Plate.ModernSatsuma.Drawing.SystemDrawing.Tests
{
    public class NodeStyleTests : IDisposable
    {
        private readonly SystemDrawingRenderSurfaceFactory _surfaceFactory;
        private readonly IRenderSurface _surface;
        private readonly IGraphicsContext _context;

        public NodeStyleTests()
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

        [Fact]
        public void Constructor_ShouldSetDefaultValues()
        {
            // Arrange
            var factory = _surfaceFactory.GraphicsFactory;

            // Act
            var style = new NodeStyle(factory);

            // Assert
            style.Should().NotBeNull();
            style.Pen.Should().NotBeNull();
            style.Brush.Should().NotBeNull();
            style.Shape.Should().NotBeNull();
            style.TextFont.Should().NotBeNull();
            style.TextBrush.Should().NotBeNull();
            
            // Check default colors
            style.Pen.Color.Should().Be(Color.Black);
            style.Brush.Color.Should().Be(Color.White);
            style.TextBrush.Color.Should().Be(Color.Black);
        }

        [Fact]
        public void DrawNode_WithoutText_ShouldNotThrow()
        {
            // Arrange
            var factory = _surfaceFactory.GraphicsFactory;
            var style = new NodeStyle(factory);

            // Act & Assert
            style.Invoking(s => s.DrawNode(_context, 100, 100, null)).Should().NotThrow();
            style.Invoking(s => s.DrawNode(_context, 200, 200, "")).Should().NotThrow();
        }

        [Fact]
        public void DrawNode_WithText_ShouldNotThrow()
        {
            // Arrange
            var factory = _surfaceFactory.GraphicsFactory;
            var style = new NodeStyle(factory);

            // Act & Assert
            style.Invoking(s => s.DrawNode(_context, 100, 100, "Node 1")).Should().NotThrow();
            style.Invoking(s => s.DrawNode(_context, 200, 200, "A")).Should().NotThrow();
            style.Invoking(s => s.DrawNode(_context, 300, 300, "Long text")).Should().NotThrow();
        }

        [Fact]
        public void Properties_ShouldBeSettable()
        {
            // Arrange
            var factory = _surfaceFactory.GraphicsFactory;
            var style = new NodeStyle(factory);
            
            var newPen = factory.CreatePen(Color.Red, 2.0);
            var newBrush = factory.CreateBrush(Color.Yellow);
            var newShape = new NodeShape(NodeShapeKind.Rectangle, new Size2D(50, 50));
            var newFont = factory.CreateFont("Arial", 14.0, bold: true);
            var newTextBrush = factory.CreateBrush(Color.Blue);

            // Act
            style.Pen = newPen;
            style.Brush = newBrush;
            style.Shape = newShape;
            style.TextFont = newFont;
            style.TextBrush = newTextBrush;

            // Assert
            style.Pen.Should().BeSameAs(newPen);
            style.Brush.Should().BeSameAs(newBrush);
            style.Shape.Should().BeSameAs(newShape);
            style.TextFont.Should().BeSameAs(newFont);
            style.TextBrush.Should().BeSameAs(newTextBrush);
        }

        [Fact]
        public void DrawNode_WithCustomColors_ShouldNotThrow()
        {
            // Arrange
            var factory = _surfaceFactory.GraphicsFactory;
            var style = new NodeStyle(factory)
            {
                Pen = factory.CreatePen(Color.Red, 2.0),
                Brush = factory.CreateBrush(Color.Yellow),
                TextBrush = factory.CreateBrush(Color.Blue)
            };

            // Act & Assert
            style.Invoking(s => s.DrawNode(_context, 100, 100, "Custom")).Should().NotThrow();
        }

        [Theory]
        [InlineData(NodeShapeKind.Diamond)]
        [InlineData(NodeShapeKind.Ellipse)]
        [InlineData(NodeShapeKind.Rectangle)]
        [InlineData(NodeShapeKind.Triangle)]
        [InlineData(NodeShapeKind.UpsideDownTriangle)]
        public void DrawNode_WithDifferentShapes_ShouldNotThrow(NodeShapeKind shapeKind)
        {
            // Arrange
            var factory = _surfaceFactory.GraphicsFactory;
            var style = new NodeStyle(factory)
            {
                Shape = new NodeShape(shapeKind, new Size2D(40, 40))
            };

            // Act & Assert
            style.Invoking(s => s.DrawNode(_context, 100, 100, "Text")).Should().NotThrow();
        }

        [Fact]
        public void DrawNode_AtVariousPositions_ShouldNotThrow()
        {
            // Arrange
            var factory = _surfaceFactory.GraphicsFactory;
            var style = new NodeStyle(factory);

            // Act & Assert - Various positions
            style.Invoking(s => s.DrawNode(_context, 0, 0, "Origin")).Should().NotThrow();
            style.Invoking(s => s.DrawNode(_context, -50, -50, "Negative")).Should().NotThrow();
            style.Invoking(s => s.DrawNode(_context, 500, 500, "Outside")).Should().NotThrow();
            style.Invoking(s => s.DrawNode(_context, 200.5, 150.7, "Floating")).Should().NotThrow();
        }

        [Fact]
        public void DrawNode_MultipleNodesWithDifferentStyles_ShouldNotInterfere()
        {
            // Arrange
            var factory = _surfaceFactory.GraphicsFactory;
            
            var style1 = new NodeStyle(factory)
            {
                Brush = factory.CreateBrush(Color.Red),
                Shape = new NodeShape(NodeShapeKind.Rectangle, new Size2D(30, 30))
            };
            
            var style2 = new NodeStyle(factory)
            {
                Brush = factory.CreateBrush(Color.Blue),
                Shape = new NodeShape(NodeShapeKind.Ellipse, new Size2D(40, 40))
            };
            
            var style3 = new NodeStyle(factory)
            {
                Brush = factory.CreateBrush(Color.Green),
                Shape = new NodeShape(NodeShapeKind.Triangle, new Size2D(35, 35))
            };

            // Act & Assert
            style1.Invoking(s => s.DrawNode(_context, 50, 50, "1")).Should().NotThrow();
            style2.Invoking(s => s.DrawNode(_context, 150, 50, "2")).Should().NotThrow();
            style3.Invoking(s => s.DrawNode(_context, 250, 50, "3")).Should().NotThrow();
        }

        [Fact]
        public void DefaultShape_ShouldBeEllipse()
        {
            // Assert
            NodeStyle.DefaultShape.Should().NotBeNull();
            ((NodeShape)NodeStyle.DefaultShape).Kind.Should().Be(NodeShapeKind.Ellipse);
            ((NodeShape)NodeStyle.DefaultShape).Size.Should().Be(new Size2D(10, 10));
        }
    }
}
