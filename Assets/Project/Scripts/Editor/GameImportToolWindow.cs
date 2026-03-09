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
        items.Add(new ImportItem("Agent Table", AgentTableImporter.Import));
        items.Add(new ImportItem("Monster Table", MonsterTableImporter.Import));
        items.Add(new ImportItem("Skill Table", SkillTableImporter.Import));
        items.Add(new ImportItem("Wave Table", WaveTableImporter.Import));
        items.Add(new ImportItem("Dialogue Table", DialogueTableImporter.Import));
        items.Add(new ImportItem("Badword Table", BadwordTableImporter.Import));
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Import Tables", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("임포트할 테이블을 여러 개 선택한 뒤, 'Import Selected'를 눌러 일괄 임포트하세요.", MessageType.Info);

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
            EditorUtility.DisplayDialog("Game Import Tool", "선택된 테이블이 없습니다.", "확인");
            return;
        }

        if (!EditorUtility.DisplayDialog("Game Import Tool", $"선택된 {selected.Count}개 테이블을 임포트하시겠습니까?", "Import", "Cancel"))
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
            Debug.LogError($"[GameImportToolWindow] Import 중 오류 발생: {ex}");
            EditorUtility.DisplayDialog("Game Import Tool", "Import 중 오류가 발생했습니다. Console을 확인하세요.", "확인");
            throw;
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Game Import Tool", "선택된 테이블 임포트가 완료되었습니다.", "확인");
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
