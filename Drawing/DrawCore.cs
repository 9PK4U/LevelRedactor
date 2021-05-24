using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Diagnostics;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LevelRedactor.Drawing
{
    public class DrawCore : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private int lastZIndex;
        private Point lastPosition;
        private Point CursorPoint;
        private Figure currentFigure;
        private Primitive currentPrimitive;
        private ObservableCollection<Figure> figures = new();

        public Canvas Canvas { get; init; }
        public Figure CurrentFigure 
        {
            get => currentFigure; 
            set
            {
                currentFigure = value;
                OnPropertyChanged("CurrentFigure");
            }
        }
        public Primitive CurrentPrimitive 
        {
            get => currentPrimitive;
            set
            {
                currentPrimitive = value;
                OnPropertyChanged("CurrentPrimitive");
            }
        }
        public Action Action { get; set; }
        public ObservableCollection<Figure> Figures { get => figures; private set => figures = value; }

        public DrawCore(Canvas canvas)
        {
            Canvas = canvas;

            Canvas.MouseLeftButtonDown += Canvas_MouseLeftButtonDown;
            Canvas.MouseMove += Canvas_MouseMove;
            Canvas.MouseLeftButtonUp += Canvas_MouseLeftButtonUp;

            Action = new() { Type = ActionTypes.Draw, DrawingType = DrawingType.Rect, Context = new() { BorderColor = Colors.Black, FillColor = Colors.Red, BorderWidth = 2 } };
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point currentPoint = e.GetPosition(Canvas);

            switch (Action.Type)
            {
                case ActionTypes.Draw:
                    Draw(currentPoint);
                    break;
                case ActionTypes.Move:
                    if ((CurrentFigure = GetFigure(currentPoint)) != null)
                    {
                        lastPosition = currentFigure.DrawPoint;
                        CursorPoint = new(currentPoint.X - CurrentFigure.DrawPoint.X, currentPoint.Y - CurrentFigure.DrawPoint.Y);
                    }
                    break;
                case ActionTypes.Resize:
                    break;
                case ActionTypes.Unit:
                    UnitFigures(currentPoint);
                    break;
                case ActionTypes.Link:
                    break;
                case ActionTypes.Choice:
                    if ((CurrentFigure = GetFigure(currentPoint)) != null)
                        CurrentPrimitive = GetPrimitive(currentPoint) ?? CurrentPrimitive;
                    break;
                default:
                    break;
            }
        }
        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            Point currentPoint = e.GetPosition(Canvas);

            switch (Action.Type)
            {
                case ActionTypes.Draw:
                    if (e.LeftButton == MouseButtonState.Pressed)
                        Draw(currentPoint, false);
                    break;
                case ActionTypes.Move:
                    if (CurrentFigure != null && e.LeftButton == MouseButtonState.Pressed)
                        Move(currentPoint);
                    break;
                case ActionTypes.Resize:
                    break;
                case ActionTypes.Unit:
                    break;
                case ActionTypes.Link:
                    break;
                case ActionTypes.Choice:
                    break;
                default:
                    break;
            }
        }
        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            switch (Action.Type)
            {
                case ActionTypes.Draw:
                    CurrentFigure.DrawPoint = CurrentFigure.Primitives[0].GeometryDrawing.Bounds.TopLeft;
                    figures.Add(CurrentFigure);
                    break;
                case ActionTypes.Move:
                    if (CurrentFigure != null)
                    {
                        //Point newDrawPoint = GetDrawPoint();
                        //MakeNewPrimitive(newDrawPoint);
                        MakeNewPrimitive(CurrentFigure.DrawPoint);
                        //CurrentFigure.DrawPoint = newDrawPoint;
                    }
                    break;
                case ActionTypes.Resize:
                    break;
                case ActionTypes.Unit:
                    break;
                case ActionTypes.Link:
                    break;
                case ActionTypes.Choice:
                    break;
                default:
                    break;
            }
        }
        private void Draw(Point point, bool isBegin = true)
        { 
            if (isBegin)
            {
                CurrentFigure = new Figure
                {
                    DrawPoint = point,
                    ZIndex = lastZIndex++
                };
                CurrentFigure.Primitives.Add(new Primitive()
                {
                    GeometryDrawing = new GeometryDrawing
                    {
                        Brush = new SolidColorBrush(Action.Context.FillColor),
                        Pen = new Pen(new SolidColorBrush(Action.Context.BorderColor), Action.Context.BorderWidth)
                    }
                });

                Canvas.Children.Add(CurrentFigure);

                Canvas.SetTop(CurrentFigure, point.Y);
                Canvas.SetLeft(CurrentFigure, point.X);
            }
            else
            {
                switch (Action.DrawingType)
                {
                    case DrawingType.Rect:
                        CurrentFigure.Primitives[0].Type = "Прямоугольник";
                        CurrentFigure.Primitives[0].GeometryDrawing.Geometry =
                            new RectangleGeometry(MakeRightRect(CurrentFigure.DrawPoint, point));
                        break;
                    case DrawingType.Line:
                        CurrentFigure.Primitives[0].Type = "Линия";
                        CurrentFigure.Primitives[0].GeometryDrawing.Geometry =
                            new LineGeometry(CurrentFigure.DrawPoint, point);
                        break;
                    case DrawingType.Ellipse:
                        CurrentFigure.Primitives[0].Type = "Овал";
                        CurrentFigure.Primitives[0].GeometryDrawing.Geometry =
                            new EllipseGeometry(MakeRightRect(CurrentFigure.DrawPoint, point));
                        break;
                    case DrawingType.Triengle:
                        CurrentFigure.Primitives[0].Type = "Треугольник";
                        CurrentFigure.Primitives[0].GeometryDrawing.Geometry =
                            MakeRightTriangle(CurrentFigure.DrawPoint, point);
                        break;
                    default:
                        break;
                }

                Canvas.SetTop(CurrentFigure, CurrentFigure.Primitives[0].GeometryDrawing.Bounds.Top);
                Canvas.SetLeft(CurrentFigure, CurrentFigure.Primitives[0].GeometryDrawing.Bounds.Left);
                Canvas.SetZIndex(CurrentFigure, CurrentFigure.ZIndex); // ??
            }
            
        }
        private void Move(Point point)
        {
            double width = CurrentFigure.ActualWidth;
            double height = CurrentFigure.ActualHeight;

            double x_pos = (int)(point.X - CursorPoint.X);
            double y_pos = (int)(point.Y - CursorPoint.Y);

            if (x_pos >= 0 && y_pos >= 0
                && x_pos + width <= Canvas.ActualWidth
                && y_pos + height <= Canvas.ActualHeight)
            {
                Canvas.SetTop(CurrentFigure, y_pos);
                Canvas.SetLeft(CurrentFigure, x_pos);
                CurrentFigure.DrawPoint = GetDrawPoint();
            }
        }
        private void UnitFigures(Point point)
        {
            if (GetFigure(point) is Figure temp && temp != currentFigure && temp != null && currentFigure != null)
            {

                foreach (Primitive primitive in currentFigure.Primitives)
                {
                    temp.Primitives.Add(primitive);
                }

                Point newFigurePos = new();

                if (temp.DrawPoint.X < currentFigure.DrawPoint.X)
                {
                    newFigurePos.X = temp.DrawPoint.X;
                }
                else
                {
                    newFigurePos.X = currentFigure.DrawPoint.X;
                }

                if (temp.DrawPoint.Y < currentFigure.DrawPoint.Y)
                {
                    newFigurePos.Y = temp.DrawPoint.Y;
                }
                else
                {
                    newFigurePos.Y = currentFigure.DrawPoint.Y;
                }

                temp.DrawPoint = newFigurePos;

                Canvas.Children.Remove(currentFigure);
                figures.Remove(currentFigure);

                Canvas.SetLeft(temp, newFigurePos.X);
                Canvas.SetTop(temp, newFigurePos.Y);

                currentFigure = temp;
            }
            else
            {
                MessageBox.Show("Действие невозможно");
            }
        }
        private void LinkFigures(Point point)
        {

        }

        private static Rect MakeRightRect(Point p1, Point p2)
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
        private static PathGeometry MakeRightTriangle(Point p1, Point p2)
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
        private void MakeNewPrimitive(Point currentPoint)
        {
            double differenceX = (currentPoint.X - lastPosition.X);
            double differenceY = (currentPoint.Y - lastPosition.Y);

            foreach (Primitive primitive in CurrentFigure.Primitives)
            {
                if (primitive.GeometryDrawing.Geometry is RectangleGeometry rectangle)
                {
                    double newX = rectangle.Rect.Left + differenceX;
                    double newY = rectangle.Rect.Top + differenceY;
                    primitive.GeometryDrawing.Geometry =
                        new RectangleGeometry(new Rect(newX, newY,
                        rectangle.Rect.Width, rectangle.Rect.Height));
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
                if (primitive.GeometryDrawing.Geometry is PathGeometry triengle)
                {
                    double newX = triengle.Bounds.Left + differenceX;
                    double newY = triengle.Bounds.Top + differenceY;

                    Point point1 = new(newX, newY);
                    Point point2 = new(newX + triengle.Bounds.Width, newY + triengle.Bounds.Height);

                    primitive.GeometryDrawing.Geometry = MakeRightTriangle(point1, point2);
                }
            }
        }
        private Figure GetFigure(in Point point)
        {
            if (VisualTreeHelper.HitTest(Canvas, point) is var hit && hit == null || hit.VisualHit == Canvas)
            {
                return null;
            }

            try
            {
                return (Figure)((Image)hit.VisualHit).Parent;
            }
            catch (Exception)
            { }


            return null;
        }
        private Primitive GetPrimitive(Point point)
        {
            for (int i = currentFigure.Primitives.Count - 1; i >= 0; i--)
            {
                if (currentFigure.Primitives[i].GeometryDrawing.Geometry.FillContains(point))
                    return currentFigure.Primitives[i];
            }
            
            return null;
        }
        private Point GetDrawPoint() => new(Canvas.GetLeft(CurrentFigure), Canvas.GetTop(CurrentFigure));
        
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
