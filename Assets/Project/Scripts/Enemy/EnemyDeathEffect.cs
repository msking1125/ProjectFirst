using System;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 몬스터가 사망할 때 물리적으로 날아가고, 작아지면서 사라지는 연출을 담당합니다.
/// Enemy 프리팹에 선택적으로 부착합니다.
/// </summary>
public class EnemyDeathEffect : MonoBehaviour
{
    [Header("날아가는 힘")]
    [Tooltip("충격으로 날아가는 수평 힘입니다. 높을수록 멀리 튕깁니다.")]
    [SerializeField] private float 날아가는힘 = 12f;

    [Header("올라가는 비율(y축 반영)")]
    [Tooltip("위쪽으로 올라가는 힘의 비율입니다. 작을수록 수평으로 낮게 날아갑니다.")]
    [SerializeField] private float 위로올리는계수 = 0.3f;

    [Header("연출 타이밍")]
    [Tooltip("전체 사망 연출(물리+축소) 시간(초)")]
    [SerializeField] private float 연출전체시간 = 1.5f;

    [Tooltip("축소(사라짐) 효과가 시작되는 타이밍 비율 [0.1~0.9]")]
    [Range(0.1f, 0.9f)]
    [SerializeField] private float 축소시작비율 = 0.4f;

    private Rigidbody 리지드;
    private Animator 애니메이터;
    private Collider 콜라이더;
    private Transform 플레이어위치;

    void Awake()
    {
        리지드 = GetComponent<Rigidbody>();
        애니메이터 = GetComponentInChildren<Animator>();
        콜라이더 = GetComponent<Collider>();
    }

    /// <summary>
    /// 공격자(플레이어 등) 방향을 지정합니다.
    /// </summary>
    public void SetTarget(Transform attacker)
    {
        플레이어위치 = attacker;
    }

    /// <summary>
    /// 사망(물리) 연출을 실행합니다. 연출 종료 시 onComplete가 호출됩니다.
    /// </summary>
    public void Play(Action onComplete)
    {
        // 1. 애니메이터 비활성화
        if (애니메이터 != null)
            애니메이터.enabled = false;

        // 2. 콜라이더 비활성화
        if (콜라이더 != null)
            콜라이더.enabled = false;

        // 3. Rigidbody 물리 활성화
        if (리지드 != null)
        {
            리지드.isKinematic = false;

            // 모든 축 회전 고정 — 총알에 맞고 날아가는 연출 (뒤집히지 않음)
            리지드.constraints = RigidbodyConstraints.FreezeRotation;

            // 공격자 반대 방향(XZ 평면)으로 날아감
            Vector3 방향 = Vector3.zero;
            if (플레이어위치 != null)
            {
                Vector3 차이 = transform.position - 플레이어위치.position;
                차이.y = 0f;
                방향 = 차이.normalized;
            }

            // 수평 날아가기 + 약간의 상승 (총알 피격 느낌)
            Vector3 힘 = new Vector3(
                방향.x * 날아가는힘,
                날아가는힘 * 위로올리는계수,
                방향.z * 날아가는힘
            );

            리지드.AddForce(힘, ForceMode.Impulse);
        }

        // 4. 마지막 일정 시간만큼 천천히 축소 → 완전 사라지면 콜백(onComplete)
        float 축소대기시간 = 연출전체시간 * (1f - 축소시작비율);
        float 축소시간 = 연출전체시간 * 축소시작비율;

        transform.DOScale(Vector3.zero, 축소시간)
            .SetDelay(축소대기시간)
            .SetEase(Ease.InBack)
            .OnComplete(() => onComplete?.Invoke());
    }

    /// <summary>
    /// 풀로 반환시 상태를 초기화합니다. Enemy.ResetForPool()에서 호출하세요.
    /// </summary>
    public void ResetState()
    {
        transform.DOKill();
        transform.localScale = Vector3.one;

        if (리지드 != null)
        {
            // velocity/angularVelocity는 kinematic 상태에서 설정 불가
            // → 먼저 dynamic으로 전환 후 초기화, 이후 kinematic 복원
            리지드.isKinematic = false;
            리지드.velocity = Vector3.zero;
            리지드.angularVelocity = Vector3.zero;
            리지드.constraints = RigidbodyConstraints.None;
            리지드.isKinematic = true;
        }

        if (콜라이더 != null)
            콜라이더.enabled = true;
        if (애니메이터 != null)
            애니메이터.enabled = true;
    }
}
