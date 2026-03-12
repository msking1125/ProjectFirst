using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace ProjectFirst.Data
{
    /// <summary>
    /// 챕터(월드맵 섬) 정보를 보관하는 ScriptableObject 에셋.
    /// 생성: Project 우클릭 → Create/Soul Ark/Data/ChapterData
    /// </summary>
#if ODIN_INSPECTOR
    [CreateAssetMenu(fileName = "ChapterData", menuName = "Soul Ark/Data/ChapterData")]
#else
    [CreateAssetMenu(fileName = "ChapterData", menuName = "MindArk/Data/ChapterData")]
#endif
    public class ChapterData : ScriptableObject
    {
        [System.Serializable]
#if ODIN_INSPECTOR
        [HideLabel]
#endif
        public class ChapterInfo
        {
#if ODIN_INSPECTOR
            [HorizontalGroup("기본", 0.5f)]
            [BoxGroup("기본/ID")]
            [LabelText("챕터 ID")]
            [Tooltip("챕터 고유 ID (1~10)")]
#endif
            public int chapterId;

#if ODIN_INSPECTOR
            [HorizontalGroup("기본", 0.5f)]
            [BoxGroup("기본/이름")]
            [LabelText("챕터명")]
#endif
            public string chapterName;

#if ODIN_INSPECTOR
            [BoxGroup("정보")]
            [LabelText("설명")]
            [MultiLineProperty(3)]
#else
            [TextArea(2, 4)]
#endif
            public string description;

#if ODIN_INSPECTOR
            [HorizontalGroup("아이콘", 0.5f)]
            [BoxGroup("아이콘/챕터")]
            [LabelText("챕터 아이콘")]
            [Tooltip("챕터 선택 화면용 아이콘")]
            [PreviewField(60, ObjectFieldAlignment.Left)]
#endif
            public Sprite chapterIcon;

#if ODIN_INSPECTOR
            [HorizontalGroup("아이콘", 0.5f)]
            [BoxGroup("아이콘/월드맵")]
            [LabelText("월드맵 아이콘")]
            [Tooltip("월드맵 위 섬 이미지")]
            [PreviewField(60, ObjectFieldAlignment.Left)]
#endif
            public Sprite worldMapIcon;

#if ODIN_INSPECTOR
            [BoxGroup("위치")]
            [LabelText("월드맵 위치")]
            [Tooltip("월드맵 상 배치 위치 (px)")]
#endif
            public Vector2 worldMapPosition;

#if ODIN_INSPECTOR
            [HorizontalGroup("상태", 0.5f)]
            [BoxGroup("상태/별점")]
            [LabelText("클리어 별점")]
            [ProgressBar(0, 3, ColorGetter = "GetStarColor")]
            [Tooltip("챕터 클리어 별점 (0~3)")]
#endif
            [Range(0, 3)]
            public int clearStars;

#if ODIN_INSPECTOR
            [HorizontalGroup("상태", 0.5f)]
            [BoxGroup("상태/잠금")]
            [LabelText("잠금 해제")]
            [ToggleLeft]
#endif
            public bool isUnlocked;

#if ODIN_INSPECTOR
            [BoxGroup("스테이지")]
            [LabelText("스테이지 ID 목록")]
            [ListDrawerSettings(Expanded = false, ShowPaging = true)]
            [Tooltip("해당 챕터에 포함된 스테이지 ID 목록")]
#endif
            public List<int> stageIds = new List<int>();

#if ODIN_INSPECTOR
            private static Color GetStarColor() => new Color(1f, 0.8f, 0.2f);
#endif
        }

#if ODIN_INSPECTOR
        [Title("챕터 목록", TitleAlignment = TitleAlignments.Centered)]
        [TableList(ShowIndexLabels = true, AlwaysExpanded = true)]
        [Searchable]
#endif
        public List<ChapterInfo> chapters = new List<ChapterInfo>();

        /// <summary>
        /// chapterId에 해당하는 ChapterInfo를 반환합니다. 없으면 null.
        /// </summary>
        public ChapterInfo GetById(int id) =>
            chapters.FirstOrDefault(c => c.chapterId == id);
    }
}
