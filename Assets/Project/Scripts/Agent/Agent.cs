using UnityEngine;

public class Agent : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private string agentId = "agent01";
    [SerializeField] private AgentTable agentTable;
    [SerializeField] private AgentStatsTable agentStatsTable;

    [Tooltip("캐릭터 고유 데이터 (공격 VFX, 액티브 스킬 등). 비워두면 기본값 사용.")]
    [SerializeField] private AgentData agentData;

    /// <summary>AgentData 공개 접근자 (CharUltimateController 초기화 시 사용)</summary>
    public AgentData AgentData => agentData;

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
    private bool       animatorResolved;

    // ── 공격 타이밍 제어 ──────────────────────────────────────────────────────
    // 애니메이션과 공격 타이밍을 분리하여 끊김 없이 동작합니다.
    //
    // ● attackTimer      : 다음 공격까지 남은 시간 (attackRate 주기)
    // ● animReturnTimer  : 공격 애니 재생 후 Idle 복귀까지 남은 시간
    // ● pendingHitTimer  : 타격 데미지 발생까지 남은 시간 (hitTiming 기반)
    //
    // 핵심 규칙:
    //   - attackTimer가 0이 되면 Attack 애니 재생 + 타격 예약
    //   - animReturnTimer가 끝나기 전에 다시 공격이 오면 애니 중단 없이 재시작
    //   - Idle 복귀는 animReturnTimer로만 결정 (공격 타이밍과 독립)

    private float attackTimer;         // 남은 공격 간격
    private float animReturnTimer;     // 이 시간 후 Idle로 복귀 (공격 애니 길이)
    private float attackAnimDuration  = 0.5f;

    // 히트 지연: AgentData.hitTiming 만큼 기다렸다가 실제 데미지 처리
    private bool  hasPendingHit;
    private float pendingHitTimer;
    private Enemy pendingHitTarget;
    private bool  pendingHitIsCrit;
    private int   pendingHitDamage;

    // ── 기타 ────────────────────────────────────────────────────────────────
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
        isCombatStarted  = true;
        attackTimer      = 0f; // 전투 시작 즉시 첫 공격
        animReturnTimer  = 0f;
        hasPendingHit    = false;
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
        float dt = Time.deltaTime;

        // ── Idle 복귀 타이머 ──────────────────────────────────────────────
        if (animReturnTimer > 0f)
        {
            animReturnTimer -= dt;
            if (animReturnTimer <= 0f)
            {
                // 다음 공격이 거의 바로 온다면 Idle 전환 생략 (끊김 방지)
                bool nextAttackImminent = isCombatStarted && (attackTimer <= attackAnimDuration * 0.15f);
                if (!nextAttackImminent && !string.IsNullOrEmpty(resolvedIdleState))
                    PlayState(resolvedIdleState);
            }
        }

        // ── 히트 지연 처리 ────────────────────────────────────────────────
        if (hasPendingHit)
        {
            pendingHitTimer -= dt;
            if (pendingHitTimer <= 0f)
            {
                hasPendingHit = false;
                ApplyPendingHit();
            }
        }

        if (!isCombatStarted) return;

        // ── 공격 간격 타이머 ──────────────────────────────────────────────
        attackTimer -= dt;
        if (attackTimer <= 0f)
        {
            attackTimer = attackRate;
            Attack();
        }
    }

    // ── 공격 ─────────────────────────────────────────────────────────────────

    private void Attack()
    {
        Enemy target = FindClosestEnemy();
        if (target == null) return;

        // ── 애니메이션 재생 ──────────────────────────────────────────────
        PlayAttackAnimation();

        // ── 공격 VFX 즉시 스폰 (애니메이션 이벤트 아님 → Idle에서 절대 발동 안 함) ──
        SpawnNormalAttackVfx();

        // ── 히트 타이밍 지연 ─────────────────────────────────────────────
        // AgentData.hitTiming 비율만큼 대기 후 데미지 적용
        float hitDelay = attackAnimDuration * (agentData != null ? agentData.hitTiming : 0.3f);

        bool isCrit;
        int finalDmg = DamageCalculator.ComputeCharacterDamage(
            Mathf.RoundToInt(stats.atk),
            Mathf.RoundToInt(target.Defense),
            stats.critChance,
            stats.critMultiplier,
            out isCrit);

        // 이미 대기 중인 히트가 있으면 즉시 처리
        if (hasPendingHit && pendingHitTarget != null && pendingHitTarget.IsAlive)
            ApplyPendingHit();

        hasPendingHit     = true;
        pendingHitTimer   = hitDelay;
        pendingHitTarget  = target;
        pendingHitDamage  = finalDmg;
        pendingHitIsCrit  = isCrit;
    }

    private void ApplyPendingHit()
    {
        if (pendingHitTarget != null && pendingHitTarget.IsAlive)
            pendingHitTarget.TakeDamage(pendingHitDamage, pendingHitIsCrit);
        pendingHitTarget = null;
    }

    private void PlayAttackAnimation()
    {
        if (string.IsNullOrEmpty(resolvedAttackState)) return;

        // 공격 애니 길이 측정 (첫 번째 호출 시 1회)
        if (cachedAnimator != null)
        {
            RuntimeAnimatorController rac = cachedAnimator.runtimeAnimatorController;
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
        }

        // 공격 애니가 재생 중일 때 다시 호출되면 처음부터 재시작 (seamless loop)
        PlayState(resolvedAttackState);
        animReturnTimer = attackAnimDuration;
    }

    /// <summary>
    /// 기본 공격 VFX를 스폰합니다.
    /// AgentData에 프리팹이 없으면 아무것도 하지 않습니다.
    /// 이 메서드는 Attack()에서만 호출 → Idle 재생과 완전히 분리됩니다.
    /// </summary>
    private void SpawnNormalAttackVfx()
    {
        if (agentData == null || agentData.normalAttackVfxPrefab == null) return;

        Vector3 spawnPos = transform.position
                         + transform.TransformDirection(agentData.normalAttackVfxOffset);

        GameObject vfx = Object.Instantiate(
            agentData.normalAttackVfxPrefab, spawnPos, transform.rotation);

        if (vfx == null) return;

        float lifetime = agentData.normalAttackVfxLifetime > 0f
            ? agentData.normalAttackVfxLifetime
            : attackAnimDuration; // 기본값: 공격 애니 길이

        StopAndDestroyVfx(vfx, lifetime);
    }

    /// <summary>
    /// 루프 파티클도 안전하게 소멸시킵니다.
    /// Stop() 없이 Destroy만 하면 루프 파티클이 씬에 남습니다.
    /// </summary>
    private static void StopAndDestroyVfx(GameObject vfx, float delay)
    {
        // StopEmittingAndClear: 기존 파티클까지 즉시 제거 (루프 파티클 누적 방지)
        foreach (ParticleSystem ps in vfx.GetComponentsInChildren<ParticleSystem>(true))
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        // 파티클 완전 제거 후 짧은 딜레이로 오브젝트 소멸
        Object.Destroy(vfx, Mathf.Max(0.05f, delay));
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
