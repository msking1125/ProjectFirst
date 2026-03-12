using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProjectFirst.Data;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectFirst.OutGame.UI
{
    /// <summary>
    /// 罹먮┃??愿由??붾㈃???덈꺼?????⑤꼸.
    /// 寃쏀뿕移??꾩씠???좏깮 ???덈꺼??誘몃━蹂닿린 ???뺤젙 ?먮쫫???대떦?⑸땲??
    /// </summary>
    public class CharacterLevelUpPanel : MonoBehaviour
    {
        // ?? ?곸닔 ????????????????????????????????????????????????
        private const int MaxLevel = 100;

        // ?? Inspector ???????????????????????????????????????????
        [SerializeField] private PlayerData _playerData;
        [SerializeField] private VoidEventChannelSO _onCharacterChanged;
        [SerializeField] private CharacterGrowthCatalogSO _growthCatalog;

        // ?? ?곗씠????????????????????????????????????????????????
        private AgentInfo _agent;
        private int _currentLevel;
        private int _currentExp;
        private List<ExpItem> _expItems = new();
        private Dictionary<ExpItemType, int> _selectedCounts = new();

        // ?? UI ?붿냼 罹먯떆 ????????????????????????????????????????
        private VisualElement _root;
        private Label _currentLevelLabel;
        private Label _expBarLabel;
        private VisualElement _expBarFill;
        private VisualElement _expItemListContainer;
        private Label _previewLevelLabel;
        private VisualElement _statComparePanel;
        private Button _levelUpBtn;

        // ?ㅽ꺈 鍮꾧탳 ?덉씠釉?
        private Label _hpBeforeLabel;
        private Label _hpAfterLabel;
        private Label _hpDiffLabel;
        private Label _atkBeforeLabel;
        private Label _atkAfterLabel;
        private Label _atkDiffLabel;
        private Label _defBeforeLabel;
        private Label _defAfterLabel;
        private Label _defDiffLabel;

        // ?? ?곗텧 ????????????????????????????????????????????????
        [SerializeField] private CanvasGroup _effectCanvasGroup;

        // ?????????????????????????????????????????????????????????
        // Public API
        // ?????????????????????????????????????????????????????????

        /// <summary>?덈꺼???⑤꼸??珥덇린?뷀빀?덈떎. ???꾪솚 ??CharacterManageManager媛 ?몄텧?⑸땲??</summary>
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

        // ?????????????????????????????????????????????????????????
        // UI 諛붿씤??
        // ?????????????????????????????????????????????????????????

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

            // ?ㅽ꺈 鍮꾧탳
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

        // ?????????????????????????????????????????????????????????
        // 寃쏀뿕移??꾩씠??濡쒕뱶
        // ?????????????????????????????????????????????????????????

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

        // ?????????????????????????????????????????????????????????
        // UI 媛깆떊
        // ?????????????????????????????????????????????????????????

        private void RefreshUI()
        {
            if (_agent == null) return;

            // ?꾩옱 ?덈꺼 ?쒖떆
            if (_currentLevelLabel != null)
            {
                _currentLevelLabel.text = $"Lv.{_currentLevel}";
            }

            // 寃쏀뿕移?諛?
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

            // ?꾩씠??紐⑸줉 ?앹꽦
            BuildExpItemList();

            // 誘몃━蹂닿린 珥덇린??
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

            // ?꾩씠肄?
            var iconEl = new VisualElement();
            iconEl.AddToClassList("exp-item-icon");
            if (item.icon != null)
            {
                iconEl.style.backgroundImage = new StyleBackground(item.icon);
            }

            // ?대쫫
            var nameLabel = new Label(item.itemName);
            nameLabel.AddToClassList("exp-item-name");

            // 蹂댁쑀 ?섎웾
            var ownedLabel = new Label($"Owned: {item.count}");
            ownedLabel.AddToClassList("exp-item-owned");

            // ?섎웾 議곗젅 ?ㅽ뀒?? [-] [N] [+]
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

        // ?????????????????????????????????????????????????????????
        // 寃쏀뿕移?怨꾩궛
        // ?????????????????????????????????????????????????????????

        /// <summary>lv?먯꽌 lv+1濡??щ━?????꾩슂??寃쏀뿕移섎? 諛섑솚?⑸땲??</summary>
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

        // ?????????????????????????????????????????????????????????
        // ?ㅽ꺈 鍮꾧탳
        // ?????????????????????????????????????????????????????????

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

        // ?????????????????????????????????????????????????????????
        // ?덈꺼???뺤젙
        // ?????????????????????????????????????????????????????????

        private void OnLevelUpConfirmed()
        {
            if (_agent == null) return;

            int totalExp = _selectedCounts.Sum(kv => (int)kv.Key * kv.Value);
            if (totalExp <= 0) return;

            // ?꾩씠???뚮え
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

            // ?덈꺼 怨꾩궛
            int newLevel = _currentLevel;
            int remainExp = _currentExp + totalExp;

            while (newLevel < MaxLevel && remainExp >= ExpForLevel(newLevel))
            {
                remainExp -= ExpForLevel(newLevel);
                newLevel++;
            }

            // ???
            _playerData?.SetCharacterProgress(_agent.id, newLevel, remainExp);

            int previousLevel = _currentLevel;
            _currentLevel = newLevel;
            _currentExp = remainExp;

            // ?대깽??諛쒗뻾
            _onCharacterChanged?.RaiseEvent();

            // ?곗텧 ?쒖옉
            StartCoroutine(PlayLevelUpEffect(previousLevel, newLevel));
        }

        // ?????????????????????????????????????????????????????????
        // ?덈꺼???곗텧
        // ?????????????????????????????????????????????????????????

        private IEnumerator PlayLevelUpEffect(int fromLevel, int toLevel)
        {
            // 踰꾪듉 鍮꾪솢?깊솕 (?곗텧 以??ы겢由?諛⑹?)
            if (_levelUpBtn != null) _levelUpBtn.SetEnabled(false);

            // 鍮쏅굹???댄럺??(CanvasGroup ?뚰뙆 蹂닿컙)
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

            // "LEVEL UP!" ?띿뒪???쒖떆
            Label levelUpText = null;
            if (_root != null)
            {
                levelUpText = new Label("LEVEL UP!");
                levelUpText.AddToClassList("levelup-popup-text");
                _root.Add(levelUpText);
            }

            yield return new WaitForSeconds(1f);

            // ?댄럺???섏씠?쒖븘??
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

            // ?앹뾽 ?띿뒪???쒓굅
            if (levelUpText != null)
            {
                levelUpText.RemoveFromHierarchy();
            }

            // UI 媛깆떊 (???ㅽ꺈 ?쒖떆)
            _selectedCounts.Clear();
            RefreshUI();
        }
    }
}

