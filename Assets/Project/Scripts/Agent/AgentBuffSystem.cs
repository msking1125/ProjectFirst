using System.Collections;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Project
{

/// <summary>
/// Agent에 버프/디버프 효과를 일시적으로 적용하는 컴포넌트.
/// Agent 오브젝트에 자동으로 추가됩니다.
/// </summary>
#if ODIN_INSPECTOR
    [HideMonoScript]
#endif
    public class AgentBuffSystem : MonoBehaviour
    {
        private Project.Agent agent;

        // 현재 활성 버프 코루틴 (같은 타입 재발동 시 기존 덮어쓰기)
        private Coroutine atkBuffCoroutine;
        private Coroutine defBuffCoroutine;
        private Coroutine spdBuffCoroutine;

        // 버프 수치 추적 (UI 표시용)
#if ODIN_INSPECTOR
        [ShowInInspector, ReadOnly]
        [BoxGroup("버프")]
        [LabelText("공격력 배율")]
        [ProgressBar(1f, 3f, ColorGetter = "GetAtkColor")]
#endif
        public float AtkBuffMultiplier { get; private set; } = 1f;

#if ODIN_INSPECTOR
        [ShowInInspector, ReadOnly]
        [BoxGroup("버프")]
        [LabelText("방어력 배율")]
        [ProgressBar(1f, 3f, ColorGetter = "GetDefColor")]
#endif
        public float DefBuffMultiplier { get; private set; } = 1f;

#if ODIN_INSPECTOR
        [ShowInInspector, ReadOnly]
        [BoxGroup("버프")]
        [LabelText("공격속도 배율")]
        [ProgressBar(1f, 3f, ColorGetter = "GetSpdColor")]
#endif
        public float SpdBuffMultiplier { get; private set; } = 1f;

#if ODIN_INSPECTOR
        private static Color GetAtkColor() => new Color(1f, 0.4f, 0.4f);
        private static Color GetDefColor() => new Color(0.4f, 0.4f, 1f);
        private static Color GetSpdColor() => new Color(0.4f, 1f, 0.4f);
#endif

        public event System.Action OnBuffChanged;

        private void Awake()
        {
            agent = GetComponent<Agent>();
        }

        // ── 버프 적용 API ─────────────────────────────────────────────────────────

        public void ApplyBuff(BuffStatType stat, float multiplier, float duration)
        {
            switch (stat)
            {
                case BuffStatType.AttackPower:
                    if (atkBuffCoroutine != null) StopCoroutine(atkBuffCoroutine);
                    atkBuffCoroutine = StartCoroutine(BuffRoutine(stat, multiplier, duration));
                    break;
                case BuffStatType.Defense:
                    if (defBuffCoroutine != null) StopCoroutine(defBuffCoroutine);
                    defBuffCoroutine = StartCoroutine(BuffRoutine(stat, multiplier, duration));
                    break;
                case BuffStatType.AttackSpeed:
                    if (spdBuffCoroutine != null) StopCoroutine(spdBuffCoroutine);
                    spdBuffCoroutine = StartCoroutine(BuffRoutine(stat, multiplier, duration));
                    break;
            }
        }

        private IEnumerator BuffRoutine(BuffStatType stat, float multiplier, float duration)
        {
            SetBuff(stat, 1f + multiplier);
            Debug.Log($"[AgentBuff] {stat} +{multiplier*100:F0}% 버프 시작 ({duration}s)");

            yield return new WaitForSecondsRealtime(duration);

            SetBuff(stat, 1f);
            Debug.Log($"[AgentBuff] {stat} 버프 종료");
        }

        private void SetBuff(BuffStatType stat, float value)
        {
            switch (stat)
            {
                case BuffStatType.AttackPower: AtkBuffMultiplier = value; break;
                case BuffStatType.Defense:     DefBuffMultiplier = value; break;
                case BuffStatType.AttackSpeed: SpdBuffMultiplier = value; break;
            }
            OnBuffChanged?.Invoke();
        }

        /// <summary>버프 배율을 적용한 실제 공격력 반환</summary>
        public float GetBuffedAttackPower()
        {
            if (agent == null) return 0f;
            return agent.AttackPower * AtkBuffMultiplier;
        }

#if ODIN_INSPECTOR
        [ShowInInspector, ReadOnly]
        [BoxGroup("버프")]
        [LabelText("버프 활성")]
        [ToggleLeft]
        [GUIColor(0.3f, 0.8f, 0.3f)]
#endif
        public bool HasAnyBuff => AtkBuffMultiplier > 1f || DefBuffMultiplier > 1f || SpdBuffMultiplier > 1f;
    }
} // namespace Project
