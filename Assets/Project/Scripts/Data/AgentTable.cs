using System.Collections.Generic;
using UnityEngine;

namespace ProjectFirst.Data
{
    /// <summary>
    /// Documentation cleaned.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Agent Table")]
    public class AgentTable : ScriptableObject
    {
        [SerializeField] private CombatStats _defaultStats = new CombatStats(100f, 3f, 0f, 0f, 1.5f);

        [SerializeField] private ElementType _defaultElement = ElementType.Reason;

        public List<AgentRow> rows = new();

        [Header("Settings")]
        [SerializeField] private List<AgentInfo> _agentInfos = new();

        private readonly Dictionary<int, AgentRow> _index = new();

        private void OnEnable() => RebuildIndex();
        private void OnValidate() => RebuildIndex();

        /// Documentation cleaned.
        public AgentRow GetById(int id)
        {
            if (id <= 0) return null;
            if (_index.Count != rows.Count) RebuildIndex();
            return _index.TryGetValue(id, out AgentRow row) ? row : null;
        }

        /// Documentation cleaned.
        public CombatStats GetStats(int id)
        {
            AgentRow row = GetById(id);
            return row != null ? row.ToCombatStats() : _defaultStats.Sanitized();
        }

        /// Documentation cleaned.
        public ElementType GetElement(int id)
        {
            AgentRow row = GetById(id);
            return row != null ? row.element : _defaultElement;
        }

        /// Documentation cleaned.
        public IReadOnlyList<AgentInfo> GetAll() => _agentInfos;

        /// Documentation cleaned.
        public AgentInfo GetAgentInfo(int id)
        {
            for (int i = 0; i < _agentInfos.Count; i++)
            {
                if (_agentInfos[i] != null && _agentInfos[i].id == id)
                    return _agentInfos[i];
            }
            return null;
        }

        private void RebuildIndex()
        {
            _index.Clear();
            if (rows == null) return;

            for (int i = 0; i < rows.Count; i++)
            {
                AgentRow row = rows[i];
                if (row == null || row.id <= 0) continue;
                _index[row.id] = row;
            }
        }
    }
}
