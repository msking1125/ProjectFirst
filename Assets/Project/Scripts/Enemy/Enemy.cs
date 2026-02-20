using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Enemy 클래스는 Ark를 향해 이동하고, 피해를 입으면 풀로 반환됩니다.
/// EnemyManager에 자신을 등록/해제하여 활성 적 리스트를 관리합니다.
/// </summary>
public class Enemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    public float moveSpeed = 2f;
    public float maxHP = 10f;
    public float attackDamage = 1f;

    private float baseMoveSpeed;
    private float baseMaxHP;
    private float baseAttackDamage;

    private float currentHP;
    private Transform target;
    private EnemyPool ownerPool;

    private bool isAlive;
    private bool isRegistered;
    private bool isInPool;

    private Rigidbody cachedRigidbody;
    private NavMeshAgent cachedNavMeshAgent;

    void Awake()
    {
        cachedRigidbody = GetComponent<Rigidbody>();
        cachedNavMeshAgent = GetComponent<NavMeshAgent>();

        baseMoveSpeed = moveSpeed;
        baseMaxHP = maxHP;
        baseAttackDamage = attackDamage;
    }

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

        if (ownerPool == null)
        {
            Debug.LogError("[Enemy] Init 실패: ownerPool이 설정되지 않았습니다.");
            return;
        }

        target = arkTarget;
        currentHP = maxHP;
        isAlive = true;
        isInPool = false;

        if (EnemyManager.Instance == null)
        {
            Debug.LogError("[Enemy] EnemyManager.Instance가 없어 Register할 수 없습니다.");
            return;
        }

        EnemyManager.Instance.Register(this);
        isRegistered = true;
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
            ReturnToPool();
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
        moveSpeed = baseMoveSpeed;
        maxHP = baseMaxHP;
        attackDamage = baseAttackDamage;
        currentHP = maxHP;
        target = null;
        isInPool = true;

        if (isRegistered)
        {
            isRegistered = false;
        }

        ResetMotion();
    }

    public bool IsInPool()
    {
        return isInPool;
    }

    private void ReturnToPool()
    {
        if (ownerPool == null)
        {
            Debug.LogError("[Enemy] ownerPool이 없어 Return 할 수 없습니다. 비활성화 처리만 수행합니다.");
            gameObject.SetActive(false);
            return;
        }

        if (isInPool)
        {
            Debug.LogError("[Enemy] 이미 풀에 반환된 Enemy를 중복 Return 하려고 했습니다.");
            return;
        }

        ownerPool.Return(this);
    }

    private void ResetMotion()
    {
        if (cachedRigidbody != null)
        {
            cachedRigidbody.velocity = Vector3.zero;
            cachedRigidbody.angularVelocity = Vector3.zero;
        }

        if (cachedNavMeshAgent != null)
        {
            cachedNavMeshAgent.ResetPath();
            cachedNavMeshAgent.velocity = Vector3.zero;
        }
    }

    private void OnDisable()
    {
        if (isInPool)
        {
            return;
        }

        if (!isRegistered)
        {
            return;
        }

        if (EnemyManager.Instance == null)
        {
            isRegistered = false;
            return;
        }

        EnemyManager.Instance.Unregister(this);
        isRegistered = false;
    }

    public void OnSpawnedFromPool(Transform arkTarget)
    {
        OnSpawnedFromPool(arkTarget, 1f, 1f, 1f);
    }

    public void OnSpawnedFromPool(Transform arkTarget, float hpMul, float speedMul, float damageMul)
    {
        moveSpeed = baseMoveSpeed * Mathf.Max(0f, speedMul);
        maxHP = baseMaxHP * Mathf.Max(0f, hpMul);
        attackDamage = baseAttackDamage * Mathf.Max(0f, damageMul);
        Init(arkTarget);
    }

    public void OnReturnedToPool()
    {
        if (isRegistered && EnemyManager.Instance != null)
        {
            EnemyManager.Instance.Unregister(this);
            isRegistered = false;
        }

        ResetForPool();
    }
}
