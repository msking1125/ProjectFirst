using System;

[Serializable]
public class AgentStatsRow
{
    public string id;
    public string name;
    public float hp;
    public float atk;
    public float def;
    public float critChance;
    public float critMultiplier = 1.5f;
    public ElementType element = ElementType.Reason;

    public CombatStats stats => new CombatStats(hp, atk, def, critChance, critMultiplier).Sanitized();
}
