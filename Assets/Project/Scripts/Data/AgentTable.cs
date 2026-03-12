using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Agent Table")]
public class AgentTable : ScriptableObject
{
    [SerializeField] private CombatStats defaultStats = new CombatStats(100f, 3f, 0f, 0f, 1.5f);
    [SerializeField] private ElementType defaultElement = ElementType.Reason;
    public List<AgentRow> rows = new();

    [Header("캐릭터 상세 정보")]
    [SerializeField] private List<AgentInfo> _agentInfos = new();

    private readonly Dictionary<int, AgentRow> index = new();

    private void OnEnable()
    {
        RebuildIndex();
    }

    private void OnValidate()
    {
        RebuildIndex();
    }

    public AgentRow GetById(int id)
    {
        if (id <= 0)
        {
            return null;
        }

        if (index.Count != rows.Count)
        {
            RebuildIndex();
        }

        return index.TryGetValue(id, out AgentRow row) ? row : null;
    }

    public CombatStats GetStats(int id)
    {
        AgentRow row = GetById(id);
        return row != null ? row.ToCombatStats() : defaultStats.Sanitized();
    }

    public ElementType GetElement(int id)
    {
        AgentRow row = GetById(id);
        return row != null ? row.element : defaultElement;
    }

    /// <summary>모든 AgentInfo 목록을 반환합니다.</summary>
    public IReadOnlyList<AgentInfo> GetAll() => _agentInfos;

    /// <summary>ID로 AgentInfo를 검색합니다.</summary>
    public AgentInfo GetAgentInfo(int id)
    {
        for (int i = 0; i < _agentInfos.Count; i++)
        {
            if (_agentInfos[i] != null && _agentInfos[i].id == id)
            {
                return _agentInfos[i];
            }
        }
        return null;
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
            if (row == null || row.id <= 0)
            {
                continue;
            }

            index[row.id] = row;
        }
    }
}
