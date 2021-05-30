using System;
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

        public Primitive(PrimitiveData primitiveData)
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
                GeometryDrawing.Geometry = new RectangleGeometry(primitiveData.Bounds);
            }
            if (primitiveData.Type == "Ellipse")
            {
                Type = "Овал";
                GeometryDrawing.Geometry = new EllipseGeometry(primitiveData.Bounds);
            }
            if (primitiveData.Type == "Line")
            {
                Type = "Линия";
                GeometryDrawing.Geometry = new LineGeometry(primitiveData.Points[0], primitiveData.Points[1]);
            }
            if (primitiveData.Type == "Triangle")
            {
                Type = "Треугольник";

                PathFigure pf_triangle = new();
                pf_triangle.IsClosed = true;
                pf_triangle.StartPoint = primitiveData.Points[0];

                for (int i = 1; i < primitiveData.Points.Count; i++)
                    pf_triangle.Segments.Add(new LineSegment(primitiveData.Points[i], true));

                GeometryDrawing.Geometry = new PathGeometry();
                ((PathGeometry)GeometryDrawing.Geometry).Figures.Add(pf_triangle);
            }
        }
        public Primitive()
        { }

        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        public object Clone()
        {
            return new Primitive() { Type = Type, GeometryDrawing = GeometryDrawing.Clone(), Angle = Angle };
        }
    }
}
