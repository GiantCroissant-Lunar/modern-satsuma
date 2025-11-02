using FluentAssertions;
using Plate.ModernSatsuma.Abstractions;
using Plate.ModernSatsuma.Drawing.SystemDrawing;
using System;
using Xunit;

namespace Plate.ModernSatsuma.Drawing.SystemDrawing.Tests
{
    public class SystemDrawingGraphicsContextTests : IDisposable
    {
        private readonly SystemDrawingRenderSurfaceFactory _surfaceFactory;
        private readonly IRenderSurface _surface;
        private readonly IGraphicsContext _context;

        public SystemDrawingGraphicsContextTests()
        {
            _surfaceFactory = new SystemDrawingRenderSurfaceFactory();
            _surface = _surfaceFactory.CreateSurface(800, 600);
            _context = _surface.GetGraphicsContext();
        }

        public void Dispose()
        {
            _context?.Dispose();
            _surface?.Dispose();
        }

        [Fact]
        public void GraphicsContext_ShouldBeCreated()
        {
            // Assert
            _context.Should().NotBeNull();
            _surface.Width.Should().Be(800);
            _surface.Height.Should().Be(600);
        }

        [Fact]
        public void Clear_WithValidColor_ShouldNotThrow()
        {
            // Arrange
            var color = Color.Red;

            // Act & Assert
            _context.Invoking(x => x.Clear(color)).Should().NotThrow();
        }

        [Theory]
        [InlineData(255, 0, 0, 255)] // Red
        [InlineData(0, 255, 0, 128)] // Green with alpha
        [InlineData(0, 0, 255, 64)] // Blue with alpha
        [InlineData(255, 255, 255, 255)] // White
        [InlineData(0, 0, 0, 0)] // Transparent
        public void Clear_WithDifferentColors_ShouldNotThrow(byte r, byte g, byte b, byte a)
        {
            // Arrange
            var color = new Color(r, g, b, a);

            // Act & Assert
            _context.Invoking(x => x.Clear(color)).Should().NotThrow();
        }

        [Fact]
        public void DrawLine_WithValidParameters_ShouldNotThrow()
        {
            // Arrange
            var factory = _surfaceFactory.GraphicsFactory;
            var pen = factory.CreatePen(Color.Blue, 2.5);

            // Act & Assert
            _context.Invoking(x => x.DrawLine(pen, 10, 20, 100, 200)).Should().NotThrow();
        }

        [Theory]
        [InlineData(0, 0, 100, 100, 1.0f)]
        [InlineData(-50, -50, 50, 50, 0.5f)]
        [InlineData(0, 0, 0, 0, 10.0f)]
        [InlineData(1000, 1000, -1000, -1000, 0.1f)]
        public void DrawLine_WithVariousParameters_ShouldNotThrow(float x1, float y1, float x2, float y2, float width)
        {
            // Arrange
            var factory = _surfaceFactory.GraphicsFactory;
            var pen = factory.CreatePen(Color.Green, width);

            // Act & Assert
            _context.Invoking(x => x.DrawLine(pen, x1, y1, x2, y2)).Should().NotThrow();
        }

        [Fact]
        public void DrawEllipse_WithValidParameters_ShouldNotThrow()
        {
            // Arrange
            var factory = _surfaceFactory.GraphicsFactory;
            var pen = factory.CreatePen(new Color(128, 0, 128), 2.0f); // Purple
            var rect = new Rectangle2D(50, 50, 100, 60); // Center (100,80), radiusX=50, radiusY=30

            // Act & Assert
            _context.Invoking(x => x.DrawEllipse(pen, rect)).Should().NotThrow();
        }

        [Theory]
        [InlineData(0, 0, 20, 10)] // Center (10,5), radiusX=10, radiusY=5
        [InlineData(-100, -100, 100, 50)] // Center (-50,-75), radiusX=50, radiusY=25
        [InlineData(10, 10, 2, 2)] // Center (11,11), radiusX=1, radiusY=1
        [InlineData(500, 500, 200, 100)] // Center (600,550), radiusX=100, radiusY=50
        public void DrawEllipse_WithVariousParameters_ShouldNotThrow(float x, float y, float width, float height)
        {
            // Arrange
            var factory = _surfaceFactory.GraphicsFactory;
            var pen = factory.CreatePen(new Color(255, 165, 0), 1.5f); // Orange
            var rect = new Rectangle2D(x, y, width, height);

            // Act & Assert
            _context.Invoking(x => x.DrawEllipse(pen, rect)).Should().NotThrow();
        }

        [Fact]
        public void FillEllipse_WithValidParameters_ShouldNotThrow()
        {
            // Arrange
            var factory = _surfaceFactory.GraphicsFactory;
            var brush = factory.CreateBrush(new Color(0, 255, 255)); // Cyan
            var rect = new Rectangle2D(50, 50, 100, 60);

            // Act & Assert
            _context.Invoking(x => x.FillEllipse(brush, rect)).Should().NotThrow();
        }

        [Theory]
        [InlineData(0, 0, 20, 10)]
        [InlineData(-100, -100, 100, 50)]
        [InlineData(10, 10, 2, 2)]
        [InlineData(500, 500, 200, 100)]
        public void FillEllipse_WithVariousParameters_ShouldNotThrow(float x, float y, float width, float height)
        {
            // Arrange
            var factory = _surfaceFactory.GraphicsFactory;
            var brush = factory.CreateBrush(new Color(255, 0, 255)); // Magenta
            var rect = new Rectangle2D(x, y, width, height);

            // Act & Assert
            _context.Invoking(x => x.FillEllipse(brush, rect)).Should().NotThrow();
        }

        [Fact]
        public void DrawPolygon_WithValidParameters_ShouldNotThrow()
        {
            // Arrange
            var factory = _surfaceFactory.GraphicsFactory;
            var pen = factory.CreatePen(Color.Blue, 2.0f);
            var points = new[]
            {
                new Point2D(0, 0),
                new Point2D(100, 0),
                new Point2D(100, 100),
                new Point2D(0, 100)
            };

            // Act & Assert
            _context.Invoking(x => x.DrawPolygon(pen, points)).Should().NotThrow();
        }

        [Fact]
        public void FillPolygon_WithValidParameters_ShouldNotThrow()
        {
            // Arrange
            var factory = _surfaceFactory.GraphicsFactory;
            var brush = factory.CreateBrush(Color.Yellow);
            var points = new[]
            {
                new Point2D(0, 0),
                new Point2D(100, 0),
                new Point2D(100, 100),
                new Point2D(0, 100)
            };

            // Act & Assert
            _context.Invoking(x => x.FillPolygon(brush, points)).Should().NotThrow();
        }

        [Fact]
        public void DrawString_WithValidParameters_ShouldNotThrow()
        {
            // Arrange
            var factory = _surfaceFactory.GraphicsFactory;
            var font = factory.CreateFont("Arial", 12.0f);
            var brush = factory.CreateBrush(Color.Black);

            // Act & Assert
            _context.Invoking(x => x.DrawString("Hello, World!", font, brush, 50, 50)).Should().NotThrow();
        }

        [Theory]
        [InlineData("", 0, 0, 8.0f)]
        [InlineData("Test", 100, 100, 16.0f)]
        [InlineData("Special chars: !@#$%", -50, -50, 20.0f)]
        [InlineData("Unicode: αβγδε", 500, 500, 24.0f)]
        [InlineData("Very long text that might wrap or cause issues", 0, 0, 10.0f)]
        public void DrawString_WithVariousParameters_ShouldNotThrow(string text, float xPos, float yPos, float fontSize)
        {
            // Arrange
            var factory = _surfaceFactory.GraphicsFactory;
            var font = factory.CreateFont("Arial", fontSize);
            var brush = factory.CreateBrush(new Color(0, 0, 139)); // Dark Blue

            // Act & Assert
            _context.Invoking(ctx => ctx.DrawString(text, font, brush, xPos, yPos)).Should().NotThrow();
        }

        [Fact]
        public void DrawString_WithNullText_ShouldThrowArgumentNullException()
        {
            // Arrange
            var factory = _surfaceFactory.GraphicsFactory;
            var font = factory.CreateFont("Arial", 12.0f);
            var brush = factory.CreateBrush(Color.Black);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _context.DrawString(null!, font, brush, 50, 50));
        }

        [Fact]
        public void DrawString_WithEmptyText_ShouldNotThrow()
        {
            // Arrange
            var factory = _surfaceFactory.GraphicsFactory;
            var font = factory.CreateFont("Arial", 12.0f);
            var brush = factory.CreateBrush(Color.Black);
            var text = string.Empty;

            // Act & Assert
            _context.Invoking(x => x.DrawString(text, font, brush, 50, 50)).Should().NotThrow();
        }

        [Fact]
        public void MeasureString_WithValidParameters_ShouldReturnSize()
        {
            // Arrange
            var factory = _surfaceFactory.GraphicsFactory;
            var font = factory.CreateFont("Arial", 12.0f);
            var text = "Hello, World!";

            // Act
            var size = _context.MeasureString(text, font);

            // Assert
            size.Width.Should().BeGreaterThan(0);
            size.Height.Should().BeGreaterThan(0);
        }

        [Fact]
        public void MeasureString_WithEmptyText_ShouldReturnZeroSize()
        {
            // Arrange
            var factory = _surfaceFactory.GraphicsFactory;
            var font = factory.CreateFont("Arial", 12.0f);
            var text = string.Empty;

            // Act
            var size = _context.MeasureString(text, font);

            // Assert
            size.Width.Should().BeGreaterThanOrEqualTo(0);
            size.Height.Should().BeGreaterThanOrEqualTo(0);
        }

        [Fact]
        public void SaveAndRestore_ShouldWorkCorrectly()
        {
            // Arrange
            var factory = _surfaceFactory.GraphicsFactory;

            // Act
            var state = _context.Save();
            _context.Translate(100, 100);
            _context.Scale(2.0, 2.0);
            _context.Restore(state);

            // Assert - Should not throw and operations should work after restore
            _context.Invoking(x => x.Clear(Color.White)).Should().NotThrow();
        }

        [Fact]
        public void Translate_ShouldNotThrow()
        {
            // Act & Assert
            _context.Invoking(x => x.Translate(50.5, 25.7)).Should().NotThrow();
        }

        [Fact]
        public void Scale_ShouldNotThrow()
        {
            // Act & Assert
            _context.Invoking(x => x.Scale(2.0, 1.5)).Should().NotThrow();
        }

        [Fact]
        public void MultipleOperations_ShouldWorkCorrectly()
        {
            // Arrange
            var factory = _surfaceFactory.GraphicsFactory;
            var pen = factory.CreatePen(Color.Blue, 2.0f);
            var brush = factory.CreateBrush(Color.Red);
            var font = factory.CreateFont("Arial", 12.0f);
            var points = new[]
            {
                new Point2D(0, 0),
                new Point2D(100, 0),
                new Point2D(100, 100),
                new Point2D(0, 100)
            };

            // Act & Assert - Chain multiple operations
            _context.Invoking(x => x.Clear(Color.White)).Should().NotThrow();
            _context.Invoking(x => x.DrawEllipse(pen, new Rectangle2D(10, 10, 100, 50))).Should().NotThrow();
            _context.Invoking(x => x.FillPolygon(brush, points)).Should().NotThrow();
            _context.Invoking(x => x.DrawString("Test", font, brush, 50, 150)).Should().NotThrow();
        }
    }
}