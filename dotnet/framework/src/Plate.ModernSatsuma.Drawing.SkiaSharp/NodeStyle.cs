using Plate.ModernSatsuma.Abstractions;

namespace Plate.ModernSatsuma.Drawing.SkiaSharp
{
    /// <summary>
    /// The visual style for a drawn node.
    /// </summary>
    public sealed class NodeStyle : INodeStyle
    {
        /// <summary>
        /// The default node shape.
        /// </summary>
        public static readonly INodeShape DefaultShape = new NodeShape(NodeShapeKind.Ellipse, new Size2D(10, 10));

        private readonly IGraphicsFactory _factory;

        /// <summary>
        /// The pen used to draw the node.
        /// Default: Black pen.
        /// </summary>
        public IPen Pen { get; set; }

        /// <summary>
        /// The brush used to draw the node.
        /// Default: White brush.
        /// </summary>
        public IBrush Brush { get; set; }

        /// <summary>
        /// The shape of the node.
        /// Default: DefaultShape.
        /// </summary>
        public INodeShape Shape { get; set; }

        /// <summary>
        /// The font used to draw the caption.
        /// Default: Arial 12pt.
        /// </summary>
        public IFont TextFont { get; set; }

        /// <summary>
        /// The brush used to draw the caption.
        /// Default: Black brush.
        /// </summary>
        public IBrush TextBrush { get; set; }

        public NodeStyle(IGraphicsFactory factory)
        {
            _factory = factory;
            Pen = factory.CreatePen(Abstractions.Color.Black);
            Brush = factory.CreateBrush(Abstractions.Color.White);
            Shape = DefaultShape;
            TextFont = factory.GetDefaultFont();
            TextBrush = factory.CreateBrush(Abstractions.Color.Black);
        }

        public void DrawNode(IGraphicsContext graphics, double x, double y, string? text)
        {
            var state = graphics.Save();
            graphics.Translate(x, y);
            Shape.Draw(graphics, Pen, Brush);
            
            if (!string.IsNullOrEmpty(text))
            {
                graphics.DrawString(text, TextFont, TextBrush, 0, 0, TextAlignment.Center);
            }
            
            graphics.Restore(state);
        }
    }
}
