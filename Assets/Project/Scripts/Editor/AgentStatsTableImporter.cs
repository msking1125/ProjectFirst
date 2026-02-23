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
    private const string AssetPath   = "Assets/Project/Data/AgentStatsTable.asset";

    [MenuItem("Tools/Game/Import Agent CSV")]
    public static void Import()
    {
        string csvPath = File.Exists(CsvPathLower) ? CsvPathLower : File.Exists(CsvPathUpper) ? CsvPathUpper : null;
        if (string.IsNullOrEmpty(csvPath))
        {
            Debug.LogError($"Agent CSV not found at: {CsvPathLower} (or {CsvPathUpper})");
            return;
        }

        var lines = File.ReadAllLines(csvPath);
        if (lines.Length < 2)
        {
            Debug.LogError("Agent CSV has no data rows.");
            return;
        }

        var table = AssetDatabase.LoadAssetAtPath<AgentStatsTable>(AssetPath) 
                    ?? ScriptableObject.CreateInstance<AgentStatsTable>();
        if (AssetDatabase.LoadAssetAtPath<AgentStatsTable>(AssetPath) == null)
            AssetDatabase.CreateAsset(table, AssetPath);

        table.rows.Clear();

        var header = lines[0].Split(',').Select(h => h.Trim()).ToArray();
        int idx(string col) => Array.IndexOf(header, col);

        string[] required = { "id", "name", "hp", "atk", "def", "critChance", "critMultiplier", "element" };
        foreach (var c in required)
            if (idx(c) < 0)
            {
                Debug.LogError($"Missing column: {c}");
                return;
            }

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            var cols = lines[i].Split(',').Select(c => c.Trim()).ToArray();

            AgentStatsRow row = new AgentStatsRow
            {
                id = SafeCell(cols, idx("id")),
                name = SafeCell(cols, idx("name")),
                hp = ParseFloat(SafeCell(cols, idx("hp"))),
                atk = ParseFloat(SafeCell(cols, idx("atk"))),
                def = ParseFloat(SafeCell(cols, idx("def"))),
                critChance = Mathf.Clamp01(ParseFloat(SafeCell(cols, idx("critChance")))),
                critMultiplier = Mathf.Max(1f, ParseFloat(SafeCell(cols, idx("critMultiplier")), 1.5f)),
                element = ParseElement(SafeCell(cols, idx("element")), SafeCell(cols, idx("id")))
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
        if (Enum.TryParse(raw, true, out ElementType e)) return e;
        Debug.LogWarning($"[AgentStatsTableImporter] Failed to parse element for id '{id}'. raw='{raw}'");
        return ElementType.Reason;
    }

    private static string SafeCell(string[] cols, int idx)
        => (idx >= 0 && cols != null && idx < cols.Length) ? cols[idx] : string.Empty;

    private static float ParseFloat(string s, float def = 0f)
    {
        if (float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out float v)) return v;
        if (float.TryParse(s, out v)) return v;
        return def;
    }
}
#endif
