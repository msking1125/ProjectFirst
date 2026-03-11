using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 챕터(월드맵 섬) 정보를 보관하는 ScriptableObject 에셋.
/// 생성: Project 우클릭 → Create/MindArk/Data/ChapterData
/// </summary>
[CreateAssetMenu(fileName = "ChapterData", menuName = "MindArk/Data/ChapterData")]
public class ChapterData : ScriptableObject
{
    [System.Serializable]
    public class ChapterInfo
    {
        [Tooltip("챕터 고유 ID (1~10)")]
        public int chapterId;

        public string chapterName;

        [TextArea(2, 4)]
        public string description;

        [Tooltip("챕터 선택 화면용 아이콘")]
        public Sprite chapterIcon;

        [Tooltip("월드맵 위 섬 이미지")]
        public Sprite worldMapIcon;

        [Tooltip("월드맵 상 배치 위치 (px)")]
        public Vector2 worldMapPosition;

        [Range(0, 3)]
        [Tooltip("챕터 클리어 별점 (0~3)")]
        public int clearStars;

        public bool isUnlocked;

        [Tooltip("해당 챕터에 포함된 스테이지 ID 목록")]
        public List<int> stageIds = new List<int>();
    }

    public List<ChapterInfo> chapters = new List<ChapterInfo>();

    /// <summary>
    /// chapterId에 해당하는 ChapterInfo를 반환합니다. 없으면 null.
    /// </summary>
    public ChapterInfo GetById(int id) =>
        chapters.FirstOrDefault(c => c.chapterId == id);
}
