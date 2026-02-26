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
    // 싱글톤 인스턴스 관리
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
    /// 프레스티지 버튼 클릭 이벤트(외부 구독용)
    /// </summary>
    public event Action PrestigeClicked;

    private UIDocument _uiDocument;
    private PanelSettings _panelSettings;
    private ResultPopup _popup;
    private Coroutine _currentRoutine;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoCreate()
    {
        if (_instance == null) CreateSingleton();
    }

    // 싱글톤 생성기
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
        // 중복 인스턴스 방지
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
        EnsureUIDocument();
    }

    /// <summary>
    /// UIDocument 및 UXML, PanelSettings 초기화. 다양한 상황을 자동 커버하도록 설계됨.
    /// </summary>
    private void EnsureUIDocument()
    {
        // 이미 초기화된 경우 무시
        if (_uiDocument != null && _popup != null) return;

        // ── 1순위: 씬 내 UIDocument에서 result-popup-root 찾아 재사용 ───
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
                if (doc == null) continue;
                var candidate = doc.rootVisualElement?.Q<VisualElement>("result-popup-root");
                if (candidate != null)
                {
                    _uiDocument = doc;
                    root = candidate;
                    break;
                }
            }
        }

        // ── 2순위: 직접 UIDocument 생성 및 UXML 동적 로드 ──────────────
        if (_uiDocument == null)
        {
            _uiDocument = GetComponent<UIDocument>() ?? gameObject.AddComponent<UIDocument>();
            // PanelSettings 생성 및 할당
            if (_panelSettings == null)
            {
                _panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
#if UNITY_EDITOR
                TryAssignEditorDefaultTheme();
#endif
            }
            _uiDocument.panelSettings = _panelSettings;

            // UXML 가져오기 - Resources 우선, 없으면 에디터 AssetDatabase 시도
            VisualTreeAsset treeAsset = Resources.Load<VisualTreeAsset>("UI/Result/ResultPopup");

#if UNITY_EDITOR
            if (treeAsset == null)
            {
                string[] guids = UnityEditor.AssetDatabase.FindAssets("ResultPopup t:VisualTreeAsset");
                if (guids.Length > 0)
                {
                    string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                    treeAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);
                    if (treeAsset != null)
                        Debug.Log($"[ResultPopupService] AssetDatabase에서 UXML 로드 성공: {path}");
                }
            }
#endif

            // UXML 실패시: 명확하고 직관적인 에러 메시지 제공
            if (treeAsset == null)
            {
                Debug.LogError(
                    "[ResultPopupService] ResultPopup.uxml을 찾을 수 없습니다.\n" +
                    "해결 방법:\n" +
                    "1) Assets/Project/UI/Result/ResultPopup.uxml 파일을\n" +
                    "   Assets/Resources/UI/Result/ResultPopup.uxml로 복사하세요.\n" +
                    "2) 혹은 씬의 ResultUI 오브젝트에 UIDocument + ResultPanelManager가 연결되어 있는지 확인하세요."
                );
                return;
            }
            _uiDocument.visualTreeAsset = treeAsset;
        }

        // ── root 요소 최종 획득 ─────────────────────────────────────────────────
        if (root == null)
            root = _uiDocument.rootVisualElement?.Q<VisualElement>("result-popup-root");

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

#if UNITY_EDITOR
    // 에디터 디폴트 테마 자동 연결
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
            if (theme != null)
            {
                _panelSettings.themeStyleSheet = theme;
                return;
            }
        }
    }
#endif

    #region Public API

    /// <summary>
    /// 승리 팝업 표시
    /// </summary>
    public static void ShowWin(int wave, int gold, int prestige, bool autoClose = false)
    {
        Instance.InternalShow(ResultPopup.Mode.Win, wave, gold, prestige, autoClose);
    }

    /// <summary>
    /// 패배 팝업 표시
    /// </summary>
    public static void ShowLose(int wave, int gold, bool autoClose = false)
    {
        Instance.InternalShow(ResultPopup.Mode.Lose, wave, gold, 0, autoClose);
    }

    #endregion

    /// <summary>
    /// 내부 팝업 표시/설정 루틴
    /// </summary>
    private void InternalShow(ResultPopup.Mode mode, int wave, int gold, int prestige, bool autoClose)
    {
        EnsureUIDocument();
        if (_popup == null)
            return;

        if (_currentRoutine != null)
            StopCoroutine(_currentRoutine);

        if (mode == ResultPopup.Mode.Win)
            _popup.ConfigureWin(wave, gold, prestige);
        else
            _popup.ConfigureLose(wave, gold);

        _currentRoutine = StartCoroutine(ShowRoutine(autoClose));
    }

    /// <summary>
    /// 팝업 표시(페이드인, 자동닫힘 지원)
    /// </summary>
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

    /// <summary>
    /// 팝업 닫힘(페이드아웃)
    /// </summary>
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

    /// <summary>
    /// 계속 버튼 핸들러
    /// </summary>
    private void OnContinueClicked()
    {
        if (_currentRoutine != null)
            StopCoroutine(_currentRoutine);

        _currentRoutine = StartCoroutine(HideRoutine());
    }

    /// <summary>
    /// 프레스티지 버튼 핸들러
    /// </summary>
    private void OnPrestigeClicked()
    {
        if (_currentRoutine != null)
            StopCoroutine(_currentRoutine);

        _currentRoutine = StartCoroutine(HideRoutine());
        PrestigeClicked?.Invoke();
    }

    /// <summary>
    /// 재시도 버튼 핸들러
    /// </summary>
    private void OnRetryClicked()
    {
        if (_currentRoutine != null)
            StopCoroutine(_currentRoutine);

        _currentRoutine = StartCoroutine(HideRoutine());

        var manager = BattleGameManager.Instance;
        if (manager != null)
            manager.Restart();
    }

    /// <summary>
    /// 타이틀 버튼 핸들러
    /// </summary>
    private void OnTitleClicked()
    {
        if (_currentRoutine != null)
            StopCoroutine(_currentRoutine);

        _currentRoutine = StartCoroutine(HideRoutine());

        var manager = BattleGameManager.Instance;
        if (manager != null)
            manager.BackToTitle();
    }

    /// <summary>
    /// 닫기 버튼 핸들러
    /// </summary>
    private void OnCloseClicked()
    {
        if (_currentRoutine != null)
            StopCoroutine(_currentRoutine);

        _currentRoutine = StartCoroutine(HideRoutine());
    }

    #endregion
}

/// <summary>
/// (설명)
/// 주요 변경 및 오류 수정 사항: 
/// 1. 문자열 상수 내 줄바꿈(\n) 적용으로 컴파일러 오류 수정. 
/// 2. 로직 전반의 if문, 호출 순서, 지역변수 사용을 가독성과 견고성을 위해 개선. 
/// 3. 각 핸들러 및 주요 메서드에 상세 주석 추가. 
/// 4. 필요 없는 불필요한 중첩 또는 반복 제거, 책임 분리 강조.
/// 5. 서비스 사용자는 Instance 및 ShowWin/ShowLose만 호출하면 내부 관리가 자동 수행됨.
/// </summary>