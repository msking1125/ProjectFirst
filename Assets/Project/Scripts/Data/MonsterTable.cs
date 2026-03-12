using System.Collections.Generic;
using UnityEngine;

namespace ProjectFirst.Data
{
    [CreateAssetMenu(menuName = "Game/Monster Table")]
    public class MonsterTable : ScriptableObject
    {
        public List<MonsterRow> rows = new();

        private readonly Dictionary<int, MonsterRow> _idLookup = new();
        private readonly Dictionary<string, MonsterRow> _nameLookup = new();

        private static string NormalizeKey(string raw)
        {
            return string.IsNullOrWhiteSpace(raw) ? null : raw.Trim().ToLowerInvariant();
        }

        private void OnEnable() => BuildIndex();
        private void OnValidate() => BuildIndex();

        private void BuildIndex()
        {
            _idLookup.Clear();
            _nameLookup.Clear();
            if (rows == null) return;

            foreach (MonsterRow row in rows)
            {
                if (row == null) continue;

                string nameKey = NormalizeKey(row.name);
                if (row.id > 0)
                {
                    _idLookup[row.id] = row;
                }

                if (!string.IsNullOrEmpty(nameKey))
                {
                    _nameLookup[nameKey] = row;
                }
            }
        }

        public MonsterRow GetById(int id)
        {
            if (id <= 0) return null;
            BuildIndex();
            return _idLookup.TryGetValue(id, out MonsterRow row) ? row : null;
        }

        public MonsterRow GetByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;
            BuildIndex();
            string key = NormalizeKey(name);
            return string.IsNullOrEmpty(key) ? null : _nameLookup.TryGetValue(key, out MonsterRow row) ? row : null;
        }

        public MonsterRow GetByIdAndGrade(int id, MonsterGrade grade)
        {
            MonsterRow baseRow = GetById(id);
            if (baseRow == null) return null;
            if (baseRow.grade == grade) return baseRow;
            if (rows == null) return baseRow;

            foreach (MonsterRow row in rows)
            {
                if (row == null) continue;
                if (row.id == baseRow.id && row.grade == grade) return row;
            }

            return baseRow;
        }
    }
}

