using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Enemy 클래스는 Ark를 향해 이동하고, 피해를 입으면 죽음 처리가 됩니다.
/// EnemyManager에 자신을 등록/해제하여 활성 적 리스트를 관리합니다.
/// 
/// Enemy 사용 가이드:
/// 1. EnemySpawner에서 Instantiate 후 Init(Transform target)으로 타겟 지정.
/// 2. Enemy는 Init을 통해 Ark Target을 받아 초기화.
/// 3. TakeDamage로 데미지 입힘.
/// 4. 체력이 0 이하가 되면 비활성화되고 EnemyManager에서 자동 해제.
/// </summary>
public class Enemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    public float moveSpeed = 2f;         // 적 이동 속도
    public float maxHP = 10f;            // 최대 체력

    private float currentHP;             // 현재 체력
    private Transform target;            // 이동 타겟(Ark)

    /// <summary>
    /// 반드시 스폰 후 타겟을 지정할 것!
    /// </summary>
    /// <param name="arkTarget">공격 타겟(Ark)</param>
    void Update()
    {
        // 타겟이 없을 시 이동 중지
        if (target == null) return;

        // 타겟 방향으로 이동
        Vector3 dir = (target.position - transform.position).normalized;
        transform.position += dir * moveSpeed * Time.deltaTime;
    }

    /// <summary>
    /// 피해를 입는다. 체력이 0 이하가 되면 비활성화.
    /// </summary>
    /// <param name="dmg">입는 데미지</param>
    public void TakeDamage(float dmg)
    {
        currentHP -= dmg;
        if (currentHP <= 0f)
        {
            gameObject.SetActive(false);
        }
    }
public void Init(Transform arkTarget)
{
    target = arkTarget;
    currentHP = maxHP;

    // ✅ 스폰되자마자 등록
    EnemyManager.Instance.Register(this);
}

private void OnDisable()
{
    // ✅ 죽거나 비활성화될 때 해제
    if (EnemyManager.Instance != null)
        EnemyManager.Instance.Unregister(this);
}

}
