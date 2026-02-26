using UnityEngine;

public class Agent : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private string agentId = "agent01";
    [SerializeField] private AgentTable agentTable;

    public float range = 5f;
    public float attackRate = 1f;

    [Header("Legacy")]
    public float attackDamage = 3f;

    [Header("Combat Stats")]
    [SerializeField] private CombatStats stats = new CombatStats(100f, 3f, 0f, 0f, 1.5f);
    [SerializeField] private ElementType element = ElementType.Reason;

    [Header("Debug")]
    [SerializeField] private bool logElementDamage;
    [SerializeField] private bool logElementAdvantageOnce = true;

    public float AttackPower => stats.atk;
    public float CritChance => stats.critChance;
    public float CritMultiplier => stats.critMultiplier;
    public ElementType Element => element;

    private float timer;
    private bool hasLoggedMissingManager;
    private bool hasLoggedElementAdvantage;

    private void Awake()
    {
        ApplyStatsFromTable();
    }

    private void ApplyStatsFromTable()
    {
        if (agentTable != null)
        {
            stats = agentTable.GetStats(agentId);
            element = agentTable.GetElement(agentId);
        }
        else if (stats.atk <= 0f)
        {
            stats.atk = Mathf.Max(1f, attackDamage);
        }

        stats = stats.Sanitized();
        attackDamage = stats.atk;
    }

    public bool isCombatStarted = false;

    void Update()
    {
        if (!isCombatStarted) return;

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

        int finalDmg = DamageCalculator.ComputeBasicDamage(
            Mathf.RoundToInt(stats.atk),
            Mathf.RoundToInt(target.Defense),
            stats.critChance,
            stats.critMultiplier,
            element,
            target.Element,
            out bool isCrit);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (logElementDamage && (!logElementAdvantageOnce || !hasLoggedElementAdvantage))
        {
            bool hasAdvantage = ElementRules.HasAdvantage(element, target.Element);
            float elementMultiplier = ElementRules.GetMultiplier(element, target.Element);
            Debug.Log($"[Agent] DamageCalc attackerElement={element} defenderElement={target.Element} advantageApplied={hasAdvantage} multiplier={elementMultiplier:F1} finalDmg={finalDmg}", this);
            hasLoggedElementAdvantage = true;
        }
#endif

        target.TakeDamage(finalDmg, isCrit);
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
