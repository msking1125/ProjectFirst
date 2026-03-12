using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace Project
{
public class EnemyDeathEffect : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("인스펙터에서 설정합니다.")]
    [SerializeField] private float flyPower = 12f;

    [Header("Settings")]
    [Tooltip("인스펙터에서 설정합니다.")]
    [SerializeField] private float upwardRatio = 0.3f;

    [Header("Settings")]
    [Tooltip("인스펙터에서 설정합니다.")]
    [SerializeField] private float totalDuration = 1.5f;

    [Tooltip("인스펙터에서 설정합니다.")]
    [Range(0.1f, 0.9f)] [SerializeField] private float shrinkStartRatio = 0.4f;
    [Header("Glitch Death")]
    [Tooltip("인스펙터에서 설정합니다.")]
    [SerializeField] private Material glitchDeathMaterial;

    [Tooltip("인스펙터에서 설정합니다.")]
    [SerializeField] private float glitchIntensityMax = 0.8f;

    [Tooltip("인스펙터에서 설정합니다.")]
    [SerializeField] private float chromaShiftMax = 0.04f;

    [Tooltip("인스펙터에서 설정합니다.")]
    [SerializeField] private Color glitchEmissionColor = new Color(0f, 1f, 0.8f, 1f);

    [Tooltip("인스펙터에서 설정합니다.")]
    [SerializeField, Min(0f)] private float glitchStartDelay = 0.1f;

    [Tooltip("인스펙터에서 설정합니다.")]
    [SerializeField, Min(0.05f)] private float glitchRampDuration = 0.35f;

    [Tooltip("인스펙터에서 설정합니다.")]
    [Range(0.2f, 0.8f)] [SerializeField] private float dissolveStartRatio = 0.45f;
    [Header("Settings")]
    [Tooltip("인스펙터에서 설정합니다.")]
    [SerializeField] private GameObject deathSparksPrefab;

    [Tooltip("인스펙터에서 설정합니다.")]
    [SerializeField] private float sparksLifetime = 1.5f;
    private Rigidbody     _rb;
    private Animator      _anim;
    private Collider      _col;
    private Renderer[]    _renderers;
    private Material[]    _originalMaterials;   // Original materials are cached so they can be restored later.
    private Transform     _attacker;
    private Coroutine     _glitchCoroutine;
    private static readonly int GlitchIntensityId  = Shader.PropertyToID("_GlitchIntensity");
    private static readonly int ChromaShiftId      = Shader.PropertyToID("_ChromaShift");
    private static readonly int DissolveId         = Shader.PropertyToID("_Dissolve");
    private static readonly int GlitchColorId      = Shader.PropertyToID("_GlitchColor");

    private static readonly Vector3 VectorZero = Vector3.zero;
    private static readonly Vector3 VectorOne  = Vector3.one;
    private Material[] _glitchMaterialInstances;

    private void Awake()
    {
        _rb         = GetComponent<Rigidbody>();
        _anim       = GetComponentInChildren<Animator>();
        _col        = GetComponent<Collider>();
        _renderers  = GetComponentsInChildren<Renderer>(true);
        CacheOriginalMaterials();
    }
    // Public API
    public void SetTarget(Transform attacker) => _attacker = attacker;
    public void Play(Action onComplete)
    {
        if (_anim) _anim.enabled = false;
        if (_col)  _col.enabled  = false;
        ApplyLaunchForce();
        float shrinkDelay    = totalDuration * (1f - shrinkStartRatio);
        float shrinkDuration = totalDuration * shrinkStartRatio;

        transform.DOKill();
        transform.DOScale(VectorZero, shrinkDuration)
            .SetDelay(shrinkDelay)
            .SetEase(Ease.InBack)
            .OnComplete(() => onComplete?.Invoke());
        if (_glitchCoroutine != null)
            StopCoroutine(_glitchCoroutine);
        _glitchCoroutine = StartCoroutine(GlitchSequence());
        SpawnSparks();
    }
    public void ResetState()
    {
        if (_glitchCoroutine != null)
        {
            StopCoroutine(_glitchCoroutine);
            _glitchCoroutine = null;
        }
        transform.DOKill();
        transform.localScale = VectorOne;
        RestoreOriginalMaterials();
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

    private IEnumerator GlitchSequence()
    {
        if (glitchDeathMaterial == null || _renderers == null || _renderers.Length == 0)
            yield break;
        if (glitchStartDelay > 0f)
            yield return new WaitForSeconds(glitchStartDelay);
        ApplyGlitchMaterials();

        float dissolveStartTime = totalDuration * dissolveStartRatio;
        float dissolveDuration  = totalDuration - dissolveStartTime - glitchStartDelay;
        dissolveDuration        = Mathf.Max(0.1f, dissolveDuration);
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
        float dissolveElapsed = 0f;
        while (dissolveElapsed < dissolveDuration)
        {
            dissolveElapsed += Time.deltaTime;
            float t       = Mathf.Clamp01(dissolveElapsed / dissolveDuration);
            float dissolve = t * t;
            float fadeOff  = Mathf.InverseLerp(0.7f, 1f, t);
            float intensity = Mathf.Lerp(glitchIntensityMax, 0f, fadeOff);
            float chroma    = Mathf.Lerp(chromaShiftMax,      0f, fadeOff);

            SetGlitchParams(intensity, chroma, dissolve);
            yield return null;
        }
        SetGlitchParams(0f, 0f, 1f);
        _glitchCoroutine = null;
    }

    private void CacheOriginalMaterials()
    {
        if (_renderers == null) return;
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
            Material instance = new Material(glitchDeathMaterial);
            if (_originalMaterials != null && i < _originalMaterials.Length && _originalMaterials[i] != null)
            {
                Material orig = _originalMaterials[i];
                if (orig.HasProperty("_BaseMap"))
                    instance.SetTexture("_BaseMap", orig.GetTexture("_BaseMap"));
                else if (orig.HasProperty("_MainTex"))
                    instance.SetTexture("_BaseMap", orig.GetTexture("_MainTex"));
                if (orig.HasProperty("_BaseColor"))
                    instance.SetColor("_BaseColor", orig.GetColor("_BaseColor"));
            }
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
        if (_glitchMaterialInstances != null)
        {
            foreach (var mat in _glitchMaterialInstances)
            {
                if (mat != null)
                    Destroy(mat);
            }
            _glitchMaterialInstances = null;
        }
        if (_renderers == null || _originalMaterials == null) return;

        for (int i = 0; i < _renderers.Length && i < _originalMaterials.Length; i++)
        {
            if (_renderers[i] != null && _originalMaterials[i] != null)
                _renderers[i].sharedMaterial = _originalMaterials[i];
        }
    }

    private void SpawnSparks()
    {
        if (deathSparksPrefab == null) return;

        Vector3 spawnPos = transform.position + Vector3.up * 0.5f;
        GameObject sparks = Instantiate(deathSparksPrefab, spawnPos, Quaternion.identity);
        if (sparks == null) return;
        foreach (var ps in sparks.GetComponentsInChildren<ParticleSystem>(true))
            ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        Destroy(sparks, Mathf.Max(0.1f, sparksLifetime));
    }
}
} // namespace Project

