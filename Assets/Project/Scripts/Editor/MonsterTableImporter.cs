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

    [MenuItem("Tools/Game/Import Monster CSV")]
    public static void Import()
    {
        // Allow for either 'monsters.csv' or 'Monsters.csv' for case-insensitivity
        string csvPath = File.Exists(CsvPathLower) ? CsvPathLower : (File.Exists(CsvPathUpper) ? CsvPathUpper : null);
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
        int idx(string name) => Array.FindIndex(header, h => string.Equals(h, name, StringComparison.OrdinalIgnoreCase));

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

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i];
            if (string.IsNullOrWhiteSpace(line))
                continue;

            string[] cols = line.Split(',').Select(c => c.Trim()).ToArray();
            // Ensure columns are at least as long as header (when trailing empty columns present)
            if (cols.Length < header.Length)
            {
                Array.Resize(ref cols, header.Length);
            }

            string id = SafeGet(cols, idIdx);
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
                prefab = ResolvePrefab(SafeGet(cols, prefabIdx), id)
            };

            string gradeRaw = SafeGet(cols, gradeIdx);
            if (TryParseEnumInsensitive(gradeRaw, out MonsterGrade grade))
            {
                row.grade = grade;
            }
            else
            {
                Debug.LogWarning($"[MonsterTableImporter] Failed to parse grade for id '{id ?? "(null)"}'. raw='{gradeRaw ?? "(null)"}'. fallback=Normal");
                row.grade = MonsterGrade.Normal;
            }

            string moveSpeedRaw = SafeGet(cols, moveSpeedIdx);
            if (TryParseFloat(moveSpeedRaw, out float moveSpeedValue))
            {
                row.moveSpeed = moveSpeedValue;
            }
            else if (!string.IsNullOrWhiteSpace(moveSpeedRaw))
            {
                Debug.LogWarning($"[MonsterTableImporter] Failed to parse moveSpeed for id '{id ?? "(null)"}'. raw='{moveSpeedRaw}'");
            }

            string elementRaw = SafeGet(cols, elementIdx);
            if (TryParseEnumInsensitive(elementRaw, out ElementType element))
            {
                row.element = element;
            }
            else
            {
                Debug.LogWarning($"[MonsterTableImporter] Failed to parse element for id '{id ?? "(null)"}'. raw='{elementRaw ?? "(null)"}'. fallback=Reason");
                row.element = ElementType.Reason;
            }

            table.rows.Add(row);
        }

        EditorUtility.SetDirty(table);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Imported {table.rows.Count} monster{(table.rows.Count == 1 ? "" : "s")} into {AssetPath}");
    }

    [MenuItem("Assets/Import CSV/Monster Table", false, 2000)]
    private static void ImportFromAssetsMenu()
    {
        Import();
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

    private static bool TryParseEnumInsensitive<TEnum>(string raw, out TEnum value) where TEnum : struct, Enum
    {
        string normalized = string.IsNullOrWhiteSpace(raw) ? string.Empty : raw.Trim();
        if (Enum.TryParse(normalized, true, out value))
        {
            return true;
        }

        string compact = normalized.Replace(" ", string.Empty).Replace("_", string.Empty);
        if (!string.IsNullOrEmpty(compact))
        {
            string[] names = Enum.GetNames(typeof(TEnum));
            for (int i = 0; i < names.Length; i++)
            {
                string candidate = names[i].Replace("_", string.Empty);
                if (string.Equals(candidate, compact, StringComparison.OrdinalIgnoreCase))
                {
                    value = (TEnum)Enum.Parse(typeof(TEnum), names[i]);
                    return true;
                }
            }
        }

        value = default;
        return false;
    }

    private static GameObject ResolvePrefab(string prefabName, string id)
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
