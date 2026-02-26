#if UNITY_EDITOR
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// AgentTableImporter는 에이전트(CSV) 데이터를 ScriptableObject로 임포트하는 에디터 유틸입니다.
/// 
/// <b>다음과 같은 오류 발생 원인과 해결방법을 참고하세요:</b>
/// <para>
/// <b>[에러 메시지]</b>
/// Agent CSV not found at: Assets/Project/Data/agents.csv
/// UnityEngine.Debug:LogError (object)
/// AgentTableImporter:Import () (at Assets/Project/Scripts/Editor/AgentTableImporter.cs:23)
/// </para>
/// <b>[원인]</b>
/// 지정된 경로(Assets/Project/Data/agents.csv)에 에이전트 데이터를 담은 CSV 파일이 없다는 뜻입니다. 
/// 이 파일이 존재하지 않으면 데이터를 불러오지 못해 임포트가 중단됩니다.
/// 
/// <b>[수정 방법]</b>
/// 1. <b>프로젝트의 Assets/Project/Data/ 경로에 agents.csv 파일이 실제로 있는지 확인하세요.</b>
/// 2. <b>CSV 파일명을 오탈자 없이 정확하게 맞추어 놓으세요. (예: Agents.csv, agentsCSV.csv 등은 안됩니다)</b>
/// 3. <b>경로가 다를 경우 아래 CsvPath 상수를 올바른 경로로 수정하세요.</b>
/// </summary>
public static class AgentTableImporter
{
    // [agents.csv 경로와 파일명을 꼭 확인하세요!]
    private const string CsvPath = "Assets/Project/Data/agents.csv";
    private const string AssetPath = "Assets/Project/Data/AgentTable.asset";
    private static readonly string[] RequiredColumns =
    {
        "id", "name", "hp", "def", "critChance", "critMultiplier", "element"
    };

    [MenuItem("Tools/Game/Import Agent CSV")]
    public static void Import()
    {
        if (!File.Exists(CsvPath))
        {
            Debug.LogError($"Agent CSV not found at: {CsvPath}\n\n- 위 경로에 agents.csv 파일이 있는지 확인하세요!\n- 파일명 오탈자 혹은 위치 오류가 없는지도 점검하세요.");
            return;
        }

        string[] lines = File.ReadAllLines(CsvPath);
        if (lines.Length < 2)
        {
            Debug.LogError("Agent CSV has no data rows.");
            return;
        }

        AgentTable table = AssetDatabase.LoadAssetAtPath<AgentTable>(AssetPath);
        if (table == null)
        {
            table = ScriptableObject.CreateInstance<AgentTable>();
            AssetDatabase.CreateAsset(table, AssetPath);
        }

        table.rows.Clear();

        string[] header = lines[0].Split(',').Select(h => h.Trim()).ToArray();
        int idx(string name) => Array.IndexOf(header, name);

        foreach (string col in RequiredColumns)
        {
            if (idx(col) < 0)
            {
                Debug.LogError($"Missing column: {col}");
                return;
            }
        }

        int idIdx = idx("id");
        int nameIdx = idx("name");
        int hpIdx = idx("hp");
        int atkIdx = idx("atk");
        if (atkIdx < 0)
        {
            atkIdx = idx("baseAtk");
        }
        int defIdx = idx("def");
        int critChanceIdx = idx("critChance");
        int critMultiplierIdx = idx("critMultiplier");
        int elementIdx = idx("element");

        if (atkIdx < 0)
        {
            Debug.LogError("Missing column: atk (or baseAtk)");
            return;
        }

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i]))
            {
                continue;
            }

            string[] cols = lines[i].Split(',').Select(c => c.Trim()).ToArray();
            string id = ReadCell(cols, idIdx);
            string elementRaw = ReadCell(cols, elementIdx);

            AgentRow row = new AgentRow
            {
                id = id,
                name = ReadCell(cols, nameIdx),
                hp = ParseFloat(ReadCell(cols, hpIdx)),
                atk = ParseFloat(ReadCell(cols, atkIdx)),
                def = ParseFloat(ReadCell(cols, defIdx)),
                critChance = Mathf.Clamp01(ParseFloat(ReadCell(cols, critChanceIdx))),
                critMultiplier = Mathf.Max(1f, ParseFloat(ReadCell(cols, critMultiplierIdx), 1.5f)),
                element = ParseElement(elementRaw, id)
            };

            table.rows.Add(row);
        }

        EditorUtility.SetDirty(table);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Imported {table.rows.Count} agents into {AssetPath}");
    }

    [MenuItem("Assets/Import CSV/Agent Table", false, 2001)]
    private static void ImportFromAssetsMenu()
    {
        Import();
    }

    private static string ReadCell(string[] cols, int index)
    {
        if (index < 0 || cols == null || index >= cols.Length)
        {
            return string.Empty;
        }

        return cols[index];
    }

    private static float ParseFloat(string s, float defaultValue = 0f)
    {
        if (float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out float v))
        {
            return v;
        }

        if (float.TryParse(s, out v))
        {
            return v;
        }

        return defaultValue;
    }

    private static ElementType ParseElement(string raw, string id)
    {
        if (Enum.TryParse(raw, true, out ElementType element))
        {
            return element;
        }

        Debug.LogWarning($"[AgentTableImporter] Failed to parse element for id '{id}'. raw='{raw}'. fallback=Reason");
        return ElementType.Reason;
    }
}
#endif
