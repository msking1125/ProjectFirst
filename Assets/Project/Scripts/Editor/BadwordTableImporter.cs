#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using ProjectFirst.Data;

public static class BadwordTableImporter
{
    private const string CsvPathProject = "Assets/Project/Data/badwords.csv";
    private const string CsvPathProjectUpper = "Assets/Project/Data/Badwords.csv";
    private const string LegacyCsvPathResources = "Assets/Resources/Data/badwords.csv";
    private const string LegacyCsvPathResourcesUpper = "Assets/Resources/Data/Badwords.csv";
    private const string AssetPath = "Assets/Project/Data/BadwordTable.asset";

    private const string SampleCsv =
        "word\n" +
        "spam\n" +
        "abuse\n" +
        "testbadword\n";

    public static void Import()
    {
        TryMoveLegacyCsvToProjectData();
        CsvImportUtility.EnsureCsvExists(CsvPathProject, SampleCsv, nameof(BadwordTableImporter));

        if (!CsvImportUtility.TryResolveCsvPath(out string csvPath,
                CsvPathProject,
                CsvPathProjectUpper,
                LegacyCsvPathResources,
                LegacyCsvPathResourcesUpper))
        {
            Debug.LogError("[BadwordTableImporter] CSV를 찾을 수 없거나 데이터 행이 없습니다.");
            return;
        }

        if (!CsvImportUtility.TryReadCsvLines(csvPath, out string[] lines))
        {
            Debug.LogError("[BadwordTableImporter] CSV를 찾을 수 없거나 데이터 행이 없습니다.");
            return;
        }

        BadwordTable table = CsvImportUtility.LoadOrCreateAsset<BadwordTable>(AssetPath);

        string[] header = CsvImportUtility.ParseHeader(lines[0]);
        int wordIdx = CsvImportUtility.FindColumn(header, "word");
        if (wordIdx < 0)
            wordIdx = 0;

        HashSet<string> uniqueWords = new(System.StringComparer.OrdinalIgnoreCase);

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i]))
                continue;

            string[] cols = CsvImportUtility.ParseRow(lines[i], header.Length > 0 ? header.Length : 1);
            string word = CsvImportUtility.GetCell(cols, wordIdx).Trim();
            if (string.IsNullOrWhiteSpace(word))
                continue;

            uniqueWords.Add(word);
        }

        table.SetWords(new List<string>(uniqueWords));
        EditorUtility.SetDirty(table);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[BadwordTableImporter] {uniqueWords.Count}개 금칙어 임포트 완료 → {AssetPath}");
    }

    private static void TryMoveLegacyCsvToProjectData()
    {
        string legacyPath = null;
        if (File.Exists(LegacyCsvPathResources))
            legacyPath = LegacyCsvPathResources;
        else if (File.Exists(LegacyCsvPathResourcesUpper))
            legacyPath = LegacyCsvPathResourcesUpper;

        if (string.IsNullOrEmpty(legacyPath))
            return;

        if (File.Exists(CsvPathProject) || File.Exists(CsvPathProjectUpper))
            return;

        string projectDir = Path.GetDirectoryName(CsvPathProject);
        if (!Directory.Exists(projectDir))
            Directory.CreateDirectory(projectDir);

        string targetPath = CsvPathProject;
        string metaSource = legacyPath + ".meta";
        string metaTarget = targetPath + ".meta";

        FileUtil.CopyFileOrDirectory(legacyPath, targetPath);
        if (File.Exists(metaSource) && !File.Exists(metaTarget))
            FileUtil.CopyFileOrDirectory(metaSource, metaTarget);

        AssetDatabase.Refresh();
        Debug.Log($"[BadwordTableImporter] 레거시 CSV를 Project/Data 경로로 복사했습니다 → {targetPath}");
    }
}
#endif
