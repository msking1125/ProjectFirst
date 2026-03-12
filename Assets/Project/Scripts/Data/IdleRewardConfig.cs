using UnityEngine;

namespace ProjectFirst.Data
{
    /// <summary>
    /// Documentation cleaned.
    /// Documentation cleaned.
    /// Documentation cleaned.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Idle Reward Config", fileName = "IdleRewardConfig")]
    public class IdleRewardConfig : ScriptableObject
    {
        [Header("Settings")]
        [Tooltip("Configured in inspector.")]
        public int goldPerHour = 100;

        [Tooltip("Configured in inspector.")]
        public int ticketPerHour = 0;

        [Tooltip("Configured in inspector.")]
        public int diamondPerHour = 0;

        [Header("Settings")]
        [Tooltip("Configured in inspector.")]
        [Min(1f)]
        public float maxOfflineHours = 12f;

        [Tooltip("Configured in inspector.")]
        [Min(0f)]
        public float minElapsedSecondsForPopup = 60f;

        [Tooltip("Configured in inspector.")]
        [Min(0f)]
        public float animDuration = 1.5f;
    }
}
