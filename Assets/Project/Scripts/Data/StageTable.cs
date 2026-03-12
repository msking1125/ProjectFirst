using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProjectFirst.Data
{
    /// <summary>
    /// Documentation cleaned.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Stage Table")]
    public class StageTable : ScriptableObject
    {
        public List<StageRow> rows = new List<StageRow>();

        private Dictionary<int, StageRow> _idIndex;
        private Dictionary<int, List<StageRow>> _chapterIndex;

        private void OnEnable() => BuildIndex();
        private void OnValidate() => BuildIndex();

        private void BuildIndex()
        {
            _idIndex = new Dictionary<int, StageRow>();
            _chapterIndex = new Dictionary<int, List<StageRow>>();
            if (rows == null) return;

            foreach (var row in rows)
            {
                if (row == null) continue;

                if (row.id > 0)
                    _idIndex[row.id] = row;

                if (!_chapterIndex.TryGetValue(row.chapterId, out var list))
                {
                    list = new List<StageRow>();
                    _chapterIndex[row.chapterId] = list;
                }
                list.Add(row);
            }
        }

        /// <summary>
        /// Documentation cleaned.
        /// </summary>
        public StageRow GetById(int id)
        {
            BuildIndex();
            return _idIndex.TryGetValue(id, out var row) ? row : null;
        }

        /// <summary>
        /// Documentation cleaned.
        /// </summary>
        public List<StageRow> GetByChapter(int chapterId)
        {
            BuildIndex();
            if (_chapterIndex.TryGetValue(chapterId, out var list))
                return list.OrderBy(s => s.stageNumber).ToList();
            return new List<StageRow>();
        }
    }
}
