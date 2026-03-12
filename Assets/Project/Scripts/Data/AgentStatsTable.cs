using System.Collections.Generic;
using UnityEngine;

namespace ProjectFirst.Data
{
    [CreateAssetMenu(menuName = "Game/Agent Stats Table")]
    public class AgentStatsTable : ScriptableObject
    {
        [SerializeField] private CombatStats _defaultStats = new CombatStats(100f, 3f, 0f, 0f, 1.5f);

        [SerializeField] private ElementType _defaultElement = ElementType.Reason;

        public List<AgentStatsRow> rows = new();

        private readonly Dictionary<int, AgentStatsRow> _index = new();

        private void OnEnable() => RebuildIndex();
        private void OnValidate() => RebuildIndex();

        public CombatStats GetStats(int agentId)
        {
            AgentStatsRow row = GetById(agentId);
            return row != null ? row.stats.Sanitized() : _defaultStats.Sanitized();
        }

        public ElementType GetElement(int agentId)
        {
            AgentStatsRow row = GetById(agentId);
            return row != null ? row.element : _defaultElement;
        }

        private AgentStatsRow GetById(int agentId)
        {
            if (agentId <= 0) return null;
            if (_index.Count != rows.Count) RebuildIndex();
            return _index.TryGetValue(agentId, out AgentStatsRow row) ? row : null;
        }

        private void RebuildIndex()
        {
            _index.Clear();
            if (rows == null) return;

            for (int i = 0; i < rows.Count; i++)
            {
                AgentStatsRow row = rows[i];
                if (row == null || row.id <= 0) continue;
                _index[row.id] = row;
            }
        }
    }
}

