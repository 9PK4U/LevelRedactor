using System;
using System.Windows;
using System.Windows.Media;
using System.ComponentModel;
using LevelRedactor.Parser.Models;
using System.Runtime.CompilerServices;

namespace LevelRedactor.Drawing
{
    public class Primitive : ICloneable, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private int angle = 0;

        public string Type { get; set; }
        public int Angle 
        {
            get => angle;
            set
            {
                angle = value;
                GeometryDrawing.Geometry.Transform = new RotateTransform(value, GeometryDrawing.Geometry.Bounds.Width / 2, GeometryDrawing.Geometry.Bounds.Height / 2);
                OnPropertyChanged("Angle");
            }
        }
        public GeometryDrawing GeometryDrawing { get; set; }

        public Primitive(PrimitiveData primitiveData, Point figureDrawPoint)
        {
            GeometryDrawing = new GeometryDrawing()
            {
                Brush = new SolidColorBrush((Color)ColorConverter
                    .ConvertFromString(primitiveData.FillColor)),
                Pen = new Pen(new SolidColorBrush((Color)ColorConverter
                    .ConvertFromString(primitiveData.BorderColor)), primitiveData.BorderWidth)
            };

            if (primitiveData.Type == "Rectangle")
            {
                Type = "Прямоугольник";
                Point LT = new (primitiveData.Bounds.Left + figureDrawPoint.X, primitiveData.Bounds.Top + figureDrawPoint.Y);
                Point RB = new (primitiveData.Bounds.Right + figureDrawPoint.X, primitiveData.Bounds.Bottom + figureDrawPoint.Y);
                Rect rect = new (LT, RB);
                GeometryDrawing.Geometry = new RectangleGeometry(rect);
            }
            if (primitiveData.Type == "Ellipse")
            {
                Type = "Овал";
                Point LT = new(primitiveData.Bounds.Left + figureDrawPoint.X, primitiveData.Bounds.Top + figureDrawPoint.Y);
                Point RB = new(primitiveData.Bounds.Right + figureDrawPoint.X, primitiveData.Bounds.Bottom + figureDrawPoint.Y);
                Rect rect = new(LT, RB);
                GeometryDrawing.Geometry = new EllipseGeometry(rect);
            }
            if (primitiveData.Type == "Line")
            {
                Type = "Линия";
                Point startPoint = new (primitiveData.Points[0].X + figureDrawPoint.X, primitiveData.Points[0].Y + figureDrawPoint.Y);
                Point endPoint = new (primitiveData.Points[1].X + figureDrawPoint.X, primitiveData.Points[1].Y + figureDrawPoint.Y);
                GeometryDrawing.Geometry = new LineGeometry(startPoint, endPoint);
            }
            if (primitiveData.Type == "Triangle")
            {
                Type = "Треугольник";

                PathFigure pf_triangle = new();
                pf_triangle.IsClosed = true;
                pf_triangle.StartPoint = new (primitiveData.Points[0].X + figureDrawPoint.X, primitiveData.Points[0].Y + figureDrawPoint.Y);

                for (int i = 1; i < primitiveData.Points.Count; i++)
                    pf_triangle.Segments.Add(new LineSegment(new (primitiveData.Points[i].X + figureDrawPoint.X, primitiveData.Points[i].Y + figureDrawPoint.Y), true));

                GeometryDrawing.Geometry = new PathGeometry();
                ((PathGeometry)GeometryDrawing.Geometry).Figures.Add(pf_triangle);
            }
            if (primitiveData.Type == "Polygon")
            {
                Type = "Многоугольник";

                PathFigure pf = new();
                pf.IsClosed = true;
                pf.StartPoint = new(primitiveData.Points[0].X + figureDrawPoint.X, primitiveData.Points[0].Y + figureDrawPoint.Y);

                for (int i = 1; i < primitiveData.Points.Count; i++)
                    pf.Segments.Add(new LineSegment(new(primitiveData.Points[i].X + figureDrawPoint.X, primitiveData.Points[i].Y + figureDrawPoint.Y), true));

                GeometryDrawing.Geometry = new PathGeometry();
                ((PathGeometry)GeometryDrawing.Geometry).Figures.Add(pf);
            }
            if (primitiveData.Type == "Polyline")
            {
                Type = "Ломаная";

                PathFigure pf = new();
                pf.IsClosed = false;
                pf.StartPoint = new(primitiveData.Points[0].X + figureDrawPoint.X, primitiveData.Points[0].Y + figureDrawPoint.Y);

                for (int i = 1; i < primitiveData.Points.Count; i++)
                    pf.Segments.Add(new LineSegment(new(primitiveData.Points[i].X + figureDrawPoint.X, primitiveData.Points[i].Y + figureDrawPoint.Y), true));

                GeometryDrawing.Geometry = new PathGeometry();
                ((PathGeometry)GeometryDrawing.Geometry).Figures.Add(pf);
            }
        }
        public Primitive()
        { }

        public void OnPropertyChanged([CallerMemberName] string prop = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        public object Clone() => 
            new Primitive() { Type = Type, GeometryDrawing = GeometryDrawing.Clone(), Angle = Angle };
    }
}
