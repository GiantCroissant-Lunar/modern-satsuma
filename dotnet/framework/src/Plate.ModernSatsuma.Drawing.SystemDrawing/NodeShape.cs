using System;
using Plate.ModernSatsuma.Abstractions;

namespace Plate.ModernSatsuma.Drawing.SystemDrawing
{
    /// <summary>
    /// A standard implementation of INodeShape (immutable).
    /// </summary>
    public sealed class NodeShape : INodeShape
    {
        /// <summary>
        /// The kind of the shape.
        /// </summary>
        public NodeShapeKind Kind { get; }

        /// <summary>
        /// The size of the shape, in graphic units.
        /// </summary>
        public Size2D Size { get; }

        private readonly Rectangle2D _rect;
        private readonly Point2D[] _points;

        public NodeShape(NodeShapeKind kind, Size2D size)
        {
            Kind = kind;
            Size = size;

            _rect = new Rectangle2D(-size.Width * 0.5, -size.Height * 0.5, size.Width, size.Height);
            
            switch (Kind)
            {
                case NodeShapeKind.Diamond:
                    _points = new[]
                    {
                        P(_rect, 0, 0.5),
                        P(_rect, 0.5, 1),
                        P(_rect, 1, 0.5),
                        P(_rect, 0.5, 0)
                    };
                    break;
                    
                case NodeShapeKind.Rectangle:
                    _points = new[]
                    {
                        new Point2D(_rect.Left, _rect.Top),
                        new Point2D(_rect.Left, _rect.Bottom),
                        new Point2D(_rect.Right, _rect.Bottom),
                        new Point2D(_rect.Right, _rect.Top)
                    };
                    break;
                    
                case NodeShapeKind.Triangle:
                    _points = new[]
                    {
                        P(_rect, 0.5, 0),
                        P(_rect, 0, 1),
                        P(_rect, 1, 1)
                    };
                    break;
                    
                case NodeShapeKind.UpsideDownTriangle:
                    _points = new[]
                    {
                        P(_rect, 0.5, 1),
                        P(_rect, 0, 0),
                        P(_rect, 1, 0)
                    };
                    break;
                    
                default:
                    _points = Array.Empty<Point2D>();
                    break;
            }
        }

        private static Point2D P(Rectangle2D rect, double x, double y)
        {
            return new Point2D(rect.Left + rect.Width * x, rect.Top + rect.Height * y);
        }

        public void Draw(IGraphicsContext graphics, IPen pen, IBrush brush)
        {
            switch (Kind)
            {
                case NodeShapeKind.Ellipse:
                    graphics.FillEllipse(brush, _rect);
                    graphics.DrawEllipse(pen, _rect);
                    break;

                default:
                    graphics.FillPolygon(brush, _points);
                    graphics.DrawPolygon(pen, _points);
                    break;
            }
        }

        public Point2D GetBoundary(double angle)
        {
            double cos = Math.Cos(angle);
            double sin = Math.Sin(angle);
            
            switch (Kind)
            {
                case NodeShapeKind.Ellipse:
                    return new Point2D(Size.Width * 0.5 * cos, Size.Height * 0.5 * sin);

                default:
                    // We have a polygon, try to intersect all sides with the ray
                    for (int i = 0; i < _points.Length; i++)
                    {
                        int i2 = (i + 1) % _points.Length;
                        double t = (_points[i].Y * cos - _points[i].X * sin) /
                                   ((_points[i2].X - _points[i].X) * sin - (_points[i2].Y - _points[i].Y) * cos);
                        
                        if (t >= 0 && t <= 1)
                        {
                            var result = new Point2D(
                                _points[i].X + t * (_points[i2].X - _points[i].X),
                                _points[i].Y + t * (_points[i2].Y - _points[i].Y)
                            );
                            
                            if (result.X * cos + result.Y * sin > 0)
                                return result;
                        }
                    }
                    return new Point2D(0, 0); // should not happen
            }
        }
    }
}
