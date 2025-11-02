using FluentAssertions;
using Plate.ModernSatsuma.Abstractions;
using Plate.ModernSatsuma.Drawing.SystemDrawing;
using System;
using Xunit;

namespace Plate.ModernSatsuma.Drawing.SystemDrawing.Tests
{
    public class GraphDrawerTests
    {
        private readonly SystemDrawingRenderSurfaceFactory _surfaceFactory;

        public GraphDrawerTests()
        {
            _surfaceFactory = new SystemDrawingRenderSurfaceFactory();
        }

        [Fact]
        public void Constructor_ShouldSetDefaults()
        {
            // Arrange
            var graph = new CompleteGraph(3, Directedness.Undirected);
            var factory = _surfaceFactory.GraphicsFactory;

            // Act
            var drawer = new GraphDrawer(graph, factory);

            // Assert
            drawer.Should().NotBeNull();
            drawer.DirectedPen.Should().NotBeNull();
            drawer.UndirectedPen.Should().NotBeNull();
        }

        [Fact]
        public void Draw_WithValidGraph_ShouldNotThrow()
        {
            // Arrange
            var graph = new CompleteGraph(3, Directedness.Undirected);
            var factory = _surfaceFactory.GraphicsFactory;
            var drawer = new GraphDrawer(graph, factory)
            {
                NodePosition = node => new Point2D(100 + graph.GetNodeIndex(node) * 50, 100)
            };

            // Act & Assert
            drawer.Invoking(d =>
            {
                using var surface = d.Draw(_surfaceFactory, 400, 300, Color.White);
            }).Should().NotThrow();
        }

        [Fact]
        public void Draw_ToSurface_ShouldReturnValidSurface()
        {
            // Arrange
            var graph = new CompleteGraph(3, Directedness.Undirected);
            var factory = _surfaceFactory.GraphicsFactory;
            var drawer = new GraphDrawer(graph, factory)
            {
                NodePosition = node => new Point2D(100, 100)
            };

            // Act
            using var surface = drawer.Draw(_surfaceFactory, 400, 300, Color.White);

            // Assert
            surface.Should().NotBeNull();
            surface.Width.Should().Be(400);
            surface.Height.Should().Be(300);
        }

        [Fact]
        public void Draw_WithoutNodePosition_ShouldThrow()
        {
            // Arrange
            var graph = new CompleteGraph(3, Directedness.Undirected);
            var factory = _surfaceFactory.GraphicsFactory;
            var drawer = new GraphDrawer(graph, factory);
            // NodePosition not set

            // Act & Assert
            drawer.Invoking(d =>
            {
                using var surface = d.Draw(_surfaceFactory, 400, 300, Color.White);
            }).Should().Throw<Exception>();
        }

        [Fact]
        public void Draw_EmptyGraph_ShouldNotThrow()
        {
            // Arrange
            var graph = new CompleteGraph(0, Directedness.Undirected);
            var factory = _surfaceFactory.GraphicsFactory;
            var drawer = new GraphDrawer(graph, factory)
            {
                NodePosition = node => new Point2D(100, 100)
            };

            // Act & Assert
            drawer.Invoking(d =>
            {
                using var surface = d.Draw(_surfaceFactory, 400, 300, Color.White);
            }).Should().NotThrow();
        }

        [Fact]
        public void Draw_SingleNode_ShouldNotThrow()
        {
            // Arrange
            var graph = new CompleteGraph(1, Directedness.Undirected);
            var factory = _surfaceFactory.GraphicsFactory;
            var drawer = new GraphDrawer(graph, factory)
            {
                NodePosition = node => new Point2D(200, 150),
                NodeCaption = node => "Node"
            };

            // Act & Assert
            drawer.Invoking(d =>
            {
                using var surface = d.Draw(_surfaceFactory, 400, 300, Color.White);
            }).Should().NotThrow();
        }

        [Fact]
        public void Properties_ShouldBeSettable()
        {
            // Arrange
            var graph = new CompleteGraph(3, Directedness.Undirected);
            var factory = _surfaceFactory.GraphicsFactory;
            var drawer = new GraphDrawer(graph, factory);

            var customDirectedPen = factory.CreatePen(Color.Blue, 2.0, arrowCap: true);
            var customUndirectedPen = factory.CreatePen(Color.Green, 1.5);

            // Act
            drawer.DirectedPen = customDirectedPen;
            drawer.UndirectedPen = customUndirectedPen;
            drawer.NodePosition = node => new Point2D(100, 100);
            drawer.NodeCaption = node => "Test";
            drawer.NodeStyle = node => new NodeStyle(factory);
            drawer.ArcPen = arc => customDirectedPen;

            // Assert
            drawer.DirectedPen.Should().BeSameAs(customDirectedPen);
            drawer.UndirectedPen.Should().BeSameAs(customUndirectedPen);
            drawer.NodePosition.Should().NotBeNull();
            drawer.NodeCaption.Should().NotBeNull();
            drawer.NodeStyle.Should().NotBeNull();
            drawer.ArcPen.Should().NotBeNull();
        }
    }
}
