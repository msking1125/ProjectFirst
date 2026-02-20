using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName = "Game/Monster Table")]
public class MonsterTable : ScriptableObject
{
    [TableList]
    public List<MonsterRow> rows = new();

    private readonly Dictionary<string, MonsterRow> index = new();

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

        if (index.Count != rows.Count)
        {
            RebuildIndex();
        }

        return index.TryGetValue(id, out MonsterRow row) ? row : null;
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
            if (row == null || string.IsNullOrWhiteSpace(row.id))
            {
                continue;
            }

            index[row.id] = row;
        }
    }
}
