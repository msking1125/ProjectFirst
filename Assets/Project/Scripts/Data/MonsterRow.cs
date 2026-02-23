using System;
using UnityEngine;
using Sirenix.OdinInspector;

[Serializable]
public class MonsterRow
{
    public string id;
    public MonsterGrade grade = MonsterGrade.Normal;
    public string name;
    public GameObject prefab;
    public float hp;
    public float atk;
    public float def;
    [Range(0f, 1f), LabelText(""), LabelWidth(1)]
    public float critChance;
    [Min(1f), LabelText(""), LabelWidth(1)]
    public float critMultiplier = 1f;
    public float moveSpeed = -1f;
    public ElementType element = ElementType.Reason;
    public int expReward;
    public int goldReward;

    public CombatStats ToCombatStats()
    {
        return new CombatStats(hp, atk, def, critChance, critMultiplier);
    }
}
