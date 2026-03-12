using System;
using UnityEngine;

namespace ProjectFirst.Data
{
    [Serializable]
    public class SkillRow
    {
        public int id;

        public string name;

        public ElementType element = ElementType.Reason;

        public float coefficient = 1f;

        public float cooldown = 5f;

        public float range = 9999f;

        [Tooltip("인스펙터에서 설정합니다.")]
        public string description;

        [Tooltip("인스펙터에서 설정합니다.")]
        public Sprite icon;

        [Tooltip("인스펙터에서 설정합니다.")]
        public GameObject castVfxPrefab;

        [Header("Settings")]
        public SkillEffectType effectType = SkillEffectType.AllEnemies;
        [Header("Settings")]
        public float singleTargetBonus = 2f;
        [Header("Settings")]
        public BuffStatType buffStat = BuffStatType.AttackPower;

        [Tooltip("인스펙터에서 설정합니다.")]
        [Range(0f, 5f)]
        public float buffMultiplier = 0.3f;

        [Tooltip("인스펙터에서 설정합니다.")]
        public float buffDuration = 10f;
        [Header("Settings")]
        public DebuffType debuffType = DebuffType.Slow;

        [Tooltip("인스펙터에서 설정합니다.")]
        [Range(0f, 1f)]
        public float debuffValue = 0.5f;

        [Tooltip("인스펙터에서 설정합니다.")]
        public float debuffDuration = 5f;

    }
}

