using System;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 몬스터 사망 시 물리 기반 날아가기 + 축소 소멸 효과.
/// Enemy 프리팹에 선택적으로 부착. 없으면 기존 Death 애니 동작 유지.
/// </summary>
public class EnemyDeathEffect : MonoBehaviour
{
    [Header("Physics Launch")]
    [SerializeField] private float flyForce    = 5f;
    [SerializeField] private float upwardBias  = 1.5f;

    [Header("Timing")]
    [SerializeField] private float ragdollDuration = 1.5f;
    [Range(0.1f, 0.9f)]
    [SerializeField] private float shrinkStartRatio = 0.4f; // 총 시간 중 마지막 N% 구간에서 축소

    private Rigidbody rb;
    private Animator  anim;
    private Collider  col;

    void Awake()
    {
        rb   = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
        col  = GetComponent<Collider>();
    }

    /// <summary>
    /// 사망 연출 재생. 완료되면 onComplete 호출 (→ ReturnToPool).
    /// </summary>
    public void Play(Action onComplete)
    {
        // 1. 애니메이터 정지
        if (anim != null) anim.enabled = false;

        // 2. 콜라이더 비활성 (베이스 재트리거 방지)
        if (col != null) col.enabled = false;

        // 3. Rigidbody 비키네마틱 전환 + 임펄스 & 토크
        if (rb != null)
        {
            rb.isKinematic = false;
            Vector3 force = new Vector3(
                UnityEngine.Random.Range(-flyForce, flyForce),
                flyForce * upwardBias,
                UnityEngine.Random.Range(-flyForce * 0.5f, flyForce * 0.5f)
            );
            rb.AddForce(force, ForceMode.Impulse);
            rb.AddTorque(UnityEngine.Random.insideUnitSphere * flyForce * 2f, ForceMode.Impulse);
        }

        // 4. 지연 후 스케일 0으로 축소 → 완료 시 풀 반환 콜백
        float shrinkDelay    = ragdollDuration * (1f - shrinkStartRatio);
        float shrinkDuration = ragdollDuration * shrinkStartRatio;

        transform.DOScale(Vector3.zero, shrinkDuration)
            .SetDelay(shrinkDelay)
            .SetEase(Ease.InBack)
            .OnComplete(() => onComplete?.Invoke());
    }

    /// <summary>
    /// 풀 반환 시 상태 초기화. Enemy.ResetForPool()에서 호출.
    /// </summary>
    public void ResetState()
    {
        transform.DOKill();
        transform.localScale = Vector3.one;

        if (rb != null)
        {
            rb.velocity        = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic     = true;
        }

        if (col  != null) col.enabled  = true;
        if (anim != null) anim.enabled = true;
    }
}
