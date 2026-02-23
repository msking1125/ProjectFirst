#if UNITY_EDITOR
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class MonsterTableImporter
{
    private const string CsvPath = "Assets/Project/Data/monsters.csv";
    private const string AssetPath = "Assets/Project/Data/MonsterTable.asset";
    private const string PrefabDirectory = "Assets/Project/Prefabs/Enemy";
    private static readonly string[] RequiredColumns =
    {
        "id", "grade", "name", "hp", "atk", "def", "critChance", "critMultiplier", "moveSpeed", "element", "prefab"
    };

    [MenuItem("Tools/Game/Import Monster CSV")]
    public static void Import()
    {
        if (!File.Exists(CsvPath))
        {
            Debug.LogError($"Monster CSV not found at: {CsvPath}");
            return;
        }

        string[] lines = File.ReadAllLines(CsvPath);
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
        int defIdx = idx("def");
        int critChanceIdx = idx("critChance");
        int critMultiplierIdx = idx("critMultiplier");
        int moveSpeedIdx = idx("moveSpeed");
        int gradeIdx = idx("grade");
        int elementIdx = idx("element");
        int prefabIdx = idx("prefab");

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i]))
                continue;

            string[] cols = lines[i].Split(',').Select(c => c.Trim()).ToArray();
            string id = ReadCell(cols, idIdx);
            string name = ReadCell(cols, nameIdx);

            // Parse critMultiplier: Remove all non-numeric, non-dot, non-minus chars (e.g., "x2", "2배", "x1.5", "2.5" etc.)
            float critMultiplierValue = 1f;
            string critMultiplierStr = ReadCell(cols, critMultiplierIdx);
            if (!string.IsNullOrEmpty(critMultiplierStr))
            {
                string numericPart = new string(critMultiplierStr.Where(c => char.IsDigit(c) || c == '.' || c == '-').ToArray());
                if (!float.TryParse(numericPart, NumberStyles.Float, CultureInfo.InvariantCulture, out critMultiplierValue) || critMultiplierValue < 1f)
                {
                    critMultiplierValue = 1f;
                }
            }

            string gradeRaw = ReadCell(cols, gradeIdx);
            MonsterGrade rowGrade = MonsterGrade.Normal;
            if (Enum.TryParse(gradeRaw, true, out MonsterGrade grade))
            {
                rowGrade = grade;
            }
            else
            {
                Debug.LogWarning($"[MonsterTableImporter] Failed to parse grade for id '{id}'. raw='{gradeRaw}'. fallback=Normal");
            }

            string prefabName = ReadCell(cols, prefabIdx);
            GameObject prefabAsset = ResolvePrefab(prefabName, id);

            MonsterRow row = new MonsterRow
            {
                id = id,
                grade = rowGrade,
                name = name,
                hp = ParseFloat(ReadCell(cols, hpIdx)),
                atk = ParseFloat(ReadCell(cols, atkIdx)),
                def = ParseFloat(ReadCell(cols, defIdx)),
                critChance = Mathf.Clamp01(ParseFloat(ReadCell(cols, critChanceIdx))),
                critMultiplier = critMultiplierValue,
                prefab = prefabAsset
            };

            string moveSpeedRaw = ReadCell(cols, moveSpeedIdx);
            if (TryParseFloat(moveSpeedRaw, out float moveSpeedValue))
            {
                row.moveSpeed = moveSpeedValue;
            }
            else if (!string.IsNullOrWhiteSpace(moveSpeedRaw))
            {
                Debug.LogWarning($"[MonsterTableImporter] Failed to parse moveSpeed for id '{id}'. raw='{moveSpeedRaw}'");
            }

            string elementRaw = ReadCell(cols, elementIdx);
            if (Enum.TryParse(elementRaw, true, out ElementType element))
            {
                row.element = element;
            }
            else
            {
                Debug.LogWarning($"[MonsterTableImporter] Failed to parse element for id '{id}'. raw='{elementRaw}'. fallback=Reason");
                row.element = ElementType.Reason;
            }

            table.rows.Add(row);

            if (prefabAsset != null)
            {
                Debug.Log($"[MonsterTableImporter] Row imported. id='{id}', grade='{row.grade}', prefabName='{prefabName}', prefabAsset='{prefabAsset.name}'");
            }
            else
            {
                Debug.LogWarning($"[MonsterTableImporter] Row imported with missing prefab. id='{id}', grade='{row.grade}', prefabName='{prefabName}', prefabAsset='null'");
            }
        }

        EditorUtility.SetDirty(table);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Imported {table.rows.Count} monsters into {AssetPath}");
    }

    [MenuItem("Assets/Import CSV/Monster Table", false, 2000)]
    private static void ImportFromAssetsMenu()
    {
        Import();
    }

    private static string ReadCell(string[] cols, int index)
    {
        if (index < 0 || cols == null || index >= cols.Length)
            return string.Empty;
        return cols[index];
    }

    private static float ParseFloat(string s)
    {
        if (TryParseFloat(s, out float v))
            return v;
        return 0f;
    }

    private static bool TryParseFloat(string s, out float v)
    {
        if (float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out v))
            return true;
        if (float.TryParse(s, out v))
            return true;
        v = 0f;
        return false;
    }

    private static GameObject ResolvePrefab(string prefabName, string id)
    {
        if (string.IsNullOrWhiteSpace(prefabName))
        {
            return null;
        }

        string[] guids = AssetDatabase.FindAssets($"{prefabName} t:Prefab");
        string prefabPath = null;

        for (int i = 0; i < guids.Length; i++)
        {
            string candidatePath = AssetDatabase.GUIDToAssetPath(guids[i]);
            if (candidatePath.StartsWith($"{PrefabDirectory}/", StringComparison.OrdinalIgnoreCase))
            {
                prefabPath = candidatePath;
                break;
            }

            if (prefabPath == null)
            {
                prefabPath = candidatePath;
            }
        }

        GameObject prefab = string.IsNullOrWhiteSpace(prefabPath) ? null : AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
        {
            Debug.LogWarning($"[MonsterTableImporter] Prefab not found for {prefabName} (id='{id}')");
        }

        return prefab;
    }
}
#endif
