using System;
using UnityEngine;

[Serializable]
public class SkillRow
{
    public string id;
    public string name;
    public ElementType element  = ElementType.Reason;
    public float coefficient    = 1f;
    public float cooldown       = 5f;
    public float range          = 9999f;

    [Tooltip("스킬 설명 (스킬 선택 패널에 표시)")]
    public string description;

    [Tooltip("스킬 아이콘 이미지 (Assets/Project/UI/Icon/*.jpg|png)")]
    public Sprite icon;

    [Tooltip("스킬 발동 시 재생할 VFX 프리팹")]
    public GameObject castVfxPrefab;

    // ── 효과 타입 ──────────────────────────────────────────────────────────
    [Header("효과 타입")]
    [Tooltip("AllEnemies=범위, SingleTarget=단일강타, Buff=버프, Debuff=디버프")]
    public SkillEffectType effectType = SkillEffectType.AllEnemies;

    // ── SingleTarget 설정 ──────────────────────────────────────────────────
    [Header("단일 강타 설정 (effectType = SingleTarget)")]
    [Tooltip("단일 타겟 추가 배율. 최종 데미지 = coefficient × singleTargetBonus")]
    public float singleTargetBonus = 2f;

    // ── 버프 설정 ──────────────────────────────────────────────────────────
    [Header("버프 설정 (effectType = Buff)")]
    public BuffStatType buffStat      = BuffStatType.AttackPower;
    [Tooltip("강화 비율. 0.3 = 30% 증가")]
    [Range(0f, 5f)]
    public float buffMultiplier       = 0.3f;
    [Tooltip("버프 지속 시간 (초)")]
    public float buffDuration         = 10f;

    // ── 디버프 설정 ────────────────────────────────────────────────────────
    [Header("디버프 설정 (effectType = Debuff)")]
    public DebuffType debuffType      = DebuffType.Slow;
    [Tooltip("디버프 강도. 슬로우라면 0.5 = 50% 감소")]
    [Range(0f, 1f)]
    public float debuffValue          = 0.5f;
    [Tooltip("디버프 지속 시간 (초)")]
    public float debuffDuration       = 5f;
}
