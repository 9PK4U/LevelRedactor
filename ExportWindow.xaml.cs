using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using LevelRedactor.Parser.Models;
using System.Collections.ObjectModel;

namespace LevelRedactor
{
    /// <summary>
    /// Логика взаимодействия для ExportWindow.xaml
    /// </summary>
    public partial class ExportWindow : Window
    {
        ObservableCollection<LevelData> levels = new();
        public ExportWindow()
        {
            InitializeComponent();

            dataGrid.ItemsSource = levels;

        }

        private void AddFiles(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                Filter = "Файл уровня|*.json",
                Title = "Добавление файлов",
                InitialDirectory = "C:\\Users\\" + Environment.UserName + "\\Documents\\LevelRedactor",
                Multiselect = true
            };
            if (openFileDialog.ShowDialog() == true)
            {
                string[] paths = openFileDialog.FileNames;

                foreach (string path in paths)
                {
                    try
                    {
                        levels.Add(Parser.Parser.GetLevelData(File.ReadAllText(path)));
                    }
                    catch (Exception)
                    { }
                    
                }
            }
        }
        private void RemoveFile(object sender, RoutedEventArgs e)
        {
            if (dataGrid.SelectedItem is not null)
            {
                levels.Remove((LevelData)dataGrid.SelectedItem);
            }
        }
        private void SendFiles(object sender, RoutedEventArgs e)
        {
            foreach (LevelData level in levels)
            {
                try
                {
                    LevelRepository lr = new();
                    lr.SendSetToServer(level);
                }
                catch (Exception)
                {
                    MessageBox.Show("Сервер недоступен, проверьте соединение.", "Ошибка", MessageBoxButton.OK);
                    return;
                }
            }
        }
    }
}
