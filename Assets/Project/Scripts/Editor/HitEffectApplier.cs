#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using ProjectFirst.Data;
/// <summary>
/// ?꾨줈?앺듃??紐⑤뱺 Enemy ?꾨━?뱀뿉 HitEffectTable????踰덉뿉 ?쇨큵 ?곸슜?⑸땲??
/// 
/// ?ъ슜踰?
/// 1. Assets/Project/Data/HitEffectTable.asset ?앹꽦 ??VFX ?꾨━???곌껐
/// 2. Unity 硫붾돱 ??Tools ??Game ??Apply HitEffectTable to All Enemies
/// </summary>
public static class HitEffectApplier
{
    private const string HitTablePath = "Assets/Project/Data/HitEffectTable.asset";

    [MenuItem("Tools/Game/Apply HitEffectTable to All Enemies")]
    public static void Apply()
    {
        // HitEffectTable 濡쒕뱶
        HitEffectTable table = AssetDatabase.LoadAssetAtPath<HitEffectTable>(HitTablePath);
        if (table == null)
        {
            Debug.LogError(
                $"[HitEffectApplier] HitEffectTable??李얠쓣 ???놁뒿?덈떎: {HitTablePath}\n" +
                "癒쇱? Create ??Game ??Hit Effect Table 濡??먯뀑???앹꽦?섍퀬 VFX ?꾨━?뱀쓣 ?곌껐?섏꽭??");
            return;
        }

        // 紐⑤뱺 Enemy ?꾨━???먯깋
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
                Debug.LogWarning($"[HitEffectApplier] {path} ??hitEffectTable ?꾨뱶瑜?李얠? 紐삵뻽?듬땲??");
                skipped++;
                continue;
            }

            tableProp.objectReferenceValue = table;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(prefab);
            applied++;
            Debug.Log($"[HitEffectApplier] ?곸슜: {path}");
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[HitEffectApplier] Completed. applied={applied}, skipped={skipped}");
    }

    [MenuItem("Tools/Game/Apply HitEffectTable to All Enemies", validate = true)]
    private static bool ApplyValidate()
    {
        return AssetDatabase.LoadAssetAtPath<HitEffectTable>(HitTablePath) != null;
    }
}
#endif



