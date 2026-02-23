using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Skill Table")]
public class SkillTable : ScriptableObject
{
    [SerializeField] private List<SkillRow> rows = new List<SkillRow>();
    private readonly Dictionary<string, SkillRow> index = new Dictionary<string, SkillRow>();

    public IReadOnlyList<SkillRow> Rows => rows;
    public IReadOnlyList<SkillRow> AllSkills => rows;
    public IReadOnlyDictionary<string, SkillRow> Index => index;

    private void OnEnable() => RebuildIndex();
    private void OnValidate() => RebuildIndex();

    public SkillRow GetById(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        RebuildIndex();
        index.TryGetValue(id.Trim(), out SkillRow row);
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
            if (row == null || string.IsNullOrWhiteSpace(row.id))
            {
                continue;
            }

            index[row.id.Trim()] = row;
        }
    }
}
