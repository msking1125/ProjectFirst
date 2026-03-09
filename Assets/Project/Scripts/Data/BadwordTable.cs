using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Badword Table", fileName = "BadwordTable")]
public class BadwordTable : ScriptableObject
{
    [SerializeField] private List<string> words = new();

    public IReadOnlyList<string> Words => words;

    public bool Contains(string input)
    {
        if (string.IsNullOrWhiteSpace(input) || words == null || words.Count == 0)
            return false;

        for (int i = 0; i < words.Count; i++)
        {
            string word = words[i];
            if (string.IsNullOrWhiteSpace(word))
                continue;

            if (input.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
        }

        return false;
    }

    public void SetWords(List<string> newWords)
    {
        words = newWords ?? new List<string>();
    }
}
