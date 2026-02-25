using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

/// <summary>
/// 타이틀 화면 UI 및 버튼 동작 관리.
/// 3D 프리뷰 대신, 배경은 별도의 2D 이미지나 월드 연출로 처리합니다.
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

        // UI Document 찾기 우선순위 조정: 
        // (1) 인스펙터 할당, (2) 자식 오브젝트 포함하여 GetComponentInChildren 시도
        if (uiDocument == null)
        {
            // 우선 현재 오브젝트, 없으면 자식에서 찾기
            uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null)
            {
                uiDocument = GetComponentInChildren<UIDocument>(true);
            }
        }
    }

    private void OnEnable()
    {
        // SetupUI에서 uiDocument가 없으면 마지막 시도로 한 번 더 찾아줌
        if (uiDocument == null)
        {
            uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null)
            {
                uiDocument = GetComponentInChildren<UIDocument>(true);
            }
        }
        SetupUI();
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
    }

    private void SetupUI()
    {
        if (uiDocument == null)
        {
            Debug.LogWarning("[TitleManager] UIDocument 가 설정되지 않았습니다. Scene에 UIDocument가 포함된지 다시 확인하세요.");
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
