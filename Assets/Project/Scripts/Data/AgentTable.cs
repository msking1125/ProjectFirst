using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Agent Table")]
public class AgentTable : ScriptableObject
{
    [SerializeField] private CombatStats defaultStats = new CombatStats(100f, 3f, 0f, 0f, 1.5f);
    [SerializeField] private ElementType defaultElement = ElementType.Reason;
    public List<AgentRow> rows = new();

    private readonly Dictionary<string, AgentRow> index = new();

    private void OnEnable()
    {
        RebuildIndex();
    }

    private void OnValidate()
    {
        RebuildIndex();
    }

    public AgentRow GetById(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        if (index.Count != rows.Count)
        {
            RebuildIndex();
        }

        return index.TryGetValue(id, out AgentRow row) ? row : null;
    }

    public CombatStats GetStats(string id)
    {
        AgentRow row = GetById(id);
        return row != null ? row.ToCombatStats() : defaultStats.Sanitized();
    }

    public ElementType GetElement(string id)
    {
        AgentRow row = GetById(id);
        return row != null ? row.element : defaultElement;
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
            AgentRow row = rows[i];
            if (row == null || string.IsNullOrWhiteSpace(row.id))
            {
                continue;
            }

            index[row.id] = row;
        }
    }
}
