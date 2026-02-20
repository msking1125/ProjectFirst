using UnityEngine;

public class Agent : MonoBehaviour
{
    public float range = 5f;
    public float attackRate = 1f;

    [Header("Legacy")]
    public float attackDamage = 3f;

    [Header("Combat Stats")]
    [SerializeField] private CombatStats stats = new CombatStats(100f, 3f, 0f, 0f, 1.5f);

    private float timer;
    private bool hasLoggedMissingManager;

    private void Awake()
    {
        if (stats.atk <= 0f)
        {
            stats.atk = Mathf.Max(1f, attackDamage);
        }

        stats = stats.Sanitized();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= attackRate)
        {
            Attack();
            timer = 0f;
        }
    }

    void Attack()
    {
        Enemy target = FindClosestEnemy();
        if (target == null)
        {
            return;
        }

        float damage = Mathf.Max(1f, stats.atk - target.Defense);
        if (Random.value < Mathf.Clamp01(stats.critChance))
        {
            damage *= Mathf.Max(1f, stats.critMultiplier);
        }

        target.TakeDamage(Mathf.RoundToInt(damage));
    }

    Enemy FindClosestEnemy()
    {
        EnemyManager manager = EnemyManager.Instance;
        if (manager == null)
        {
            if (!hasLoggedMissingManager)
            {
                Debug.LogError("[Agent] EnemyManager.Instance가 없어 타겟을 탐색할 수 없습니다.");
                hasLoggedMissingManager = true;
            }

            return null;
        }

        return manager.GetClosest(transform.position, range);
    }
}
