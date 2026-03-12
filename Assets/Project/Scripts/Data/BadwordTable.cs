using System;
using System.Collections.Generic;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace ProjectFirst.Data
{
    /// <summary>
    /// 금칙어 테이블
    /// </summary>
#if ODIN_INSPECTOR
    [CreateAssetMenu(menuName = "Soul Ark/Badword Table", fileName = "BadwordTable")]
#else
    [CreateAssetMenu(menuName = "Game/Badword Table", fileName = "BadwordTable")]
#endif
    public class BadwordTable : ScriptableObject
    {
#if ODIN_INSPECTOR
        [Title("금칙어 목록", TitleAlignment = TitleAlignments.Centered)]
        [ListDrawerSettings(Expanded = true, ShowPaging = true, NumberOfItemsPerPage = 20, IsReadOnly = false)]
        [Searchable]
#endif
        [SerializeField] private List<string> words = new();

        public IReadOnlyList<string> Words => words;

#if ODIN_INSPECTOR
        [Button("CSV에서 임포트", ButtonSizes.Medium)]
        [GUIColor(0.3f, 0.7f, 1f)]
#endif
        public void ImportFromCsv()
        {
            // CSV 임포트 로직 (에디터에서 구현)
        }

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
}
