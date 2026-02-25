using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// ResultPopup 을 전역에서 호출할 수 있는 서비스.
/// DontDestroyOnLoad 로 유지되며, UI Toolkit 오버레이를 사용합니다.
/// </summary>
public class ResultPopupService : MonoBehaviour
{
    private static ResultPopupService _instance;
    public static ResultPopupService Instance
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

    [Header("Settings")]
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private float autoCloseSeconds = 3f;

    /// <summary>
    /// 프레스티지 버튼이 눌렸을 때 호출되는 이벤트. 외부에서 구독해 사용.
    /// </summary>
    public event Action PrestigeClicked;

    private UIDocument _uiDocument;
    private PanelSettings _panelSettings;
    private ResultPopup _popup;
    private Coroutine _currentRoutine;

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
        if (_uiDocument != null && _popup != null) return;

        _uiDocument = GetComponent<UIDocument>();
        if (_uiDocument == null)
        {
            _uiDocument = gameObject.AddComponent<UIDocument>();
        }

        if (_panelSettings == null)
        {
            _panelSettings = ScriptableObject.CreateInstance<PanelSettings>();

            // UIThemeStyleSheet 적용이 안되어 있으면 기본 Theme Style Sheet를 시도해서 경고를 방지
#if UNITY_EDITOR
            // 에디터에서 경고를 막기 위해 패키지 경로에서 DefaultTheme를 시도하여 로드
            var defaultTheme = UnityEditor.AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(
                "Packages/com.unity.ui/Runtime/Themes/DefaultCommonLight.uss"
            );
            if (defaultTheme != null)
            {
                _panelSettings.themeStyleSheet = defaultTheme;
            }
            else
            {
                Debug.LogWarning(
                    "[ResultPopupService] PanelSettings에 할당할 ThemeStyleSheet(기본 Light 테마)를 찾을 수 없습니다.\n" +
                    "UnityEngine.Debug:LogWarning (object)\n" +
                    "ResultPopupService:EnsureUIDocument () (at Assets/Project/Scripts/UI/ResultPopupService.cs:103)\n" +
                    "ResultPopupService:Awake () (at Assets/Project/Scripts/UI/ResultPopupService.cs:74)\n" +
                    "UnityEngine.GameObject:AddComponent<ResultPopupService> ()\n" +
                    "ResultPopupService:CreateSingleton () (at Assets/Project/Scripts/UI/ResultPopupService.cs:59)\n" +
                    "ResultPopupService:AutoCreate () (at Assets/Project/Scripts/UI/ResultPopupService.cs:45)"
                );
            }
#else
            // 런타임 - 유저가 직접 테마를 Resources나 Addressables등으로 지정하도록 가이드
            // 이 자리에 ThemeStyleSheet가 없으면 경고
            if (_panelSettings.themeStyleSheet == null)
            {
                Debug.LogWarning("[ResultPopupService] PanelSettings에 Theme Style Sheet가 지정되지 않아 UI가 예상대로 렌더링되지 않을 수 있습니다. Resources 폴더에서 커스텀 ThemeStyleSheet를 로드해 적용하세요.");
            }
#endif
        }
        _uiDocument.panelSettings = _panelSettings;

        var treeAsset = Resources.Load<VisualTreeAsset>("UI/Result/ResultPopup");
        if (treeAsset == null)
        {
            Debug.LogError("[ResultPopupService] Resources/UI/Result/ResultPopup.uxml 을 찾을 수 없습니다.");
            return;
        }

        _uiDocument.visualTreeAsset = treeAsset;

        var root = _uiDocument.rootVisualElement?.Q<VisualElement>("result-popup-root");
        if (root == null)
        {
            Debug.LogError("[ResultPopupService] result-popup-root 요소를 찾을 수 없습니다.");
            return;
        }

        _popup = new ResultPopup(root);
        _popup.SetVisible(false);
        _popup.SetOpacity(0f);

        _popup.BindButtons(
            OnContinueClicked,
            OnPrestigeClicked,
            OnRetryClicked,
            OnTitleClicked,
            OnCloseClicked
        );
    }

    #region Public API

    public static void ShowWin(int wave, int gold, int prestige, bool autoClose = false)
    {
        Instance.InternalShow(ResultPopup.Mode.Win, wave, gold, prestige, autoClose);
    }

    public static void ShowLose(int wave, int gold, bool autoClose = false)
    {
        Instance.InternalShow(ResultPopup.Mode.Lose, wave, gold, 0, autoClose);
    }

    #endregion

    private void InternalShow(ResultPopup.Mode mode, int wave, int gold, int prestige, bool autoClose)
    {
        EnsureUIDocument();
        if (_popup == null)
        {
            return;
        }

        if (_currentRoutine != null)
        {
            StopCoroutine(_currentRoutine);
        }

        if (mode == ResultPopup.Mode.Win)
        {
            _popup.ConfigureWin(wave, gold, prestige);
        }
        else
        {
            _popup.ConfigureLose(wave, gold);
        }

        _currentRoutine = StartCoroutine(ShowRoutine(autoClose));
    }

    private IEnumerator ShowRoutine(bool autoClose)
    {
        _popup.SetVisible(true);

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float alpha = Mathf.Clamp01(t / fadeDuration);
            _popup.SetOpacity(alpha);
            yield return null;
        }

        _popup.SetOpacity(1f);

        if (autoClose)
        {
            yield return new WaitForSecondsRealtime(autoCloseSeconds);
            yield return StartCoroutine(HideRoutine());
        }

        _currentRoutine = null;
    }

    private IEnumerator HideRoutine()
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float alpha = 1f - Mathf.Clamp01(t / fadeDuration);
            _popup.SetOpacity(alpha);
            yield return null;
        }

        _popup.SetOpacity(0f);
        _popup.SetVisible(false);
        _currentRoutine = null;
    }

    #region Button Handlers

    private void OnContinueClicked()
    {
        if (_currentRoutine != null)
        {
            StopCoroutine(_currentRoutine);
        }

        _currentRoutine = StartCoroutine(HideRoutine());
    }

    private void OnPrestigeClicked()
    {
        if (_currentRoutine != null)
        {
            StopCoroutine(_currentRoutine);
        }

        _currentRoutine = StartCoroutine(HideRoutine());
        PrestigeClicked?.Invoke();
    }

    private void OnRetryClicked()
    {
        if (_currentRoutine != null)
        {
            StopCoroutine(_currentRoutine);
        }

        _currentRoutine = StartCoroutine(HideRoutine());

        var manager = BattleGameManager.Instance;
        if (manager != null)
        {
            manager.Restart();
        }
    }

    private void OnTitleClicked()
    {
        if (_currentRoutine != null)
        {
            StopCoroutine(_currentRoutine);
        }

        _currentRoutine = StartCoroutine(HideRoutine());

        var manager = BattleGameManager.Instance;
        if (manager != null)
        {
            manager.BackToTitle();
        }
    }

    private void OnCloseClicked()
    {
        if (_currentRoutine != null)
        {
            StopCoroutine(_currentRoutine);
        }

        _currentRoutine = StartCoroutine(HideRoutine());
    }

    #endregion
}

