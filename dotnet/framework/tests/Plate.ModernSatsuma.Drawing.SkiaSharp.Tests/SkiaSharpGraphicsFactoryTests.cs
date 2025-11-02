using FluentAssertions;
using Plate.ModernSatsuma.Abstractions;
using Plate.ModernSatsuma.Drawing.SkiaSharp;
using Xunit;

namespace Plate.ModernSatsuma.Drawing.SkiaSharp.Tests
{
    public class SkiaSharpGraphicsFactoryTests
    {
        private readonly SkiaSharpGraphicsFactory _factory;

        public SkiaSharpGraphicsFactoryTests()
        {
            _factory = new SkiaSharpGraphicsFactory();
        }

        [Fact]
        public void CreatePen_WithValidColor_ShouldReturnValidPen()
        {
            // Arrange
            var color = Color.Red;

            // Act
            var pen = _factory.CreatePen(color);

            // Assert
            pen.Should().NotBeNull();
            pen.Color.Should().Be(color);
            pen.Width.Should().Be(1.0);
            pen.HasArrowCap.Should().BeFalse();
        }

        [Fact]
        public void CreatePen_WithCustomWidth_ShouldReturnPenWithCorrectWidth()
        {
            // Arrange
            var color = Color.Blue;
            var width = 2.5;

            // Act
            var pen = _factory.CreatePen(color, width);

            // Assert
            pen.Should().NotBeNull();
            pen.Width.Should().Be(width);
        }

        [Fact]
        public void CreatePen_WithArrowCap_ShouldReturnPenWithArrowCap()
        {
            // Arrange
            var color = Color.Green;

            // Act
            var pen = _factory.CreatePen(color, arrowCap: true);

            // Assert
            pen.Should().NotBeNull();
            pen.HasArrowCap.Should().BeTrue();
        }

        [Fact]
        public void CreateBrush_WithValidColor_ShouldReturnValidBrush()
        {
            // Arrange
            var color = Color.Yellow;

            // Act
            var brush = _factory.CreateBrush(color);

            // Assert
            brush.Should().NotBeNull();
            brush.Color.Should().Be(color);
        }

        [Theory]
        [InlineData(255, 0, 0, 255)]    // Red
        [InlineData(0, 255, 0, 255)]    // Green
        [InlineData(0, 0, 255, 255)]    // Blue
        [InlineData(128, 128, 128, 128)] // Gray with alpha
        public void CreateBrush_WithVariousColors_ShouldReturnValidBrush(byte r, byte g, byte b, byte a)
        {
            // Arrange
            var color = new Color(r, g, b, a);

            // Act
            var brush = _factory.CreateBrush(color);

            // Assert
            brush.Should().NotBeNull();
            brush.Color.Should().Be(color);
        }

        [Fact]
        public void CreateFont_WithValidParameters_ShouldReturnValidFont()
        {
            // Arrange
            var fontFamily = "Arial";
            var size = 12.0;

            // Act
            var font = _factory.CreateFont(fontFamily, size);

            // Assert
            font.Should().NotBeNull();
            font.FontFamily.Should().Be(fontFamily);
            font.Size.Should().Be(size);
            font.Bold.Should().BeFalse();
            font.Italic.Should().BeFalse();
        }

        [Fact]
        public void CreateFont_WithBold_ShouldReturnBoldFont()
        {
            // Arrange
            var fontFamily = "Times New Roman";
            var size = 14.0;

            // Act
            var font = _factory.CreateFont(fontFamily, size, bold: true);

            // Assert
            font.Should().NotBeNull();
            font.Bold.Should().BeTrue();
        }

        [Fact]
        public void CreateFont_WithItalic_ShouldReturnItalicFont()
        {
            // Arrange
            var fontFamily = "Courier New";
            var size = 10.0;

            // Act
            var font = _factory.CreateFont(fontFamily, size, italic: true);

            // Assert
            font.Should().NotBeNull();
            font.Italic.Should().BeTrue();
        }

        [Fact]
        public void CreateFont_WithBoldAndItalic_ShouldReturnBoldItalicFont()
        {
            // Arrange
            var fontFamily = "Verdana";
            var size = 16.0;

            // Act
            var font = _factory.CreateFont(fontFamily, size, bold: true, italic: true);

            // Assert
            font.Should().NotBeNull();
            font.Bold.Should().BeTrue();
            font.Italic.Should().BeTrue();
        }

        [Fact]
        public void GetDefaultFont_ShouldReturnValidFont()
        {
            // Act
            var font = _factory.GetDefaultFont();

            // Assert
            font.Should().NotBeNull();
            font.FontFamily.Should().NotBeNullOrEmpty();
            font.Size.Should().BeGreaterThan(0);
        }

        [Fact]
        public void GetDefaultFont_ShouldReturnConsistentFont()
        {
            // Act
            var font1 = _factory.GetDefaultFont();
            var font2 = _factory.GetDefaultFont();

            // Assert
            font1.FontFamily.Should().Be(font2.FontFamily);
            font1.Size.Should().Be(font2.Size);
            font1.Bold.Should().Be(font2.Bold);
            font1.Italic.Should().Be(font2.Italic);
        }
    }
}
