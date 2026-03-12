using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using ProjectFirst.Data;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using DG.Tweening;

namespace Project
{

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
    // ??? Animator Run State ?먮룞 ?먯? ??????????????????????????????????????????
    // _run ???ы븿??State ?대쫫???고??꾩뿉 ?먮룞?쇰줈 ?먯??⑸땲??
    // ?뱀젙 ?대쫫??吏곸젒 吏?뺥븯?ㅻ㈃ Inspector??runStateOverride???낅젰?섏꽭??
    [SerializeField, Tooltip("鍮꾩썙?먮㈃ ?대쫫??'_run'???ы븿??State瑜??먮룞 ?먯??⑸땲??")]
    private string runStateOverride = string.Empty;
    private string resolvedRunStateName = string.Empty; // ?고???罹먯떆
    private static readonly int SpeedParamId = Animator.StringToHash("Speed");
    private static readonly int IsMovingParamId = Animator.StringToHash("IsMoving");
    private static readonly int HitTriggerId = Animator.StringToHash("Hit");
    private static readonly int DieTriggerId = Animator.StringToHash("Die");

    private float CurrentHP => currentHP;
    private CombatStats Stats => currentCombatStats;

    [Header("Enemy Stats")]
    public float moveSpeed = 2f;
    [SerializeField] private int defaultMonsterId = 1;

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
    [Header("Hit Effect (?뚯씠釉?沅뚯옣)")]
    [Tooltip("HitEffectTable???곌껐?섎㈃ ?꾨옒 媛쒕퀎 ?щ’蹂대떎 ?곗꽑 ?곸슜?⑸땲??\n" +
             "Assets/Project/Data/HitEffectTable.asset")]
    [SerializeField] private HitEffectTable hitEffectTable;

    [Header("Hit Effect (媛쒕퀎 ?ㅼ젙 - ?뚯씠釉?誘몄궗????")]
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

    // ?? ?붾쾭????????????????????????????????????????????????????????????????
    private float  debuffSpeedFactor    = 1f;
    private float  debuffAtkFactor      = 1f;
    private float  debuffDefFactor      = 1f;
    private float  debuffEndTime        = 0f;
    private bool   hasDebuff            = false;
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
    private EnemyDeathEffect deathEffect;
    private bool hasMonsterRunState;
    private bool hasPlayedRunFallback;
    private bool hasHitBarrier;
    private int appliedMonsterId = 1;
    private MonsterGrade appliedMonsterGrade = MonsterGrade.Normal;
    private string appliedMoveSpeedSource = "default";
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private bool hasLoggedMoveSpeedForSpawn;
#endif

    private bool UsesRigidbodyMovement => cachedRigidbody != null && cachedRigidbody.isKinematic && cachedRigidbody.gameObject.activeInHierarchy;

    public ElementType Element => currentElement;
    private ElementType currentElement = ElementType.Reason;

    public float Defense => currentCombatStats.def;
    public bool IsAlive => isAlive;
    public int MonsterId => appliedMonsterId;
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
        baseCombatStats = new CombatStats(10f, 1f, 0f, 0f, 1f).Sanitized();
        currentElement = ElementType.Reason;
        appliedMonsterId = defaultMonsterId;
        appliedMoveSpeedSource = "default";
        currentCombatStats = baseCombatStats;
        deathEffect = GetComponent<EnemyDeathEffect>();
    }

    void Update()
    {
        if (!isAlive || target == null)
        {
            return;
        }

        UpdateDebuff();

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
        float effectiveSpeed = moveSpeed * (hasDebuff ? debuffSpeedFactor : 1f);
        Vector3 nextPos = transform.position + dir * effectiveSpeed * dt;
        transform.position = nextPos;
    }

    private void MoveByRigidbody(float fixedDt)
    {
        Vector3 dir = (target.position - transform.position).normalized;
        float effectiveSpeed = moveSpeed * (hasDebuff ? debuffSpeedFactor : 1f);
        Vector3 nextPos = transform.position + dir * effectiveSpeed * fixedDt;
        cachedRigidbody.MovePosition(nextPos);
    }

    public void SetPool(EnemyPool pool) => ownerPool = pool;

    public void Init(Transform arkTarget, int monsterId, MonsterGrade grade, WaveMultipliers waveMultipliers, MonsterTable monsterTable)
    {
        if (arkTarget == null) return;

        // ownerPool??null?대㈃ EnemyPool.Instance濡??먮룞 蹂듦뎄
        // (CreateOne ??SetPool ??대컢 臾몄젣, ?먮뒗 吏곸젒 Instantiate??寃쎌슦 ?鍮?
        if (ownerPool == null)
        {
            ownerPool = EnemyPool.Instance;
            if (ownerPool == null)
            {
                Debug.LogError($"[Enemy] Init ?ㅽ뙣: ownerPool怨?EnemyPool.Instance 紐⑤몢 null. name={gameObject.name}", this);
                return;
            }
            Debug.LogWarning($"[Enemy] ownerPool??null?댁뼱??EnemyPool.Instance濡??먮룞 蹂듦뎄?덉뒿?덈떎. name={gameObject.name}", this);
        }

        int resolvedMonsterId = monsterId <= 0 ? defaultMonsterId : monsterId;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        hasLoggedMoveSpeedForSpawn = false;
#endif
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
        deathEffect?.SetTarget(target);
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

    /// <summary>?ㅽ궗 ?쇨꺽 (?띿꽦 VFX ?곸슜)</summary>
    public void TakeDamage(int dmg, bool isCrit, ElementType element)
        => TakeDamageInternal(dmg, isCrit, element, isSkillHit: true);

    private void TakeDamageInternal(int dmg, bool isCrit, ElementType element, bool isSkillHit)
    {
        if (!isAlive)
        {
            return;
        }

        currentHP -= dmg;

        SpawnDamageText(dmg, isCrit);
        SpawnHitVfx(isCrit, element, isSkillHit);
        PlayHitFeedback(isCrit);
        TrySetAnimatorTrigger(HitTriggerId, hasHitTrigger);

        if (currentHP <= 0f)
        {
            Debug.Log($"[Enemy] Die name={name} monsterId={MonsterId} grade={Grade}");
            HandleDeath();
        }
    }

    /// <summary>?쇰컲 怨듦꺽 ?쇨꺽 (?ㅽ궗 ?꾨떂)</summary>
    public void TakeDamage(int dmg, bool isCrit) => TakeDamageInternal(dmg, isCrit, ElementType.Reason, false);

    public void TakeDamage(int dmg) => TakeDamageInternal(dmg, false, ElementType.Reason, false);

    public void TakeDamage(float dmg) => TakeDamageInternal(Mathf.RoundToInt(dmg), false, ElementType.Reason, false);

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
        deathEffect?.ResetState();
        ResetMotion();
    }

    public bool IsInPool() => isInPool;

    public void OnSpawnedFromPool(Transform arkTarget, MonsterTable monsterTable, int enemyId, MonsterGrade grade, WaveMultipliers multipliers)
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

    private void ApplyMonsterBase(MonsterTable monsterTable, int enemyId, MonsterGrade grade)
    {
        baseMoveSpeed = moveSpeed;
        baseCombatStats = new CombatStats(10f, 1f, 0f, 0f, 1f).Sanitized();
        currentElement = ElementType.Reason;
        appliedMonsterId = defaultMonsterId;
        appliedMoveSpeedSource = "default";

        if (monsterTable == null)
        {
            return;
        }

        int id = enemyId <= 0 ? defaultMonsterId : enemyId;
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
    }

    private void HandleDeath()
    {
        if (!isAlive || isDeathReturning)
        {
            return;
        }

        isAlive = false;
        int monsterId = MonsterId;
        MonsterGrade grade = Grade;
        Debug.Log($"[Enemy] Killed. id={monsterId} grade={grade}");
        EnemyKilled?.Invoke(this);

        // EnemyDeathEffect 而댄룷?뚰듃媛 ?덉쑝硫??섍렇???곗텧濡??泥?
        if (deathEffect != null)
        {
            isDeathReturning = true;
            deathEffect.Play(() =>
            {
                isDeathReturning = false;
                ReturnToPool();
            });
            return;
        }

        // 湲곗〈 ?숈옉: Die ?좊땲 + 吏??諛섑솚
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

    /// <summary>
    /// ?쇨꺽 VFX ?ㅽ룿.
    /// hitEffectTable???곌껐??寃쎌슦 ?뚯씠釉붿쓣 ?곗꽑 ?ъ슜?⑸땲??
    /// isSkillHit=true?대㈃ ?띿꽦蹂?VFX瑜??좏깮?⑸땲??
    /// </summary>
    // ?? ?붾쾭??API ??????????????????????????????????????????????????????????

    public void ApplyDebuff(DebuffType debuffType, float value, float duration)
    {
        if (!isAlive) return;
        debuffEndTime = Time.unscaledTime + duration;
        hasDebuff = true;
        switch (debuffType)
        {
            case DebuffType.Slow:
                debuffSpeedFactor = Mathf.Clamp(1f - value, 0.1f, 1f);
                break;
            case DebuffType.WeakenAtk:
                debuffAtkFactor = Mathf.Clamp(1f - value, 0.1f, 1f);
                break;
            case DebuffType.WeakenDef:
                debuffDefFactor = Mathf.Clamp(1f - value, 0.1f, 1f);
                break;
        }
        Debug.Log($"[Enemy] {gameObject.name} ?붾쾭???곸슜: {debuffType} {value*100:F0}% / {duration}s");
    }

    private void UpdateDebuff()
    {
        if (!hasDebuff) return;
        if (Time.unscaledTime >= debuffEndTime)
        {
            hasDebuff = false;
            debuffSpeedFactor = debuffAtkFactor = debuffDefFactor = 1f;
        }
    }

    private void SpawnHitVfx(bool isCrit, ElementType element, bool isSkillHit = false)
    {
        GameObject hitVfxPrefab;

        // ?? 1?쒖쐞: HitEffectTable ?????????????????????????????????????????????
        if (hitEffectTable != null)
        {
            hitVfxPrefab = hitEffectTable.Resolve(isCrit, element, isSkillHit);
        }
        // ?? 2?쒖쐞: 媛쒕퀎 ?꾨뱶 (湲곗〈 諛⑹떇) ????????????????????????????????????????
        else
        {
            hitVfxPrefab = normalHitVfxPrefab;

            if (isCrit && critHitVfxPrefab != null)
            {
                hitVfxPrefab = critHitVfxPrefab;
            }
            else if (isSkillHit)
            {
                hitVfxPrefab = element switch
                {
                    ElementType.Passion   => passionHitVfxPrefab   ?? normalHitVfxPrefab,
                    ElementType.Intuition => intuitionHitVfxPrefab ?? normalHitVfxPrefab,
                    ElementType.Reason    => reasonHitVfxPrefab    ?? normalHitVfxPrefab,
                    _                     => normalHitVfxPrefab
                };
            }
        }

        if (hitVfxPrefab == null) return;

        Vector3 pos = transform.position + Vector3.up * 1.0f;
        GameObject vfx = Instantiate(hitVfxPrefab, pos, Quaternion.identity);
        if (vfx != null)
            StopAndDestroyVfx(vfx, 2f);
    }

    private static void StopAndDestroyVfx(GameObject vfx, float delay)
    {
        foreach (ParticleSystem ps in vfx.GetComponentsInChildren<ParticleSystem>(true))
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        Destroy(vfx, Mathf.Max(0.05f, delay));
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
        // ?곕?吏 怨꾩궛???곸슜 (諛⑹뼱??0, ?띿꽦 以묐┰(Reason) 媛??
        int dmg = DamageCalculator.ComputeDamage(
            currentCombatStats.atk,
            0f, 
            currentCombatStats.critChance,
            currentCombatStats.critMultiplier,
            currentElement,
            ElementType.Reason, 
            out _);
        barrierHealth.TakeDamage(dmg);

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
    /// Animator??Layer 0?먯꽌 ?대쫫??"_run"???ы븿??State瑜??먯??섏뿬 諛섑솚?⑸땲??
    /// runStateOverride媛 吏?뺣맂 寃쎌슦 ?대떦 ?대쫫???곗꽑 ?ъ슜?⑸땲??
    /// </summary>
    private string ResolveRunStateName(Animator animator)
    {
        if (animator == null) return string.Empty;

        // Inspector?먯꽌 吏곸젒 吏?뺥븳 寃쎌슦 ?곗꽑 ?ъ슜
        if (!string.IsNullOrWhiteSpace(runStateOverride))
        {
            int overrideHash = Animator.StringToHash(runStateOverride);
            if (animator.HasState(0, overrideHash))
                return runStateOverride;

            Debug.LogWarning($"[Enemy] runStateOverride='{runStateOverride}'媛 Animator???놁뒿?덈떎. ?먮룞 ?먯?瑜??쒕룄?⑸땲??", this);
        }

        // RuntimeAnimatorController?먯꽌 ?대┰ ?대쫫 湲곕컲?쇰줈 _run ?먯?
        // Unity ?고??꾩뿉?쒕뒗 State ?대쫫??吏곸젒 ?닿굅?????놁쑝誘濡?
        // AnimationClip ?대쫫?쇰줈 ?꾨낫瑜?留뚮뱾怨?HasState濡?寃利앺빀?덈떎.
        RuntimeAnimatorController rac = animator.runtimeAnimatorController;
        if (rac == null) return string.Empty;

        foreach (AnimationClip clip in rac.animationClips)
        {
            if (clip == null) continue;
            string clipName = clip.name;

            // clip ?대쫫??_run(??뚮Ц??臾닿?)???ы븿??寃쎌슦 State ?대쫫?쇰줈 ?쒕룄
            if (clipName.IndexOf("_run", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                clipName.IndexOf("_Run", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                int hash = Animator.StringToHash(clipName);
                if (animator.HasState(0, hash))
                {
                    Debug.Log($"[Enemy] Run State ?먮룞 ?먯? ?깃났: '{clipName}' (on '{name}')", this);
                    return clipName;
                }
            }
        }

        // 紐?李얠? 寃쎌슦 寃쎄퀬
        Debug.LogWarning($"[Enemy] '_run'???ы븿??Animator State瑜?李얠? 紐삵뻽?듬땲?? " +
                         $"Inspector??runStateOverride??State ?대쫫??吏곸젒 ?낅젰?섍굅??" +
                         $"Animator Controller??State ?대쫫??'_run'???ы븿?쒖폒 二쇱꽭?? (on '{name}')", this);
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

        // hasMonsterRunState媛 true???뚮쭔 Play ?몄텧
        // false?대㈃ Entry ??湲곕낯 State濡??먮룞 ?꾩씠?섎?濡?蹂꾨룄 Play 遺덊븘??
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
} // namespace Project




