using System;
using System.Windows;
using System.Windows.Media;
using System.ComponentModel;
using System.Windows.Controls;
using System.Collections.Generic;
using LevelRedactor.Parser.Models;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;


namespace LevelRedactor.Drawing
{
    public class Figure : Border, ICloneable, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private static int counter;
        private int id;
        private int majorFigureId = 0;
        private int zIndex;
        private Point drawPoint;
        private string title;

        public int Id
        {
            get => id;
            set => id = value;
        }
        public int MajorFigureId 
        {
            get => majorFigureId;
            set
            {
                majorFigureId = value;
                OnPropertyChanged("MajorFigureId");
            }
        }
        public string Title
        {
            get => title;
            set
            {
                title = value;
                OnPropertyChanged("Title");
            }
        }
        public int ZIndex
        {
            get => zIndex;
            set
            {
                zIndex = value;
                OnPropertyChanged("ZIndex");
            }
        }
        public Point DrawPoint
        {
            get => drawPoint;
            set
            {
                drawPoint = value;
                OnPropertyChanged("DrawPoint");
            }
        }
        public Point AnchorPoint { get; set; }
        public List<int> AnchorFiguresId { get; set; }
        public ObservableCollection<Primitive> Primitives { get; set; }

        public Figure() : base()
        {
            
            id = ++counter;
            Title = "Фигура_" + id;
            Child = new Image() { Stretch = Stretch.Fill };
            AnchorFiguresId = new List<int>();
            Primitives = new ObservableCollection<Primitive>();

            Primitives.CollectionChanged += RedrawFigure;
        }
        public Figure(FigureData figureData) : base()
        {
            Child = new Image() { Stretch = Stretch.Fill };
            AnchorFiguresId = new List<int>();
            Primitives = new ObservableCollection<Primitive>();

            Primitives.CollectionChanged += RedrawFigure;

            Title = figureData.Title;
            DrawPoint = figureData.Drawpoint;
            ZIndex = figureData.ZIndex;
            Id = figureData.Id;
            counter = id > counter ? id : counter;
            MajorFigureId = figureData.MajorFigureId;
            AnchorFiguresId = figureData.AnchorFiguresId;
            AnchorPoint = figureData.AnchorPoint;

            foreach (PrimitiveData item in figureData.PrimitivesData)
                Primitives.Add(new Primitive(item, DrawPoint));
        }

        private void RedrawFigure(object sender, EventArgs e)
        {
            if (Primitives.Count > 0)
            {
                DrawingGroup drawingGroup = new();
                foreach (var primitive in Primitives)
                {
                    drawingGroup.Children.Add(primitive.GeometryDrawing);
                }

                ((Image)this.Child).Source = new DrawingImage(drawingGroup);
            }
        }
        public void OnPropertyChanged([CallerMemberName] string prop = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        public object Clone()
        {
            Figure newFigure = new()
            {
                Child = new Image(),
                DrawPoint = DrawPoint
            };

            foreach (var item in Primitives)
            {
                newFigure.Primitives.Add((Primitive)item.Clone());
            }

            return newFigure;
        }
    }
}
