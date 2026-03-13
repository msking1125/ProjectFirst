#if UNITY_EDITOR
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

internal static class CsvImportUtility
{
    public static bool TryResolveCsvPath(out string resolvedPath, params string[] candidates)
    {
        resolvedPath = candidates?.FirstOrDefault(File.Exists);
        return !string.IsNullOrEmpty(resolvedPath);
    }

    public static bool TryReadCsvLines(string csvPath, out string[] lines)
    {
        lines = null;
        if (string.IsNullOrEmpty(csvPath) || !File.Exists(csvPath))
            return false;

        lines = File.ReadAllLines(csvPath);
        return lines != null && lines.Length >= 2;
    }

    public static string[] ParseHeader(string headerLine)
    {
        return string.IsNullOrEmpty(headerLine)
            ? Array.Empty<string>()
            : ParseCsvRow(headerLine);
    }

    public static string[] ParseRow(string line, int minLength)
    {
        string[] cols = string.IsNullOrWhiteSpace(line)
            ? Array.Empty<string>()
            : ParseCsvRow(line);

        if (minLength > 0 && cols.Length < minLength)
            Array.Resize(ref cols, minLength);

        return cols;
    }

    public static int FindColumn(string[] header, string columnName)
    {
        if (header == null) return -1;
        return Array.FindIndex(header, h => string.Equals(h, columnName, StringComparison.OrdinalIgnoreCase));
    }

    public static bool ValidateRequiredColumns(string[] header, string[] requiredColumns, string importerName)
    {
        foreach (string col in requiredColumns)
        {
            if (FindColumn(header, col) < 0)
            {
                Debug.LogError($"[{importerName}] 필수 컬럼이 없습니다: '{col}'");
                return false;
            }
        }

        return true;
    }

    public static string GetCell(string[] cols, int idx)
    {
        if (cols == null || idx < 0 || idx >= cols.Length) return string.Empty;
        return cols[idx]?.Trim() ?? string.Empty;
    }

    public static bool EnsureCsvExists(string csvPath, string sampleContent, string importerName)
    {
        if (string.IsNullOrWhiteSpace(csvPath) || File.Exists(csvPath))
            return true;

        string directory = Path.GetDirectoryName(csvPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        File.WriteAllText(csvPath, sampleContent.Replace("\n", Environment.NewLine), new UTF8Encoding(true));
        AssetDatabase.Refresh();
        Debug.Log($"[{importerName}] CSV가 없어 샘플 파일을 생성했습니다: {csvPath}");
        return true;
    }

    public static float ParseFloat(string raw, float defaultValue = 0f)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return defaultValue;

        if (float.TryParse(raw, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out float value))
            return value;

        if (float.TryParse(raw, out value))
            return value;

        return defaultValue;
    }

    public static int ParseInt(string raw, int defaultValue = 0)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return defaultValue;

        if (int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
            return value;

        if (int.TryParse(raw, out value))
            return value;

        return defaultValue;
    }

    public static bool TryParseFlexibleInt(string raw, out int value)
    {
        raw = raw?.Trim() ?? string.Empty;
        if (int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
            return true;

        string digits = new string(raw.Where(char.IsDigit).ToArray());
        if (!string.IsNullOrEmpty(digits) &&
            int.TryParse(digits, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
        {
            return true;
        }

        value = 0;
        return false;
    }

    public static bool TryParseEnumInsensitive<TEnum>(string raw, out TEnum value) where TEnum : struct, Enum
    {
        string normalized = string.IsNullOrWhiteSpace(raw) ? string.Empty : raw.Trim();
        if (Enum.TryParse(normalized, true, out value))
            return true;

        string compact = normalized.Replace(" ", string.Empty).Replace("_", string.Empty);
        if (!string.IsNullOrEmpty(compact))
        {
            foreach (string name in Enum.GetNames(typeof(TEnum)))
            {
                if (string.Equals(name.Replace("_", string.Empty), compact, StringComparison.OrdinalIgnoreCase))
                {
                    value = (TEnum)Enum.Parse(typeof(TEnum), name);
                    return true;
                }
            }
        }

        value = default;
        return false;
    }

    public static T LoadOrCreateAsset<T>(string assetPath) where T : ScriptableObject
    {
        T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
        if (asset != null)
            return asset;

        asset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(asset, assetPath);
        return asset;
    }

    private static string[] ParseCsvRow(string line)
    {
        var cells = new System.Collections.Generic.List<string>();
        var current = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char ch = line[i];
            if (ch == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }

                continue;
            }

            if (ch == ',' && !inQuotes)
            {
                cells.Add(current.ToString().Trim());
                current.Clear();
                continue;
            }

            current.Append(ch);
        }

        cells.Add(current.ToString().Trim());
        return cells.ToArray();
    }
}
#endif
