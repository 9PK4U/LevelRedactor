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

        public string LevelTitle
        {
            get { return levelTitleTextBox.Text; }
        }
        public string LevelTag
        {
            get { return levelTagTextBox.Text; }
        }
    }
}
