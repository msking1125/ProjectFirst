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
        if (!isRegistered)
        {
            return;
        }

        if (EnemyManager.Instance == null)
        {
            Debug.LogError("[Enemy] EnemyManager.Instance가 없어 Unregister할 수 없습니다.");
            isRegistered = false;
            return;
        }

        EnemyManager.Instance.Unregister(this);
        isRegistered = false;
    }
}
