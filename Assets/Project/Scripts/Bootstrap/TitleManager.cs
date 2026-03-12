using Cysharp.Threading.Tasks;
using ProjectFirst.OutGame;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectFirst.Bootstrap
{
    /// <summary>
    /// 타이틀 씬의 메인 흐름을 관리하는 매니저 클래스입니다.
    /// UI Toolkit을 통해 버튼 이벤트를 연결하고 대기/로딩 상태를 제어합니다.
    /// </summary>
    public class TitleManager : MonoBehaviour
    {
        /// <summary>
        /// 싱글톤 인스턴스
        /// </summary>
        public static TitleManager Instance { get; private set; }

        [Header("Scene Settings")]
        [SerializeField] private string _gameSceneName = "Lobby";

        [Header("UI Document")]
        [SerializeField] private UIDocument _uiDocument;

        [Header("Events")]
        [SerializeField] private VoidEventChannelSO _startButtonEvent;
        [SerializeField] private VoidEventChannelSO _settingsButtonEvent;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (_uiDocument == null)
            {
                _uiDocument = GetComponent<UIDocument>();
            }

            SetupUxmlButtonEvents();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void SetupUxmlButtonEvents()
        {
            if (_uiDocument == null) return;

            VisualElement root = _uiDocument.rootVisualElement;
            if (root == null) return;

            // UXML 버튼 바인딩
            Button startButton = root.Q<Button>("start-button");
            if (startButton != null)
            {
                startButton.clicked += OnStartClicked;
            }

            Button serverSelectButton = root.Q<Button>("server-select-button");
            if (serverSelectButton != null)
            {
                serverSelectButton.clicked += OnServerSelectClicked;
            }

            Button settingsButton = root.Q<Button>("settings-button");
            if (settingsButton != null)
            {
                settingsButton.clicked += OnSettingsClicked;
            }

            Button quitButton = root.Q<Button>("quit-button");
            if (quitButton != null)
            {
                quitButton.clicked += QuitGame;
            }
        }

        /// <summary>
        /// 게임 시작 버튼 클릭 이벤트 핸들러
        /// </summary>
        public void OnStartClicked()
        {
            Debug.Log("[TitleManager] Game start clicked.");
            _startButtonEvent?.RaiseEvent();

            // TitleLoadingManager가 있다면 로딩 트리거, 없다면 직접 로드
            TitleLoadingManager loadingManager = FindObjectOfType<TitleLoadingManager>();
            if (loadingManager != null)
            {
                loadingManager.TriggerLoad();
            }
            else
            {
                if (AsyncSceneLoader.Instance != null)
                {
                    AsyncSceneLoader.Instance.LoadScene(_gameSceneName);
                }
                else
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene(_gameSceneName);
                }
            }
        }

        private void OnServerSelectClicked()
        {
            Debug.Log("[TitleManager] Server select clicked.");
            if (LoginManager.Instance != null)
            {
                LoginManager.Instance.ShowServerSelectPopup();
            }
            else
            {
                Debug.LogWarning("[TitleManager] LoginManager instance was not found.");
            }
        }

        private void OnSettingsClicked()
        {
            Debug.Log("[TitleManager] Settings clicked.");
            _settingsButtonEvent?.RaiseEvent();

            // SettingPanel(UI Toolkit 기반)을 찾아 오픈
            SettingPanel settingPanel = FindObjectOfType<SettingPanel>(true);
            if (settingPanel != null)
            {
                settingPanel.OpenPanel();
            }
        }

        /// <summary>
        /// 게임 종료 메서드
        /// </summary>
        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
