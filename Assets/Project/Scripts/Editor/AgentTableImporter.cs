#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using ProjectFirst.Data;

public static class AgentTableImporter
{
    private const string CsvPathLower = "Assets/Project/Data/agents.csv";
    private const string CsvPathUpper = "Assets/Project/Data/Agents.csv";
    private const string AssetPath = "Assets/Project/Data/AgentTable.asset";
    private const string AgentDataFolder = "Assets/Project/Data";
    private const string AgentPrefabFolder = "Assets/Project/Prefabs/Player";

    private static readonly string[] RequiredColumns =
    {
        "id", "name", "hp", "atk", "def", "critChance", "critMultiplier", "element"
    };

    public static void Import()
    {
        if (!CsvImportUtility.TryResolveCsvPath(out string csvPath, CsvPathLower, CsvPathUpper))
        {
            Debug.LogError($"[AgentTableImporter] CSV not found: {CsvPathLower} (or {CsvPathUpper})");
            return;
        }

        if (!CsvImportUtility.TryReadCsvLines(csvPath, out string[] lines))
        {
            Debug.LogError($"[AgentTableImporter] CSV not found: {CsvPathLower} (or {CsvPathUpper})");
            return;
        }

        AgentTable table = CsvImportUtility.LoadOrCreateAsset<AgentTable>(AssetPath);
        table.rows.Clear();

        SerializedObject tableSo = new SerializedObject(table);
        SerializedProperty agentInfosProp = tableSo.FindProperty("_agentInfos");
        if (agentInfosProp != null)
            agentInfosProp.ClearArray();

        HashSet<string> syncedPrefabNames = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);

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
            string rawId = CsvImportUtility.GetCell(cols, idIdx);
            if (!CsvImportUtility.TryParseFlexibleInt(rawId, out int id))
            {
                Debug.LogWarning($"[AgentTableImporter] Skipping row with invalid id: '{rawId}'");
                continue;
            }

            string portraitName = CsvImportUtility.GetCell(cols, portraitIdx);
            Sprite portraitSprite = ResolveSpriteByName(portraitName);

            AgentRow row = new AgentRow
            {
                id = id,
                name = CsvImportUtility.GetCell(cols, nameIdx),
                stats = new CombatStats(
                    CsvImportUtility.ParseFloat(CsvImportUtility.GetCell(cols, hpIdx)),
                    CsvImportUtility.ParseFloat(CsvImportUtility.GetCell(cols, atkIdx)),
                    CsvImportUtility.ParseFloat(CsvImportUtility.GetCell(cols, defIdx)),
                    Mathf.Clamp01(CsvImportUtility.ParseFloat(CsvImportUtility.GetCell(cols, critChanceIdx))),
                    Mathf.Max(1f, CsvImportUtility.ParseFloat(CsvImportUtility.GetCell(cols, critMultiplierIdx), 1.5f))
                ),
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
            GameObject prefab = ResolvePrefab(prefabName, id);
            if (!string.IsNullOrWhiteSpace(prefabName) && syncedPrefabNames.Add(prefabName))
                SyncAgentPrefab(prefab, id, agentData);

            AppendAgentInfo(agentInfosProp, row, prefab);
            imported++;
        }

        tableSo.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(table);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[AgentTableImporter] Imported {imported} agents into {AssetPath}");
    }

    private static ElementType ParseElement(string raw, int id)
    {
        if (CsvImportUtility.TryParseEnumInsensitive(raw, out ElementType element))
            return element;

        Debug.LogWarning($"[AgentTableImporter] Failed to parse element '{raw}' for id '{id}'. Falling back to Reason.");
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

    private static AgentData ResolveAgentData(int agentId)
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

    private static GameObject ResolvePrefab(string prefabName, int agentId)
    {
        if (string.IsNullOrWhiteSpace(prefabName))
            return null;

        string prefabPath = $"{AgentPrefabFolder}/{prefabName}.prefab";
        if (!File.Exists(prefabPath))
        {
            string[] guids = AssetDatabase.FindAssets($"{prefabName} t:Prefab", new[] { "Assets/Project/Prefabs" });
            if (guids != null && guids.Length > 0)
                prefabPath = AssetDatabase.GUIDToAssetPath(guids[0]);
        }

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
            Debug.LogWarning($"[AgentTableImporter] Could not find prefab '{prefabName}' for agentId '{agentId}'");

        return prefab;
    }

    private static void SyncAgentPrefab(GameObject prefab, int agentId, AgentData agentData)
    {
        if (prefab == null)
            return;

        Component agentComp = prefab.GetComponentInChildren(typeof(Project.Agent), true);
        if (agentComp == null)
        {
            Debug.LogWarning($"[AgentTableImporter] Could not find Agent component in prefab: {prefab.name}");
            return;
        }

        SerializedObject so = new SerializedObject(agentComp);
        SerializedProperty idProp = so.FindProperty("agentId");
        if (idProp != null)
            idProp.intValue = agentId;

        SerializedProperty dataProp = so.FindProperty("agentData");
        if (dataProp != null)
            dataProp.objectReferenceValue = agentData;

        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(prefab);
        PrefabUtility.SavePrefabAsset(prefab);
    }

    private static void AppendAgentInfo(SerializedProperty agentInfosProp, AgentRow row, GameObject prefab)
    {
        if (agentInfosProp == null || row == null)
            return;

        int index = agentInfosProp.arraySize;
        agentInfosProp.InsertArrayElementAtIndex(index);
        SerializedProperty infoProp = agentInfosProp.GetArrayElementAtIndex(index);

        infoProp.FindPropertyRelative("_id").intValue = row.id;
        infoProp.FindPropertyRelative("_agentName").stringValue = row.name ?? string.Empty;
        infoProp.FindPropertyRelative("_subName").stringValue = string.Empty;
        infoProp.FindPropertyRelative("_element").enumValueIndex = (int)row.element;
        infoProp.FindPropertyRelative("_attackType").enumValueIndex = 0;
        infoProp.FindPropertyRelative("_grade").intValue = 1;
        infoProp.FindPropertyRelative("_thumbnail").objectReferenceValue = row.portrait;
        infoProp.FindPropertyRelative("_modelPrefab").objectReferenceValue = prefab;
        infoProp.FindPropertyRelative("_skills").arraySize = 0;
        infoProp.FindPropertyRelative("_baseHp").floatValue = row.stats.hp;
        infoProp.FindPropertyRelative("_baseAtk").floatValue = row.stats.atk;
        infoProp.FindPropertyRelative("_baseDef").floatValue = row.stats.def;
        infoProp.FindPropertyRelative("_critRate").floatValue = row.stats.critChance;
        infoProp.FindPropertyRelative("_critMult").floatValue = row.stats.critMultiplier;
        infoProp.FindPropertyRelative("_hpGrowth").floatValue = 0.08f;
        infoProp.FindPropertyRelative("_atkGrowth").floatValue = 0.06f;
        infoProp.FindPropertyRelative("_defGrowth").floatValue = 0.04f;
    }
}
#endif
