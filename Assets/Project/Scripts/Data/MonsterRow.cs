using System;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace ProjectFirst.Data
{
    /// <summary>
    /// 몬스터 데이터 행
    /// </summary>
    [Serializable]
#if ODIN_INSPECTOR
    [HideLabel]
#endif
    public class MonsterRow
    {
#if ODIN_INSPECTOR
        [HorizontalGroup("기본", 0.5f)]
        [BoxGroup("기본/정보")]
        [LabelText("ID")]
#endif
        public int id;

#if ODIN_INSPECTOR
        [HorizontalGroup("기본", 0.5f)]
        [BoxGroup("기본/정보")]
        [LabelText("이름")]
#endif
        public string name;

#if ODIN_INSPECTOR
        [HorizontalGroup("기본", 0.5f)]
        [BoxGroup("기본/등급")]
        [LabelText("등급")]
        [EnumToggleButtons]
#endif
        public MonsterGrade grade = MonsterGrade.Normal;

#if ODIN_INSPECTOR
        [BoxGroup("전투 스탯")]
        [HideLabel]
        [InlineProperty]
#endif
        public CombatStats stats;

#if ODIN_INSPECTOR
        [HorizontalGroup("속성/이동", 0.5f)]
        [BoxGroup("속성/이동/속성")]
        [LabelText("속성")]
        [EnumToggleButtons]
#endif
        public ElementType element = ElementType.Reason;

#if ODIN_INSPECTOR
        [HorizontalGroup("속성/이동", 0.5f)]
        [BoxGroup("속성/이동/이동")]
        [LabelText("이동속도")]
        [SuffixLabel("m/s", true)]
#endif
        public float moveSpeed = -1f;

#if ODIN_INSPECTOR
        [HorizontalGroup("보상", 0.5f)]
        [BoxGroup("보상/경험치")]
        [LabelText("경험치")]
#endif
        public int expReward;

#if ODIN_INSPECTOR
        [HorizontalGroup("보상", 0.5f)]
        [BoxGroup("보상/골드")]
        [LabelText("골드")]
#endif
        public int goldReward;

#if ODIN_INSPECTOR
        [BoxGroup("프리팹")]
        [LabelText("몬스터 프리팹")]
        [AssetsOnly]
        [PreviewField(80, ObjectFieldAlignment.Left)]
#endif
        public GameObject prefab;

        public CombatStats ToCombatStats()
        {
            return stats.Sanitized();
        }
    }
}
