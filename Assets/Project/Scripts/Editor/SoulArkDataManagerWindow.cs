#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
#endif
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using ProjectFirst.Data;

namespace ProjectFirst.Editor
{
    /// <summary>
    /// Soul Ark 데이터 에셋을 한 곳에서 불러오고 저장하는 관리 창입니다.
    /// Soul Ark 데이터 에셋을 한 곳에서 불러오고 저장하는 관리 창입니다.
    /// </summary>
#if ODIN_INSPECTOR
    public class SoulArkDataManagerWindow : OdinEditorWindow
#else
    public class SoulArkDataManagerWindow : EditorWindow
#endif
    {
#if ODIN_INSPECTOR
        [MenuItem("Soul Ark/Data Manager")]
        private static void OpenWindow()
        {
            var window = GetWindow<SoulArkDataManagerWindow>();
            window.titleContent = new GUIContent("Soul Ark Data Manager");
            window.minSize = new Vector2(1200, 700);
            window.Show();
        }
#else
        [MenuItem("Soul Ark/Data Manager")]
        private static void OpenWindow()
        {
            var window = GetWindow<SoulArkDataManagerWindow>();
            window.titleContent = new GUIContent("Soul Ark Data Manager");
            window.minSize = new Vector2(800, 600);
            window.Show();
        }
#endif

#if ODIN_INSPECTOR
        #region Tab Groups

        [EnumToggleButtons, HideLabel]
        [OnValueChanged("OnTabChanged")]
        [Title("데이터 관리", TitleAlignment = TitleAlignments.Centered)]
        public DataManagerTab CurrentTab = DataManagerTab.Agents;

        [ShowIf("@CurrentTab == DataManagerTab.Agents")]
        [BoxGroup("에이전트", centerLabel: true)]
        [HideReferenceObjectPicker]
        [InlineEditor(Expanded = true, DrawHeader = false)]
        [AssetsOnly]
        [LabelText("에이전트 테이블")]
        public AgentTable AgentTableAsset;

        [ShowIf("@CurrentTab == DataManagerTab.Monsters")]
        [BoxGroup("몬스터", centerLabel: true)]
        [HideReferenceObjectPicker]
        [InlineEditor(Expanded = true, DrawHeader = false)]
        [AssetsOnly]
        [LabelText("몬스터 테이블")]
        public MonsterTable MonsterTableAsset;

        [ShowIf("@CurrentTab == DataManagerTab.Skills")]
        [BoxGroup("스킬", centerLabel: true)]
        [HideReferenceObjectPicker]
        [InlineEditor(Expanded = true, DrawHeader = false)]
        [AssetsOnly]
        [LabelText("스킬 테이블")]
        public SkillTable SkillTableAsset;

        [ShowIf("@CurrentTab == DataManagerTab.Stages")]
        [BoxGroup("스테이지", centerLabel: true)]
        [HideReferenceObjectPicker]
        [InlineEditor(Expanded = true, DrawHeader = false)]
        [AssetsOnly]
        [LabelText("스테이지 테이블")]
        public StageTable StageTableAsset;

        #endregion

        #region Actions

        [BoxGroup("작업", centerLabel: true)]
        [HorizontalGroup("작업/Buttons")]
        [Button("모든 테이블 로드", ButtonSizes.Large)]
        [GUIColor(0.2f, 0.6f, 1f)]
        private void LoadAllTables()
        {
            AgentTableAsset = AssetDatabase.LoadAssetAtPath<AgentTable>("Assets/Project/Data/AgentTable.asset");
            MonsterTableAsset = AssetDatabase.LoadAssetAtPath<MonsterTable>("Assets/Project/Data/MonsterTable.asset");
            SkillTableAsset = AssetDatabase.LoadAssetAtPath<SkillTable>("Assets/Project/Data/SkillTable.asset");
            StageTableAsset = AssetDatabase.LoadAssetAtPath<StageTable>("Assets/Project/Data/StageTable.asset");
        }

        [HorizontalGroup("작업/Buttons")]
        [Button("인덱스 재구축", ButtonSizes.Large)]
        [GUIColor(0.3f, 0.8f, 0.3f)]
        [EnableIf("@AgentTableAsset != null || MonsterTableAsset != null || SkillTableAsset != null")]
        private void RebuildAllIndices()
        {
            if (AgentTableAsset != null)
            {
                var method = typeof(AgentTable).GetMethod("RebuildIndex",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                method?.Invoke(AgentTableAsset, null);
            }
            if (MonsterTableAsset != null)
            {
                var method = typeof(MonsterTable).GetMethod("OnValidate",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                method?.Invoke(MonsterTableAsset, null);
            }
            if (SkillTableAsset != null)
            {
                var method = typeof(SkillTable).GetMethod("RebuildIndex",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                method?.Invoke(SkillTableAsset, null);
            }
            EditorUtility.DisplayDialog("알림", "인덱스 재구축이 완료되었습니다.", "확인");
        }

        [HorizontalGroup("작업/Buttons")]
        [Button("변경사항 저장", ButtonSizes.Large)]
        [GUIColor(1f, 0.6f, 0.2f)]
        [EnableIf("@AgentTableAsset != null || MonsterTableAsset != null || SkillTableAsset != null")]
        private void SaveAllChanges()
        {
            if (AgentTableAsset != null) EditorUtility.SetDirty(AgentTableAsset);
            if (MonsterTableAsset != null) EditorUtility.SetDirty(MonsterTableAsset);
            if (SkillTableAsset != null) EditorUtility.SetDirty(SkillTableAsset);
            if (StageTableAsset != null) EditorUtility.SetDirty(StageTableAsset);
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("알림", "변경사항 저장이 완료되었습니다.", "확인");
        }

        #endregion

        #region Balance Check

        [ShowIf("@CurrentTab == DataManagerTab.Balance")]
        [BoxGroup("밸런스 분석", centerLabel: true)]
        [ShowInInspector, ReadOnly]
        [TableList(AlwaysExpanded = true)]
        [LabelText("스탯 분포")]
        private List<BalanceInfo> BalanceInfos => CalculateBalance();

        private List<BalanceInfo> CalculateBalance()
        {
            var infos = new List<BalanceInfo>();
            if (AgentTableAsset?.rows != null)
            {
                var avgHp = AgentTableAsset.rows.Average(r => r.stats.hp);
                var avgAtk = AgentTableAsset.rows.Average(r => r.stats.atk);
                var avgDef = AgentTableAsset.rows.Average(r => r.stats.def);
                infos.Add(new BalanceInfo { Category = "에이전트", Type = "평균 HP", Value = avgHp });
                infos.Add(new BalanceInfo { Category = "에이전트", Type = "평균 ATK", Value = avgAtk });
                infos.Add(new BalanceInfo { Category = "에이전트", Type = "평균 DEF", Value = avgDef });
            }
            return infos;
        }

        #endregion

        #region Import & Sync

        [BoxGroup("임포트 & 동기화", centerLabel: true)]
        [HorizontalGroup("임포트 & 동기화/Buttons")]
        [Button("CSV → 테이블 임포트", ButtonSizes.Large)]
        [GUIColor(0.4f, 0.8f, 1f)]
        private void ImportAllTables()
        {
            foreach (var entry in GameTableImportRegistry.GetEntries())
            {
                entry.ImportAction?.Invoke();
            }
            AssetDatabase.Refresh();
            LoadAllTables();
        }

        [HorizontalGroup("임포트 & 동기화/Buttons")]
        [Button("테이블 → Data 동기화", ButtonSizes.Large)]
        [GUIColor(0.4f, 1f, 0.6f)]
        private void SyncAllDataAssets()
        {
            StageDataSyncUtility.SyncStageDataFromTable();
            ChapterDataSyncUtility.SyncChapterDataFromTable();
            AssetDatabase.Refresh();
        }

        [HorizontalGroup("임포트 & 동기화/Buttons")]
        [Button("CSV → Data 풀 파이프라인", ButtonSizes.Large)]
        [GUIColor(0.2f, 0.7f, 0.9f)]
        private void RunFullPipeline()
        {
            ImportAllTables();
            SyncAllDataAssets();
            EditorUtility.DisplayDialog("알림", "CSV → 테이블 → Data 동기화가 완료되었습니다.", "확인");
        }

        #endregion

        private void OnTabChanged()
        {
            // 탭 변경 시 필요하면 추가 동작을 연결
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            LoadAllTables();
        }

#else
        private void OnGUI()
        {
            EditorGUILayout.LabelField("Odin Inspector가 필요합니다.", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "이 툴은 Odin Inspector가 설치된 경우에만 정상 작동합니다.\n" +
                "Odin Inspector를 임포트하거나 ODIN_INSPECTOR 심볼을 정의해주세요.",
                MessageType.Info);
        }
#endif
    }

    public enum DataManagerTab
    {
        Agents,
        Monsters,
        Skills,
        Stages,
        Balance
    }

#if ODIN_INSPECTOR
    public class BalanceInfo
    {
        [TableColumnWidth(100)]
        [VerticalGroup("Category")]
        public string Category;

        [TableColumnWidth(100)]
        [VerticalGroup("Type")]
        public string Type;

        [TableColumnWidth(80)]
        [VerticalGroup("Value")]
        [ProgressBar(0, 1000, ColorGetter = "GetColor")]
        public float Value;

        private static Color GetColor() => new Color(0.3f, 0.7f, 1f);
    }
#endif
}


