using UnityEngine;
using System;

[Serializable]
public class AgentRow
{
    public int id;
    public string name;
    public float hp;
    public float atk;
    public float def;
    public float critChance;
    public float critMultiplier = 1.5f;
    public ElementType element = ElementType.Reason;
    public Sprite portrait;

    public CombatStats ToCombatStats()
    {
        return new CombatStats(hp, atk, def, critChance, critMultiplier).Sanitized();
    }
}
