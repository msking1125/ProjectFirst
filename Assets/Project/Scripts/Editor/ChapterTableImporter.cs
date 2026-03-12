#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using ProjectFirst.Data;
public static class ChapterTableImporter
{
    private const string CsvPathLower = "Assets/Project/Data/chapters.csv";
    private const string CsvPathUpper = "Assets/Project/Data/Chapters.csv";
    private const string AssetPath = "Assets/Project/Data/ChapterTable.asset";

    public static void Import()
    {
        if (!CsvImportUtility.TryResolveCsvPath(out string csvPath, CsvPathLower, CsvPathUpper))
        {
            Debug.LogError($"[ChapterTableImporter] CSV를 찾을 수 없습니다: {CsvPathLower} (or {CsvPathUpper})");
            return;
        }

        if (!CsvImportUtility.TryReadCsvLines(csvPath, out string[] lines))
        {
            Debug.LogError($"[ChapterTableImporter] 데이터 행이 없습니다: {csvPath}");
            return;
        }

        var header = CsvImportUtility.ParseHeader(lines[0]);
        string[] required = { "id", "name", "description", "worldMapX", "worldMapY", "isUnlocked" };
        if (!CsvImportUtility.ValidateRequiredColumns(header, required, "ChapterTableImporter"))
            return;

        int colId = CsvImportUtility.FindColumn(header, "id");
        int colName = CsvImportUtility.FindColumn(header, "name");
        int colDesc = CsvImportUtility.FindColumn(header, "description");
        int colX = CsvImportUtility.FindColumn(header, "worldMapX");
        int colY = CsvImportUtility.FindColumn(header, "worldMapY");
        int colUnlocked = CsvImportUtility.FindColumn(header, "isUnlocked");

        ChapterTable table = CsvImportUtility.LoadOrCreateAsset<ChapterTable>(AssetPath);
        table.rows.Clear();

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            var cols = CsvImportUtility.ParseRow(lines[i], header.Length);

            var row = new ChapterRow
            {
                id = CsvImportUtility.ParseInt(CsvImportUtility.GetCell(cols, colId)),
                name = CsvImportUtility.GetCell(cols, colName),
                description = CsvImportUtility.GetCell(cols, colDesc),
                worldMapX = CsvImportUtility.ParseFloat(CsvImportUtility.GetCell(cols, colX)),
                worldMapY = CsvImportUtility.ParseFloat(CsvImportUtility.GetCell(cols, colY)),
                isUnlocked = CsvImportUtility.ParseInt(CsvImportUtility.GetCell(cols, colUnlocked)) != 0,
            };

            table.rows.Add(row);
        }

        EditorUtility.SetDirty(table);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[ChapterTableImporter] {table.rows.Count}개 챕터 임포트 완료 → {AssetPath}");
    }
}
#endif


