using System;
using UnityEngine;

[Serializable]
public class MonsterRow
{
    public string id;
    public float hp;
    public float atk;
    public float def;
    [Range(0f, 1f)] public float critChance;
    [Min(1f)] public float critMul = 1f;
    public float moveSpeed = -1f;

    public CombatStats ToCombatStats()
    {
        return new CombatStats(hp, atk, def, critChance, critMul);
    }
}
