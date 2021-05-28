using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LevelRedactor.Drawing
{
    public class DrawCore : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private DrawingState drawingState = new();
        private Figure currentFigure;
        private Figure tempFigure;
        private Primitive currentPrimitive;
        private Action action;
        private ObservableCollection<Figure> figures = new();
        private Point cursorPoint;

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
        public Action Action 
        {
            get => action;
            set
            {
                if (value.Type == ActionTypes.Link && CurrentFigure.MajorFigureId != 0)
                {
                    MessageBox.Show("Фигура уже имеет привязку", "Ошибка", MessageBoxButton.OK);
                    return;
                }
                action = value;
                OnPropertyChanged("Action");
            }
        }
        public Point CursorPoint
        {
            get => cursorPoint;
            private set
            {
                cursorPoint = value;
                OnPropertyChanged("CursorPoint");
                
            }
        }
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

            if (e.ClickCount == 2 && Action.Type == ActionTypes.Draw && 
                (Action.DrawingType is DrawingType.Polygon or DrawingType.Polyline) && 
                drawingState.IsDrawing)
            {
                CurrentFigure.DrawPoint = CurrentFigure.Primitives[0].GeometryDrawing.Bounds.TopLeft;
                figures.Add(CurrentFigure);
                CurrentPrimitive = CurrentFigure.Primitives[0];
                Action.Type = ActionTypes.Move;
                drawingState.IsDrawing = false;
            }

            switch (Action.Type)
            {
                case ActionTypes.Draw:
                    if (Action.DrawingType is not DrawingType.Polygon and not DrawingType.Polyline)
                    {
                        Draw(currentPoint);
                        return;
                    }
                    if ((Action.DrawingType is DrawingType.Polygon or DrawingType.Polyline) && drawingState.IsDrawing == false)
                        DrawPoly(currentPoint);
                    else
                        DrawPoly(currentPoint, false);
                    break;
                case ActionTypes.Move:
                    if ( (CurrentFigure = GetFigure(currentPoint)) is not null && (CurrentPrimitive = GetPrimitive(currentPoint)) is not null)
                    {
                        drawingState.LastPoint = currentFigure.DrawPoint;
                        drawingState.OffsetPoint = new(currentPoint.X - CurrentFigure.DrawPoint.X, currentPoint.Y - CurrentFigure.DrawPoint.Y);
                    }
                    else
                    {
                        CurrentFigure = null;
                    }
                    break;
                case ActionTypes.Unit:
                    UnitFigures(currentPoint);
                    break;
                case ActionTypes.Link:
                    LinkFigures(currentPoint);
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
            CursorPoint = currentPoint;

            switch (Action.Type)
            {
                case ActionTypes.Draw:
                    if (e.LeftButton == MouseButtonState.Pressed && currentFigure is not null)
                    {
                        Draw(currentPoint, false);
                    }
                    break;
                case ActionTypes.Move:
                    if (CurrentFigure != null && e.LeftButton == MouseButtonState.Pressed)
                    {
                        MoveFigure(currentPoint);
                    }
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
                    if (Action.DrawingType is not DrawingType.Polygon and not DrawingType.Polyline)
                    {
                        CurrentFigure.DrawPoint = CurrentFigure.Primitives[0].GeometryDrawing.Bounds.TopLeft;
                        figures.Add(CurrentFigure);
                        CurrentPrimitive = CurrentFigure.Primitives[0];
                        drawingState.IsDrawing = false;
                        Action.Type = ActionTypes.Move;
                    }
                    break;
                case ActionTypes.Move:
                    if (CurrentFigure is not null)
                    {
                        CurrentFigure.DrawPoint = GetDrawPoint();
                        PrimitiveBuilder.MakeNewPrimitives(CurrentFigure, drawingState.LastPoint);
                    }
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
                drawingState.IsDrawing = true;

                CurrentFigure = new Figure
                {
                    DrawPoint = point,
                    ZIndex = drawingState.LastZIndex++
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
                            new RectangleGeometry(PrimitiveBuilder.MakeRightRect(CurrentFigure.DrawPoint, point));
                        break;
                    case DrawingType.Line:
                        CurrentFigure.Primitives[0].Type = "Линия";
                        CurrentFigure.Primitives[0].GeometryDrawing.Geometry =
                            new LineGeometry(CurrentFigure.DrawPoint, point);
                        break;
                    case DrawingType.Ellipse:
                        CurrentFigure.Primitives[0].Type = "Овал";
                        CurrentFigure.Primitives[0].GeometryDrawing.Geometry =
                            new EllipseGeometry(PrimitiveBuilder.MakeRightRect(CurrentFigure.DrawPoint, point));
                        break;
                    case DrawingType.Triengle:
                        CurrentFigure.Primitives[0].Type = "Треугольник";
                        CurrentFigure.Primitives[0].GeometryDrawing.Geometry =
                            PrimitiveBuilder.MakeRightTriangle(CurrentFigure.DrawPoint, point);
                        break;
                    default:
                        break;
                }

                Canvas.SetTop(CurrentFigure, CurrentFigure.Primitives[0].GeometryDrawing.Bounds.Top);
                Canvas.SetLeft(CurrentFigure, CurrentFigure.Primitives[0].GeometryDrawing.Bounds.Left);
                Canvas.SetZIndex(CurrentFigure, CurrentFigure.ZIndex); // ??
            }
            
        }
        private void DrawPoly(Point point, bool isBegin = true)
        {
            if (isBegin)
            {
                CurrentFigure = new Figure
                {
                    DrawPoint = point,
                    ZIndex = drawingState.LastZIndex++
                };
                CurrentFigure.Primitives.Add(new Primitive()
                {
                    GeometryDrawing = new GeometryDrawing
                    {
                        Brush = new SolidColorBrush(Action.Context.FillColor),
                        Pen = new Pen(new SolidColorBrush(Action.Context.BorderColor), Action.Context.BorderWidth)
                    }
                });

                PathFigure pf;

                if (Action.DrawingType == DrawingType.Polygon)
                {
                    CurrentFigure.Primitives[0].Type = "Многоугольник";
                    pf = new() { StartPoint = CurrentFigure.DrawPoint, IsClosed = true };
                }
                else
                {
                    CurrentFigure.Primitives[0].Type = "Ломанная";
                    CurrentFigure.Primitives[0].GeometryDrawing.Brush = Brushes.Transparent;
                    pf = new() { StartPoint = CurrentFigure.DrawPoint, IsClosed = false };
                }

                CurrentFigure.Primitives[0].GeometryDrawing.Geometry = new PathGeometry();
                ((PathGeometry)CurrentFigure.Primitives[0].GeometryDrawing.Geometry).Figures.Add(pf);

                Canvas.Children.Add(CurrentFigure);

                Canvas.SetTop(CurrentFigure, point.Y);
                Canvas.SetLeft(CurrentFigure, point.X);

                drawingState.IsDrawing = true;
            }
            else
            {
                PrimitiveBuilder.AddSegment(CurrentFigure.Primitives[0], point);
                Canvas.SetLeft(CurrentFigure, CurrentFigure.Primitives[0].GeometryDrawing.Bounds.Left);
                Canvas.SetTop(CurrentFigure, CurrentFigure.Primitives[0].GeometryDrawing.Bounds.Top);
            }

        }
        private void MoveFigure(Point point)
        {
            double width = CurrentFigure.ActualWidth;
            double height = CurrentFigure.ActualHeight;

            double x_pos = (int)(point.X - drawingState.OffsetPoint.X);
            double y_pos = (int)(point.Y - drawingState.OffsetPoint.Y);

            if (x_pos >= 0 && x_pos + width <= Canvas.ActualWidth)
            {
                Canvas.SetLeft(CurrentFigure, x_pos);
            }
            if (y_pos >= 0 && y_pos + height <= Canvas.ActualHeight)
            {
                Canvas.SetTop(CurrentFigure, y_pos);
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

            Action.Type = ActionTypes.Choice;
        }
        private void LinkFigures(Point point)
        {
            if (GetFigure(point) is Figure temp && temp != null && currentFigure != null && temp != currentFigure)
            {
                if (currentFigure.MajorFigureId == temp.Id || temp.MajorFigureId == currentFigure.Id)
                {
                    MessageBox.Show("Эти фигуры уже связаны", "Действие невозможно", MessageBoxButton.OK);
                    Action.Type = ActionTypes.Choice;
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

            Action.Type = ActionTypes.Choice;
        }
        public void Divorce()
        {
            if (CurrentPrimitive is not null && CurrentFigure.Primitives.Count > 1)
            {
                Figure f = new() { ZIndex = ++drawingState.LastZIndex, DrawPoint = CurrentPrimitive.GeometryDrawing.Bounds.Location };
                f.Primitives.Add(CurrentPrimitive);
                CurrentFigure.Primitives.Remove(CurrentPrimitive);
                figures.Add(f);
                Canvas.Children.Add(f);
                CurrentFigure = f;

                Canvas.SetLeft(CurrentFigure, CurrentFigure.DrawPoint.X);
                Canvas.SetTop(CurrentFigure, CurrentFigure.DrawPoint.Y);
                Canvas.SetZIndex(CurrentFigure, CurrentFigure.ZIndex);
            }
        }
        public void ChangeFigureZIndex(bool isInc)
        {
            if (CurrentFigure is not null)
            {
                if (isInc && CurrentFigure.ZIndex != drawingState.LastZIndex - 1)
                {
                    Canvas.SetZIndex(CurrentFigure, ++CurrentFigure.ZIndex);
                    foreach (Figure item in Figures)
                    {
                        if (item != CurrentFigure && item.ZIndex == CurrentFigure.ZIndex)
                            Canvas.SetZIndex(item, --item.ZIndex);
                    }
                }
                if (isInc == false && CurrentFigure.ZIndex != 0)
                {
                    Canvas.SetZIndex(CurrentFigure, --CurrentFigure.ZIndex);
                    foreach (Figure item in Figures)
                    {
                        if (item != CurrentFigure && item.ZIndex == CurrentFigure.ZIndex)
                            Canvas.SetZIndex(item, ++item.ZIndex);
                    }
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
                {
                    Action.Context.FillColor = ((SolidColorBrush)currentFigure.Primitives[i].GeometryDrawing.Brush).Color;
                    Action.Context.BorderColor = ((SolidColorBrush)currentFigure.Primitives[i].GeometryDrawing.Pen.Brush).Color;
                    Action.Context.BorderWidth = (int)currentFigure.Primitives[i].GeometryDrawing.Pen.Thickness;

                    return currentFigure.Primitives[i];
                }
            }
            
            return null;
        }
        
        private Point GetDrawPoint() => new(Canvas.GetLeft(CurrentFigure), Canvas.GetTop(CurrentFigure));

        public void DeleteFigure(object sender, ExecutedRoutedEventArgs e)
        {
            if (currentFigure is null)
                return;

            figures.Remove(currentFigure);
            Canvas.Children.Remove(currentFigure);

            if (currentFigure.MajorFigureId != 0 || currentFigure.AnchorFiguresId.Count > 0)
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
        public void CopyFigure(object sender, ExecutedRoutedEventArgs e) => tempFigure = CurrentFigure;
        public void PasteFigure(object sender, ExecutedRoutedEventArgs e)
        {
            if (tempFigure is not null)
            {
                currentFigure = (Figure)tempFigure.Clone();
                currentFigure.ZIndex = drawingState.LastZIndex++;

                figures.Add(currentFigure);
                Canvas.Children.Add(currentFigure);

                Canvas.SetLeft(currentFigure, currentFigure.DrawPoint.X);
                Canvas.SetTop(currentFigure, currentFigure.DrawPoint.Y);
                Canvas.SetZIndex(currentFigure, currentFigure.ZIndex);
            }
        }
        public void RecalcAnchorPoints()
        {
            Figure GetById(int id)
            {
                foreach (Figure item in Figures)
                {
                    if (item.Id == id)
                    {
                        return item;
                    }
                }
                return null;
            }

            foreach (Figure dependenеtFigure in Figures)
            {
                if (dependenеtFigure.MajorFigureId != 0)
                {
                    Figure majorFigure = GetById(dependenеtFigure.MajorFigureId);

                    dependenеtFigure.AnchorPoint = new(majorFigure.DrawPoint.X - currentFigure.DrawPoint.X,
                                                        majorFigure.DrawPoint.Y - currentFigure.DrawPoint.Y);
                }
            }
        }
        
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
