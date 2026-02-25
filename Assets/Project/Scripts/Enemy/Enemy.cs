using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Sirenix.OdinInspector;

public struct WaveMultipliers
{
    public float hp;
    public float speed;
    public float damage;

    public static WaveMultipliers Default => new WaveMultipliers { hp = 1f, speed = 1f, damage = 1f };
}

public class Enemy : MonoBehaviour
{
    public static event Action<Enemy> EnemyKilled;
    private static readonly int BaseColorPropertyId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorPropertyId = Shader.PropertyToID("_Color");
    private const string MonsterRunStateName = "Monster_run";
    private static readonly int SpeedParamId = Animator.StringToHash("Speed");
    private static readonly int IsMovingParamId = Animator.StringToHash("IsMoving");
    private static readonly int HitTriggerId = Animator.StringToHash("Hit");
    private static readonly int DieTriggerId = Animator.StringToHash("Die");

    [ShowInInspector, ReadOnly] private float CurrentHP => currentHP;
    [ShowInInspector, ReadOnly] private CombatStats Stats => currentCombatStats;

    [Header("Enemy Stats")]
    public float moveSpeed = 2f;
    public float maxHP = 10f;
    public float attackDamage = 1f;
    [SerializeField] private string defaultMonsterId = "slime";

    [Header("Grade Scale")]
    [SerializeField] private float eliteHpScale = 1.5f;
    [SerializeField] private float eliteAtkScale = 1.3f;
    [SerializeField] private float eliteDefScale = 1.2f;
    [SerializeField] private float eliteSpeedScale = 1.1f;
    [SerializeField] private float bossHpScale = 3f;
    [SerializeField] private float bossAtkScale = 2f;
    [SerializeField] private float bossDefScale = 1.8f;
    [SerializeField] private float bossSpeedScale = 1.2f;

    [Header("Damage Feedback")]
    [SerializeField] private DamageText damageTextPrefab;
    [SerializeField] private Transform damageTextAnchor;
    [SerializeField] private Renderer colorRenderer;
    [SerializeField] private float shakeDuration = 0.18f;
    [SerializeField] private float shakeStrength = 0.2f;

    [Header("Motion Blur")]
    [SerializeField] private Volume postProcessVolume;
    [SerializeField] private float critMotionBlurIntensity = 0.85f;
    [SerializeField] private float critMotionBlurDuration = 0.12f;

    [Header("Animation")]
    [SerializeField] private bool useDeathReturnDelay = true;
    [SerializeField, Min(0f)] private float deathReturnDelay = 0.45f;

    [Header("Debug")]
    [SerializeField] private bool logSpawnAppliedMonsterData;

    private const float MinimumMoveSpeed = 0.5f;

    private float baseMoveSpeed;
    private CombatStats baseCombatStats;
    private CombatStats currentCombatStats;
    private float currentHP;
    private Transform target;
    private EnemyPool ownerPool;
    private bool isAlive;
    private bool isRegistered;
    private bool isInPool;
    private Rigidbody cachedRigidbody;
    private NavMeshAgent cachedNavMeshAgent;
    private MotionBlur cachedMotionBlur;
    private float baseMotionBlur;
    private Sequence feedbackSequence;
    private Tween colorFeedbackTween;
    private MaterialPropertyBlock colorPropertyBlock;
    private int hitColorPropertyId = -1;
    private Color baseHitColor = Color.white;
    private Animator cachedAnimator;
    private bool hasSpeedParam;
    private bool hasIsMovingParam;
    private bool hasHitTrigger;
    private bool hasDieTrigger;
    private bool hasWarnedAnimatorMissing;
    private bool isDeathReturning;
    private bool hasMonsterRunState;
    private bool hasPlayedRunFallback;
    private bool hasHitBarrier;
    private string appliedMonsterId = string.Empty;
    private MonsterGrade appliedMonsterGrade = MonsterGrade.Normal;
    private string appliedMoveSpeedSource = "default";
    private bool hasLoggedMoveSpeedForSpawn;
    private bool shouldNotifyKilled;

    private bool UsesRigidbodyMovement => cachedRigidbody != null && cachedRigidbody.isKinematic && cachedRigidbody.gameObject.activeInHierarchy;

    public ElementType Element => currentElement;
    private ElementType currentElement = ElementType.Reason;

    public float Defense => currentCombatStats.def;
    public bool IsAlive => isAlive;
    public string MonsterId => appliedMonsterId;
    public MonsterGrade Grade => appliedMonsterGrade;

    void Awake()
    {
        cachedRigidbody = GetComponent<Rigidbody>();
        cachedNavMeshAgent = GetComponent<NavMeshAgent>();
        ConfigureNavMeshAgentGuard();
        ResolveRenderer();
        ResolveMotionBlur();
        ResolveAnimator();

        baseMoveSpeed = moveSpeed;
        baseCombatStats = new CombatStats(maxHP, attackDamage, 0f, 0f, 1f).Sanitized();
        currentElement = ElementType.Reason;
        appliedMonsterId = defaultMonsterId;
        appliedMoveSpeedSource = "default";
        currentCombatStats = baseCombatStats;
    }

    void Update()
    {
        if (!isAlive || target == null)
        {
            return;
        }

        if (!UsesRigidbodyMovement)
        {
            MoveByTransform(Time.deltaTime);
        }

        UpdateMovementAnimation(moveSpeed);
    }

    private void FixedUpdate()
    {
        if (!isAlive || target == null)
        {
            return;
        }

        if (UsesRigidbodyMovement)
        {
            MoveByRigidbody(Time.fixedDeltaTime);
        }
    }

    private void MoveByTransform(float dt)
    {
        Vector3 dir = (target.position - transform.position).normalized;
        Vector3 nextPos = transform.position + dir * moveSpeed * dt;
        transform.position = nextPos;
    }

    private void MoveByRigidbody(float fixedDt)
    {
        Vector3 dir = (target.position - transform.position).normalized;
        Vector3 nextPos = transform.position + dir * moveSpeed * fixedDt;
        cachedRigidbody.MovePosition(nextPos);
    }

    public void SetPool(EnemyPool pool) => ownerPool = pool;

    public void Init(Transform arkTarget, string monsterId, MonsterGrade grade, WaveMultipliers waveMultipliers, MonsterTable monsterTable)
    {
        if (arkTarget == null || ownerPool == null)
        {
            return;
        }

        string resolvedMonsterId = string.IsNullOrWhiteSpace(monsterId) ? defaultMonsterId : monsterId;
        hasLoggedMoveSpeedForSpawn = false;
        ApplyMonsterBase(monsterTable, resolvedMonsterId, grade);
        ApplyGradeScale(grade);
        ApplyWaveMultipliers(waveMultipliers);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (logSpawnAppliedMonsterData && !hasLoggedMoveSpeedForSpawn)
        {
            Debug.Log($"[Enemy] SpawnApplied name='{gameObject.name}' id='{appliedMonsterId}' grade='{grade}' finalMoveSpeed={moveSpeed:F2} element={currentElement} speedSource={appliedMoveSpeedSource}", this);
            hasLoggedMoveSpeedForSpawn = true;
        }
#endif

        appliedMonsterGrade = grade;

        target = arkTarget;
        currentHP = currentCombatStats.hp;
        isAlive = true;
        isInPool = false;
        isDeathReturning = false;
        hasHitBarrier = false;
        shouldNotifyKilled = false;

        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.Register(this);
            isRegistered = true;
        }
    }

    public void TakeDamage(int dmg, bool isCrit)
    {
        if (!isAlive)
        {
            return;
        }

        int finalDamage = Mathf.Max(0, dmg);
        currentHP -= finalDamage;

        SpawnDamageText(finalDamage, isCrit);
        PlayHitFeedback(isCrit);
        TrySetAnimatorTrigger(HitTriggerId, hasHitTrigger);

        if (currentHP <= 0f)
        {
            HandleDeath();
        }
    }

    public void TakeDamage(int dmg) => TakeDamage(dmg, false);

    public void TakeDamage(float dmg) => TakeDamage(Mathf.RoundToInt(dmg), false);

    public void Despawn()
    {
        if (isAlive)
        {
            ReturnToPool();
        }
    }

    public void ResetForPool()
    {
        isAlive = false;
        target = null;
        isInPool = true;
        feedbackSequence?.Kill();
        colorFeedbackTween?.Kill();
        RestoreBaseHitColor();
        transform.DOKill();
        isDeathReturning = false;
        ResetMotion();
    }

    public bool IsInPool() => isInPool;

    public void OnSpawnedFromPool(Transform arkTarget, MonsterTable monsterTable, string enemyId, MonsterGrade grade, WaveMultipliers multipliers)
    {
        CancelInvoke(nameof(ReturnToPoolAfterDeathDelay));
        InitializeAnimatorOnSpawn();
        Init(arkTarget, enemyId, grade, multipliers, monsterTable);
    }

    public void OnReturnedToPool()
    {
        if (isRegistered && EnemyManager.Instance != null)
        {
            EnemyManager.Instance.Unregister(this);
            isRegistered = false;
        }

        ResetForPool();
    }

    private void ConfigureNavMeshAgentGuard()
    {
        if (cachedNavMeshAgent == null)
        {
            return;
        }

        if (cachedRigidbody != null)
        {
            cachedNavMeshAgent.enabled = false;
        }
    }

    private void ReturnToPool()
    {
        if (shouldNotifyKilled)
        {
            shouldNotifyKilled = false;
            EnemyKilled?.Invoke(this);
            Debug.Log($"[Enemy] Killed. id={MonsterId} grade={Grade}");
        }

        if (ownerPool != null && !isInPool)
        {
            ownerPool.Return(this);
        }
    }

    private void ApplyMonsterBase(MonsterTable monsterTable, string enemyId, MonsterGrade grade)
    {
        baseMoveSpeed = moveSpeed;
        baseCombatStats = new CombatStats(maxHP, attackDamage, 0f, 0f, 1f).Sanitized();
        currentElement = ElementType.Reason;
        appliedMonsterId = defaultMonsterId;
        appliedMoveSpeedSource = "default";

        if (monsterTable == null)
        {
            return;
        }

        string id = string.IsNullOrWhiteSpace(enemyId) ? defaultMonsterId : enemyId;
        MonsterRow row = monsterTable.GetByIdAndGrade(id, grade)
                         ?? monsterTable.GetByIdAndGrade(defaultMonsterId, grade)
                         ?? monsterTable.GetById(id)
                         ?? monsterTable.GetById(defaultMonsterId);
        if (row == null)
        {
            return;
        }

        baseCombatStats = row.ToCombatStats().Sanitized();
        currentElement = row.element;
        appliedMonsterId = row.id;
        if (row.moveSpeed > 0f)
        {
            baseMoveSpeed = row.moveSpeed;
            appliedMoveSpeedSource = $"table:{row.id}";
        }
    }

    private void ApplyGradeScale(MonsterGrade grade)
    {
        float hpScale = 1f;
        float atkScale = 1f;
        float defScale = 1f;
        float speedScale = 1f;

        if (grade == MonsterGrade.Elite)
        {
            hpScale = eliteHpScale;
            atkScale = eliteAtkScale;
            defScale = eliteDefScale;
            speedScale = eliteSpeedScale;
        }
        else if (grade == MonsterGrade.Boss)
        {
            hpScale = bossHpScale;
            atkScale = bossAtkScale;
            defScale = bossDefScale;
            speedScale = bossSpeedScale;
        }

        currentCombatStats = new CombatStats(
            baseCombatStats.hp * Mathf.Max(0f, hpScale),
            baseCombatStats.atk * Mathf.Max(0f, atkScale),
            baseCombatStats.def * Mathf.Max(0f, defScale),
            baseCombatStats.critChance,
            baseCombatStats.critMultiplier).Sanitized();

        moveSpeed = baseMoveSpeed * Mathf.Max(0f, speedScale);
    }

    private void ApplyWaveMultipliers(WaveMultipliers multipliers)
    {
        moveSpeed *= Mathf.Max(0f, multipliers.speed);
        moveSpeed = Mathf.Max(MinimumMoveSpeed, moveSpeed);
        currentCombatStats = new CombatStats(
            currentCombatStats.hp * Mathf.Max(0f, multipliers.hp),
            currentCombatStats.atk * Mathf.Max(0f, multipliers.damage),
            currentCombatStats.def,
            currentCombatStats.critChance,
            currentCombatStats.critMultiplier).Sanitized();

        maxHP = currentCombatStats.hp;
        attackDamage = currentCombatStats.atk;
    }

    private void HandleDeath()
    {
        if (!isAlive || isDeathReturning)
        {
            return;
        }

        isAlive = false;
        shouldNotifyKilled = true;
        TrySetAnimatorTrigger(DieTriggerId, hasDieTrigger);

        if (!useDeathReturnDelay || deathReturnDelay <= 0f)
        {
            ReturnToPool();
            return;
        }

        isDeathReturning = true;
        Invoke(nameof(ReturnToPoolAfterDeathDelay), Mathf.Clamp(deathReturnDelay, 0.3f, 0.6f));
    }

    private void ReturnToPoolAfterDeathDelay()
    {
        isDeathReturning = false;
        ReturnToPool();
    }

    private void SpawnDamageText(int damage, bool isCrit)
    {
        if (damageTextPrefab == null)
        {
            return;
        }

        Vector3 spawnPos = damageTextAnchor != null ? damageTextAnchor.position : transform.position + Vector3.up * 1.5f;
        DamageText text = Instantiate(damageTextPrefab, spawnPos, Quaternion.identity);
        if (text != null)
        {
            text.Init(damage, isCrit);
        }
    }

    private void PlayHitFeedback(bool isCrit)
    {
        transform.DOShakePosition(shakeDuration, shakeStrength, 15, 30f, false, true);

        if (!isCrit)
        {
            return;
        }

        if (colorRenderer != null)
        {
            PlayCritColorFeedback();
        }

        transform.DOPunchScale(Vector3.one * 0.2f, 0.2f, 6);
        TriggerCritMotionBlur();
    }

    private void ResolveRenderer()
    {
        if (colorRenderer == null)
        {
            colorRenderer = GetComponentInChildren<Renderer>();
        }

        if (colorRenderer == null)
        {
            hitColorPropertyId = -1;
            return;
        }

        Material sharedMaterial = colorRenderer.sharedMaterial;
        if (sharedMaterial == null)
        {
            hitColorPropertyId = -1;
            return;
        }

        if (sharedMaterial.HasProperty(BaseColorPropertyId))
        {
            hitColorPropertyId = BaseColorPropertyId;
        }
        else if (sharedMaterial.HasProperty(ColorPropertyId))
        {
            hitColorPropertyId = ColorPropertyId;
        }
        else
        {
            hitColorPropertyId = -1;
            return;
        }

        baseHitColor = sharedMaterial.GetColor(hitColorPropertyId);
        colorPropertyBlock = new MaterialPropertyBlock();
        colorRenderer.GetPropertyBlock(colorPropertyBlock);
        colorPropertyBlock.SetColor(hitColorPropertyId, baseHitColor);
        colorRenderer.SetPropertyBlock(colorPropertyBlock);
    }

    private void PlayCritColorFeedback()
    {
        if (colorRenderer == null || hitColorPropertyId < 0)
        {
            return;
        }

        if (colorPropertyBlock == null)
        {
            colorPropertyBlock = new MaterialPropertyBlock();
        }

        colorFeedbackTween?.Kill();

        Sequence sequence = DOTween.Sequence();
        sequence.Append(DOTween.To(() => 0f,
            value => SetRendererHitColor(Color.Lerp(baseHitColor, Color.red, value)),
            1f,
            0.08f));
        sequence.Append(DOTween.To(() => 0f,
            value => SetRendererHitColor(Color.Lerp(Color.red, baseHitColor, value)),
            1f,
            0.12f));
        colorFeedbackTween = sequence;
    }

    private void SetRendererHitColor(Color color)
    {
        if (colorRenderer == null || hitColorPropertyId < 0)
        {
            return;
        }

        colorRenderer.GetPropertyBlock(colorPropertyBlock);
        colorPropertyBlock.SetColor(hitColorPropertyId, color);
        colorRenderer.SetPropertyBlock(colorPropertyBlock);
    }

    private void RestoreBaseHitColor()
    {
        SetRendererHitColor(baseHitColor);
    }

    private void ResolveMotionBlur()
    {
        if (postProcessVolume == null)
        {
            postProcessVolume = FindFirstObjectByType<Volume>();
        }

        if (postProcessVolume != null && postProcessVolume.profile != null)
        {
            postProcessVolume.profile.TryGet(out cachedMotionBlur);
            if (cachedMotionBlur != null)
            {
                baseMotionBlur = cachedMotionBlur.intensity.value;
            }
        }
    }

    private void TriggerCritMotionBlur()
    {
        if (cachedMotionBlur == null)
        {
            return;
        }

        DOTween.To(() => cachedMotionBlur.intensity.value, x => cachedMotionBlur.intensity.value = x, critMotionBlurIntensity, critMotionBlurDuration * 0.5f)
            .OnComplete(() => DOTween.To(() => cachedMotionBlur.intensity.value, x => cachedMotionBlur.intensity.value = x, baseMotionBlur, critMotionBlurDuration));
    }

    private void ResetMotion()
    {
        CancelInvoke(nameof(ReturnToPoolAfterDeathDelay));

        if (cachedRigidbody != null)
        {
            if (!cachedRigidbody.isKinematic)
            {
                cachedRigidbody.velocity = Vector3.zero;
                cachedRigidbody.angularVelocity = Vector3.zero;
            }
        }

        if (cachedNavMeshAgent != null)
        {
            cachedNavMeshAgent.ResetPath();
            cachedNavMeshAgent.velocity = Vector3.zero;
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (!isAlive || hasHitBarrier)
        {
            return;
        }

        if (other == null)
        {
            return;
        }

        Transform otherRoot = other.transform.root;
        Transform targetRoot = target != null ? target.root : null;
        if (targetRoot != null && otherRoot != targetRoot)
        {
            return;
        }

        BaseHealth barrierHealth = other.GetComponent<BaseHealth>();
        if (barrierHealth == null)
        {
            barrierHealth = other.GetComponentInParent<BaseHealth>();
        }

        if (barrierHealth == null)
        {
            return;
        }

        hasHitBarrier = true;
        barrierHealth.TakeDamage(Mathf.Max(0f, currentCombatStats.atk));

        if (!useDeathReturnDelay || deathReturnDelay <= 0f)
        {
            ReturnToPool();
            return;
        }

        if (!isDeathReturning)
        {
            isDeathReturning = true;
            Invoke(nameof(ReturnToPoolAfterDeathDelay), Mathf.Clamp(deathReturnDelay, 0.3f, 0.6f));
        }
    }

    private void OnDisable()
    {
        if (!isInPool && isRegistered && EnemyManager.Instance != null)
        {
            EnemyManager.Instance.Unregister(this);
            isRegistered = false;
        }
    }

    private void ResolveAnimator()
    {
        if (cachedAnimator == null)
        {
            cachedAnimator = GetComponentInChildren<Animator>(true);
        }

        if (cachedAnimator == null)
        {
            if (!hasWarnedAnimatorMissing)
            {
                Debug.LogWarning($"[Enemy] Animator not found on '{name}'. Animation sync will be skipped.", this);
                hasWarnedAnimatorMissing = true;
            }

            return;
        }

        hasSpeedParam = HasAnimatorParameter("Speed", AnimatorControllerParameterType.Float);
        hasIsMovingParam = HasAnimatorParameter("IsMoving", AnimatorControllerParameterType.Bool);
        hasHitTrigger = HasAnimatorParameter("Hit", AnimatorControllerParameterType.Trigger);
        hasDieTrigger = HasAnimatorParameter("Die", AnimatorControllerParameterType.Trigger);
        hasMonsterRunState = cachedAnimator.HasState(0, Animator.StringToHash(MonsterRunStateName));
    }

    private bool HasAnimatorParameter(string parameterName, AnimatorControllerParameterType parameterType)
    {
        if (cachedAnimator == null)
        {
            return false;
        }

        foreach (AnimatorControllerParameter parameter in cachedAnimator.parameters)
        {
            if (parameter.name == parameterName && parameter.type == parameterType)
            {
                return true;
            }
        }

        return false;
    }

    private void InitializeAnimatorOnSpawn()
    {
        ResolveAnimator();
        if (cachedAnimator == null)
        {
            return;
        }

        cachedAnimator.Rebind();
        cachedAnimator.Update(0f);
        cachedAnimator.Play(MonsterRunStateName, 0, 0f);
        cachedAnimator.Update(0f);
        hasPlayedRunFallback = false;
    }

    private void UpdateMovementAnimation(float speedValue)
    {
        if (cachedAnimator == null)
        {
            return;
        }

        bool isMoving = speedValue > 0.01f;

        if (hasSpeedParam)
        {
            cachedAnimator.SetFloat(SpeedParamId, speedValue);
        }

        if (hasIsMovingParam)
        {
            cachedAnimator.SetBool(IsMovingParamId, isMoving);
        }

        if (!hasSpeedParam && !hasIsMovingParam)
        {
            if (!isMoving)
            {
                hasPlayedRunFallback = false;
                return;
            }

            if (hasPlayedRunFallback || !hasMonsterRunState)
            {
                return;
            }

            cachedAnimator.Play(MonsterRunStateName, 0, 0f);
            hasPlayedRunFallback = true;
        }
    }

    private void TrySetAnimatorTrigger(int triggerId, bool hasTrigger)
    {
        if (cachedAnimator == null || !hasTrigger)
        {
            return;
        }

        cachedAnimator.SetTrigger(triggerId);
    }
}
