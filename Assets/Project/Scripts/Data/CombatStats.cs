using System;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace ProjectFirst.Data
{
    /// <summary>
    /// 전투 스탯 데이터 (HP, ATK, DEF, 치명타)
    /// </summary>
    [Serializable]
#if ODIN_INSPECTOR
    [InlineProperty]
#endif
    public struct CombatStats
    {
#if ODIN_INSPECTOR
        [BoxGroup("기본 스탯")]
        [LabelText("HP")]
        [ProgressBar(0, 10000, ColorGetter = "GetHpColor")]
#endif
        [Min(0f)] public float hp;

#if ODIN_INSPECTOR
        [BoxGroup("기본 스탯")]
        [LabelText("ATK")]
        [ProgressBar(0, 500, ColorGetter = "GetAtkColor")]
#endif
        [Min(0f)] public float atk;

#if ODIN_INSPECTOR
        [BoxGroup("기본 스탯")]
        [LabelText("DEF")]
        [ProgressBar(0, 200, ColorGetter = "GetDefColor")]
#endif
        [Min(0f)] public float def;

#if ODIN_INSPECTOR
        [BoxGroup("치명타")]
        [LabelText("치명타 확률")]
        [ShowIf("ShowCritFields")]
        [Range(0f, 1f)]
#endif
        public float critChance;

#if ODIN_INSPECTOR
        [BoxGroup("치명타")]
        [LabelText("치명타 배율")]
        [ShowIf("ShowCritFields")]
        [Min(1f)]
#endif
        public float critMultiplier;

#if ODIN_INSPECTOR
        private static Color GetHpColor() => new Color(1f, 0.3f, 0.3f);
        private static Color GetAtkColor() => new Color(1f, 0.6f, 0.2f);
        private static Color GetDefColor() => new Color(0.3f, 0.6f, 1f);
        private bool ShowCritFields() => critChance > 0;
#endif

        public CombatStats(float hpValue, float atkValue, float defValue, float critChanceValue, float critMultiplierValue)
        {
            hp = Mathf.Max(0f, hpValue);
            atk = Mathf.Max(0f, atkValue);
            def = Mathf.Max(0f, defValue);
            critChance = Mathf.Clamp01(critChanceValue);
            critMultiplier = Mathf.Max(1f, critMultiplierValue);
        }

        public CombatStats Sanitized()
        {
            return new CombatStats(hp, atk, def, critChance, critMultiplier);
        }

        public CombatStats Multiply(float hpRatio, float atkRatio, float defRatio)
        {
            return new CombatStats(
                hp * hpRatio,
                atk * atkRatio,
                def * defRatio,
                critChance,
                critMultiplier
            ).Sanitized();
        }
    }
}
