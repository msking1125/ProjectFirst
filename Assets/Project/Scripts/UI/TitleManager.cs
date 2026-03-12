using Cysharp.Threading.Tasks;
using ProjectFirst.OutGame;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

namespace Project
{
    public class TitleManager : MonoBehaviour
    {
        public static TitleManager Instance { get; private set; }

        [Header("Scene Settings")]
        [SerializeField] private string gameSceneName = "Battle_Test";

        [Header("Dependencies")]
        [SerializeField] private TitleUIManager uiManager;
        [SerializeField] private TitleSettingsManager settingsManager;
        [SerializeField] private SettingPanel settingPanel;

        [Header("Events")]
        [SerializeField] private VoidEventChannelSO startButtonEvent;
        [SerializeField] private VoidEventChannelSO settingsButtonEvent;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            uiManager ??= GetComponent<TitleUIManager>();
            if (uiManager == null)
            {
                uiManager = gameObject.AddComponent<TitleUIManager>();
            }

            settingsManager ??= GetComponent<TitleSettingsManager>();
            if (settingsManager == null)
            {
                settingsManager = gameObject.AddComponent<TitleSettingsManager>();
            }

            settingPanel ??= FindObjectOfType<SettingPanel>(true);

            uiManager.OnStartClicked = OnStartClicked;
            uiManager.OnServerSelectClicked = OnServerSelectClicked;
            uiManager.OnSettingsClicked = OnSettingsClicked;

            uiManager.Initialize();
            settingsManager.Initialize();

            ShowTitleButtons();
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
            UIDocument uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null)
            {
                return;
            }

            VisualElement root = uiDocument.rootVisualElement;
            if (root == null)
            {
                return;
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

        public void ShowTitleButtons()
        {
            uiManager?.ShowTitleButtons();
        }

        private void OnStartClicked()
        {
            Debug.Log("[TitleManager] Game start clicked.");
            startButtonEvent?.RaiseEvent();

            TitleLoadingManager loadingManager = FindObjectOfType<TitleLoadingManager>();
            if (loadingManager != null)
            {
                loadingManager.TriggerLoad();
            }
            else
            {
                LoadGameSceneAsync().Forget();
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
            settingsButtonEvent?.RaiseEvent();

            settingPanel ??= FindObjectOfType<SettingPanel>(true);
            if (settingPanel != null)
            {
                settingPanel.OpenPanel();
                return;
            }

            settingsManager?.OpenSettingsPanel();
        }

        private async UniTaskVoid LoadGameSceneAsync()
        {
            if (string.IsNullOrWhiteSpace(gameSceneName))
            {
                Debug.LogError("[TitleManager] gameSceneName is empty.");
                return;
            }

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
}
