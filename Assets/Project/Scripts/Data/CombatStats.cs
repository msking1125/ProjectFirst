using System;
using UnityEngine;

[Serializable]
public struct CombatStats
{
    [Min(0f)] public float hp;
    [Min(0f)] public float atk;
    [Min(0f)] public float def;
    [Range(0f, 1f)] public float critChance;
    [Min(1f)] public float critMultiplier;

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
}
