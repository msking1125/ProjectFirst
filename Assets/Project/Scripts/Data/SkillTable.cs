using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Skill Table")]
public class SkillTable : ScriptableObject
{
    [SerializeField] private List<SkillRow> rows = new List<SkillRow>();
    private readonly Dictionary<int, SkillRow> index = new Dictionary<int, SkillRow>();

    public IReadOnlyList<SkillRow> Rows => rows;
    public IReadOnlyList<SkillRow> AllSkills => rows;
    public IReadOnlyDictionary<int, SkillRow> Index => index;

    private void OnEnable() => RebuildIndex();
    private void OnValidate() => RebuildIndex();

    public SkillRow GetById(int id)
    {
        if (id <= 0)
        {
            return null;
        }

        RebuildIndex();
        index.TryGetValue(id, out SkillRow row);
        return row;
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
            SkillRow row = rows[i];
            if (row == null || row.id <= 0)
            {
                continue;
            }

            index[row.id] = row;
        }
    }
}
