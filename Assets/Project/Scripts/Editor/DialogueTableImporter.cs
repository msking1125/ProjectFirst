#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// CSV → DialogueTable ScriptableObject 임포터.
/// 주소 방식 로딩을 지원하도록 설계됨.
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

            // 쉼표 안의 텍스트가 있을 수 있지만, 간단한 구조를 위해 Split 사용.
            // 필요에 따라 CsvHelper나 정규식을 사용하여 온전한 파싱을 하는 것이 좋음.
            string[] cols = CsvImportUtility.ParseRow(line, header.Length);
            
            // 만약 컬럼 수가 모자라다면 건너뛰거나 공백 대입
            if (cols.Length <= groupIdIdx || cols.Length <= dialogueIdIdx || cols.Length <= speakerNameIdx || cols.Length <= textIdx)
            {
                Debug.LogWarning($"[DialogueTableImporter] 라인 {i + 1}의 데이터가 너무 짧아 건너뜁니다: {line}");
                continue;
            }

            rowsProp.InsertArrayElementAtIndex(imported);
            SerializedProperty row = rowsProp.GetArrayElementAtIndex(imported);

            row.FindPropertyRelative("groupId").stringValue     = CsvImportUtility.GetCell(cols, groupIdIdx);
            row.FindPropertyRelative("dialogueId").stringValue  = CsvImportUtility.GetCell(cols, dialogueIdIdx);
            row.FindPropertyRelative("speakerName").stringValue = CsvImportUtility.GetCell(cols, speakerNameIdx);
            row.FindPropertyRelative("text").stringValue        = CsvImportUtility.GetCell(cols, textIdx).Replace("\\n", "\n"); // 개행문자 지원

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

        Debug.Log($"[DialogueTableImporter] {imported}개 대화 행 임포트 완료 → {AssetPath}");
    }

    private static DialogueTable CreateAsset()
    {
        DialogueTable t = ScriptableObject.CreateInstance<DialogueTable>();
        AssetDatabase.CreateAsset(t, AssetPath);
        return t;
    }

}
#endif
