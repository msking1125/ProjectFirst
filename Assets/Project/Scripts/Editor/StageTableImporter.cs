#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using ProjectFirst.Data;
public static class StageTableImporter
{
    private const string CsvPathLower = "Assets/Project/Data/stages.csv";
    private const string CsvPathUpper = "Assets/Project/Data/Stages.csv";
    private const string AssetPath = "Assets/Project/Data/StageTable.asset";

    public static void Import()
    {
        if (!CsvImportUtility.TryResolveCsvPath(out string csvPath, CsvPathLower, CsvPathUpper))
        {
            Debug.LogError($"[StageTableImporter] CSV를 찾을 수 없습니다: {CsvPathLower} (or {CsvPathUpper})");
            return;
        }

        if (!CsvImportUtility.TryReadCsvLines(csvPath, out string[] lines))
        {
            Debug.LogError($"[StageTableImporter] 데이터 행이 없습니다: {csvPath}");
            return;
        }

        var header = CsvImportUtility.ParseHeader(lines[0]);
        string[] required = { "id", "chapterId", "stageNumber", "name", "enemyElement", "staminaCost" };
        if (!CsvImportUtility.ValidateRequiredColumns(header, required, "StageTableImporter"))
            return;

        int colId = CsvImportUtility.FindColumn(header, "id");
        int colChapter = CsvImportUtility.FindColumn(header, "chapterId");
        int colNumber = CsvImportUtility.FindColumn(header, "stageNumber");
        int colName = CsvImportUtility.FindColumn(header, "name");
        int colDesc = CsvImportUtility.FindColumn(header, "description");
        int colPower = CsvImportUtility.FindColumn(header, "recommendedPower");
        int colElement = CsvImportUtility.FindColumn(header, "enemyElement");
        int colStamina = CsvImportUtility.FindColumn(header, "staminaCost");
        int colGold = CsvImportUtility.FindColumn(header, "rewardGold");
        int colExp = CsvImportUtility.FindColumn(header, "rewardExp");
        int colWave = CsvImportUtility.FindColumn(header, "waveDataId");

        StageTable table = CsvImportUtility.LoadOrCreateAsset<StageTable>(AssetPath);
        table.rows.Clear();

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            var cols = CsvImportUtility.ParseRow(lines[i], header.Length);

            var row = new StageRow
            {
                id = CsvImportUtility.ParseInt(CsvImportUtility.GetCell(cols, colId)),
                chapterId = CsvImportUtility.ParseInt(CsvImportUtility.GetCell(cols, colChapter)),
                stageNumber = CsvImportUtility.ParseInt(CsvImportUtility.GetCell(cols, colNumber)),
                name = CsvImportUtility.GetCell(cols, colName),
                description = CsvImportUtility.GetCell(cols, colDesc),
                recommendedPower = CsvImportUtility.ParseInt(CsvImportUtility.GetCell(cols, colPower)),
                staminaCost = CsvImportUtility.ParseInt(CsvImportUtility.GetCell(cols, colStamina)),
                rewardGold = CsvImportUtility.ParseInt(CsvImportUtility.GetCell(cols, colGold)),
                rewardExp = CsvImportUtility.ParseInt(CsvImportUtility.GetCell(cols, colExp)),
                waveDataId = CsvImportUtility.GetCell(cols, colWave),
            };

            string elementRaw = CsvImportUtility.GetCell(cols, colElement);
            if (CsvImportUtility.TryParseEnumInsensitive<ElementType>(elementRaw, out var elem))
                row.enemyElement = elem;

            table.rows.Add(row);
        }

        EditorUtility.SetDirty(table);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[StageTableImporter] {table.rows.Count}개 스테이지 임포트 완료 → {AssetPath}");
    }
}
#endif


