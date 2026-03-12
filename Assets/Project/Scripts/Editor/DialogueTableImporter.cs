#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using ProjectFirst.Data;
/// <summary>
/// Imports a CSV file into DialogueTable.
/// Supports escaped newlines in the dialogue text.
/// </summary>
public static class DialogueTableImporter
{
    private const string CsvPathLower = "Assets/Project/Data/dialogues.csv";
    private const string CsvPathUpper = "Assets/Project/Data/Dialogues.csv";
    private const string AssetPath = "Assets/Project/Data/DialogueTable.asset";

    private static readonly string[] RequiredColumns = { "groupId", "dialogueId", "speakerName", "text" };

    public static void Import()
    {
        if (!CsvImportUtility.TryResolveCsvPath(out string csvPath, CsvPathLower, CsvPathUpper))
        {
            Debug.LogError($"[DialogueTableImporter] Could not find the CSV file: {CsvPathLower} (or {CsvPathUpper})");
            return;
        }

        if (!CsvImportUtility.TryReadCsvLines(csvPath, out string[] lines))
        {
            Debug.LogError($"[DialogueTableImporter] CSV data is empty: {csvPath}");
            return;
        }

        DialogueTable table = AssetDatabase.LoadAssetAtPath<DialogueTable>(AssetPath)
                           ?? CreateAsset();

        SerializedObject so       = new SerializedObject(table);
        SerializedProperty rowsProp = so.FindProperty("rows");
        if (rowsProp == null)
        {
            Debug.LogError("[DialogueTableImporter] DialogueTable does not contain a 'rows' field.");
            return;
        }
        rowsProp.ClearArray();

        string[] header = CsvImportUtility.ParseHeader(lines[0]);
        int ColIdx(string n) => CsvImportUtility.FindColumn(header, n);

        foreach (string col in RequiredColumns)
        {
            if (ColIdx(col) < 0)
            {
                Debug.LogError($"[DialogueTableImporter] Required column '{col}' is missing.");
                return;
            }
        }

        int groupIdIdx     = ColIdx("groupId");
        int dialogueIdIdx  = ColIdx("dialogueId");
        int speakerNameIdx = ColIdx("speakerName");
        int textIdx        = ColIdx("text");
        int backgroundIdx  = ColIdx("background");
        int characterLIdx  = ColIdx("characterL");
        int characterRIdx  = ColIdx("characterR");
        int choiceAIdx     = ColIdx("choiceA");
        int choiceBIdx     = ColIdx("choiceB");

        int imported = 0;
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i];
            if (string.IsNullOrWhiteSpace(line)) continue;

            // ParseRow handles quoted values and escaped separators.
            // If the format grows further, consider a dedicated CSV parser.
            string[] cols = CsvImportUtility.ParseRow(line, header.Length);
            
            // Skip malformed rows that do not contain the required columns.
            if (cols.Length <= groupIdIdx || cols.Length <= dialogueIdIdx || cols.Length <= speakerNameIdx || cols.Length <= textIdx)
            {
                Debug.LogWarning($"[DialogueTableImporter] Skipping short row at line {i + 1}: {line}");
                continue;
            }

            rowsProp.InsertArrayElementAtIndex(imported);
            SerializedProperty row = rowsProp.GetArrayElementAtIndex(imported);

            row.FindPropertyRelative("groupId").stringValue     = CsvImportUtility.GetCell(cols, groupIdIdx);
            row.FindPropertyRelative("dialogueId").stringValue  = CsvImportUtility.GetCell(cols, dialogueIdIdx);
            row.FindPropertyRelative("speakerName").stringValue = CsvImportUtility.GetCell(cols, speakerNameIdx);
            row.FindPropertyRelative("text").stringValue        = CsvImportUtility.GetCell(cols, textIdx).Replace("\\n", "\n"); // Restore escaped newlines.

            if (backgroundIdx >= 0) row.FindPropertyRelative("background").stringValue = CsvImportUtility.GetCell(cols, backgroundIdx);
            if (characterLIdx >= 0) row.FindPropertyRelative("characterL").stringValue = CsvImportUtility.GetCell(cols, characterLIdx);
            if (characterRIdx >= 0) row.FindPropertyRelative("characterR").stringValue = CsvImportUtility.GetCell(cols, characterRIdx);
            if (choiceAIdx >= 0)    row.FindPropertyRelative("choiceA").stringValue = CsvImportUtility.GetCell(cols, choiceAIdx);
            if (choiceBIdx >= 0)    row.FindPropertyRelative("choiceB").stringValue = CsvImportUtility.GetCell(cols, choiceBIdx);

            imported++;
        }

        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(table);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[DialogueTableImporter] Imported {imported} dialogue rows into {AssetPath}");
    }

    private static DialogueTable CreateAsset()
    {
        DialogueTable t = ScriptableObject.CreateInstance<DialogueTable>();
        AssetDatabase.CreateAsset(t, AssetPath);
        return t;
    }

}
#endif


