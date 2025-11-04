using System;
using System.Linq;
using Plate.ModernSatsuma.Abstractions;

namespace Plate.ModernSatsuma.Drawing.SkiaSharp
{
    /// <summary>
    /// Draws a graph using the SkiaSharp backend.
    /// 
    /// Example:
    /// <code>
    /// var graph = new CompleteGraph(7);
    /// var layout = new ForceDirectedLayout(graph);
    /// layout.Run();
    /// 
    /// var factory = new SkiaSharpRenderSurfaceFactory();
    /// var nodeShape = new NodeShape(NodeShapeKind.Diamond, new Size2D(40, 40));
    /// var nodeStyle = new NodeStyle(factory.GraphicsFactory) 
    /// { 
    ///     Brush = factory.GraphicsFactory.CreateBrush(Color.Yellow), 
    ///     Shape = nodeShape 
    /// };
    /// 
    /// var drawer = new GraphDrawer(graph, factory)
    /// {
    ///     NodePosition = node => new Point2D(layout.NodePositions[node].X, layout.NodePositions[node].Y),
    ///     NodeCaption = node => graph.GetNodeIndex(node).ToString(),
    ///     NodeStyle = node => nodeStyle
    /// };
    /// 
    /// using var surface = drawer.Draw(factory, 300, 300, Color.White);
    /// surface.Save("graph.png");
    /// </code>
    /// </summary>
    public sealed class GraphDrawer : IGraphDrawer
    {
        private readonly IGraph _graph;
        private readonly IGraphicsFactory _factory;

        /// <summary>
        /// Assigns a position to a node.
        /// </summary>
        public Func<Node, Point2D>? NodePosition { get; set; }

        /// <summary>
        /// Assigns a caption to a node.
        /// Default: returns empty string for each node.
        /// </summary>
        public Func<Node, string>? NodeCaption { get; set; }

        /// <summary>
        /// Assigns a style to a node.
        /// Default: returns a default NodeStyle for each node.
        /// Warning: This function is called multiple times (at least twice for each arc).
        /// Avoid creating a NodeStyle object on each call - return pre-made objects instead.
        /// </summary>
        public Func<Node, INodeStyle>? NodeStyle { get; set; }

        /// <summary>
        /// Assigns a pen to each arc.
        /// Default: assigns DirectedPen to directed arcs, and UndirectedPen to edges.
        /// </summary>
        public Func<Arc, IPen>? ArcPen { get; set; }

        /// <summary>
        /// The pen used for directed arcs.
        /// Default: a black pen with an arrow end.
        /// Unused if ArcPen is set to a custom value.
        /// </summary>
        public IPen DirectedPen { get; set; }

        /// <summary>
        /// The pen used for undirected arcs (edges).
        /// Default: a black pen.
        /// Unused if ArcPen is set to a custom value.
        /// </summary>
        public IPen UndirectedPen { get; set; }

        public GraphDrawer(IGraph graph, IGraphicsFactory factory)
        {
            _graph = graph ?? throw new ArgumentNullException(nameof(graph));
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));

            NodeCaption = _ => "";
            var defaultNodeStyle = new NodeStyle(factory);
            NodeStyle = _ => defaultNodeStyle;
            DirectedPen = factory.CreatePen(Abstractions.Color.Black, 1.0, arrowCap: true);
            UndirectedPen = factory.CreatePen(Abstractions.Color.Black);
            ArcPen = arc => _graph.IsEdge(arc) ? UndirectedPen : DirectedPen;
        }

        /// <summary>
        /// Draws the graph to the specified graphics context.
        /// </summary>
        public void Draw(IGraphicsContext graphics)
        {
            Draw(graphics, transform: null);
        }

        /// <summary>
        /// Draws the graph to fit within the specified bounding box.
        /// </summary>
        public void Draw(IGraphicsContext graphics, Rectangle2D boundingBox)
        {
            if (!_graph.Nodes().Any()) return;
            if (NodePosition == null) throw new InvalidOperationException("NodePosition must be set before drawing");

            double maxShapeWidth = 0, maxShapeHeight = 0;
            double xmin = double.PositiveInfinity, ymin = double.PositiveInfinity;
            double xmax = double.NegativeInfinity, ymax = double.NegativeInfinity;

            foreach (var node in _graph.Nodes())
            {
                var style = NodeStyle?.Invoke(node);
                if (style != null)
                {
                    var size = style.Shape.Size;
                    maxShapeWidth = Math.Max(maxShapeWidth, size.Width);
                    maxShapeHeight = Math.Max(maxShapeHeight, size.Height);
                }

                var pos = NodePosition(node);
                xmin = Math.Min(xmin, pos.X);
                xmax = Math.Max(xmax, pos.X);
                ymin = Math.Min(ymin, pos.Y);
                ymax = Math.Max(ymax, pos.Y);
            }

            double xspan = xmax - xmin;
            if (xspan == 0) xspan = 1;
            double yspan = ymax - ymin;
            if (yspan == 0) yspan = 1;

            // Calculate transformation
            var state = graphics.Save();
            graphics.Translate(maxShapeWidth * 0.6, maxShapeHeight * 0.6);
            graphics.Scale(
                (boundingBox.Width - maxShapeWidth * 1.2) / xspan,
                (boundingBox.Height - maxShapeHeight * 1.2) / yspan
            );
            graphics.Translate(-xmin, -ymin);

            Draw(graphics, transform: null);
            graphics.Restore(state);
        }

        /// <summary>
        /// Draws the graph to a new surface and returns it.
        /// </summary>
        public IRenderSurface Draw(IRenderSurfaceFactory factory, int width, int height, Abstractions.Color backColor, bool antialias = true)
        {
            var surface = factory.CreateSurface(width, height);
            using (var graphics = surface.GetGraphicsContext())
            {
                graphics.Clear(backColor);
                Draw(graphics, new Rectangle2D(0, 0, width, height));
            }
            return surface;
        }

        private void Draw(IGraphicsContext graphics, Func<Point2D, Point2D>? transform)
        {
            if (NodePosition == null) throw new InvalidOperationException("NodePosition must be set before drawing");

            // Draw arcs
            foreach (var arc in _graph.Arcs())
            {
                var u = _graph.U(arc);
                var v = _graph.V(arc);
                var arcPos0 = NodePosition(u);
                var arcPos1 = NodePosition(v);

                if (transform != null)
                {
                    arcPos0 = transform(arcPos0);
                    arcPos1 = transform(arcPos1);
                }

                // Arc should run between shape boundaries
                double angle = Math.Atan2(arcPos1.Y - arcPos0.Y, arcPos1.X - arcPos0.X);
                var uStyle = NodeStyle?.Invoke(u);
                var vStyle = NodeStyle?.Invoke(v);
                
                var boundary0 = uStyle?.Shape.GetBoundary(angle) ?? new Point2D(0, 0);
                var boundary1 = vStyle?.Shape.GetBoundary(angle + Math.PI) ?? new Point2D(0, 0);

                var pen = ArcPen?.Invoke(arc) ?? DirectedPen;
                graphics.DrawLine(pen,
                    arcPos0.X + boundary0.X, arcPos0.Y + boundary0.Y,
                    arcPos1.X + boundary1.X, arcPos1.Y + boundary1.Y);
            }

            // Draw nodes
            foreach (var node in _graph.Nodes())
            {
                var nodePos = NodePosition(node);
                if (transform != null)
                {
                    nodePos = transform(nodePos);
                }

                var style = NodeStyle?.Invoke(node);
                var caption = NodeCaption?.Invoke(node) ?? "";
                style?.DrawNode(graphics, nodePos.X, nodePos.Y, caption);
            }
        }
    }
}
