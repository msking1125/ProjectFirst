using ProjectFirst.Data;
using UnityEngine;

namespace Project
{
    public class Agent : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private int agentId = 1;
        [SerializeField] private AgentData agentData;
        [SerializeField] private AgentTable agentTable;
        [SerializeField] private AgentStatsTable agentStatsTable;

        [Header("Combat")]
        [SerializeField] private CombatStats stats = new CombatStats(100f, 10f, 0f, 0.05f, 1.5f);
        [SerializeField] private ElementType element = ElementType.Reason;
        [SerializeField] private float attackRate = 1.2f;
        [SerializeField] private float range = 15f;
        [SerializeField] private Vector3 modelRotationOffset;

        [Header("Animation")]
        [SerializeField] private Animator cachedAnimator;
        [SerializeField] private string idleStateOverride = string.Empty;
        [SerializeField] private string attackStateOverride = string.Empty;
        [SerializeField] private float attackAnimDuration = 0.6f;

        private Firerailgun cachedRailgun;
        private string resolvedIdleState = string.Empty;
        private string resolvedAttackState = string.Empty;
        private bool animatorResolved;
        private float attackTimer;
        private float animReturnTimer;
        private bool hasPendingHit;
        private float pendingHitTimer;
        private Enemy pendingHitTarget;
        private bool pendingHitIsCrit;
        private int pendingHitDamage;
        private bool isCombatStarted;

        public int AgentId => agentId;
        public AgentData AgentData => agentData;
        public CombatStats Stats => stats;
        public ElementType Element => element;
        public float AttackPower => stats.atk;
        public float CritChance => stats.critChance;
        public float CritMultiplier => stats.critMultiplier;
        public float Range => range;

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
                stats = agentStatsTable.GetStats(agentId);
                element = agentStatsTable.GetElement(agentId);
                statsFromTable = true;
            }

            if (!statsFromTable && stats.atk <= 0f)
            {
                stats.atk = 1f;
            }

            stats = stats.Sanitized();
        }

        private void ResolveAnimator()
        {
            if (animatorResolved)
            {
                return;
            }

            if (cachedAnimator == null)
            {
                cachedAnimator = GetComponentInChildren<Animator>(true);
            }

            if (cachedAnimator == null)
            {
                return;
            }

            resolvedIdleState = ResolveStateName(cachedAnimator, idleStateOverride, "_idle");
            resolvedAttackState = ResolveStateName(cachedAnimator, attackStateOverride, "_attack");

            if (!string.IsNullOrEmpty(resolvedIdleState))
            {
                PlayState(resolvedIdleState);
            }

            animatorResolved = true;
        }

        private static string ResolveStateName(Animator animator, string overrideName, string keyword)
        {
            if (!string.IsNullOrWhiteSpace(overrideName) && animator.HasState(0, Animator.StringToHash(overrideName)))
            {
                return overrideName;
            }

            RuntimeAnimatorController controller = animator.runtimeAnimatorController;
            if (controller == null)
            {
                return string.Empty;
            }

            foreach (AnimationClip clip in controller.animationClips)
            {
                if (clip == null)
                {
                    continue;
                }

                if (clip.name.IndexOf(keyword, System.StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                if (animator.HasState(0, Animator.StringToHash(clip.name)))
                {
                    return clip.name;
                }
            }

            return string.Empty;
        }

        private void PlayState(string stateName)
        {
            if (cachedAnimator == null || string.IsNullOrEmpty(stateName))
            {
                return;
            }

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
            {
                PlayState(resolvedIdleState);
            }
        }

        public void PauseCombat()
        {
            isCombatStarted = false;
            if (cachedAnimator != null && !string.IsNullOrEmpty(resolvedIdleState))
            {
                PlayState(resolvedIdleState);
            }
        }

        public void ResumeCombat()
        {
            isCombatStarted = true;
        }

        private void Update()
        {
            if (cachedAnimator != null)
            {
                AnimatorStateInfo stateInfo = cachedAnimator.GetCurrentAnimatorStateInfo(0);
                if (stateInfo.IsName("Tabi_skill_action") || stateInfo.IsName("Tabi_skill"))
                {
                    return;
                }
            }

            float dt = Time.deltaTime;

            if (animReturnTimer > 0f)
            {
                animReturnTimer -= dt;
                if (animReturnTimer <= 0f)
                {
                    Enemy target = FindClosestEnemy();
                    bool nextAttackImminent = target != null && isCombatStarted && attackTimer <= attackAnimDuration * 0.15f;
                    if (!nextAttackImminent && !string.IsNullOrEmpty(resolvedIdleState))
                    {
                        PlayState(resolvedIdleState);
                    }
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

            if (!isCombatStarted)
            {
                return;
            }

            attackTimer -= dt;
            if (attackTimer <= 0f)
            {
                attackTimer = Mathf.Max(0.05f, attackRate);
                Attack();
            }
        }

        private void Attack()
        {
            Enemy target = FindClosestEnemy();
            if (target == null)
            {
                return;
            }

            Vector3 directionToTarget = (target.transform.position - transform.position).normalized;
            directionToTarget.y = 0f;
            if (directionToTarget != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                Quaternion modelOffset = Quaternion.Euler(modelRotationOffset);
                transform.rotation = targetRotation * modelOffset;
            }

            PlayAttackAnimation();
            SpawnNormalAttackVfx();

            float hitDelay = attackAnimDuration * (agentData != null ? agentData.hitTiming : 0.3f);
            int finalDamage = DamageCalculator.ComputeDamage(
                stats.atk,
                target.Defense,
                stats.critChance,
                stats.critMultiplier,
                element,
                target.Element,
                out bool isCrit);

            if (hasPendingHit && pendingHitTarget != null && pendingHitTarget.IsAlive)
            {
                ApplyPendingHit();
            }

            hasPendingHit = true;
            pendingHitTimer = hitDelay;
            pendingHitTarget = target;
            pendingHitDamage = finalDamage;
            pendingHitIsCrit = isCrit;
        }

        private void ApplyPendingHit()
        {
            if (pendingHitTarget != null && pendingHitTarget.IsAlive)
            {
                pendingHitTarget.TakeDamage(pendingHitDamage, pendingHitIsCrit);
            }

            pendingHitTarget = null;
        }

        private void PlayAttackAnimation()
        {
            if (string.IsNullOrEmpty(resolvedAttackState))
            {
                return;
            }

            if (cachedAnimator != null)
            {
                RuntimeAnimatorController controller = cachedAnimator.runtimeAnimatorController;
                if (controller != null)
                {
                    foreach (AnimationClip clip in controller.animationClips)
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

            if (agentData == null || agentData.normalAttackVfxPrefab == null)
            {
                return;
            }

            Vector3 spawnPos = transform.position + transform.TransformDirection(agentData.normalAttackVfxOffset);
            GameObject vfx = Instantiate(agentData.normalAttackVfxPrefab, spawnPos, transform.rotation);
            if (vfx == null)
            {
                return;
            }

            float lifetime = agentData.normalAttackVfxLifetime > 0f ? agentData.normalAttackVfxLifetime : attackAnimDuration;
            StopAndDestroyVfx(vfx, lifetime);
        }

        private static void StopAndDestroyVfx(GameObject vfx, float delay)
        {
            foreach (ParticleSystem particleSystem in vfx.GetComponentsInChildren<ParticleSystem>(true))
            {
                particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            Destroy(vfx, Mathf.Max(0.05f, delay));
        }

        private Enemy FindClosestEnemy()
        {
            EnemyManager manager = EnemyManager.Instance;
            return manager != null ? manager.GetClosest(transform.position, range) : null;
        }
    }
}

