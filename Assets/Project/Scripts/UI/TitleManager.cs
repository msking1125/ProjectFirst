using UnityEngine;
using UnityEngine.UIElements;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using ProjectFirst.OutGame;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Project
{

/// <summary>
/// 타이틀 씬 메인 관리자
/// UI 생성, 설정 관리, 씬 로딩 등의 책임을 분리된 클래스들에게 위임
/// </summary>
#if ODIN_INSPECTOR
    [HideMonoScript]
#endif
    public class TitleManager : MonoBehaviour
    {
        public static TitleManager Instance { get; private set; }

#if ODIN_INSPECTOR
        [Title("씬 설정", TitleAlignment = TitleAlignments.Left)]
        [BoxGroup("씬")]
        [LabelText("게임 씬 이름")]
        [Tooltip("게임 시작 시 로드할 씬 이름")]
#endif
        [Header("Scene Settings")]
        [SerializeField] private string gameSceneName = "Battle_Test";
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

        // start-button은 TitleLoadingManager가 직접 처리하므로 여기서는 등록하지 않음
        // (이중 등록 시 씬 로드 충돌 발생)

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
        Debug.Log("[TitleManager] 게임 시작 클릭 (UGUI 버튼)");
        startButtonEvent?.RaiseEvent();

        // TitleLoadingManager에 위임 — 씬 로드는 한 곳에서만 수행
        var loadingManager = FindObjectOfType<TitleLoadingManager>();
        if (loadingManager != null)
        {
            loadingManager.TriggerLoad();
        }
        else
        {
            Debug.LogError("[TitleManager] TitleLoadingManager를 찾을 수 없습니다.");
        }
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

        // 새 UIToolkit 설정 패널 우선, 없으면 레거시 UGUI 폴백
        if (settingPanel != null)
        {
            settingPanel.OpenPanel();
        }
        else if (settingsManager != null)
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