using System.Collections.Generic;
using UnityEngine;

namespace ProjectFirst.Data
{
    /// <summary>
    /// 스킬 데이터 테이블
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Skill Table")]
    public class SkillTable : ScriptableObject
    {
        [SerializeField] private List<SkillRow> _rows = new();

        private readonly Dictionary<int, SkillRow> _index = new();

        public IReadOnlyList<SkillRow> Rows => _rows;
        public IReadOnlyList<SkillRow> AllSkills => _rows;
        public IReadOnlyDictionary<int, SkillRow> Index => _index;

        private void OnEnable() => RebuildIndex();
        private void OnValidate() => RebuildIndex();

        /// <summary>ID로 스킬을 조회합니다.</summary>
        public SkillRow GetById(int id)
        {
            if (id <= 0) return null;
            RebuildIndex();
            _index.TryGetValue(id, out SkillRow row);
            return row;
        }

        private void RebuildIndex()
        {
            _index.Clear();
            if (_rows == null) return;

            for (int i = 0; i < _rows.Count; i++)
            {
                SkillRow row = _rows[i];
                if (row == null || row.id <= 0) continue;
                _index[row.id] = row;
            }
        }
    }
}
