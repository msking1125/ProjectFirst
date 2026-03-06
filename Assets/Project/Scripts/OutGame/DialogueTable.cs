using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueTable", menuName = "MindArk/Dialogue Table")]
public class DialogueTable : ScriptableObject
{
    [SerializeField] private List<DialogueData> rows = new List<DialogueData>();
    private readonly Dictionary<string, List<DialogueData>> groupIndex = new Dictionary<string, List<DialogueData>>();

    public IReadOnlyList<DialogueData> Rows => rows;

    private void OnEnable() => RebuildIndex();
    private void OnValidate() => RebuildIndex();

    public List<DialogueData> GetByGroupId(string groupId)
    {
        if (string.IsNullOrWhiteSpace(groupId))
        {
            return new List<DialogueData>();
        }

        RebuildIndex();
        
        if (groupIndex.TryGetValue(groupId.Trim(), out List<DialogueData> list))
        {
            return list;
        }
        
        return new List<DialogueData>();
    }

    private void RebuildIndex()
    {
        groupIndex.Clear();
        if (rows == null)
        {
            return;
        }

        for (int i = 0; i < rows.Count; i++)
        {
            DialogueData row = rows[i];
            if (row == null || string.IsNullOrWhiteSpace(row.groupId))
            {
                continue;
            }

            string gid = row.groupId.Trim();
            if (!groupIndex.ContainsKey(gid))
            {
                groupIndex[gid] = new List<DialogueData>();
            }
            groupIndex[gid].Add(row);
        }
    }
}
