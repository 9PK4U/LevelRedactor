using System.Windows;

namespace LevelRedactor
{
    public partial class SetLevelDataWindow : Window
    {
        public SetLevelDataWindow()
        {
            InitializeComponent();
        }
        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        public string LevelName
        {
            get { return levelNameBox.Text; }
        }
        public string LevelTag
        {
            get { return levelTagBox.Text; }
        }
    }
}
