#if UNITY_EDITOR
using System;
using System.Globalization;
using System.IO;
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

        var table = AssetDatabase.LoadAssetAtPath<WaveTable>(AssetPath) ?? ScriptableObject.CreateInstance<WaveTable>();
        if (AssetDatabase.LoadAssetAtPath<WaveTable>(AssetPath) == null)
            AssetDatabase.CreateAsset(table, AssetPath);

        table.wave.Clear();

        var header = lines[0].Split(',');
        int colCount = header.Length;
        for (int h = 0; h < colCount; ++h)
            header[h] = header[h].Trim();

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
        if (waveIdx < 0 || spawnCountIdx < 0 || spawnIntervalIdx < 0 ||
            enemyHpMulIdx < 0 || enemySpeedMulIdx < 0 || enemyDamageMulIdx < 0 ||
            eliteEveryIdx < 0 || bossIdx < 0 || rewardGoldIdx < 0)
        {
            Debug.LogError("Missing required columns in CSV.");
            return;
        }

        for (int i = 1, n = lines.Length; i < n; ++i)
        {
            var line = lines[i];
            if (string.IsNullOrWhiteSpace(line)) continue;
            var cols = line.Split(',');
            if (cols.Length < colCount)
            {
                Debug.LogWarning($"Skipping incomplete row at line {i+1} (expected {colCount} columns, got {cols.Length}).");
                continue;
            }
            for (int c = 0; c < colCount; ++c)
                cols[c] = cols[c].Trim();

            WaveRow row = new WaveRow
            {
                wave = ParseInt(cols[waveIdx]),
                spawnCount = ParseInt(cols[spawnCountIdx]),
                spawnInterval = ParseFloat(cols[spawnIntervalIdx]),
                enemyHpMul = ParseFloat(cols[enemyHpMulIdx]),
                enemySpeedMul = ParseFloat(cols[enemySpeedMulIdx]),
                enemyDamageMul = ParseFloat(cols[enemyDamageMulIdx]),
                eliteEvery = ParseInt(cols[eliteEveryIdx]),
                boss = ParseInt(cols[bossIdx]) != 0,
                rewardGold = ParseInt(cols[rewardGoldIdx]),
                enemyId = ResolvePreferredId(cols, enemyIdIdx, monsterIdIdx),
                monsterId = ReadIdCell(cols, monsterIdIdx)
            };
            table.wave.Add(row);
        }

        EditorUtility.SetDirty(table);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Imported {table.wave.Count} wave into {AssetPath}");
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
        if (index < 0 || cols == null || index >= cols.Length)
        {
            return string.Empty;
        }

        return cols[index].Trim();
    }

    private static int ParseInt(string s)
    {
        int v;
        return int.TryParse(s, out v) ? v : 0;
    }

    private static float ParseFloat(string s)
    {
        if (float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out float v)) return v;
        if (float.TryParse(s, out v)) return v;
        return 0f;
    }
}
#endif
