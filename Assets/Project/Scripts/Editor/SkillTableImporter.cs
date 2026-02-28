#if UNITY_EDITOR
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// CSV → SkillTable ScriptableObject 임포터.
/// icon 컬럼: 파일명(확장자 제외) → Assets/Project/UI/Icon/ 에서 Sprite 자동 연결
/// castVfxPrefab 컬럼: 파일명(확장자 제외) → 프로젝트 전체에서 Prefab 자동 연결
/// </summary>
public static class SkillTableImporter
{
    private const string CsvPath   = "Assets/Project/Data/skills.csv";
    private const string AssetPath = "Assets/Project/Data/SkillTable.asset";

    // 아이콘 검색 우선 폴더 (없으면 프로젝트 전체 탐색)
    private const string IconFolder = "Assets/Project/UI/Icon";
    // VFX 검색 우선 폴더 (없으면 프로젝트 전체 탐색)
    private const string VfxFolder  = "Assets/Project/Prefabs/VFX";

    private static readonly string[] RequiredColumns = { "id", "name", "element", "coefficient", "range" };

    [MenuItem("Tools/Game/Import Skill CSV")]
    public static void Import()
    {
        if (!File.Exists(CsvPath))
        {
            Debug.LogError($"[SkillTableImporter] CSV를 찾을 수 없습니다: {CsvPath}");
            return;
        }

        string[] lines = File.ReadAllLines(CsvPath);
        if (lines.Length < 2)
        {
            Debug.LogError("[SkillTableImporter] 데이터 행이 없습니다.");
            return;
        }

        SkillTable table = AssetDatabase.LoadAssetAtPath<SkillTable>(AssetPath)
                           ?? CreateAsset();

        SerializedObject so       = new SerializedObject(table);
        SerializedProperty rowsProp = so.FindProperty("rows");
        if (rowsProp == null)
        {
            Debug.LogError("[SkillTableImporter] SkillTable에 'rows' 필드가 없습니다.");
            return;
        }
        rowsProp.ClearArray();

        string[] header = lines[0].Split(',').Select(x => x.Trim()).ToArray();
        int ColIdx(string n) => Array.FindIndex(header, h => h.Equals(n, StringComparison.OrdinalIgnoreCase));

        foreach (string col in RequiredColumns)
        {
            if (ColIdx(col) < 0)
            {
                Debug.LogError($"[SkillTableImporter] 필수 컬럼 '{col}'이 없습니다.");
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

            string[] cols = line.Split(',').Select(x => x.Trim()).ToArray();

            rowsProp.InsertArrayElementAtIndex(imported);
            SerializedProperty row = rowsProp.GetArrayElementAtIndex(imported);

            string id = GetCell(cols, idIdx);
            row.FindPropertyRelative("id").stringValue          = id;
            row.FindPropertyRelative("name").stringValue        = GetCell(cols, nameIdx);
            row.FindPropertyRelative("coefficient").floatValue  = Mathf.Max(0.1f, StrToFloat(GetCell(cols, coeffIdx), 1f));
            row.FindPropertyRelative("range").floatValue        = Mathf.Max(0f,   StrToFloat(GetCell(cols, rangeIdx), 9999f));

            if (cooldownIdx >= 0)
                row.FindPropertyRelative("cooldown").floatValue = Mathf.Max(0f, StrToFloat(GetCell(cols, cooldownIdx), 5f));

            // ElementType
            string elemRaw = GetCell(cols, elementIdx);
            if (!Enum.TryParse(elemRaw, true, out ElementType element))
            {
                Debug.LogWarning($"[SkillTableImporter] id='{id}' element='{elemRaw}' 파싱 실패 → Reason으로 대체");
                element = ElementType.Reason;
            }
            row.FindPropertyRelative("element").enumValueIndex = (int)element;

            // ── icon: 파일명으로 Sprite 탐색 ──────────────────────────────────
            if (iconIdx >= 0)
            {
                string iconName = GetCell(cols, iconIdx);
                Sprite sprite   = FindSprite(iconName, id);
                SerializedProperty iconProp = row.FindPropertyRelative("icon");
                if (sprite != null)
                    iconProp.objectReferenceValue = sprite;
                else
                    iconProp.objectReferenceValue = null;
            }

            // ── castVfxPrefab ──────────────────────────────────────────────────
            if (vfxIdx >= 0)
            {
                string vfxName   = GetCell(cols, vfxIdx);
                GameObject prefab = FindPrefab(vfxName, id);
                SerializedProperty vfxProp = row.FindPropertyRelative("castVfxPrefab");
                vfxProp.objectReferenceValue = prefab;
            }

            // ── description ────────────────────────────────────────────────
            if (descIdx >= 0)
                row.FindPropertyRelative("description").stringValue = GetCell(cols, descIdx);

            // ── effectType & 효과별 수치 ────────────────────────────────────
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

        Debug.Log($"[SkillTableImporter] {imported}개 스킬 임포트 완료 → {AssetPath}");
    }

    // ── 에셋 탐색 헬퍼 ───────────────────────────────────────────────────────

    /// <summary>
    /// 파일명(확장자 없음)으로 Sprite를 탐색합니다.
    /// 우선 IconFolder 안에서 찾고, 없으면 프로젝트 전체에서 탐색.
    /// </summary>
    private static Sprite FindSprite(string assetName, string rowId)
    {
        if (string.IsNullOrWhiteSpace(assetName)) return null;

        // 1. IconFolder 내 직접 경로 시도 (jpg / png)
        foreach (string ext in new[] { ".jpg", ".png", ".jpeg" })
        {
            string path = $"{IconFolder}/{assetName}{ext}";
            // Texture로 로드한 뒤 Sprite로 변환
            Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (s != null)
            {
                Debug.Log($"[SkillTableImporter] id='{rowId}' icon 연결: {path}");
                return s;
            }
        }

        // 2. 프로젝트 전체 탐색 (t:Sprite)
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
                    Debug.Log($"[SkillTableImporter] id='{rowId}' icon 연결 (전체탐색): {path}");
                    return s;
                }
            }
        }

        // 3. Texture2D로 찾아 Sprite 변환 시도
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
                    Debug.Log($"[SkillTableImporter] id='{rowId}' icon 연결 (Texture2D): {path}");
                    return s;
                }
            }
        }

        Debug.LogWarning(
            $"[SkillTableImporter] id='{rowId}' icon='{assetName}' 을 찾지 못했습니다.\n" +
            $"확인: {IconFolder}/{assetName}.jpg|png 이 존재하는지 확인하세요.\n" +
            $"또한 Texture Import Settings → Sprite Mode = Single 로 설정하세요.");
        return null;
    }

    /// <summary>
    /// 파일명(확장자 없음)으로 Prefab을 탐색합니다.
    /// 우선 VfxFolder 안에서 찾고, 없으면 프로젝트 전체에서 탐색.
    /// </summary>
    private static GameObject FindPrefab(string assetName, string rowId)
    {
        if (string.IsNullOrWhiteSpace(assetName)) return null;

        // 1. VfxFolder 직접 경로 시도
        string directPath = $"{VfxFolder}/{assetName}.prefab";
        GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(directPath);
        if (go != null)
        {
            Debug.Log($"[SkillTableImporter] id='{rowId}' VFX 연결: {directPath}");
            return go;
        }

        // 2. 프로젝트 전체 탐색 (t:Prefab)
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
                    Debug.Log($"[SkillTableImporter] id='{rowId}' VFX 연결 (전체탐색): {path}");
                    return go;
                }
            }
        }

        Debug.LogWarning(
            $"[SkillTableImporter] id='{rowId}' castVfxPrefab='{assetName}' 을 찾지 못했습니다.\n" +
            $"확인: 프로젝트 어딘가에 '{assetName}.prefab' 이 존재하는지 확인하세요.");
        return null;
    }

    // ── 유틸 ─────────────────────────────────────────────────────────────────

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
