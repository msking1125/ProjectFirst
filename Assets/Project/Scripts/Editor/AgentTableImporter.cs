#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Agent CSV를 AgentTable / AgentData / Agent 프리팹에 동기화합니다.
/// </summary>
public static class AgentTableImporter
{
    private const string CsvPathLower = "Assets/Project/Data/agents.csv";
    private const string CsvPathUpper = "Assets/Project/Data/Agents.csv";
    private const string AssetPath = "Assets/Project/Data/AgentTable.asset";
    private const string AgentDataFolder = "Assets/Project/Data";
    private const string AgentPrefabFolder = "Assets/Project/Prefabs/Player";

    private static readonly string[] RequiredColumns =
    {
        "id", "name", "hp", "atk", "def", "critChance", "critMultiplier", "element", "portrait"
    };

    public static void Import()
    {
        if (!CsvImportUtility.TryResolveCsvPath(out string csvPath, CsvPathLower, CsvPathUpper))
        {
            Debug.LogError($"[AgentTableImporter] CSV를 찾을 수 없습니다: {CsvPathLower} (or {CsvPathUpper})");
            return;
        }

        if (!CsvImportUtility.TryReadCsvLines(csvPath, out string[] lines))
        {
            Debug.LogError($"[AgentTableImporter] 데이터 행이 없습니다: {csvPath}");
            return;
        }

        AgentTable table = CsvImportUtility.LoadOrCreateAsset<AgentTable>(AssetPath);
        table.rows.Clear();

        string[] header = CsvImportUtility.ParseHeader(lines[0]);
        if (!CsvImportUtility.ValidateRequiredColumns(header, RequiredColumns, nameof(AgentTableImporter)))
            return;

        int idIdx = CsvImportUtility.FindColumn(header, "id");
        int nameIdx = CsvImportUtility.FindColumn(header, "name");
        int hpIdx = CsvImportUtility.FindColumn(header, "hp");
        int atkIdx = CsvImportUtility.FindColumn(header, "atk");
        int defIdx = CsvImportUtility.FindColumn(header, "def");
        int critChanceIdx = CsvImportUtility.FindColumn(header, "critChance");
        int critMultiplierIdx = CsvImportUtility.FindColumn(header, "critMultiplier");
        int elementIdx = CsvImportUtility.FindColumn(header, "element");
        int portraitIdx = CsvImportUtility.FindColumn(header, "portrait");
        int prefabIdx = CsvImportUtility.FindColumn(header, "prefab");

        int imported = 0;
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i]))
                continue;

            string[] cols = CsvImportUtility.ParseRow(lines[i], header.Length);
            string id = CsvImportUtility.GetCell(cols, idIdx);
            if (string.IsNullOrWhiteSpace(id))
                continue;

            string portraitName = CsvImportUtility.GetCell(cols, portraitIdx);
            Sprite portraitSprite = ResolveSpriteByName(portraitName);

            AgentRow row = new AgentRow
            {
                id = id,
                name = CsvImportUtility.GetCell(cols, nameIdx),
                hp = CsvImportUtility.ParseFloat(CsvImportUtility.GetCell(cols, hpIdx)),
                atk = CsvImportUtility.ParseFloat(CsvImportUtility.GetCell(cols, atkIdx)),
                def = CsvImportUtility.ParseFloat(CsvImportUtility.GetCell(cols, defIdx)),
                critChance = Mathf.Clamp01(CsvImportUtility.ParseFloat(CsvImportUtility.GetCell(cols, critChanceIdx))),
                critMultiplier = Mathf.Max(1f, CsvImportUtility.ParseFloat(CsvImportUtility.GetCell(cols, critMultiplierIdx), 1.5f)),
                element = ParseElement(CsvImportUtility.GetCell(cols, elementIdx), id),
                portrait = portraitSprite
            };

            table.rows.Add(row);

            AgentData agentData = ResolveAgentData(id);
            if (agentData != null)
            {
                agentData.agentId = id;
                agentData.displayName = row.name;
                agentData.portrait = portraitSprite;
                EditorUtility.SetDirty(agentData);
            }

            string prefabName = CsvImportUtility.GetCell(cols, prefabIdx);
            SyncAgentPrefab(prefabName, id, agentData);

            imported++;
        }

        EditorUtility.SetDirty(table);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[AgentTableImporter] Imported {imported} agents into {AssetPath}");
    }

    private static ElementType ParseElement(string raw, string id)
    {
        if (CsvImportUtility.TryParseEnumInsensitive(raw, out ElementType element))
            return element;

        Debug.LogWarning($"[AgentTableImporter] Failed to parse element for id '{id}'. raw='{raw}'. fallback=Reason");
        return ElementType.Reason;
    }

    private static Sprite ResolveSpriteByName(string spriteName)
    {
        if (string.IsNullOrWhiteSpace(spriteName))
            return null;

        string[] guids = AssetDatabase.FindAssets($"{spriteName} t:Sprite", new[] { "Assets/Project" });
        if (guids == null || guids.Length == 0)
            return null;

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static AgentData ResolveAgentData(string agentId)
    {
        string[] guids = AssetDatabase.FindAssets("t:AgentData", new[] { AgentDataFolder });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AgentData data = AssetDatabase.LoadAssetAtPath<AgentData>(path);
            if (data != null && data.agentId == agentId)
                return data;
        }

        return null;
    }

    private static void SyncAgentPrefab(string prefabName, string agentId, AgentData agentData)
    {
        if (string.IsNullOrWhiteSpace(prefabName))
            return;

        string prefabPath = $"{AgentPrefabFolder}/{prefabName}.prefab";
        if (!File.Exists(prefabPath))
        {
            string[] guids = AssetDatabase.FindAssets($"{prefabName} t:Prefab", new[] { "Assets/Project/Prefabs" });
            if (guids != null && guids.Length > 0)
                prefabPath = AssetDatabase.GUIDToAssetPath(guids[0]);
        }

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
        {
            Debug.LogWarning($"[AgentTableImporter] Prefab not found for '{prefabName}' (agentId='{agentId}').");
            return;
        }

        Component agentComp = prefab.GetComponentInChildren(typeof(Project.Agent), true);
        if (agentComp == null)
        {
            Debug.LogWarning($"[AgentTableImporter] Agent component not found in prefab '{prefabPath}'.");
            return;
        }

        SerializedObject so = new SerializedObject(agentComp);
        SerializedProperty idProp = so.FindProperty("agentId");
        if (idProp != null)
            idProp.stringValue = agentId;
        SerializedProperty dataProp = so.FindProperty("agentData");
        if (dataProp != null)
            dataProp.objectReferenceValue = agentData;

        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(prefab);
    }

}
#endif
