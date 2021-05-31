using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using System.Windows.Input;
using System.Windows.Media;
using LevelRedactor.Drawing;
using System.Windows.Controls;
using Toolkit = Xceed.Wpf.Toolkit;

using LevelRedactor.Parser.Models;
using System.Diagnostics;

namespace LevelRedactor
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        private DrawCore DrawCore;

        public MainWindow()
        {
            InitializeComponent();

            DrawCore = new(canvas);
            DataContext = DrawCore;

            treeView.ItemsSource = DrawCore.Figures;

            InitButtons();
            InitHotKeys();

            DrawCore.Action.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName is "Type" or "DrawingType")
                {
                    SetToolBarButtonsStyle();
                }
            };
            DrawCore.Action.Context.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case "FillColor":
                        fillColorCodeTextBox.Text = DrawCore.Action.Context.FillColor.ToString();
                        break;
                    case "BorderColor":
                        borderColorCodeTextBox.Text = DrawCore.Action.Context.BorderColor.ToString();
                        break;
                    default:
                        break;
                }
            };
            fillColorCodeTextBox.KeyDown += (s, e) =>
            {
                if (e.Key is Key.Enter)
                {
                    SetColorFromString(fillColorCodeTextBox, fillColorPicker, fillColorCodeTextBox.Text);
                    fillColorCodeTextBox.Focusable = false;
                    fillColorCodeTextBox.Focus();
                }
            };
            borderColorCodeTextBox.KeyDown += (s, e) =>
            {
                if (e.Key is Key.Enter)
                {
                    SetColorFromString(borderColorCodeTextBox, borderColorPicker, borderColorCodeTextBox.Text);
                    fillColorCodeTextBox.Focusable = false;
                    fillColorCodeTextBox.Focus();
                }
            };
            figureTitleTextBox.KeyDown += (s, e) =>
            {
                if (e.Key is Key.Enter && String.IsNullOrWhiteSpace(figureTitleTextBox.Text) == false)
                {
                    DrawCore.CurrentFigure.Title = figureTitleTextBox.Text;
                    fillColorCodeTextBox.Focusable = false;
                    fillColorCodeTextBox.Focus();
                }
                
            };

            treeView.SelectedItemChanged += (s, e) =>
            {
                if (treeView.SelectedItem is Figure figure)
                    DrawCore.CurrentFigure = figure;

                if (treeView.SelectedItem is Primitive primitive)
                {
                    foreach (Figure f in DrawCore.Figures)
                    {
                        foreach (Primitive p in f.Primitives)
                        {
                            if (p == primitive)
                            {
                                DrawCore.CurrentFigure = f;
                            }
                        }
                    }
                    DrawCore.CurrentPrimitive = primitive;
                }
            };
        }

        private static void SetColorFromString(TextBox sender, Toolkit.ColorPicker target, string hexColor)
        {
            try
            {
                target.SelectedColor = (Color)ColorConverter.ConvertFromString(hexColor);
            }
            catch (Exception)
            {
                sender.Text = null;
            }
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

            SetToolAndPromptInto();

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
            void SetToolAndPromptInto()
            {
                string drawingPrompt = "Нажмите и удерживайте ЛКМ для рисования";
                (toolLabel.Content, promptLabel.Content) = (DrawCore.Action.Type, DrawCore.Action.DrawingType) switch
                {
                    (ActionTypes.Choice, _) => ("Указатель", "Нажмите чтобы выбрать примитив и фигуру"),
                    (ActionTypes.Move, _) => ("Перемещение", "Нажмите и удерживайте ЛКМ чтобы переместить фиугуру"),
                    (ActionTypes.Unit, _) => ("Объединение", "Выберете фигуру для объединения"),
                    (ActionTypes.Link, _) => ("Привязка", "Выберете фигуру для привязки"),
                    (_, DrawingType.Ellipse) => ("Овал", drawingPrompt),
                    (_, DrawingType.Rect) => ("Прямоугольник", drawingPrompt),
                    (_, DrawingType.Triengle) => ("Треугольник", drawingPrompt),
                    (_, DrawingType.Line) => ("Прямая", drawingPrompt),
                    (_, DrawingType.Polyline) => ("Ломанная", "Дважды нажмите ЛКМ чтобы закончить рисовать"),
                    (_, DrawingType.Polygon) => ("Многоугольник", "Дважды нажмите ЛКМ чтобы закончить рисовать"),
                };
            }
        }
        private void InitHotKeys()
        {
            CommandBinding copyCommand = new() { Command = ApplicationCommands.Copy };
            copyCommand.Executed += DrawCore.CopyFigure;
            CommandBindings.Add(copyCommand);

            CommandBinding pasteCommand = new() { Command = ApplicationCommands.Paste };
            pasteCommand.Executed += DrawCore.PasteFigure;
            CommandBindings.Add(pasteCommand);

            CommandBinding deleteCommand = new() { Command = ApplicationCommands.Delete };
            deleteCommand.Executed += DrawCore.DeleteFigure;
            CommandBindings.Add(deleteCommand);

            CommandBinding openCommand = new() { Command = ApplicationCommands.Open };
            openCommand.Executed += OpenFile;
            CommandBindings.Add(openCommand);

            CommandBinding saveCommand = new() { Command = ApplicationCommands.Save };
            saveCommand.Executed += SaveFile;
            CommandBindings.Add(saveCommand);
        }
        private void InitButtons()
        {
            arrowButton.Click += (s, e) => DrawCore.Action.Type = ActionTypes.Choice;
            moveButton.Click += (s, e) => DrawCore.Action.Type = ActionTypes.Move;
            unitButton.Click += (s, e) => DrawCore.Action.Type = ActionTypes.Unit;
            linkButton.Click += (s, e) => DrawCore.Action.Type = ActionTypes.Link;

            deleteLinkButton.Click += (s, e) => DrawCore.DeleteLink();
            divorceButton.Click += (s, e) => DrawCore.Divorce();
            incZIndexButton.Click += (s, e) => DrawCore.ChangeFigureZIndex(true);
            decZIndexButton.Click += (s, e) => DrawCore.ChangeFigureZIndex(false);
            recalcAnchorpointsButton.Click += (s, e) => DrawCore.RecalcAnchorPoints();

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
        }
        private void CreateFile(object sender, RoutedEventArgs e)
        {
            var currentExecutablePath = Process.GetCurrentProcess().MainModule.FileName;
            Process.Start(currentExecutablePath);
            Application.Current.Shutdown();
        }
        private void SaveFile(object sender, RoutedEventArgs e)
        {
            if (!IsDataCorrect())
                return;

            SetLevelDataWindow setLevelDataWindow = new();
            if (setLevelDataWindow.ShowDialog() == true)
            {
                if (setLevelDataWindow.DialogResult == false)
                    return;

                string levelName = setLevelDataWindow.LevelTitle;
                string tag = setLevelDataWindow.LevelTag;
                string jsonString = Parser.Parser.ToJson(levelName, tag, DrawCore.Figures);

                SaveFileDialog saveFileDialog = new() 
                {
                    Filter = "Файл уровня|*.json", 
                    Title = "Открытие уровня",
                    InitialDirectory = "C:\\Users\\" + Environment.UserName + "\\Documents\\LevelRedactor"
                };
                if (saveFileDialog.ShowDialog() == true)
                {
                    File.WriteAllText(saveFileDialog.FileName, jsonString);
                }
            }
        }
        private void OpenFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                Filter = "Файл уровня|*.json",
                Title = "Открытие уровня",
                InitialDirectory = "C:\\Users\\" + Environment.UserName + "\\Documents\\LevelRedactor",
                Multiselect = false
            };
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    DrawCore.Figures = Parser.Parser.FromJson(File.ReadAllText(openFileDialog.FileName));
                }
                catch (Exception)
                {
                    MessageBox.Show("Данный файл не является файлом уровня", "Ошибка", MessageBoxButton.OK);
                    return;
                }

                DrawCore.Canvas.Children.Clear();
                DrawCore.Figures.Clear();

                foreach (Figure figure in DrawCore.Figures)
                {
                    DrawCore.Canvas.Children.Add(figure);
                    DrawCore.Figures.Add(figure);
                    Canvas.SetLeft(figure, figure.DrawPoint.X);
                    Canvas.SetTop(figure, figure.DrawPoint.Y);
                    Canvas.SetZIndex(figure, figure.ZIndex);
                }
            }
        }
        private void OpenExportWindow(object sender, RoutedEventArgs e) => new ExportWindow().Show();
        private void FastExport(object sender, RoutedEventArgs e)
        {
            if (!IsDataCorrect())
                return;

            SetLevelDataWindow setLevelDataWindow = new();
            if (setLevelDataWindow.ShowDialog() == true)
            {
                if (setLevelDataWindow.DialogResult == false)
                    return;

                string title = setLevelDataWindow.LevelTitle;
                string tag = setLevelDataWindow.LevelTag;

                LevelData lvlData = new(DrawCore.Figures) { Title = title, Tag = tag };

                try
                {
                    LevelRepository lr = new();
                    lr.SendSetToServer(lvlData);
                }
                catch (Exception)
                {
                    MessageBox.Show("Сервер недоступен, проверьте соединение.", "Ошибка", MessageBoxButton.OK);
                }
                
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
