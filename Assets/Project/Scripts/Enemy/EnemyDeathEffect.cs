using System;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 몬스터가 사망할 때 물리적으로 날아가고, 작아지면서 사라지는 연출을 담당합니다.
/// Enemy 프리팹에 선택적으로 부착합니다.
/// </summary>
public class EnemyDeathEffect : MonoBehaviour
{
    [Header("물리 날아가기(튕김) 세기")]
    [Tooltip("몬스터가 날아가는 힘의 크기입니다. 높을수록 멀리 튕깁니다.")]
    [SerializeField] private float 날아가는힘 = 5f;

    [Header("올라가는 비율(y축 반영)")]
    [Tooltip("위쪽으로 올라가는 힘의 비율입니다. 클수록 더 높이 뜹니다.")]
    [SerializeField] private float 위로올리는계수 = 1.5f;
    
    [Header("연출 타이밍")]
    [Tooltip("전체 사망 연출(물리+축소) 시간(초)")]
    [SerializeField] private float 연출전체시간 = 1.5f;

    [Tooltip("축소(사라짐) 효과가 시작되는 타이밍 비율 [0.1~0.9]")]
    [Range(0.1f, 0.9f)]
    [SerializeField] private float 축소시작비율 = 0.4f; // 마지막 N%에서 축소 시작
    
    /// <summary>
    /// • Agent(플레이어)가 가까이 있으면 그 반대 방향으로 날아갑니다.
    /// • Enemy 프리팹에 이 컴포넌트가 있으면 물리 죽음 연출이 실행됩니다.
    /// </summary>

    private Rigidbody 리지드;
    private Animator 애니메이터;
    private Collider 콜라이더;
    private Transform 플레이어위치; // 공격자

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
    /// <param name="onComplete">연출이 완전히 끝났을 때 호출할 콜백</param>
    public void Play(Action onComplete)
    {
        // 1. 애니메이터 비활성화(기존 애니 동작 정지)
        if (애니메이터 != null)
            애니메이터.enabled = false;

        // 2. 콜라이더 비활성화(중복 처리 방지, 베이스 재트리거 차단)
        if (콜라이더 != null)
            콜라이더.enabled = false;

        // 3. Rigidbody 물리 활성화 + 힘/회전 적용
        if (리지드 != null)
        {
            리지드.isKinematic = false;

            // 공격자 반대 방향(XZ 평면)으로 날아감
            Vector3 방향 = Vector3.zero;
            if (플레이어위치 != null)
            {
                Vector3 차이 = transform.position - 플레이어위치.position;
                차이.y = 0f;
                방향 = 차이.normalized;
            }

            Vector3 힘 = new Vector3(
                방향.x * 날아가는힘,
                날아가는힘 * 위로올리는계수,
                방향.z * 날아가는힘
            );

            리지드.AddForce(힘, ForceMode.Impulse);
            리지드.AddTorque(UnityEngine.Random.insideUnitSphere * 날아가는힘 * 2f, ForceMode.Impulse);
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
            리지드.velocity = Vector3.zero;
            리지드.angularVelocity = Vector3.zero;
            리지드.isKinematic = true;
        }

        if (콜라이더 != null)
            콜라이더.enabled = true;
        if (애니메이터 != null)
            애니메이터.enabled = true;
    }
}
