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
    private const string CsvPath   = "Assets/Project/Data/Dialogues.csv";
    private const string AssetPath = "Assets/Project/Data/DialogueTable.asset";

    private static readonly string[] RequiredColumns = { "groupId", "dialogueId", "speakerName", "text" };

    [MenuItem("Tools/Game/Import Dialogue CSV")]
    public static void Import()
    {
        if (!File.Exists(CsvPath))
        {
            Debug.LogError($"[DialogueTableImporter] CSV를 찾을 수 없습니다: {CsvPath}");
            return;
        }

        string[] lines = File.ReadAllLines(CsvPath);
        if (lines.Length < 2)
        {
            Debug.LogError("[DialogueTableImporter] 데이터 행이 없습니다.");
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

        string[] header = lines[0].Split(',').Select(x => x.Trim()).ToArray();
        int ColIdx(string n) => Array.FindIndex(header, h => h.Equals(n, StringComparison.OrdinalIgnoreCase));

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
            string[] cols = line.Split(',').Select(x => x.Trim()).ToArray();
            
            // 만약 컬럼 수가 모자라다면 건너뛰거나 공백 대입
            if (cols.Length <= groupIdIdx || cols.Length <= dialogueIdIdx || cols.Length <= speakerNameIdx || cols.Length <= textIdx)
            {
                Debug.LogWarning($"[DialogueTableImporter] 라인 {i + 1}의 데이터가 너무 짧아 건너뜁니다: {line}");
                continue;
            }

            rowsProp.InsertArrayElementAtIndex(imported);
            SerializedProperty row = rowsProp.GetArrayElementAtIndex(imported);

            row.FindPropertyRelative("groupId").stringValue     = GetCell(cols, groupIdIdx);
            row.FindPropertyRelative("dialogueId").stringValue  = GetCell(cols, dialogueIdIdx);
            row.FindPropertyRelative("speakerName").stringValue = GetCell(cols, speakerNameIdx);
            row.FindPropertyRelative("text").stringValue        = GetCell(cols, textIdx).Replace("\\n", "\n"); // 개행문자 지원

            if (backgroundIdx >= 0) row.FindPropertyRelative("background").stringValue = GetCell(cols, backgroundIdx);
            if (characterLIdx >= 0) row.FindPropertyRelative("characterL").stringValue = GetCell(cols, characterLIdx);
            if (characterRIdx >= 0) row.FindPropertyRelative("characterR").stringValue = GetCell(cols, characterRIdx);
            if (choiceAIdx >= 0)    row.FindPropertyRelative("choiceA").stringValue = GetCell(cols, choiceAIdx);
            if (choiceBIdx >= 0)    row.FindPropertyRelative("choiceB").stringValue = GetCell(cols, choiceBIdx);

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

    private static string GetCell(string[] arr, int idx)
        => arr == null || idx < 0 || idx >= arr.Length ? string.Empty : arr[idx];
}
#endif
