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
    // ─── Animator Run State 자동 탐지 ──────────────────────────────────────────
    // _run 이 포함된 State 이름을 런타임에 자동으로 탐지합니다.
    // 특정 이름을 직접 지정하려면 Inspector의 runStateOverride에 입력하세요.
    [SerializeField, Tooltip("비워두면 이름에 '_run'이 포함된 State를 자동 탐지합니다.")]
    private string runStateOverride = string.Empty;
    private string resolvedRunStateName = string.Empty; // 런타임 캐시
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

    [Header("Hit VFX")]
    [SerializeField] private GameObject normalHitVfxPrefab;
    [SerializeField] private GameObject critHitVfxPrefab;
    [SerializeField] private GameObject passionHitVfxPrefab;
    [SerializeField] private GameObject intuitionHitVfxPrefab;
    [SerializeField] private GameObject reasonHitVfxPrefab;

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
        if (arkTarget == null) return;

        // ownerPool이 null이면 EnemyPool.Instance로 자동 복구
        // (CreateOne → SetPool 타이밍 문제, 또는 직접 Instantiate된 경우 대비)
        if (ownerPool == null)
        {
            ownerPool = EnemyPool.Instance;
            if (ownerPool == null)
            {
                Debug.LogError($"[Enemy] Init 실패: ownerPool과 EnemyPool.Instance 모두 null. name={gameObject.name}", this);
                return;
            }
            Debug.LogWarning($"[Enemy] ownerPool이 null이어서 EnemyPool.Instance로 자동 복구했습니다. name={gameObject.name}", this);
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

        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.Register(this);
            isRegistered = true;
        }
    }

    public void TakeDamage(int dmg, bool isCrit, ElementType element)
    {
        if (!isAlive)
        {
            return;
        }

        currentHP -= dmg;

        SpawnDamageText(dmg, isCrit);
        SpawnHitVfx(isCrit, element);
        PlayHitFeedback(isCrit);
        TrySetAnimatorTrigger(HitTriggerId, hasHitTrigger);

        if (currentHP <= 0f)
        {
            Debug.Log($"[Enemy] Die name={name} monsterId={MonsterId} grade={Grade}");
            HandleDeath();
        }
    }

    public void TakeDamage(int dmg, bool isCrit) => TakeDamage(dmg, isCrit, ElementType.Reason);

    public void TakeDamage(int dmg) => TakeDamage(dmg, false, ElementType.Reason);

    public void TakeDamage(float dmg) => TakeDamage(Mathf.RoundToInt(dmg), false, ElementType.Reason);

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
        string monsterId = MonsterId;
        MonsterGrade grade = Grade;
        Debug.Log($"[Enemy] Killed. id={monsterId} grade={grade}");
        EnemyKilled?.Invoke(this);
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

    private void SpawnHitVfx(bool isCrit, ElementType element)
    {
        GameObject hitVfxPrefab = normalHitVfxPrefab;

        if (isCrit && critHitVfxPrefab != null)
        {
            hitVfxPrefab = critHitVfxPrefab;
        }
        else
        {
            switch (element)
            {
                case ElementType.Passion:
                    if (passionHitVfxPrefab != null)
                    {
                        hitVfxPrefab = passionHitVfxPrefab;
                    }
                    break;
                case ElementType.Intuition:
                    if (intuitionHitVfxPrefab != null)
                    {
                        hitVfxPrefab = intuitionHitVfxPrefab;
                    }
                    break;
                case ElementType.Reason:
                    if (reasonHitVfxPrefab != null)
                    {
                        hitVfxPrefab = reasonHitVfxPrefab;
                    }
                    break;
            }
        }

        if (hitVfxPrefab == null)
        {
            return;
        }

        Vector3 hitVfxPosition = transform.position + Vector3.up * 1.0f;
        GameObject hitVfx = Instantiate(hitVfxPrefab, hitVfxPosition, Quaternion.identity);
        if (hitVfx != null)
        {
            Destroy(hitVfx, 2f);
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

    /// <summary>
    /// Animator의 Layer 0에서 이름에 "_run"이 포함된 State를 탐지하여 반환합니다.
    /// runStateOverride가 지정된 경우 해당 이름을 우선 사용합니다.
    /// </summary>
    private string ResolveRunStateName(Animator animator)
    {
        if (animator == null) return string.Empty;

        // Inspector에서 직접 지정한 경우 우선 사용
        if (!string.IsNullOrWhiteSpace(runStateOverride))
        {
            int overrideHash = Animator.StringToHash(runStateOverride);
            if (animator.HasState(0, overrideHash))
                return runStateOverride;

            Debug.LogWarning($"[Enemy] runStateOverride='{runStateOverride}'가 Animator에 없습니다. 자동 탐지를 시도합니다.", this);
        }

        // RuntimeAnimatorController에서 클립 이름 기반으로 _run 탐지
        // Unity 런타임에서는 State 이름을 직접 열거할 수 없으므로
        // AnimationClip 이름으로 후보를 만들고 HasState로 검증합니다.
        RuntimeAnimatorController rac = animator.runtimeAnimatorController;
        if (rac == null) return string.Empty;

        foreach (AnimationClip clip in rac.animationClips)
        {
            if (clip == null) continue;
            string clipName = clip.name;

            // clip 이름에 _run(대소문자 무관)이 포함된 경우 State 이름으로 시도
            if (clipName.IndexOf("_run", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                clipName.IndexOf("_Run", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                int hash = Animator.StringToHash(clipName);
                if (animator.HasState(0, hash))
                {
                    Debug.Log($"[Enemy] Run State 자동 탐지 성공: '{clipName}' (on '{name}')", this);
                    return clipName;
                }
            }
        }

        // 못 찾은 경우 경고
        Debug.LogWarning($"[Enemy] '_run'이 포함된 Animator State를 찾지 못했습니다. " +
                         $"Inspector의 runStateOverride에 State 이름을 직접 입력하거나 " +
                         $"Animator Controller의 State 이름에 '_run'을 포함시켜 주세요. (on '{name}')", this);
        return string.Empty;
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
        resolvedRunStateName = ResolveRunStateName(cachedAnimator);
        hasMonsterRunState = !string.IsNullOrEmpty(resolvedRunStateName);
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

        // hasMonsterRunState가 true일 때만 Play 호출
        // false이면 Entry → 기본 State로 자동 전이되므로 별도 Play 불필요
        if (hasMonsterRunState)
        {
            cachedAnimator.Play(resolvedRunStateName, 0, 0f);
            cachedAnimator.Update(0f);
        }

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

            cachedAnimator.Play(resolvedRunStateName, 0, 0f);
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
