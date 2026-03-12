using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProjectFirst.Data
{
    [CreateAssetMenu(fileName = "ChapterData", menuName = "MindArk/Data/ChapterData")]
    public class ChapterData : ScriptableObject
    {
        [System.Serializable]
        public class ChapterInfo
        {
            public int chapterId;

            public string chapterName;

            [TextArea(2, 4)]
            public string description;

            public Sprite chapterIcon;

            public Sprite worldMapIcon;

            public Vector2 worldMapPosition;

            [Range(0, 3)]
            public int clearStars;

            public bool isUnlocked;

            public List<int> stageIds = new List<int>();

        }

        public List<ChapterInfo> chapters = new List<ChapterInfo>();
        public ChapterInfo GetById(int id) =>
            chapters.FirstOrDefault(c => c.chapterId == id);
    }
}

