using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Project
{

/// <summary>
/// 플레이어 캐릭터(Agent)의 전투 및 데이터 관리 컴포넌트.
/// </summary>
#if ODIN_INSPECTOR
    [HideMonoScript]
#endif
    public class Agent : MonoBehaviour
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
        cachedRailgun = GetComponentInChildren<Firerailgun>(true);
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
            stats.atk = 1f;

        stats = stats.Sanitized();
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
        int finalDmg = DamageCalculator.ComputeDamage(
            stats.atk,
            target.Defense,
            stats.critChance,
            stats.critMultiplier,
            element,
            target.Element,
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
        if (cachedRailgun != null)
        {
            cachedRailgun.FireRailgun();
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
}