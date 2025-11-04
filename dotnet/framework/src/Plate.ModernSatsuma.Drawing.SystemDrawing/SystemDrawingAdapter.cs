using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using Plate.ModernSatsuma.Abstractions;
using SysColor = System.Drawing.Color;
using SysPointF = System.Drawing.PointF;
using SysRectangleF = System.Drawing.RectangleF;
using AbsColor = Plate.ModernSatsuma.Abstractions.Color;

namespace Plate.ModernSatsuma.Drawing.SystemDrawing
{
    /// <summary>
    /// System.Drawing implementation of IGraphicsContext.
    /// </summary>
    public class SystemDrawingGraphicsContext : IGraphicsContext
    {
        private readonly Graphics _graphics;
        private bool _disposed;

        public SystemDrawingGraphicsContext(Graphics graphics)
        {
            _graphics = graphics ?? throw new ArgumentNullException(nameof(graphics));
        }

        public IGraphicsState Save()
        {
            var state = _graphics.Save();
            return new SystemDrawingGraphicsState(state);
        }

        public void Restore(IGraphicsState state)
        {
            if (state is SystemDrawingGraphicsState sysState)
            {
                _graphics.Restore(sysState.State);
            }
        }

        public void Translate(double x, double y)
        {
            _graphics.TranslateTransform((float)x, (float)y);
        }

        public void Scale(double scaleX, double scaleY)
        {
            _graphics.ScaleTransform((float)scaleX, (float)scaleY);
        }

        public void Clear(AbsColor color)
        {
            _graphics.Clear(ToSystemColor(color));
        }

        public void DrawLine(IPen pen, double x1, double y1, double x2, double y2)
        {
            using var sysPen = ToSystemPen(pen);
            _graphics.DrawLine(sysPen, (float)x1, (float)y1, (float)x2, (float)y2);
        }

        public void DrawEllipse(IPen pen, Rectangle2D rect)
        {
            using var sysPen = ToSystemPen(pen);
            _graphics.DrawEllipse(sysPen, ToSystemRectangle(rect));
        }

        public void FillEllipse(IBrush brush, Rectangle2D rect)
        {
            using var sysBrush = ToSystemBrush(brush);
            _graphics.FillEllipse(sysBrush, ToSystemRectangle(rect));
        }

        public void DrawPolygon(IPen pen, Point2D[] points)
        {
            using var sysPen = ToSystemPen(pen);
            _graphics.DrawPolygon(sysPen, ToSystemPoints(points));
        }

        public void FillPolygon(IBrush brush, Point2D[] points)
        {
            using var sysBrush = ToSystemBrush(brush);
            _graphics.FillPolygon(sysBrush, ToSystemPoints(points));
        }

        public void DrawString(string text, IFont font, IBrush brush, double x, double y, TextAlignment alignment = TextAlignment.Center)
        {
            using var sysFont = ToSystemFont(font);
            using var sysBrush = ToSystemBrush(brush);
            
            var format = new StringFormat();
            switch (alignment)
            {
                case TextAlignment.TopLeft:
                    format.Alignment = StringAlignment.Near;
                    format.LineAlignment = StringAlignment.Near;
                    break;
                case TextAlignment.TopCenter:
                    format.Alignment = StringAlignment.Center;
                    format.LineAlignment = StringAlignment.Near;
                    break;
                case TextAlignment.TopRight:
                    format.Alignment = StringAlignment.Far;
                    format.LineAlignment = StringAlignment.Near;
                    break;
                case TextAlignment.MiddleLeft:
                    format.Alignment = StringAlignment.Near;
                    format.LineAlignment = StringAlignment.Center;
                    break;
                case TextAlignment.Center:
                    format.Alignment = StringAlignment.Center;
                    format.LineAlignment = StringAlignment.Center;
                    break;
                case TextAlignment.MiddleRight:
                    format.Alignment = StringAlignment.Far;
                    format.LineAlignment = StringAlignment.Center;
                    break;
                case TextAlignment.BottomLeft:
                    format.Alignment = StringAlignment.Near;
                    format.LineAlignment = StringAlignment.Far;
                    break;
                case TextAlignment.BottomCenter:
                    format.Alignment = StringAlignment.Center;
                    format.LineAlignment = StringAlignment.Far;
                    break;
                case TextAlignment.BottomRight:
                    format.Alignment = StringAlignment.Far;
                    format.LineAlignment = StringAlignment.Far;
                    break;
            }

            _graphics.DrawString(text, sysFont, sysBrush, (float)x, (float)y, format);
        }

        public Size2D MeasureString(string text, IFont font)
        {
            using var sysFont = ToSystemFont(font);
            var size = _graphics.MeasureString(text, sysFont);
            return new Size2D(size.Width, size.Height);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
            }
        }

        // Conversion helpers
        private static SysColor ToSystemColor(AbsColor color)
        {
            return SysColor.FromArgb(color.A, color.R, color.G, color.B);
        }

        private static Pen ToSystemPen(IPen pen)
        {
            var sysPen = new Pen(ToSystemColor(pen.Color), (float)pen.Width);
            if (pen.HasArrowCap)
            {
                sysPen.CustomEndCap = new AdjustableArrowCap(3, 5);
            }
            return sysPen;
        }

        private static Brush ToSystemBrush(IBrush brush)
        {
            return new SolidBrush(ToSystemColor(brush.Color));
        }

        private static Font ToSystemFont(IFont font)
        {
            var style = FontStyle.Regular;
            if (font.Bold) style |= FontStyle.Bold;
            if (font.Italic) style |= FontStyle.Italic;
            return new Font(font.FontFamily, (float)font.Size, style);
        }

        private static SysRectangleF ToSystemRectangle(Rectangle2D rect)
        {
            return new SysRectangleF((float)rect.X, (float)rect.Y, (float)rect.Width, (float)rect.Height);
        }

        private static SysPointF[] ToSystemPoints(Point2D[] points)
        {
            var sysPoints = new SysPointF[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                sysPoints[i] = new SysPointF((float)points[i].X, (float)points[i].Y);
            }
            return sysPoints;
        }
    }

    internal class SystemDrawingGraphicsState : IGraphicsState
    {
        public GraphicsState State { get; }

        public SystemDrawingGraphicsState(GraphicsState state)
        {
            State = state;
        }

        public void Dispose()
        {
            // GraphicsState doesn't require disposal
        }
    }

    /// <summary>
    /// System.Drawing implementation of IRenderSurface.
    /// </summary>
    public class SystemDrawingRenderSurface : IRenderSurface
    {
        private readonly Bitmap _bitmap;
        private bool _disposed;

        public int Width => _bitmap.Width;
        public int Height => _bitmap.Height;

        public SystemDrawingRenderSurface(int width, int height)
        {
            _bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        }

        public IGraphicsContext GetGraphicsContext()
        {
            var graphics = Graphics.FromImage(_bitmap);
            return new SystemDrawingGraphicsContext(graphics);
        }

        public void Save(string path, Abstractions.ImageFormat format = Abstractions.ImageFormat.Png)
        {
            var sysFormat = ToSystemImageFormat(format);
            _bitmap.Save(path, sysFormat);
        }

        public void Save(Stream stream, Abstractions.ImageFormat format = Abstractions.ImageFormat.Png)
        {
            var sysFormat = ToSystemImageFormat(format);
            _bitmap.Save(stream, sysFormat);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _bitmap.Dispose();
                _disposed = true;
            }
        }

        private static System.Drawing.Imaging.ImageFormat ToSystemImageFormat(Abstractions.ImageFormat format)
        {
            return format switch
            {
                Abstractions.ImageFormat.Png => System.Drawing.Imaging.ImageFormat.Png,
                Abstractions.ImageFormat.Jpeg => System.Drawing.Imaging.ImageFormat.Jpeg,
                Abstractions.ImageFormat.Bmp => System.Drawing.Imaging.ImageFormat.Bmp,
                Abstractions.ImageFormat.Gif => System.Drawing.Imaging.ImageFormat.Gif,
                _ => System.Drawing.Imaging.ImageFormat.Png
            };
        }
    }

    /// <summary>
    /// System.Drawing implementation of graphics factory.
    /// </summary>
    public class SystemDrawingGraphicsFactory : IGraphicsFactory
    {
        public IPen CreatePen(AbsColor color, double width = 1.0, bool arrowCap = false)
        {
            return new SystemDrawingPen(color, width, arrowCap);
        }

        public IBrush CreateBrush(AbsColor color)
        {
            return new SystemDrawingBrush(color);
        }

        public IFont CreateFont(string fontFamily, double size, bool bold = false, bool italic = false)
        {
            return new SystemDrawingFont(fontFamily, size, bold, italic);
        }

        public IFont GetDefaultFont()
        {
            return new SystemDrawingFont(SystemFonts.DefaultFont.FontFamily.Name, SystemFonts.DefaultFont.Size, false, false);
        }
    }

    /// <summary>
    /// System.Drawing implementation of render surface factory.
    /// </summary>
    public class SystemDrawingRenderSurfaceFactory : IRenderSurfaceFactory
    {
        public IGraphicsFactory GraphicsFactory { get; }

        public SystemDrawingRenderSurfaceFactory()
        {
            GraphicsFactory = new SystemDrawingGraphicsFactory();
        }

        public IRenderSurface CreateSurface(int width, int height)
        {
            return new SystemDrawingRenderSurface(width, height);
        }
    }

    // Simple implementations of drawing primitives
    internal class SystemDrawingPen : IPen
    {
        public AbsColor Color { get; }
        public double Width { get; }
        public bool HasArrowCap { get; }

        public SystemDrawingPen(AbsColor color, double width, bool arrowCap)
        {
            Color = color;
            Width = width;
            HasArrowCap = arrowCap;
        }
    }

    internal class SystemDrawingBrush : IBrush
    {
        public AbsColor Color { get; }

        public SystemDrawingBrush(AbsColor color)
        {
            Color = color;
        }
    }

    internal class SystemDrawingFont : IFont
    {
        public string FontFamily { get; }
        public double Size { get; }
        public bool Bold { get; }
        public bool Italic { get; }

        public SystemDrawingFont(string fontFamily, double size, bool bold, bool italic)
        {
            FontFamily = fontFamily;
            Size = size;
            Bold = bold;
            Italic = italic;
        }
    }
}
