using System;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

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
#if ODIN_INSPECTOR
    [Range(0f, 1f), LabelText(""), LabelWidth(1)]
#else
    [Range(0f, 1f)]
#endif
    public float critChance;
#if ODIN_INSPECTOR
    [Min(1f), LabelText(""), LabelWidth(1)]
#else
    [Min(1f)]
#endif
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
