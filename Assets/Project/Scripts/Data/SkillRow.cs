using System;
using UnityEngine;

namespace ProjectFirst.Data
{
    /// <summary>
    /// 스킬 데이터 행
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

        [Tooltip("스킬 설명 (스킬 선택 패널에 표시)")]
        public string description;

        [Tooltip("스킬 아이콘 이미지")]
        public Sprite icon;

        [Tooltip("스킬 발동 시 재생할 VFX 프리팹")]
        public GameObject castVfxPrefab;

        [Header("효과 타입")]
        public SkillEffectType effectType = SkillEffectType.AllEnemies;

        // ── SingleTarget 설정 ──────────────────────────────────────────────────
        [Header("단일 강타 설정 (effectType = SingleTarget)")]
        public float singleTargetBonus = 2f;

        // ── 버프 설정 ──────────────────────────────────────────────────────────
        [Header("버프 설정 (effectType = Buff)")]
        public BuffStatType buffStat = BuffStatType.AttackPower;

        [Tooltip("강화 비율. 0.3 = 30% 증가")]
        [Range(0f, 5f)]
        public float buffMultiplier = 0.3f;

        [Tooltip("버프 지속 시간 (초)")]
        public float buffDuration = 10f;

        // ── 디버프 설정 ────────────────────────────────────────────────────────
        [Header("디버프 설정 (effectType = Debuff)")]
        public DebuffType debuffType = DebuffType.Slow;

        [Tooltip("디버프 강도. 슬로우라면 0.5 = 50% 감소")]
        [Range(0f, 1f)]
        public float debuffValue = 0.5f;

        [Tooltip("디버프 지속 시간 (초)")]
        public float debuffDuration = 5f;

    }
}
