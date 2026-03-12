using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProjectFirst.Data
{
    /// <summary>
    /// Documentation cleaned.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Chapter Table")]
    public class ChapterTable : ScriptableObject
    {
        public List<ChapterRow> rows = new List<ChapterRow>();

        private Dictionary<int, ChapterRow> _index;

        private void OnEnable() => BuildIndex();
        private void OnValidate() => BuildIndex();

        private void BuildIndex()
        {
            _index = new Dictionary<int, ChapterRow>();
            if (rows == null) return;

            foreach (var row in rows)
            {
                if (row != null && row.id > 0)
                    _index[row.id] = row;
            }
        }

        /// <summary>
        /// Documentation cleaned.
        /// </summary>
        public ChapterRow GetById(int id)
        {
            BuildIndex();
            return _index.TryGetValue(id, out var row) ? row : null;
        }

        /// <summary>
        /// Documentation cleaned.
        /// </summary>
        public List<ChapterRow> GetAll() =>
            rows?.OrderBy(r => r.id).ToList() ?? new List<ChapterRow>();
    }
}
