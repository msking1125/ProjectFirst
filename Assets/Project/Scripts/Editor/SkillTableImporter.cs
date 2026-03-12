#if UNITY_EDITOR
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using ProjectFirst.Data;
/// <summary>
/// CSV ??SkillTable ScriptableObject ?꾪룷??
/// icon 而щ읆: ?뚯씪紐??뺤옣???쒖쇅) ??Assets/Project/UI/Icon/ ?먯꽌 Sprite ?먮룞 ?곌껐
/// castVfxPrefab 而щ읆: ?뚯씪紐??뺤옣???쒖쇅) ???꾨줈?앺듃 ?꾩껜?먯꽌 Prefab ?먮룞 ?곌껐
/// </summary>
public static class SkillTableImporter
{
    private const string CsvPathLower = "Assets/Project/Data/skills.csv";
    private const string CsvPathUpper = "Assets/Project/Data/Skills.csv";
    private const string AssetPath = "Assets/Project/Data/SkillTable.asset";

    // ?꾩씠肄?寃???곗꽑 ?대뜑 (?놁쑝硫??꾨줈?앺듃 ?꾩껜 ?먯깋)
    private const string IconFolder = "Assets/Project/UI/Icon";
    // VFX 寃???곗꽑 ?대뜑 (?놁쑝硫??꾨줈?앺듃 ?꾩껜 ?먯깋)
    private const string VfxFolder  = "Assets/Project/Prefabs/VFX";

    private static readonly string[] RequiredColumns = { "id", "name", "element", "coefficient", "range" };

    public static void Import()
    {
        if (!CsvImportUtility.TryResolveCsvPath(out string csvPath, CsvPathLower, CsvPathUpper))
        {
            Debug.LogError($"[SkillTableImporter] CSV瑜?李얠쓣 ???놁뒿?덈떎: {CsvPathLower} (or {CsvPathUpper})");
            return;
        }

        if (!CsvImportUtility.TryReadCsvLines(csvPath, out string[] lines))
        {
            Debug.LogError($"[SkillTableImporter] ?곗씠???됱씠 ?놁뒿?덈떎: {csvPath}");
            return;
        }

        SkillTable table = AssetDatabase.LoadAssetAtPath<SkillTable>(AssetPath)
                           ?? CreateAsset();

        SerializedObject so       = new SerializedObject(table);
        SerializedProperty rowsProp = so.FindProperty("rows");
        if (rowsProp == null)
        {
            Debug.LogError("[SkillTableImporter] SkillTable??'rows' ?꾨뱶媛 ?놁뒿?덈떎.");
            return;
        }
        rowsProp.ClearArray();

        string[] header = CsvImportUtility.ParseHeader(lines[0]);
        int ColIdx(string n) => CsvImportUtility.FindColumn(header, n);

        foreach (string col in RequiredColumns)
        {
            if (ColIdx(col) < 0)
            {
                Debug.LogError($"[SkillTableImporter] ?꾩닔 而щ읆 '{col}'???놁뒿?덈떎.");
                return;
            }
        }

        int idIdx          = ColIdx("id");
        int nameIdx        = ColIdx("name");
        int elementIdx     = ColIdx("element");
        int coeffIdx       = ColIdx("coefficient");
        int rangeIdx       = ColIdx("range");
        int cooldownIdx    = ColIdx("cooldown");
        int iconIdx           = ColIdx("icon");
        int vfxIdx            = ColIdx("castVfxPrefab");
        int descIdx           = ColIdx("description");
        int effectTypeIdx     = ColIdx("effectType");
        int singleBonusIdx    = ColIdx("singleTargetBonus");
        int buffStatIdx       = ColIdx("buffStat");
        int buffMultIdx       = ColIdx("buffMultiplier");
        int buffDurIdx        = ColIdx("buffDuration");
        int debuffTypeIdx     = ColIdx("debuffType");
        int debuffValueIdx    = ColIdx("debuffValue");
        int debuffDurIdx      = ColIdx("debuffDuration");

        int imported = 0;
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i];
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] cols = CsvImportUtility.ParseRow(line, header.Length);

            rowsProp.InsertArrayElementAtIndex(imported);
            SerializedProperty row = rowsProp.GetArrayElementAtIndex(imported);

            if (!int.TryParse(GetCell(cols, idIdx), out int id))
                continue;
            row.FindPropertyRelative("id").intValue             = id;
            row.FindPropertyRelative("name").stringValue        = GetCell(cols, nameIdx);
            row.FindPropertyRelative("coefficient").floatValue  = Mathf.Max(0.1f, StrToFloat(GetCell(cols, coeffIdx), 1f));
            row.FindPropertyRelative("range").floatValue        = Mathf.Max(0f,   StrToFloat(GetCell(cols, rangeIdx), 9999f));

            if (cooldownIdx >= 0)
                row.FindPropertyRelative("cooldown").floatValue = Mathf.Max(0f, StrToFloat(GetCell(cols, cooldownIdx), 5f));

            // ElementType
            string elemRaw = GetCell(cols, elementIdx);
            if (!Enum.TryParse(elemRaw, true, out ElementType element))
            {
                Debug.LogWarning($"[SkillTableImporter] id='{id}' element='{elemRaw}' parse failed. fallback=Reason");
                element = ElementType.Reason;
            }
            row.FindPropertyRelative("element").enumValueIndex = (int)element;

            // ?? icon: ?뚯씪紐낆쑝濡?Sprite ?먯깋 ??????????????????????????????????
            if (iconIdx >= 0)
            {
                string iconName = GetCell(cols, iconIdx);
                Sprite sprite   = FindSprite(iconName, id.ToString());
                SerializedProperty iconProp = row.FindPropertyRelative("icon");
                if (sprite != null)
                    iconProp.objectReferenceValue = sprite;
                else
                    iconProp.objectReferenceValue = null;
            }

            // ?? castVfxPrefab ??????????????????????????????????????????????????
            if (vfxIdx >= 0)
            {
                string vfxName   = GetCell(cols, vfxIdx);
                GameObject prefab = FindPrefab(vfxName, id.ToString());
                SerializedProperty vfxProp = row.FindPropertyRelative("castVfxPrefab");
                vfxProp.objectReferenceValue = prefab;
            }

            // ?? description ????????????????????????????????????????????????
            if (descIdx >= 0)
                row.FindPropertyRelative("description").stringValue = GetCell(cols, descIdx);

            // ?? effectType & ?④낵蹂??섏튂 ????????????????????????????????????
            if (effectTypeIdx >= 0)
            {
                string etRaw = GetCell(cols, effectTypeIdx);
                if (System.Enum.TryParse(etRaw, true, out SkillEffectType et))
                    row.FindPropertyRelative("effectType").enumValueIndex = (int)et;
            }
            if (singleBonusIdx >= 0)
                row.FindPropertyRelative("singleTargetBonus").floatValue = StrToFloat(GetCell(cols, singleBonusIdx), 2f);
            if (buffStatIdx >= 0)
            {
                if (System.Enum.TryParse(GetCell(cols, buffStatIdx), true, out BuffStatType bs))
                    row.FindPropertyRelative("buffStat").enumValueIndex = (int)bs;
            }
            if (buffMultIdx >= 0)
                row.FindPropertyRelative("buffMultiplier").floatValue = StrToFloat(GetCell(cols, buffMultIdx), 0.3f);
            if (buffDurIdx >= 0)
                row.FindPropertyRelative("buffDuration").floatValue = StrToFloat(GetCell(cols, buffDurIdx), 10f);
            if (debuffTypeIdx >= 0)
            {
                if (System.Enum.TryParse(GetCell(cols, debuffTypeIdx), true, out DebuffType dt))
                    row.FindPropertyRelative("debuffType").enumValueIndex = (int)dt;
            }
            if (debuffValueIdx >= 0)
                row.FindPropertyRelative("debuffValue").floatValue = StrToFloat(GetCell(cols, debuffValueIdx), 0.5f);
            if (debuffDurIdx >= 0)
                row.FindPropertyRelative("debuffDuration").floatValue = StrToFloat(GetCell(cols, debuffDurIdx), 5f);

            imported++;
        }

        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(table);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[SkillTableImporter] {imported}媛??ㅽ궗 ?꾪룷???꾨즺 ??{AssetPath}");
    }

    // ?? ?먯뀑 ?먯깋 ?ы띁 ???????????????????????????????????????????????????????

    /// <summary>
    /// ?뚯씪紐??뺤옣???놁쓬)?쇰줈 Sprite瑜??먯깋?⑸땲??
    /// ?곗꽑 IconFolder ?덉뿉??李얘퀬, ?놁쑝硫??꾨줈?앺듃 ?꾩껜?먯꽌 ?먯깋.
    /// </summary>
    private static Sprite FindSprite(string assetName, string rowId)
    {
        if (string.IsNullOrWhiteSpace(assetName)) return null;

        // 1. IconFolder ??吏곸젒 寃쎈줈 ?쒕룄 (jpg / png)
        foreach (string ext in new[] { ".jpg", ".png", ".jpeg" })
        {
            string path = $"{IconFolder}/{assetName}{ext}";
            // Texture濡?濡쒕뱶????Sprite濡?蹂??
            Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (s != null)
            {
                Debug.Log($"[SkillTableImporter] id='{rowId}' icon ?곌껐: {path}");
                return s;
            }
        }

        // 2. ?꾨줈?앺듃 ?꾩껜 ?먯깋 (t:Sprite)
        string[] guids = AssetDatabase.FindAssets($"{assetName} t:Sprite");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string fileName = Path.GetFileNameWithoutExtension(path);
            if (fileName.Equals(assetName, StringComparison.OrdinalIgnoreCase))
            {
                Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (s != null)
                {
                    Debug.Log($"[SkillTableImporter] id='{rowId}' icon ?곌껐 (?꾩껜?먯깋): {path}");
                    return s;
                }
            }
        }

        // 3. Texture2D濡?李얠븘 Sprite 蹂???쒕룄
        guids = AssetDatabase.FindAssets($"{assetName} t:Texture2D");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string fileName = Path.GetFileNameWithoutExtension(path);
            if (fileName.Equals(assetName, StringComparison.OrdinalIgnoreCase))
            {
                Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (s != null)
                {
                    Debug.Log($"[SkillTableImporter] id='{rowId}' icon ?곌껐 (Texture2D): {path}");
                    return s;
                }
            }
        }

        Debug.LogWarning(
            $"[SkillTableImporter] id='{rowId}' icon='{assetName}' ??李얠? 紐삵뻽?듬땲??\n" +
            $"?뺤씤: {IconFolder}/{assetName}.jpg|png ??議댁옱?섎뒗吏 ?뺤씤?섏꽭??\n" +
            $"?먰븳 Texture Import Settings ??Sprite Mode = Single 濡??ㅼ젙?섏꽭??");
        return null;
    }

    /// <summary>
    /// ?뚯씪紐??뺤옣???놁쓬)?쇰줈 Prefab???먯깋?⑸땲??
    /// ?곗꽑 VfxFolder ?덉뿉??李얘퀬, ?놁쑝硫??꾨줈?앺듃 ?꾩껜?먯꽌 ?먯깋.
    /// </summary>
    private static GameObject FindPrefab(string assetName, string rowId)
    {
        if (string.IsNullOrWhiteSpace(assetName)) return null;

        // 1. VfxFolder 吏곸젒 寃쎈줈 ?쒕룄
        string directPath = $"{VfxFolder}/{assetName}.prefab";
        GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(directPath);
        if (go != null)
        {
            Debug.Log($"[SkillTableImporter] id='{rowId}' VFX ?곌껐: {directPath}");
            return go;
        }

        // 2. ?꾨줈?앺듃 ?꾩껜 ?먯깋 (t:Prefab)
        string[] guids = AssetDatabase.FindAssets($"{assetName} t:Prefab");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string fileName = Path.GetFileNameWithoutExtension(path);
            if (fileName.Equals(assetName, StringComparison.OrdinalIgnoreCase))
            {
                go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (go != null)
                {
                    Debug.Log($"[SkillTableImporter] id='{rowId}' VFX ?곌껐 (?꾩껜?먯깋): {path}");
                    return go;
                }
            }
        }

        Debug.LogWarning(
            $"[SkillTableImporter] id='{rowId}' castVfxPrefab='{assetName}' ??李얠? 紐삵뻽?듬땲??\n" +
            $"?뺤씤: ?꾨줈?앺듃 ?대뵖媛??'{assetName}.prefab' ??議댁옱?섎뒗吏 ?뺤씤?섏꽭??");
        return null;
    }

    // ?? ?좏떥 ?????????????????????????????????????????????????????????????????

    private static SkillTable CreateAsset()
    {
        SkillTable t = ScriptableObject.CreateInstance<SkillTable>();
        AssetDatabase.CreateAsset(t, AssetPath);
        return t;
    }

    private static string GetCell(string[] arr, int idx)
        => arr == null || idx < 0 || idx >= arr.Length ? string.Empty : arr[idx];

    private static float StrToFloat(string s, float fallback)
    {
        if (string.IsNullOrWhiteSpace(s)) return fallback;
        if (float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out float v)) return v;
        if (float.TryParse(s, out v)) return v;
        return fallback;
    }
}
#endif


