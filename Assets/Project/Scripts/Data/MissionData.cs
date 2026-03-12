using System;
using System.Collections.Generic;
using UnityEngine;

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
    public class MissionEntry
    {
        [SerializeField] private string _missionId;
        [SerializeField] private MissionType _type;
        [SerializeField] private MissionCondition _condition;
        [SerializeField] private string _title;
        [SerializeField] private string _description;
        [SerializeField] private int _targetCount;
        [SerializeField] private int _currentCount;
        [SerializeField] private int _rewardPoint;
        [SerializeField] private List<RewardItem> _rewards = new();
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
    public class PointRewardTier
    {
        [SerializeField] private int _requiredPoints;
        [SerializeField] private List<RewardItem> _rewards = new();
        [SerializeField] private bool _isClaimed;

        public int requiredPoints { get => _requiredPoints; set => _requiredPoints = Mathf.Max(0, value); }
        public List<RewardItem> rewards { get => _rewards; set => _rewards = value ?? new List<RewardItem>(); }
        public bool isClaimed { get => _isClaimed; set => _isClaimed = value; }
    }

    [Serializable]
    public class MissionProgressRecord
    {
        [SerializeField] private string _missionId;
        [SerializeField] private int _currentCount;
        [SerializeField] private bool _isClaimed;

        public string missionId { get => _missionId; set => _missionId = value; }
        public int currentCount { get => _currentCount; set => _currentCount = Mathf.Max(0, value); }
        public bool isClaimed { get => _isClaimed; set => _isClaimed = value; }
    }

    [Serializable]
    public class MissionTierClaimRecord
    {
        [SerializeField] private MissionType _missionType;
        [SerializeField] private int _requiredPoints;
        [SerializeField] private bool _isClaimed;

        public MissionType missionType { get => _missionType; set => _missionType = value; }
        public int requiredPoints { get => _requiredPoints; set => _requiredPoints = Mathf.Max(0, value); }
        public bool isClaimed { get => _isClaimed; set => _isClaimed = value; }
    }
}

