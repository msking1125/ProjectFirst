using System.Collections;
using UnityEngine;

/// <summary>
/// Agent에 버프/디버프 효과를 일시적으로 적용하는 컴포넌트.
/// Agent 오브젝트에 자동으로 추가됩니다.
/// </summary>
public class AgentBuffSystem : MonoBehaviour
{
    private Agent agent;

    // 현재 활성 버프 코루틴 (같은 타입 재발동 시 기존 덮어쓰기)
    private Coroutine atkBuffCoroutine;
    private Coroutine defBuffCoroutine;
    private Coroutine spdBuffCoroutine;

    // 버프 수치 추적 (UI 표시용)
    public float AtkBuffMultiplier { get; private set; } = 1f;
    public float DefBuffMultiplier { get; private set; } = 1f;
    public float SpdBuffMultiplier { get; private set; } = 1f;

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

    public bool HasAnyBuff => AtkBuffMultiplier > 1f || DefBuffMultiplier > 1f || SpdBuffMultiplier > 1f;
}
