using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 몬스터 사망 시 물리적으로 날아가며 + 글리치 이펙트로 소멸하는 연출 담당.
/// Enemy 프리팹에 부착(선택).
/// </summary>
public class EnemyDeathEffect : MonoBehaviour
{
    // ─── 날아가는 연출 ──────────────────────────────────────────────────────
    [Header("날아가는 힘 (수평)")]
    [Tooltip("충격으로 날아가는 수평 힘. 클수록 멀리 튕김.")]
    [SerializeField] private float flyPower = 12f;

    [Header("올라가는 계수 (Y축)")]
    [Tooltip("위쪽 상승 힘 계수. 작으면 수평에 가깝게 날아감.")]
    [SerializeField] private float upwardRatio = 0.3f;

    [Header("연출 타이밍")]
    [Tooltip("전체 사망 연출(물리+글리치+축소) 시간(초)")]
    [SerializeField] private float totalDuration = 1.5f;

    [Tooltip("축소 시작 타이밍 비율 [0.1~0.9]")]
    [Range(0.1f, 0.9f)] [SerializeField] private float shrinkStartRatio = 0.4f;

    // ─── 글리치 연출 ──────────────────────────────────────────────────────
    [Header("Glitch Death")]
    [Tooltip("GlitchDeath.shader를 사용하는 머티리얼. Inspector에서 할당.")]
    [SerializeField] private Material glitchDeathMaterial;

    [Tooltip("글리치 UV 흔들림 최대 강도 (0~1)")]
    [SerializeField] private float glitchIntensityMax = 0.8f;

    [Tooltip("색수차(RGB 분리) 최대 값 (0~0.1)")]
    [SerializeField] private float chromaShiftMax = 0.04f;

    [Tooltip("글리치 발광 색상")]
    [SerializeField] private Color glitchEmissionColor = new Color(0f, 1f, 0.8f, 1f);

    [Tooltip("글리치 시작 지연 시간(초). 0이면 사망 직후 즉시 시작.")]
    [SerializeField, Min(0f)] private float glitchStartDelay = 0.1f;

    [Tooltip("글리치 강도가 최대에 도달하는 시간(초)")]
    [SerializeField, Min(0.05f)] private float glitchRampDuration = 0.35f;

    [Tooltip("디졸브 소멸이 시작되는 타이밍 비율 [0.2~0.8]")]
    [Range(0.2f, 0.8f)] [SerializeField] private float dissolveStartRatio = 0.45f;

    // ─── 선택적: 보조 파티클 ──────────────────────────────────────────────
    [Header("보조 파티클 (선택)")]
    [Tooltip("Legacy Particle Pack의 ElectricalSparksEffect 등 할당.")]
    [SerializeField] private GameObject deathSparksPrefab;

    [Tooltip("파티클 자동 소멸 시간(초)")]
    [SerializeField] private float sparksLifetime = 1.5f;

    // ─── 캐싱 ─────────────────────────────────────────────────────────────
    private Rigidbody     _rb;
    private Animator      _anim;
    private Collider      _col;
    private Renderer[]    _renderers;
    private Material[]    _originalMaterials;   // 원본 머티리얼 보존 (풀 반환 시 복원)
    private Transform     _attacker;
    private Coroutine     _glitchCoroutine;

    // ─── Shader Property ID ───────────────────────────────────────────────
    private static readonly int GlitchIntensityId  = Shader.PropertyToID("_GlitchIntensity");
    private static readonly int ChromaShiftId      = Shader.PropertyToID("_ChromaShift");
    private static readonly int DissolveId         = Shader.PropertyToID("_Dissolve");
    private static readonly int GlitchColorId      = Shader.PropertyToID("_GlitchColor");

    private static readonly Vector3 VectorZero = Vector3.zero;
    private static readonly Vector3 VectorOne  = Vector3.one;

    // ─── 런타임 상태 ──────────────────────────────────────────────────────
    // 현재 인스턴스화된 글리치 머티리얼들 (DOKill 및 복원을 위해)
    private Material[] _glitchMaterialInstances;

    private void Awake()
    {
        _rb         = GetComponent<Rigidbody>();
        _anim       = GetComponentInChildren<Animator>();
        _col        = GetComponent<Collider>();
        _renderers  = GetComponentsInChildren<Renderer>(true);

        // 원본 머티리얼 스냅샷 (풀 반환용)
        CacheOriginalMaterials();
    }

    // ────────────────────────────────────────────────────────────────────────
    // Public API
    // ────────────────────────────────────────────────────────────────────────

    /// <summary>공격자 방향을 지정합니다.</summary>
    public void SetTarget(Transform attacker) => _attacker = attacker;

    /// <summary>
    /// 사망 연출 실행. 물리 날아가기 + 글리치 소멸을 동시에 진행하며
    /// 완전히 소멸하면 onComplete를 호출합니다.
    /// </summary>
    public void Play(Action onComplete)
    {
        // 애니메이터 / 콜라이더 비활성화
        if (_anim) _anim.enabled = false;
        if (_col)  _col.enabled  = false;

        // 1. 물리 날아가기
        ApplyLaunchForce();

        // 2. DOTween 축소 (기존 연출 유지)
        float shrinkDelay    = totalDuration * (1f - shrinkStartRatio);
        float shrinkDuration = totalDuration * shrinkStartRatio;

        transform.DOKill();
        transform.DOScale(VectorZero, shrinkDuration)
            .SetDelay(shrinkDelay)
            .SetEase(Ease.InBack)
            .OnComplete(() => onComplete?.Invoke());

        // 3. 글리치 코루틴 동시 시작
        if (_glitchCoroutine != null)
            StopCoroutine(_glitchCoroutine);
        _glitchCoroutine = StartCoroutine(GlitchSequence());

        // 4. 보조 파티클 스폰 (선택)
        SpawnSparks();
    }

    /// <summary>
    /// 풀 반환 시 상태 완전 초기화.
    /// Enemy.ResetForPool()에서 호출 필요.
    /// </summary>
    public void ResetState()
    {
        // 글리치 코루틴 중단
        if (_glitchCoroutine != null)
        {
            StopCoroutine(_glitchCoroutine);
            _glitchCoroutine = null;
        }

        // Tween 정리 및 스케일 복원
        transform.DOKill();
        transform.localScale = VectorOne;

        // 글리치 머티리얼 인스턴스 해제 + 원본 복원
        RestoreOriginalMaterials();

        // Rigidbody / Collider / Animator 복원
        if (_rb)
        {
            _rb.isKinematic    = false;
            _rb.velocity       = VectorZero;
            _rb.angularVelocity = VectorZero;
            _rb.constraints    = RigidbodyConstraints.None;
            _rb.isKinematic    = true;
        }

        if (_col)  _col.enabled  = true;
        if (_anim) _anim.enabled = true;

        _attacker = null;
    }

    // ────────────────────────────────────────────────────────────────────────
    // Private — 물리
    // ────────────────────────────────────────────────────────────────────────

    private void ApplyLaunchForce()
    {
        if (_rb == null) return;

        _rb.isKinematic = false;
        _rb.constraints = RigidbodyConstraints.FreezeRotation;

        Vector3 direction = VectorZero;
        if (_attacker != null)
        {
            direction = transform.position - _attacker.position;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.01f)
                direction.Normalize();
            else
                direction = Vector3.forward;
        }

        Vector3 force = new Vector3(
            direction.x * flyPower,
            flyPower * upwardRatio,
            direction.z * flyPower
        );

        _rb.velocity       = VectorZero;
        _rb.angularVelocity = VectorZero;
        _rb.AddForce(force, ForceMode.Impulse);
    }

    // ────────────────────────────────────────────────────────────────────────
    // Private — 글리치 코루틴
    // ────────────────────────────────────────────────────────────────────────

    private IEnumerator GlitchSequence()
    {
        // 글리치 머티리얼이 없으면 스킵
        if (glitchDeathMaterial == null || _renderers == null || _renderers.Length == 0)
            yield break;

        // 시작 지연
        if (glitchStartDelay > 0f)
            yield return new WaitForSeconds(glitchStartDelay);

        // 각 렌더러에 글리치 머티리얼 인스턴스를 적용
        ApplyGlitchMaterials();

        float dissolveStartTime = totalDuration * dissolveStartRatio;
        float dissolveDuration  = totalDuration - dissolveStartTime - glitchStartDelay;
        dissolveDuration        = Mathf.Max(0.1f, dissolveDuration);

        // ── Phase 1: 글리치 Ramp-up ──────────────────────────────────────
        // _GlitchIntensity / _ChromaShift 를 rampDuration 동안 0→max로 증가
        float elapsed = 0f;
        while (elapsed < glitchRampDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / glitchRampDuration);

            // Ease Out Cubic
            float eased = 1f - Mathf.Pow(1f - t, 3f);

            float intensity = Mathf.Lerp(0f, glitchIntensityMax, eased);
            float chroma    = Mathf.Lerp(0f, chromaShiftMax,      eased);

            SetGlitchParams(intensity, chroma, 0f);
            yield return null;
        }

        // ── Phase 2: 글리치 유지 + 디졸브 시작 ──────────────────────────
        float dissolveElapsed = 0f;
        while (dissolveElapsed < dissolveDuration)
        {
            dissolveElapsed += Time.deltaTime;
            float t       = Mathf.Clamp01(dissolveElapsed / dissolveDuration);

            // 디졸브: Ease In Quad (천천히 시작해서 빠르게 소멸)
            float dissolve = t * t;

            // 글리치 강도는 최대로 유지하되, 마지막 30%에서 약해짐
            float fadeOff  = Mathf.InverseLerp(0.7f, 1f, t);
            float intensity = Mathf.Lerp(glitchIntensityMax, 0f, fadeOff);
            float chroma    = Mathf.Lerp(chromaShiftMax,      0f, fadeOff);

            SetGlitchParams(intensity, chroma, dissolve);
            yield return null;
        }

        // 완전 소멸
        SetGlitchParams(0f, 0f, 1f);
        _glitchCoroutine = null;
    }

    // ────────────────────────────────────────────────────────────────────────
    // Private — 머티리얼 관리
    // ────────────────────────────────────────────────────────────────────────

    private void CacheOriginalMaterials()
    {
        if (_renderers == null) return;

        // 렌더러 수만큼 배열 할당
        int total = 0;
        foreach (var rend in _renderers)
            if (rend != null) total += rend.sharedMaterials.Length;

        _originalMaterials = new Material[_renderers.Length];
        for (int i = 0; i < _renderers.Length; i++)
        {
            if (_renderers[i] != null && _renderers[i].sharedMaterials.Length > 0)
                _originalMaterials[i] = _renderers[i].sharedMaterial;
        }
    }

    private void ApplyGlitchMaterials()
    {
        if (_renderers == null || glitchDeathMaterial == null) return;

        _glitchMaterialInstances = new Material[_renderers.Length];

        for (int i = 0; i < _renderers.Length; i++)
        {
            if (_renderers[i] == null) continue;

            // 원본 텍스처를 글리치 머티리얼 인스턴스에 이식
            Material instance = new Material(glitchDeathMaterial);

            // 원본 머티리얼의 _BaseMap 텍스처가 있으면 이식
            if (_originalMaterials != null && i < _originalMaterials.Length && _originalMaterials[i] != null)
            {
                Material orig = _originalMaterials[i];
                if (orig.HasProperty("_BaseMap"))
                    instance.SetTexture("_BaseMap", orig.GetTexture("_BaseMap"));
                else if (orig.HasProperty("_MainTex"))
                    instance.SetTexture("_BaseMap", orig.GetTexture("_MainTex"));

                // 원본 Base Color 이식
                if (orig.HasProperty("_BaseColor"))
                    instance.SetColor("_BaseColor", orig.GetColor("_BaseColor"));
            }

            // 글리치 색상 설정
            instance.SetColor(GlitchColorId, glitchEmissionColor);
            instance.SetFloat(GlitchIntensityId, 0f);
            instance.SetFloat(ChromaShiftId, 0f);
            instance.SetFloat(DissolveId, 0f);

            _glitchMaterialInstances[i]   = instance;
            _renderers[i].sharedMaterial  = instance;
        }
    }

    private void SetGlitchParams(float intensity, float chroma, float dissolve)
    {
        if (_glitchMaterialInstances == null) return;

        foreach (var mat in _glitchMaterialInstances)
        {
            if (mat == null) continue;
            mat.SetFloat(GlitchIntensityId, intensity);
            mat.SetFloat(ChromaShiftId,     chroma);
            mat.SetFloat(DissolveId,        dissolve);
        }
    }

    private void RestoreOriginalMaterials()
    {
        // 글리치 인스턴스 파괴
        if (_glitchMaterialInstances != null)
        {
            foreach (var mat in _glitchMaterialInstances)
            {
                if (mat != null)
                    Destroy(mat);
            }
            _glitchMaterialInstances = null;
        }

        // 원본 머티리얼 복원
        if (_renderers == null || _originalMaterials == null) return;

        for (int i = 0; i < _renderers.Length && i < _originalMaterials.Length; i++)
        {
            if (_renderers[i] != null && _originalMaterials[i] != null)
                _renderers[i].sharedMaterial = _originalMaterials[i];
        }
    }

    // ────────────────────────────────────────────────────────────────────────
    // Private — 보조 파티클
    // ────────────────────────────────────────────────────────────────────────

    private void SpawnSparks()
    {
        if (deathSparksPrefab == null) return;

        Vector3 spawnPos = transform.position + Vector3.up * 0.5f;
        GameObject sparks = Instantiate(deathSparksPrefab, spawnPos, Quaternion.identity);
        if (sparks == null) return;

        // 파티클 자동 정지 + 지연 파괴
        foreach (var ps in sparks.GetComponentsInChildren<ParticleSystem>(true))
            ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        Destroy(sparks, Mathf.Max(0.1f, sparksLifetime));
    }
}
