using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProjectFirst.Data;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectFirst.OutGame
{
    /// <summary>
    /// 미션 화면의 탭 전환·목록·포인트 바·카운트다운을 총괄하는 매니저.
    /// </summary>
    public class MissionManager : MonoBehaviour
    {
        public static MissionManager Instance { get; private set; }

        // ── Inspector ───────────────────────────────────────────
        [SerializeField] private UIDocument _uiDocument;
        [SerializeField] private PlayerData _playerData;
        [SerializeField] private VoidEventChannelSO _onCurrencyChanged;

        // ── 데이터 ──────────────────────────────────────────────
        private List<MissionEntry> _allMissions = new();
        private List<PointRewardTier> _dailyTiers = new();
        private MissionType _currentTab = MissionType.Daily;
        private Coroutine _timerCoroutine;

        // ── 리셋 시간 ──────────────────────────────────────────
        private DateTime NextDailyReset => DateTime.Today.AddDays(1);
        private DateTime NextWeeklyReset => GetNextMonday();

        // ── UI 요소 캐시 ────────────────────────────────────────
        private VisualElement _root;
        private Button _closeBtn;

        // 탭 버튼
        private Button _tabDailyBtn;
        private Button _tabWeeklyBtn;
        private Button _tabRepeatBtn;
        private Button _tabAchievementBtn;

        // 탭 뱃지
        private Label _dailyBadge;
        private Label _weeklyBadge;
        private Label _repeatBadge;
        private Label _achievementBadge;

        // 포인트 바
        private VisualElement _pointBarFill;
        private Label _pointLabel;
        private VisualElement _tierMarkerContainer;

        // 타이머
        private VisualElement _resetTimerRow;
        private Label _resetTimerLabel;

        // 리스트
        private ListView _missionListView;
        private Button _claimAllBtn;

        // ── 필터링된 목록 ───────────────────────────────────────
        private List<MissionEntry> _filteredMissions = new();

        // ─────────────────────────────────────────────────────────
        // Lifecycle
        // ─────────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            LoadMissions();
            InitPointTiers();
        }

        private void OnEnable()
        {
            BindUI();
            SwitchTab(MissionType.Daily);
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

        // ─────────────────────────────────────────────────────────
        // 데이터 초기화
        // ─────────────────────────────────────────────────────────

        private void LoadMissions()
        {
            _allMissions = new List<MissionEntry>
            {
                CreateMission("d001", MissionType.Daily, MissionCondition.StageCleared,
                    "스테이지 클리어", "스테이지를 3회 클리어하세요", 3, 1, 10,
                    new List<RewardItem> { new RewardItem { itemName = "골드", amount = 500 } }),
                CreateMission("d002", MissionType.Daily, MissionCondition.EnemyKilled,
                    "적 처치", "적을 30마리 처치하세요", 30, 12, 10,
                    new List<RewardItem> { new RewardItem { itemName = "젬", amount = 20 } }),
                CreateMission("d003", MissionType.Daily, MissionCondition.LoginCount,
                    "출석 체크", "게임에 접속하세요", 1, 1, 10,
                    new List<RewardItem> { new RewardItem { itemName = "골드", amount = 300 } }),
                CreateMission("d004", MissionType.Daily, MissionCondition.ItemObtained,
                    "아이템 획득", "아이템을 5개 획득하세요", 5, 0, 10,
                    new List<RewardItem> { new RewardItem { itemName = "강화석", amount = 1 } }),
                CreateMission("w001", MissionType.Weekly, MissionCondition.StageCleared,
                    "주간 스테이지", "스테이지를 20회 클리어하세요", 20, 5, 20,
                    new List<RewardItem> { new RewardItem { itemName = "젬", amount = 100 } }),
                CreateMission("w002", MissionType.Weekly, MissionCondition.EnemyKilled,
                    "주간 적 처치", "적을 200마리 처치하세요", 200, 45, 20,
                    new List<RewardItem> { new RewardItem { itemName = "골드", amount = 3000 } }),
                CreateMission("r001", MissionType.Repeat, MissionCondition.EnemyKilled,
                    "적 처치 (반복)", "적을 10마리 처치하세요", 10, 3, 5,
                    new List<RewardItem> { new RewardItem { itemName = "골드", amount = 200 } }),
                CreateMission("a001", MissionType.Achievement, MissionCondition.LevelReached,
                    "레벨 달성", "캐릭터 레벨 10 달성", 10, 4, 30,
                    new List<RewardItem> { new RewardItem { itemName = "계약서", amount = 1 } }),
            };
        }

        private static MissionEntry CreateMission(string id, MissionType type,
            MissionCondition condition, string title, string desc,
            int target, int current, int point, List<RewardItem> rewards)
        {
            return new MissionEntry
            {
                missionId = id,
                type = type,
                condition = condition,
                title = title,
                description = desc,
                targetCount = target,
                currentCount = current,
                rewardPoint = point,
                rewards = rewards
            };
        }

        private void InitPointTiers()
        {
            _dailyTiers = new List<PointRewardTier>
            {
                new PointRewardTier
                {
                    requiredPoints = 20,
                    rewards = new List<RewardItem> { new RewardItem { itemName = "골드", amount = 1000 } }
                },
                new PointRewardTier
                {
                    requiredPoints = 40,
                    rewards = new List<RewardItem> { new RewardItem { itemName = "젬", amount = 30 } }
                },
                new PointRewardTier
                {
                    requiredPoints = 60,
                    rewards = new List<RewardItem>
                        { new RewardItem { itemName = "강화석", amount = 3 } }
                },
                new PointRewardTier
                {
                    requiredPoints = 80,
                    rewards = new List<RewardItem> { new RewardItem { itemName = "골드", amount = 3000 } }
                },
                new PointRewardTier
                {
                    requiredPoints = 100,
                    rewards = new List<RewardItem>
                        { new RewardItem { itemName = "계약서", amount = 1 } }
                },
            };
        }

        // ─────────────────────────────────────────────────────────
        // UI 바인딩
        // ─────────────────────────────────────────────────────────

        private void BindUI()
        {
            _root = _uiDocument.rootVisualElement;

            _closeBtn = _root.Q<Button>("close-btn");
            _closeBtn?.RegisterCallback<ClickEvent>(_ => gameObject.SetActive(false));

            // 탭 버튼
            _tabDailyBtn = _root.Q<Button>("tab-daily-btn");
            _tabWeeklyBtn = _root.Q<Button>("tab-weekly-btn");
            _tabRepeatBtn = _root.Q<Button>("tab-repeat-btn");
            _tabAchievementBtn = _root.Q<Button>("tab-achievement-btn");

            _tabDailyBtn?.RegisterCallback<ClickEvent>(_ => SwitchTab(MissionType.Daily));
            _tabWeeklyBtn?.RegisterCallback<ClickEvent>(_ => SwitchTab(MissionType.Weekly));
            _tabRepeatBtn?.RegisterCallback<ClickEvent>(_ => SwitchTab(MissionType.Repeat));
            _tabAchievementBtn?.RegisterCallback<ClickEvent>(_ => SwitchTab(MissionType.Achievement));

            // 탭 뱃지
            _dailyBadge = _root.Q<Label>("daily-badge");
            _weeklyBadge = _root.Q<Label>("weekly-badge");
            _repeatBadge = _root.Q<Label>("repeat-badge");
            _achievementBadge = _root.Q<Label>("achievement-badge");

            // 포인트 바
            _pointBarFill = _root.Q<VisualElement>("point-bar-fill");
            _pointLabel = _root.Q<Label>("point-label");
            _tierMarkerContainer = _root.Q<VisualElement>("tier-marker-container");

            // 타이머
            _resetTimerRow = _root.Q<VisualElement>("reset-timer-row");
            _resetTimerLabel = _root.Q<Label>("reset-timer-label");

            // 리스트
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
        }

        // ─────────────────────────────────────────────────────────
        // 탭 전환
        // ─────────────────────────────────────────────────────────

        private void SwitchTab(MissionType type)
        {
            _currentTab = type;

            SetTabActive(_tabDailyBtn, type == MissionType.Daily);
            SetTabActive(_tabWeeklyBtn, type == MissionType.Weekly);
            SetTabActive(_tabRepeatBtn, type == MissionType.Repeat);
            SetTabActive(_tabAchievementBtn, type == MissionType.Achievement);

            // 타이머 행: 일일/주간만 표시
            bool showTimer = type == MissionType.Daily || type == MissionType.Weekly;
            if (_resetTimerRow != null)
            {
                _resetTimerRow.style.display = showTimer ? DisplayStyle.Flex : DisplayStyle.None;
            }

            RefreshAll();
        }

        private static void SetTabActive(Button btn, bool active)
        {
            if (btn == null) return;
            btn.EnableInClassList("tab-active", active);
        }

        // ─────────────────────────────────────────────────────────
        // 목록 갱신
        // ─────────────────────────────────────────────────────────

        private void RefreshAll()
        {
            RefreshList();
            RefreshPointBar();
            RefreshBadges();
            RefreshClaimAllButton();
        }

        /// <summary>현재 탭 기준으로 미션 목록을 갱신합니다.</summary>
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

        /// <summary>포인트 보상 바를 갱신합니다.</summary>
        private void RefreshPointBar()
        {
            int currentPoints = GetCurrentPoints(_currentTab);
            int maxPoints = _dailyTiers.Count > 0
                ? _dailyTiers[_dailyTiers.Count - 1].requiredPoints
                : 100;

            if (_pointLabel != null)
            {
                _pointLabel.text = $"{currentPoints} / {maxPoints}";
            }

            if (_pointBarFill != null)
            {
                float ratio = Mathf.Clamp01((float)currentPoints / maxPoints);
                _pointBarFill.style.width = Length.Percent(ratio * 100f);
            }

            RefreshTierMarkers(currentPoints);
        }

        private void RefreshTierMarkers(int currentPoints)
        {
            if (_tierMarkerContainer == null) return;

            _tierMarkerContainer.Clear();

            int maxPoints = _dailyTiers.Count > 0
                ? _dailyTiers[_dailyTiers.Count - 1].requiredPoints
                : 100;

            for (int i = 0; i < _dailyTiers.Count; i++)
            {
                PointRewardTier tier = _dailyTiers[i];

                var marker = new VisualElement();
                marker.AddToClassList("tier-marker");

                float posPercent = (float)tier.requiredPoints / maxPoints * 100f;
                marker.style.left = Length.Percent(posPercent);

                var tierLabel = new Label($"{tier.requiredPoints}");
                tierLabel.AddToClassList("tier-label");

                var rewardInfo = new Label(tier.rewards.Count > 0
                    ? $"{tier.rewards[0].itemName} x{tier.rewards[0].amount}"
                    : "");
                rewardInfo.AddToClassList("tier-reward-info");

                var tierBtn = new Button();
                tierBtn.AddToClassList("tier-claim-btn");

                bool canClaim = currentPoints >= tier.requiredPoints && !tier.isClaimed;
                bool alreadyClaimed = tier.isClaimed;

                if (alreadyClaimed)
                {
                    tierBtn.text = "완료";
                    tierBtn.SetEnabled(false);
                    tierBtn.AddToClassList("tier-claimed");
                }
                else if (canClaim)
                {
                    tierBtn.text = "받기";
                    tierBtn.SetEnabled(true);
                    tierBtn.AddToClassList("tier-available");
                    int tierIndex = i;
                    tierBtn.RegisterCallback<ClickEvent>(_ => ClaimTierReward(tierIndex));
                }
                else
                {
                    tierBtn.text = "";
                    tierBtn.SetEnabled(false);
                    tierBtn.AddToClassList("tier-locked");
                }

                marker.Add(tierLabel);
                marker.Add(rewardInfo);
                marker.Add(tierBtn);
                _tierMarkerContainer.Add(marker);
            }
        }

        /// <summary>각 탭의 미완료 수 뱃지를 갱신합니다.</summary>
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

            bool hasClaimable = _allMissions.Any(m => m.type == _currentTab && m.CanClaim);
            _claimAllBtn.SetEnabled(hasClaimable);
        }

        // ─────────────────────────────────────────────────────────
        // 미션 카드
        // ─────────────────────────────────────────────────────────

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

            var actionBtn = new Button { name = "action-btn" };
            actionBtn.AddToClassList("action-btn");

            card.Add(typeBadge);
            card.Add(infoCol);
            card.Add(rewardPreview);
            card.Add(actionBtn);

            return card;
        }

        private void BindMissionCard(VisualElement element, int index)
        {
            if (index < 0 || index >= _filteredMissions.Count) return;

            MissionEntry mission = _filteredMissions[index];

            // 타입 뱃지
            var typeBadge = element.Q<Label>("type-badge");
            if (typeBadge != null)
            {
                typeBadge.text = mission.type switch
                {
                    MissionType.Daily => "일일",
                    MissionType.Weekly => "주간",
                    MissionType.Repeat => "반복",
                    MissionType.Achievement => "업적",
                    _ => ""
                };
                typeBadge.EnableInClassList("badge-daily", mission.type == MissionType.Daily);
                typeBadge.EnableInClassList("badge-weekly", mission.type == MissionType.Weekly);
                typeBadge.EnableInClassList("badge-repeat", mission.type == MissionType.Repeat);
                typeBadge.EnableInClassList("badge-achievement", mission.type == MissionType.Achievement);
            }

            // 제목·설명
            var titleLabel = element.Q<Label>("mission-title");
            if (titleLabel != null) titleLabel.text = mission.title;

            var descLabel = element.Q<Label>("mission-desc");
            if (descLabel != null) descLabel.text = mission.description;

            // 진행도
            var progressFill = element.Q<VisualElement>("progress-fill");
            if (progressFill != null)
            {
                progressFill.style.width = Length.Percent(mission.Progress * 100f);
            }

            var progressText = element.Q<Label>("progress-text");
            if (progressText != null)
            {
                progressText.text = $"{mission.currentCount}/{mission.targetCount}";
            }

            // 보상 미리보기
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

            // 액션 버튼
            var actionBtn = element.Q<Button>("action-btn");
            if (actionBtn != null)
            {
                actionBtn.clickable = null;

                if (mission.isClaimed)
                {
                    actionBtn.text = "완료";
                    actionBtn.SetEnabled(false);
                    actionBtn.RemoveFromClassList("action-claim");
                    actionBtn.RemoveFromClassList("action-go");
                    actionBtn.AddToClassList("action-done");
                }
                else if (mission.CanClaim)
                {
                    actionBtn.text = "받기";
                    actionBtn.SetEnabled(true);
                    actionBtn.RemoveFromClassList("action-done");
                    actionBtn.RemoveFromClassList("action-go");
                    actionBtn.AddToClassList("action-claim");
                    string missionId = mission.missionId;
                    actionBtn.RegisterCallback<ClickEvent>(_ => ClaimMission(missionId));
                }
                else
                {
                    actionBtn.text = "이동";
                    actionBtn.SetEnabled(true);
                    actionBtn.RemoveFromClassList("action-done");
                    actionBtn.RemoveFromClassList("action-claim");
                    actionBtn.AddToClassList("action-go");
                    MissionCondition cond = mission.condition;
                    actionBtn.RegisterCallback<ClickEvent>(_ => NavigateToContent(cond));
                }
            }
        }

        // ─────────────────────────────────────────────────────────
        // 미션 수령
        // ─────────────────────────────────────────────────────────

        /// <summary>단일 미션의 보상을 수령합니다.</summary>
        public void ClaimMission(string missionId)
        {
            MissionEntry mission = _allMissions.FirstOrDefault(m => m.missionId == missionId);
            if (mission == null || !mission.CanClaim) return;

            mission.isClaimed = true;
            GrantRewards(mission.rewards);
            RefreshAll();
        }

        /// <summary>현재 탭의 수령 가능한 모든 미션을 일괄 수령합니다.</summary>
        public void ClaimAllMissions()
        {
            List<MissionEntry> claimable = _allMissions
                .Where(m => m.type == _currentTab && m.CanClaim)
                .ToList();

            if (claimable.Count == 0) return;

            List<RewardItem> totalRewards = new();
            foreach (MissionEntry m in claimable)
            {
                m.isClaimed = true;
                if (m.rewards != null)
                {
                    totalRewards.AddRange(m.rewards);
                }
            }

            GrantRewards(totalRewards);
            RefreshAll();
        }

        /// <summary>포인트 티어 보상을 수령합니다.</summary>
        private void ClaimTierReward(int tierIndex)
        {
            if (tierIndex < 0 || tierIndex >= _dailyTiers.Count) return;

            PointRewardTier tier = _dailyTiers[tierIndex];
            int currentPoints = GetCurrentPoints(_currentTab);
            if (currentPoints < tier.requiredPoints || tier.isClaimed) return;

            tier.isClaimed = true;
            GrantRewards(tier.rewards);
            RefreshPointBar();
        }

        // ─────────────────────────────────────────────────────────
        // 포인트 계산
        // ─────────────────────────────────────────────────────────

        /// <summary>해당 미션 타입의 누적 수령 포인트를 반환합니다.</summary>
        public int GetCurrentPoints(MissionType type)
        {
            return _allMissions
                .Where(m => m.type == type && m.isClaimed)
                .Sum(m => m.rewardPoint);
        }

        // ─────────────────────────────────────────────────────────
        // 외부 진행 업데이트
        // ─────────────────────────────────────────────────────────

        /// <summary>외부 시스템에서 미션 조건 진행 시 호출합니다.</summary>
        public void UpdateProgress(MissionCondition condition, int amount)
        {
            List<MissionEntry> matching = _allMissions
                .Where(m => m.condition == condition && !m.isClaimed)
                .ToList();

            foreach (MissionEntry m in matching)
            {
                m.currentCount = Mathf.Min(m.currentCount + amount, m.targetCount);
            }

            if (gameObject.activeInHierarchy)
            {
                RefreshAll();
            }
        }

        // ─────────────────────────────────────────────────────────
        // 보상 지급
        // ─────────────────────────────────────────────────────────

        private void GrantRewards(List<RewardItem> rewards)
        {
            if (rewards == null || _playerData == null) return;

            foreach (RewardItem reward in rewards)
            {
                switch (reward.itemName)
                {
                    case "골드":
                        _playerData.AddGold(reward.amount);
                        break;
                    case "젬":
                        _playerData.AddGem(reward.amount);
                        break;
                }
            }

            _onCurrencyChanged?.RaiseEvent();
        }

        // ─────────────────────────────────────────────────────────
        // 네비게이션
        // ─────────────────────────────────────────────────────────

        private void NavigateToContent(MissionCondition condition)
        {
            // TODO: 조건별 해당 컨텐츠 씬으로 이동
            Debug.Log($"[MissionManager] Navigate to content for condition: {condition}");
        }

        // ─────────────────────────────────────────────────────────
        // 카운트다운 타이머
        // ─────────────────────────────────────────────────────────

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
                _ => DateTime.MaxValue
            };

            if (resetTime == DateTime.MaxValue)
            {
                _resetTimerLabel.text = "";
                return;
            }

            TimeSpan remaining = resetTime - DateTime.Now;
            if (remaining.TotalSeconds <= 0)
            {
                _resetTimerLabel.text = "초기화 중...";
                return;
            }

            _resetTimerLabel.text =
                $"초기화까지 {remaining.Hours:D2}:{remaining.Minutes:D2}:{remaining.Seconds:D2}";
        }

        private static DateTime GetNextMonday()
        {
            DateTime today = DateTime.Today;
            int daysUntilMonday = ((int)DayOfWeek.Monday - (int)today.DayOfWeek + 7) % 7;
            return daysUntilMonday == 0 ? today.AddDays(7) : today.AddDays(daysUntilMonday);
        }
    }
}
