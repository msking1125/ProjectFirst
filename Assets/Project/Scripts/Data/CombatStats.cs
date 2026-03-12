using System;
using UnityEngine;

namespace ProjectFirst.Data
{
    /// <summary>
    /// ?꾪닾 ?ㅽ꺈 ?곗씠??(HP, ATK, DEF, 移섎챸?)
    /// </summary>
    [Serializable]
    public struct CombatStats
    {
        [Min(0f)] public float hp;
        [Min(0f)] public float atk;
        [Min(0f)] public float def;
        public float critChance;
        public float critMultiplier;

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

