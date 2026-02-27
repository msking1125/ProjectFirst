using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

/// <summary>
/// 타이틀 씬 UI 관리자
/// UIDocument를 Inspector에서 연결하거나, 자동으로 탐색합니다.
/// </summary>
public class TitleManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private string gameSceneName = "Battle_Test";

    [Header("Events (선택 - 없어도 동작함)")]
    [SerializeField] private VoidEventChannelSO startButtonEvent;
    [SerializeField] private VoidEventChannelSO settingsButtonEvent;
    [SerializeField] private VoidEventChannelSO quitButtonEvent;

    private Button _startButton;
    private Button _settingsButton;
    private Button _quitButton;

    private void Awake()
    {
        // Inspector에서 연결이 안 된 경우, 씬에서 자동 탐색
        if (uiDocument == null)
        {
            uiDocument = GetComponent<UIDocument>();

            if (uiDocument == null)
                uiDocument = FindObjectOfType<UIDocument>();

            if (uiDocument == null)
                Debug.LogError("[TitleManager] UIDocument를 찾을 수 없습니다. TitleUIRoot 오브젝트에 UIDocument 컴포넌트가 있는지 확인하세요.");
            else
                Debug.Log($"[TitleManager] UIDocument 자동 탐색 성공: {uiDocument.gameObject.name}");
        }
    }

    private void OnEnable()
    {
        if (uiDocument == null) return;

        var root = uiDocument.rootVisualElement;
        if (root == null)
        {
            Debug.LogError("[TitleManager] rootVisualElement가 null입니다. Source Asset(UXML)이 연결되었는지 확인하세요.");
            return;
        }

        _startButton    = root.Q<Button>("start-button");
        _settingsButton = root.Q<Button>("settings-button");
        _quitButton     = root.Q<Button>("quit-button");

        if (_startButton == null)    Debug.LogError("[TitleManager] 'start-button' 버튼을 찾지 못했습니다.");
        if (_settingsButton == null) Debug.LogWarning("[TitleManager] 'settings-button' 버튼을 찾지 못했습니다.");
        if (_quitButton == null)     Debug.LogError("[TitleManager] 'quit-button' 버튼을 찾지 못했습니다.");

        _startButton?.RegisterCallback<ClickEvent>(OnStartClicked);
        _settingsButton?.RegisterCallback<ClickEvent>(OnSettingsClicked);
        _quitButton?.RegisterCallback<ClickEvent>(OnQuitClicked);

        Debug.Log("[TitleManager] 버튼 이벤트 등록 완료");
    }

    private void OnDisable()
    {
        _startButton?.UnregisterCallback<ClickEvent>(OnStartClicked);
        _settingsButton?.UnregisterCallback<ClickEvent>(OnSettingsClicked);
        _quitButton?.UnregisterCallback<ClickEvent>(OnQuitClicked);
    }

    // ── 버튼 핸들러 ──────────────────────────────────────────

    private void OnStartClicked(ClickEvent evt)
    {
        Debug.Log("[TitleManager] 게임 시작 클릭");
        startButtonEvent?.RaiseEvent();
        LoadGameScene();
    }

    private void OnSettingsClicked(ClickEvent evt)
    {
        Debug.Log("[TitleManager] 설정 클릭 (미구현)");
        settingsButtonEvent?.RaiseEvent();
    }

    private void OnQuitClicked(ClickEvent evt)
    {
        Debug.Log("[TitleManager] 종료 클릭");
        quitButtonEvent?.RaiseEvent();
        QuitGame();
    }

    // ── Public 메서드 ─────────────────────────────────────────

    public void LoadGameScene()
    {
        if (string.IsNullOrEmpty(gameSceneName))
        {
            Debug.LogError("[TitleManager] gameSceneName이 비어 있습니다.");
            return;
        }

        // Build Settings에 씬이 등록되어 있는지 확인
        bool sceneExists = false;
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i);
            if (path.Contains(gameSceneName))
            {
                sceneExists = true;
                break;
            }
        }

        if (!sceneExists)
        {
            Debug.LogError($"[TitleManager] '{gameSceneName}' 씬이 Build Settings에 등록되지 않았습니다.\n" +
                           "File > Build Settings > Scenes In Build 에 해당 씬을 추가하세요.");
            return;
        }

        Debug.Log($"[TitleManager] 씬 이동: {gameSceneName}");
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
}
