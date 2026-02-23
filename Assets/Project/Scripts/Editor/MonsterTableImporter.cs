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
        foreach (string requiredColumn in required)
        {
            if (idx(requiredColumn) < 0)
            {
                Debug.LogError($"Missing column: {requiredColumn}");
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
        int elementIdx = idx("element");

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i]))
            {
                continue;
            }

            string[] cols = lines[i].Split(',').Select(c => c.Trim()).ToArray();
            string id = ReadCell(cols, idx("id"));
            MonsterRow row = new MonsterRow
            {
                id = id,
                hp = ParseFloat(ReadCell(cols, idx("hp"))),
                atk = ParseFloat(ReadCell(cols, idx("atk"))),
                def = ParseFloat(ReadCell(cols, idx("def"))),
                critChance = Mathf.Clamp01(ParseFloat(ReadCell(cols, idx("critChance"))),
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
                string moveSpeedRaw = ReadCell(cols, moveSpeedIdx);
                if (TryParseFloat(moveSpeedRaw, out float moveSpeedValue))
                {
                    row.moveSpeed = moveSpeedValue;
                }
                else
                {
                    Debug.LogWarning($"[MonsterTableImporter] Failed to parse moveSpeed for id '{id}'. raw='{moveSpeedRaw}'");
                }
            }

            if (elementIdx >= 0)
            {
                string elementRaw = ReadCell(cols, elementIdx);
                if (Enum.TryParse(elementRaw, true, out ElementType element))
                {
                    row.element = element;
                }
                else
                {
                    Debug.LogWarning($"[MonsterTableImporter] Failed to parse element for id '{id}'. raw='{elementRaw}'");
                    row.element = ElementType.Reason;
                }
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
        if (TryParseFloat(s, out float v))
        {
            return v;
        }

        return 0f;
    }

    private static bool TryParseFloat(string s, out float v)
    {
        if (float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out v))
        {
            return true;
        }

        if (float.TryParse(s, out v))
        {
            return true;
        }

        v = 0f;
        return false;
    }
}
#endif
