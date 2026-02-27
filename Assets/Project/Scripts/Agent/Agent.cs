using UnityEngine;

public class Agent : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private string agentId = "agent01";
    [SerializeField] private AgentTable agentTable;
    [SerializeField] private AgentStatsTable agentStatsTable;

    public float range = 5f;
    public float attackRate = 1f;

    [Header("Legacy")]
    public float attackDamage = 3f;

    [Header("Combat Stats")]
    [SerializeField] private CombatStats stats = new CombatStats(100f, 3f, 0f, 0f, 1.5f);
    [SerializeField] private ElementType element = ElementType.Reason;

    public float AttackPower    => stats.atk;
    public float CritChance     => stats.critChance;
    public float CritMultiplier => stats.critMultiplier;
    public ElementType Element  => element;

    // ── 애니메이터 ─────────────────────────────────────────────────────────
    // 이름에 _idle / _attack 이 포함된 State를 자동으로 탐지합니다.
    // 직접 지정하려면 Inspector의 idleStateOverride / attackStateOverride를 사용하세요.
    [Header("Animation")]
    [Tooltip("비워두면 '_idle'이 포함된 State를 자동 탐지합니다.")]
    [SerializeField] private string idleStateOverride   = string.Empty;
    [Tooltip("비워두면 '_attack'이 포함된 State를 자동 탐지합니다.")]
    [SerializeField] private string attackStateOverride = string.Empty;

    private Animator   cachedAnimator;
    private string     resolvedIdleState   = string.Empty;
    private string     resolvedAttackState = string.Empty;
    private bool       isAttackPlaying;
    private float      attackAnimTimer;
    private float      attackAnimDuration  = 0.5f; // 공격 애니메이션 재생 시간(초)
    private bool       animatorResolved;

    // ── 기타 ────────────────────────────────────────────────────────────────
    private float timer;
    private bool  hasLoggedMissingManager;
    private bool  isCombatStarted;

    // ────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        ApplyStatsFromTables();
        ResolveAnimator();
    }

    // ── 스탯 적용 ────────────────────────────────────────────────────────────

    private void ApplyStatsFromTables()
    {
        bool statsFromTable = false;

        if (agentTable != null)
        {
            stats = agentTable.GetStats(agentId);
            element = agentTable.GetElement(agentId);
            statsFromTable = true;
        }
        else if (agentStatsTable != null)
        {
            CombatStats? tableStats = agentStatsTable.GetStats(agentId);
            if (tableStats != null)
            {
                stats = tableStats.Value;
                statsFromTable = true;
            }

            try
            {
                var method = agentStatsTable.GetType().GetMethod("GetElement");
                if (method != null)
                {
                    object result = method.Invoke(agentStatsTable, new object[] { agentId });
                    if (result is ElementType foundElement)
                        element = foundElement;
                }
            }
            catch { }
        }

        if (!statsFromTable && stats.atk <= 0f)
            stats.atk = Mathf.Max(1f, attackDamage);

        stats = stats.Sanitized();
        attackDamage = stats.atk;
    }

    // ── 애니메이터 탐지 ──────────────────────────────────────────────────────

    private void ResolveAnimator()
    {
        if (animatorResolved) return;

        if (cachedAnimator == null)
            cachedAnimator = GetComponentInChildren<Animator>(true);

        if (cachedAnimator == null)
        {
            Debug.LogWarning($"[Agent] Animator를 찾지 못했습니다: {gameObject.name}");
            return;
        }

        resolvedIdleState   = ResolveStateName(cachedAnimator, idleStateOverride,   "_idle");
        resolvedAttackState = ResolveStateName(cachedAnimator, attackStateOverride, "_attack");

        if (!string.IsNullOrEmpty(resolvedIdleState))
            PlayState(resolvedIdleState);

        animatorResolved = true;
    }

    /// <summary>
    /// override가 지정된 경우 그것을 우선 사용하고,
    /// 없으면 AnimationClip 이름에 keyword가 포함된 State를 자동 탐지합니다.
    /// </summary>
    private string ResolveStateName(Animator animator, string overrideName, string keyword)
    {
        // 1순위: 직접 지정한 이름
        if (!string.IsNullOrWhiteSpace(overrideName))
        {
            if (animator.HasState(0, Animator.StringToHash(overrideName)))
                return overrideName;
            Debug.LogWarning($"[Agent] '{overrideName}' State를 찾을 수 없습니다. 자동 탐지를 시도합니다. ({gameObject.name})");
        }

        // 2순위: AnimationClip 이름 기반 자동 탐지
        RuntimeAnimatorController rac = animator.runtimeAnimatorController;
        if (rac == null) return string.Empty;

        foreach (AnimationClip clip in rac.animationClips)
        {
            if (clip == null) continue;
            if (clip.name.IndexOf(keyword, System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                int hash = Animator.StringToHash(clip.name);
                if (animator.HasState(0, hash))
                {
                    Debug.Log($"[Agent] '{keyword}' State 자동 탐지 성공: '{clip.name}' ({gameObject.name})");
                    return clip.name;
                }
            }
        }

        Debug.LogWarning($"[Agent] '{keyword}'이 포함된 State를 찾지 못했습니다. Inspector의 Override 필드에 직접 입력하거나 AnimationClip 이름에 '{keyword}'를 포함시켜 주세요. ({gameObject.name})");
        return string.Empty;
    }

    private void PlayState(string stateName)
    {
        if (cachedAnimator == null || string.IsNullOrEmpty(stateName)) return;
        cachedAnimator.Play(stateName, 0, 0f);
    }

    // ── 전투 제어 ────────────────────────────────────────────────────────────

    public void StartCombat()
    {
        isCombatStarted = true;
        timer = 0f;
        ResolveAnimator();

        // 전투 시작 시 Idle 재생
        if (!string.IsNullOrEmpty(resolvedIdleState))
            PlayState(resolvedIdleState);

        Debug.Log($"[Agent] 전투 시작: {gameObject.name}, ATK={stats.atk}, Range={range}, Rate={attackRate}");
    }

    public void PauseCombat()
    {
        isCombatStarted = false;
        // 일시 정지 시 Idle로 복귀
        if (cachedAnimator != null && !string.IsNullOrEmpty(resolvedIdleState))
            PlayState(resolvedIdleState);
    }

    public void ResumeCombat()
    {
        isCombatStarted = true;
    }

    // ── 업데이트 ─────────────────────────────────────────────────────────────

    private void Update()
    {
        HandleAttackAnimReturn();

        if (!isCombatStarted) return;

        timer += Time.deltaTime;
        if (timer >= attackRate)
        {
            Attack();
            timer = 0f;
        }
    }

    /// <summary>
    /// 공격 애니메이션이 재생 중이면 시간을 체크하여 Idle로 돌아옵니다.
    /// </summary>
    private void HandleAttackAnimReturn()
    {
        if (!isAttackPlaying) return;

        attackAnimTimer += Time.deltaTime;
        if (attackAnimTimer >= attackAnimDuration)
        {
            isAttackPlaying = false;
            attackAnimTimer = 0f;
            if (!string.IsNullOrEmpty(resolvedIdleState))
                PlayState(resolvedIdleState);
        }
    }

    // ── 공격 ─────────────────────────────────────────────────────────────────

    private void Attack()
    {
        Enemy target = FindClosestEnemy();
        if (target == null) return;

        // 공격 애니메이션 재생
        PlayAttackAnimation();

        bool isCrit;
        int finalDmg = DamageCalculator.ComputeCharacterDamage(
            Mathf.RoundToInt(stats.atk),
            Mathf.RoundToInt(target.Defense),
            stats.critChance,
            stats.critMultiplier,
            out isCrit);

        target.TakeDamage(finalDmg, isCrit);
    }

    private void PlayAttackAnimation()
    {
        if (string.IsNullOrEmpty(resolvedAttackState)) return;

        PlayState(resolvedAttackState);

        // 공격 애니메이션 길이 측정
        RuntimeAnimatorController rac = cachedAnimator?.runtimeAnimatorController;
        if (rac != null)
        {
            foreach (AnimationClip clip in rac.animationClips)
            {
                if (clip != null && clip.name == resolvedAttackState)
                {
                    attackAnimDuration = Mathf.Max(0.1f, clip.length);
                    break;
                }
            }
        }

        isAttackPlaying = true;
        attackAnimTimer = 0f;
    }

    private Enemy FindClosestEnemy()
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
