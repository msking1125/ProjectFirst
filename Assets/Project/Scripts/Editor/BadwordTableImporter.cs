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

    public static void Import()
    {
        TryMoveLegacyCsvToProjectData();

        if (!CsvImportUtility.TryResolveCsvPath(out string csvPath,
                CsvPathProject,
                CsvPathProjectUpper,
                LegacyCsvPathResources,
                LegacyCsvPathResourcesUpper))
        {
            Debug.LogError($"[BadwordTableImporter] CSV瑜?李얠쓣 ???놁뒿?덈떎: {CsvPathProject} (?먮뒗 ?덇굅??寃쎈줈)");
            return;
        }

        if (!CsvImportUtility.TryReadCsvLines(csvPath, out string[] lines))
        {
            Debug.LogError($"[BadwordTableImporter] ?곗씠???됱씠 ?놁뒿?덈떎: {csvPath}");
            return;
        }

        BadwordTable table = CsvImportUtility.LoadOrCreateAsset<BadwordTable>(AssetPath);

        string[] header = CsvImportUtility.ParseHeader(lines[0]);
        int wordIdx = CsvImportUtility.FindColumn(header, "word");
        if (wordIdx < 0)
        {
            // ?⑥씪 而щ읆 CSV(?ㅻ뜑 ?놁씠 ?⑥뼱留??섏뿴)???명솚
            wordIdx = 0;
        }

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

        Debug.Log($"[BadwordTableImporter] {uniqueWords.Count}媛?湲덉튃???꾪룷???꾨즺 ??{AssetPath}");
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
        Debug.Log($"[BadwordTableImporter] ?덇굅??CSV瑜?Project/Data濡??대룞(蹂듭궗)?덉뒿?덈떎: {legacyPath} -> {targetPath}");
    }
}
#endif


