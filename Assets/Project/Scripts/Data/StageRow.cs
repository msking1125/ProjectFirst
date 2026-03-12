using System;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace ProjectFirst.Data
{
    /// <summary>
    /// CSV에서 임포트되는 스테이지 1행 데이터.
    /// </summary>
    [Serializable]
#if ODIN_INSPECTOR
    [HideLabel]
#endif
    public class StageRow
    {
#if ODIN_INSPECTOR
        [HorizontalGroup("기본", 0.33f)]
        [BoxGroup("기본/ID")]
        [LabelText("스테이지 ID")]
#endif
        public int id;

#if ODIN_INSPECTOR
        [HorizontalGroup("기본", 0.33f)]
        [BoxGroup("기본/챕터")]
        [LabelText("챕터 ID")]
#endif
        public int chapterId;

#if ODIN_INSPECTOR
        [HorizontalGroup("기본", 0.34f)]
        [BoxGroup("기본/순번")]
        [LabelText("순번")]
#endif
        public int stageNumber;

#if ODIN_INSPECTOR
        [BoxGroup("정보")]
        [LabelText("이름")]
#endif
        public string name;

#if ODIN_INSPECTOR
        [BoxGroup("정보")]
        [LabelText("설명")]
        [MultiLineProperty(2)]
#else
        [TextArea(2, 4)]
#endif
        public string description;

#if ODIN_INSPECTOR
        [HorizontalGroup("전투", 0.5f)]
        [BoxGroup("전투/전투력")]
        [LabelText("권장 전투력")]
        [ProgressBar(0, 100000)]
#endif
        public int recommendedPower;

#if ODIN_INSPECTOR
        [HorizontalGroup("전투", 0.5f)]
        [BoxGroup("전투/속성")]
        [LabelText("적 속성")]
        [EnumToggleButtons]
#endif
        public ElementType enemyElement = ElementType.Reason;

#if ODIN_INSPECTOR
        [HorizontalGroup("소모/보상", 0.5f)]
        [BoxGroup("소모/보상/스태미나")]
        [LabelText("소모 스태미나")]
        [SuffixLabel("stamina", true)]
#endif
        public int staminaCost;

#if ODIN_INSPECTOR
        [HorizontalGroup("소모/보상", 0.25f)]
        [BoxGroup("소모/보상/보상")]
        [LabelText("골드")]
#endif
        public int rewardGold;

#if ODIN_INSPECTOR
        [HorizontalGroup("소모/보상", 0.25f)]
        [BoxGroup("소모/보상/보상")]
        [LabelText("경험치")]
#endif
        public int rewardExp;

#if ODIN_INSPECTOR
        [BoxGroup("웨이브")]
        [LabelText("웨이브 데이터 ID")]
#endif
        public string waveDataId;
    }
}
