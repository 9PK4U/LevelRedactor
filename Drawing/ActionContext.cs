using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace LevelRedactor.Drawing
{
    public class ActionContext : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Color fillColor;
        private Color borderColor;
        private int borderWidth;

        public Color FillColor 
        {
            get => fillColor;
            set
            {
                fillColor = value;
                OnPropertyChanged("FillColor");
            }
        }
        public Color BorderColor 
        {
            get => borderColor;
            set
            {
                borderColor = value;
                OnPropertyChanged("BorderColor");
            }
        }
        public int BorderWidth 
        {
            get => borderWidth;
            set
            {
                borderWidth = value;
                OnPropertyChanged("BorderWidth");
            }
        }

        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
