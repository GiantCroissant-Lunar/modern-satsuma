using FluentAssertions;
using Plate.ModernSatsuma.Abstractions;
using Plate.ModernSatsuma.Drawing.SystemDrawing;
using System.IO;
using System.Runtime.Versioning;
using Xunit;

namespace Plate.ModernSatsuma.Drawing.SystemDrawing.Tests
{
    [SupportedOSPlatform("windows")]
    public class SystemDrawingIntegrationTests
    {
        private readonly SystemDrawingRenderSurfaceFactory _surfaceFactory;

        public SystemDrawingIntegrationTests()
        {
            _surfaceFactory = new SystemDrawingRenderSurfaceFactory();
        }

        [Fact]
        public void CompleteGraph_ShouldRenderCorrectly()
        {
            // Arrange
            var graph = new CompleteGraph(5, Directedness.Undirected);
            var factory = _surfaceFactory.GraphicsFactory;
            var drawer = new GraphDrawer(graph, factory)
            {
                NodePosition = node =>
                {
                    var index = graph.GetNodeIndex(node);
                    var angle = 2.0 * System.Math.PI * index / 5;
                    return new Point2D(200 + 100 * System.Math.Cos(angle), 200 + 100 * System.Math.Sin(angle));
                },
                NodeCaption = node => graph.GetNodeIndex(node).ToString()
            };

            // Act & Assert
            drawer.Invoking(d =>
            {
                using var surface = d.Draw(_surfaceFactory, 400, 400, Color.White);
                surface.Should().NotBeNull();
            }).Should().NotThrow();
        }

        [Fact]
        public void PathGraph_ShouldRenderCorrectly()
        {
            // Arrange
            var graph = new PathGraph(10, PathGraph.Topology.Cycle, Directedness.Undirected);
            var factory = _surfaceFactory.GraphicsFactory;
            var drawer = new GraphDrawer(graph, factory)
            {
                NodePosition = node =>
                {
                    var index = graph.GetNodeIndex(node);
                    return new Point2D(50 + index * 35, 150);
                },
                NodeCaption = node => graph.GetNodeIndex(node).ToString()
            };

            // Act & Assert
            drawer.Invoking(d =>
            {
                using var surface = d.Draw(_surfaceFactory, 400, 300, Color.White);
                surface.Should().NotBeNull();
            }).Should().NotThrow();
        }

        [Fact]
        public void CustomGraph_WithLayout_ShouldRenderCorrectly()
        {
            // Arrange
            var graph = new CustomGraph();
            var n1 = graph.AddNode();
            var n2 = graph.AddNode();
            var n3 = graph.AddNode();
            graph.AddArc(n1, n2, Directedness.Undirected);
            graph.AddArc(n2, n3, Directedness.Undirected);
            graph.AddArc(n3, n1, Directedness.Undirected);

            var factory = _surfaceFactory.GraphicsFactory;
            var positions = new System.Collections.Generic.Dictionary<Node, Point2D>
            {
                [n1] = new Point2D(200, 100),
                [n2] = new Point2D(300, 250),
                [n3] = new Point2D(100, 250)
            };

            var drawer = new GraphDrawer(graph, factory)
            {
                NodePosition = node => positions[node]
            };

            // Act & Assert
            drawer.Invoking(d =>
            {
                using var surface = d.Draw(_surfaceFactory, 400, 350, Color.White);
                surface.Should().NotBeNull();
            }).Should().NotThrow();
        }

        [Fact]
        public void StyledGraph_WithCustomColors_ShouldRenderCorrectly()
        {
            // Arrange
            var graph = new CompleteGraph(4, Directedness.Undirected);
            var factory = _surfaceFactory.GraphicsFactory;

            var colors = new[] { Color.Red, Color.Green, Color.Blue, Color.Yellow };
            var style1 = new NodeStyle(factory) { Brush = factory.CreateBrush(colors[0]) };
            var style2 = new NodeStyle(factory) { Brush = factory.CreateBrush(colors[1]) };
            var style3 = new NodeStyle(factory) { Brush = factory.CreateBrush(colors[2]) };
            var style4 = new NodeStyle(factory) { Brush = factory.CreateBrush(colors[3]) };
            var styles = new[] { style1, style2, style3, style4 };

            var drawer = new GraphDrawer(graph, factory)
            {
                NodePosition = node =>
                {
                    var index = graph.GetNodeIndex(node);
                    return new Point2D(100 + (index % 2) * 200, 100 + (index / 2) * 200);
                },
                NodeCaption = node => graph.GetNodeIndex(node).ToString(),
                NodeStyle = node => styles[graph.GetNodeIndex(node)]
            };

            // Act & Assert
            drawer.Invoking(d =>
            {
                using var surface = d.Draw(_surfaceFactory, 400, 400, Color.White);
                surface.Should().NotBeNull();
            }).Should().NotThrow();
        }

        [Fact]
        public void LargeGraph_ShouldCompleteWithoutTimeout()
        {
            // Arrange
            var graph = new CompleteGraph(50, Directedness.Undirected);
            var factory = _surfaceFactory.GraphicsFactory;
            var drawer = new GraphDrawer(graph, factory)
            {
                NodePosition = node =>
                {
                    var index = graph.GetNodeIndex(node);
                    var row = index / 10;
                    var col = index % 10;
                    return new Point2D(50 + col * 40, 50 + row * 40);
                }
            };

            // Act & Assert
            drawer.Invoking(d =>
            {
                using var surface = d.Draw(_surfaceFactory, 500, 250, Color.White);
                surface.Should().NotBeNull();
            }).Should().NotThrow();
        }
    }
}
