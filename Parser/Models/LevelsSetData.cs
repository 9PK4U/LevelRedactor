using System;
using System.Collections.Generic;

namespace LevelRedactor.Parser.Models
{
    [Serializable]
    public class LevelsSetData
    {
        public string Tematic { get; set; }
        public List<LevelData> Levels { get; set; }

        public LevelsSetData()
        { }
    }
}
