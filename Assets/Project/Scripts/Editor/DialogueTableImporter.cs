#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using ProjectFirst.Data;
/// <summary>
/// CSV ??DialogueTable ScriptableObject ?꾪룷??
/// 二쇱냼 諛⑹떇 濡쒕뵫??吏?먰븯?꾨줉 ?ㅺ퀎??
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
            Debug.LogError($"[DialogueTableImporter] CSV瑜?李얠쓣 ???놁뒿?덈떎: {CsvPathLower} (or {CsvPathUpper})");
            return;
        }

        if (!CsvImportUtility.TryReadCsvLines(csvPath, out string[] lines))
        {
            Debug.LogError($"[DialogueTableImporter] ?곗씠???됱씠 ?놁뒿?덈떎: {csvPath}");
            return;
        }

        DialogueTable table = AssetDatabase.LoadAssetAtPath<DialogueTable>(AssetPath)
                           ?? CreateAsset();

        SerializedObject so       = new SerializedObject(table);
        SerializedProperty rowsProp = so.FindProperty("rows");
        if (rowsProp == null)
        {
            Debug.LogError("[DialogueTableImporter] DialogueTable??'rows' ?꾨뱶媛 ?놁뒿?덈떎.");
            return;
        }
        rowsProp.ClearArray();

        string[] header = CsvImportUtility.ParseHeader(lines[0]);
        int ColIdx(string n) => CsvImportUtility.FindColumn(header, n);

        foreach (string col in RequiredColumns)
        {
            if (ColIdx(col) < 0)
            {
                Debug.LogError($"[DialogueTableImporter] ?꾩닔 而щ읆 '{col}'???놁뒿?덈떎.");
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

            // ?쇳몴 ?덉쓽 ?띿뒪?멸? ?덉쓣 ???덉?留? 媛꾨떒??援ъ“瑜??꾪빐 Split ?ъ슜.
            // ?꾩슂???곕씪 CsvHelper???뺢퇋?앹쓣 ?ъ슜?섏뿬 ?⑥쟾???뚯떛???섎뒗 寃껋씠 醫뗭쓬.
            string[] cols = CsvImportUtility.ParseRow(line, header.Length);
            
            // 留뚯빟 而щ읆 ?섍? 紐⑥옄?쇰떎硫?嫄대꼫?곌굅??怨듬갚 ???
            if (cols.Length <= groupIdIdx || cols.Length <= dialogueIdIdx || cols.Length <= speakerNameIdx || cols.Length <= textIdx)
            {
                Debug.LogWarning($"[DialogueTableImporter] ?쇱씤 {i + 1}???곗씠?곌? ?덈Т 吏㏃븘 嫄대꼫?곷땲?? {line}");
                continue;
            }

            rowsProp.InsertArrayElementAtIndex(imported);
            SerializedProperty row = rowsProp.GetArrayElementAtIndex(imported);

            row.FindPropertyRelative("groupId").stringValue     = CsvImportUtility.GetCell(cols, groupIdIdx);
            row.FindPropertyRelative("dialogueId").stringValue  = CsvImportUtility.GetCell(cols, dialogueIdIdx);
            row.FindPropertyRelative("speakerName").stringValue = CsvImportUtility.GetCell(cols, speakerNameIdx);
            row.FindPropertyRelative("text").stringValue        = CsvImportUtility.GetCell(cols, textIdx).Replace("\\n", "\n"); // 媛쒗뻾臾몄옄 吏??

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

        Debug.Log($"[DialogueTableImporter] {imported}媛???????꾪룷???꾨즺 ??{AssetPath}");
    }

    private static DialogueTable CreateAsset()
    {
        DialogueTable t = ScriptableObject.CreateInstance<DialogueTable>();
        AssetDatabase.CreateAsset(t, AssetPath);
        return t;
    }

}
#endif


