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


    public partial class MainWindow : Window
    {
        private DrawCore DrawCore;

        public MainWindow()
        {
            InitializeComponent();

            InitButtons();
            //InitHotKeys();

            DrawCore = new(canvas);
            DataContext = DrawCore;


            polygonButton.Click += (s, e) =>
            {
                treeView.Visibility = Visibility.Collapsed;
            };
            polylineButton.Click += (s, e) =>
            {
                treeView.Visibility = Visibility.Visible;
            };

            treeView.ItemsSource = DrawCore.Figures;

            //fillColorPicker.SelectedColor = Colors.Blue;
            //borderColorPicker.SelectedColor = Colors.Black;
            //borderWidthNumeric.Value = 2;

            fillColorCodeTextBox.LostFocus += (s, e) =>
            {
                fillColorPicker.SelectedColor = (Color)ColorConverter.ConvertFromString(fillColorCodeTextBox.Text);
            };
        }



        //private void Resize(Point currentPoint)
        //{
        //    switch (hitType)
        //    {
        //        case HitTypes.BR:
        //            if (currentPoint.X > currentFigure.DrawPoint.X && currentPoint.Y > currentFigure.DrawPoint.Y)
        //            {
        //                ((Image)currentFigure.Child).Width = currentPoint.X - currentFigure.DrawPoint.X;
        //                ((Image)currentFigure.Child).Height = currentPoint.Y - currentFigure.DrawPoint.Y;
        //            }
        //            break;
        //        case HitTypes.R:
        //            if (currentPoint.X > currentFigure.DrawPoint.X)
        //            {
        //                ((Image)currentFigure.Child).Width = currentPoint.X - currentFigure.DrawPoint.X;
        //                ((Image)currentFigure.Child).Height = ((Image)currentFigure.Child).Height;
        //            }
        //            break;
        //        case HitTypes.B:
        //            if (currentPoint.Y > currentFigure.DrawPoint.Y)
        //            {
        //                ((Image)currentFigure.Child).Height = currentPoint.Y - currentFigure.DrawPoint.Y;
        //                ((Image)currentFigure.Child).Width = ((Image)currentFigure.Child).Width;
        //            }
        //            break;
        //        default:
        //            break;
        //    }
        //}
       
        //private void LinkFigures(Point currentPoint)
        //{
        //    if (GetFigure(currentPoint) is Figure temp && temp != null && currentFigure != null && temp != currentFigure)
        //    {
        //        if (currentFigure.MajorFigureId == temp.Id || temp.MajorFigureId == currentFigure.Id)
        //        {
        //            MessageBox.Show("Эти фигуры уже связаны", "Действие невозможно", MessageBoxButton.OK);
        //            currentAction = ActionTypes.None;
        //            return;
        //        }

        //        currentFigure.MajorFigureId = temp.Id;

        //        temp.AnchorFiguresId.Add(currentFigure.Id);

        //        currentFigure.AnchorPoint = new(temp.DrawPoint.X - currentFigure.DrawPoint.X,
        //                                        temp.DrawPoint.Y - currentFigure.DrawPoint.Y);
        //    }
        //    else
        //    {
        //        MessageBox.Show("Действие невозможно");
        //    }

        //    currentAction = ActionTypes.None;
        //}


        //private static HitTypes SetHitType(Figure figure, Point point)
        //{
        //    if (figure == null)
        //        return HitTypes.None;

        //    double left = figure.DrawPoint.X;
        //    double top = figure.DrawPoint.Y;
        //    double right = left + figure.ActualWidth;
        //    double bottom = top + figure.ActualHeight;

        //    if (point.X < left)
        //        return HitTypes.None;

        //    if (point.X > right)
        //        return HitTypes.None;

        //    if (point.Y < top)
        //        return HitTypes.None;

        //    if (point.Y > bottom)
        //        return HitTypes.None;

        //    const double GAP = 10;
        //    if (right - point.X < GAP)
        //    {
        //        // Right edge.
        //        if (point.Y - top < GAP)
        //            return HitTypes.None;

        //        if (bottom - point.Y < GAP)
        //            return HitTypes.BR;

        //        return HitTypes.R;
        //    }

        //    if (bottom - point.Y < GAP)
        //        return HitTypes.B;

        //    return HitTypes.Body;
        //}
        //private void SetCursor()
        //{
        //    if (currentTool != Tools.Arrow)
        //    {
        //        Cursor = Cursors.Arrow;
        //        return;
        //    }

        //    Cursor = hitType switch
        //    {
        //        //HitTypes.None => Cursors.Arrow,
        //        HitTypes.Body => Cursors.SizeAll,
        //        HitTypes.BR => Cursors.SizeNWSE,
        //        HitTypes.R => Cursors.SizeWE,
        //        HitTypes.B => Cursors.SizeNS,
        //        _ => Cursors.Arrow,
        //    };
        //}

        //private void InitHotKeys()
        //{
        //    CommandBinding copyCommand = new() { Command = ApplicationCommands.Copy };
        //    copyCommand.Executed += CopyFigure;
        //    CommandBindings.Add(copyCommand);

        //    CommandBinding pasteCommand = new() { Command = ApplicationCommands.Paste };
        //    pasteCommand.Executed += PasteFigure;
        //    CommandBindings.Add(pasteCommand);

        //    CommandBinding deleteCommand = new() { Command = ApplicationCommands.Delete };
        //    deleteCommand.Executed += DeleteFigure;
        //    CommandBindings.Add(deleteCommand);
        //}
        private void InitButtons()
        {
            arrowButton.Click += (s, e) => DrawCore.Action.Type = Drawing.ActionTypes.Choice;
            moveButton.Click += (s, e) => DrawCore.Action.Type = Drawing.ActionTypes.Move;
            ellipseButton.Click += (s, e) => DrawCore.Action = new() { Type = Drawing.ActionTypes.Draw, DrawingType = DrawingType.Ellipse, Context = new() { BorderColor = Colors.Black, FillColor = Colors.Red, BorderWidth = 2 } };
            rectButton.Click += (s, e) => DrawCore.Action = new() { Type = Drawing.ActionTypes.Draw, DrawingType = DrawingType.Rect, Context = new() { BorderColor = Colors.Black, FillColor = Colors.Green, BorderWidth = 3 } };
            triangleButton.Click += (s, e) => DrawCore.Action = new() { Type = Drawing.ActionTypes.Draw, DrawingType = DrawingType.Triengle, Context = new() { BorderColor = Colors.Black, FillColor = Colors.Blue, BorderWidth = 4 } };
            lineButton.Click += (s, e) => DrawCore.Action = new() { Type = Drawing.ActionTypes.Draw, DrawingType = DrawingType.Line, Context = new() { BorderColor = Colors.Black, FillColor = Colors.Yellow, BorderWidth = 5 } };

            unitButton.Click += (s, e) => DrawCore.Action = new() { Type = Drawing.ActionTypes.Unit };
            //setMajorFigureButton.Click += (s, e) =>
            //{
            //    if (currentFigure.MajorFigureId == 0)
            //        currentAction = ActionTypes.Link;
            //    else
            //        MessageBox.Show("Эта фигура уже имеет привязку", "Действие невозможно", MessageBoxButton.OK);
            //};
        }
        //private void CopyFigure(object sender, ExecutedRoutedEventArgs e)
        //{
        //    if (currentFigure != null)
        //        tempFigure = currentFigure;
        //}
        //private void PasteFigure(object sender, ExecutedRoutedEventArgs e)
        //{
        //    if (tempFigure != null)
        //    {
        //        currentFigure = (Figure)tempFigure.Clone();

        //        figures.Add(currentFigure);
        //        canvas.Children.Add(currentFigure);

        //        Canvas.SetLeft(currentFigure, currentFigure.DrawPoint.X);
        //        Canvas.SetTop(currentFigure, currentFigure.DrawPoint.Y);
        //        Canvas.SetZIndex(currentFigure, ++lastZIndex);
        //    }
        //}
        //private void DeleteFigure(object sender, ExecutedRoutedEventArgs e)
        //{
        //    if (currentFigure == null)
        //        return;

        //    figures.Remove(currentFigure);
        //    canvas.Children.Remove(currentFigure);

        //    if (currentFigure.MajorFigureId != 0)
        //    {
        //        foreach (var item in figures)
        //        {
        //            if (item.Id == currentFigure.MajorFigureId)
        //            {
        //                item.AnchorFiguresId.Remove(currentFigure.Id);
        //            }
        //            if (item.MajorFigureId == currentFigure.Id)
        //            {
        //                item.MajorFigureId = 0;
        //            }
        //        }
        //    }

        //    currentFigure = null;
        //}
    }
}
