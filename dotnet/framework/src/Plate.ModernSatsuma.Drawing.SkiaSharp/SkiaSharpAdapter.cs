using System;
using System.IO;
using Plate.ModernSatsuma.Abstractions;
using SkiaSharp;
using AbsColor = Plate.ModernSatsuma.Abstractions.Color;

namespace Plate.ModernSatsuma.Drawing.SkiaSharp
{
    /// <summary>
    /// SkiaSharp implementation of IGraphicsContext.
    /// </summary>
    public class SkiaSharpGraphicsContext : IGraphicsContext
    {
        private readonly SKCanvas _canvas;
        private bool _disposed;

        public SkiaSharpGraphicsContext(SKCanvas canvas)
        {
            _canvas = canvas ?? throw new ArgumentNullException(nameof(canvas));
        }

        public IGraphicsState Save()
        {
            var count = _canvas.Save();
            return new SkiaSharpGraphicsState(count);
        }

        public void Restore(IGraphicsState state)
        {
            if (state is SkiaSharpGraphicsState skiaState)
            {
                _canvas.RestoreToCount(skiaState.SaveCount);
            }
        }

        public void Translate(double x, double y)
        {
            _canvas.Translate((float)x, (float)y);
        }

        public void Scale(double scaleX, double scaleY)
        {
            _canvas.Scale((float)scaleX, (float)scaleY);
        }

        public void Clear(AbsColor color)
        {
            _canvas.Clear(ToSkiaColor(color));
        }

        public void DrawLine(IPen pen, double x1, double y1, double x2, double y2)
        {
            using var paint = CreateSkiaPaint(pen);
            _canvas.DrawLine((float)x1, (float)y1, (float)x2, (float)y2, paint);
            
            // Draw arrow cap if needed
            if (pen.HasArrowCap)
            {
                DrawArrowCap(x1, y1, x2, y2, pen);
            }
        }

        public void DrawEllipse(IPen pen, Rectangle2D rect)
        {
            using var paint = CreateSkiaPaint(pen);
            var skRect = ToSkiaRect(rect);
            _canvas.DrawOval(skRect, paint);
        }

        public void FillEllipse(IBrush brush, Rectangle2D rect)
        {
            using var paint = CreateSkiaPaint(brush);
            var skRect = ToSkiaRect(rect);
            _canvas.DrawOval(skRect, paint);
        }

        public void DrawPolygon(IPen pen, Point2D[] points)
        {
            using var paint = CreateSkiaPaint(pen);
            using var path = CreatePath(points, close: true);
            _canvas.DrawPath(path, paint);
        }

        public void FillPolygon(IBrush brush, Point2D[] points)
        {
            using var paint = CreateSkiaPaint(brush);
            using var path = CreatePath(points, close: true);
            _canvas.DrawPath(path, paint);
        }

        public void DrawString(string text, IFont font, IBrush brush, double x, double y, TextAlignment alignment = TextAlignment.Center)
        {
            using var paint = CreateSkiaPaint(brush);
            using var typeface = CreateTypeface(font);
            using var skFont = new SKFont(typeface, (float)font.Size);
            
            paint.IsAntialias = true;
            
            // Measure text for alignment
            var width = skFont.MeasureText(text);
            var metrics = skFont.Metrics;
            var bounds = new SKRect(0, metrics.Ascent, width, metrics.Descent);
            
            float dx = 0, dy = 0;
            
            // Horizontal alignment
            switch (alignment)
            {
                case TextAlignment.TopLeft:
                case TextAlignment.MiddleLeft:
                case TextAlignment.BottomLeft:
                    dx = 0;
                    break;
                    
                case TextAlignment.TopCenter:
                case TextAlignment.Center:
                case TextAlignment.BottomCenter:
                    dx = -bounds.Width / 2;
                    break;
                    
                case TextAlignment.TopRight:
                case TextAlignment.MiddleRight:
                case TextAlignment.BottomRight:
                    dx = -bounds.Width;
                    break;
            }
            
            // Vertical alignment (note: text is drawn from baseline)
            switch (alignment)
            {
                case TextAlignment.TopLeft:
                case TextAlignment.TopCenter:
                case TextAlignment.TopRight:
                    dy = -bounds.Top;
                    break;
                    
                case TextAlignment.MiddleLeft:
                case TextAlignment.Center:
                case TextAlignment.MiddleRight:
                    dy = -bounds.MidY;
                    break;
                    
                case TextAlignment.BottomLeft:
                case TextAlignment.BottomCenter:
                case TextAlignment.BottomRight:
                    dy = -bounds.Bottom;
                    break;
            }
            
            _canvas.DrawText(text, (float)x + dx, (float)y + dy, skFont, paint);
        }

        public Size2D MeasureString(string text, IFont font)
        {
            using var typeface = CreateTypeface(font);
            using var skFont = new SKFont(typeface, (float)font.Size);
            
            var width = skFont.MeasureText(text);
            var metrics = skFont.Metrics;
            var height = metrics.Descent - metrics.Ascent;
            
            return new Size2D(width, height);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
            }
        }

        // Helper methods
        private static SKColor ToSkiaColor(AbsColor color)
        {
            return new SKColor(color.R, color.G, color.B, color.A);
        }

        private static SKRect ToSkiaRect(Rectangle2D rect)
        {
            return new SKRect((float)rect.X, (float)rect.Y, (float)rect.Right, (float)rect.Bottom);
        }

        private static SKPaint CreateSkiaPaint(IPen pen)
        {
            return new SKPaint
            {
                Color = ToSkiaColor(pen.Color),
                StrokeWidth = (float)pen.Width,
                Style = SKPaintStyle.Stroke,
                IsAntialias = true
            };
        }

        private static SKPaint CreateSkiaPaint(IBrush brush)
        {
            return new SKPaint
            {
                Color = ToSkiaColor(brush.Color),
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };
        }

        private static SKPath CreatePath(Point2D[] points, bool close)
        {
            var path = new SKPath();
            if (points.Length > 0)
            {
                path.MoveTo((float)points[0].X, (float)points[0].Y);
                for (int i = 1; i < points.Length; i++)
                {
                    path.LineTo((float)points[i].X, (float)points[i].Y);
                }
                if (close)
                {
                    path.Close();
                }
            }
            return path;
        }

        private static SKTypeface CreateTypeface(IFont font)
        {
            var style = SKFontStyle.Normal;
            
            if (font.Bold && font.Italic)
                style = SKFontStyle.BoldItalic;
            else if (font.Bold)
                style = SKFontStyle.Bold;
            else if (font.Italic)
                style = SKFontStyle.Italic;
            
            return SKTypeface.FromFamilyName(font.FontFamily, style);
        }

        private void DrawArrowCap(double x1, double y1, double x2, double y2, IPen pen)
        {
            // Calculate arrow direction
            double dx = x2 - x1;
            double dy = y2 - y1;
            double length = Math.Sqrt(dx * dx + dy * dy);
            
            if (length < 0.001) return;
            
            // Normalize direction
            dx /= length;
            dy /= length;
            
            // Arrow dimensions
            double arrowLength = Math.Min(10.0, length * 0.3);
            double arrowWidth = arrowLength * 0.5;
            
            // Calculate arrow points
            double perpX = -dy;
            double perpY = dx;
            
            var arrowPoints = new[]
            {
                new Point2D(x2, y2),
                new Point2D(x2 - dx * arrowLength + perpX * arrowWidth, y2 - dy * arrowLength + perpY * arrowWidth),
                new Point2D(x2 - dx * arrowLength - perpX * arrowWidth, y2 - dy * arrowLength - perpY * arrowWidth)
            };
            
            using var paint = new SKPaint
            {
                Color = ToSkiaColor(pen.Color),
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };
            
            using var path = CreatePath(arrowPoints, close: true);
            _canvas.DrawPath(path, paint);
        }
    }

    internal class SkiaSharpGraphicsState : IGraphicsState
    {
        public int SaveCount { get; }

        public SkiaSharpGraphicsState(int saveCount)
        {
            SaveCount = saveCount;
        }

        public void Dispose()
        {
            // No cleanup needed
        }
    }

    /// <summary>
    /// SkiaSharp implementation of IRenderSurface.
    /// </summary>
    public class SkiaSharpRenderSurface : IRenderSurface
    {
        private readonly SKBitmap _bitmap;
        private readonly SKSurface _surface;
        private bool _disposed;

        public int Width => _bitmap.Width;
        public int Height => _bitmap.Height;

        public SkiaSharpRenderSurface(int width, int height)
        {
            _bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
            _surface = SKSurface.Create(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul));
        }

        public IGraphicsContext GetGraphicsContext()
        {
            return new SkiaSharpGraphicsContext(_surface.Canvas);
        }

        public void Save(string path, Abstractions.ImageFormat format = Abstractions.ImageFormat.Png)
        {
            using var image = _surface.Snapshot();
            using var data = image.Encode(ToSkiaFormat(format), 90);
            using var stream = File.OpenWrite(path);
            data.SaveTo(stream);
        }

        public void Save(Stream stream, Abstractions.ImageFormat format = Abstractions.ImageFormat.Png)
        {
            using var image = _surface.Snapshot();
            using var data = image.Encode(ToSkiaFormat(format), 90);
            data.SaveTo(stream);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _surface.Dispose();
                _bitmap.Dispose();
                _disposed = true;
            }
        }

        private static SKEncodedImageFormat ToSkiaFormat(Abstractions.ImageFormat format)
        {
            return format switch
            {
                Abstractions.ImageFormat.Png => SKEncodedImageFormat.Png,
                Abstractions.ImageFormat.Jpeg => SKEncodedImageFormat.Jpeg,
                Abstractions.ImageFormat.Bmp => SKEncodedImageFormat.Bmp,
                Abstractions.ImageFormat.Gif => SKEncodedImageFormat.Gif,
                _ => SKEncodedImageFormat.Png
            };
        }
    }

    /// <summary>
    /// SkiaSharp implementation of graphics factory.
    /// </summary>
    public class SkiaSharpGraphicsFactory : IGraphicsFactory
    {
        public IPen CreatePen(AbsColor color, double width = 1.0, bool arrowCap = false)
        {
            return new SkiaSharpPen(color, width, arrowCap);
        }

        public IBrush CreateBrush(AbsColor color)
        {
            return new SkiaSharpBrush(color);
        }

        public IFont CreateFont(string fontFamily, double size, bool bold = false, bool italic = false)
        {
            return new SkiaSharpFont(fontFamily, size, bold, italic);
        }

        public IFont GetDefaultFont()
        {
            return new SkiaSharpFont("Arial", 12, false, false);
        }
    }

    /// <summary>
    /// SkiaSharp implementation of render surface factory.
    /// </summary>
    public class SkiaSharpRenderSurfaceFactory : IRenderSurfaceFactory
    {
        public IGraphicsFactory GraphicsFactory { get; }

        public SkiaSharpRenderSurfaceFactory()
        {
            GraphicsFactory = new SkiaSharpGraphicsFactory();
        }

        public IRenderSurface CreateSurface(int width, int height)
        {
            return new SkiaSharpRenderSurface(width, height);
        }
    }

    // Simple implementations of drawing primitives
    internal class SkiaSharpPen : IPen
    {
        public AbsColor Color { get; }
        public double Width { get; }
        public bool HasArrowCap { get; }

        public SkiaSharpPen(AbsColor color, double width, bool arrowCap)
        {
            Color = color;
            Width = width;
            HasArrowCap = arrowCap;
        }
    }

    internal class SkiaSharpBrush : IBrush
    {
        public AbsColor Color { get; }

        public SkiaSharpBrush(AbsColor color)
        {
            Color = color;
        }
    }

    internal class SkiaSharpFont : IFont
    {
        public string FontFamily { get; }
        public double Size { get; }
        public bool Bold { get; }
        public bool Italic { get; }

        public SkiaSharpFont(string fontFamily, double size, bool bold, bool italic)
        {
            FontFamily = fontFamily;
            Size = size;
            Bold = bold;
            Italic = italic;
        }
    }
}
