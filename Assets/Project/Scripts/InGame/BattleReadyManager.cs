using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace ProjectFirst.InGame
{
    /// <summary>
    /// 전투 준비 화면의 파티 편성·스테이지 정보·출격을 총괄하는 매니저.
    /// </summary>
    public class BattleReadyManager : MonoBehaviour
    {
        // ── 상수 ────────────────────────────────────────────────
        private const int MaxPartySize = 3;

        // ── Inspector 연결 ──────────────────────────────────────
        [SerializeField] private UIDocument _uiDocument;
        [SerializeField] private PlayerData _playerData;
        [SerializeField] private AgentTable _agentTable;
        [SerializeField] private StageData _stageData;
        [SerializeField] private RunSession _runSession;

        // ── 런타임 데이터 ────────────────────────────────────────
        private readonly int[] _partySlots = new int[MaxPartySize];
        private StageData.StageInfo _targetStage;
        private int _pendingSlotIndex = -1;

        // ── UI 요소 캐시 ────────────────────────────────────────
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

        // ─────────────────────────────────────────────────────────
        // Lifecycle
        // ─────────────────────────────────────────────────────────

        private void OnEnable()
        {
            _targetStage = GetTargetStage();
            for (int i = 0; i < MaxPartySize; i++) _partySlots[i] = -1;

            BindUI();
            RefreshStageInfo();
            RefreshAllSlots();
            RefreshPartyPower();
            RefreshBattleStartBtn();

            ProjectFirst.OutGame.TutorialManager.Instance?.TryTrigger("first_battle_ready");
        }

        // ─────────────────────────────────────────────────────────
        // 스테이지 조회
        // ─────────────────────────────────────────────────────────

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

        // ─────────────────────────────────────────────────────────
        // UI 바인딩
        // ─────────────────────────────────────────────────────────

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

        // ─────────────────────────────────────────────────────────
        // 스테이지 정보 표시
        // ─────────────────────────────────────────────────────────

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

        // ─────────────────────────────────────────────────────────
        // 슬롯 조작
        // ─────────────────────────────────────────────────────────

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

                var removeBtn = new Button { text = "✕" };
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

        // ─────────────────────────────────────────────────────────
        // 캐릭터 선택 팝업
        // ─────────────────────────────────────────────────────────

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

                if (assigned.Contains(ownedId))
                {
                    card.SetEnabled(false);
                    card.AddToClassList("char-select-disabled");
                }
                else
                {
                    int capturedId = ownedId;
                    card.RegisterCallback<ClickEvent>(_ => OnCharSelected(capturedId));
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

        // ─────────────────────────────────────────────────────────
        // 자동 편성
        // ─────────────────────────────────────────────────────────

        /// <summary>
        /// 보유 캐릭터를 적 속성 유리 우선 + 전투력 내림차순으로 정렬해 최대 3명을 자동 선택합니다.
        /// </summary>
        private void OnAutoPartyClicked()
        {
            ElementType enemy = _targetStage?.enemyElement ?? ElementType.Reason;

            List<AgentInfo> owned = _playerData.ownedCharacterIds
                .Select(id => _agentTable.GetAgentInfo(id))
                .Where(a => a != null)
                .ToList();

            // 유리 속성 우선, 동급이면 전투력 내림차순
            owned.Sort((a, b) =>
            {
                bool aAdv = ElementTypeHelper.HasAdvantage(a.element, enemy);
                bool bAdv = ElementTypeHelper.HasAdvantage(b.element, enemy);
                if (aAdv != bAdv) return bAdv.CompareTo(aAdv);
                return b.GetPower(GetAgentLevel(b.id)).CompareTo(a.GetPower(GetAgentLevel(a.id)));
            });

            for (int i = 0; i < MaxPartySize; i++)
                _partySlots[i] = i < owned.Count ? owned[i].id : -1;

            RefreshAllSlots();
            RefreshPartyPower();
            RefreshBattleStartBtn();
        }

        // ─────────────────────────────────────────────────────────
        // 파티 전투력 갱신
        // ─────────────────────────────────────────────────────────

        /// <summary>파티 총 전투력을 계산하고 색상 피드백을 포함해 표시합니다.</summary>
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

        // ─────────────────────────────────────────────────────────
        // 전투 시작
        // ─────────────────────────────────────────────────────────

        private void RefreshBattleStartBtn()
        {
            if (_battleStartBtn == null) return;
            _battleStartBtn.SetEnabled(_partySlots.Any(id => id >= 0));
        }

        /// <summary>스태미나 차감 후 RunSession을 저장하고 InGame 씬으로 전환합니다.</summary>
        private void OnBattleStartClicked()
        {
            if (_partySlots.All(id => id < 0)) return;

            int cost = _targetStage?.staminaCost ?? 1;
            if (!_playerData.SpendStamina(cost))
            {
                Debug.LogWarning($"[BattleReadyManager] 스태미나 부족 (필요: {cost}, 보유: {_playerData.stamina})");
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

            if (AsyncSceneLoader.Instance != null)
                AsyncSceneLoader.Instance.LoadSceneAsync("InGame", LoadSceneMode.Single);
            else
                SceneManager.LoadScene("InGame");
        }

        // ─────────────────────────────────────────────────────────
        // 소탕
        // ─────────────────────────────────────────────────────────

        /// <summary>3성 클리어 스테이지 한정으로 소탕 처리합니다.</summary>
        private void OnSweepClicked()
        {
            if (_targetStage == null || _targetStage.clearStars < 3)
            {
                Debug.Log("[BattleReadyManager] 3성 클리어 후 소탕 가능합니다.");
                return;
            }

            int cost = _targetStage.staminaCost;
            if (!_playerData.SpendStamina(cost))
            {
                Debug.LogWarning($"[BattleReadyManager] 소탕 스태미나 부족 (필요: {cost}, 보유: {_playerData.stamina})");
                return;
            }

            if (_targetStage.previewRewards != null)
            {
                foreach (RewardItem reward in _targetStage.previewRewards)
                {
                    if (reward.itemName == "골드") _playerData.AddGold(reward.amount);
                    else if (reward.itemName == "젬") _playerData.AddGem(reward.amount);
                }
            }

            Debug.Log("[BattleReadyManager] 소탕 완료. 보상이 지급되었습니다.");
        }

        // ─────────────────────────────────────────────────────────
        // 뒤로가기
        // ─────────────────────────────────────────────────────────

        private void OnBackClicked()
        {
            if (AsyncSceneLoader.Instance != null)
                AsyncSceneLoader.Instance.LoadSceneAsync("MapChapterScene", LoadSceneMode.Single);
            else
                SceneManager.LoadScene("MapChapterScene");
        }

        // ─────────────────────────────────────────────────────────
        // 유틸
        // ─────────────────────────────────────────────────────────

        private static int GetAgentLevel(int agentId)
            => PlayerPrefs.GetInt($"agent_lv_{agentId}", 1);
    }
}
