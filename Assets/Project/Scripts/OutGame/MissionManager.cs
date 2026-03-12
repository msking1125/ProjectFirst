using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProjectFirst.Data;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectFirst.OutGame
{
    public class MissionManager : MonoBehaviour
    {
        private const string RewardGold = "Gold";
        private const string RewardGem = "Gem";
        private const string RewardEnhanceStone = "EnhanceStone";
        private const string RewardContract = "Contract";

        public static MissionManager Instance { get; private set; }

        [SerializeField] private UIDocument _uiDocument;
        [SerializeField] private PlayerData _playerData;
        [SerializeField] private VoidEventChannelSO _onCurrencyChanged;
        [SerializeField] private MissionCatalogSO _missionCatalog;

        private List<MissionEntry> _allMissions = new();
        private List<PointRewardTier> _dailyTiers = new();
        private List<MissionEntry> _filteredMissions = new();
        private MissionType _currentTab = MissionType.Daily;
        private Coroutine _timerCoroutine;
        private bool _uiBound;

        private DateTime NextDailyReset => DateTime.Today.AddDays(1);
        private DateTime NextWeeklyReset => GetNextMonday();

        private VisualElement _root;
        private Button _closeBtn;
        private Button _tabDailyBtn;
        private Button _tabWeeklyBtn;
        private Button _tabRepeatBtn;
        private Button _tabAchievementBtn;
        private Label _dailyBadge;
        private Label _weeklyBadge;
        private Label _repeatBadge;
        private Label _achievementBadge;
        private VisualElement _pointBarFill;
        private Label _pointLabel;
        private VisualElement _tierMarkerContainer;
        private VisualElement _resetTimerRow;
        private Label _resetTimerLabel;
        private ListView _missionListView;
        private Button _claimAllBtn;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            LoadMissionDefinitions();
            InitPointTiers();
            EnsurePersistentStateContainers();
            ResetMissionsIfNeeded();
            ApplyPersistentState();
            MarkLoginProgress();
        }

        private void OnEnable()
        {
            BindUI();
            SwitchTab(_currentTab);
            _timerCoroutine = StartCoroutine(CountdownTimerRoutine());
        }

        private void OnDisable()
        {
            if (_timerCoroutine != null)
            {
                StopCoroutine(_timerCoroutine);
                _timerCoroutine = null;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void LoadMissionDefinitions()
        {
            if (_missionCatalog != null && _missionCatalog.missions != null && _missionCatalog.missions.Count > 0)
            {
                _allMissions = _missionCatalog.missions.Select(CloneMission).ToList();
                return;
            }

            _allMissions = new List<MissionEntry>
            {
                CreateMission("d001", MissionType.Daily, MissionCondition.StageCleared, "Clear stages", "Clear 3 stages.", 3, 10, CreateRewards((RewardGold, 500))),
                CreateMission("d002", MissionType.Daily, MissionCondition.EnemyKilled, "Defeat enemies", "Defeat 30 enemies.", 30, 10, CreateRewards((RewardGem, 20))),
                CreateMission("d003", MissionType.Daily, MissionCondition.LoginCount, "Daily login", "Log in once today.", 1, 10, CreateRewards((RewardGold, 300))),
                CreateMission("d004", MissionType.Daily, MissionCondition.ItemObtained, "Collect items", "Obtain 5 items.", 5, 10, CreateRewards((RewardEnhanceStone, 1))),
                CreateMission("w001", MissionType.Weekly, MissionCondition.StageCleared, "Weekly stages", "Clear 20 stages this week.", 20, 20, CreateRewards((RewardGem, 100))),
                CreateMission("w002", MissionType.Weekly, MissionCondition.EnemyKilled, "Weekly hunt", "Defeat 200 enemies this week.", 200, 20, CreateRewards((RewardGold, 3000))),
                CreateMission("r001", MissionType.Repeat, MissionCondition.EnemyKilled, "Repeat hunt", "Defeat 10 enemies.", 10, 5, CreateRewards((RewardGold, 200))),
                CreateMission("a001", MissionType.Achievement, MissionCondition.LevelReached, "Reach level 10", "Reach account or character level 10.", 10, 30, CreateRewards((RewardContract, 1))),
            };
        }
        private static MissionEntry CreateMission(string id, MissionType type, MissionCondition condition, string title, string description, int targetCount, int rewardPoint, List<RewardItem> rewards)
        {
            return new MissionEntry
            {
                missionId = id,
                type = type,
                condition = condition,
                title = title,
                description = description,
                targetCount = targetCount,
                currentCount = 0,
                rewardPoint = rewardPoint,
                rewards = rewards,
                isClaimed = false,
            };
        }

        private static List<RewardItem> CreateRewards(params (string itemName, int amount)[] items)
        {
            var rewards = new List<RewardItem>(items.Length);
            foreach ((string itemName, int amount) in items)
            {
                rewards.Add(new RewardItem { itemName = itemName, amount = amount });
            }
            return rewards;
        }

        private void InitPointTiers()
        {
            if (_missionCatalog != null && _missionCatalog.pointRewardTiers != null && _missionCatalog.pointRewardTiers.Count > 0)
            {
                _dailyTiers = _missionCatalog.pointRewardTiers.Select(CloneTier).ToList();
                return;
            }

            _dailyTiers = new List<PointRewardTier>
            {
                CreateTier(20, CreateRewards((RewardGold, 1000))),
                CreateTier(40, CreateRewards((RewardGem, 30))),
                CreateTier(60, CreateRewards((RewardEnhanceStone, 3))),
                CreateTier(80, CreateRewards((RewardGold, 3000))),
                CreateTier(100, CreateRewards((RewardContract, 1))),
            };
        }
        private static PointRewardTier CreateTier(int requiredPoints, List<RewardItem> rewards)
        {
            return new PointRewardTier { requiredPoints = requiredPoints, rewards = rewards, isClaimed = false };
        }

        private static MissionEntry CloneMission(MissionEntry source)
        {
            if (source == null) return null;

            return new MissionEntry
            {
                missionId = source.missionId,
                type = source.type,
                condition = source.condition,
                title = source.title,
                description = source.description,
                targetCount = source.targetCount,
                currentCount = source.currentCount,
                rewardPoint = source.rewardPoint,
                rewards = source.rewards?.Select(CloneReward).ToList() ?? new List<RewardItem>(),
                isClaimed = source.isClaimed,
            };
        }

        private static PointRewardTier CloneTier(PointRewardTier source)
        {
            if (source == null) return null;

            return new PointRewardTier
            {
                requiredPoints = source.requiredPoints,
                rewards = source.rewards?.Select(CloneReward).ToList() ?? new List<RewardItem>(),
                isClaimed = source.isClaimed,
            };
        }

        private static RewardItem CloneReward(RewardItem source)
        {
            if (source == null) return null;

            return new RewardItem
            {
                itemId = source.itemId,
                itemName = source.itemName,
                amount = source.amount,
            };
        }
        private void EnsurePersistentStateContainers()
        {
            if (_playerData == null) return;
            _playerData.missionProgressRecords ??= new List<MissionProgressRecord>();
            _playerData.missionTierClaimRecords ??= new List<MissionTierClaimRecord>();
        }

        private void ApplyPersistentState()
        {
            if (_playerData == null) return;

            foreach (MissionEntry mission in _allMissions)
            {
                MissionProgressRecord record = GetOrCreateMissionRecord(mission.missionId);
                mission.currentCount = Mathf.Min(record.currentCount, mission.targetCount);
                mission.isClaimed = record.isClaimed;
            }

            ApplyTierClaimStateForCurrentTab();
            SyncMissionStateToPlayerData();
        }
        private void BindUI()
        {
            if (_uiBound || _uiDocument == null) return;

            _root = _uiDocument.rootVisualElement;
            if (_root == null) return;

            _closeBtn = _root.Q<Button>("close-btn");
            _closeBtn?.RegisterCallback<ClickEvent>(_ => gameObject.SetActive(false));

            _tabDailyBtn = _root.Q<Button>("tab-daily-btn");
            _tabWeeklyBtn = _root.Q<Button>("tab-weekly-btn");
            _tabRepeatBtn = _root.Q<Button>("tab-repeat-btn");
            _tabAchievementBtn = _root.Q<Button>("tab-achievement-btn");

            _tabDailyBtn?.RegisterCallback<ClickEvent>(_ => SwitchTab(MissionType.Daily));
            _tabWeeklyBtn?.RegisterCallback<ClickEvent>(_ => SwitchTab(MissionType.Weekly));
            _tabRepeatBtn?.RegisterCallback<ClickEvent>(_ => SwitchTab(MissionType.Repeat));
            _tabAchievementBtn?.RegisterCallback<ClickEvent>(_ => SwitchTab(MissionType.Achievement));

            _dailyBadge = _root.Q<Label>("daily-badge");
            _weeklyBadge = _root.Q<Label>("weekly-badge");
            _repeatBadge = _root.Q<Label>("repeat-badge");
            _achievementBadge = _root.Q<Label>("achievement-badge");
            _pointBarFill = _root.Q<VisualElement>("point-bar-fill");
            _pointLabel = _root.Q<Label>("point-label");
            _tierMarkerContainer = _root.Q<VisualElement>("tier-marker-container");
            _resetTimerRow = _root.Q<VisualElement>("reset-timer-row");
            _resetTimerLabel = _root.Q<Label>("reset-timer-label");

            _missionListView = _root.Q<ListView>("mission-list");
            if (_missionListView != null)
            {
                _missionListView.makeItem = MakeMissionCard;
                _missionListView.bindItem = BindMissionCard;
                _missionListView.selectionType = SelectionType.None;
                _missionListView.fixedItemHeight = 100;
            }

            _claimAllBtn = _root.Q<Button>("claim-all-btn");
            _claimAllBtn?.RegisterCallback<ClickEvent>(_ => ClaimAllMissions());
            _uiBound = true;
        }

        private void SwitchTab(MissionType type)
        {
            _currentTab = type;
            ApplyTierClaimStateForCurrentTab();
            SetTabActive(_tabDailyBtn, type == MissionType.Daily);
            SetTabActive(_tabWeeklyBtn, type == MissionType.Weekly);
            SetTabActive(_tabRepeatBtn, type == MissionType.Repeat);
            SetTabActive(_tabAchievementBtn, type == MissionType.Achievement);

            if (_resetTimerRow != null)
            {
                bool showTimer = type == MissionType.Daily || type == MissionType.Weekly;
                _resetTimerRow.style.display = showTimer ? DisplayStyle.Flex : DisplayStyle.None;
            }

            RefreshAll();
        }

        private static void SetTabActive(Button button, bool active)
        {
            if (button == null) return;
            button.EnableInClassList("tab-active", active);
        }

        private void RefreshAll()
        {
            RefreshList();
            RefreshPointBar();
            RefreshBadges();
            RefreshClaimAllButton();
        }

        private void RefreshList()
        {
            _filteredMissions = _allMissions
                .Where(m => m.type == _currentTab)
                .OrderBy(m => m.isClaimed ? 2 : m.IsCompleted ? 0 : 1)
                .ToList();

            if (_missionListView != null)
            {
                _missionListView.itemsSource = _filteredMissions;
                _missionListView.Rebuild();
            }
        }

        private void RefreshPointBar()
        {
            int currentPoints = GetCurrentPoints(_currentTab);
            int maxPoints = _dailyTiers.Count > 0 ? _dailyTiers[_dailyTiers.Count - 1].requiredPoints : 100;

            if (_pointLabel != null) _pointLabel.text = $"{currentPoints} / {maxPoints}";
            if (_pointBarFill != null)
            {
                float ratio = maxPoints > 0 ? Mathf.Clamp01((float)currentPoints / maxPoints) : 0f;
                _pointBarFill.style.width = Length.Percent(ratio * 100f);
            }

            RefreshTierMarkers(currentPoints);
        }

        private void RefreshTierMarkers(int currentPoints)
        {
            if (_tierMarkerContainer == null) return;
            _tierMarkerContainer.Clear();

            int maxPoints = _dailyTiers.Count > 0 ? _dailyTiers[_dailyTiers.Count - 1].requiredPoints : 100;
            for (int i = 0; i < _dailyTiers.Count; i++)
            {
                PointRewardTier tier = _dailyTiers[i];
                var marker = new VisualElement();
                marker.AddToClassList("tier-marker");
                marker.style.left = Length.Percent(maxPoints > 0 ? (float)tier.requiredPoints / maxPoints * 100f : 0f);

                var tierLabel = new Label($"{tier.requiredPoints}");
                tierLabel.AddToClassList("tier-label");
                string rewardPreview = tier.rewards.Count > 0 ? $"{tier.rewards[0].itemName} x{tier.rewards[0].amount}" : string.Empty;
                var rewardInfo = new Label(rewardPreview);
                rewardInfo.AddToClassList("tier-reward-info");

                var tierButton = new Button();
                tierButton.AddToClassList("tier-claim-btn");
                bool canClaim = currentPoints >= tier.requiredPoints && !tier.isClaimed;
                if (tier.isClaimed)
                {
                    tierButton.text = "Done";
                    tierButton.SetEnabled(false);
                    tierButton.AddToClassList("tier-claimed");
                }
                else if (canClaim)
                {
                    int tierIndex = i;
                    tierButton.text = "Claim";
                    tierButton.SetEnabled(true);
                    tierButton.AddToClassList("tier-available");
                    tierButton.RegisterCallback<ClickEvent>(_ => ClaimTierReward(tierIndex));
                }
                else
                {
                    tierButton.text = string.Empty;
                    tierButton.SetEnabled(false);
                    tierButton.AddToClassList("tier-locked");
                }

                marker.Add(tierLabel);
                marker.Add(rewardInfo);
                marker.Add(tierButton);
                _tierMarkerContainer.Add(marker);
            }
        }

        private void RefreshBadges()
        {
            SetBadge(_dailyBadge, MissionType.Daily);
            SetBadge(_weeklyBadge, MissionType.Weekly);
            SetBadge(_repeatBadge, MissionType.Repeat);
            SetBadge(_achievementBadge, MissionType.Achievement);
        }

        private void SetBadge(Label badge, MissionType type)
        {
            if (badge == null) return;
            int claimableCount = _allMissions.Count(m => m.type == type && m.CanClaim);
            if (claimableCount > 0)
            {
                badge.text = claimableCount.ToString();
                badge.style.display = DisplayStyle.Flex;
            }
            else
            {
                badge.style.display = DisplayStyle.None;
            }
        }

        private void RefreshClaimAllButton()
        {
            if (_claimAllBtn == null) return;
            _claimAllBtn.SetEnabled(_allMissions.Any(m => m.type == _currentTab && m.CanClaim));
        }

        private VisualElement MakeMissionCard()
        {
            var card = new VisualElement();
            card.AddToClassList("mission-card");

            var typeBadge = new Label { name = "type-badge" };
            typeBadge.AddToClassList("type-badge");
            var infoCol = new VisualElement();
            infoCol.AddToClassList("mission-info-col");
            var titleLabel = new Label { name = "mission-title" };
            titleLabel.AddToClassList("mission-title");
            var descLabel = new Label { name = "mission-desc" };
            descLabel.AddToClassList("mission-desc");
            var progressRow = new VisualElement();
            progressRow.AddToClassList("progress-row");
            var progressBar = new VisualElement { name = "progress-bar" };
            progressBar.AddToClassList("mission-progress-bar");
            var progressFill = new VisualElement { name = "progress-fill" };
            progressFill.AddToClassList("mission-progress-fill");
            progressBar.Add(progressFill);
            var progressText = new Label { name = "progress-text" };
            progressText.AddToClassList("progress-text");
            progressRow.Add(progressBar);
            progressRow.Add(progressText);
            infoCol.Add(titleLabel);
            infoCol.Add(descLabel);
            infoCol.Add(progressRow);
            var rewardPreview = new VisualElement { name = "reward-preview" };
            rewardPreview.AddToClassList("reward-preview");
            var rewardIcon = new VisualElement { name = "reward-icon" };
            rewardIcon.AddToClassList("reward-icon");
            var rewardText = new Label { name = "reward-text" };
            rewardText.AddToClassList("reward-text");
            rewardPreview.Add(rewardIcon);
            rewardPreview.Add(rewardText);
            var actionButton = new Button { name = "action-btn" };
            actionButton.AddToClassList("action-btn");
            card.Add(typeBadge);
            card.Add(infoCol);
            card.Add(rewardPreview);
            card.Add(actionButton);
            return card;
        }
        private void BindMissionCard(VisualElement element, int index)
        {
            if (index < 0 || index >= _filteredMissions.Count) return;

            MissionEntry mission = _filteredMissions[index];
            var typeBadge = element.Q<Label>("type-badge");
            if (typeBadge != null)
            {
                typeBadge.text = mission.type switch
                {
                    MissionType.Daily => "Daily",
                    MissionType.Weekly => "Weekly",
                    MissionType.Repeat => "Repeat",
                    MissionType.Achievement => "Achieve",
                    _ => string.Empty,
                };
                typeBadge.EnableInClassList("badge-daily", mission.type == MissionType.Daily);
                typeBadge.EnableInClassList("badge-weekly", mission.type == MissionType.Weekly);
                typeBadge.EnableInClassList("badge-repeat", mission.type == MissionType.Repeat);
                typeBadge.EnableInClassList("badge-achievement", mission.type == MissionType.Achievement);
            }

            var titleLabel = element.Q<Label>("mission-title");
            if (titleLabel != null) titleLabel.text = mission.title;
            var descLabel = element.Q<Label>("mission-desc");
            if (descLabel != null) descLabel.text = mission.description;
            var progressFill = element.Q<VisualElement>("progress-fill");
            if (progressFill != null) progressFill.style.width = Length.Percent(mission.Progress * 100f);
            var progressText = element.Q<Label>("progress-text");
            if (progressText != null) progressText.text = $"{mission.currentCount}/{mission.targetCount}";

            var rewardIcon = element.Q<VisualElement>("reward-icon");
            var rewardText = element.Q<Label>("reward-text");
            if (mission.rewards != null && mission.rewards.Count > 0)
            {
                RewardItem firstReward = mission.rewards[0];
                if (rewardIcon != null && firstReward.icon != null)
                {
                    rewardIcon.style.backgroundImage = new StyleBackground(firstReward.icon);
                }
                if (rewardText != null)
                {
                    rewardText.text = $"{firstReward.itemName} x{firstReward.amount}";
                }
            }

            var actionButton = element.Q<Button>("action-btn");
            if (actionButton == null) return;

            actionButton.clickable = null;
            actionButton.RemoveFromClassList("action-done");
            actionButton.RemoveFromClassList("action-claim");
            actionButton.RemoveFromClassList("action-go");

            if (mission.isClaimed)
            {
                actionButton.text = "Done";
                actionButton.SetEnabled(false);
                actionButton.AddToClassList("action-done");
            }
            else if (mission.CanClaim)
            {
                string missionId = mission.missionId;
                actionButton.text = "Claim";
                actionButton.SetEnabled(true);
                actionButton.AddToClassList("action-claim");
                actionButton.RegisterCallback<ClickEvent>(_ => ClaimMission(missionId));
            }
            else
            {
                MissionCondition condition = mission.condition;
                actionButton.text = "Go";
                actionButton.SetEnabled(true);
                actionButton.AddToClassList("action-go");
                actionButton.RegisterCallback<ClickEvent>(_ => NavigateToContent(condition));
            }
        }

        public void ClaimMission(string missionId)
        {
            MissionEntry mission = _allMissions.FirstOrDefault(m => m.missionId == missionId);
            if (mission == null || !mission.CanClaim) return;

            mission.isClaimed = true;
            GrantRewards(mission.rewards);
            SyncMissionStateToPlayerData();
            RefreshAll();
        }

        public void ClaimAllMissions()
        {
            List<MissionEntry> claimable = _allMissions.Where(m => m.type == _currentTab && m.CanClaim).ToList();
            if (claimable.Count == 0) return;

            List<RewardItem> totalRewards = new();
            foreach (MissionEntry mission in claimable)
            {
                mission.isClaimed = true;
                if (mission.rewards != null) totalRewards.AddRange(mission.rewards);
            }

            GrantRewards(totalRewards);
            SyncMissionStateToPlayerData();
            RefreshAll();
        }

        private void ClaimTierReward(int tierIndex)
        {
            if (tierIndex < 0 || tierIndex >= _dailyTiers.Count) return;

            PointRewardTier tier = _dailyTiers[tierIndex];
            int currentPoints = GetCurrentPoints(_currentTab);
            if (currentPoints < tier.requiredPoints || tier.isClaimed) return;

            tier.isClaimed = true;
            GrantRewards(tier.rewards);
            SyncMissionStateToPlayerData();
            RefreshPointBar();
        }

        public int GetCurrentPoints(MissionType type)
        {
            return _allMissions.Where(m => m.type == type && m.isClaimed).Sum(m => m.rewardPoint);
        }

        public void UpdateProgress(MissionCondition condition, int amount)
        {
            if (amount <= 0) return;

            List<MissionEntry> matching = _allMissions.Where(m => m.condition == condition && !m.isClaimed).ToList();
            foreach (MissionEntry mission in matching)
            {
                mission.currentCount = Mathf.Min(mission.currentCount + amount, mission.targetCount);
            }

            SyncMissionStateToPlayerData();
            if (gameObject.activeInHierarchy) RefreshAll();
        }

        private void GrantRewards(List<RewardItem> rewards)
        {
            if (rewards == null || _playerData == null) return;

            foreach (RewardItem reward in rewards)
            {
                if (!_playerData.TryGrantReward(reward))
                {
                    Debug.Log($"[MissionManager] Unhandled reward: {reward.itemName} x{reward.amount}");
                }
            }

            _onCurrencyChanged?.RaiseEvent();
        }

        private void NavigateToContent(MissionCondition condition)
        {
            Debug.Log($"[MissionManager] Navigate to content for condition: {condition}");
        }

        private IEnumerator CountdownTimerRoutine()
        {
            WaitForSeconds wait = new WaitForSeconds(1f);
            while (true)
            {
                UpdateCountdownTimer();
                yield return wait;
            }
        }

        private void UpdateCountdownTimer()
        {
            if (_resetTimerLabel == null) return;

            DateTime resetTime = _currentTab switch
            {
                MissionType.Daily => NextDailyReset,
                MissionType.Weekly => NextWeeklyReset,
                _ => DateTime.MaxValue,
            };

            if (resetTime == DateTime.MaxValue)
            {
                _resetTimerLabel.text = string.Empty;
                return;
            }

            TimeSpan remaining = resetTime - DateTime.Now;
            if (remaining.TotalSeconds <= 0)
            {
                ResetMissionsIfNeeded();
                ApplyPersistentState();
                RefreshAll();
                _resetTimerLabel.text = "Resetting...";
                return;
            }

            _resetTimerLabel.text = $"Reset in {remaining.Hours:D2}:{remaining.Minutes:D2}:{remaining.Seconds:D2}";
        }
        private void ResetMissionsIfNeeded()
        {
            ResetDailyMissionsIfNeeded();
            ResetWeeklyMissionsIfNeeded();
        }

        private void ResetDailyMissionsIfNeeded()
        {
            if (_playerData == null || HasResetToday(_playerData.lastDailyMissionResetUtc)) return;

            ResetMissionType(MissionType.Daily);
            ResetTierClaims(MissionType.Daily);
            _playerData.lastDailyMissionResetUtc = DateTime.UtcNow.ToString("o");
            SyncMissionStateToPlayerData();
        }

        private void ResetWeeklyMissionsIfNeeded()
        {
            if (_playerData == null || HasResetThisWeek(_playerData.lastWeeklyMissionResetUtc)) return;

            ResetMissionType(MissionType.Weekly);
            ResetTierClaims(MissionType.Weekly);
            _playerData.lastWeeklyMissionResetUtc = DateTime.UtcNow.ToString("o");
            SyncMissionStateToPlayerData();
        }

        private void ResetMissionType(MissionType type)
        {
            foreach (MissionEntry mission in _allMissions.Where(m => m.type == type))
            {
                mission.currentCount = 0;
                mission.isClaimed = false;
            }
        }

        private void ResetTierClaims(MissionType type)
        {
            if (_playerData == null) return;

            foreach (MissionTierClaimRecord record in _playerData.missionTierClaimRecords.Where(r => r.missionType == type))
            {
                record.isClaimed = false;
            }

            if (_currentTab == type)
            {
                foreach (PointRewardTier tier in _dailyTiers)
                {
                    tier.isClaimed = false;
                }
            }
        }

        private void MarkLoginProgress()
        {
            bool changed = false;
            foreach (MissionEntry mission in _allMissions.Where(m => m.condition == MissionCondition.LoginCount && !m.isClaimed))
            {
                int nextValue = Mathf.Max(mission.currentCount, 1);
                if (nextValue != mission.currentCount)
                {
                    mission.currentCount = Mathf.Min(nextValue, mission.targetCount);
                    changed = true;
                }
            }

            if (changed) SyncMissionStateToPlayerData();
        }

        private void ApplyTierClaimStateForCurrentTab()
        {
            if (_playerData == null) return;

            foreach (PointRewardTier tier in _dailyTiers)
            {
                MissionTierClaimRecord record = GetOrCreateTierRecord(_currentTab, tier.requiredPoints);
                tier.isClaimed = record.isClaimed;
            }
        }

        private void SyncMissionStateToPlayerData()
        {
            if (_playerData == null) return;
            EnsurePersistentStateContainers();

            foreach (MissionEntry mission in _allMissions)
            {
                MissionProgressRecord record = GetOrCreateMissionRecord(mission.missionId);
                record.currentCount = mission.currentCount;
                record.isClaimed = mission.isClaimed;
            }

            foreach (PointRewardTier tier in _dailyTiers)
            {
                MissionTierClaimRecord record = GetOrCreateTierRecord(_currentTab, tier.requiredPoints);
                record.isClaimed = tier.isClaimed;
            }
        }

        private MissionProgressRecord GetOrCreateMissionRecord(string missionId)
        {
            MissionProgressRecord record = _playerData.missionProgressRecords.FirstOrDefault(r => string.Equals(r.missionId, missionId, StringComparison.Ordinal));
            if (record != null) return record;

            record = new MissionProgressRecord { missionId = missionId, currentCount = 0, isClaimed = false };
            _playerData.missionProgressRecords.Add(record);
            return record;
        }

        private MissionTierClaimRecord GetOrCreateTierRecord(MissionType type, int requiredPoints)
        {
            MissionTierClaimRecord record = _playerData.missionTierClaimRecords.FirstOrDefault(r => r.missionType == type && r.requiredPoints == requiredPoints);
            if (record != null) return record;

            record = new MissionTierClaimRecord { missionType = type, requiredPoints = requiredPoints, isClaimed = false };
            _playerData.missionTierClaimRecords.Add(record);
            return record;
        }

        private static bool HasResetToday(string isoUtc)
        {
            return TryParseUtc(isoUtc, out DateTime parsed) && parsed.Date == DateTime.UtcNow.Date;
        }

        private static bool HasResetThisWeek(string isoUtc)
        {
            return TryParseUtc(isoUtc, out DateTime parsed) && GetWeekStartUtc(parsed) == GetWeekStartUtc(DateTime.UtcNow);
        }

        private static bool TryParseUtc(string value, out DateTime parsed)
        {
            return DateTime.TryParse(value, null, System.Globalization.DateTimeStyles.RoundtripKind, out parsed);
        }

        private static DateTime GetWeekStartUtc(DateTime dateTime)
        {
            int diff = ((int)dateTime.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
            return dateTime.Date.AddDays(-diff);
        }

        private static DateTime GetNextMonday()
        {
            DateTime today = DateTime.Today;
            int daysUntilMonday = ((int)DayOfWeek.Monday - (int)today.DayOfWeek + 7) % 7;
            return daysUntilMonday == 0 ? today.AddDays(7) : today.AddDays(daysUntilMonday);
        }
    }
}
