using UnityEngine;

public class Agent : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private string agentId = "agent01";
    [SerializeField] private AgentTable agentTable;
    [SerializeField] private AgentStatsTable agentStatsTable;

    [Tooltip("캐릭터 고유 데이터 (공격 VFX, 액티브 스킬 등). 비워두면 기본값 사용.")]
    [SerializeField] private AgentData agentData;

    public AgentData AgentData => agentData;

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

    [Header("Animation")]
    [Tooltip("비워두면 '_idle'이 포함된 State를 자동 탐지합니다.")]
    [SerializeField] private string idleStateOverride = string.Empty;
    [Tooltip("비워두면 '_attack'이 포함된 State를 자동 탐지합니다.")]
    [SerializeField] private string attackStateOverride = string.Empty;

    [Header("Model Rotation Offset")]
    [Tooltip("캐릭터 모델이 정면을 보지 않고 옆을 볼 때 각도를 보정합니다. (예: Y축 -90 또는 90)")]
    public Vector3 modelRotationOffset = new Vector3(0f, -90f, 0f);

    private Animator cachedAnimator;
    private string resolvedIdleState = string.Empty;
    private string resolvedAttackState = string.Empty;
    private bool animatorResolved;

    private float attackTimer;
    private float animReturnTimer;
    private float attackAnimDuration = 0.5f;

    private bool hasPendingHit;
    private float pendingHitTimer;
    private Enemy pendingHitTarget;
    private bool pendingHitIsCrit;
    private int pendingHitDamage;

    private bool hasLoggedMissingManager;
    private bool isCombatStarted;

    private void Awake()
    {
        ApplyStatsFromTables();
        ResolveAnimator();
    }

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

    private void ResolveAnimator()
    {
        if (animatorResolved) return;

        if (cachedAnimator == null)
            cachedAnimator = GetComponentInChildren<Animator>(true);

        if (cachedAnimator == null) return;

        resolvedIdleState = ResolveStateName(cachedAnimator, idleStateOverride, "_idle");
        resolvedAttackState = ResolveStateName(cachedAnimator, attackStateOverride, "_attack");

        if (!string.IsNullOrEmpty(resolvedIdleState))
            PlayState(resolvedIdleState);

        animatorResolved = true;
    }

    private string ResolveStateName(Animator animator, string overrideName, string keyword)
    {
        if (!string.IsNullOrWhiteSpace(overrideName))
        {
            if (animator.HasState(0, Animator.StringToHash(overrideName)))
                return overrideName;
        }

        RuntimeAnimatorController rac = animator.runtimeAnimatorController;
        if (rac == null) return string.Empty;

        foreach (AnimationClip clip in rac.animationClips)
        {
            if (clip == null) continue;
            if (clip.name.IndexOf(keyword, System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                int hash = Animator.StringToHash(clip.name);
                if (animator.HasState(0, hash)) return clip.name;
            }
        }
        return string.Empty;
    }

    private void PlayState(string stateName)
    {
        if (cachedAnimator == null || string.IsNullOrEmpty(stateName)) return;
        cachedAnimator.CrossFadeInFixedTime(stateName, 0.15f);
    }

    public void StartCombat()
    {
        isCombatStarted = true;
        attackTimer = 0f;
        animReturnTimer = 0f;
        hasPendingHit = false;
        ResolveAnimator();

        if (!string.IsNullOrEmpty(resolvedIdleState))
            PlayState(resolvedIdleState);
    }

    public void PauseCombat()
    {
        isCombatStarted = false;
        if (cachedAnimator != null && !string.IsNullOrEmpty(resolvedIdleState))
            PlayState(resolvedIdleState);
    }

    public void ResumeCombat()
    {
        isCombatStarted = true;
    }

    private void Update()
    {
        // [핵심 추가] 스킬 컷신이나 스킬 공격 중일 때는 평타 로직을 완전히 멈춥니다!
        if (cachedAnimator != null)
        {
            AnimatorStateInfo stateInfo = cachedAnimator.GetCurrentAnimatorStateInfo(0);

            // 애니메이터 상태 이름이 정확히 일치해야 합니다.
            if (stateInfo.IsName("Tabi_skill_action") || stateInfo.IsName("Tabi_skill"))
            {
                return; // 아래의 타이머 계산과 평타 발동을 무시합니다.
            }
        }

        float dt = Time.deltaTime;

        if (animReturnTimer > 0f)
        {
            animReturnTimer -= dt;
            if (animReturnTimer <= 0f)
            {
                Enemy target = FindClosestEnemy();
                bool nextAttackImminent = (target != null) && isCombatStarted && (attackTimer <= attackAnimDuration * 0.15f);

                if (!nextAttackImminent && !string.IsNullOrEmpty(resolvedIdleState))
                    PlayState(resolvedIdleState);
            }
        }

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

        attackTimer -= dt;
        if (attackTimer <= 0f)
        {
            attackTimer = attackRate;
            Attack();
        }
    }

    private void Attack()
    {
        Enemy target = FindClosestEnemy();
        if (target == null) return;

        Vector3 directionToTarget = (target.transform.position - transform.position).normalized;
        directionToTarget.y = 0;
        if (directionToTarget != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(directionToTarget);
            Quaternion modelOffset = Quaternion.Euler(modelRotationOffset);
            transform.rotation = targetRot * modelOffset;
        }

        PlayAttackAnimation();
        SpawnNormalAttackVfx();

        float hitDelay = attackAnimDuration * (agentData != null ? agentData.hitTiming : 0.3f);

        bool isCrit;
        int finalDmg = DamageCalculator.ComputeCharacterDamage(
            Mathf.RoundToInt(stats.atk),
            Mathf.RoundToInt(target.Defense),
            stats.critChance,
            stats.critMultiplier,
            out isCrit);

        if (hasPendingHit && pendingHitTarget != null && pendingHitTarget.IsAlive)
            ApplyPendingHit();

        hasPendingHit = true;
        pendingHitTimer = hitDelay;
        pendingHitTarget = target;
        pendingHitDamage = finalDmg;
        pendingHitIsCrit = isCrit;
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

        PlayState(resolvedAttackState);
        animReturnTimer = attackAnimDuration;
    }

    private void SpawnNormalAttackVfx()
    {
        Firerailgun railgun = GetComponentInChildren<Firerailgun>();
        if (railgun != null)
        {
            railgun.FireRailgun();
            return;
        }

        if (agentData == null || agentData.normalAttackVfxPrefab == null) return;

        Vector3 spawnPos = transform.position + transform.TransformDirection(agentData.normalAttackVfxOffset);
        GameObject vfx = Object.Instantiate(agentData.normalAttackVfxPrefab, spawnPos, transform.rotation);

        if (vfx == null) return;

        float lifetime = agentData.normalAttackVfxLifetime > 0f ? agentData.normalAttackVfxLifetime : attackAnimDuration;
        StopAndDestroyVfx(vfx, lifetime);
    }

    private static void StopAndDestroyVfx(GameObject vfx, float delay)
    {
        foreach (ParticleSystem ps in vfx.GetComponentsInChildren<ParticleSystem>(true))
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        Object.Destroy(vfx, Mathf.Max(0.05f, delay));
    }

    private Enemy FindClosestEnemy()
    {
        EnemyManager manager = EnemyManager.Instance;
        if (manager == null) return null;
        return manager.GetClosest(transform.position, range);
    }
}