#if UNITY_EDITOR
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class AgentTableImporter
{
    private const string CsvPath = "Assets/Project/Data/agents.csv";
    private const string AssetPath = "Assets/Project/Data/AgentTable.asset";
    private static readonly string[] RequiredColumns =
    {
        "id", "name", "hp", "def", "critChance", "critMultiplier", "element"
    };

    [MenuItem("Tools/Game/Import Agent CSV")]
    public static void Import()
    {
        if (!File.Exists(CsvPath))
        {
            Debug.LogError($"Agent CSV not found at: {CsvPath}");
            return;
        }

        string[] lines = File.ReadAllLines(CsvPath);
        if (lines.Length < 2)
        {
            Debug.LogError("Agent CSV has no data rows.");
            return;
        }

        AgentTable table = AssetDatabase.LoadAssetAtPath<AgentTable>(AssetPath);
        if (table == null)
        {
            table = ScriptableObject.CreateInstance<AgentTable>();
            AssetDatabase.CreateAsset(table, AssetPath);
        }

        table.rows.Clear();

        string[] header = lines[0].Split(',').Select(h => h.Trim()).ToArray();
        int idx(string name) => Array.IndexOf(header, name);

        foreach (string col in RequiredColumns)
        {
            if (idx(col) < 0)
            {
                Debug.LogError($"Missing column: {col}");
                return;
            }
        }

        int idIdx = idx("id");
        int nameIdx = idx("name");
        int hpIdx = idx("hp");
        int atkIdx = idx("atk");
        if (atkIdx < 0)
        {
            atkIdx = idx("baseAtk");
        }
        int defIdx = idx("def");
        int critChanceIdx = idx("critChance");
        int critMultiplierIdx = idx("critMultiplier");
        int elementIdx = idx("element");

        if (atkIdx < 0)
        {
            Debug.LogError("Missing column: atk (or baseAtk)");
            return;
        }

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i]))
            {
                continue;
            }

            string[] cols = lines[i].Split(',').Select(c => c.Trim()).ToArray();
            string id = ReadCell(cols, idIdx);
            string elementRaw = ReadCell(cols, elementIdx);

            AgentRow row = new AgentRow
            {
                id = id,
                name = ReadCell(cols, nameIdx),
                hp = ParseFloat(ReadCell(cols, hpIdx)),
                atk = ParseFloat(ReadCell(cols, atkIdx)),
                def = ParseFloat(ReadCell(cols, defIdx)),
                critChance = Mathf.Clamp01(ParseFloat(ReadCell(cols, critChanceIdx))),
                critMultiplier = Mathf.Max(1f, ParseFloat(ReadCell(cols, critMultiplierIdx), 1.5f)),
                element = ParseElement(elementRaw, id)
            };

            table.rows.Add(row);
        }

        EditorUtility.SetDirty(table);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Imported {table.rows.Count} agents into {AssetPath}");
    }

    [MenuItem("Assets/Import CSV/Agent Table", false, 2001)]
    private static void ImportFromAssetsMenu()
    {
        Import();
    }

    private static string ReadCell(string[] cols, int index)
    {
        if (index < 0 || cols == null || index >= cols.Length)
        {
            return string.Empty;
        }

        return cols[index];
    }

    private static float ParseFloat(string s, float defaultValue = 0f)
    {
        if (float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out float v))
        {
            return v;
        }

        if (float.TryParse(s, out v))
        {
            return v;
        }

        return defaultValue;
    }

    private static ElementType ParseElement(string raw, string id)
    {
        if (Enum.TryParse(raw, true, out ElementType element))
        {
            return element;
        }

        Debug.LogWarning($"[AgentTableImporter] Failed to parse element for id '{id}'. raw='{raw}'. fallback=Reason");
        return ElementType.Reason;
    }
}
#endif
