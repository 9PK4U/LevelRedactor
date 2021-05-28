using System.Windows;

namespace LevelRedactor.Drawing
{
    public class  DrawingState
    {
        public bool IsDrawing { get; set; }
        public Point OffsetPoint { get; set; }
        public Point LastPoint { get; set; }
        public int LastZIndex { get; set; }
    }
}
