using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProjectFirst.Data;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectFirst.OutGame
{
    /// <summary>
    /// 캐릭터 관리 화면의 리스트·상세·3D 모델 스폰을 총괄하는 매니저.
    /// </summary>
    public class CharacterManageManager : MonoBehaviour
    {
        // ── Inspector ───────────────────────────────────────────
        [SerializeField] private UIDocument _uiDocument;
        [SerializeField] private AgentTable _agentTable;
        [SerializeField] private PlayerData _playerData;
        [SerializeField] private Transform _modelSpawnPoint;

        // ── 필터 / 정렬 상태 ────────────────────────────────────
        private ElementType _filterElement = ElementType.All;
        private SortType _sortType = SortType.Power_Desc;

        // ── 현재 선택 캐릭터 ────────────────────────────────────
        private AgentInfo _selectedAgent;
        private int _selectedAgentLevel;

        // ── 탭 ──────────────────────────────────────────────────
        private enum DetailTab { Info, LevelUp, Equipment, Collection }
        private DetailTab _currentTab;

        // ── 3D 모델 ─────────────────────────────────────────────
        private GameObject _currentModel;

        // ── UI 요소 캐시 ────────────────────────────────────────
        private VisualElement _root;
        private ListView _agentListView;
        private VisualElement _rightPanel;
        private VisualElement _tabContentArea;

        private Button _filterAllBtn;
        private Button _filterPassionBtn;
        private Button _filterIntuitionBtn;
        private Button _filterReasonBtn;
        private DropdownField _sortDropdown;

        // 상세 패널 요소
        private Label _nameLabel;
        private Label _subNameLabel;
        private VisualElement _gradeStarsRow;
        private Label _levelLabel;
        private Label _powerLabel;
        private Label _hpLabel;
        private Label _atkLabel;
        private Label _defLabel;
        private Label _critRateLabel;
        private Label _critMultLabel;
        private VisualElement _skillRow;

        // 탭 버튼
        private Button _tabInfoBtn;
        private Button _tabLevelUpBtn;
        private Button _tabEquipmentBtn;
        private Button _tabCollectionBtn;

        // 탭 컨텐츠
        private VisualElement _infoTab;
        private VisualElement _levelUpTab;
        private VisualElement _equipmentTab;
        private VisualElement _collectionTab;

        // ── 리스트 데이터 ───────────────────────────────────────
        private List<AgentInfo> _filteredAgents = new();

        // ── 등급 테두리 색상 ────────────────────────────────────
        private static readonly Dictionary<int, Color> GradeColors = new()
        {
            { 1, ColorUtility.TryParseHtmlString("#9CA3AF", out Color c1) ? c1 : Color.gray },
            { 2, ColorUtility.TryParseHtmlString("#22C55E", out Color c2) ? c2 : Color.green },
            { 3, ColorUtility.TryParseHtmlString("#3B82F6", out Color c3) ? c3 : Color.blue },
            { 4, ColorUtility.TryParseHtmlString("#A855F7", out Color c4) ? c4 : Color.magenta },
            { 5, ColorUtility.TryParseHtmlString("#F97316", out Color c5) ? c5 : Color.yellow }
        };

        // ─────────────────────────────────────────────────────────
        // Lifecycle
        // ─────────────────────────────────────────────────────────

        private void OnEnable()
        {
            BindUI();
            RefreshList();
        }

        private void OnDisable()
        {
            DestroyCurrentModel();
        }

        // ─────────────────────────────────────────────────────────
        // UI 바인딩
        // ─────────────────────────────────────────────────────────

        private void BindUI()
        {
            _root = _uiDocument.rootVisualElement;

            // 필터 버튼
            _filterAllBtn = _root.Q<Button>("filter-all-btn");
            _filterPassionBtn = _root.Q<Button>("filter-passion-btn");
            _filterIntuitionBtn = _root.Q<Button>("filter-intuition-btn");
            _filterReasonBtn = _root.Q<Button>("filter-reason-btn");

            _filterAllBtn?.RegisterCallback<ClickEvent>(_ => OnFilterChanged(ElementType.All));
            _filterPassionBtn?.RegisterCallback<ClickEvent>(_ => OnFilterChanged(ElementType.Passion));
            _filterIntuitionBtn?.RegisterCallback<ClickEvent>(_ => OnFilterChanged(ElementType.Intuition));
            _filterReasonBtn?.RegisterCallback<ClickEvent>(_ => OnFilterChanged(ElementType.Reason));

            // 정렬 드롭다운
            _sortDropdown = _root.Q<DropdownField>("sort-dropdown");
            if (_sortDropdown != null)
            {
                _sortDropdown.choices = new List<string> { "전투력▼", "등급▼" };
                _sortDropdown.index = 0;
                _sortDropdown.RegisterValueChangedCallback(OnSortChanged);
            }

            // 리스트뷰
            _agentListView = _root.Q<ListView>("agent-list");
            if (_agentListView != null)
            {
                _agentListView.makeItem = MakeAgentCard;
                _agentListView.bindItem = BindAgentCard;
                _agentListView.selectionChanged += OnListSelectionChanged;
                _agentListView.selectionType = SelectionType.Single;
                _agentListView.fixedItemHeight = 120;
            }

            // 우측 패널
            _rightPanel = _root.Q<VisualElement>("right-panel");
            _tabContentArea = _root.Q<VisualElement>("tab-content-area");

            // 상세 정보 레이블
            _nameLabel = _root.Q<Label>("agent-detail-name");
            _subNameLabel = _root.Q<Label>("agent-detail-subname");
            _gradeStarsRow = _root.Q<VisualElement>("grade-stars-row");
            _levelLabel = _root.Q<Label>("level-label");
            _powerLabel = _root.Q<Label>("power-detail-label");
            _hpLabel = _root.Q<Label>("stat-hp");
            _atkLabel = _root.Q<Label>("stat-atk");
            _defLabel = _root.Q<Label>("stat-def");
            _critRateLabel = _root.Q<Label>("stat-crit-rate");
            _critMultLabel = _root.Q<Label>("stat-crit-mult");
            _skillRow = _root.Q<VisualElement>("skill-row");

            // 탭 버튼
            _tabInfoBtn = _root.Q<Button>("tab-info-btn");
            _tabLevelUpBtn = _root.Q<Button>("tab-levelup-btn");
            _tabEquipmentBtn = _root.Q<Button>("tab-equipment-btn");
            _tabCollectionBtn = _root.Q<Button>("tab-collection-btn");

            _tabInfoBtn?.RegisterCallback<ClickEvent>(_ => ShowDetailTab(DetailTab.Info));
            _tabLevelUpBtn?.RegisterCallback<ClickEvent>(_ => ShowDetailTab(DetailTab.LevelUp));
            _tabEquipmentBtn?.RegisterCallback<ClickEvent>(_ => ShowDetailTab(DetailTab.Equipment));
            _tabCollectionBtn?.RegisterCallback<ClickEvent>(_ => ShowDetailTab(DetailTab.Collection));

            // 탭 컨텐츠
            _infoTab = _root.Q<VisualElement>("info-tab");
            _levelUpTab = _root.Q<VisualElement>("levelup-tab");
            _equipmentTab = _root.Q<VisualElement>("equipment-tab");
            _collectionTab = _root.Q<VisualElement>("collection-tab");
        }

        // ─────────────────────────────────────────────────────────
        // 리스트 영역
        // ─────────────────────────────────────────────────────────

        /// <summary>필터·정렬을 적용하여 캐릭터 목록을 갱신합니다.</summary>
        private void RefreshList()
        {
            IEnumerable<AgentInfo> agents = _agentTable.GetAll()
                .Where(a => a != null && _playerData.ownedCharacterIds.Contains(a.id));

            if (_filterElement != ElementType.All)
            {
                agents = agents.Where(a => a.element == _filterElement);
            }

            _filteredAgents = _sortType switch
            {
                SortType.Power_Desc => agents.OrderByDescending(a => a.GetPower(GetLevel(a.id))).ToList(),
                SortType.Grade_Desc => agents.OrderByDescending(a => a.grade).ToList(),
                _ => agents.ToList()
            };

            if (_agentListView != null)
            {
                _agentListView.itemsSource = _filteredAgents;
                _agentListView.Rebuild();
            }
        }

        private VisualElement MakeAgentCard()
        {
            var card = new VisualElement();
            card.AddToClassList("agent-card");

            var gradeBorder = new VisualElement { name = "grade-border" };
            gradeBorder.AddToClassList("grade-border");

            var thumbnail = new VisualElement { name = "thumbnail-img" };
            thumbnail.AddToClassList("thumbnail-img");

            var nameLabel = new Label { name = "agent-name-label" };
            nameLabel.AddToClassList("agent-name-label");

            var powerLabel = new Label { name = "power-label" };
            powerLabel.AddToClassList("power-label");

            var redDot = new VisualElement { name = "levelup-reddot" };
            redDot.AddToClassList("levelup-reddot");

            gradeBorder.Add(thumbnail);
            card.Add(gradeBorder);
            card.Add(nameLabel);
            card.Add(powerLabel);
            card.Add(redDot);

            return card;
        }

        private void BindAgentCard(VisualElement element, int index)
        {
            if (index < 0 || index >= _filteredAgents.Count) return;

            AgentInfo agent = _filteredAgents[index];
            int level = GetLevel(agent.id);

            var gradeBorder = element.Q<VisualElement>("grade-border");
            if (gradeBorder != null && GradeColors.TryGetValue(agent.grade, out Color borderColor))
            {
                gradeBorder.style.borderTopColor = borderColor;
                gradeBorder.style.borderBottomColor = borderColor;
                gradeBorder.style.borderLeftColor = borderColor;
                gradeBorder.style.borderRightColor = borderColor;
            }

            var thumbnail = element.Q<VisualElement>("thumbnail-img");
            if (thumbnail != null && agent.thumbnail != null)
            {
                thumbnail.style.backgroundImage = new StyleBackground(agent.thumbnail);
            }

            var nameLabel = element.Q<Label>("agent-name-label");
            if (nameLabel != null)
            {
                nameLabel.text = agent.agentName;
            }

            var powerLabel = element.Q<Label>("power-label");
            if (powerLabel != null)
            {
                powerLabel.text = $"CP {agent.GetPower(level)}";
            }

            var redDot = element.Q<VisualElement>("levelup-reddot");
            if (redDot != null)
            {
                redDot.style.display = CanLevelUp(agent.id)
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;
            }
        }

        // ─────────────────────────────────────────────────────────
        // 필터 / 정렬
        // ─────────────────────────────────────────────────────────

        private void OnFilterChanged(ElementType element)
        {
            _filterElement = element;
            UpdateFilterButtonStates();
            RefreshList();
        }

        private void UpdateFilterButtonStates()
        {
            SetFilterActive(_filterAllBtn, _filterElement == ElementType.All);
            SetFilterActive(_filterPassionBtn, _filterElement == ElementType.Passion);
            SetFilterActive(_filterIntuitionBtn, _filterElement == ElementType.Intuition);
            SetFilterActive(_filterReasonBtn, _filterElement == ElementType.Reason);
        }

        private static void SetFilterActive(Button btn, bool active)
        {
            if (btn == null) return;
            btn.EnableInClassList("filter-active", active);
        }

        private void OnSortChanged(ChangeEvent<string> evt)
        {
            _sortType = _sortDropdown.index switch
            {
                0 => SortType.Power_Desc,
                1 => SortType.Grade_Desc,
                _ => SortType.Power_Desc
            };
            RefreshList();
        }

        // ─────────────────────────────────────────────────────────
        // 리스트 선택
        // ─────────────────────────────────────────────────────────

        private void OnListSelectionChanged(IEnumerable<object> selection)
        {
            AgentInfo agent = selection.FirstOrDefault() as AgentInfo;
            if (agent == null) return;

            OnAgentSelected(agent);
        }

        /// <summary>에이전트 선택 시 상세 패널을 갱신하고 3D 모델을 스폰합니다.</summary>
        private void OnAgentSelected(AgentInfo agent)
        {
            _selectedAgent = agent;
            _selectedAgentLevel = GetLevel(agent.id);
            ShowDetailTab(DetailTab.Info);
            Spawn3DModel(agent);
        }

        // ─────────────────────────────────────────────────────────
        // 상세 탭
        // ─────────────────────────────────────────────────────────

        private void ShowDetailTab(DetailTab tab)
        {
            _currentTab = tab;

            SetTabVisible(_infoTab, tab == DetailTab.Info);
            SetTabVisible(_levelUpTab, tab == DetailTab.LevelUp);
            SetTabVisible(_equipmentTab, tab == DetailTab.Equipment);
            SetTabVisible(_collectionTab, tab == DetailTab.Collection);

            SetTabButtonActive(_tabInfoBtn, tab == DetailTab.Info);
            SetTabButtonActive(_tabLevelUpBtn, tab == DetailTab.LevelUp);
            SetTabButtonActive(_tabEquipmentBtn, tab == DetailTab.Equipment);
            SetTabButtonActive(_tabCollectionBtn, tab == DetailTab.Collection);

            if (tab == DetailTab.Info)
            {
                ShowInfoTab();
            }
        }

        private static void SetTabVisible(VisualElement tab, bool visible)
        {
            if (tab == null) return;
            tab.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private static void SetTabButtonActive(Button btn, bool active)
        {
            if (btn == null) return;
            btn.EnableInClassList("tab-active", active);
        }

        /// <summary>정보 탭 내용을 현재 선택된 캐릭터 기준으로 표시합니다.</summary>
        private void ShowInfoTab()
        {
            if (_selectedAgent == null) return;

            int lv = _selectedAgentLevel;

            if (_nameLabel != null) _nameLabel.text = _selectedAgent.agentName;
            if (_subNameLabel != null) _subNameLabel.text = _selectedAgent.subName;
            if (_levelLabel != null) _levelLabel.text = $"Lv.{lv}";
            if (_powerLabel != null) _powerLabel.text = $"CP {_selectedAgent.GetPower(lv)}";

            // 등급 별 표시
            if (_gradeStarsRow != null)
            {
                _gradeStarsRow.Clear();
                for (int i = 0; i < _selectedAgent.grade; i++)
                {
                    var star = new Label("★");
                    star.AddToClassList("grade-star");
                    _gradeStarsRow.Add(star);
                }
            }

            // 스탯 수치
            if (_hpLabel != null) _hpLabel.text = $"{_selectedAgent.GetHp(lv):F0}";
            if (_atkLabel != null) _atkLabel.text = $"{_selectedAgent.GetAtk(lv):F0}";
            if (_defLabel != null) _defLabel.text = $"{_selectedAgent.GetDef(lv):F0}";
            if (_critRateLabel != null) _critRateLabel.text = $"{_selectedAgent.critRate * 100f:F1}%";
            if (_critMultLabel != null) _critMultLabel.text = $"x{_selectedAgent.critMult:F2}";

            // 스킬 표시
            BindSkillRow();
        }

        private void BindSkillRow()
        {
            if (_skillRow == null || _selectedAgent.skills == null) return;

            _skillRow.Clear();
            for (int i = 0; i < _selectedAgent.skills.Length; i++)
            {
                SkillRow skill = _selectedAgent.skills[i];
                if (skill == null) continue;

                var skillCard = new VisualElement();
                skillCard.AddToClassList("skill-card");

                var iconEl = new VisualElement();
                iconEl.AddToClassList("skill-icon");
                if (skill.icon != null)
                {
                    iconEl.style.backgroundImage = new StyleBackground(skill.icon);
                }

                var skillName = new Label(skill.name);
                skillName.AddToClassList("skill-name");

                var skillDesc = new Label(skill.description);
                skillDesc.AddToClassList("skill-desc");

                skillCard.Add(iconEl);
                skillCard.Add(skillName);
                skillCard.Add(skillDesc);

                _skillRow.Add(skillCard);
            }
        }

        // ─────────────────────────────────────────────────────────
        // 3D 모델
        // ─────────────────────────────────────────────────────────

        /// <summary>기존 모델을 제거하고 새 캐릭터 모델을 스폰합니다.</summary>
        private void Spawn3DModel(AgentInfo agent)
        {
            DestroyCurrentModel();

            if (agent.modelPrefab == null || _modelSpawnPoint == null) return;

            _currentModel = Instantiate(agent.modelPrefab, _modelSpawnPoint.position,
                _modelSpawnPoint.rotation, _modelSpawnPoint);

            Animator animator = _currentModel.GetComponent<Animator>();
            if (animator != null)
            {
                animator.Play("Idle");
            }
        }

        private void DestroyCurrentModel()
        {
            if (_currentModel != null)
            {
                Destroy(_currentModel);
                _currentModel = null;
            }
        }

        // ─────────────────────────────────────────────────────────
        // 레벨 유틸
        // ─────────────────────────────────────────────────────────

        private int GetLevel(int agentId)
            => PlayerPrefs.GetInt($"agent_lv_{agentId}", 1);

        private void SaveLevel(int agentId, int lv)
            => PlayerPrefs.SetInt($"agent_lv_{agentId}", lv);

        private bool CanLevelUp(int agentId)
        {
            // TODO: P4-S2에서 레벨업 조건 구현
            return false;
        }
    }
}
