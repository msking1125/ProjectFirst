using System;
using System.Collections.Generic;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace ProjectFirst.Data
{
    public enum MissionType
    {
        Daily,
        Weekly,
        Repeat,
        Achievement
    }

    public enum MissionCondition
    {
        StageCleared,
        EnemyKilled,
        ItemObtained,
        LevelReached,
        LoginCount
    }

    [Serializable]
#if ODIN_INSPECTOR
    [HideLabel]
#endif
    public class MissionEntry
    {
#if ODIN_INSPECTOR
        [HorizontalGroup("기본", 0.5f)]
        [BoxGroup("기본/ID")]
        [LabelText("미션 ID")]
#endif
        [SerializeField] private string _missionId;

#if ODIN_INSPECTOR
        [HorizontalGroup("기본", 0.5f)]
        [BoxGroup("기본/타입")]
        [LabelText("타입")]
        [EnumToggleButtons]
#endif
        [SerializeField] private MissionType _type;

#if ODIN_INSPECTOR
        [BoxGroup("조건")]
        [LabelText("조건")]
        [EnumToggleButtons]
#endif
        [SerializeField] private MissionCondition _condition;

#if ODIN_INSPECTOR
        [BoxGroup("정보")]
        [LabelText("제목")]
#endif
        [SerializeField] private string _title;

#if ODIN_INSPECTOR
        [BoxGroup("정보")]
        [LabelText("설명")]
        [MultiLineProperty(2)]
#endif
        [SerializeField] private string _description;

#if ODIN_INSPECTOR
        [HorizontalGroup("진행", 0.5f)]
        [BoxGroup("진행/목표")]
        [LabelText("목표 수")]
        [ProgressBar(1, 100)]
#endif
        [SerializeField] private int _targetCount;

#if ODIN_INSPECTOR
        [HorizontalGroup("진행", 0.5f)]
        [BoxGroup("진행/현재")]
        [LabelText("현재 수")]
        [ProgressBar(0, "_targetCount")]
#endif
        [SerializeField] private int _currentCount;

#if ODIN_INSPECTOR
        [BoxGroup("보상")]
        [LabelText("보상 포인트")]
#endif
        [SerializeField] private int _rewardPoint;

#if ODIN_INSPECTOR
        [BoxGroup("보상")]
        [LabelText("보상 아이템")]
        [TableList(ShowIndexLabels = true, AlwaysExpanded = false)]
#endif
        [SerializeField] private List<RewardItem> _rewards = new();

#if ODIN_INSPECTOR
        [BoxGroup("상태")]
        [LabelText("수령 완료")]
        [ToggleLeft]
        [GUIColor(0.3f, 0.8f, 0.3f)]
#endif
        [SerializeField] private bool _isClaimed;

        public string missionId { get => _missionId; set => _missionId = value; }
        public MissionType type { get => _type; set => _type = value; }
        public MissionCondition condition { get => _condition; set => _condition = value; }
        public string title { get => _title; set => _title = value; }
        public string description { get => _description; set => _description = value; }
        public int targetCount { get => _targetCount; set => _targetCount = Mathf.Max(1, value); }
        public int currentCount { get => _currentCount; set => _currentCount = Mathf.Max(0, value); }
        public int rewardPoint { get => _rewardPoint; set => _rewardPoint = value; }
        public List<RewardItem> rewards { get => _rewards; set => _rewards = value ?? new List<RewardItem>(); }
        public bool isClaimed { get => _isClaimed; set => _isClaimed = value; }

        public bool IsCompleted => _currentCount >= _targetCount;
        public bool CanClaim => IsCompleted && !_isClaimed;
        public float Progress => Mathf.Clamp01((float)_currentCount / _targetCount);
    }

    [Serializable]
#if ODIN_INSPECTOR
    [HideLabel]
#endif
    public class PointRewardTier
    {
#if ODIN_INSPECTOR
        [HorizontalGroup("티어", 0.5f)]
        [BoxGroup("티어/포인트")]
        [LabelText("필요 포인트")]
        [ProgressBar(0, 1000)]
#endif
        [SerializeField] private int _requiredPoints;

#if ODIN_INSPECTOR
        [BoxGroup("티어")]
        [LabelText("보상")]
        [TableList(ShowIndexLabels = true, AlwaysExpanded = false)]
#endif
        [SerializeField] private List<RewardItem> _rewards = new();

#if ODIN_INSPECTOR
        [HorizontalGroup("티어", 0.5f)]
        [BoxGroup("티어/상태")]
        [LabelText("수령 완료")]
        [ToggleLeft]
#endif
        [SerializeField] private bool _isClaimed;

        public int requiredPoints { get => _requiredPoints; set => _requiredPoints = Mathf.Max(0, value); }
        public List<RewardItem> rewards { get => _rewards; set => _rewards = value ?? new List<RewardItem>(); }
        public bool isClaimed { get => _isClaimed; set => _isClaimed = value; }
    }

    [Serializable]
#if ODIN_INSPECTOR
    [HideLabel]
#endif
    public class MissionProgressRecord
    {
#if ODIN_INSPECTOR
        [HorizontalGroup("진행", 0.5f)]
        [BoxGroup("진행/ID")]
        [LabelText("미션 ID")]
#endif
        [SerializeField] private string _missionId;

#if ODIN_INSPECTOR
        [HorizontalGroup("진행", 0.5f)]
        [BoxGroup("진행/카운트")]
        [LabelText("현재 카운트")]
#endif
        [SerializeField] private int _currentCount;

#if ODIN_INSPECTOR
        [BoxGroup("진행")]
        [LabelText("수령 완료")]
        [ToggleLeft]
#endif
        [SerializeField] private bool _isClaimed;

        public string missionId { get => _missionId; set => _missionId = value; }
        public int currentCount { get => _currentCount; set => _currentCount = Mathf.Max(0, value); }
        public bool isClaimed { get => _isClaimed; set => _isClaimed = value; }
    }

    [Serializable]
#if ODIN_INSPECTOR
    [HideLabel]
#endif
    public class MissionTierClaimRecord
    {
#if ODIN_INSPECTOR
        [HorizontalGroup("티어", 0.5f)]
        [BoxGroup("티어/타입")]
        [LabelText("미션 타입")]
        [EnumToggleButtons]
#endif
        [SerializeField] private MissionType _missionType;

#if ODIN_INSPECTOR
        [HorizontalGroup("티어", 0.5f)]
        [BoxGroup("티어/포인트")]
        [LabelText("필요 포인트")]
#endif
        [SerializeField] private int _requiredPoints;

#if ODIN_INSPECTOR
        [BoxGroup("티어")]
        [LabelText("수령 완료")]
        [ToggleLeft]
#endif
        [SerializeField] private bool _isClaimed;

        public MissionType missionType { get => _missionType; set => _missionType = value; }
        public int requiredPoints { get => _requiredPoints; set => _requiredPoints = Mathf.Max(0, value); }
        public bool isClaimed { get => _isClaimed; set => _isClaimed = value; }
    }
}
