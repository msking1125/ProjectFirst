#if UNITY_EDITOR
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class WaveTableImporter
{
    private const string CsvPath = "Assets/Project/Data/wave.csv";
    private const string AssetPath = "Assets/Project/Data/WaveTable.asset";

    [MenuItem("Tools/Game/Import Wave CSV")]
    public static void Import()
    {
        if (!File.Exists(CsvPath))
        {
            Debug.LogError($"Wave CSV not found at: {CsvPath}");
            return;
        }

        var lines = File.ReadAllLines(CsvPath);
        if (lines.Length < 2)
        {
            Debug.LogError("Wave CSV has no data rows.");
            return;
        }

        var table = AssetDatabase.LoadAssetAtPath<WaveTable>(AssetPath);
        if (table == null)
        {
            table = ScriptableObject.CreateInstance<WaveTable>();
            AssetDatabase.CreateAsset(table, AssetPath);
        }

        // "wave"가 아니라 "waves" 리스트를 가진 WaveTable에 맞게 변경
        // waves가 아니라 wave라는 리스트명으로 바꾸고 싶으면 WaveTable.cs에서 public List<WaveRow> waves = new(); 을 
        // public List<WaveRow> wave = new(); 로 명확히 바꿔야 한다

        table.wave.Clear();

        // 헤더 파싱
        var header = lines[0].Split(',').Select(h => h.Trim()).ToArray();
        int idx(string name) => Array.IndexOf(header, name);

        // 필수 컬럼 체크
        string[] required = { "wave", "spawnCount", "spawnInterval", "enemyHpMul", "enemySpeedMul", "enemyDamageMul", "eliteEvery", "boss", "rewardGold" };
        foreach (var r in required)
            if (idx(r) < 0) { Debug.LogError($"Missing column: {r}"); return; }

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            var cols = lines[i].Split(',').Select(c => c.Trim()).ToArray();

            WaveRow row = new WaveRow();
            row.wave = ParseInt(cols[idx("wave")]);
            row.spawnCount = ParseInt(cols[idx("spawnCount")]);
            row.spawnInterval = ParseFloat(cols[idx("spawnInterval")]);
            row.enemyHpMul = ParseFloat(cols[idx("enemyHpMul")]);
            row.enemySpeedMul = ParseFloat(cols[idx("enemySpeedMul")]);
            row.enemyDamageMul = ParseFloat(cols[idx("enemyDamageMul")]);
            row.eliteEvery = ParseInt(cols[idx("eliteEvery")]);
            row.boss = ParseInt(cols[idx("boss")]) != 0;
            row.rewardGold = ParseInt(cols[idx("rewardGold")]);

            table.wave.Add(row);
        }

        EditorUtility.SetDirty(table);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Imported {table.wave.Count} wave into {AssetPath}");
    }

    private static int ParseInt(string s) => int.TryParse(s, out var v) ? v : 0;

    private static float ParseFloat(string s)
    {
        // 엑셀 지역설정(콤마/점) 이슈 방지
        if (float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var v))
            return v;
        if (float.TryParse(s, out v)) return v;
        return 0f;
    }
}
#endif