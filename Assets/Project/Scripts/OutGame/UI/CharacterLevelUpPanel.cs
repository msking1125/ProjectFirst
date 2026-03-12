using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProjectFirst.Data;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectFirst.OutGame.UI
{
    public class CharacterLevelUpPanel : MonoBehaviour
    {
        private const int MaxLevel = 100;
        [SerializeField] private PlayerData _playerData;
        [SerializeField] private VoidEventChannelSO _onCharacterChanged;
        [SerializeField] private CharacterGrowthCatalogSO _growthCatalog;
        private AgentInfo _agent;
        private int _currentLevel;
        private int _currentExp;
        private List<ExpItem> _expItems = new();
        private Dictionary<ExpItemType, int> _selectedCounts = new();
        private VisualElement _root;
        private Label _currentLevelLabel;
        private Label _expBarLabel;
        private VisualElement _expBarFill;
        private VisualElement _expItemListContainer;
        private Label _previewLevelLabel;
        private VisualElement _statComparePanel;
        private Button _levelUpBtn;
        private Label _hpBeforeLabel;
        private Label _hpAfterLabel;
        private Label _hpDiffLabel;
        private Label _atkBeforeLabel;
        private Label _atkAfterLabel;
        private Label _atkDiffLabel;
        private Label _defBeforeLabel;
        private Label _defAfterLabel;
        private Label _defDiffLabel;
        [SerializeField] private CanvasGroup _effectCanvasGroup;
        // Public API
        public void Setup(AgentInfo agent, int level, VisualElement levelUpTabRoot)
        {
            _agent = agent;
            _currentLevel = level;
            _currentExp = _playerData != null ? _playerData.GetCharacterExp(agent.id) : 0;
            _selectedCounts.Clear();

            BindUI(levelUpTabRoot);
            LoadExpItems();
            RefreshUI();
        }

        private void BindUI(VisualElement root)
        {
            _root = root;

            _currentLevelLabel = root.Q<Label>("current-level-label");
            _expBarLabel = root.Q<Label>("exp-bar-label");
            _expBarFill = root.Q<VisualElement>("exp-bar-fill");
            _expItemListContainer = root.Q<VisualElement>("exp-item-list");
            _previewLevelLabel = root.Q<Label>("preview-level-label");
            _statComparePanel = root.Q<VisualElement>("stat-compare-grid");
            _levelUpBtn = root.Q<Button>("levelup-confirm-btn");
            _hpBeforeLabel = root.Q<Label>("hp-before");
            _hpAfterLabel = root.Q<Label>("hp-after");
            _hpDiffLabel = root.Q<Label>("hp-diff");
            _atkBeforeLabel = root.Q<Label>("atk-before");
            _atkAfterLabel = root.Q<Label>("atk-after");
            _atkDiffLabel = root.Q<Label>("atk-diff");
            _defBeforeLabel = root.Q<Label>("def-before");
            _defAfterLabel = root.Q<Label>("def-after");
            _defDiffLabel = root.Q<Label>("def-diff");

            if (_levelUpBtn != null)
            {
                _levelUpBtn.RegisterCallback<ClickEvent>(_ => OnLevelUpConfirmed());
                _levelUpBtn.SetEnabled(false);
            }
        }

        private void LoadExpItems()
        {
            _expItems.Clear();

            if (_growthCatalog != null && _growthCatalog.expItems != null && _growthCatalog.expItems.Count > 0)
            {
                foreach (ExpItemDefinition def in _growthCatalog.expItems)
                {
                    int count = _playerData != null ? _playerData.GetExpItemCount(def.type) : 0;
                    _expItems.Add(new ExpItem(def.type, string.IsNullOrWhiteSpace(def.itemName) ? def.type.ToString() : def.itemName, def.icon, count));
                }
                return;
            }

            _expItems.Add(CreateExpItem(ExpItemType.Small, "Small XP Note"));
            _expItems.Add(CreateExpItem(ExpItemType.Medium, "Medium XP Note"));
            _expItems.Add(CreateExpItem(ExpItemType.Large, "Large XP Note"));
            _expItems.Add(CreateExpItem(ExpItemType.Crystal, "XP Crystal"));
        }

        private ExpItem CreateExpItem(ExpItemType type, string fallbackName)
        {
            int count = _playerData != null ? _playerData.GetExpItemCount(type) : 0;
            return new ExpItem(type, fallbackName, null, count);
        }

        private void RefreshUI()
        {
            if (_agent == null) return;
            if (_currentLevelLabel != null)
            {
                _currentLevelLabel.text = $"Lv.{_currentLevel}";
            }
            int requiredExp = ExpForLevel(_currentLevel);
            if (_expBarLabel != null)
            {
                _expBarLabel.text = $"{_currentExp} / {requiredExp}";
            }

            if (_expBarFill != null)
            {
                float ratio = requiredExp > 0 ? Mathf.Clamp01((float)_currentExp / requiredExp) : 0f;
                _expBarFill.style.width = Length.Percent(ratio * 100f);
            }
            BuildExpItemList();
            UpdatePreview(0);
        }

        private void BuildExpItemList()
        {
            if (_expItemListContainer == null) return;

            _expItemListContainer.Clear();

            for (int i = 0; i < _expItems.Count; i++)
            {
                ExpItem item = _expItems[i];
                _expItemListContainer.Add(CreateExpItemRow(item));
            }
        }

        private VisualElement CreateExpItemRow(ExpItem item)
        {
            var row = new VisualElement();
            row.AddToClassList("exp-item-row");
            var iconEl = new VisualElement();
            iconEl.AddToClassList("exp-item-icon");
            if (item.icon != null)
            {
                iconEl.style.backgroundImage = new StyleBackground(item.icon);
            }
            var nameLabel = new Label(item.itemName);
            nameLabel.AddToClassList("exp-item-name");
            var ownedLabel = new Label($"Owned: {item.count}");
            ownedLabel.AddToClassList("exp-item-owned");
            var stepperRow = new VisualElement();
            stepperRow.AddToClassList("count-stepper");

            var countLabel = new Label("0");
            countLabel.AddToClassList("stepper-count");

            var minusBtn = new Button { text = "-" };
            minusBtn.AddToClassList("stepper-btn");

            var plusBtn = new Button { text = "+" };
            plusBtn.AddToClassList("stepper-btn");

            ExpItemType itemType = item.type;
            int maxCount = item.count;

            minusBtn.RegisterCallback<ClickEvent>(_ =>
            {
                int current = _selectedCounts.GetValueOrDefault(itemType, 0);
                if (current <= 0) return;

                current--;
                _selectedCounts[itemType] = current;
                countLabel.text = current.ToString();
                OnExpItemCountChanged();
            });

            plusBtn.RegisterCallback<ClickEvent>(_ =>
            {
                int current = _selectedCounts.GetValueOrDefault(itemType, 0);
                if (current >= maxCount) return;

                current++;
                _selectedCounts[itemType] = current;
                countLabel.text = current.ToString();
                OnExpItemCountChanged();
            });

            stepperRow.Add(minusBtn);
            stepperRow.Add(countLabel);
            stepperRow.Add(plusBtn);

            row.Add(iconEl);
            row.Add(nameLabel);
            row.Add(ownedLabel);
            row.Add(stepperRow);

            return row;
        }
        private static int ExpForLevel(int lv)
        {
            return 100 + (lv - 1) * 120;
        }

        private void OnExpItemCountChanged()
        {
            int totalExp = _selectedCounts.Sum(kv => (int)kv.Key * kv.Value);
            UpdatePreview(totalExp);
        }

        private void UpdatePreview(int totalExp)
        {
            int newLevel = _currentLevel;
            int remainExp = _currentExp + totalExp;

            while (newLevel < MaxLevel && remainExp >= ExpForLevel(newLevel))
            {
                remainExp -= ExpForLevel(newLevel);
                newLevel++;
            }

            if (_previewLevelLabel != null)
            {
                _previewLevelLabel.text = newLevel > _currentLevel
                    ? $"??Lv.{newLevel}"
                    : $"Lv.{_currentLevel}";
            }

            ShowStatCompare(_currentLevel, newLevel);

            if (_levelUpBtn != null)
            {
                _levelUpBtn.SetEnabled(newLevel > _currentLevel);
            }
        }

        private void ShowStatCompare(int before, int after)
        {
            if (_agent == null) return;

            float hpBefore = _agent.GetHp(before);
            float hpAfter = _agent.GetHp(after);
            float atkBefore = _agent.GetAtk(before);
            float atkAfter = _agent.GetAtk(after);
            float defBefore = _agent.GetDef(before);
            float defAfter = _agent.GetDef(after);

            SetStatLabels(_hpBeforeLabel, _hpAfterLabel, _hpDiffLabel, hpBefore, hpAfter);
            SetStatLabels(_atkBeforeLabel, _atkAfterLabel, _atkDiffLabel, atkBefore, atkAfter);
            SetStatLabels(_defBeforeLabel, _defAfterLabel, _defDiffLabel, defBefore, defAfter);
        }

        private static void SetStatLabels(Label beforeLabel, Label afterLabel,
            Label diffLabel, float before, float after)
        {
            if (beforeLabel != null) beforeLabel.text = $"{before:F0}";
            if (afterLabel != null) afterLabel.text = $"{after:F0}";

            if (diffLabel != null)
            {
                float diff = after - before;
                if (diff > 0f)
                {
                    diffLabel.text = $"+{diff:F0}";
                    diffLabel.RemoveFromClassList("stat-diff-zero");
                    diffLabel.AddToClassList("stat-diff-positive");
                }
                else
                {
                    diffLabel.text = "";
                    diffLabel.RemoveFromClassList("stat-diff-positive");
                    diffLabel.AddToClassList("stat-diff-zero");
                }
            }
        }

        private void OnLevelUpConfirmed()
        {
            if (_agent == null) return;

            int totalExp = _selectedCounts.Sum(kv => (int)kv.Key * kv.Value);
            if (totalExp <= 0) return;
            foreach (var kv in _selectedCounts)
            {
                if (kv.Value <= 0) continue;

                ExpItem item = _expItems.FirstOrDefault(e => e.type == kv.Key);
                if (item != null)
                {
                    item.count -= kv.Value;
                    _playerData?.SetExpItemCount(kv.Key, item.count);
                }
            }
            int newLevel = _currentLevel;
            int remainExp = _currentExp + totalExp;

            while (newLevel < MaxLevel && remainExp >= ExpForLevel(newLevel))
            {
                remainExp -= ExpForLevel(newLevel);
                newLevel++;
            }
            _playerData?.SetCharacterProgress(_agent.id, newLevel, remainExp);

            int previousLevel = _currentLevel;
            _currentLevel = newLevel;
            _currentExp = remainExp;
            _onCharacterChanged?.RaiseEvent();
            StartCoroutine(PlayLevelUpEffect(previousLevel, newLevel));
        }

        private IEnumerator PlayLevelUpEffect(int fromLevel, int toLevel)
        {
            if (_levelUpBtn != null) _levelUpBtn.SetEnabled(false);
            if (_effectCanvasGroup != null)
            {
                _effectCanvasGroup.alpha = 0f;
                _effectCanvasGroup.gameObject.SetActive(true);

                float elapsed = 0f;
                float fadeDuration = 0.3f;
                while (elapsed < fadeDuration)
                {
                    elapsed += Time.deltaTime;
                    _effectCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
                    yield return null;
                }

                _effectCanvasGroup.alpha = 1f;
            }

            yield return new WaitForSeconds(0.2f);
            Label levelUpText = null;
            if (_root != null)
            {
                levelUpText = new Label("LEVEL UP!");
                levelUpText.AddToClassList("levelup-popup-text");
                _root.Add(levelUpText);
            }

            yield return new WaitForSeconds(1f);
            if (_effectCanvasGroup != null)
            {
                float elapsed = 0f;
                float fadeDuration = 0.3f;
                while (elapsed < fadeDuration)
                {
                    elapsed += Time.deltaTime;
                    _effectCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                    yield return null;
                }

                _effectCanvasGroup.alpha = 0f;
                _effectCanvasGroup.gameObject.SetActive(false);
            }
            if (levelUpText != null)
            {
                levelUpText.RemoveFromHierarchy();
            }
            _selectedCounts.Clear();
            RefreshUI();
        }
    }
}


