using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using ProjectFirst.OutGame;

/// <summary>
/// 타이틀 씬 메인 관리자
/// UI 생성, 설정 관리, 씬 로딩 등의 책임을 분리된 클래스들에게 위임
/// </summary>
public class TitleManager : MonoBehaviour
{
    public static TitleManager Instance { get; private set; }

    [Header("Scene Settings")]
    [SerializeField] private string gameSceneName = "Battle_Test";

    [Header("Managers")]
    [SerializeField] private TitleUIManager uiManager;
    [SerializeField] private TitleSettingsManager settingsManager;

    [Header("Events (Optional)")]
    [SerializeField] private VoidEventChannelSO startButtonEvent;
    [SerializeField] private VoidEventChannelSO settingsButtonEvent;
    [SerializeField] private VoidEventChannelSO quitButtonEvent;

    private void Awake()
    {
        Instance = this;

        // 매니저들 초기화 (런타임 UI 생성용)
        if (uiManager == null)
        {
            uiManager = gameObject.AddComponent<TitleUIManager>();
        }
        uiManager.OnStartClicked = OnStartClicked;
        uiManager.OnServerSelectClicked = OnServerSelectClicked;
        uiManager.OnSettingsClicked = OnSettingsClicked;

        if (settingsManager == null)
        {
            settingsManager = gameObject.AddComponent<TitleSettingsManager>();
        }

        uiManager.Initialize();
        settingsManager.Initialize();

        // 타이틀 화면 로드 시 항상 버튼 표시
        ShowTitleButtons();

        // UXML 버튼 이벤트 연결 시도
        SetupUXMLButtonEvents();
    }

    private void SetupUXMLButtonEvents()
    {
        var uiDoc = GetComponent<UnityEngine.UIElements.UIDocument>();
        if (uiDoc == null) return;

        var root = uiDoc.rootVisualElement;
        if (root == null) return;

        // UXML 버튼 이벤트 연결
        var startButton = root.Q<UnityEngine.UIElements.Button>("start-button");
        if (startButton != null) startButton.clicked += OnStartClicked;

        var serverSelectButton = root.Q<UnityEngine.UIElements.Button>("server-select-button");
        if (serverSelectButton != null) serverSelectButton.clicked += OnServerSelectClicked;

        var settingsButton = root.Q<UnityEngine.UIElements.Button>("settings-button");
        if (settingsButton != null) settingsButton.clicked += OnSettingsClicked;

        var quitButton = root.Q<UnityEngine.UIElements.Button>("quit-button");
        if (quitButton != null) quitButton.clicked += QuitGame;
    }

    public void ShowTitleButtons()
    {
        if (uiManager != null)
        {
            uiManager.ShowTitleButtons();
        }
    }

    // ── 버튼 핸들러 ──────────────────────────────────────────

    private void OnStartClicked()
    {
        Debug.Log("[TitleManager] 게임 시작 클릭 -> LoginManager.Instance.ConnectToSelectedServer() 실행");
        startButtonEvent?.RaiseEvent();
        LoginManager.Instance.ConnectToSelectedServer();
    }

    private void OnServerSelectClicked()
    {
        Debug.Log("[TitleManager] 서버 선택 클릭");
        LoginManager.Instance.ShowServerSelectPopup();
    }

    private void OnSettingsClicked()
    {
        Debug.Log("[TitleManager] 설정 클릭");
        settingsButtonEvent?.RaiseEvent();
        if (settingsManager != null)
        {
            settingsManager.OpenSettingsPanel();
        }
    }

    // ── Public 메서드 ─────────────────────────────────────────

    /// <summary>
    /// Addressables 기반 Async 씬 로딩
    /// </summary>
    private async UniTaskVoid LoadGameSceneAsync()
    {
        if (string.IsNullOrEmpty(gameSceneName))
        {
            Debug.LogError("[TitleManager] gameSceneName이 비어 있습니다.");
            return;
        }

        // Addressables로 씬 비동기 로드
        var handle = Addressables.LoadSceneAsync(gameSceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
        while (!handle.IsDone)
        {
            await UniTask.Yield();
        }
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}