using System;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace ProjectFirst.Data
{
    [Serializable]
#if ODIN_INSPECTOR
    [HideLabel]
#endif
    public class CharacterProgressRecord
    {
#if ODIN_INSPECTOR
        [HorizontalGroup("진행", 0.33f)]
        [BoxGroup("진행/에이전트")]
        [LabelText("Agent ID")]
#endif
        [SerializeField] private int _agentId;

#if ODIN_INSPECTOR
        [HorizontalGroup("진행", 0.33f)]
        [BoxGroup("진행/레벨")]
        [LabelText("레벨")]
        [ProgressBar(1, 100, ColorGetter = "GetLevelColor")]
#endif
        [SerializeField] private int _level = 1;

#if ODIN_INSPECTOR
        [HorizontalGroup("진행", 0.34f)]
        [BoxGroup("진행/경험치")]
        [LabelText("경험치")]
        [ProgressBar(0, 1000, ColorGetter = "GetExpColor")]
#endif
        [SerializeField] private int _exp;

#if ODIN_INSPECTOR
        private static Color GetLevelColor() => new Color(0.3f, 0.7f, 1f);
        private static Color GetExpColor() => new Color(0.3f, 0.8f, 0.3f);
#endif

        public int agentId { get => _agentId; set => _agentId = value; }
        public int level { get => _level; set => _level = Mathf.Max(1, value); }
        public int exp { get => _exp; set => _exp = Mathf.Max(0, value); }
    }

    [Serializable]
#if ODIN_INSPECTOR
    [HideLabel]
#endif
    public class ExpItemInventoryRecord
    {
#if ODIN_INSPECTOR
        [HorizontalGroup("아이템", 0.5f)]
        [BoxGroup("아이템/타입")]
        [LabelText("타입")]
        [EnumToggleButtons]
#endif
        [SerializeField] private ExpItemType _type;

#if ODIN_INSPECTOR
        [HorizontalGroup("아이템", 0.5f)]
        [BoxGroup("아이템/수량")]
        [LabelText("수량")]
        [ProgressBar(0, 999)]
#endif
        [SerializeField] private int _count;

        public ExpItemType type { get => _type; set => _type = value; }
        public int count { get => _count; set => _count = Mathf.Max(0, value); }
    }
}