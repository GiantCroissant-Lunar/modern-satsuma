using FluentAssertions;
using Plate.ModernSatsuma.Abstractions;
using Plate.ModernSatsuma.Drawing.SystemDrawing;
using System;
using System.IO;
using Xunit;

namespace Plate.ModernSatsuma.Drawing.SystemDrawing.Tests
{
    public class SystemDrawingRenderSurfaceTests : IDisposable
    {
        private readonly SystemDrawingRenderSurfaceFactory _surfaceFactory;
        private readonly string _tempDirectory;

        public SystemDrawingRenderSurfaceTests()
        {
            _surfaceFactory = new SystemDrawingRenderSurfaceFactory();
            _tempDirectory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempDirectory);
        }

        public void Dispose()
        {
            if (Directory.Exists(_tempDirectory))
            {
                Directory.Delete(_tempDirectory, true);
            }
        }

        [Fact]
        public void CreateSurface_WithValidSize_ShouldReturnValidSurface()
        {
            // Act
            using var surface = _surfaceFactory.CreateSurface(800, 600);

            // Assert
            surface.Should().NotBeNull();
            surface.Width.Should().Be(800);
            surface.Height.Should().Be(600);
        }

        [Fact]
        public void CreateSurface_WithZeroSize_ShouldReturnValidSurface()
        {
            // Act
            using var surface = _surfaceFactory.CreateSurface(0, 0);

            // Assert
            surface.Should().NotBeNull();
            surface.Width.Should().Be(0);
            surface.Height.Should().Be(0);
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(100, 100)]
        [InlineData(1920, 1080)]
        [InlineData(4000, 3000)]
        public void CreateSurface_WithVariousSizes_ShouldReturnValidSurface(int width, int height)
        {
            // Act
            using var surface = _surfaceFactory.CreateSurface(width, height);

            // Assert
            surface.Should().NotBeNull();
            surface.Width.Should().Be(width);
            surface.Height.Should().Be(height);
        }

        [Fact]
        public void GetGraphicsContext_ShouldReturnValidContext()
        {
            // Arrange
            using var surface = _surfaceFactory.CreateSurface(100, 100);

            // Act
            using var context = surface.GetGraphicsContext();

            // Assert
            context.Should().NotBeNull();
        }

        [Fact]
        public void GraphicsFactory_ShouldBeAvailable()
        {
            // Act & Assert
            _surfaceFactory.GraphicsFactory.Should().NotBeNull();
        }

        [Fact]
        public void SaveToFile_WithValidPath_ShouldCreateFile()
        {
            // Arrange
            using var surface = _surfaceFactory.CreateSurface(100, 100);
            var filePath = System.IO.Path.Combine(_tempDirectory, "test.png");

            // Act
            surface.Save(filePath);

            // Assert
            File.Exists(filePath).Should().BeTrue();
            new FileInfo(filePath).Length.Should().BeGreaterThan(0);
        }

        [Fact]
        public void SaveToFile_WithDifferentFormats_ShouldCreateFiles()
        {
            // Arrange
            using var surface = _surfaceFactory.CreateSurface(100, 100);

            // Act & Assert
            var pngPath = System.IO.Path.Combine(_tempDirectory, "test.png");
            surface.Save(pngPath, ImageFormat.Png);
            File.Exists(pngPath).Should().BeTrue();

            var jpgPath = System.IO.Path.Combine(_tempDirectory, "test.jpg");
            surface.Save(jpgPath, ImageFormat.Jpeg);
            File.Exists(jpgPath).Should().BeTrue();

            var bmpPath = System.IO.Path.Combine(_tempDirectory, "test.bmp");
            surface.Save(bmpPath, ImageFormat.Bmp);
            File.Exists(bmpPath).Should().BeTrue();

            var gifPath = System.IO.Path.Combine(_tempDirectory, "test.gif");
            surface.Save(gifPath, ImageFormat.Gif);
            File.Exists(gifPath).Should().BeTrue();
        }

        [Fact]
        public void SaveToFile_WithInvalidPath_ShouldThrowException()
        {
            // Arrange
            using var surface = _surfaceFactory.CreateSurface(100, 100);
            var invalidPath = "|<>?*";

            // Act & Assert
            Assert.ThrowsAny<Exception>(() => surface.Save(invalidPath));
        }

        [Fact]
        public void SaveToFile_WithNullPath_ShouldThrowArgumentNullException()
        {
            // Arrange
            using var surface = _surfaceFactory.CreateSurface(100, 100);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => surface.Save((string)null!));
        }

        [Fact]
        public void SaveToStream_WithValidStream_ShouldWriteData()
        {
            // Arrange
            using var surface = _surfaceFactory.CreateSurface(100, 100);
            using var stream = new MemoryStream();

            // Act
            surface.Save(stream);

            // Assert
            stream.Length.Should().BeGreaterThan(0);
            stream.Position = 0;
            stream.Capacity.Should().BeGreaterThan(0);
        }

        [Fact]
        public void SaveToStream_WithDifferentFormats_ShouldWriteData()
        {
            // Arrange
            using var surface = _surfaceFactory.CreateSurface(100, 100);

            // Act & Assert
            using var pngStream = new MemoryStream();
            surface.Save(pngStream, ImageFormat.Png);
            pngStream.Length.Should().BeGreaterThan(0);

            using var jpgStream = new MemoryStream();
            surface.Save(jpgStream, ImageFormat.Jpeg);
            jpgStream.Length.Should().BeGreaterThan(0);

            using var bmpStream = new MemoryStream();
            surface.Save(bmpStream, ImageFormat.Bmp);
            bmpStream.Length.Should().BeGreaterThan(0);

            using var gifStream = new MemoryStream();
            surface.Save(gifStream, ImageFormat.Gif);
            gifStream.Length.Should().BeGreaterThan(0);
        }

        [Fact]
        public void SaveToStream_WithNullStream_ShouldThrowArgumentNullException()
        {
            // Arrange
            using var surface = _surfaceFactory.CreateSurface(100, 100);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => surface.Save((System.IO.Stream)null!));
        }

        [Fact]
        public void SaveToStream_WithDisposedStream_ShouldThrowException()
        {
            // Arrange
            using var surface = _surfaceFactory.CreateSurface(100, 100);
            var disposedStream = new MemoryStream();
            disposedStream.Dispose();

            // Act & Assert
            Assert.ThrowsAny<Exception>(() => surface.Save(disposedStream));
        }

        [Fact]
        public void MultipleSurfaceCreation_ShouldWorkCorrectly()
        {
            // Act
            using var surface1 = _surfaceFactory.CreateSurface(100, 100);
            using var surface2 = _surfaceFactory.CreateSurface(200, 150);
            using var surface3 = _surfaceFactory.CreateSurface(300, 200);

            // Assert
            surface1.Width.Should().Be(100);
            surface1.Height.Should().Be(100);
            surface2.Width.Should().Be(200);
            surface2.Height.Should().Be(150);
            surface3.Width.Should().Be(300);
            surface3.Height.Should().Be(200);
        }

        [Fact]
        public void LargeSurface_ShouldHandleCorrectly()
        {
            // Arrange
            var width = 5000;
            var height = 4000;

            // Act
            using var surface = _surfaceFactory.CreateSurface(width, height);

            // Assert
            surface.Should().NotBeNull();
            surface.Width.Should().Be(width);
            surface.Height.Should().Be(height);
        }

        [Fact]
        public void SaveToFile_WithEmptyPath_ShouldThrowException()
        {
            // Arrange
            using var surface = _surfaceFactory.CreateSurface(100, 100);

            // Act & Assert
            Assert.ThrowsAny<Exception>(() => surface.Save(string.Empty));
        }

        [Fact]
        public void SaveToStream_WithReadOnlyStream_ShouldThrowException()
        {
            // Arrange
            using var surface = _surfaceFactory.CreateSurface(100, 100);
            using var readOnlyStream = new MemoryStream(new byte[100], false);

            // Act & Assert
            Assert.ThrowsAny<Exception>(() => surface.Save(readOnlyStream));
        }

        [Fact]
        public void Dispose_ShouldWorkCorrectly()
        {
            // Arrange
            var surface = _surfaceFactory.CreateSurface(100, 100);
            var context = surface.GetGraphicsContext();

            // Act
            surface.Dispose();

            // Assert - Should not throw
            surface.Invoking(s => s.Save(new MemoryStream())).Should().Throw<ObjectDisposedException>();
        }

        [Fact]
        public void GetGraphicsContext_AfterDispose_ShouldThrowObjectDisposedException()
        {
            // Arrange
            var surface = _surfaceFactory.CreateSurface(100, 100);
            surface.Dispose();

            // Act & Assert
            Assert.Throws<ObjectDisposedException>(() => surface.GetGraphicsContext());
        }
    }
}