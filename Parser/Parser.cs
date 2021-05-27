using System;
using System.Text.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using LevelRedactor.Drawing;
using LevelRedactor.Parser.Models;

namespace LevelRedactor.Parser
{
    public static class Parser
    {
        public static string ToJson(string title, string tag, IList<Figure> figures)
        {
            LevelData levelData = new(figures);
            levelData.Title = title;
            levelData.Tag = tag;

            return JsonSerializer.Serialize(levelData);
        }
        public static ObservableCollection<Figure> FromJson(string data)
        {
            LevelData ld = JsonSerializer.Deserialize<LevelData>(data);

            ObservableCollection<Figure> figures = new();

            foreach (FigureData item in ld.FiguresData)
                figures.Add(new Figure(item));

            return figures;
        }
        public static LevelData GetLevelData(string data)
        {
            return JsonSerializer.Deserialize<LevelData>(data);
        }
    }
}
