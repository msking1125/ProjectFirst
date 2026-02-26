using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

/// <summary>
/// 타이틀 씬 UI 관리자
/// UI Toolkit 기반 버튼 이벤트 처리 및 씬 전환
/// </summary>
public class TitleManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private string gameSceneName = "Battle_Test";

    [Header("Events (선택 - ScriptableObject 이벤트 채널 사용 시 연결)")]
    [SerializeField] private VoidEventChannelSO startButtonEvent;
    [SerializeField] private VoidEventChannelSO settingsButtonEvent;
    [SerializeField] private VoidEventChannelSO quitButtonEvent;

    // UI 요소 캐시
    private Button _startButton;
    private Button _settingsButton;
    private Button _quitButton;
    private Label  _titleLabel;

    // ──────────────────────────────────────────
    #region Unity Lifecycle
    private void OnEnable()
    {
        if (uiDocument == null)
        {
            Debug.LogError("[TitleManager] UIDocument가 연결되지 않았습니다.");
            return;
        }

        var root = uiDocument.rootVisualElement;
        QueryElements(root);
        RegisterCallbacks();
    }

    private void OnDisable()
    {
        UnregisterCallbacks();
    }
    #endregion

    // ──────────────────────────────────────────
    #region UI Setup
    private void QueryElements(VisualElement root)
    {
        _startButton    = root.Q<Button>("start-button");
        _settingsButton = root.Q<Button>("settings-button");
        _quitButton     = root.Q<Button>("quit-button");
        _titleLabel     = root.Q<Label>("title-label");

        // null 체크 로그
        if (_startButton    == null) Debug.LogWarning("[TitleManager] start-button 을 찾을 수 없습니다.");
        if (_settingsButton == null) Debug.LogWarning("[TitleManager] settings-button 을 찾을 수 없습니다.");
        if (_quitButton     == null) Debug.LogWarning("[TitleManager] quit-button 을 찾을 수 없습니다.");
    }

    private void RegisterCallbacks()
    {
        _startButton?.RegisterCallback<ClickEvent>(OnStartClicked);
        _settingsButton?.RegisterCallback<ClickEvent>(OnSettingsClicked);
        _quitButton?.RegisterCallback<ClickEvent>(OnQuitClicked);
    }

    private void UnregisterCallbacks()
    {
        _startButton?.UnregisterCallback<ClickEvent>(OnStartClicked);
        _settingsButton?.UnregisterCallback<ClickEvent>(OnSettingsClicked);
        _quitButton?.UnregisterCallback<ClickEvent>(OnQuitClicked);
    }
    #endregion

    // ──────────────────────────────────────────
    #region Button Handlers
    private void OnStartClicked(ClickEvent evt)
    {
        Debug.Log("[TitleManager] 게임 시작");

        // ScriptableObject 이벤트 채널이 연결된 경우 발행
        startButtonEvent?.RaiseEvent();

        // 씬 전환 (이벤트 채널 미사용 시 직접 전환)
        if (startButtonEvent == null)
            LoadGameScene();
    }

    private void OnSettingsClicked(ClickEvent evt)
    {
        Debug.Log("[TitleManager] 설정 열기");
        settingsButtonEvent?.RaiseEvent();
    }

    private void OnQuitClicked(ClickEvent evt)
    {
        Debug.Log("[TitleManager] 게임 종료");
        quitButtonEvent?.RaiseEvent();

        if (quitButtonEvent == null)
            QuitGame();
    }
    #endregion

    // ──────────────────────────────────────────
    #region Public Methods (이벤트 채널 리스너로 연결 가능)
    public void LoadGameScene()
    {
        if (string.IsNullOrEmpty(gameSceneName))
        {
            Debug.LogError("[TitleManager] gameSceneName이 비어 있습니다.");
            return;
        }
        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    #endregion
}
