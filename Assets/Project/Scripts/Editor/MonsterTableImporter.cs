#if UNITY_EDITOR
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class MonsterTableImporter
{
    private const string CsvPathLower = "Assets/Project/Data/monsters.csv";
    private const string CsvPathUpper = "Assets/Project/Data/Monsters.csv";
    private const string AssetPath = "Assets/Project/Data/MonsterTable.asset";

    [MenuItem("Tools/Game/Import Monster CSV")]
    public static void Import()
    {
        string csvPath = ResolveCsvPath();
        if (string.IsNullOrEmpty(csvPath))
        {
            Debug.LogError($"Monster CSV not found at: {CsvPathLower} (or {CsvPathUpper})");
            return;
        }

        string[] lines = File.ReadAllLines(csvPath);
        if (lines.Length < 2)
        {
            Debug.LogError("Monster CSV has no data rows.");
            return;
        }

        MonsterTable table = AssetDatabase.LoadAssetAtPath<MonsterTable>(AssetPath);
        if (table == null)
        {
            table = ScriptableObject.CreateInstance<MonsterTable>();
            AssetDatabase.CreateAsset(table, AssetPath);
        }

        table.rows.Clear();

        string[] header = lines[0].Split(',').Select(h => h.Trim()).ToArray();
        int idx(string name) => Array.IndexOf(header, name);

        string[] required = { "id", "hp", "atk", "def", "critChance" };
        foreach (string r in required)
        {
            if (idx(r) < 0)
            {
                Debug.LogError($"Missing column: {r}");
                return;
            }
        }

        int critMultiplierIdx = idx("critMultiplier");
        if (critMultiplierIdx < 0)
        {
            critMultiplierIdx = idx("critMul");
        }

        int moveSpeedIdx = idx("moveSpeed");
        int gradeIdx = idx("grade");

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i]))
            {
                continue;
            }

            string[] cols = lines[i].Split(',').Select(c => c.Trim()).ToArray();
            MonsterRow row = new MonsterRow
            {
                id = ReadCell(cols, idx("id")),
                hp = ParseFloat(ReadCell(cols, idx("hp"))),
                atk = ParseFloat(ReadCell(cols, idx("atk"))),
                def = ParseFloat(ReadCell(cols, idx("def"))),
                critChance = Mathf.Clamp01(ParseFloat(ReadCell(cols, idx("critChance")))),
                critMultiplier = Mathf.Max(1f, ParseFloat(ReadCell(cols, critMultiplierIdx)))
            };

            if (gradeIdx >= 0)
            {
                string gradeRaw = ReadCell(cols, gradeIdx);
                if (Enum.TryParse(gradeRaw, true, out MonsterGrade grade))
                {
                    row.grade = grade;
                }
            }

            if (moveSpeedIdx >= 0)
            {
                row.moveSpeed = ParseFloat(ReadCell(cols, moveSpeedIdx));
            }

            table.rows.Add(row);
        }

        EditorUtility.SetDirty(table);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Imported {table.rows.Count} monsters into {AssetPath}");
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

    private static float ParseFloat(string s)
    {
        if (float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out float v))
        {
            return v;
        }

        if (float.TryParse(s, out v))
        {
            return v;
        }

        return 0f;
    }
}
#endif
