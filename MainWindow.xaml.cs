using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LevelRedactor.Drawing;

namespace LevelRedactor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public enum Tools
    {
        Rect,
        Ellipse,
        Line,
        Triengle,
        Move,
        Arrow
    }

    public enum ActionTypes
    {
        Draw,
        Drag,
        Resize,
        Unit,
        Link,
        Choice,
        None
    }

    public enum HitTypes
    {
        None, Body, BR, R, B
    }


    public partial class MainWindow : Window
    {
        private Figure currentFigure;
        private Figure tempFigure;
        private ObservableCollection<Figure> figures = new();

        private HitTypes hitType;
        private Tools currentTool = Tools.Arrow;
        private ActionTypes currentAction = ActionTypes.None;

        private DrawCore DrawCore;

        private int lastZIndex;

        public MainWindow()
        {
            InitializeComponent();

            InitButtons();
            InitHotKeys();

            DrawCore = new(canvas);
            DataContext = DrawCore;


            treeView.ItemsSource = DrawCore.Figures;

            fillColorPicker.SelectedColor = Colors.Blue;
            borderColorPicker.SelectedColor = Colors.Black;
            borderWidthNumeric.Value = 2;

            fillColorCodeTextBox.LostFocus += (s, e) =>
            {
                fillColorPicker.SelectedColor = (Color)ColorConverter.ConvertFromString(fillColorCodeTextBox.Text);
            };
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point currentPoint = e.GetPosition(canvas);

            if (currentTool != Tools.Arrow)
            {
                currentAction = ActionTypes.Draw;

                currentFigure = new Figure
                {
                    DrawPoint = currentPoint
                };
                currentFigure.Primitives.Add(new Primitive()
                {
                    GeometryDrawing = new GeometryDrawing
                    {
                        Brush = new SolidColorBrush((Color)fillColorPicker.SelectedColor),
                        Pen = new Pen(new SolidColorBrush((Color)borderColorPicker.SelectedColor), (double)borderWidthNumeric.Value)
                    }
                });

                canvas.Children.Add(currentFigure);

                Canvas.SetTop(currentFigure, currentPoint.Y);
                Canvas.SetLeft(currentFigure, currentPoint.X);
                Canvas.SetZIndex(currentFigure, ++lastZIndex);

                return;
            }

            if (currentAction == ActionTypes.Unit)
            {
                UnitFigures(currentPoint);
                return;
            }

            if (currentAction == ActionTypes.Link)
            {
                LinkFigures(currentPoint);
                return;
            }

            if (currentTool == Tools.Arrow && (currentFigure = GetFigure(currentPoint)) != null)
            {
                hitType = SetHitType(currentFigure, currentPoint);
                SetCursor();

                if (hitType == HitTypes.Body)
                {
                    currentAction = ActionTypes.Drag;
                    Canvas.SetZIndex(currentFigure, ++lastZIndex);
                    return;
                }

                if (hitType != HitTypes.None)
                {
                    currentAction = ActionTypes.Resize;
                    return;
                }
            }

            hitType = SetHitType(currentFigure, currentPoint);
            SetCursor();
        }
        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            Point currentPoint = e.GetPosition(canvas);

            if (currentAction == ActionTypes.Draw)
            {
                Draw(currentPoint);
                return;
            }

            if (currentAction == ActionTypes.Drag)
            {
                Drag(currentPoint);
                return;
            }

            if (currentAction == ActionTypes.Resize)
            {
                Resize(currentPoint);
                return;
            }

            hitType = SetHitType(currentFigure, currentPoint);
            SetCursor();
        }
        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (currentAction == ActionTypes.Draw)
            {
                currentFigure.DrawPoint = currentFigure.Primitives[0].GeometryDrawing.Bounds.TopLeft;
                figures.Add(currentFigure);
            }

            if (currentAction == ActionTypes.Drag)
            {
                Point newDrawPoint = GetDrawPoint();
                MakeNewPrimitive(newDrawPoint);
                currentFigure.DrawPoint = newDrawPoint;
            }

            currentAction = ActionTypes.None;
        }
        private void Draw(Point currentPoint)
        {
            switch (currentTool)
            {
                case Tools.Rect:
                    currentFigure.Primitives[0].Type = "Прямоугольник";
                    currentFigure.Primitives[0].GeometryDrawing.Geometry =
                        new RectangleGeometry(MakeRightRect(currentFigure.DrawPoint, currentPoint));
                    break;
                case Tools.Line:
                    currentFigure.Primitives[0].Type = "Линия";
                    currentFigure.Primitives[0].GeometryDrawing.Geometry =
                        new LineGeometry(currentFigure.DrawPoint, currentPoint);
                    break;
                case Tools.Ellipse:
                    currentFigure.Primitives[0].Type = "Овал";
                    currentFigure.Primitives[0].GeometryDrawing.Geometry =
                        new EllipseGeometry(MakeRightRect(currentFigure.DrawPoint, currentPoint));
                    break;
                case Tools.Triengle:
                    currentFigure.Primitives[0].Type = "Треугольник";
                    currentFigure.Primitives[0].GeometryDrawing.Geometry =
                        MakeRightTriangle(currentFigure.DrawPoint, currentPoint);
                    break;
                default:
                    break;
            }

            Canvas.SetTop(currentFigure, currentFigure.Primitives[0].GeometryDrawing.Bounds.Top);
            Canvas.SetLeft(currentFigure, currentFigure.Primitives[0].GeometryDrawing.Bounds.Left);
        }
        private void Drag(Point currentPoint)
        {
            double width = currentFigure.ActualWidth;
            double height = currentFigure.ActualHeight;

            double x_pos = currentPoint.X - width / 2;
            double y_pos = currentPoint.Y - height / 2;

            if (x_pos >= 0 && y_pos >= 0
                && x_pos + width <= canvas.ActualWidth
                && y_pos + height <= canvas.ActualHeight)
            {
                Canvas.SetTop(currentFigure, y_pos);
                Canvas.SetLeft(currentFigure, x_pos);
            }
        }
        private void Resize(Point currentPoint)
        {
            switch (hitType)
            {
                case HitTypes.BR:
                    if (currentPoint.X > currentFigure.DrawPoint.X && currentPoint.Y > currentFigure.DrawPoint.Y)
                    {
                        ((Image)currentFigure.Child).Width = currentPoint.X - currentFigure.DrawPoint.X;
                        ((Image)currentFigure.Child).Height = currentPoint.Y - currentFigure.DrawPoint.Y;
                    }
                    break;
                case HitTypes.R:
                    if (currentPoint.X > currentFigure.DrawPoint.X)
                    {
                        ((Image)currentFigure.Child).Width = currentPoint.X - currentFigure.DrawPoint.X;
                        ((Image)currentFigure.Child).Height = ((Image)currentFigure.Child).Height;
                    }
                    break;
                case HitTypes.B:
                    if (currentPoint.Y > currentFigure.DrawPoint.Y)
                    {
                        ((Image)currentFigure.Child).Height = currentPoint.Y - currentFigure.DrawPoint.Y;
                        ((Image)currentFigure.Child).Width = ((Image)currentFigure.Child).Width;
                    }
                    break;
                default:
                    break;
            }
        }
        private void UnitFigures(Point currentPoint)
        {
            if (GetFigure(currentPoint) is Figure temp && temp != currentFigure && temp != null && currentFigure != null)
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

                canvas.Children.Remove(currentFigure);
                figures.Remove(currentFigure);

                Canvas.SetLeft(temp, newFigurePos.X);
                Canvas.SetTop(temp, newFigurePos.Y);

                currentFigure = temp;
            }
            else
            {
                MessageBox.Show("Действие невозможно");
            }

            currentAction = ActionTypes.None;
        }
        private void LinkFigures(Point currentPoint)
        {
            if (GetFigure(currentPoint) is Figure temp && temp != null && currentFigure != null && temp != currentFigure)
            {
                if (currentFigure.MajorFigureId == temp.Id || temp.MajorFigureId == currentFigure.Id)
                {
                    MessageBox.Show("Эти фигуры уже связаны", "Действие невозможно", MessageBoxButton.OK);
                    currentAction = ActionTypes.None;
                    return;
                }

                currentFigure.MajorFigureId = temp.Id;

                temp.AnchorFiguresId.Add(currentFigure.Id);

                currentFigure.AnchorPoint = new(temp.DrawPoint.X - currentFigure.DrawPoint.X,
                                                temp.DrawPoint.Y - currentFigure.DrawPoint.Y);
            }
            else
            {
                MessageBox.Show("Действие невозможно");
            }

            currentAction = ActionTypes.None;
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
            double differenceX = (currentPoint.X - currentFigure.DrawPoint.X);
            double differenceY = (currentPoint.Y - currentFigure.DrawPoint.Y);

            foreach (Primitive primitive in currentFigure.Primitives)
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
            if (VisualTreeHelper.HitTest(canvas, point) is var hit && hit == null || hit.VisualHit == canvas)
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
        private static HitTypes SetHitType(Figure figure, Point point)
        {
            if (figure == null)
                return HitTypes.None;

            double left = figure.DrawPoint.X;
            double top = figure.DrawPoint.Y;
            double right = left + figure.ActualWidth;
            double bottom = top + figure.ActualHeight;

            if (point.X < left)
                return HitTypes.None;

            if (point.X > right)
                return HitTypes.None;

            if (point.Y < top)
                return HitTypes.None;

            if (point.Y > bottom)
                return HitTypes.None;

            const double GAP = 10;
            if (right - point.X < GAP)
            {
                // Right edge.
                if (point.Y - top < GAP)
                    return HitTypes.None;

                if (bottom - point.Y < GAP)
                    return HitTypes.BR;

                return HitTypes.R;
            }

            if (bottom - point.Y < GAP)
                return HitTypes.B;

            return HitTypes.Body;
        }
        private void SetCursor()
        {
            if (currentTool != Tools.Arrow)
            {
                Cursor = Cursors.Arrow;
                return;
            }

            Cursor = hitType switch
            {
                //HitTypes.None => Cursors.Arrow,
                HitTypes.Body => Cursors.SizeAll,
                HitTypes.BR => Cursors.SizeNWSE,
                HitTypes.R => Cursors.SizeWE,
                HitTypes.B => Cursors.SizeNS,
                _ => Cursors.Arrow,
            };
        }
        private Point GetDrawPoint() => new(Canvas.GetLeft(currentFigure), Canvas.GetTop(currentFigure));

        private void InitHotKeys()
        {
            CommandBinding copyCommand = new() { Command = ApplicationCommands.Copy };
            copyCommand.Executed += CopyFigure;
            CommandBindings.Add(copyCommand);

            CommandBinding pasteCommand = new() { Command = ApplicationCommands.Paste };
            pasteCommand.Executed += PasteFigure;
            CommandBindings.Add(pasteCommand);

            CommandBinding deleteCommand = new() { Command = ApplicationCommands.Delete };
            deleteCommand.Executed += DeleteFigure;
            CommandBindings.Add(deleteCommand);
        }
        private void InitButtons()
        {
            arrowButton.Click += (s, e) => DrawCore.Action.Type = Drawing.ActionTypes.Choice;
            moveButton.Click += (s, e) => DrawCore.Action.Type = Drawing.ActionTypes.Move;
            ellipseButton.Click += (s, e) => DrawCore.Action = new() { Type = Drawing.ActionTypes.Draw, DrawingType = DrawingType.Ellipse, Context = new() { BorderColor = Colors.Black, FillColor = Colors.Red, BorderWidth = 2 } };
            rectButton.Click += (s, e) => DrawCore.Action = new() { Type = Drawing.ActionTypes.Draw, DrawingType = DrawingType.Rect, Context = new() { BorderColor = Colors.Black, FillColor = Colors.Red, BorderWidth = 2 } };
            triangleButton.Click += (s, e) => DrawCore.Action = new() { Type = Drawing.ActionTypes.Draw, DrawingType = DrawingType.Triengle, Context = new() { BorderColor = Colors.Black, FillColor = Colors.Red, BorderWidth = 2 } };
            lineButton.Click += (s, e) => DrawCore.Action = new() { Type = Drawing.ActionTypes.Draw, DrawingType = DrawingType.Line, Context = new() { BorderColor = Colors.Black, FillColor = Colors.Red, BorderWidth = 2 } };

            unitButton.Click += (s, e) => DrawCore.Action = new() { Type = Drawing.ActionTypes.Unit };
            //setMajorFigureButton.Click += (s, e) =>
            //{
            //    if (currentFigure.MajorFigureId == 0)
            //        currentAction = ActionTypes.Link;
            //    else
            //        MessageBox.Show("Эта фигура уже имеет привязку", "Действие невозможно", MessageBoxButton.OK);
            //};
        }
        private void CopyFigure(object sender, ExecutedRoutedEventArgs e)
        {
            if (currentFigure != null)
                tempFigure = currentFigure;
        }
        private void PasteFigure(object sender, ExecutedRoutedEventArgs e)
        {
            if (tempFigure != null)
            {
                currentFigure = (Figure)tempFigure.Clone();

                figures.Add(currentFigure);
                canvas.Children.Add(currentFigure);

                Canvas.SetLeft(currentFigure, currentFigure.DrawPoint.X);
                Canvas.SetTop(currentFigure, currentFigure.DrawPoint.Y);
                Canvas.SetZIndex(currentFigure, ++lastZIndex);
            }
        }
        private void DeleteFigure(object sender, ExecutedRoutedEventArgs e)
        {
            if (currentFigure == null)
                return;

            figures.Remove(currentFigure);
            canvas.Children.Remove(currentFigure);

            if (currentFigure.MajorFigureId != 0)
            {
                foreach (var item in figures)
                {
                    if (item.Id == currentFigure.MajorFigureId)
                    {
                        item.AnchorFiguresId.Remove(currentFigure.Id);
                    }
                    if (item.MajorFigureId == currentFigure.Id)
                    {
                        item.MajorFigureId = 0;
                    }
                }
            }

            currentFigure = null;
        }
    }
}
