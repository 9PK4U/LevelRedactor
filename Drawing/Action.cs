using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LevelRedactor.Drawing
{
    public enum ActionTypes
    {
        Draw,
        Move,
        Unit,
        Link,
        Choice,
        Fill
    }
    public enum DrawingType
    {
        Rect,
        Ellipse,
        Line,
        Triengle,
        Polygon,
        Polyline
    }

    public class Action : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private ActionTypes type;
        private DrawingType drawingType;
        private ActionContext context;

        public ActionTypes Type
        {
            get => type;
            set
            {
                type = value;
                OnPropertyChanged("Type");
            }
        }
        public DrawingType DrawingType 
        {
            get => drawingType; 
            set
            {
                drawingType = value;
                OnPropertyChanged("DrawingType");
            }
        }
        public ActionContext Context
        {
            get => context;
            set
            {
                context = value;
                OnPropertyChanged("Context");
            }
        }

        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
