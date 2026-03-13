#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using ProjectFirst.Data;

/// <summary>
/// CSV를 DialogueTable ScriptableObject로 임포트합니다.
/// 대사 text 컬럼의 이스케이프 줄바꿈(\n)을 실제 줄바꿈으로 복원합니다.
/// CSV가 없으면 샘플 파일을 자동 생성합니다.
/// </summary>
public static class DialogueTableImporter
{
    private const string CsvPathLower = "Assets/Project/Data/dialogues.csv";
    private const string CsvPathUpper = "Assets/Project/Data/Dialogues.csv";
    private const string AssetPath = "Assets/Project/Data/DialogueTable.asset";

    private const string SampleCsv =
        "groupId,dialogueId,orderInGroup,speakerName,text,background,characterL,characterR,videoKey,choiceA,choiceANext,choiceB,choiceBNext,nextId\n" +
        "intro,intro_001,1,Guide,\"환영합니다.\\n첫 전투를 시작해볼까요?\",lobby_bg,guide_portrait,, ,,, ,,,intro_002\n" +
        "intro,intro_002,2,Hero,준비됐어.,lobby_bg,guide_portrait,hero_portrait, ,좋아요,intro_choice_yes,잠깐만요,intro_choice_no,\n" +
        "intro_choice_yes,intro_choice_yes,1,Guide,좋아요. 바로 출발하죠!,battle_bg,guide_portrait,hero_portrait, ,,, ,,,\n" +
        "intro_choice_no,intro_choice_no,1,Guide,천천히 둘러본 뒤 다시 말을 걸어주세요.,lobby_bg,guide_portrait,, ,,, ,,,\n";

    private static readonly string[] RequiredColumns = { "groupId", "dialogueId", "speakerName", "text" };

    public static void Import()
    {
        CsvImportUtility.EnsureCsvExists(CsvPathLower, SampleCsv, nameof(DialogueTableImporter));

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

        DialogueTable table = AssetDatabase.LoadAssetAtPath<DialogueTable>(AssetPath) ?? CreateAsset();

        SerializedObject so = new SerializedObject(table);
        SerializedProperty linesProp = so.FindProperty("_lines") ?? so.FindProperty("lines");
        if (linesProp == null)
        {
            Debug.LogError("[DialogueTableImporter] DialogueTable에 '_lines' 또는 'lines' 필드가 없습니다.");
            return;
        }
        linesProp.ClearArray();

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

        int groupIdIdx = ColIdx("groupId");
        int dialogueIdIdx = ColIdx("dialogueId");
        int orderIdx = ColIdx("orderInGroup");
        int speakerNameIdx = ColIdx("speakerName");
        int textIdx = ColIdx("text");
        int backgroundIdx = ColIdx("background");
        int characterLIdx = ColIdx("characterL");
        int characterRIdx = ColIdx("characterR");
        int videoKeyIdx = ColIdx("videoKey");
        int choiceAIdx = ColIdx("choiceA");
        int choiceANextIdx = ColIdx("choiceANext");
        int choiceBIdx = ColIdx("choiceB");
        int choiceBNextIdx = ColIdx("choiceBNext");
        int nextIdIdx = ColIdx("nextId");

        int imported = 0;
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i];
            if (string.IsNullOrWhiteSpace(line))
                continue;

            string[] cols = CsvImportUtility.ParseRow(line, header.Length);

            string groupId = CsvImportUtility.GetCell(cols, groupIdIdx);
            string dialogueId = CsvImportUtility.GetCell(cols, dialogueIdIdx);
            string speakerName = CsvImportUtility.GetCell(cols, speakerNameIdx);
            string text = CsvImportUtility.GetCell(cols, textIdx);
            if (string.IsNullOrWhiteSpace(groupId) || string.IsNullOrWhiteSpace(dialogueId) || string.IsNullOrWhiteSpace(text))
            {
                Debug.LogWarning($"[DialogueTableImporter] {i + 1}행 필수 값이 비어 있어 건너뜁니다: {line}");
                continue;
            }

            linesProp.InsertArrayElementAtIndex(imported);
            SerializedProperty row = linesProp.GetArrayElementAtIndex(imported);

            row.FindPropertyRelative("groupId").stringValue = groupId;
            row.FindPropertyRelative("dialogueId").stringValue = dialogueId;
            row.FindPropertyRelative("orderInGroup").intValue = CsvImportUtility.ParseInt(CsvImportUtility.GetCell(cols, orderIdx), imported + 1);
            row.FindPropertyRelative("speakerName").stringValue = speakerName;
            row.FindPropertyRelative("text").stringValue = text.Replace("\\n", "\n");
            row.FindPropertyRelative("backgroundKey").stringValue = CsvImportUtility.GetCell(cols, backgroundIdx);
            row.FindPropertyRelative("characterLKey").stringValue = CsvImportUtility.GetCell(cols, characterLIdx);
            row.FindPropertyRelative("characterRKey").stringValue = CsvImportUtility.GetCell(cols, characterRIdx);
            row.FindPropertyRelative("videoKey").stringValue = CsvImportUtility.GetCell(cols, videoKeyIdx);
            row.FindPropertyRelative("choiceAText").stringValue = CsvImportUtility.GetCell(cols, choiceAIdx);
            row.FindPropertyRelative("choiceANext").stringValue = CsvImportUtility.GetCell(cols, choiceANextIdx);
            row.FindPropertyRelative("choiceBText").stringValue = CsvImportUtility.GetCell(cols, choiceBIdx);
            row.FindPropertyRelative("choiceBNext").stringValue = CsvImportUtility.GetCell(cols, choiceBNextIdx);
            row.FindPropertyRelative("nextId").stringValue = CsvImportUtility.GetCell(cols, nextIdIdx);

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
