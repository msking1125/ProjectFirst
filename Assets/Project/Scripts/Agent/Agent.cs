using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Playables;
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
        [BoxGroup("data")]
        [LabelText("agent id")]
#endif
        [Header("data")]
        [SerializeField] private int agentId;

#if ODIN_INSPECTOR
        [BoxGroup("data")]
        [LabelText("agent data")]
        [AssetsOnly]
#endif
        [SerializeField] private AgentData agentData;

#if ODIN_INSPECTOR
        [BoxGroup("data")]
        [LabelText("agent table")]
#endif
        [SerializeField] private UnityEngine.Object agentTable;

#if ODIN_INSPECTOR
        [BoxGroup("data")]
        [LabelText("agent stats table")]
#endif
        [SerializeField] private UnityEngine.Object agentStatsTable;

#if ODIN_INSPECTOR
        [BoxGroup("combat")]
        [LabelText("attack range")]
#endif
        [Header("combat")]
        [SerializeField] private float range = 10f;

#if ODIN_INSPECTOR
        [BoxGroup("combat")]
        [LabelText("attack interval")]
#endif
        [SerializeField] private float attackRate = 1f;

#if ODIN_INSPECTOR
        [BoxGroup("combat")]
        [LabelText("model rotation offset")]
#endif
        [SerializeField] private Vector3 modelRotationOffset = Vector3.zero;

        [SerializeField] private CombatStats stats;
        [SerializeField] private ElementType element = ElementType.Reason;

        private AgentAnimatorBridge cachedAnimatorBridge;
        private Animator cachedAnimator;
        private Transform cachedVisualRoot;
        private ProjectileShooter cachedProjectileShooter;
        private GameObject runtimeVisualInstance;

        private bool isCombatStarted;
        private float attackTimer;

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
            if (TryDisableNestedAgent())
                return;

            NormalizeAgentId();
            ApplyStatsFromTables();
            CacheComponents();
        }

        private bool TryDisableNestedAgent()
        {
            Agent parentAgent = GetComponentInParent<Agent>();
            if (parentAgent == null || parentAgent == this)
                return false;

            enabled = false;
            return true;
        }

        private void NormalizeAgentId()
        {
            if (agentData != null && agentData.agentId > 0)
                agentId = agentData.agentId;
        }

        private void PrepareExistingVisualIfPresent()
        {
            if (transform.childCount <= 0)
                return;

            runtimeVisualInstance = transform.GetChild(0).gameObject;
            if (runtimeVisualInstance == null)
                return;

            PrepareInstantiatedVisual(runtimeVisualInstance);
            runtimeVisualInstance.SetActive(true);
        }

        private void SpawnRuntimeVisualIfNeeded()
        {
            if (!TryGetAgentTable(out AgentTable table))
                return;

            AgentInfo agentInfo = table.GetAgentInfo(agentId);
            if (agentInfo == null || agentInfo.modelPrefab == null)
                return;

            ClearVisualChildren();

            GameObject visualContainer = new GameObject(agentInfo.modelPrefab.name + "_Container");
            visualContainer.transform.SetParent(transform, false);
            visualContainer.SetActive(false);

            GameObject visualInstance = Instantiate(agentInfo.modelPrefab, visualContainer.transform, false);
            visualInstance.name = agentInfo.modelPrefab.name + "_Runtime";
            visualInstance.transform.localPosition = Vector3.zero;
            visualInstance.transform.localRotation = Quaternion.identity;
            visualInstance.transform.localScale = Vector3.one;

            PrepareInstantiatedVisual(visualInstance);

            visualInstance.transform.SetParent(transform, false);
            if (Application.isPlaying)
                Destroy(visualContainer);
            else
                DestroyImmediate(visualContainer);

            runtimeVisualInstance = visualInstance;
            runtimeVisualInstance.SetActive(true);
        }

        private bool TryGetAgentTable(out AgentTable table)
        {
            table = agentTable as AgentTable;
            return table != null;
        }

        private void ClearVisualChildren()
        {
            runtimeVisualInstance = null;
            cachedVisualRoot = null;
            cachedAnimatorBridge = null;
            cachedAnimator = null;
            cachedProjectileShooter = null;

            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);
                if (Application.isPlaying)
                    Destroy(child.gameObject);
                else
                    DestroyImmediate(child.gameObject);
            }
        }

        private void PrepareInstantiatedVisual(GameObject visualInstance)
        {
            if (visualInstance == null)
                return;

            foreach (PlayableDirector director in visualInstance.GetComponentsInChildren<PlayableDirector>(true))
            {
                if (director == null)
                    continue;

                director.Stop();
                director.enabled = false;
            }

            Agent[] nestedAgents = visualInstance.GetComponentsInChildren<Agent>(true);
            foreach (Agent nestedAgent in nestedAgents)
            {
                if (nestedAgent == null || nestedAgent == this)
                    continue;

                if (Application.isPlaying)
                    Destroy(nestedAgent);
                else
                    DestroyImmediate(nestedAgent);
            }

            foreach (AgentAnimationEvents animationEvents in visualInstance.GetComponentsInChildren<AgentAnimationEvents>(true))
            {
                if (animationEvents != null)
                    animationEvents.Bind(this);
            }

            foreach (Camera cameraComponent in visualInstance.GetComponentsInChildren<Camera>(true))
            {
                if (cameraComponent != null)
                    cameraComponent.gameObject.SetActive(false);
            }

            foreach (AudioListener audioListener in visualInstance.GetComponentsInChildren<AudioListener>(true))
            {
                if (audioListener != null)
                    audioListener.enabled = false;
            }

            foreach (Behaviour behaviour in visualInstance.GetComponentsInChildren<Behaviour>(true))
            {
                if (behaviour == null)
                    continue;

                Type behaviourType = behaviour.GetType();
                string fullName = behaviourType.FullName ?? string.Empty;
                string typeName = behaviourType.Name ?? string.Empty;
                string namespaceName = behaviourType.Namespace ?? string.Empty;

                if (namespaceName.StartsWith("Cinemachine", StringComparison.Ordinal) ||
                    fullName.Contains("Cinemachine", StringComparison.Ordinal) ||
                    typeName.Contains("Cinemachine", StringComparison.Ordinal))
                {
                    behaviour.enabled = false;
                }
            }

            foreach (Transform child in visualInstance.GetComponentsInChildren<Transform>(true))
            {
                if (child == null)
                    continue;

                if (string.Equals(child.name, "Virtual Camera", StringComparison.OrdinalIgnoreCase))
                    child.gameObject.SetActive(false);
            }

            Transform primaryVisualRoot = ResolvePrimaryVisualRoot(visualInstance.transform);
            if (primaryVisualRoot != null && primaryVisualRoot != visualInstance.transform)
            {
                primaryVisualRoot.localPosition = Vector3.zero;
                primaryVisualRoot.localRotation = Quaternion.identity;
            }

            Animator playableAnimator = ResolvePlayableAnimatorFromRoot(visualInstance.transform);
            if (playableAnimator != null)
            {
                AgentAnimationEvents animationEvents = playableAnimator.GetComponent<AgentAnimationEvents>();
                if (animationEvents == null)
                    animationEvents = playableAnimator.gameObject.AddComponent<AgentAnimationEvents>();

                animationEvents.Bind(this);
            }
        }

        private Transform ResolvePrimaryVisualRoot(Transform visualRoot)
        {
            if (visualRoot == null)
                return null;

            Animator playableAnimator = ResolvePlayableAnimatorFromRoot(visualRoot);
            if (playableAnimator != null)
                return playableAnimator.transform;

            ProjectileShooter shooter = visualRoot.GetComponentInChildren<ProjectileShooter>(true);
            if (shooter != null)
                return shooter.transform;

            return visualRoot;
        }

        private void CacheComponents()
        {
            if (cachedAnimatorBridge == null)
                cachedAnimatorBridge = GetComponentInChildren<AgentAnimatorBridge>(true);

            cachedAnimator = ResolvePlayableAnimator();
            cachedVisualRoot = ResolveVisualRoot();

            if (cachedProjectileShooter == null)
                cachedProjectileShooter = GetComponentInChildren<ProjectileShooter>(true);
        }

        private Animator ResolvePlayableAnimator()
        {
            if (cachedAnimatorBridge != null && cachedAnimatorBridge.CachedAnimator != null)
                return cachedAnimatorBridge.CachedAnimator;

            if (cachedAnimator != null && cachedAnimator.runtimeAnimatorController != null)
                return cachedAnimator;

            Animator[] animators = GetComponentsInChildren<Animator>(true);
            foreach (Animator candidate in animators)
            {
                if (candidate != null && candidate.runtimeAnimatorController != null)
                    return candidate;
            }

            return animators.Length > 0 ? animators[0] : null;
        }

        private Animator ResolvePlayableAnimatorFromRoot(Transform visualRoot)
        {
            if (visualRoot == null)
                return null;

            AgentAnimatorBridge bridge = visualRoot.GetComponentInChildren<AgentAnimatorBridge>(true);
            if (bridge != null && bridge.CachedAnimator != null)
                return bridge.CachedAnimator;

            Animator[] animators = visualRoot.GetComponentsInChildren<Animator>(true);
            foreach (Animator candidate in animators)
            {
                if (candidate != null && candidate.runtimeAnimatorController != null)
                    return candidate;
            }

            return animators.Length > 0 ? animators[0] : null;
        }

        private Transform ResolveVisualRoot()
        {
            if (runtimeVisualInstance != null)
                return ResolvePrimaryVisualRoot(runtimeVisualInstance.transform) ?? transform;

            Transform source = null;

            if (cachedAnimatorBridge != null && cachedAnimatorBridge.CachedAnimator != null)
                source = cachedAnimatorBridge.CachedAnimator.transform;
            else if (cachedAnimator != null)
                source = cachedAnimator.transform;
            else if (cachedProjectileShooter != null)
                source = cachedProjectileShooter.transform;

            if (source == null)
                return transform;

            while (source.parent != null && source.parent != transform)
                source = source.parent;

            return source.parent == transform ? source : transform;
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

            if (cachedAnimatorBridge != null && cachedAnimatorBridge.IsInSkillState())
                return;

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
            if (target == null)
                return;

            Vector3 directionToTarget = target.transform.position - transform.position;
            directionToTarget.y = 0f;

            if (directionToTarget.sqrMagnitude <= 0.0001f)
                return;

            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget.normalized);
            Quaternion modelOffset = Quaternion.Euler(modelRotationOffset);
            Transform rotationTarget = cachedVisualRoot != null ? cachedVisualRoot : transform;
            rotationTarget.rotation = targetRotation * modelOffset;
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

        public void ApplyAttackHit_01()
        {
            ApplySingleAttackHit();
        }

        public void ApplyAttackHit_02()
        {
            ApplySingleAttackHit();
        }

        public void FireNormalAttack()
        {
            CacheComponents();

            if (cachedProjectileShooter != null && cachedProjectileShooter.CanFireNormalAttack())
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

            Transform vfxAnchor = cachedVisualRoot != null ? cachedVisualRoot : transform;
            Vector3 spawnPos = vfxAnchor.position + vfxAnchor.TransformDirection(agentData.normalAttackVfxOffset);
            GameObject vfx = Instantiate(agentData.normalAttackVfxPrefab, spawnPos, vfxAnchor.rotation);
            if (vfx == null)
                return;

            float lifetime = agentData.normalAttackVfxLifetime > 0f
                ? agentData.normalAttackVfxLifetime
                : 1f;

            StopAndDestroyVfx(vfx, lifetime);
        }

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





