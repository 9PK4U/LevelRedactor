using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace LevelRedactor.Drawing
{
    public static class PrimitiveBuilder
    {
        public static void AddSegment(Primitive primitive, Point point)
        {
            ((PathGeometry)primitive.GeometryDrawing.Geometry).Figures[0].Segments.Add(new LineSegment(point, true));
        }
        public static Rect MakeRightRect(Point p1, Point p2)
        {
            if (p1.X < p2.X)
            {
                if (p1.Y > p2.Y)
                    return new Rect(p1, p2);
                else
                    return new Rect(new Point(p1.X, p2.Y), new Point(p2.X, p1.Y));
            }
            else
            {
                if (p1.Y > p2.Y)
                    return new Rect(new Point(p2.X, p1.Y), new Point(p1.X, p2.Y));
                else
                    return new Rect(p2, p1);
            }
        }
        public static PathGeometry MakeRightTriangle(Point p1, Point p2)
        {
            PathFigure pf_triangle = new();
            pf_triangle.IsClosed = true;

            Rect rect = MakeRightRect(p1, p2);

            Point tr_point = rect.TopLeft;
            tr_point.X += rect.Width / 2;

            pf_triangle.StartPoint = rect.BottomLeft;
            pf_triangle.Segments.Add(new LineSegment(rect.BottomRight, true));
            pf_triangle.Segments.Add(new LineSegment(tr_point, true));

            PathGeometry triangle = new();
            triangle.Figures.Add(pf_triangle);

            return triangle;
        }
        public static void MakeNewPrimitives(Figure figure, Point lastPosition)
        {
            double differenceX = (figure.DrawPoint.X - lastPosition.X);
            double differenceY = (figure.DrawPoint.Y - lastPosition.Y);

            foreach (Primitive primitive in figure.Primitives)
            {
                if (primitive.GeometryDrawing.Geometry is RectangleGeometry rectangle)
                {
                    double posX = rectangle.Rect.TopLeft.X + differenceX;
                    double posY = rectangle.Rect.TopLeft.Y + differenceY;

                    int angle = primitive.Angle;
                    primitive.Angle = 0;

                    primitive.GeometryDrawing.Geometry = new RectangleGeometry(new Rect(posX, posY, rectangle.Rect.Width, rectangle.Rect.Height));
                    primitive.Angle = angle;
                }
                if (primitive.GeometryDrawing.Geometry is LineGeometry line)
                {
                    Point newStartPoint = new(line.StartPoint.X + differenceX,
                        line.StartPoint.Y + differenceY);
                    Point newEndPoint = new(line.EndPoint.X + differenceX,
                        line.EndPoint.Y + differenceY);

                    primitive.GeometryDrawing.Geometry = new LineGeometry(newStartPoint, newEndPoint);
                }
                if (primitive.GeometryDrawing.Geometry is EllipseGeometry ellipse)
                {
                    double newX = ellipse.Bounds.Left + differenceX;
                    double newY = ellipse.Bounds.Top + differenceY;

                    Rect newRect = new(newX, newY, ellipse.Bounds.Width, ellipse.Bounds.Height);

                    primitive.GeometryDrawing.Geometry = new EllipseGeometry(newRect);
                }
                if (primitive.GeometryDrawing.Geometry is PathGeometry path)
                {
                    if (primitive.Type == "Треугольник")
                    {
                        double newX = path.Bounds.Left + differenceX;
                        double newY = path.Bounds.Top + differenceY;

                        Point point1 = new(newX, newY);
                        Point point2 = new(newX + path.Bounds.Width, newY + path.Bounds.Height);

                        primitive.GeometryDrawing.Geometry = MakeRightTriangle(point1, point2);
                    }
                    else
                    {
                        PathFigure pf = ((PathGeometry)primitive.GeometryDrawing.Geometry).Figures[0];

                        pf.StartPoint = new(pf.StartPoint.X + differenceX, pf.StartPoint.Y + differenceY);
                        foreach (LineSegment item in pf.Segments)
                        {
                            item.Point = new(item.Point.X + differenceX, item.Point.Y + differenceY);
                        }

                        primitive.GeometryDrawing.Geometry = new PathGeometry();
                        ((PathGeometry)primitive.GeometryDrawing.Geometry).Figures.Add(pf);
                    }
                }
            }
        }
    }
}
