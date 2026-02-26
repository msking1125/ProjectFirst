using UnityEngine;

public class Agent : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private string agentId = "agent01";
    [SerializeField] private AgentTable agentTable;
    [SerializeField] private AgentStatsTable agentStatsTable; // 추가됨

    public float range = 5f;
    public float attackRate = 1f;

    [Header("Legacy")]
    public float attackDamage = 3f;

    [Header("Combat Stats")]
    [SerializeField] private CombatStats stats = new CombatStats(100f, 3f, 0f, 0f, 1.5f);
    [SerializeField] private ElementType element = ElementType.Reason;

    public float AttackPower => stats.atk;
    public float CritChance => stats.critChance;
    public float CritMultiplier => stats.critMultiplier;
    public ElementType Element => element;

    private float timer;
    private bool hasLoggedMissingManager;

    private void Awake()
    {
        ApplyStatsFromTables();
    }

    /// <summary>
    /// AgentTable 또는 AgentStatsTable을 통해 스탯/속성 정보 적용
    /// </summary>
    private void ApplyStatsFromTables()
    {
        bool statsFromTable = false;

        // 우선순위: AgentTable → AgentStatsTable → 기본값
        if (agentTable != null)
        {
            stats = agentTable.GetStats(agentId);
            element = agentTable.GetElement(agentId);
            statsFromTable = true;
        }
        else if (agentStatsTable != null)
        {
            // AgentStatsTable에서 스탯 가져오기
            CombatStats? tableStats = agentStatsTable.GetStats(agentId); // 구현에 따라 인덱서나 메서드명 조정
            if (tableStats != null)
            {
                stats = tableStats.Value;
                statsFromTable = true;
            }

            // AgentStatsTable에 GetElement(string agentId) 메서드가 있다고 가정하고 호출
            // 실제 구현에서 해당 메서드가 없으면 수정 필요
            try
            {
                // 만약 GetElement 메서드가 있다면
                var method = agentStatsTable.GetType().GetMethod("GetElement");
                if (method != null)
                {
                    object result = method.Invoke(agentStatsTable, new object[] { agentId });
                    if (result is ElementType foundElement)
                    {
                        element = foundElement;
                    }
                }
            }
            catch
            {
                // 안전하게 실패
            }
        }

        // fallback: Inspector 값 보정
        if (!statsFromTable && stats.atk <= 0f)
        {
            stats.atk = Mathf.Max(1f, attackDamage);
        }

        stats = stats.Sanitized();
        attackDamage = stats.atk;
    }

    private bool isCombatStarted = false;

    /// <summary>
    /// BattleGameManager에서 호출하여 전투를 시작합니다.
    /// </summary>
    public void StartCombat()
    {
        isCombatStarted = true;
        timer = 0f;
        Debug.Log($"[Agent] 전투 시작: {gameObject.name}, ATK={stats.atk}, Range={range}, Rate={attackRate}");
    }

    /// <summary>전투를 일시 중지합니다 (스킬 선택 패널 등).</summary>
    public void PauseCombat() => isCombatStarted = false;

    /// <summary>전투를 재개합니다.</summary>
    public void ResumeCombat() => isCombatStarted = true;

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
        if (target == null) return;

        bool isCrit;
        int finalDmg = DamageCalculator.ComputeCharacterDamage(
            Mathf.RoundToInt(stats.atk),
            Mathf.RoundToInt(target.Defense),
            stats.critChance,
            stats.critMultiplier,
            out isCrit);

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
