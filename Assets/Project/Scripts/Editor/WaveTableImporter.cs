#if UNITY_EDITOR
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class WaveTableImporter
{
    private const string DefaultMonsterId = "1";

    private const string CsvPathLower = "Assets/Project/Data/wave.csv";
    private const string CsvPathUpper = "Assets/Project/Data/Wave.csv";
    private const string AssetPath = "Assets/Project/Data/WaveTable.asset";

    [MenuItem("Tools/Game/Import Wave CSV")]
    public static void Import()
    {
        string csvPath = File.Exists(CsvPathLower) ? CsvPathLower : (File.Exists(CsvPathUpper) ? CsvPathUpper : null);
        if (string.IsNullOrEmpty(csvPath))
        {
            Debug.LogError($"Wave CSV not found at: {CsvPathLower} (or {CsvPathUpper})");
            return;
        }

        var lines = File.ReadAllLines(csvPath);
        if (lines.Length < 2)
        {
            Debug.LogError("Wave CSV has no data rows.");
            return;
        }

        WaveTable table = AssetDatabase.LoadAssetAtPath<WaveTable>(AssetPath);
        if (table == null)
        {
            table = ScriptableObject.CreateInstance<WaveTable>();
            AssetDatabase.CreateAsset(table, AssetPath);
        }

        table.wave.Clear();

        var header = lines[0].Split(',').Select(h => h.Trim()).ToArray();
        int colCount = header.Length;

        // 인덱스 캐싱
        int waveIdx = Array.IndexOf(header, "wave");
        int spawnCountIdx = Array.IndexOf(header, "spawnCount");
        int spawnIntervalIdx = Array.IndexOf(header, "spawnInterval");
        int enemyHpMulIdx = Array.IndexOf(header, "enemyHpMul");
        int enemySpeedMulIdx = Array.IndexOf(header, "enemySpeedMul");
        int enemyDamageMulIdx = Array.IndexOf(header, "enemyDamageMul");
        int eliteEveryIdx = Array.IndexOf(header, "eliteEvery");
        int bossIdx = Array.IndexOf(header, "boss");
        int rewardGoldIdx = Array.IndexOf(header, "rewardGold");
        int enemyIdIdx = Array.IndexOf(header, "enemyId");
        int monsterIdIdx = Array.IndexOf(header, "monsterId");

        // 필수 컬럼 존재 체크
        string[] required = { "wave", "spawnCount", "spawnInterval", "enemyHpMul", "enemySpeedMul", "enemyDamageMul", "eliteEvery", "boss", "rewardGold" };
        foreach (var col in required)
        {
            if (Array.IndexOf(header, col) < 0)
            {
                Debug.LogError($"Missing required column in CSV: '{col}'.");
                return;
            }
        }

        for (int i = 1, n = lines.Length; i < n; ++i)
        {
            var line = lines[i];
            if (string.IsNullOrWhiteSpace(line)) continue;
            var cols = line.Split(',');
            if (cols.Length < colCount)
            {
                Debug.LogWarning($"Skipping incomplete row at line {i + 1} (expected {colCount} columns, got {cols.Length}).");
                continue;
            }

            for (int c = 0; c < colCount; ++c)
                cols[c] = cols[c].Trim();

            WaveRow row = new WaveRow
            {
                wave = ParseInt(SafeGet(cols, waveIdx)),
                spawnCount = ParseInt(SafeGet(cols, spawnCountIdx)),
                spawnInterval = ParseFloat(SafeGet(cols, spawnIntervalIdx)),
                enemyHpMul = ParseFloat(SafeGet(cols, enemyHpMulIdx)),
                enemySpeedMul = ParseFloat(SafeGet(cols, enemySpeedMulIdx)),
                enemyDamageMul = ParseFloat(SafeGet(cols, enemyDamageMulIdx)),
                eliteEvery = ParseInt(SafeGet(cols, eliteEveryIdx)),
                boss = ParseInt(SafeGet(cols, bossIdx)) != 0,
                rewardGold = ParseInt(SafeGet(cols, rewardGoldIdx)),
                enemyId = ResolvePreferredId(cols, enemyIdIdx, monsterIdIdx),
                monsterId = ReadIdCell(cols, monsterIdIdx)
            };
            table.wave.Add(row);
        }

        EditorUtility.SetDirty(table);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Imported {table.wave.Count} wave{(table.wave.Count == 1 ? "" : "s")} into {AssetPath}");
    }

    private static string SafeGet(string[] cols, int index)
    {
        if (cols == null || index < 0 || index >= cols.Length)
            return string.Empty;
        return cols[index].Trim();
    }

    private static string ResolvePreferredId(string[] cols, int enemyIdIdx, int monsterIdIdx)
    {
        string enemyId = ReadIdCell(cols, enemyIdIdx);
        if (!string.IsNullOrWhiteSpace(enemyId))
        {
            return enemyId;
        }

        string monsterId = ReadIdCell(cols, monsterIdIdx);
        if (!string.IsNullOrWhiteSpace(monsterId))
        {
            return monsterId;
        }

        return DefaultMonsterId;
    }

    private static string ReadIdCell(string[] cols, int index)
    {
        if (cols == null || index < 0 || index >= cols.Length)
        {
            return string.Empty;
        }

        return cols[index].Trim();
    }

    private static int ParseInt(string s)
    {
        if (string.IsNullOrWhiteSpace(s))
            return 0;
        int v;
        // Try parsing with InvariantCulture (for possible CSV differences)
        if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out v))
            return v;
        if (int.TryParse(s, out v))
            return v;
        return 0;
    }

    private static float ParseFloat(string s)
    {
        if (string.IsNullOrWhiteSpace(s))
            return 0f;
        if (float.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out float v)) return v;
        if (float.TryParse(s, out v)) return v;
        return 0f;
    }
}
#endif
