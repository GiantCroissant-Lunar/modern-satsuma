using System;

namespace Plate.ModernSatsuma.Abstractions
{
    /// <summary>
    /// Represents a 2D point with double-precision coordinates.
    /// </summary>
    public readonly struct Point2D : IEquatable<Point2D>
    {
        public double X { get; }
        public double Y { get; }

        public Point2D(double x, double y)
        {
            X = x;
            Y = y;
        }

        public bool Equals(Point2D other) => X == other.X && Y == other.Y;
        public override bool Equals(object? obj) => obj is Point2D other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(X, Y);
        public static bool operator ==(Point2D left, Point2D right) => left.Equals(right);
        public static bool operator !=(Point2D left, Point2D right) => !left.Equals(right);

        public override string ToString() => $"({X}, {Y})";

        public static Point2D operator +(Point2D a, Point2D b) => new(a.X + b.X, a.Y + b.Y);
        public static Point2D operator -(Point2D a, Point2D b) => new(a.X - b.X, a.Y - b.Y);
        public static Point2D operator *(Point2D p, double scalar) => new(p.X * scalar, p.Y * scalar);

        public double Distance(Point2D other)
        {
            double dx = X - other.X;
            double dy = Y - other.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }

    /// <summary>
    /// Represents a size in 2D space.
    /// </summary>
    public readonly struct Size2D
    {
        public double Width { get; }
        public double Height { get; }

        public Size2D(double width, double height)
        {
            Width = width;
            Height = height;
        }
    }

    /// <summary>
    /// Represents a rectangle in 2D space.
    /// </summary>
    public readonly struct Rectangle2D
    {
        public double X { get; }
        public double Y { get; }
        public double Width { get; }
        public double Height { get; }

        public Rectangle2D(double x, double y, double width, double height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public double Left => X;
        public double Top => Y;
        public double Right => X + Width;
        public double Bottom => Y + Height;
    }

    /// <summary>
    /// Represents a color with RGBA components.
    /// </summary>
    public readonly struct Color
    {
        public byte R { get; }
        public byte G { get; }
        public byte B { get; }
        public byte A { get; }

        public Color(byte r, byte g, byte b, byte a = 255)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public static Color Black => new(0, 0, 0);
        public static Color White => new(255, 255, 255);
        public static Color Red => new(255, 0, 0);
        public static Color Green => new(0, 255, 0);
        public static Color Blue => new(0, 0, 255);
        public static Color Yellow => new(255, 255, 0);
        public static Color Transparent => new(0, 0, 0, 0);
    }

    /// <summary>
    /// Graphics state that can be saved and restored.
    /// </summary>
    public interface IGraphicsState : IDisposable
    {
    }

    /// <summary>
    /// Platform-agnostic graphics context for drawing operations.
    /// </summary>
    public interface IGraphicsContext : IDisposable
    {
        /// <summary>
        /// Saves the current graphics state and returns a token that can be used to restore it.
        /// </summary>
        IGraphicsState Save();

        /// <summary>
        /// Restores the graphics state.
        /// </summary>
        void Restore(IGraphicsState state);

        /// <summary>
        /// Translates the coordinate system.
        /// </summary>
        void Translate(double x, double y);

        /// <summary>
        /// Scales the coordinate system.
        /// </summary>
        void Scale(double scaleX, double scaleY);

        /// <summary>
        /// Clears the entire surface with the specified color.
        /// </summary>
        void Clear(Color color);

        /// <summary>
        /// Draws a line between two points.
        /// </summary>
        void DrawLine(IPen pen, double x1, double y1, double x2, double y2);

        /// <summary>
        /// Draws an ellipse inside the specified rectangle.
        /// </summary>
        void DrawEllipse(IPen pen, Rectangle2D rect);

        /// <summary>
        /// Fills an ellipse inside the specified rectangle.
        /// </summary>
        void FillEllipse(IBrush brush, Rectangle2D rect);

        /// <summary>
        /// Draws a polygon defined by an array of points.
        /// </summary>
        void DrawPolygon(IPen pen, Point2D[] points);

        /// <summary>
        /// Fills a polygon defined by an array of points.
        /// </summary>
        void FillPolygon(IBrush brush, Point2D[] points);

        /// <summary>
        /// Draws text at the specified location.
        /// </summary>
        void DrawString(string text, IFont font, IBrush brush, double x, double y, TextAlignment alignment = TextAlignment.Center);

        /// <summary>
        /// Measures the size of the specified text.
        /// </summary>
        Size2D MeasureString(string text, IFont font);
    }

    /// <summary>
    /// Text alignment options.
    /// </summary>
    public enum TextAlignment
    {
        TopLeft,
        TopCenter,
        TopRight,
        MiddleLeft,
        Center,
        MiddleRight,
        BottomLeft,
        BottomCenter,
        BottomRight
    }

    /// <summary>
    /// Represents a pen for drawing lines and outlines.
    /// </summary>
    public interface IPen
    {
        Color Color { get; }
        double Width { get; }
        bool HasArrowCap { get; }
    }

    /// <summary>
    /// Represents a brush for filling shapes.
    /// </summary>
    public interface IBrush
    {
        Color Color { get; }
    }

    /// <summary>
    /// Represents a font for text rendering.
    /// </summary>
    public interface IFont
    {
        string FontFamily { get; }
        double Size { get; }
        bool Bold { get; }
        bool Italic { get; }
    }

    /// <summary>
    /// Factory for creating graphics objects (pens, brushes, fonts).
    /// </summary>
    public interface IGraphicsFactory
    {
        IPen CreatePen(Color color, double width = 1.0, bool arrowCap = false);
        IBrush CreateBrush(Color color);
        IFont CreateFont(string fontFamily, double size, bool bold = false, bool italic = false);
        IFont GetDefaultFont();
    }

    /// <summary>
    /// Surface that can be rendered to and saved.
    /// </summary>
    public interface IRenderSurface : IDisposable
    {
        int Width { get; }
        int Height { get; }
        
        /// <summary>
        /// Gets a graphics context for drawing on this surface.
        /// </summary>
        IGraphicsContext GetGraphicsContext();

        /// <summary>
        /// Saves the surface to a file.
        /// </summary>
        void Save(string path, ImageFormat format = ImageFormat.Png);

        /// <summary>
        /// Saves the surface to a stream.
        /// </summary>
        void Save(System.IO.Stream stream, ImageFormat format = ImageFormat.Png);
    }

    /// <summary>
    /// Supported image formats for saving.
    /// </summary>
    public enum ImageFormat
    {
        Png,
        Jpeg,
        Bmp,
        Gif
    }

    /// <summary>
    /// Factory for creating render surfaces.
    /// </summary>
    public interface IRenderSurfaceFactory
    {
        IRenderSurface CreateSurface(int width, int height);
        IGraphicsFactory GraphicsFactory { get; }
    }
}
