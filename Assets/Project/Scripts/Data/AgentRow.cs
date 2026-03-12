using UnityEngine;
using System;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace ProjectFirst.Data
{
    /// <summary>
    /// 에이전트(캐릭터) 기본 데이터 행
    /// </summary>
    [Serializable]
#if ODIN_INSPECTOR
    [HideLabel]
#endif
    public class AgentRow
    {
#if ODIN_INSPECTOR
        [HorizontalGroup("기본", 0.5f)]
        [BoxGroup("기본/ID")]
        [LabelText("ID")]
#endif
        public int id;

#if ODIN_INSPECTOR
        [HorizontalGroup("기본", 0.5f)]
        [BoxGroup("기본/이름")]
        [LabelText("캐릭터명")]
#endif
        public string name;

#if ODIN_INSPECTOR
        [BoxGroup("전투 스탯")]
        [HideLabel]
        [InlineProperty]
#endif
        public CombatStats stats;

#if ODIN_INSPECTOR
        [HorizontalGroup("속성", 0.5f)]
        [BoxGroup("속성/타입")]
        [LabelText("속성")]
        [EnumToggleButtons]
#endif
        public ElementType element = ElementType.Reason;

#if ODIN_INSPECTOR
        [HorizontalGroup("속성", 0.5f)]
        [BoxGroup("속성/아이콘")]
        [LabelText("초상화")]
        [PreviewField(60, ObjectFieldAlignment.Left)]
#endif
        public Sprite portrait;

        public CombatStats ToCombatStats()
        {
            return stats.Sanitized();
        }
    }
}
