using System;
using UnityEngine;

namespace ProjectFirst.Data
{
    /// <summary>
    /// Documentation cleaned.
    /// </summary>
    [Serializable]
    public class SkillRow
    {
        public int id;

        public string name;

        public ElementType element = ElementType.Reason;

        public float coefficient = 1f;

        public float cooldown = 5f;

        public float range = 9999f;

        [Tooltip("Configured in inspector.")]
        public string description;

        [Tooltip("Configured in inspector.")]
        public Sprite icon;

        [Tooltip("Configured in inspector.")]
        public GameObject castVfxPrefab;

        [Header("Settings")]
        public SkillEffectType effectType = SkillEffectType.AllEnemies;

        // Note: cleaned comment.
        [Header("Settings")]
        public float singleTargetBonus = 2f;

        // Note: cleaned comment.
        [Header("Settings")]
        public BuffStatType buffStat = BuffStatType.AttackPower;

        [Tooltip("Configured in inspector.")]
        [Range(0f, 5f)]
        public float buffMultiplier = 0.3f;

        [Tooltip("Configured in inspector.")]
        public float buffDuration = 10f;

        // Note: cleaned comment.
        [Header("Settings")]
        public DebuffType debuffType = DebuffType.Slow;

        [Tooltip("Configured in inspector.")]
        [Range(0f, 1f)]
        public float debuffValue = 0.5f;

        [Tooltip("Configured in inspector.")]
        public float debuffDuration = 5f;

    }
}
