using System;
using System.Windows;
using LevelRedactor.Drawing;
using System.Collections.Generic;

namespace LevelRedactor.Parser.Models
{
    [Serializable]
    public class FigureData
    {
        public string Title { get; set; }
        public int Id { get; set; }
        public int MajorFigureId { get; set; }
        public Point AnchorPoint { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int ZIndex { get; set; }
        public Point Drawpoint { get; set; }
        public List<int> AnchorFiguresId { get; set; }
        public List<PrimitiveData> PrimitivesData { get; set; }

        public FigureData()
        {
        }
        public FigureData(Figure figure)
        {
            PrimitivesData = new();

            Id = figure.Id;
            Title = figure.Title;
            ZIndex = figure.ZIndex;
            Width = (int)figure.ActualWidth;
            Height = (int)figure.ActualHeight;
            MajorFigureId = figure.MajorFigureId;
            AnchorFiguresId = figure.AnchorFiguresId;
            Drawpoint = new((int)figure.DrawPoint.X,(int)figure.DrawPoint.Y);
            AnchorPoint = new((int)figure.AnchorPoint.X, (int)figure.AnchorPoint.Y);

            foreach (Primitive primitive in figure.Primitives)
            {
                PrimitivesData.Add(new(primitive, Drawpoint));
            }
        }
    }
}
