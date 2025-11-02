using FluentAssertions;
using Plate.ModernSatsuma.Abstractions;
using Xunit;

namespace Plate.ModernSatsuma.Abstractions.Tests
{
    public class ColorTests
    {
        [Fact]
        public void Constructor_ShouldSetRGBA()
        {
            // Arrange
            var r = (byte)255;
            var g = (byte)128;
            var b = (byte)64;
            var a = (byte)200;

            // Act
            var color = new Color(r, g, b, a);

            // Assert
            color.R.Should().Be(r);
            color.G.Should().Be(g);
            color.B.Should().Be(b);
            color.A.Should().Be(a);
        }

        [Fact]
        public void Constructor_WithDefaultAlpha_ShouldSetAlphaTo255()
        {
            // Arrange
            var r = (byte)255;
            var g = (byte)128;
            var b = (byte)64;

            // Act
            var color = new Color(r, g, b);

            // Assert
            color.R.Should().Be(r);
            color.G.Should().Be(g);
            color.B.Should().Be(b);
            color.A.Should().Be((byte)255);
        }

        [Fact]
        public void PredefinedColors_ShouldHaveCorrectValues()
        {
            // Test Black
            Color.Black.R.Should().Be((byte)0);
            Color.Black.G.Should().Be((byte)0);
            Color.Black.B.Should().Be((byte)0);
            Color.Black.A.Should().Be((byte)255);

            // Test White
            Color.White.R.Should().Be((byte)255);
            Color.White.G.Should().Be((byte)255);
            Color.White.B.Should().Be((byte)255);
            Color.White.A.Should().Be((byte)255);

            // Test Red
            Color.Red.R.Should().Be((byte)255);
            Color.Red.G.Should().Be((byte)0);
            Color.Red.B.Should().Be((byte)0);
            Color.Red.A.Should().Be((byte)255);

            // Test Green
            Color.Green.R.Should().Be((byte)0);
            Color.Green.G.Should().Be((byte)255);
            Color.Green.B.Should().Be((byte)0);
            Color.Green.A.Should().Be((byte)255);

            // Test Blue
            Color.Blue.R.Should().Be((byte)0);
            Color.Blue.G.Should().Be((byte)0);
            Color.Blue.B.Should().Be((byte)255);
            Color.Blue.A.Should().Be((byte)255);

            // Test Yellow
            Color.Yellow.R.Should().Be((byte)255);
            Color.Yellow.G.Should().Be((byte)255);
            Color.Yellow.B.Should().Be((byte)0);
            Color.Yellow.A.Should().Be((byte)255);

            // Test Transparent
            Color.Transparent.R.Should().Be((byte)0);
            Color.Transparent.G.Should().Be((byte)0);
            Color.Transparent.B.Should().Be((byte)0);
            Color.Transparent.A.Should().Be((byte)0);
        }

        [Theory]
        [InlineData(0, 0, 0, 0)]
        [InlineData(255, 255, 255, 255)]
        [InlineData(128, 64, 192, 200)]
        [InlineData(255, 0, 0, 128)]
        [InlineData(0, 255, 0, 64)]
        [InlineData(0, 0, 255, 32)]
        public void Constructor_ShouldHandleAllValues(byte r, byte g, byte b, byte a)
        {
            // Act
            var color = new Color(r, g, b, a);

            // Assert
            color.R.Should().Be(r);
            color.G.Should().Be(g);
            color.B.Should().Be(b);
            color.A.Should().Be(a);
        }

        [Fact]
        public void Constructor_ShouldHandleEdgeCases()
        {
            // Test minimum values
            var minColor = new Color(0, 0, 0, 0);
            minColor.R.Should().Be((byte)0);
            minColor.G.Should().Be((byte)0);
            minColor.B.Should().Be((byte)0);
            minColor.A.Should().Be((byte)0);

            // Test maximum values
            var maxColor = new Color(255, 255, 255, 255);
            maxColor.R.Should().Be((byte)255);
            maxColor.G.Should().Be((byte)255);
            maxColor.B.Should().Be((byte)255);
            maxColor.A.Should().Be((byte)255);
        }

        [Fact]
        public void Color_ShouldBeImmutable()
        {
            // Arrange
            var color = new Color(128, 64, 192, 200);

            // Act & Assert - Since it's readonly, we can't modify it directly
            // This test verifies the struct maintains its values
            var copy = color;
            copy.R.Should().Be(color.R);
            copy.G.Should().Be(color.G);
            copy.B.Should().Be(color.B);
            copy.A.Should().Be(color.A);
        }

        [Fact]
        public void SameColorValues_ShouldBeEqual()
        {
            // Arrange
            var color1 = new Color(128, 64, 192, 200);
            var color2 = new Color(128, 64, 192, 200);

            // Act & Assert
            color1.Should().Be(color2);
            color1.GetHashCode().Should().Be(color2.GetHashCode());
        }

        [Fact]
        public void DifferentColorValues_ShouldNotBeEqual()
        {
            // Arrange
            var color1 = new Color(128, 64, 192, 200);
            var color2 = new Color(128, 64, 192, 199); // Different alpha
            var color3 = new Color(128, 64, 191, 200); // Different blue
            var color4 = new Color(128, 63, 192, 200); // Different green
            var color5 = new Color(127, 64, 192, 200); // Different red

            // Act & Assert
            color1.Should().NotBe(color2);
            color1.Should().NotBe(color3);
            color1.Should().NotBe(color4);
            color1.Should().NotBe(color5);
        }

        [Fact]
        public void PredefinedColors_ShouldBeConsistent()
        {
            // Test that predefined colors are always the same instances
            var black1 = Color.Black;
            var black2 = Color.Black;
            black1.Should().Be(black2);

            var white1 = Color.White;
            var white2 = Color.White;
            white1.Should().Be(white2);

            var red1 = Color.Red;
            var red2 = Color.Red;
            red1.Should().Be(red2);
        }

        [Theory]
        [InlineData(255, 0, 0)] // Pure red
        [InlineData(0, 255, 0)] // Pure green
        [InlineData(0, 0, 255)] // Pure blue
        [InlineData(255, 255, 255)] // White
        [InlineData(0, 0, 0)] // Black
        [InlineData(128, 128, 128)] // Gray
        public void Constructor_WithoutAlpha_ShouldCreateOpaqueColor(byte r, byte g, byte b)
        {
            // Act
            var color = new Color(r, g, b);

            // Assert
            color.R.Should().Be(r);
            color.G.Should().Be(g);
            color.B.Should().Be(b);
            color.A.Should().Be((byte)255); // Should be fully opaque
        }
    }
}