using System;
using UnityEngine;

[Serializable]
public class MonsterRow
{
    public string id;
    public MonsterGrade grade = MonsterGrade.Normal;
    public GameObject prefab;
    public float hp;
    public float atk;
    public float def;
    [Range(0f, 1f)] public float critChance;
    [Min(1f)] public float critMultiplier = 1f;
    public float moveSpeed = -1f;

    public CombatStats ToCombatStats()
    {
        return new CombatStats(hp, atk, def, critChance, critMultiplier);
    }
}
