using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

/// <summary>
/// 타이틀 화면 UI 및 버튼 동작 관리.
/// </summary>
public class TitleManager : MonoBehaviour
{
    public static TitleManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private string gameSceneName = "Battle_Test";

    [Header("Events")]
    [SerializeField] private VoidEventChannelSO startButtonEvent;
    [SerializeField] private VoidEventChannelSO settingsButtonEvent;
    [SerializeField] private VoidEventChannelSO quitButtonEvent;

    [Header("Background (Addressables)")]
    [SerializeField] private bool useAddressableBackground = true;
    [SerializeField] private AssetReferenceGameObject backgroundPrefab;

    private AsyncOperationHandle<GameObject>? _backgroundHandle;

    private Button _startButton;
    private Button _settingsButton;
    private Button _quitButton;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoCreate()
    {
        if (Instance != null) return;

#if UNITY_2022_2_OR_NEWER
        var existing = FindFirstObjectByType<TitleManager>();
#else
        var existing = GameObject.FindObjectOfType<TitleManager>();
#endif

        if (existing != null)
        {
            Instance = existing;
            return;
        }

        var go = new GameObject("TitleManager");
        go.AddComponent<TitleManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (uiDocument == null)
        {
            uiDocument = GetComponent<UIDocument>();
        }
    }

    private void OnEnable()
    {
        SetupUI();
        LoadBackgroundAsync();
    }

    private void OnDisable()
    {
        TeardownUI();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        if (_backgroundHandle.HasValue)
        {
            Addressables.Release(_backgroundHandle.Value);
            _backgroundHandle = null;
        }
    }

    private void SetupUI()
    {
        if (uiDocument == null)
        {
            Debug.LogWarning("[TitleManager] UIDocument 가 설정되지 않았습니다.");
            return;
        }

        var root = uiDocument.rootVisualElement;
        if (root == null)
        {
            Debug.LogWarning("[TitleManager] UIDocument root 가 null 입니다.");
            return;
        }

        _startButton = root.Q<Button>("start-button");
        _settingsButton = root.Q<Button>("settings-button");
        _quitButton = root.Q<Button>("quit-button");

        if (_startButton != null)
        {
            _startButton.clicked += HandleStartClicked;
        }

        if (_settingsButton != null)
        {
            _settingsButton.clicked += HandleSettingsClicked;
        }

        if (_quitButton != null)
        {
            _quitButton.clicked += HandleQuitClicked;
        }
    }

    private void TeardownUI()
    {
        if (_startButton != null)
        {
            _startButton.clicked -= HandleStartClicked;
        }

        if (_settingsButton != null)
        {
            _settingsButton.clicked -= HandleSettingsClicked;
        }

        if (_quitButton != null)
        {
            _quitButton.clicked -= HandleQuitClicked;
        }
    }

    private void LoadBackgroundAsync()
    {
        if (!useAddressableBackground || backgroundPrefab == null)
        {
            return;
        }

        _backgroundHandle = backgroundPrefab.InstantiateAsync();
    }

    private void HandleStartClicked()
    {
        if (startButtonEvent != null)
        {
            startButtonEvent.Raise();
        }

        if (!string.IsNullOrEmpty(gameSceneName))
        {
            AsyncSceneLoader.Instance.LoadSceneAsync(gameSceneName, LoadSceneMode.Single);
        }
        else
        {
            Debug.LogWarning("[TitleManager] gameSceneName 이 비어 있습니다.");
        }
    }

    private void HandleSettingsClicked()
    {
        if (settingsButtonEvent != null)
        {
            settingsButtonEvent.Raise();
        }

        // 설정 창은 나중에 UI Toolkit 패널로 구현
        Debug.Log("[TitleManager] Settings 버튼 클릭.");
    }

    private void HandleQuitClicked()
    {
        if (quitButtonEvent != null)
        {
            quitButtonEvent.Raise();
        }

        Debug.Log("[TitleManager] Quit 버튼 클릭. 애플리케이션 종료.");
        Application.Quit();
    }
}

