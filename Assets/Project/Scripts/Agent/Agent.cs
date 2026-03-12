using System;
using System.Reflection;
using UnityEngine;
using ProjectFirst.Data;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Project
{

#if ODIN_INSPECTOR
    [HideMonoScript]
    [BoxGroup("data")]
    [BoxGroup("combat")]
#endif
    public class Agent : MonoBehaviour
    {
#if ODIN_INSPECTOR
        [Title("data", TitleAlignment = TitleAlignments.Left)]
        [BoxGroup("data/id")]
        [LabelText("agent id")]
#endif
        [Header("data")]
        [SerializeField] private int agentId;

#if ODIN_INSPECTOR
        [BoxGroup("data/agent data")]
        [LabelText("agent data")]
        [AssetsOnly]
#endif
        [SerializeField] private AgentData agentData;

#if ODIN_INSPECTOR
        [BoxGroup("data/table")]
        [LabelText("agent table")]
#endif
        [SerializeField] private UnityEngine.Object agentTable;

#if ODIN_INSPECTOR
        [BoxGroup("data/table")]
        [LabelText("agent stats table")]
#endif
        [SerializeField] private UnityEngine.Object agentStatsTable;

#if ODIN_INSPECTOR
        [Title("combat", TitleAlignment = TitleAlignments.Left)]
        [BoxGroup("combat/range")]
        [LabelText("attack range")]
#endif
        [Header("combat")]
        [SerializeField] private float range = 10f;

#if ODIN_INSPECTOR
        [BoxGroup("combat/rate")]
        [LabelText("attack interval")]
#endif
        [SerializeField] private float attackRate = 1f;

#if ODIN_INSPECTOR
        [BoxGroup("combat/rotation")]
        [LabelText("model rotation offset")]
#endif
        [SerializeField] private Vector3 modelRotationOffset = Vector3.zero;

        [SerializeField] private CombatStats stats;
        [SerializeField] private ElementType element = ElementType.Reason;

        private AgentAnimatorBridge cachedAnimatorBridge;
        private Animator cachedAnimator;
        private ProjectileShooter cachedProjectileShooter;

        private bool isCombatStarted;
        private float attackTimer;

        // 현재 진행 중인 평타 1세트(2타) 대상
        private Enemy currentAttackTarget;
        private bool comboAttackActive;

        public AgentData AgentData => agentData;
        public float AttackPower => stats.atk;
        public float CritChance => stats.critChance;
        public float CritMultiplier => stats.critMultiplier;
        public CombatStats Stats => stats;
        public ElementType Element => element;

        private void Awake()
        {
            ApplyStatsFromTables();
            CacheComponents();
        }

        private void CacheComponents()
        {
            if (cachedAnimatorBridge == null)
                cachedAnimatorBridge = GetComponentInChildren<AgentAnimatorBridge>(true);

            if (cachedAnimator == null)
                cachedAnimator = GetComponentInChildren<Animator>(true);

            if (cachedProjectileShooter == null)
                cachedProjectileShooter = GetComponentInChildren<ProjectileShooter>(true);
        }

        public void StartCombat()
        {
            CacheComponents();

            isCombatStarted = true;
            attackTimer = 0f;
            comboAttackActive = false;
            currentAttackTarget = null;
        }

        public void PauseCombat()
        {
            isCombatStarted = false;
        }

        public void ResumeCombat()
        {
            isCombatStarted = true;
        }

        private void Update()
        {
            if (!isCombatStarted)
                return;

            CacheComponents();

            // 스킬/궁극기 중에는 자동 평타 정지
            if (cachedAnimatorBridge != null && cachedAnimatorBridge.IsInSkillState())
                return;

            // 현재 평타 2타 세트 진행 중이면 다음 평타 시작 금지
            if (comboAttackActive)
                return;

            attackTimer -= Time.deltaTime;
            if (attackTimer > 0f)
                return;

            Enemy target = FindClosestEnemy();
            if (target == null)
                return;

            BeginAttackCombo(target);
            attackTimer = Mathf.Max(0.01f, attackRate);
        }

        private void BeginAttackCombo(Enemy target)
        {
            if (target == null)
                return;

            currentAttackTarget = target;
            comboAttackActive = true;

            RotateTowardTarget(target);

            if (cachedAnimatorBridge != null)
            {
                cachedAnimatorBridge.TriggerAttack();
            }
            else if (cachedAnimator != null)
            {
                cachedAnimator.SetTrigger("attack");
            }
        }

        private void RotateTowardTarget(Enemy target)
        {
            if (target == null) return;

            Vector3 directionToTarget = target.transform.position - transform.position;
            directionToTarget.y = 0f;

            if (directionToTarget.sqrMagnitude <= 0.0001f)
                return;

            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget.normalized);
            Quaternion modelOffset = Quaternion.Euler(modelRotationOffset);
            transform.rotation = targetRotation * modelOffset;
        }

        private Enemy ResolveAttackTarget()
        {
            if (currentAttackTarget != null && currentAttackTarget.IsAlive)
                return currentAttackTarget;

            Enemy fallback = FindClosestEnemy();
            currentAttackTarget = fallback;
            return fallback;
        }

        private void ApplySingleAttackHit()
        {
            Enemy target = ResolveAttackTarget();
            if (target == null)
                return;

            bool isCrit;
            int finalDmg = DamageCalculator.ComputeDamage(
                stats.atk,
                target.Defense,
                stats.critChance,
                stats.critMultiplier,
                element,
                target.Element,
                out isCrit
            );

            target.TakeDamage(finalDmg, isCrit);
        }

        /// <summary>
        /// attack_01 animation event
        /// </summary>
        public void ApplyAttackHit_01()
        {
            ApplySingleAttackHit();
        }

        /// <summary>
        /// attack_02 animation event
        /// </summary>
        public void ApplyAttackHit_02()
        {
            ApplySingleAttackHit();
        }

        /// <summary>
        /// attack_01 / attack_02 둘 다에서 호출 가능
        /// </summary>
        public void FireNormalAttack()
        {
            CacheComponents();

            if (cachedProjectileShooter != null)
            {
                cachedProjectileShooter.FireNormalAttack();
                return;
            }

            SpawnNormalAttackVfxFallback();
        }

        private void SpawnNormalAttackVfxFallback()
        {
            if (agentData == null || agentData.normalAttackVfxPrefab == null)
                return;

            Vector3 spawnPos = transform.position + transform.TransformDirection(agentData.normalAttackVfxOffset);
            GameObject vfx = Instantiate(agentData.normalAttackVfxPrefab, spawnPos, transform.rotation);
            if (vfx == null)
                return;

            float lifetime = agentData.normalAttackVfxLifetime > 0f
                ? agentData.normalAttackVfxLifetime
                : 1f;

            StopAndDestroyVfx(vfx, lifetime);
        }

        /// <summary>
        /// attack_02 끝 프레임에 넣는 이벤트
        /// 평타 1세트 종료 처리
        /// </summary>
        public void EndAttackCombo()
        {
            comboAttackActive = false;
            currentAttackTarget = null;
        }

        public void TriggerActiveSkillAnimation()
        {
            CacheComponents();

            if (cachedAnimatorBridge != null)
                cachedAnimatorBridge.TriggerActiveSkill();
            else if (cachedAnimator != null)
                cachedAnimator.SetTrigger("activeskill");
        }

        public void TriggerUltimateAnimation()
        {
            CacheComponents();

            if (cachedAnimatorBridge != null)
                cachedAnimatorBridge.TriggerUltimate();
            else if (cachedAnimator != null)
                cachedAnimator.SetTrigger("ultimate");
        }

        private static void StopAndDestroyVfx(GameObject vfx, float delay)
        {
            foreach (ParticleSystem ps in vfx.GetComponentsInChildren<ParticleSystem>(true))
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            Destroy(vfx, Mathf.Max(0.05f, delay));
        }

        private Enemy FindClosestEnemy()
        {
            EnemyManager manager = EnemyManager.Instance;
            if (manager == null) return null;
            return manager.GetClosest(transform.position, range);
        }

        private void ApplyStatsFromTables()
        {
            bool statsApplied = false;

            if (TryGetStatsFromTableObject(agentTable, agentId, out CombatStats tableStats))
            {
                stats = tableStats;
                statsApplied = true;
            }
            else if (TryGetStatsFromTableObject(agentStatsTable, agentId, out CombatStats fallbackStats))
            {
                stats = fallbackStats;
                statsApplied = true;
            }

            if (TryGetElementFromTableObject(agentTable, agentId, out ElementType tableElement))
            {
                element = tableElement;
            }
            else if (TryGetElementFromTableObject(agentStatsTable, agentId, out ElementType fallbackElement))
            {
                element = fallbackElement;
            }

            if (!statsApplied && stats.atk <= 0f)
                stats.atk = 1f;

            if (stats.atk <= 0f)
                stats.atk = 1f;
        }

        private static bool TryGetStatsFromTableObject(UnityEngine.Object tableObject, int id, out CombatStats result)
        {
            result = default;
            if (tableObject == null) return false;

            MethodInfo method = tableObject.GetType().GetMethod("GetStats", BindingFlags.Public | BindingFlags.Instance);
            if (method == null) return false;

            try
            {
                object value = method.Invoke(tableObject, new object[] { id });

                if (value is CombatStats directStats)
                {
                    result = directStats;
                    return true;
                }

                Type nullableType = Nullable.GetUnderlyingType(method.ReturnType);
                if (nullableType == typeof(CombatStats) && value != null)
                {
                    result = (CombatStats)value;
                    return true;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Agent] Failed to invoke GetStats on {tableObject.name}: {e.Message}");
            }

            return false;
        }

        private static bool TryGetElementFromTableObject(UnityEngine.Object tableObject, int id, out ElementType result)
        {
            result = default;
            if (tableObject == null) return false;

            MethodInfo method = tableObject.GetType().GetMethod("GetElement", BindingFlags.Public | BindingFlags.Instance);
            if (method == null) return false;

            try
            {
                object value = method.Invoke(tableObject, new object[] { id });
                if (value is ElementType directElement)
                {
                    result = directElement;
                    return true;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Agent] Failed to invoke GetElement on {tableObject.name}: {e.Message}");
            }

            return false;
        }
    }
}

