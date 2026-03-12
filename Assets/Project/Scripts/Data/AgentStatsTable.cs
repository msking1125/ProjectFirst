using System.Collections.Generic;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace ProjectFirst.Data
{
    /// <summary>
    /// 에이전트(캐릭터) 스탯 데이터 테이블
    /// </summary>
#if ODIN_INSPECTOR
    [CreateAssetMenu(menuName = "Soul Ark/Agent Stats Table")]
#else
    [CreateAssetMenu(menuName = "Game/Agent Stats Table")]
#endif
    public class AgentStatsTable : ScriptableObject
    {
#if ODIN_INSPECTOR
        [BoxGroup("기본 설정")]
        [LabelText("기본 스탯")]
        [InlineProperty]
#endif
        [SerializeField] private CombatStats _defaultStats = new CombatStats(100f, 3f, 0f, 0f, 1.5f);

#if ODIN_INSPECTOR
        [BoxGroup("기본 설정")]
        [LabelText("기본 속성")]
        [EnumToggleButtons]
#endif
        [SerializeField] private ElementType _defaultElement = ElementType.Reason;

#if ODIN_INSPECTOR
        [Title("에이전트 스탯 목록", TitleAlignment = TitleAlignments.Centered)]
        [TableList(ShowIndexLabels = true, AlwaysExpanded = true)]
        [Searchable]
#endif
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

#if ODIN_INSPECTOR
        [Button("인덱스 재구축", ButtonSizes.Medium)]
        [GUIColor(0.3f, 0.8f, 0.3f)]
#endif
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
