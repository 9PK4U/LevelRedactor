﻿using LevelRedactor.Parser.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


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
        public Point AnchorPoint { get; set; }
        public List<int> AnchorFiguresId { get; set; }
        private double angle = 0;
        public double Angle 
        { 
            get { return angle; } 
            set
            {
                if (value >= 0 && value <= 359)
                {
                    angle = value;
                    Child.RenderTransform = new RotateTransform(value);
                }
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
        public ObservableCollection<Primitive> Primitives { get; set; }

        public Figure() : base()
        {
            id = ++counter;
            Title = "Фигура_" + id;
            Child = new Image() { Stretch = Stretch.Fill };
            AnchorFiguresId = new List<int>();
            Primitives = new ObservableCollection<Primitive>();

            

            Primitives.CollectionChanged += (s, e) =>
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
            };
        }
        public Figure(Parser.Models.FigureData figureData) : base()
        {
            Child = new Image() { Stretch = Stretch.Fill };
            AnchorFiguresId = new List<int>();
            Primitives = new ObservableCollection<Primitive>();

            Primitives.CollectionChanged += (s, e) =>
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
            };

            Title = figureData.Title;
            DrawPoint = figureData.Drawpoint;
            ZIndex = figureData.ZIndex;
            Id = figureData.Id;
            counter = id > counter ? id : counter;
            MajorFigureId = figureData.MajorFigureId;
            AnchorFiguresId = figureData.AnchorFiguresId;
            AnchorPoint = figureData.AnchorPoint;

            foreach (PrimitiveData item in figureData.PrimitivesData)
                Primitives.Add(new Primitive(item));

            Angle = figureData.Angle;
        }

        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        public object Clone()
        {
            Figure newFigure = new()
            {
                Child = new Image(),
                Angle = Angle,
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