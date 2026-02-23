#if UNITY_EDITOR
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// SkillTableImporter는 CSV 파일로부터 SkillTable ScriptableObject를 생성 또는 갱신하는 유틸리티입니다.
/// </summary>
public static class SkillTableImporter
{
    // CSV 및 스킬 데이터 에셋 경로 상수 정의
    private const string CsvPath = "Assets/Project/Data/skills.csv";
    private const string AssetPath = "Assets/Project/Data/SkillTable.asset";

    // 필수 컬럼 명시 (헤더 일치 검사에 사용)
    private static readonly string[] RequiredColumns =
    {
        "id", "name", "element", "coefficient", "cooldown", "range"
    };

    /// <summary>
    /// 메뉴에서 실행: CSV를 SkillTable.asset로 임포트합니다.
    /// </summary>
    [MenuItem("Tools/Game/Import Skill CSV")]
    public static void Import()
    {
        // 1. 파일 존재 검사
        if (!File.Exists(CsvPath))
        {
            Debug.LogError($"Skill CSV 파일을 찾을 수 없습니다: {CsvPath}");
            return;
        }

        // 2. CSV 전체 라인 읽기 (헤더 포함)
        string[] lines = File.ReadAllLines(CsvPath);
        if (lines.Length < 2)
        {
            Debug.LogError("Skill CSV에 데이터 행이 없습니다.");
            return;
        }

        // 3. SkillTable 에셋 로드(없으면 새로 생성)
        SkillTable table = AssetDatabase.LoadAssetAtPath<SkillTable>(AssetPath);
        if (table == null)
        {
            table = ScriptableObject.CreateInstance<SkillTable>();
            AssetDatabase.CreateAsset(table, AssetPath);
        }

        // 4. SkillTable rows 필드 초기화
        SerializedObject so = new SerializedObject(table);
        SerializedProperty rowsProp = so.FindProperty("rows");
        if (rowsProp == null)
        {
            Debug.LogError("SkillTable에 'rows' 필드를 찾을 수 없습니다.");
            return;
        }
        rowsProp.ClearArray();

        // 5. 헤더 파싱 및 컬럼 인덱스 확인
        string[] header = lines[0].Split(',').Select(h => h.Trim()).ToArray();
        int GetColumnIndex(string name) => Array.FindIndex(header, h => string.Equals(h, name, StringComparison.OrdinalIgnoreCase));

        foreach (string col in RequiredColumns)
        {
            if (GetColumnIndex(col) < 0)
            {
                Debug.LogError($"필수 컬럼이 누락되었습니다: {col}");
                return;
            }
        }

        // 각 컬럼 인덱스 저장
        int idIdx = GetColumnIndex("id");
        int nameIdx = GetColumnIndex("name");
        int elementIdx = GetColumnIndex("element");
        int coefficientIdx = GetColumnIndex("coefficient");
        int cooldownIdx = GetColumnIndex("cooldown");
        int rangeIdx = GetColumnIndex("range");

        // 6. 실제 데이터 행 파싱 및 에셋에 저장
        int rowIndex = 0;
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i];
            if (string.IsNullOrWhiteSpace(line))
                continue;

            string[] cols = line.Split(',').Select(c => c.Trim()).ToArray();
            if (cols.Length < header.Length)
            {
                Debug.LogWarning($"{i + 1}번째 라인에 컬럼 개수가 부족합니다. 건너뜁니다: '{line}'");
                continue;
            }
            rowsProp.InsertArrayElementAtIndex(rowIndex);
            SerializedProperty row = rowsProp.GetArrayElementAtIndex(rowIndex);

            row.FindPropertyRelative("id").stringValue = ReadCell(cols, idIdx);
            row.FindPropertyRelative("name").stringValue = ReadCell(cols, nameIdx);
            row.FindPropertyRelative("coefficient").floatValue = Mathf.Max(0.1f, ParseFloat(ReadCell(cols, coefficientIdx), 1f));
            row.FindPropertyRelative("cooldown").floatValue = Mathf.Max(0f, ParseFloat(ReadCell(cols, cooldownIdx), 0f));
            row.FindPropertyRelative("range").floatValue = Mathf.Max(0f, ParseFloat(ReadCell(cols, rangeIdx), 9999f));

            string elementRaw = ReadCell(cols, elementIdx);
            // Enum TryParse 오류 시 기본값 Reason 사용
            if (!Enum.TryParse(elementRaw, true, out ElementType element))
            {
                Debug.LogWarning($"'{elementRaw}'은(는) 올바른 ElementType이 아닙니다. 기본값 Reason이 할당됩니다.");
                element = ElementType.Reason;
            }
            row.FindPropertyRelative("element").enumValueIndex = (int)element;

            rowIndex++;
        }

        // 7. 변경 적용 및 에셋 저장
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(table);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"총 {rowIndex}개의 스킬 데이터를 성공적으로 임포트했습니다: {AssetPath}");
    }

    /// <summary>
    /// 해당 인덱스의 셀 값을 안전하게 반환합니다.
    /// </summary>
    private static string ReadCell(string[] cols, int index)
    {
        if (cols == null || index < 0 || index >= cols.Length)
            return string.Empty;
        return cols[index];
    }

    /// <summary>
    /// 문자열을 float으로 파싱. 실패 시 기본값 반환.
    /// </summary>
    private static float ParseFloat(string s, float defaultValue)
    {
        if (string.IsNullOrWhiteSpace(s))
            return defaultValue;

        if (float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
            return value;
        if (float.TryParse(s, out value))
            return value;

        return defaultValue;
    }
}
#endif
