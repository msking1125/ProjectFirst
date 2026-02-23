#if UNITY_EDITOR
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class SkillTableImporter
{
    private const string CsvPath = "Assets/Project/Data/skills.csv";
    private const string AssetPath = "Assets/Project/Data/SkillTable.asset";
    private static readonly string[] RequiredColumns =
    {
        "id", "name", "element", "coefficient", "cooldown", "range"
    };

    [MenuItem("Tools/Game/Import Skill CSV")]
    public static void Import()
    {
        if (!File.Exists(CsvPath))
        {
            Debug.LogError($"Skill CSV not found at: {CsvPath}");
            return;
        }

        string[] lines = File.ReadAllLines(CsvPath);
        if (lines.Length < 2)
        {
            Debug.LogError("Skill CSV has no data rows.");
            return;
        }

        SkillTable table = AssetDatabase.LoadAssetAtPath<SkillTable>(AssetPath);
        if (table == null)
        {
            table = ScriptableObject.CreateInstance<SkillTable>();
            AssetDatabase.CreateAsset(table, AssetPath);
        }

        SerializedObject so = new SerializedObject(table);
        SerializedProperty rowsProp = so.FindProperty("rows");
        rowsProp.ClearArray();

        string[] header = lines[0].Split(',').Select(h => h.Trim()).ToArray();
        int idx(string name) => Array.FindIndex(header, h => string.Equals(h, name, StringComparison.OrdinalIgnoreCase));

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
        int elementIdx = idx("element");
        int coefficientIdx = idx("coefficient");
        int cooldownIdx = idx("cooldown");
        int rangeIdx = idx("range");

        int rowIndex = 0;
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i]))
            {
                continue;
            }

            string[] cols = lines[i].Split(',').Select(c => c.Trim()).ToArray();
            rowsProp.InsertArrayElementAtIndex(rowIndex);
            SerializedProperty row = rowsProp.GetArrayElementAtIndex(rowIndex);

            row.FindPropertyRelative("id").stringValue = ReadCell(cols, idIdx);
            row.FindPropertyRelative("name").stringValue = ReadCell(cols, nameIdx);
            row.FindPropertyRelative("coefficient").floatValue = Mathf.Max(0.1f, ParseFloat(ReadCell(cols, coefficientIdx), 1f));
            row.FindPropertyRelative("cooldown").floatValue = Mathf.Max(0f, ParseFloat(ReadCell(cols, cooldownIdx), 0f));
            row.FindPropertyRelative("range").floatValue = Mathf.Max(0f, ParseFloat(ReadCell(cols, rangeIdx), 9999f));

            string elementRaw = ReadCell(cols, elementIdx);
            if (!Enum.TryParse(elementRaw, true, out ElementType element))
            {
                element = ElementType.Reason;
            }

            row.FindPropertyRelative("element").enumValueIndex = (int)element;
            rowIndex++;
        }

        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(table);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Imported {rowIndex} skills into {AssetPath}");
    }

    private static string ReadCell(string[] cols, int index)
    {
        if (cols == null || index < 0 || index >= cols.Length)
        {
            return string.Empty;
        }

        return cols[index];
    }

    private static float ParseFloat(string s, float defaultValue)
    {
        if (float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
        {
            return value;
        }

        if (float.TryParse(s, out value))
        {
            return value;
        }

        return defaultValue;
    }
}
#endif
