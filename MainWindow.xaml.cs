using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LevelRedactor.Drawing;
using Microsoft.Win32;

using LevelRedactor.Parser;
using LevelRedactor.Parser.Models;

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

            DrawCore.Action.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "Type" || e.PropertyName == "DrawingType")
                {
                    SetToolBarButtonsStyle();
                }
            };
            
            treeView.ItemsSource = DrawCore.Figures;

            fillColorCodeTextBox.LostFocus += (s, e) =>
            {
                fillColorPicker.SelectedColor = (Color)ColorConverter.ConvertFromString(fillColorCodeTextBox.Text);
            };
        }

        private void SetToolBarButtonsStyle()
        {
            switch (DrawCore.Action.Type)
            {
                case ActionTypes.Draw:
                    switch (DrawCore.Action.DrawingType)
                    {
                        case DrawingType.Rect:
                            SetButtonsStyle(rectButton);
                            break;
                        case DrawingType.Ellipse:
                            SetButtonsStyle(ellipseButton);
                            break;
                        case DrawingType.Line:
                            SetButtonsStyle(lineButton);
                            break;
                        case DrawingType.Triengle:
                            SetButtonsStyle(triangleButton);
                            break;
                        case DrawingType.Polygon:
                            SetButtonsStyle(polygonButton);
                            break;
                        case DrawingType.Polyline:
                            SetButtonsStyle(polylineButton);
                            break;
                        default:
                            break;
                    }
                    break;
                case ActionTypes.Move:
                    SetButtonsStyle(moveButton);
                    break;
                case ActionTypes.Resize:
                    break;
                case ActionTypes.Unit:
                    SetButtonsStyle(unitButton);
                    break;
                case ActionTypes.Link:
                    SetButtonsStyle(linkButton);
                    break;
                case ActionTypes.Choice:
                    SetButtonsStyle(arrowButton);
                    break;
                default:
                    break;
            }

            void SetButtonsStyle(object sender)
            {
                foreach (var item in toolBar.Items)
                {
                    if (item is not Separator)
                        if (item == sender)
                        {
                            ((Button)item).Background = new SolidColorBrush(Color.FromArgb(35, 0, 0, 150));
                        }
                        else
                        {
                            ((Button)item).Background = Brushes.Transparent;
                        }
                }
            }
        }

        


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
            arrowButton.Click += (s, e) => DrawCore.Action.Type = ActionTypes.Choice;
            moveButton.Click += (s, e) => DrawCore.Action.Type = ActionTypes.Move;
            unitButton.Click += (s, e) => DrawCore.Action.Type = ActionTypes.Unit;
            linkButton.Click += (s, e) => DrawCore.Action.Type = ActionTypes.Link;

            ellipseButton.Click += (s, e) =>
            {
                DrawCore.Action.Type = ActionTypes.Draw;
                DrawCore.Action.DrawingType = DrawingType.Ellipse;
            };
            rectButton.Click += (s, e) =>
            {
                DrawCore.Action.Type = ActionTypes.Draw;
                DrawCore.Action.DrawingType = DrawingType.Rect;
            };
            triangleButton.Click += (s, e) =>
            {
                DrawCore.Action.Type = ActionTypes.Draw;
                DrawCore.Action.DrawingType = DrawingType.Triengle;
            };
            lineButton.Click += (s, e) =>
            {
                DrawCore.Action.Type = ActionTypes.Draw;
                DrawCore.Action.DrawingType = DrawingType.Line;
            };

            polygonButton.Click += (s, e) =>
            {
                DrawCore.Action.Type = ActionTypes.Draw;
                DrawCore.Action.DrawingType = DrawingType.Polygon;
            };

            polylineButton.Click += (s, e) =>
            {
                DrawCore.Action.Type = ActionTypes.Draw;
                DrawCore.Action.DrawingType = DrawingType.Polyline;
            };

            divorceButton.Click += (s, e) => 
            {
                DrawCore.Divorce();
            };

            //void SetButtonsStyle(object sender)
            //{
            //    foreach (var item in toolBar.Items)
            //    {
            //        if (item is not Separator)
            //            if (item == sender)
            //            {
            //                ((Button)item).Background = new SolidColorBrush(Color.FromArgb(35, 0, 0, 150));
            //            }
            //            else
            //            {
            //                ((Button)item).Background = Brushes.Transparent;
            //            }
            //    }
            //}

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

        private void SaveFile(object sender, EventArgs e)
        {
            if (!IsDataCorrect())
                return;

            SetLevelDataWindow setLevelDataWindow = new();
            if (setLevelDataWindow.ShowDialog() == true)
            {
                if (setLevelDataWindow.DialogResult == false)
                    return;

                string levelName = setLevelDataWindow.LevelName;
                string tag = setLevelDataWindow.LevelTag;
                string jsonString = Parser.Parser.ToJson(levelName, tag, DrawCore.Figures);

                SaveFileDialog saveFileDialog = new() { Filter = "Файл уровня|*.json" };
                if (saveFileDialog.ShowDialog() == true)
                {
                    File.WriteAllText(saveFileDialog.FileName, jsonString);
                }
            }
        }
        private void FastExport(object sender, EventArgs e)
        {
            if (!IsDataCorrect())
                return;

            SetLevelDataWindow setLevelDataWindow = new();
            if (setLevelDataWindow.ShowDialog() == true)
            {
                if (setLevelDataWindow.DialogResult == false)
                    return;

                string levelName = setLevelDataWindow.LevelName;
                string tag = setLevelDataWindow.LevelTag;

                string jsonData = Parser.Parser.ToJson(levelName, tag, DrawCore.Figures);

                LevelRepository lr = new();
                lr.SendSetToServer(jsonData);
            }
        }
        private bool IsDataCorrect()
        {
            if (DrawCore.Figures.Count == 0)
            {
                MessageBox.Show("Добавьте хотя бы одну фигуру", "Ошибка", MessageBoxButton.OK);
                return false;
            }

            int countFiguresWithoutLink = 0;

            foreach (Figure figure in DrawCore.Figures)
            {
                if (figure.MajorFigureId == 0)
                    countFiguresWithoutLink++;
            }

            if (countFiguresWithoutLink > 1)
            {
                MessageBox.Show("Не все фигуры связаны", "Ошибка", MessageBoxButton.OK);
                return false;
            }

            return true;
        }
    }
}
