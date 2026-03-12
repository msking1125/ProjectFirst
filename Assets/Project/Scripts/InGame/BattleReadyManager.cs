using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using ProjectFirst.Data;
namespace ProjectFirst.InGame
{
    /// <summary>
    /// Pre-battle setup screen controller for party selection, stage info, and battle entry.
    /// </summary>
    public class BattleReadyManager : MonoBehaviour
    {
        // Constants
        private const int MaxPartySize = 3;

        // Inspector references
        [SerializeField] private UIDocument _uiDocument;
        [SerializeField] private PlayerData _playerData;
        [SerializeField] private AgentTable _agentTable;
        [SerializeField] private StageData _stageData;
        [SerializeField] private RunSession _runSession;
        
        [Header("Enhanced Systems")]
        [SerializeField] private BattleReadyUIAnimator _uiAnimator;
        [SerializeField] private CharacterTooltipManager _tooltipManager;
        [SerializeField] private PartyValidationSystem _validationSystem;

        // Runtime state
        private readonly int[] _partySlots = new int[MaxPartySize];
        private StageData.StageInfo _targetStage;
        private int _pendingSlotIndex = -1;

        // Cached UI references
        private VisualElement _root;
        private Label _stageTitleLabel;
        private Label _stageDescLabel;
        private Label _recommendPowerLabel;
        private VisualElement _enemyElementRow;
        private Label _totalPowerLabel;
        private Label _powerCompareLabel;
        private readonly VisualElement[] _slotElements = new VisualElement[MaxPartySize];
        private Button _autoPartyBtn;
        private Button _battleStartBtn;
        private Button _sweepBtn;
        private Button _backBtn;
        private Label _staminaCostLabel;
        private VisualElement _rewardPreviewContainer;
        private VisualElement _charSelectPopup;
        private VisualElement _charGrid;
        private Button _charSelectCloseBtn;
        // Lifecycle

        private void OnEnable()
        {
            _targetStage = GetTargetStage();
            for (int i = 0; i < MaxPartySize; i++) _partySlots[i] = -1;

            InitializeEnhancedSystems();
            BindUI();
            RefreshStageInfo();
            RefreshAllSlots();
            RefreshPartyPower();
            RefreshBattleStartBtn();

            ProjectFirst.OutGame.TutorialManager.Instance?.TryTrigger("first_battle_ready");
        }
        
        /// <summary>
        /// 개선된 시스템들 초기화
        /// </summary>
        private void InitializeEnhancedSystems()
        {
            // UI 애니메이터 초기화
            if (_uiAnimator == null)
                _uiAnimator = GetComponent<BattleReadyUIAnimator>();
            
            // 툴팁 매니저 초기화
            if (_tooltipManager == null)
                _tooltipManager = GetComponent<CharacterTooltipManager>();
            
            // 파티 유효성 검증 시스템 초기화
            if (_validationSystem == null)
                _validationSystem = GetComponent<PartyValidationSystem>();
            
            // 필요한 데이터 참조 설정
            if (_tooltipManager != null)
            {
                var tooltipAgentTable = _tooltipManager.GetType().GetField("agentTable", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var tooltipPlayerData = _tooltipManager.GetType().GetField("playerData", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (tooltipAgentTable != null) tooltipAgentTable.SetValue(_tooltipManager, _agentTable);
                if (tooltipPlayerData != null) tooltipPlayerData.SetValue(_tooltipManager, _playerData);
            }
            
            if (_validationSystem != null)
            {
                var validationAgentTable = _validationSystem.GetType().GetField("agentTable", 
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                var validationStageData = _validationSystem.GetType().GetField("stageData", 
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                var validationPlayerData = _validationSystem.GetType().GetField("playerData", 
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                
                if (validationAgentTable != null) validationAgentTable.SetValue(_validationSystem, _agentTable);
                if (validationStageData != null) validationStageData.SetValue(_validationSystem, _stageData);
                if (validationPlayerData != null) validationPlayerData.SetValue(_validationSystem, _playerData);
            }
        }
        // Stage lookup

        private StageData.StageInfo GetTargetStage()
        {
            if (_stageData == null) return null;

            int stageId = _runSession != null ? _runSession.currentStageId : 0;
            if (stageId > 0)
                return _stageData.stages.FirstOrDefault(s => s.stageId == stageId);

            if (_playerData == null) return null;
            return _stageData.GetByChapter(_playerData.currentChapter)
                             .FirstOrDefault(s => s.stageNumber == _playerData.currentStage);
        }
        // UI binding

        private void BindUI()
        {
            _root = _uiDocument.rootVisualElement;

            _stageTitleLabel     = _root.Q<Label>("stage-title");
            _stageDescLabel      = _root.Q<Label>("stage-description");
            _recommendPowerLabel = _root.Q<Label>("recommend-power");
            _enemyElementRow     = _root.Q<VisualElement>("enemy-element-row");
            _totalPowerLabel     = _root.Q<Label>("total-power-label");
            _powerCompareLabel   = _root.Q<Label>("power-compare-label");

            for (int i = 0; i < MaxPartySize; i++)
            {
                _slotElements[i] = _root.Q<VisualElement>($"party-slot-{i}");
                int captured = i;
                _slotElements[i]?.RegisterCallback<ClickEvent>(_ => OnSlotClicked(captured));
            }

            _autoPartyBtn    = _root.Q<Button>("auto-party-btn");
            _battleStartBtn  = _root.Q<Button>("battle-start-btn");
            _sweepBtn        = _root.Q<Button>("sweep-btn");
            _backBtn         = _root.Q<Button>("back-btn");
            _staminaCostLabel = _root.Q<Label>("stamina-cost-label");

            _autoPartyBtn?.RegisterCallback<ClickEvent>(_ => OnAutoPartyClicked());
            _battleStartBtn?.RegisterCallback<ClickEvent>(_ => OnBattleStartClicked());
            _sweepBtn?.RegisterCallback<ClickEvent>(_ => OnSweepClicked());
            _backBtn?.RegisterCallback<ClickEvent>(_ => OnBackClicked());

            _rewardPreviewContainer = _root.Q<VisualElement>("reward-preview");
            _charSelectPopup        = _root.Q<VisualElement>("char-select-popup");
            _charGrid               = _root.Q<VisualElement>("char-grid");
            _charSelectCloseBtn     = _root.Q<Button>("char-select-close-btn");
            _charSelectCloseBtn?.RegisterCallback<ClickEvent>(_ => HideCharSelectPopup());

            if (_charSelectPopup != null) _charSelectPopup.style.display = DisplayStyle.None;
        }
        // Stage info

        private void RefreshStageInfo()
        {
            if (_targetStage == null) return;

            if (_stageTitleLabel     != null) _stageTitleLabel.text = _targetStage.stageName;
            if (_stageDescLabel      != null) _stageDescLabel.text  = _targetStage.description;
            if (_recommendPowerLabel != null)
                _recommendPowerLabel.text = $"{_targetStage.recommendedPower:N0}";
            if (_staminaCostLabel    != null)
                _staminaCostLabel.text = $"스태미나 {_targetStage.staminaCost}";

            if (_enemyElementRow != null)
            {
                _enemyElementRow.Clear();
                var label = new Label(_targetStage.enemyElement.ToString());
                label.AddToClassList("element-label");
                label.AddToClassList($"element-{_targetStage.enemyElement.ToString().ToLower()}");
                _enemyElementRow.Add(label);
            }

            if (_sweepBtn != null)
                _sweepBtn.SetEnabled(_targetStage.clearStars >= 3);

            RefreshRewardPreview();
        }

        private void RefreshRewardPreview()
        {
            if (_rewardPreviewContainer == null || _targetStage?.previewRewards == null) return;
            _rewardPreviewContainer.Clear();

            foreach (RewardItem reward in _targetStage.previewRewards)
            {
                var slot = new VisualElement();
                slot.AddToClassList("reward-slot");

                var icon = new VisualElement();
                icon.AddToClassList("reward-icon");
                if (reward.icon != null)
                    icon.style.backgroundImage = new StyleBackground(reward.icon);

                var lbl = new Label($"{reward.itemName} x{reward.amount}");
                lbl.AddToClassList("reward-amount");

                slot.Add(icon);
                slot.Add(lbl);
                _rewardPreviewContainer.Add(slot);
            }
        }
        // Party control

        private void OnSlotClicked(int slotIndex)
        {
            if (_partySlots[slotIndex] >= 0)
            {
                _partySlots[slotIndex] = -1;
                RefreshSlot(slotIndex);
                RefreshPartyPower();
                RefreshBattleStartBtn();
                return;
            }
            _pendingSlotIndex = slotIndex;
            ShowCharSelectPopup();
        }

        private void RefreshAllSlots()
        {
            for (int i = 0; i < MaxPartySize; i++) RefreshSlot(i);
        }

        private void RefreshSlot(int slotIndex)
        {
            VisualElement slot = _slotElements[slotIndex];
            if (slot == null) return;
            slot.Clear();

            int agentId = _partySlots[slotIndex];
            if (agentId < 0)
            {
                slot.RemoveFromClassList("slot-filled");
                slot.AddToClassList("slot-empty");
                var plus = new Label("+");
                plus.AddToClassList("slot-plus-icon");
                slot.Add(plus);
            }
            else
            {
                slot.RemoveFromClassList("slot-empty");
                slot.AddToClassList("slot-filled");

                AgentInfo agentInfo = _agentTable.GetAgentInfo(agentId);
                AgentRow  agentRow  = _agentTable.GetById(agentId);

                var thumbnail = new VisualElement();
                thumbnail.AddToClassList("slot-thumbnail");
                if (agentInfo?.thumbnail != null)
                    thumbnail.style.backgroundImage = new StyleBackground(agentInfo.thumbnail);
                else if (agentRow?.portrait != null)
                    thumbnail.style.backgroundImage = new StyleBackground(agentRow.portrait);

                string name = agentInfo?.agentName ?? agentRow?.name ?? $"#{agentId}";
                var nameLabel = new Label(name);
                nameLabel.AddToClassList("slot-name");

                int power = agentInfo?.GetPower(GetAgentLevel(agentId)) ?? 0;
                var powerLabel = new Label($"CP {power:N0}");
                powerLabel.AddToClassList("slot-power");

                var removeBtn = new Button { text = "??" };
                removeBtn.AddToClassList("slot-remove-btn");
                int idx = slotIndex;
                removeBtn.RegisterCallback<ClickEvent>(evt =>
                {
                    evt.StopPropagation();
                    _partySlots[idx] = -1;
                    RefreshSlot(idx);
                    RefreshPartyPower();
                    RefreshBattleStartBtn();
                });

                slot.Add(thumbnail);
                slot.Add(nameLabel);
                slot.Add(powerLabel);
                slot.Add(removeBtn);
            }
        }
        // Character selection popup

        private void ShowCharSelectPopup()
        {
            if (_charSelectPopup == null || _charGrid == null) return;
            _charGrid.Clear();
            _charSelectPopup.style.display = DisplayStyle.Flex;

            var assigned = new HashSet<int>(_partySlots.Where(id => id >= 0));

            foreach (int ownedId in _playerData.ownedCharacterIds)
            {
                AgentInfo agentInfo = _agentTable.GetAgentInfo(ownedId);
                AgentRow  agentRow  = _agentTable.GetById(ownedId);
                if (agentInfo == null && agentRow == null) continue;

                var card = new VisualElement();
                card.AddToClassList("char-select-card");

                var thumb = new VisualElement();
                thumb.AddToClassList("char-select-thumb");
                if (agentInfo?.thumbnail != null)
                    thumb.style.backgroundImage = new StyleBackground(agentInfo.thumbnail);
                else if (agentRow?.portrait != null)
                    thumb.style.backgroundImage = new StyleBackground(agentRow.portrait);

                string charName = agentInfo?.agentName ?? agentRow?.name ?? $"#{ownedId}";
                var nameLabel  = new Label(charName);
                nameLabel.AddToClassList("char-select-name");

                int power = agentInfo?.GetPower(GetAgentLevel(ownedId)) ?? 0;
                var powerLabel = new Label($"CP {power:N0}");
                powerLabel.AddToClassList("char-select-power");

                card.Add(thumb);
                card.Add(nameLabel);
                card.Add(powerLabel);

                // 툴팁 등록
                if (_tooltipManager != null)
                {
                    _tooltipManager.RegisterCharacterCard(card, ownedId);
                }

                if (assigned.Contains(ownedId))
                {
                    card.SetEnabled(false);
                    card.AddToClassList("char-select-disabled");
                }
                else
                {
                    int capturedId = ownedId;
                    card.RegisterCallback<ClickEvent>(_ => 
                    {
                        OnCharSelected(capturedId);
                        if (_uiAnimator != null)
                            _uiAnimator.PlayCharacterSelectAnimation(card);
                    });
                }

                _charGrid.Add(card);
            }
        }

        private void OnCharSelected(int agentId)
        {
            if (_pendingSlotIndex < 0 || _pendingSlotIndex >= MaxPartySize) return;
            _partySlots[_pendingSlotIndex] = agentId;
            _pendingSlotIndex = -1;
            HideCharSelectPopup();
            RefreshAllSlots();
            RefreshPartyPower();
            RefreshBattleStartBtn();
        }

        private void HideCharSelectPopup()
        {
            if (_charSelectPopup != null) _charSelectPopup.style.display = DisplayStyle.None;
            _pendingSlotIndex = -1;
        }
        // Auto party

        /// <summary>
        /// Automatically selects up to three owned agents using the enhanced validation system.
        /// </summary>
        private void OnAutoPartyClicked()
        {
            if (_validationSystem != null && _targetStage != null)
            {
                // 개선된 자동 파티 구성 시스템 사용
                int[] autoParty = _validationSystem.GetAutoPartyComposition(_targetStage);
                for (int i = 0; i < MaxPartySize; i++)
                    _partySlots[i] = i < autoParty.Length ? autoParty[i] : -1;
            }
            else
            {
                // 기존 방식으로 fallback
                ElementType enemy = _targetStage?.enemyElement ?? ElementType.Reason;

                List<AgentInfo> owned = _playerData.ownedCharacterIds
                    .Select(id => _agentTable.GetAgentInfo(id))
                    .Where(a => a != null)
                    .ToList();

                // Prefer elemental advantage, then sort by descending combat power.
                owned.Sort((a, b) =>
                {
                    bool aAdv = ElementTypeHelper.HasAdvantage(a.element, enemy);
                    bool bAdv = ElementTypeHelper.HasAdvantage(b.element, enemy);
                    if (aAdv != bAdv) return bAdv.CompareTo(aAdv);
                    return b.GetPower(GetAgentLevel(b.id)).CompareTo(a.GetPower(GetAgentLevel(a.id)));
                });

                for (int i = 0; i < MaxPartySize; i++)
                    _partySlots[i] = i < owned.Count ? owned[i].id : -1;
            }

            RefreshAllSlots();
            RefreshPartyPower();
            RefreshBattleStartBtn();
        }
        // Party power

        /// <summary>Calculates total party power and updates the comparison UI.</summary>
        private void RefreshPartyPower()
        {
            int total = 0;
            for (int i = 0; i < MaxPartySize; i++)
            {
                int id = _partySlots[i];
                if (id < 0) continue;
                total += _agentTable.GetAgentInfo(id)?.GetPower(GetAgentLevel(id)) ?? 0;
            }

            int recommended = _targetStage?.recommendedPower ?? 0;
            bool sufficient = total >= recommended;

            if (_totalPowerLabel  != null) _totalPowerLabel.text  = $"{total:N0}";
            if (_powerCompareLabel != null)
            {
                _powerCompareLabel.text = $"/ {recommended:N0}";
                _powerCompareLabel.EnableInClassList("power-sufficient", sufficient);
                _powerCompareLabel.EnableInClassList("power-lacking",   !sufficient);
            }
        }
        // Battle start

        private void RefreshBattleStartBtn()
        {
            if (_battleStartBtn == null) return;
            _battleStartBtn.SetEnabled(_partySlots.Any(id => id >= 0));
        }

        /// <summary>Consumes stamina, stores the RunSession, and loads the in-game scene.</summary>
        private void OnBattleStartClicked()
        {
            if (_partySlots.All(id => id < 0)) return;

            // 파티 유효성 검증
            if (_validationSystem != null && _targetStage != null)
            {
                PartyValidationResult validationResult = _validationSystem.ValidateParty(_partySlots, _targetStage);
                
                if (!validationResult.IsValid)
                {
                    Debug.LogError($"[BattleReadyManager] 파티 유효성 검증 실패: {validationResult.ErrorMessage}");
                    // TODO: UI에 에러 메시지 표시
                    return;
                }
                
                if (validationResult.HasWarning)
                {
                    Debug.LogWarning($"[BattleReadyManager] 파티 경고: {validationResult.WarningMessage}");
                    // TODO: UI에 경고 메시지 표시 및 확인 절차
                }
            }

            int cost = _targetStage?.staminaCost ?? 1;
            if (!_playerData.SpendStamina(cost))
            {
                Debug.LogWarning($"[BattleReadyManager] Not enough stamina. Required: {cost}, current: {_playerData.stamina}");
                return;
            }

            if (_runSession != null)
            {
                _runSession.selectedAgentIds = _partySlots.Where(id => id >= 0).ToList();
                _runSession.currentStageId   = _targetStage?.stageId   ?? 0;
                _runSession.currentChapterId = _targetStage?.chapterId  ?? 0;
                _runSession.battleElapsedTime = 0f;
                _runSession.waveKillCount    = 0;
            }

            // 전투 시작 애니메이션
            if (_uiAnimator != null && _battleStartBtn != null)
                _uiAnimator.PlayBattleStartAnimation(_battleStartBtn);

            // 씬 전환
            StartCoroutine(LoadBattleSceneWithDelay());
        }
        
        /// <summary>
        /// 애니메이션 후 배틀 씬 로딩
        /// </summary>
        private IEnumerator LoadBattleSceneWithDelay()
        {
            // 애니메이션 대기
            yield return new WaitForSeconds(0.3f);
            
            if (AsyncSceneLoader.Instance != null)
                AsyncSceneLoader.Instance.LoadSceneAsync("InGame", LoadSceneMode.Single);
            else
                SceneManager.LoadScene("InGame");
        }
        // Sweep

        /// <summary>Runs a sweep when the stage has been cleared with three stars.</summary>
        private void OnSweepClicked()
        {
            if (_targetStage == null || _targetStage.clearStars < 3)
            {
                Debug.Log("[BattleReadyManager] Sweep is only available after earning three stars.");
                return;
            }

            int cost = _targetStage.staminaCost;
            if (!_playerData.SpendStamina(cost))
            {
                Debug.LogWarning($"[BattleReadyManager] Not enough stamina for sweep. Required: {cost}, current: {_playerData.stamina}");
                return;
            }

            if (_targetStage.previewRewards != null)
            {
                foreach (RewardItem reward in _targetStage.previewRewards)
                {
                    if (reward.itemName == "??" || reward.itemName == "Gold") _playerData.AddGold(reward.amount);
                    else if (reward.itemName == "Gem") _playerData.AddGem(reward.amount);
                }
            }

            Debug.Log("[BattleReadyManager] Sweep completed. Rewards have been granted.");
        }
        // Back

        private void OnBackClicked()
        {
            if (AsyncSceneLoader.Instance != null)
                AsyncSceneLoader.Instance.LoadSceneAsync("MapChapterScene", LoadSceneMode.Single);
            else
                SceneManager.LoadScene("MapChapterScene");
        }
        // Helpers

        private static int GetAgentLevel(int agentId)
            => PlayerPrefs.GetInt($"agent_lv_{agentId}", 1);
    }
}





