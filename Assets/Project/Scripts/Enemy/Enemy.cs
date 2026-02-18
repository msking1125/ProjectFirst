using UnityEngine;

/// <summary>
/// Enemy 클래스는 Ark를 향해 이동하고, 피해를 입으면 풀로 반환됩니다.
/// EnemyManager에 자신을 등록/해제하여 활성 적 리스트를 관리합니다.
/// </summary>
public class Enemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    public float moveSpeed = 2f;
    public float maxHP = 10f;

    private float currentHP;
    private Transform target;
    private EnemyPool ownerPool;

    private bool isAlive;

    void Update()
    {
        if (!isAlive || target == null)
        {
            return;
        }

        Vector3 dir = (target.position - transform.position).normalized;
        transform.position += dir * moveSpeed * Time.deltaTime;
    }

    public void SetPool(EnemyPool pool)
    {
        ownerPool = pool;
    }

    public void Init(Transform arkTarget)
    {
        if (arkTarget == null)
        {
            Debug.LogError("[Enemy] Init 실패: arkTarget이 null 입니다.");
            return;
        }

        target = arkTarget;
        currentHP = maxHP;
        isAlive = true;

        if (EnemyManager.Instance == null)
        {
            Debug.LogError("[Enemy] EnemyManager.Instance가 없어 Register할 수 없습니다.");
            return;
        }

        EnemyManager.Instance.Register(this);
    }

    public void TakeDamage(float dmg)
    {
        if (!isAlive)
        {
            return;
        }

        currentHP -= dmg;
        if (currentHP <= 0f)
        {
            Die();
        }
    }

    public void Despawn()
    {
        if (!isAlive)
        {
            return;
        }

        ReturnToPool();
    }

    public void ResetForPool()
    {
        isAlive = false;
        currentHP = maxHP;
        target = null;
    }

    private void Die()
    {
        ReturnToPool();
    }

    private void ReturnToPool()
    {
        if (ownerPool == null)
        {
            Debug.LogError("[Enemy] ownerPool이 없어 Return 할 수 없습니다. 비활성화 처리만 수행합니다.");
            gameObject.SetActive(false);
            return;
        }

        ownerPool.Return(this);
    }

    private void OnDisable()
    {
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.Unregister(this);
        }
    }
}
