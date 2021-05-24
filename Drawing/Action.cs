using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LevelRedactor.Drawing
{
    public enum ActionTypes
    {
        Draw,
        Move,
        Resize,
        Unit,
        Link,
        Choice
    }
    public enum DrawingType
    {
        Rect,
        Ellipse,
        Line,
        Triengle
    }

    public class Action : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private ActionTypes type;
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
        public DrawingType DrawingType { get; set; }
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
