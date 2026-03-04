using System;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 몬스터 사망 시 물리적으로 날아가며 축소되어 사라지는 연출 담당.
/// Enemy 프리팹에 부착(선택).
/// </summary>
public class EnemyDeathEffect : MonoBehaviour
{
    [Header("날아가는 힘 (수평)")]
    [Tooltip("충격으로 날아가는 수평 힘. 클수록 멀리 튕김.")]
    [SerializeField] private float flyPower = 12f;

    [Header("올라가는 계수 (Y축)")]
    [Tooltip("위쪽 상승 힘 계수. 작으면 수평에 가깝게 날아감.")]
    [SerializeField] private float upwardRatio = 0.3f;

    [Header("연출 타이밍")]
    [Tooltip("전체 사망 연출(물리+축소) 시간(초)")]
    [SerializeField] private float totalDuration = 1.5f;

    [Tooltip("축소 시작 타이밍 비율 [0.1~0.9]")]
    [Range(0.1f, 0.9f)] [SerializeField] private float shrinkStartRatio = 0.4f;

    // 캐싱용
    private Rigidbody _rb;
    private Animator _anim;
    private Collider _collider;
    private Transform _attacker;

    private static readonly Vector3 VectorZero = Vector3.zero;
    private static readonly Vector3 VectorOne = Vector3.one;

    private void Awake()
    {
        // 비싼 GetComponent 캐싱
        _rb = GetComponent<Rigidbody>();
        _anim = GetComponentInChildren<Animator>();
        _collider = GetComponent<Collider>();
    }

    /// <summary>
    /// 공격자 대상 transform 지정
    /// </summary>
    public void SetTarget(Transform attacker)
    {
        _attacker = attacker;
    }

    /// <summary>
    /// 사망 연출 실행. 연출 종료 시 onComplete 호출.
    /// </summary>
    public void Play(Action onComplete)
    {
        // 애니메이터/콜라이더 비활성화
        if (_anim) _anim.enabled = false;
        if (_collider) _collider.enabled = false;

        // Rigidbody 활성화 및 연출 처리
        if (_rb)
        {
            _rb.isKinematic = false;
            _rb.constraints = RigidbodyConstraints.FreezeRotation;

            // 반대 방향(XZ 평면)으로 날아감, Y는 0으로
            Vector3 direction = VectorZero;
            if (_attacker)
            {
                direction = (transform.position - _attacker.position);
                direction.y = 0f;
                if (direction.sqrMagnitude > 0.01f)
                    direction.Normalize();
                else
                    direction = Vector3.forward; // fallback 방향
            }

            // 적용할 물리 힘 계산
            Vector3 force = new Vector3(
                direction.x * flyPower,
                flyPower * upwardRatio,
                direction.z * flyPower
            );
            _rb.velocity = VectorZero;
            _rb.angularVelocity = VectorZero;
            _rb.AddForce(force, ForceMode.Impulse);
        }

        // 연출 타이밍 계산 및 DOTween 축소
        float shrinkDelay = totalDuration * (1f - shrinkStartRatio);
        float shrinkDuration = totalDuration * shrinkStartRatio;

        // 기존 Tween kill 방지 및 onComplete 보장
        transform.DOKill();
        transform.DOScale(VectorZero, shrinkDuration)
            .SetDelay(shrinkDelay)
            .SetEase(Ease.InBack)
            .OnComplete(() => onComplete?.Invoke());
    }

    /// <summary>
    /// 풀 반환(재사용) 시 상태 초기화. Enemy.ResetForPool()에서 호출 필요.
    /// </summary>
    public void ResetState()
    {
        // Tween 모두 해제, 스케일 복원
        transform.DOKill();
        transform.localScale = VectorOne;

        // Rigidbody/Collider/Animator 활성화 및 초기화
        if (_rb)
        {
            // dynamic 전환 후 속도 초기화, 다시 kinematic
            _rb.isKinematic = false;
            _rb.velocity = VectorZero;
            _rb.angularVelocity = VectorZero;
            _rb.constraints = RigidbodyConstraints.None;
            _rb.isKinematic = true;
        }

        if (_collider) _collider.enabled = true;
        if (_anim) _anim.enabled = true;

        // 공격자 캐싱 리셋
        _attacker = null;
    }
}
