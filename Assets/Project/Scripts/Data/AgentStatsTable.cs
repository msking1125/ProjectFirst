using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Agent Stats Table")]
public class AgentStatsTable : ScriptableObject
{
    [SerializeField] private CombatStats defaultStats = new CombatStats(100f, 3f, 0f, 0f, 1.5f);
    [SerializeField] private List<AgentStatsRow> rows = new();

    public CombatStats GetStats(string agentId)
    {
        if (!string.IsNullOrWhiteSpace(agentId) && rows != null)
        {
            for (int i = 0; i < rows.Count; i++)
            {
                AgentStatsRow row = rows[i];
                if (row == null || string.IsNullOrWhiteSpace(row.id))
                {
                    continue;
                }

                if (row.id == agentId)
                {
                    return row.stats.Sanitized();
                }
            }
        }

        return defaultStats.Sanitized();
    }
}

[System.Serializable]
public class AgentStatsRow
{
    public string id;
    public CombatStats stats = new CombatStats(100f, 3f, 0f, 0f, 1.5f);
}
