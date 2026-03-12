using System.Collections.Generic;
using UnityEngine;

namespace ProjectFirst.Data
{
    /// <summary>
    /// 에이전트(캐릭터) 데이터 테이블
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Agent Table")]
    public class AgentTable : ScriptableObject
    {
        [SerializeField] private CombatStats _defaultStats = new CombatStats(100f, 3f, 0f, 0f, 1.5f);

        [SerializeField] private ElementType _defaultElement = ElementType.Reason;

        public List<AgentRow> rows = new();

        [Header("캐릭터 상세 정보")]
        [SerializeField] private List<AgentInfo> _agentInfos = new();

        private readonly Dictionary<int, AgentRow> _index = new();

        private void OnEnable() => RebuildIndex();
        private void OnValidate() => RebuildIndex();

        /// <summary>ID로 AgentRow를 조회합니다.</summary>
        public AgentRow GetById(int id)
        {
            if (id <= 0) return null;
            if (_index.Count != rows.Count) RebuildIndex();
            return _index.TryGetValue(id, out AgentRow row) ? row : null;
        }

        /// <summary>ID로 스탯을 조회합니다.</summary>
        public CombatStats GetStats(int id)
        {
            AgentRow row = GetById(id);
            return row != null ? row.ToCombatStats() : _defaultStats.Sanitized();
        }

        /// <summary>ID로 속성을 조회합니다.</summary>
        public ElementType GetElement(int id)
        {
            AgentRow row = GetById(id);
            return row != null ? row.element : _defaultElement;
        }

        /// <summary>모든 AgentInfo 목록을 반환합니다.</summary>
        public IReadOnlyList<AgentInfo> GetAll() => _agentInfos;

        /// <summary>ID로 AgentInfo를 검색합니다.</summary>
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
