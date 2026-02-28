#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// 프로젝트의 모든 Enemy 프리팹에 HitEffectTable을 한 번에 일괄 적용합니다.
/// 
/// 사용법:
/// 1. Assets/Project/Data/HitEffectTable.asset 생성 후 VFX 프리팹 연결
/// 2. Unity 메뉴 → Tools → Game → Apply HitEffectTable to All Enemies
/// </summary>
public static class HitEffectApplier
{
    private const string HitTablePath = "Assets/Project/Data/HitEffectTable.asset";

    [MenuItem("Tools/Game/Apply HitEffectTable to All Enemies")]
    public static void Apply()
    {
        // HitEffectTable 로드
        HitEffectTable table = AssetDatabase.LoadAssetAtPath<HitEffectTable>(HitTablePath);
        if (table == null)
        {
            Debug.LogError(
                $"[HitEffectApplier] HitEffectTable을 찾을 수 없습니다: {HitTablePath}\n" +
                "먼저 Create → Game → Hit Effect Table 로 에셋을 생성하고 VFX 프리팹을 연결하세요.");
            return;
        }

        // 모든 Enemy 프리팹 탐색
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Project/Prefabs/Enemy" });
        int applied = 0;
        int skipped = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            Enemy enemy = prefab.GetComponent<Enemy>();
            if (enemy == null) continue;

            SerializedObject so = new SerializedObject(enemy);
            SerializedProperty tableProp = so.FindProperty("hitEffectTable");

            if (tableProp == null)
            {
                Debug.LogWarning($"[HitEffectApplier] {path} → hitEffectTable 필드를 찾지 못했습니다.");
                skipped++;
                continue;
            }

            tableProp.objectReferenceValue = table;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(prefab);
            applied++;
            Debug.Log($"[HitEffectApplier] 적용: {path}");
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[HitEffectApplier] 완료. 적용: {applied}개, 스킵: {skipped}개");
    }

    [MenuItem("Tools/Game/Apply HitEffectTable to All Enemies", validate = true)]
    private static bool ApplyValidate()
    {
        return AssetDatabase.LoadAssetAtPath<HitEffectTable>(HitTablePath) != null;
    }
}
#endif
