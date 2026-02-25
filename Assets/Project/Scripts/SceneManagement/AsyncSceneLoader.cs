using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 공통 비동기 씬 로더 + 페이드 처리.
/// DontDestroyOnLoad 로 유지됩니다.
/// </summary>
public class AsyncSceneLoader : MonoBehaviour
{
    private static AsyncSceneLoader _instance;
    public static AsyncSceneLoader Instance
    {
        get
        {
            if (_instance == null)
            {
                CreateSingleton();
            }

            return _instance;
        }
    }

    [Header("Fade")]
    [SerializeField] private Color fadeColor = Color.black;
    [SerializeField] private float fadeDuration = 0.3f;

    private Canvas _canvas;
    private Image _fadeImage;
    private bool _isLoading;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoCreate()
    {
        if (_instance == null)
        {
            CreateSingleton();
        }
    }

    private static void CreateSingleton()
    {
        var existing = FindObjectOfType<AsyncSceneLoader>();
        if (existing != null)
        {
            _instance = existing;
            return;
        }

        var go = new GameObject("AsyncSceneLoader");
        _instance = go.AddComponent<AsyncSceneLoader>();
        DontDestroyOnLoad(go);
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureFadeCanvas();
    }

    private void EnsureFadeCanvas()
    {
        if (_canvas != null && _fadeImage != null) return;

        var canvasGO = new GameObject("SceneLoaderCanvas");
        canvasGO.transform.SetParent(transform);

        _canvas = canvasGO.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = short.MaxValue; // 항상 맨 위

        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        var imageGO = new GameObject("Fade");
        imageGO.transform.SetParent(canvasGO.transform);

        _fadeImage = imageGO.AddComponent<Image>();
        _fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);

        var rect = _fadeImage.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        _canvas.enabled = false;
    }

    public void LoadSceneAsync(string sceneName)
    {
        LoadSceneAsync(sceneName, LoadSceneMode.Single, fadeColor, fadeDuration);
    }

    public void LoadSceneAsync(string sceneName, LoadSceneMode mode)
    {
        LoadSceneAsync(sceneName, mode, fadeColor, fadeDuration);
    }

    public void LoadSceneAsync(string sceneName, LoadSceneMode mode, Color color, float duration)
    {
        if (_isLoading || string.IsNullOrEmpty(sceneName))
        {
            return;
        }

        EnsureFadeCanvas();
        StartCoroutine(LoadSceneRoutine(sceneName, mode, color, Mathf.Max(0.01f, duration)));
    }

    private IEnumerator LoadSceneRoutine(string sceneName, LoadSceneMode mode, Color color, float duration)
    {
        _isLoading = true;
        _canvas.enabled = true;

        // Fade Out
        float half = duration * 0.5f;
        float t = 0f;
        while (t < half)
        {
            t += Time.unscaledDeltaTime;
            float alpha = Mathf.Clamp01(t / half);
            _fadeImage.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        _fadeImage.color = new Color(color.r, color.g, color.b, 1f);

        // Scene Load
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName, mode);
        loadOp.allowSceneActivation = true;

        while (!loadOp.isDone)
        {
            yield return null;
        }

        var loadedScene = SceneManager.GetSceneByName(sceneName);
        if (loadedScene.IsValid())
        {
            SceneManager.SetActiveScene(loadedScene);
        }

        // Fade In
        t = 0f;
        while (t < half)
        {
            t += Time.unscaledDeltaTime;
            float alpha = 1f - Mathf.Clamp01(t / half);
            _fadeImage.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        _fadeImage.color = new Color(color.r, color.g, color.b, 0f);
        _canvas.enabled = false;
        _isLoading = false;
    }
}

