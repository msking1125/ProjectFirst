using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Agent Stats Table")]
public class AgentStatsTable : ScriptableObject
{
    [SerializeField] private CombatStats defaultStats = new CombatStats(100f, 3f, 0f, 0f, 1.5f);
    [SerializeField] private ElementType defaultElement = ElementType.Reason;
    public List<AgentStatsRow> rows = new();

    private readonly Dictionary<string, AgentStatsRow> index = new();

    private void OnEnable()
    {
        RebuildIndex();
    }

    private void OnValidate()
    {
        RebuildIndex();
    }

    public CombatStats GetStats(string agentId)
    {
        AgentStatsRow row = GetById(agentId);
        return row != null ? row.stats.Sanitized() : defaultStats.Sanitized();
    }

    public ElementType GetElement(string agentId)
    {
        AgentStatsRow row = GetById(agentId);
        return row != null ? row.element : defaultElement;
    }

    private AgentStatsRow GetById(string agentId)
    {
        if (string.IsNullOrWhiteSpace(agentId))
        {
            return null;
        }

        if (index.Count != rows.Count)
        {
            RebuildIndex();
        }

        return index.TryGetValue(agentId, out AgentStatsRow row) ? row : null;
    }

    private void RebuildIndex()
    {
        index.Clear();

        if (rows == null)
        {
            return;
        }

        for (int i = 0; i < rows.Count; i++)
        {
            AgentStatsRow row = rows[i];
            if (row == null || string.IsNullOrWhiteSpace(row.id))
            {
                continue;
            }

            index[row.id] = row;
        }
    }
}
