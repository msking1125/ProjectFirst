using System.Collections.Generic;
using UnityEngine;

namespace ProjectFirst.OutGame.Data
{
    public class BadWordData
    {
        private HashSet<string> badWordsSet;

        public BadWordData(TextAsset csvFile)
        {
            badWordsSet = new HashSet<string>();
            ParseCSV(csvFile);
        }

        private void ParseCSV(TextAsset csvFile)
        {
            if (csvFile == null) return;
            string[] lines = csvFile.text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < lines.Length; i++)
            {
                string word = lines[i].Trim();
                if (!string.IsNullOrEmpty(word))
                {
                    badWordsSet.Add(word);
                }
            }
        }
        public bool ContainsBadWord(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            foreach (var badWord in badWordsSet)
            {
                if (text.Contains(badWord))
                {
                    return true;
                }
            }
            return false;
        }
    }
}

