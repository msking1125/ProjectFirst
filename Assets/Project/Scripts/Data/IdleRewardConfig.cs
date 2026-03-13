using UnityEngine;

namespace ProjectFirst.Data
{
    [CreateAssetMenu(menuName = "Game/Idle Reward Config", fileName = "IdleRewardConfig")]
    public class IdleRewardConfig : ScriptableObject
    {
        [Header("Settings")]
        [Tooltip("인스펙터에서 설정합니다.")]
        public int goldPerHour = 100;

        [Tooltip("인스펙터에서 설정합니다.")]
        public int staminaPerHour = 0;

        [Tooltip("인스펙터에서 설정합니다.")]
        public int gemPerHour = 0;

        [Header("Settings")]
        [Tooltip("인스펙터에서 설정합니다.")]
        [Min(1f)]
        public float maxOfflineHours = 12f;

        [Tooltip("인스펙터에서 설정합니다.")]
        [Min(0f)]
        public float minElapsedSecondsForPopup = 60f;

        [Tooltip("인스펙터에서 설정합니다.")]
        [Min(0f)]
        public float animDuration = 1.5f;
    }
}

