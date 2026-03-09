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
    private const string PrefabDirectory = "Assets/Project/Prefabs/Enemy";
    private static readonly string[] RequiredColumns =
    {
        "id", "grade", "name", "hp", "atk", "def", "critChance", "critMultiplier", "moveSpeed", "element", "prefab"
    };

    public static void Import()
    {
        // Allow for either 'monsters.csv' or 'Monsters.csv' for case-insensitivity
        if (!CsvImportUtility.TryResolveCsvPath(out string csvPath, CsvPathLower, CsvPathUpper))
        {
            Debug.LogError($"Monster CSV not found at: {CsvPathLower} (or {CsvPathUpper})");
            return;
        }

        if (!CsvImportUtility.TryReadCsvLines(csvPath, out string[] lines))
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

        string[] header = CsvImportUtility.ParseHeader(lines[0]);
        int idx(string name) => CsvImportUtility.FindColumn(header, name);

        foreach (string col in RequiredColumns)
        {
            if (idx(col) < 0)
            {
                Debug.LogError($"Missing column: '{col}'");
                return;
            }
        }

        int idIdx = idx("id");
        int nameIdx = idx("name");
        int hpIdx = idx("hp");
        int atkIdx = idx("atk");
        int defIdx = idx("def");
        int critChanceIdx = idx("critChance");
        int critMultiplierIdx = idx("critMultiplier");
        int moveSpeedIdx = idx("moveSpeed");
        int gradeIdx = idx("grade");
        int elementIdx = idx("element");
        int prefabIdx = idx("prefab");
        int expRewardIdx = idx("expReward");
        int goldRewardIdx = idx("goldReward");

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i];
            if (string.IsNullOrWhiteSpace(line))
                continue;

            string[] cols = CsvImportUtility.ParseRow(line, header.Length);

            if (!int.TryParse(SafeGet(cols, idIdx), out int id))
                continue;

            string name = SafeGet(cols, nameIdx);

            // Parse critMultiplier: Remove all non-numeric, non-dot, non-minus chars
            float critMultiplierValue = 1f;
            string critMultiplierStr = SafeGet(cols, critMultiplierIdx);
            if (!string.IsNullOrEmpty(critMultiplierStr))
            {
                string numericPart = new string(critMultiplierStr.Where(c => char.IsDigit(c) || c == '.' || c == '-').ToArray());
                if (!float.TryParse(numericPart, NumberStyles.Float, CultureInfo.InvariantCulture, out critMultiplierValue) || critMultiplierValue < 1f)
                {
                    critMultiplierValue = 1f;
                }
            }

            MonsterRow row = new MonsterRow
            {
                id = id,
                name = name,
                hp = ParseFloat(SafeGet(cols, hpIdx)),
                atk = ParseFloat(SafeGet(cols, atkIdx)),
                def = ParseFloat(SafeGet(cols, defIdx)),
                critChance = Mathf.Clamp01(ParseFloat(SafeGet(cols, critChanceIdx))),
                critMultiplier = critMultiplierValue,
                // If prefabName is missing, ResolvePrefab returns null and logs warning
                prefab = ResolvePrefab(SafeGet(cols, prefabIdx), id),
                expReward = ParseOptionalReward(cols, expRewardIdx, "expReward", id),
                goldReward = ParseOptionalReward(cols, goldRewardIdx, "goldReward", id)
            };

            string gradeRaw = SafeGet(cols, gradeIdx);
            if (CsvImportUtility.TryParseEnumInsensitive(gradeRaw, out MonsterGrade grade))
            {
                row.grade = grade;
            }
            else
            {
                Debug.LogWarning($"[MonsterTableImporter] Failed to parse grade for id '{id}'. raw='{gradeRaw ?? "(null)"}'. fallback=Normal");
                row.grade = MonsterGrade.Normal;
            }

            string moveSpeedRaw = SafeGet(cols, moveSpeedIdx);
            if (TryParseFloat(moveSpeedRaw, out float moveSpeedValue))
            {
                row.moveSpeed = moveSpeedValue;
            }
            else if (!string.IsNullOrWhiteSpace(moveSpeedRaw))
            {
                Debug.LogWarning($"[MonsterTableImporter] Failed to parse moveSpeed for id '{id}'. raw='{moveSpeedRaw}'");
            }

            string elementRaw = SafeGet(cols, elementIdx);
            if (CsvImportUtility.TryParseEnumInsensitive(elementRaw, out ElementType element))
            {
                row.element = element;
            }
            else
            {
                Debug.LogWarning($"[MonsterTableImporter] Failed to parse element for id '{id}'. raw='{elementRaw ?? "(null)"}'. fallback=Reason");
                row.element = ElementType.Reason;
            }

            table.rows.Add(row);
        }

        EditorUtility.SetDirty(table);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Imported {table.rows.Count} monster{(table.rows.Count == 1 ? "" : "s")} into {AssetPath}");
    }

    /// <summary>
    /// Safe column getter, returns trimmed value or empty string on error.
    /// </summary>
    private static string SafeGet(string[] cols, int index)
    {
        if (cols == null || index < 0 || index >= cols.Length)
            return string.Empty;
        return cols[index].Trim();
    }

    private static float ParseFloat(string s)
    {
        if (TryParseFloat(s, out float v))
            return v;
        return 0f;
    }

    private static int ParseOptionalReward(string[] cols, int index, string columnName, int monsterId)
    {
        if (index < 0)
        {
            return 0;
        }

        string raw = SafeGet(cols, index);
        if (string.IsNullOrWhiteSpace(raw))
        {
            return 0;
        }

        if (TryParseFloat(raw, out float value))
        {
            return Mathf.Max(0, Mathf.RoundToInt(value));
        }

        Debug.LogWarning($"[MonsterTableImporter] Failed to parse {columnName} for id '{monsterId}'. raw='{raw}'. default=0");
        return 0;
    }

    private static bool TryParseFloat(string s, out float v)
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            v = 0f;
            return false;
        }
        if (float.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out v))
            return true;
        if (float.TryParse(s, out v))
            return true;
        v = 0f;
        return false;
    }

    private static GameObject ResolvePrefab(string prefabName, int id)
    {
        if (string.IsNullOrWhiteSpace(prefabName))
        {
            Debug.LogWarning($"[MonsterTableImporter] Missing prefab name for id '{id}' (empty prefab name).");
            return null;
        }

        string prefabPath = $"{PrefabDirectory}/{prefabName}.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
        {
            Debug.LogWarning($"[MonsterTableImporter] Missing prefab for id '{id}'. expected='{prefabPath}'");
        }

        return prefab;
    }
}
#endif
