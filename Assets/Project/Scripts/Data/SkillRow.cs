using System;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace ProjectFirst.Data
{
    /// <summary>
    /// 스킬 데이터 행
    /// </summary>
    [Serializable]
#if ODIN_INSPECTOR
    [HideLabel]
#endif
    public class SkillRow
    {
#if ODIN_INSPECTOR
        [HorizontalGroup("기본", 0.33f)]
        [BoxGroup("기본/ID")]
        [LabelText("스킬 ID")]
#endif
        public int id;

#if ODIN_INSPECTOR
        [HorizontalGroup("기본", 0.33f)]
        [BoxGroup("기본/이름")]
        [LabelText("스킬명")]
#endif
        public string name;

#if ODIN_INSPECTOR
        [HorizontalGroup("기본", 0.34f)]
        [BoxGroup("기본/속성")]
        [LabelText("속성")]
        [EnumToggleButtons]
#endif
        public ElementType element = ElementType.Reason;

#if ODIN_INSPECTOR
        [HorizontalGroup("수치", 0.33f)]
        [BoxGroup("수치/계수")]
        [LabelText("계수")]
        [PropertyRange(0.1, 10)]
#endif
        public float coefficient = 1f;

#if ODIN_INSPECTOR
        [HorizontalGroup("수치", 0.33f)]
        [BoxGroup("수치/쿨타임")]
        [LabelText("쿨타임(초)")]
        [SuffixLabel("sec", true)]
#endif
        public float cooldown = 5f;

#if ODIN_INSPECTOR
        [HorizontalGroup("수치", 0.34f)]
        [BoxGroup("수치/사거리")]
        [LabelText("사거리")]
        [SuffixLabel("m", true)]
#endif
        public float range = 9999f;

#if ODIN_INSPECTOR
        [BoxGroup("설명")]
        [LabelText("스킬 설명")]
        [MultiLineProperty(3)]
#else
        [Tooltip("스킬 설명 (스킬 선택 패널에 표시)")]
#endif
        public string description;

#if ODIN_INSPECTOR
        [HorizontalGroup("시각", 0.5f)]
        [BoxGroup("시각/아이콘")]
        [LabelText("아이콘")]
        [PreviewField(70, ObjectFieldAlignment.Left)]
#else
        [Tooltip("스킬 아이콘 이미지")]
#endif
        public Sprite icon;

#if ODIN_INSPECTOR
        [HorizontalGroup("시각", 0.5f)]
        [BoxGroup("시각/VFX")]
        [LabelText("VFX 프리팹")]
        [AssetsOnly]
#else
        [Tooltip("스킬 발동 시 재생할 VFX 프리팹")]
#endif
        public GameObject castVfxPrefab;

#if ODIN_INSPECTOR
        [BoxGroup("효과 설정")]
        [LabelText("효과 타입")]
        [EnumToggleButtons]
        [OnValueChanged("OnEffectTypeChanged")]
#else
        [Header("효과 타입")]
#endif
        public SkillEffectType effectType = SkillEffectType.AllEnemies;

        // ── SingleTarget 설정 ──────────────────────────────────────────────────
#if ODIN_INSPECTOR
        [BoxGroup("효과 설정")]
        [ShowIf("IsSingleTarget")]
        [LabelText("단일 타겟 보너스")]
        [PropertyRange(1, 5)]
        [SuffixLabel("x", true)]
#else
        [Header("단일 강타 설정 (effectType = SingleTarget)")]
#endif
        public float singleTargetBonus = 2f;

        // ── 버프 설정 ──────────────────────────────────────────────────────────
#if ODIN_INSPECTOR
        [BoxGroup("효과 설정")]
        [ShowIf("IsBuff")]
        [LabelText("버프 스탯")]
#else
        [Header("버프 설정 (effectType = Buff)")]
#endif
        public BuffStatType buffStat = BuffStatType.AttackPower;

#if ODIN_INSPECTOR
        [BoxGroup("효과 설정")]
        [ShowIf("IsBuff")]
        [LabelText("버프 배율")]
        [ProgressBar(0, 2, ColorGetter = "GetBuffColor")]
        [SuffixLabel("%", true)]
#else
        [Tooltip("강화 비율. 0.3 = 30% 증가")]
        [Range(0f, 5f)]
#endif
        public float buffMultiplier = 0.3f;

#if ODIN_INSPECTOR
        [BoxGroup("효과 설정")]
        [ShowIf("IsBuff")]
        [LabelText("버프 지속시간")]
        [SuffixLabel("sec", true)]
#else
        [Tooltip("버프 지속 시간 (초)")]
#endif
        public float buffDuration = 10f;

        // ── 디버프 설정 ────────────────────────────────────────────────────────
#if ODIN_INSPECTOR
        [BoxGroup("효과 설정")]
        [ShowIf("IsDebuff")]
        [LabelText("디버프 타입")]
#else
        [Header("디버프 설정 (effectType = Debuff)")]
#endif
        public DebuffType debuffType = DebuffType.Slow;

#if ODIN_INSPECTOR
        [BoxGroup("효과 설정")]
        [ShowIf("IsDebuff")]
        [LabelText("디버프 강도")]
        [ProgressBar(0, 1, ColorGetter = "GetDebuffColor")]
#else
        [Tooltip("디버프 강도. 슬로우라면 0.5 = 50% 감소")]
        [Range(0f, 1f)]
#endif
        public float debuffValue = 0.5f;

#if ODIN_INSPECTOR
        [BoxGroup("효과 설정")]
        [ShowIf("IsDebuff")]
        [LabelText("디버프 지속시간")]
        [SuffixLabel("sec", true)]
#else
        [Tooltip("디버프 지속 시간 (초)")]
#endif
        public float debuffDuration = 5f;

#if ODIN_INSPECTOR
        private bool IsSingleTarget => effectType == SkillEffectType.SingleTarget;
        private bool IsBuff => effectType == SkillEffectType.Buff;
        private bool IsDebuff => effectType == SkillEffectType.Debuff;
        private static Color GetBuffColor() => new Color(0.3f, 0.8f, 0.3f);
        private static Color GetDebuffColor() => new Color(0.8f, 0.3f, 0.3f);
        private void OnEffectTypeChanged() { }
#endif
    }
}
