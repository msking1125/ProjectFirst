#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using ProjectFirst.Data;
/// <summary>
/// CSV를 DialogueTable ScriptableObject로 임포트합니다.
/// 대사 text 컬럼의 이스케이프 줄바꿈(\\n)을 실제 줄바꿈으로 복원합니다.
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
            Debug.LogError($"[DialogueTableImporter] CSV를 찾을 수 없습니다: {CsvPathLower} (or {CsvPathUpper})");
            return;
        }

        if (!CsvImportUtility.TryReadCsvLines(csvPath, out string[] lines))
        {
            Debug.LogError($"[DialogueTableImporter] 데이터 행이 없습니다: {csvPath}");
            return;
        }

        DialogueTable table = AssetDatabase.LoadAssetAtPath<DialogueTable>(AssetPath)
                           ?? CreateAsset();

        SerializedObject so       = new SerializedObject(table);
        SerializedProperty rowsProp = so.FindProperty("rows");
        if (rowsProp == null)
        {
            Debug.LogError("[DialogueTableImporter] DialogueTable에 'rows' 필드가 없습니다.");
            return;
        }
        rowsProp.ClearArray();

        string[] header = CsvImportUtility.ParseHeader(lines[0]);
        int ColIdx(string n) => CsvImportUtility.FindColumn(header, n);

        foreach (string col in RequiredColumns)
        {
            if (ColIdx(col) < 0)
            {
                Debug.LogError($"[DialogueTableImporter] 필수 컬럼 '{col}'이 없습니다.");
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

            // ParseRow는 따옴표와 이스케이프 구분자를 포함한 행을 처리합니다.
            // 포맷이 더 복잡해지면 전용 CSV 파서를 검토합니다.
            string[] cols = CsvImportUtility.ParseRow(line, header.Length);
            
            // 필수 컬럼 수보다 짧은 비정상 행은 건너뜁니다.
            if (cols.Length <= groupIdIdx || cols.Length <= dialogueIdIdx || cols.Length <= speakerNameIdx || cols.Length <= textIdx)
            {
                Debug.LogWarning($"[DialogueTableImporter] {i + 1}행 데이터가 짧아 건너뜁니다: {line}");
                continue;
            }

            rowsProp.InsertArrayElementAtIndex(imported);
            SerializedProperty row = rowsProp.GetArrayElementAtIndex(imported);

            row.FindPropertyRelative("groupId").stringValue     = CsvImportUtility.GetCell(cols, groupIdIdx);
            row.FindPropertyRelative("dialogueId").stringValue  = CsvImportUtility.GetCell(cols, dialogueIdIdx);
            row.FindPropertyRelative("speakerName").stringValue = CsvImportUtility.GetCell(cols, speakerNameIdx);
            row.FindPropertyRelative("text").stringValue        = CsvImportUtility.GetCell(cols, textIdx).Replace("\\n", "\n"); // 이스케이프 줄바꿈 복원

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

        Debug.Log($"[DialogueTableImporter] {imported}개 대사 임포트 완료 → {AssetPath}");
    }

    private static DialogueTable CreateAsset()
    {
        DialogueTable t = ScriptableObject.CreateInstance<DialogueTable>();
        AssetDatabase.CreateAsset(t, AssetPath);
        return t;
    }

}
#endif



