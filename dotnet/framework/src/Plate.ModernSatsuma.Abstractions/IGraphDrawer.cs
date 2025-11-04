using System;

namespace Plate.ModernSatsuma.Abstractions
{
    // Note: Node and Arc types are from Plate.ModernSatsuma core library
    /// <summary>
    /// Interface for drawing graphs using a rendering backend.
    /// </summary>
    public interface IGraphDrawer
    {
        /// <summary>
        /// Assigns a position to a node.
        /// </summary>
        Func<Node, Point2D>? NodePosition { get; set; }

        /// <summary>
        /// Assigns a caption to a node.
        /// </summary>
        Func<Node, string>? NodeCaption { get; set; }

        /// <summary>
        /// Assigns a style to a node.
        /// </summary>
        Func<Node, INodeStyle>? NodeStyle { get; set; }

        /// <summary>
        /// Assigns a pen to each arc.
        /// </summary>
        Func<Arc, IPen>? ArcPen { get; set; }

        /// <summary>
        /// Draws the graph to the specified graphics context.
        /// </summary>
        void Draw(IGraphicsContext graphics);

        /// <summary>
        /// Draws the graph to fit within the specified bounding box.
        /// </summary>
        void Draw(IGraphicsContext graphics, Rectangle2D boundingBox);

        /// <summary>
        /// Draws the graph to a new surface and returns it.
        /// </summary>
        IRenderSurface Draw(IRenderSurfaceFactory factory, int width, int height, Color backColor, bool antialias = true);
    }
}
