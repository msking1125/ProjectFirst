using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProjectFirst.Data
{
    /// <summary>
    /// 챕터(월드맵 섬) 정보를 보관하는 ScriptableObject 에셋.
    /// 생성: Project 우클릭 → Create/Soul Ark/Data/ChapterData
    /// </summary>
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

        /// <summary>
        /// chapterId에 해당하는 ChapterInfo를 반환합니다. 없으면 null.
        /// </summary>
        public ChapterInfo GetById(int id) =>
            chapters.FirstOrDefault(c => c.chapterId == id);
    }
}
