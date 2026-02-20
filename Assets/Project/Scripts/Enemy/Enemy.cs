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
    private static readonly int BaseColorPropertyId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorPropertyId = Shader.PropertyToID("_Color");

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

    [Header("Attack")]
    [SerializeField] private float arriveDistance = 0.6f;

    private float baseMoveSpeed;
    private CombatStats baseCombatStats;
    private CombatStats currentCombatStats;
    private float currentHP;
    private Transform target;
    private EnemyPool ownerPool;
    private BaseHealth targetBaseHealth;
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

    public float Defense => currentCombatStats.def;

    void Awake()
    {
        cachedRigidbody = GetComponent<Rigidbody>();
        cachedNavMeshAgent = GetComponent<NavMeshAgent>();
        ResolveRenderer();
        ResolveMotionBlur();

        baseMoveSpeed = moveSpeed;
        baseCombatStats = new CombatStats(maxHP, attackDamage, 0f, 0f, 1f).Sanitized();
        currentCombatStats = baseCombatStats;
    }

    void Update()
    {
        if (!isAlive || target == null)
        {
            return;
        }

        Vector3 dir = (target.position - transform.position).normalized;
        transform.position += dir * moveSpeed * Time.deltaTime;

        float sqrDistance = (target.position - transform.position).sqrMagnitude;
        if (sqrDistance <= arriveDistance * arriveDistance)
        {
            AttackBaseAndDespawn();
        }
    }

    public void SetPool(EnemyPool pool) => ownerPool = pool;

    public void Init(Transform arkTarget, string monsterId, MonsterGrade grade, WaveMultipliers waveMultipliers, MonsterTable monsterTable)
    {
        if (arkTarget == null || ownerPool == null)
        {
            return;
        }

        ApplyMonsterBase(monsterTable, monsterId);
        ApplyGradeScale(grade);
        ApplyWaveMultipliers(waveMultipliers);

        target = arkTarget;
        targetBaseHealth = target.GetComponent<BaseHealth>();
        currentHP = currentCombatStats.hp;
        isAlive = true;
        isInPool = false;

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

        if (currentHP <= 0f)
        {
            ReturnToPool();
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
        targetBaseHealth = null;
        isInPool = true;
        feedbackSequence?.Kill();
        colorFeedbackTween?.Kill();
        RestoreBaseHitColor();
        transform.DOKill();
        ResetMotion();
    }

    public bool IsInPool() => isInPool;

    public void OnSpawnedFromPool(Transform arkTarget, MonsterTable monsterTable, string enemyId, MonsterGrade grade, WaveMultipliers multipliers)
    {
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

    private void ReturnToPool()
    {
        if (ownerPool != null && !isInPool)
        {
            ownerPool.Return(this);
        }
    }

    private void ApplyMonsterBase(MonsterTable monsterTable, string enemyId)
    {
        baseMoveSpeed = moveSpeed;
        baseCombatStats = new CombatStats(maxHP, attackDamage, 0f, 0f, 1f).Sanitized();

        if (monsterTable == null)
        {
            return;
        }

        string id = string.IsNullOrWhiteSpace(enemyId) ? defaultMonsterId : enemyId;
        MonsterRow row = monsterTable.GetById(id) ?? monsterTable.GetById(defaultMonsterId);
        if (row == null)
        {
            return;
        }

        baseCombatStats = row.ToCombatStats().Sanitized();
        if (row.moveSpeed > 0f)
        {
            baseMoveSpeed = row.moveSpeed;
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
        currentCombatStats = new CombatStats(
            currentCombatStats.hp * Mathf.Max(0f, multipliers.hp),
            currentCombatStats.atk * Mathf.Max(0f, multipliers.damage),
            currentCombatStats.def,
            currentCombatStats.critChance,
            currentCombatStats.critMultiplier).Sanitized();

        maxHP = currentCombatStats.hp;
        attackDamage = currentCombatStats.atk;
    }

    private void AttackBaseAndDespawn()
    {
        if (targetBaseHealth == null && target != null)
        {
            targetBaseHealth = target.GetComponent<BaseHealth>();
        }

        if (targetBaseHealth != null)
        {
            targetBaseHealth.TakeDamage(Mathf.Max(0f, currentCombatStats.atk));
        }

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
        if (cachedRigidbody != null)
        {
            cachedRigidbody.velocity = Vector3.zero;
            cachedRigidbody.angularVelocity = Vector3.zero;
        }

        if (cachedNavMeshAgent != null)
        {
            cachedNavMeshAgent.ResetPath();
            cachedNavMeshAgent.velocity = Vector3.zero;
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
}
