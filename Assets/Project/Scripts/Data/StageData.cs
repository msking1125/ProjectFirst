using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace ProjectFirst.Data
{
    /// <summary>
    /// 스테이지 정보를 보관하는 ScriptableObject 에셋.
    /// 생성: Project 우클릭 → Create/Soul Ark/Data/StageData
    /// </summary>
#if ODIN_INSPECTOR
    [CreateAssetMenu(fileName = "StageData", menuName = "Soul Ark/Data/StageData")]
#else
    [CreateAssetMenu(fileName = "StageData", menuName = "MindArk/Data/StageData")]
#endif
    public class StageData : ScriptableObject
    {
        [System.Serializable]
#if ODIN_INSPECTOR
        [HideLabel]
#endif
        public class StageInfo
        {
#if ODIN_INSPECTOR
            [HorizontalGroup("기본", 0.5f)]
            [BoxGroup("기본/ID")]
            [LabelText("스테이지 ID")]
            [Tooltip("스테이지 고유 ID")]
#endif
            public int stageId;

#if ODIN_INSPECTOR
            [HorizontalGroup("기본", 0.5f)]
            [BoxGroup("기본/챕터")]
            [LabelText("챕터 ID")]
            [Tooltip("소속 챕터 ID")]
#endif
            public int chapterId;

#if ODIN_INSPECTOR
            [HorizontalGroup("기본", 0.5f)]
            [BoxGroup("기본/순번")]
            [LabelText("순번")]
            [Tooltip("챕터 내 순번 (1부터 시작)")]
#endif
            public int stageNumber;

#if ODIN_INSPECTOR
            [HorizontalGroup("기본", 0.5f)]
            [BoxGroup("기본/이름")]
            [LabelText("스테이지명")]
#endif
            public string stageName;

#if ODIN_INSPECTOR
            [BoxGroup("정보")]
            [LabelText("설명")]
            [MultiLineProperty(3)]
#else
            [TextArea(2, 4)]
#endif
            public string description;

#if ODIN_INSPECTOR
            [HorizontalGroup("전투", 0.5f)]
            [BoxGroup("전투/전투력")]
            [LabelText("권장 전투력")]
            [ProgressBar(0, 100000)]
            [Tooltip("권장 전투력")]
#endif
            public int recommendedPower;

#if ODIN_INSPECTOR
            [HorizontalGroup("전투", 0.5f)]
            [BoxGroup("전투/속성")]
            [LabelText("적 속성")]
            [EnumToggleButtons]
            [Tooltip("주요 적 속성")]
#endif
            public ElementType enemyElement;

#if ODIN_INSPECTOR
            [HorizontalGroup("소모", 0.5f)]
            [BoxGroup("소모/스태미나")]
            [LabelText("소모 스태미나")]
            [Tooltip("소모 스태미나")]
#endif
            public int staminaCost;

#if ODIN_INSPECTOR
            [HorizontalGroup("상태", 0.5f)]
            [BoxGroup("상태/별점")]
            [LabelText("클리어 별점")]
            [ProgressBar(0, 3, ColorGetter = "GetStarColor")]
            [Tooltip("클리어 별점 (0~3)")]
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
            [BoxGroup("보상")]
            [LabelText("보상 미리보기")]
            [TableList(ShowIndexLabels = true, AlwaysExpanded = false)]
            [Tooltip("보상 미리보기 목록")]
#endif
            public List<RewardItem> previewRewards = new List<RewardItem>();

#if ODIN_INSPECTOR
            private static Color GetStarColor() => new Color(1f, 0.8f, 0.2f);
#endif
        }

#if ODIN_INSPECTOR
        [Title("스테이지 목록", TitleAlignment = TitleAlignments.Centered)]
        [TableList(ShowIndexLabels = true, AlwaysExpanded = true)]
        [Searchable]
#endif
        public List<StageInfo> stages = new List<StageInfo>();

        /// <summary>
        /// 특정 챕터에 속한 스테이지를 stageNumber 오름차순으로 반환합니다.
        /// </summary>
        public List<StageInfo> GetByChapter(int chapterId) =>
            stages.Where(s => s.chapterId == chapterId)
                  .OrderBy(s => s.stageNumber)
                  .ToList();
    }
}
