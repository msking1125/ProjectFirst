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

    /// <summary>
    /// 미션 한 건의 데이터를 나타냅니다.
    /// </summary>
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
        public List<RewardItem> rewards { get => _rewards; set => _rewards = value; }
        public bool isClaimed { get => _isClaimed; set => _isClaimed = value; }

        /// <summary>목표 달성 여부를 반환합니다.</summary>
        public bool IsCompleted => _currentCount >= _targetCount;

        /// <summary>보상 수령 가능 여부를 반환합니다.</summary>
        public bool CanClaim => IsCompleted && !_isClaimed;

        /// <summary>진행률을 0~1 범위로 반환합니다.</summary>
        public float Progress => Mathf.Clamp01((float)_currentCount / _targetCount);
    }

    /// <summary>
    /// 포인트 누적 보상 단계 데이터입니다.
    /// </summary>
    [Serializable]
    public class PointRewardTier
    {
        [SerializeField] private int _requiredPoints;
        [SerializeField] private List<RewardItem> _rewards = new();
        [SerializeField] private bool _isClaimed;

        public int requiredPoints { get => _requiredPoints; set => _requiredPoints = value; }
        public List<RewardItem> rewards { get => _rewards; set => _rewards = value; }
        public bool isClaimed { get => _isClaimed; set => _isClaimed = value; }
    }
}
