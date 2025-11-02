using FluentAssertions;
using Plate.ModernSatsuma.Drawing.SystemDrawing;
using Xunit;

namespace Plate.ModernSatsuma.Drawing.SystemDrawing.Tests
{
    public class SystemDrawingRenderSurfaceFactoryTests
    {
        private readonly SystemDrawingRenderSurfaceFactory _factory;

        public SystemDrawingRenderSurfaceFactoryTests()
        {
            _factory = new SystemDrawingRenderSurfaceFactory();
        }

        [Fact]
        public void Constructor_ShouldInitializeGraphicsFactory()
        {
            // Assert
            _factory.GraphicsFactory.Should().NotBeNull();
            _factory.GraphicsFactory.Should().BeOfType<SystemDrawingGraphicsFactory>();
        }

        [Fact]
        public void CreateSurface_WithValidDimensions_ShouldReturnValidSurface()
        {
            // Arrange
            var width = 800;
            var height = 600;

            // Act
            using var surface = _factory.CreateSurface(width, height);

            // Assert
            surface.Should().NotBeNull();
            surface.Width.Should().Be(width);
            surface.Height.Should().Be(height);
        }

        [Theory]
        [InlineData(100, 100)]
        [InlineData(1920, 1080)]
        [InlineData(1, 1)]
        [InlineData(4000, 3000)]
        public void CreateSurface_WithVariousDimensions_ShouldMatchDimensions(int width, int height)
        {
            // Act
            using var surface = _factory.CreateSurface(width, height);

            // Assert
            surface.Should().NotBeNull();
            surface.Width.Should().Be(width);
            surface.Height.Should().Be(height);
        }

        [Fact]
        public void CreateSurface_Multiple_ShouldCreateIndependentSurfaces()
        {
            // Act
            using var surface1 = _factory.CreateSurface(100, 100);
            using var surface2 = _factory.CreateSurface(200, 200);

            // Assert
            surface1.Should().NotBeNull();
            surface2.Should().NotBeNull();
            surface1.Should().NotBeSameAs(surface2);
            surface1.Width.Should().NotBe(surface2.Width);
            surface1.Height.Should().NotBe(surface2.Height);
        }

        [Fact]
        public void GraphicsFactory_ShouldBeSharedAcrossSurfaces()
        {
            // Act
            var factory1 = _factory.GraphicsFactory;
            var factory2 = _factory.GraphicsFactory;

            // Assert
            factory1.Should().BeSameAs(factory2);
        }
    }
}
