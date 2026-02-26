using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// 전역 호출 가능한 ResultPopup 서비스.
/// DontDestroyOnLoad로 영속되고 UI Toolkit 오버레이를 사용합니다.
/// </summary>
public class ResultPopupService : MonoBehaviour
{
    private static ResultPopupService _instance;

    public static ResultPopupService Instance
    {
        get
        {
            if (_instance == null)
                CreateSingleton();

            return _instance;
        }
    }

    [Header("Settings")]
    [SerializeField] private float fadeDuration = 0.35f;
    [SerializeField] private float autoCloseSeconds = 3f;

    /// <summary>
    /// 프레스티지 버튼 클릭 이벤트(외부 구독용)
    /// </summary>
    public event Action PrestigeClicked;

    private UIDocument _uiDocument;
    private PanelSettings _panelSettings;
    private ResultPopup _popup;
    private Coroutine _currentRoutine;
    private float _currentOpacity;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoCreate()
    {
        if (_instance == null)
            CreateSingleton();
    }

    private static void CreateSingleton()
    {
        var existing = FindObjectOfType<ResultPopupService>();
        if (existing != null)
        {
            _instance = existing;
            return;
        }

        var go = new GameObject("ResultPopupService");
        _instance = go.AddComponent<ResultPopupService>();
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
        EnsureUIDocument();
    }

    private void EnsureUIDocument()
    {
        if (_uiDocument != null && _popup != null)
            return;

        VisualElement root = null;

        if (_uiDocument == null)
        {
            UIDocument[] allDocs;
#if UNITY_2022_2_OR_NEWER
            allDocs = FindObjectsByType<UIDocument>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
            allDocs = FindObjectsOfType<UIDocument>(true);
#endif
            foreach (var doc in allDocs)
            {
                if (doc == null)
                    continue;

                var candidate = doc.rootVisualElement?.Q<VisualElement>("result-popup-root");
                if (candidate == null)
                    continue;

                _uiDocument = doc;
                root = candidate;
                break;
            }
        }

        if (_uiDocument == null)
        {
            _uiDocument = GetComponent<UIDocument>() ?? gameObject.AddComponent<UIDocument>();
            if (_panelSettings == null)
            {
                _panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
#if UNITY_EDITOR
                TryAssignEditorDefaultTheme();
#endif
            }

            _uiDocument.panelSettings = _panelSettings;

            VisualTreeAsset treeAsset = Resources.Load<VisualTreeAsset>("UI/Result/ResultPopup");
#if UNITY_EDITOR
            if (treeAsset == null)
            {
                string[] guids = UnityEditor.AssetDatabase.FindAssets("ResultPopup t:VisualTreeAsset");
                if (guids.Length > 0)
                {
                    string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                    treeAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);
                }
            }
#endif

            if (treeAsset == null)
            {
                Debug.LogError("[ResultPopupService] ResultPopup.uxml을 찾을 수 없습니다. Resources/UI/Result 경로를 확인하세요.");
                return;
            }

            _uiDocument.visualTreeAsset = treeAsset;
        }

        if (root == null)
            root = _uiDocument.rootVisualElement?.Q<VisualElement>("result-popup-root");

        if (root == null)
        {
            Debug.LogError("[ResultPopupService] result-popup-root 요소를 찾을 수 없습니다.");
            return;
        }

        _popup = new ResultPopup(root);
        _popup.SetVisible(false);
        SetPopupOpacity(0f);

        _popup.BindButtons(
            OnContinueClicked,
            OnPrestigeClicked,
            OnRetryClicked,
            OnTitleClicked,
            OnCloseClicked);
    }

#if UNITY_EDITOR
    private void TryAssignEditorDefaultTheme()
    {
        if (_panelSettings.themeStyleSheet != null)
            return;

        string[] candidatePaths =
        {
            "Assets/UI Toolkit/UnityThemes/UnityDefaultRuntimeTheme.tss",
            "Packages/com.unity.ui/PackageResources/StyleSheets/Generated/Default.tss",
            "Packages/com.unity.ui/Runtime/Themes/DefaultCommonLight.uss"
        };

        foreach (var path in candidatePaths)
        {
            var theme = UnityEditor.AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(path);
            if (theme == null)
                continue;

            _panelSettings.themeStyleSheet = theme;
            return;
        }
    }
#endif

    public static void ShowWin(int wave, int gold, int prestige, bool autoClose = false)
    {
        Instance.InternalShow(ResultPopup.Mode.Win, wave, gold, prestige, autoClose);
    }

    public static void ShowLose(int wave, int gold, bool autoClose = false)
    {
        Instance.InternalShow(ResultPopup.Mode.Lose, wave, gold, 0, autoClose);
    }

    private void InternalShow(ResultPopup.Mode mode, int wave, int gold, int prestige, bool autoClose)
    {
        EnsureUIDocument();
        if (_popup == null)
            return;

        if (mode == ResultPopup.Mode.Win)
            _popup.ConfigureWin(wave, gold, prestige);
        else
            _popup.ConfigureLose(wave, gold);

        StartTransition(ShowRoutine(autoClose));
    }

    private void StartTransition(IEnumerator routine)
    {
        if (_currentRoutine != null)
            StopCoroutine(_currentRoutine);

        _currentRoutine = StartCoroutine(routine);
    }

    private IEnumerator ShowRoutine(bool autoClose)
    {
        _popup.SetVisible(true);
        yield return FadeTo(1f);

        if (autoClose)
        {
            yield return new WaitForSecondsRealtime(autoCloseSeconds);
            yield return HideRoutine();
        }

        _currentRoutine = null;
    }

    private IEnumerator HideRoutine()
    {
        yield return FadeTo(0f);
        _popup.SetVisible(false);
        _currentRoutine = null;
    }

    private IEnumerator FadeTo(float targetOpacity)
    {
        float startOpacity = _currentOpacity;
        float duration = Mathf.Max(0.01f, fadeDuration);

        if (Mathf.Approximately(startOpacity, targetOpacity))
        {
            SetPopupOpacity(targetOpacity);
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = Mathf.SmoothStep(0f, 1f, t);
            SetPopupOpacity(Mathf.Lerp(startOpacity, targetOpacity, eased));
            yield return null;
        }

        SetPopupOpacity(targetOpacity);
    }

    private void SetPopupOpacity(float value)
    {
        _currentOpacity = Mathf.Clamp01(value);
        _popup?.SetOpacity(_currentOpacity);
    }

    private void OnContinueClicked()
    {
        var waveManager = WaveManager.Instance != null ? WaveManager.Instance : FindFirstObjectByType<WaveManager>();
        if (waveManager != null)
            waveManager.StartNextWave();
        else
            Debug.LogWarning("[ResultPopupService] WaveManager를 찾지 못해 다음 웨이브를 시작하지 못했습니다.");

        StartTransition(HideRoutine());
    }

    private void OnPrestigeClicked()
    {
        Debug.Log("[Prestige] 프레스티지 버튼 클릭!");
        PrestigeClicked?.Invoke();
        StartTransition(HideRoutine());
    }

    private void OnRetryClicked()
    {
        StartTransition(HideRoutine());

        var manager = BattleGameManager.Instance;
        if (manager != null)
            manager.Restart();
    }

    private void OnTitleClicked()
    {
        StartTransition(HideRoutine());

        var manager = BattleGameManager.Instance;
        if (manager != null)
            manager.BackToTitle();
    }

    private void OnCloseClicked()
    {
        StartTransition(HideRoutine());
    }
}
