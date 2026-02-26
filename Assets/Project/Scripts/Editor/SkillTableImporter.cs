#if UNITY_EDITOR
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// SkillTableImporter:
/// CSV 파일에서 SkillTable ScriptableObject를 매우 쉽게 생성/갱신하는 에디터 유틸리티입니다.
/// </summary>
public static class SkillTableImporter
{
    // CSV/에셋 파일 경로 정의
    private const string CsvPath = "Assets/Project/Data/skills.csv";
    private const string AssetPath = "Assets/Project/Data/SkillTable.asset";

    // Skill 필수 컬럼 정의 (누락 시 import 금지)
    private static readonly string[] RequiredColumns = { "id", "name", "element", "coefficient", "range" };

    /// <summary>
    /// [Tools/Game/Import Skill CSV] 메뉴에서 실행
    /// CSV를 SkillTable.asset으로 불러옵니다.
    /// </summary>
    [MenuItem("Tools/Game/Import Skill CSV")]
    public static void Import()
    {
        // 1. 파일 체크
        if (!File.Exists(CsvPath))
        {
            Debug.LogError($"Skill CSV 파일이 존재하지 않습니다: {CsvPath}\n* 경로 및 파일명을 확인하세요.");
            return;
        }

        // 2. CSV 라인 파싱 (헤더+데이터)
        var lines = File.ReadAllLines(CsvPath);
        if (lines.Length < 2)
        {
            Debug.LogError("[SkillTableImporter] Skill CSV에 데이터 행이 없습니다.");
            return;
        }

        // 3. SkillTable 에셋 로드/생성
        SkillTable table = AssetDatabase.LoadAssetAtPath<SkillTable>(AssetPath);
        if (table == null)
        {
            table = ScriptableObject.CreateInstance<SkillTable>();
            AssetDatabase.CreateAsset(table, AssetPath);
        }

        // 4. 직렬화 rows 필드 초기화
        SerializedObject so = new SerializedObject(table);
        SerializedProperty rowsProp = so.FindProperty("rows");
        if (rowsProp == null)
        {
            Debug.LogError("SkillTable에 'rows' 리스트 필드가 존재하지 않습니다. 필드를 확인하세요.");
            return;
        }
        rowsProp.ClearArray();

        // 5. 헤더 추출 및 컬럼 검사
        var header = lines[0].Split(',').Select(x => x.Trim()).ToArray();
        int ColIdx(string n) => Array.FindIndex(header, h => string.Equals(h, n, StringComparison.OrdinalIgnoreCase));

        foreach (var col in RequiredColumns)
        {
            if (ColIdx(col) < 0)
            {
                Debug.LogError($"[SkillTableImporter] CSV에 '{col}' 컬럼이 없습니다. 헤더를 확인하세요.");
                return;
            }
        }

        // 컬럼 인덱스 캐싱 (cooldown은 옵션)
        int idIdx = ColIdx("id"),
            nameIdx = ColIdx("name"),
            elementIdx = ColIdx("element"),
            coefficientIdx = ColIdx("coefficient"),
            rangeIdx = ColIdx("range"),
            cooldownIdx = ColIdx("cooldown"); // -1이면 없는 컬럼

        // 6. 데이터 행 파싱 & ScriptableObject 반영
        int importedCount = 0;
        for (int i = 1; i < lines.Length; i++)
        {
            var line = lines[i];
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var cols = line.Split(',').Select(x => x.Trim()).ToArray();
            if (cols.Length < header.Length)
            {
                Debug.LogWarning($"[{i+1}행] 컬럼 수 부족(필요: {header.Length}, 실제: {cols.Length}) → 스킵: {line}");
                continue;
            }

            rowsProp.InsertArrayElementAtIndex(importedCount);
            var row = rowsProp.GetArrayElementAtIndex(importedCount);

            row.FindPropertyRelative("id").stringValue = GetCell(cols, idIdx);
            row.FindPropertyRelative("name").stringValue = GetCell(cols, nameIdx);
            row.FindPropertyRelative("coefficient").floatValue = Mathf.Max(0.1f, StrToFloat(GetCell(cols, coefficientIdx), 1f));
            row.FindPropertyRelative("range").floatValue = Mathf.Max(0f, StrToFloat(GetCell(cols, rangeIdx), 9999f));
            if (cooldownIdx >= 0)
                row.FindPropertyRelative("cooldown").floatValue = Mathf.Max(0f, StrToFloat(GetCell(cols, cooldownIdx), 0f));

            string elemRaw = GetCell(cols, elementIdx);
            if (!Enum.TryParse(elemRaw, true, out ElementType element))
            {
                Debug.LogWarning($"[SkillTableImporter] '{elemRaw}'는 ElementType으로 변환할 수 없습니다(id: {GetCell(cols, idIdx)}). 기본값 Reason으로 대체.");
                element = ElementType.Reason;
            }
            row.FindPropertyRelative("element").enumValueIndex = (int)element;

            importedCount++;
        }

        // 7. 에셋/변경사항 저장
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(table);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[SkillTableImporter] Skill 데이터 {importedCount}개를 성공적으로 임포트했습니다 → {AssetPath}");
    }

    /// <summary>
    /// (안전하게) 해당 인덱스 셀 추출. 인덱스 범위 밖이면 빈 문자열 반환.
    /// </summary>
    private static string GetCell(string[] arr, int idx)
        => arr == null || idx < 0 || idx >= arr.Length ? string.Empty : arr[idx];

    /// <summary>
    /// 문자열 → float 변환. 실패 시 기본값 반환.
    /// </summary>
    private static float StrToFloat(string s, float fallback)
    {
        if (string.IsNullOrWhiteSpace(s))
            return fallback;
        if (float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out float v))
            return v;
        if (float.TryParse(s, out v))
            return v;
        return fallback;
    }
}
#endif
