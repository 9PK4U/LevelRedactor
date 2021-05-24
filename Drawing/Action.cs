using System;
using System.Collections.Generic;
using System.Linq;
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

    public class Action
    {
        public ActionTypes Type { get; set; }
        public DrawingType DrawingType { get; set; }
        public ActionContext Context { get; set; }
    }
}
