using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName = "Game/Monster Table")]
public class MonsterTable : ScriptableObject
{
    [TableList]
    public List<MonsterRow> rows = new();

    private readonly Dictionary<string, MonsterRow> index = new();

    private static string NormalizeKey(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        return raw.Trim().ToLowerInvariant();
    }

    private void OnEnable()
    {
        RebuildIndex();
    }

    private void OnValidate()
    {
        RebuildIndex();
    }

    public MonsterRow GetById(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        RebuildIndex();

        string key = NormalizeKey(id);
        if (string.IsNullOrEmpty(key))
        {
            return null;
        }

        return index.TryGetValue(key, out MonsterRow row) ? row : null;
    }

    public MonsterRow GetByIdAndGrade(string id, MonsterGrade grade)
    {
        MonsterRow baseRow = GetById(id);
        if (baseRow == null)
        {
            return null;
        }

        if (baseRow.grade == grade)
        {
            return baseRow;
        }

        if (rows == null)
        {
            return baseRow;
        }

        for (int i = 0; i < rows.Count; i++)
        {
            MonsterRow row = rows[i];
            if (row == null)
            {
                continue;
            }

            if (row.grade == grade && row.id == baseRow.id)
            {
                return row;
            }
        }

        return baseRow;
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
            MonsterRow row = rows[i];
            if (row == null)
            {
                continue;
            }

            string idKey = NormalizeKey(row.id);
            if (!string.IsNullOrEmpty(idKey))
            {
                index[idKey] = row;
            }

            string nameKey = NormalizeKey(row.name);
            if (!string.IsNullOrEmpty(nameKey))
            {
                index[nameKey] = row;
            }
        }
    }
}
