using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProjectFirst.Data;
using ProjectFirst.OutGame.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectFirst.OutGame
{
    /// <summary>
    /// Documentation cleaned.
    /// </summary>
    public class CharacterManageManager : MonoBehaviour
    {
        // Note: cleaned comment.
        [SerializeField] private UIDocument _uiDocument;
        [SerializeField] private AgentTable _agentTable;
        [SerializeField] private PlayerData _playerData;
        [SerializeField] private Transform _modelSpawnPoint;
        [SerializeField] private CharacterLevelUpPanel _levelUpPanel;

        // Note: cleaned comment.
        private ElementType _filterElement = ElementType.All;
        private SortType _sortType = SortType.Power_Desc;

        // Note: cleaned comment.
        private AgentInfo _selectedAgent;
        private int _selectedAgentLevel;

        // Note: cleaned comment.
        private enum DetailTab { Info, LevelUp, Equipment, Collection }
        private DetailTab _currentTab;

        // Note: cleaned comment.
        private GameObject _currentModel;

        // Note: cleaned comment.
        private VisualElement _root;
        private ListView _agentListView;
        private VisualElement _rightPanel;
        private VisualElement _tabContentArea;

        private Button _filterAllBtn;
        private Button _filterPassionBtn;
        private Button _filterIntuitionBtn;
        private Button _filterReasonBtn;
        private DropdownField _sortDropdown;

        // Note: cleaned comment.
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

        // Note: cleaned comment.
        private Button _tabInfoBtn;
        private Button _tabLevelUpBtn;
        private Button _tabEquipmentBtn;
        private Button _tabCollectionBtn;

        // Note: cleaned comment.
        private VisualElement _infoTab;
        private VisualElement _levelUpTab;
        private VisualElement _equipmentTab;
        private VisualElement _collectionTab;

        // Note: cleaned comment.
        private List<AgentInfo> _filteredAgents = new();

        // Note: cleaned comment.
        private static readonly Dictionary<int, Color> GradeColors = new()
        {
            { 1, ColorUtility.TryParseHtmlString("#9CA3AF", out Color c1) ? c1 : Color.gray },
            { 2, ColorUtility.TryParseHtmlString("#22C55E", out Color c2) ? c2 : Color.green },
            { 3, ColorUtility.TryParseHtmlString("#3B82F6", out Color c3) ? c3 : Color.blue },
            { 4, ColorUtility.TryParseHtmlString("#A855F7", out Color c4) ? c4 : Color.magenta },
            { 5, ColorUtility.TryParseHtmlString("#F97316", out Color c5) ? c5 : Color.yellow }
        };

        // Note: cleaned comment.
        // Lifecycle
        // Note: cleaned comment.

        private void OnEnable()
        {
            BindUI();
            RefreshList();

            TutorialManager.Instance?.TryTrigger("first_character_manage");
        }

        private void OnDisable()
        {
            DestroyCurrentModel();
        }

        // Note: cleaned comment.
        // Note: cleaned comment.
        // Note: cleaned comment.

        private void BindUI()
        {
            _root = _uiDocument.rootVisualElement;

            // Note: cleaned comment.
            _filterAllBtn = _root.Q<Button>("filter-all-btn");
            _filterPassionBtn = _root.Q<Button>("filter-passion-btn");
            _filterIntuitionBtn = _root.Q<Button>("filter-intuition-btn");
            _filterReasonBtn = _root.Q<Button>("filter-reason-btn");

            _filterAllBtn?.RegisterCallback<ClickEvent>(_ => OnFilterChanged(ElementType.All));
            _filterPassionBtn?.RegisterCallback<ClickEvent>(_ => OnFilterChanged(ElementType.Passion));
            _filterIntuitionBtn?.RegisterCallback<ClickEvent>(_ => OnFilterChanged(ElementType.Intuition));
            _filterReasonBtn?.RegisterCallback<ClickEvent>(_ => OnFilterChanged(ElementType.Reason));

            // Note: cleaned comment.
            _sortDropdown = _root.Q<DropdownField>("sort-dropdown");
            if (_sortDropdown != null)
            {
                _sortDropdown.choices = new List<string> { "Power", "Grade" };
                _sortDropdown.index = 0;
                _sortDropdown.RegisterValueChangedCallback(OnSortChanged);
            }

            // Note: cleaned comment.
            _agentListView = _root.Q<ListView>("agent-list");
            if (_agentListView != null)
            {
                _agentListView.makeItem = MakeAgentCard;
                _agentListView.bindItem = BindAgentCard;
                _agentListView.selectionChanged += OnListSelectionChanged;
                _agentListView.selectionType = SelectionType.Single;
                _agentListView.fixedItemHeight = 120;
            }

            // Note: cleaned comment.
            _rightPanel = _root.Q<VisualElement>("right-panel");
            _tabContentArea = _root.Q<VisualElement>("tab-content-area");

            // Note: cleaned comment.
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

            // Note: cleaned comment.
            _tabInfoBtn = _root.Q<Button>("tab-info-btn");
            _tabLevelUpBtn = _root.Q<Button>("tab-levelup-btn");
            _tabEquipmentBtn = _root.Q<Button>("tab-equipment-btn");
            _tabCollectionBtn = _root.Q<Button>("tab-collection-btn");

            _tabInfoBtn?.RegisterCallback<ClickEvent>(_ => ShowDetailTab(DetailTab.Info));
            _tabLevelUpBtn?.RegisterCallback<ClickEvent>(_ => ShowDetailTab(DetailTab.LevelUp));
            _tabEquipmentBtn?.RegisterCallback<ClickEvent>(_ => ShowDetailTab(DetailTab.Equipment));
            _tabCollectionBtn?.RegisterCallback<ClickEvent>(_ => ShowDetailTab(DetailTab.Collection));

            // Note: cleaned comment.
            _infoTab = _root.Q<VisualElement>("info-tab");
            _levelUpTab = _root.Q<VisualElement>("levelup-tab");
            _equipmentTab = _root.Q<VisualElement>("equipment-tab");
            _collectionTab = _root.Q<VisualElement>("collection-tab");
        }

        // Note: cleaned comment.
        // Note: cleaned comment.
        // Note: cleaned comment.

        /// Documentation cleaned.
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

        // Note: cleaned comment.
        // Note: cleaned comment.
        // Note: cleaned comment.

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

        // Note: cleaned comment.
        // Note: cleaned comment.
        // Note: cleaned comment.

        private void OnListSelectionChanged(IEnumerable<object> selection)
        {
            AgentInfo agent = selection.FirstOrDefault() as AgentInfo;
            if (agent == null) return;

            OnAgentSelected(agent);
        }

        /// Documentation cleaned.
        private void OnAgentSelected(AgentInfo agent)
        {
            _selectedAgent = agent;
            _selectedAgentLevel = GetLevel(agent.id);
            ShowDetailTab(DetailTab.Info);
            Spawn3DModel(agent);
        }

        // Note: cleaned comment.
        // Note: cleaned comment.
        // Note: cleaned comment.

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
            else if (tab == DetailTab.LevelUp)
            {
                ShowLevelUpTab();
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

        /// Documentation cleaned.
        private void ShowLevelUpTab()
        {
            if (_selectedAgent == null || _levelUpPanel == null || _levelUpTab == null) return;

            _levelUpPanel.Setup(_selectedAgent, _selectedAgentLevel, _levelUpTab);
        }

        /// Documentation cleaned.
        private void ShowInfoTab()
        {
            if (_selectedAgent == null) return;

            int lv = _selectedAgentLevel;

            if (_nameLabel != null) _nameLabel.text = _selectedAgent.agentName;
            if (_subNameLabel != null) _subNameLabel.text = _selectedAgent.subName;
            if (_levelLabel != null) _levelLabel.text = $"Lv.{lv}";
            if (_powerLabel != null) _powerLabel.text = $"CP {_selectedAgent.GetPower(lv)}";

            // Note: cleaned comment.
            if (_gradeStarsRow != null)
            {
                _gradeStarsRow.Clear();
                for (int i = 0; i < _selectedAgent.grade; i++)
                {
                    var star = new Label("*");
                    star.AddToClassList("grade-star");
                    _gradeStarsRow.Add(star);
                }
            }

            // Note: cleaned comment.
            if (_hpLabel != null) _hpLabel.text = $"{_selectedAgent.GetHp(lv):F0}";
            if (_atkLabel != null) _atkLabel.text = $"{_selectedAgent.GetAtk(lv):F0}";
            if (_defLabel != null) _defLabel.text = $"{_selectedAgent.GetDef(lv):F0}";
            if (_critRateLabel != null) _critRateLabel.text = $"{_selectedAgent.critRate * 100f:F1}%";
            if (_critMultLabel != null) _critMultLabel.text = $"x{_selectedAgent.critMult:F2}";

            // Note: cleaned comment.
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

        // Note: cleaned comment.
        // Note: cleaned comment.
        // Note: cleaned comment.

        /// Documentation cleaned.
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

        // Note: cleaned comment.
        // Note: cleaned comment.
        // Note: cleaned comment.

        private int GetLevel(int agentId)
            => _playerData != null ? _playerData.GetCharacterLevel(agentId) : 1;

        private void SaveLevel(int agentId, int lv)
        {
            if (_playerData == null) return;
            _playerData.SetCharacterProgress(agentId, lv, _playerData.GetCharacterExp(agentId));
        }

        private bool CanLevelUp(int agentId)
        {
            int level = GetLevel(agentId);
            if (level >= 100 || _playerData == null) return false;

            return _playerData.GetTotalExpItemCount() > 0;
        }
    }
}

