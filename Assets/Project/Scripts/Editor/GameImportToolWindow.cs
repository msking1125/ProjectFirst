#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GameImportToolWindow : EditorWindow
{
    private readonly List<ImportItem> items = new();
    private Vector2 scroll;

    [MenuItem("Tools/Game/Import")]
    public static void Open()
    {
        GameImportToolWindow window = GetWindow<GameImportToolWindow>("Game Import Tool");
        window.minSize = new Vector2(420f, 340f);
        window.Show();
    }

    private void OnEnable()
    {
        BuildItems();
    }

    private void BuildItems()
    {
        items.Clear();

        var entries = GameTableImportRegistry.GetEntries();
        for (int i = 0; i < entries.Count; i++)
            items.Add(new ImportItem(entries[i].Label, entries[i].ImportAction));
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Import Tables", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("?꾪룷?명븷 ?뚯씠釉붿쓣 ?щ윭 媛??좏깮???? 'Import Selected'瑜??뚮윭 ?쇨큵 ?꾪룷?명븯?몄슂.", MessageType.Info);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Select All", GUILayout.Height(24f)))
                SetAll(true);

            if (GUILayout.Button("Clear", GUILayout.Height(24f)))
                SetAll(false);
        }

        EditorGUILayout.Space(6f);

        using (var sv = new EditorGUILayout.ScrollViewScope(scroll))
        {
            scroll = sv.scrollPosition;

            for (int i = 0; i < items.Count; i++)
            {
                ImportItem item = items[i];
                item.Selected = EditorGUILayout.ToggleLeft(item.Label, item.Selected);
            }
        }

        GUILayout.FlexibleSpace();
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Import Selected", GUILayout.Height(30f)))
                ImportSelected();

            if (GUILayout.Button("Close", GUILayout.Height(30f)))
                Close();
        }
    }

    private void SetAll(bool value)
    {
        for (int i = 0; i < items.Count; i++)
            items[i].Selected = value;
    }

    private void ImportSelected()
    {
        List<ImportItem> selected = items.FindAll(i => i.Selected);
        if (selected.Count == 0)
        {
            EditorUtility.DisplayDialog("Game Import Tool", "?좏깮???뚯씠釉붿씠 ?놁뒿?덈떎.", "?뺤씤");
            return;
        }

        if (!EditorUtility.DisplayDialog("Game Import Tool", $"?좏깮??{selected.Count}媛??뚯씠釉붿쓣 ?꾪룷?명븯?쒓쿋?듬땲源?", "Import", "Cancel"))
            return;

        try
        {
            for (int i = 0; i < selected.Count; i++)
            {
                ImportItem item = selected[i];
                EditorUtility.DisplayProgressBar("Importing Tables", item.Label, (i + 1f) / selected.Count);
                item.ImportAction?.Invoke();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[GameImportToolWindow] Import 以??ㅻ쪟 諛쒖깮: {ex}");
            EditorUtility.DisplayDialog("Game Import Tool", "Import 以??ㅻ쪟媛 諛쒖깮?덉뒿?덈떎. Console???뺤씤?섏꽭??", "?뺤씤");
            throw;
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Game Import Tool", "?좏깮???뚯씠釉??꾪룷?멸? ?꾨즺?섏뿀?듬땲??", "?뺤씤");
    }

    private sealed class ImportItem
    {
        public string Label { get; }
        public Action ImportAction { get; }
        public bool Selected { get; set; }

        public ImportItem(string label, Action importAction)
        {
            Label = label;
            ImportAction = importAction;
            Selected = true;
        }
    }
}
#endif
