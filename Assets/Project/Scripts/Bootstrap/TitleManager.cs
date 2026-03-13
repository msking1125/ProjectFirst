using Cysharp.Threading.Tasks;
using ProjectFirst.OutGame;
using ProjectFirst.OutGame.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectFirst.Bootstrap
{
    public class TitleManager : MonoBehaviour
    {
        public static TitleManager Instance { get; private set; }

        [Header("Scene Settings")]
        [SerializeField] private string _gameSceneName = "Lobby";

        [Header("UI Document")]
        [SerializeField] private UIDocument _uiDocument;

        [Header("Events")]
        [SerializeField] private VoidEventChannelSO _startButtonEvent;
        [SerializeField] private VoidEventChannelSO _settingsButtonEvent;

        private VisualElement _buttonContainer;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            EnsureDocument();
            SetupUxmlButtonEvents();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void EnsureDocument()
        {
            if (_uiDocument != null && _uiDocument.rootVisualElement != null)
                return;

            if (_uiDocument == null)
            {
                _uiDocument = GetComponent<UIDocument>();
            }

            if (_uiDocument == null)
            {
                foreach (UIDocument doc in FindObjectsOfType<UIDocument>())
                {
                    VisualElement root = doc.rootVisualElement;
                    if (root == null) continue;

                    // TitleView 에는 start-button 이 존재합니다.
                    if (root.Q<Button>("start-button") != null)
                    {
                        _uiDocument = doc;
                        break;
                    }
                }
            }

            // Title UI 는 기본 정렬 순서(0)를 사용해 LoginView 보다 뒤에 렌더링되도록 합니다.
            if (_uiDocument != null)
            {
                _uiDocument.sortingOrder = 0;
            }
        }

        private void SetupUxmlButtonEvents()
        {
            if (_uiDocument == null) return;

            VisualElement root = _uiDocument.rootVisualElement;
            if (root == null) return;

            _buttonContainer = root.Q<VisualElement>("ButtonContainer");
            if (_buttonContainer != null)
            {
                // 로그인 완료 전까지는 버튼 숨김
                _buttonContainer.style.display = DisplayStyle.None;
            }
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
        public void OnStartClicked()
        {
            Debug.Log("[TitleManager] Game start clicked.");
            _startButtonEvent?.RaiseEvent();
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

        /// <summary>
        /// 로그인 및 서버 인증 절차가 끝난 뒤 타이틀 버튼을 노출합니다.
        /// </summary>
        public void ShowTitleButtons()
        {
            if (_buttonContainer != null)
            {
                _buttonContainer.style.display = DisplayStyle.Flex;
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
            SettingPanel settingPanel = FindObjectOfType<SettingPanel>(true);
            if (settingPanel != null)
            {
                settingPanel.OpenPanel();
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
}


