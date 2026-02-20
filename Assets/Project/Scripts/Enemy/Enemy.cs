using UnityEngine;
using UnityEngine.AI;
using Sirenix.OdinInspector;

/// <summary>
/// Enemy 클래스는 Ark를 향해 이동하고, 피해를 입으면 풀로 반환됩니다.
/// EnemyManager에 자신을 등록/해제하여 활성 적 리스트를 관리합니다.
/// </summary>
public class Enemy : MonoBehaviour
{
    [ShowInInspector, ReadOnly]
    private float CurrentHP => currentHP;

    [ShowInInspector, ReadOnly]
    private CombatStats Stats => currentCombatStats;
    [Header("Enemy Stats")]
    public float moveSpeed = 2f;
    public float maxHP = 10f;
    public float attackDamage = 1f;
    [SerializeField] private string defaultMonsterId = "slime";

    [Header("Attack")]
    [SerializeField] private float arriveDistance = 0.6f;

    private float baseMoveSpeed;
    private CombatStats baseCombatStats;
    private CombatStats currentCombatStats;

    private float currentHP;
    private Transform target;
    private EnemyPool ownerPool;
    private BaseHealth targetBaseHealth;

    private bool isAlive;
    private bool isRegistered;
    private bool isInPool;

    private Rigidbody cachedRigidbody;
    private NavMeshAgent cachedNavMeshAgent;

    public float Defense => currentCombatStats.def;

    void Awake()
    {
        cachedRigidbody = GetComponent<Rigidbody>();
        cachedNavMeshAgent = GetComponent<NavMeshAgent>();

        baseMoveSpeed = moveSpeed;
        baseCombatStats = new CombatStats(maxHP, attackDamage, 0f, 0f, 1f).Sanitized();
        currentCombatStats = baseCombatStats;
    }

    void Update()
    {
        if (!isAlive || target == null)
        {
            return;
        }

        Vector3 dir = (target.position - transform.position).normalized;
        transform.position += dir * moveSpeed * Time.deltaTime;

        float sqrDistance = (target.position - transform.position).sqrMagnitude;
        if (sqrDistance <= arriveDistance * arriveDistance)
        {
            AttackBaseAndDespawn();
        }
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
        targetBaseHealth = target.GetComponent<BaseHealth>();
        currentHP = currentCombatStats.hp;
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

    public void TakeDamage(int dmg)
    {
        if (!isAlive)
        {
            return;
        }

        currentHP -= Mathf.Max(0, dmg);
        if (currentHP <= 0f)
        {
            ReturnToPool();
        }
    }

    public void TakeDamage(float dmg)
    {
        TakeDamage(Mathf.RoundToInt(dmg));
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
        currentCombatStats = baseCombatStats;
        maxHP = currentCombatStats.hp;
        attackDamage = currentCombatStats.atk;
        currentHP = currentCombatStats.hp;
        target = null;
        targetBaseHealth = null;
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

    private void AttackBaseAndDespawn()
    {
        if (targetBaseHealth == null && target != null)
        {
            targetBaseHealth = target.GetComponent<BaseHealth>();
        }

        if (targetBaseHealth != null)
        {
            targetBaseHealth.TakeDamage(Mathf.Max(0f, currentCombatStats.atk));
        }

        ReturnToPool();
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
        OnSpawnedFromPool(arkTarget, null, defaultMonsterId, 1f, 1f, 1f);
    }

    public void OnSpawnedFromPool(Transform arkTarget, MonsterTable monsterTable, string enemyId, float hpMul, float speedMul, float damageMul)
    {
        ApplyMonsterBase(monsterTable, enemyId);

        moveSpeed = baseMoveSpeed * Mathf.Max(0f, speedMul);
        currentCombatStats = new CombatStats(
            baseCombatStats.hp * Mathf.Max(0f, hpMul),
            baseCombatStats.atk * Mathf.Max(0f, damageMul),
            baseCombatStats.def,
            baseCombatStats.critChance,
            baseCombatStats.critMultiplier);

        maxHP = currentCombatStats.hp;
        attackDamage = currentCombatStats.atk;
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

    private void ApplyMonsterBase(MonsterTable monsterTable, string enemyId)
    {
        baseMoveSpeed = moveSpeed;
        baseCombatStats = new CombatStats(maxHP, attackDamage, 0f, 0f, 1f).Sanitized();

        if (monsterTable == null)
        {
            return;
        }

        string id = string.IsNullOrWhiteSpace(enemyId) ? defaultMonsterId : enemyId;
        MonsterRow row = monsterTable.GetById(id);
        if (row == null)
        {
            row = monsterTable.GetById(defaultMonsterId);
        }

        if (row == null)
        {
            return;
        }

        baseCombatStats = row.ToCombatStats().Sanitized();
        if (row.moveSpeed > 0f)
        {
            baseMoveSpeed = row.moveSpeed;
        }
    }
}
