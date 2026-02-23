#if UNITY_EDITOR
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class AgentStatsTableImporter
{
    private const string CsvPathLower = "Assets/Project/Data/agents.csv";
    private const string CsvPathUpper = "Assets/Project/Data/Agents.csv";
    private const string AssetPath = "Assets/Project/Data/AgentStatsTable.asset";

    [MenuItem("Tools/Game/Import Agent CSV")]
    public static void Import()
    {
        string csvPath = ResolveCsvPath();
        if (string.IsNullOrEmpty(csvPath))
        {
            Debug.LogError($"Agent CSV not found at: {CsvPathLower} (or {CsvPathUpper})");
            return;
        }

        string[] lines = File.ReadAllLines(csvPath);
        if (lines.Length < 2)
        {
            Debug.LogError("Agent CSV has no data rows.");
            return;
        }

        AgentStatsTable table = AssetDatabase.LoadAssetAtPath<AgentStatsTable>(AssetPath);
        if (table == null)
        {
            table = ScriptableObject.CreateInstance<AgentStatsTable>();
            AssetDatabase.CreateAsset(table, AssetPath);
        }

        table.rows.Clear();

        string[] header = lines[0].Split(',').Select(h => h.Trim()).ToArray();
        int idx(string name) => Array.IndexOf(header, name);

        string[] required = { "id", "name", "hp", "atk", "def", "critChance", "critMultiplier", "element" };
        foreach (string requiredColumn in required)
        {
            if (idx(requiredColumn) < 0)
            {
                Debug.LogError($"Missing column: {requiredColumn}");
                return;
            }
        }

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i]))
            {
                continue;
            }

            string[] cols = lines[i].Split(',').Select(c => c.Trim()).ToArray();
            string id = ReadCell(cols, idx("id"));
            string elementRaw = ReadCell(cols, idx("element"));

            AgentStatsRow row = new AgentStatsRow
            {
                id = id,
                name = ReadCell(cols, idx("name")),
                hp = ParseFloat(ReadCell(cols, idx("hp"))),
                atk = ParseFloat(ReadCell(cols, idx("atk"))),
                def = ParseFloat(ReadCell(cols, idx("def")), 0f),
                critChance = Mathf.Clamp01(ParseFloat(ReadCell(cols, idx("critChance")), 0f)),
                critMultiplier = Mathf.Max(1f, ParseFloat(ReadCell(cols, idx("critMultiplier")), 1.5f)),
                element = ParseElement(elementRaw, id)
            };

            table.rows.Add(row);
        }

        EditorUtility.SetDirty(table);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Imported {table.rows.Count} agents into {AssetPath}");
    }

    private static ElementType ParseElement(string raw, string id)
    {
        if (Enum.TryParse(raw, true, out ElementType element))
        {
            return element;
        }

        Debug.LogWarning($"[AgentStatsTableImporter] Failed to parse element for id '{id}'. raw='{raw}'");
        return ElementType.Reason;
    }

    private static string ResolveCsvPath()
    {
        if (File.Exists(CsvPathLower))
        {
            return CsvPathLower;
        }

        if (File.Exists(CsvPathUpper))
        {
            return CsvPathUpper;
        }

        return null;
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
}
#endif
