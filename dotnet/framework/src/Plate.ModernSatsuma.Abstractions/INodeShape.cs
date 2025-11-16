namespace Plate.ModernSatsuma.Abstractions
{
    /// <summary>
    /// The possible types of standard node shapes.
    /// </summary>
    public enum NodeShapeKind
    {
        Diamond,
        Ellipse,
        Rectangle,
        Triangle,
        UpsideDownTriangle
    }

    /// <summary>
    /// Abstract interface for shapes used to draw graph nodes.
    /// </summary>
    public interface INodeShape
    {
        /// <summary>
        /// The size of the shape, in graphic units.
        /// </summary>
        Size2D Size { get; }

        /// <summary>
        /// Draws the shape. The center of the shape is at (0, 0).
        /// </summary>
        void Draw(IGraphicsContext graphics, IPen pen, IBrush brush);

        /// <summary>
        /// Returns the furthermost point of the shape boundary at the given angular position.
        /// The center of the shape is at (0, 0).
        /// </summary>
        Point2D GetBoundary(double angle);
    }

    /// <summary>
    /// The visual style for a drawn node.
    /// </summary>
    public interface INodeStyle
    {
        /// <summary>
        /// The pen used to draw the node outline.
        /// </summary>
        IPen Pen { get; }

        /// <summary>
        /// The brush used to fill the node.
        /// </summary>
        IBrush Brush { get; }

        /// <summary>
        /// The shape of the node.
        /// </summary>
        INodeShape Shape { get; }

        /// <summary>
        /// The font used to draw the caption.
        /// </summary>
        IFont TextFont { get; }

        /// <summary>
        /// The brush used to draw the caption.
        /// </summary>
        IBrush TextBrush { get; }

        /// <summary>
        /// Draws the node at the specified position with optional text.
        /// </summary>
        void DrawNode(IGraphicsContext graphics, double x, double y, string? text);
    }
}
