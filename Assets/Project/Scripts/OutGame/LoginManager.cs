using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using ProjectFirst.Data;
using ProjectFirst.Network;
using ProjectFirst.OutGame.Data;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;

namespace ProjectFirst.OutGame
{
    public class LoginManager : MonoBehaviour
    {
        public static LoginManager Instance { get; private set; }
        [Header("UI Document")]
        [SerializeField] private UIDocument uiDocument;
        [Header("Data References")]
        [SerializeField] private ServerListSO serverListSO;
        [SerializeField] private TextAsset badWordsCSV;
        [SerializeField] private PlayerData playerData;

        // UI Elements
        private VisualElement loginPanel;
        private Button btnGoogle;
        private Button btnApple;
        private Button btnGuest;

        private VisualElement nicknamePanel;
        private TextField inputNickname;
        private Label lblNicknameError;
        private Button btnCreateNickname;
        private Button btnBackToLogin;

        private VisualElement serverSelectPanel;
        private Label lblWelcome;
        private DropdownField dropdownServer;
        private Label lblServerError;
        private Button btnConnect;
        private Button btnLogout;
        private readonly List<string> serverDropdownChoices = new();

        // State & Dependencies
        private BadWordData badWordData;
        private INicknameCheckAPI nicknameAPI;
        private IServerConnectionAPI serverConnectionAPI;

        private void Awake()
        {
            Instance = this;
            InitializeDependencies();
        }

        private void OnEnable()
        {
            if (uiDocument == null)
            {
                uiDocument = GetComponent<UIDocument>();

                if (uiDocument == null)
                {
                    foreach (var doc in FindObjectsOfType<UIDocument>())
                    {
                        var rootCandidate = doc.rootVisualElement;
                        if (rootCandidate == null) continue;

                        // LoginView 에는 LoginPanel 이 존재합니다.
                        if (rootCandidate.Q<VisualElement>("LoginPanel") != null)
                        {
                            uiDocument = doc;
                            break;
                        }
                    }
                }
            }

            if (uiDocument == null) return;

            // 로그인 UI는 Title UI 보다 앞쪽 레이어에서 렌더링되도록 설정합니다.
            // PanelSettings.sortingOrder 대신 UIDocument.sortingOrder 를 사용해
            // TitleView 와의 충돌을 피합니다.
            uiDocument.sortingOrder = 200;

            var root = uiDocument.rootVisualElement;

            // 1. Login panel
            loginPanel = root.Q<VisualElement>("LoginPanel");
            btnGoogle = root.Q<Button>("BtnGoogle");
            btnApple = root.Q<Button>("BtnApple");
            btnGuest = root.Q<Button>("BtnGuest");

            if (btnGuest == null)
            {
                Debug.LogError("[LoginManager] BtnGuest 버튼을 찾지 못했습니다. UXML 이름을 확인해주세요.");
            }
            else
            {
                Debug.Log("[LoginManager] BtnGuest 버튼 바인딩 완료.");
                // 클릭 이벤트를 두 경로 모두에 등록해 안정성을 높입니다.
                btnGuest.clicked -= OnGuestLogin;
                btnGuest.clicked += OnGuestLogin;
                btnGuest.RegisterCallback<ClickEvent>(_ => OnGuestLogin());
            }

            if (btnGoogle != null) btnGoogle.clicked += OnGoogleLogin;
            if (btnApple != null) btnApple.clicked += OnAppleLogin;

            // 2. Nickname panel
            nicknamePanel = root.Q<VisualElement>("NicknamePanel");
            inputNickname = root.Q<TextField>("InputNickname");
            lblNicknameError = root.Q<Label>("LblNicknameError");
            btnCreateNickname = root.Q<Button>("BtnCreateNickname");
            btnBackToLogin = root.Q<Button>("BtnBackToLogin");

            if (btnCreateNickname != null) btnCreateNickname.clicked += OnCreateNicknameClicked;
            if (btnBackToLogin != null) btnBackToLogin.clicked += ShowLoginPanel;
            
            // 3. Server selection panel
            serverSelectPanel = root.Q<VisualElement>("ServerSelectPanel");
            lblWelcome = root.Q<Label>("LblWelcome");
            dropdownServer = root.Q<DropdownField>("DropdownServer");
            lblServerError = root.Q<Label>("LblServerError");
            btnConnect = root.Q<Button>("BtnConnect");
            btnLogout = root.Q<Button>("BtnLogout");

            if (btnConnect != null)
            {
                btnConnect.clicked -= OnConfirmServerClicked;
                btnConnect.clicked += OnConfirmServerClicked;
            }
            if (btnLogout != null)
            {
                btnLogout.clicked -= OnCloseServerSelectClicked;
                btnLogout.clicked += OnCloseServerSelectClicked;
            }

            ShowLoginPanel();
        }

        private void InitializeDependencies()
        {
            // Mock APIs used until the real backend is wired up.
            nicknameAPI = new MockNicknameCheckAPI();
            serverConnectionAPI = new MockServerConnectionAPI();

            // Load the local bad-word table if available.
            // 빌드 환경에서 TextAsset이 누락되었거나 손상되더라도 로그인 흐름이 막히지 않도록 예외를 방지합니다.
            if (badWordsCSV != null)
            {
                try
                {
                    badWordData = new BadWordData(badWordsCSV);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[LoginManager] Failed to load BadWords CSV. Profanity filtering will be skipped. Exception: {e.Message}");
                    badWordData = null;
                }
            }
            else
            {
                Debug.LogWarning("[LoginManager] BadWords CSV is not assigned. Profanity filtering will be skipped.");
            }
        }

        private void HideAllPanels()
        {
            loginPanel?.AddToClassList("hidden");
            nicknamePanel?.AddToClassList("hidden");
            serverSelectPanel?.AddToClassList("hidden");
        }

        private void ShowLoginPanel()
        {
            HideAllPanels();
            loginPanel?.RemoveFromClassList("hidden");
        }

        private void ShowNicknamePanel()
        {
            HideAllPanels();
            
            if (lblNicknameError != null)
            {
                lblNicknameError.style.display = DisplayStyle.None;
            }
            
            if (inputNickname != null)
            {
                inputNickname.value = string.Empty;
            }

            nicknamePanel?.RemoveFromClassList("hidden");
        }

        private void ShowServerSelectPanel()
        {
            HideAllPanels();

            string currentNickname = playerData != null ? playerData.GetNicknameOrDefault("Unknown") : PlayerPrefs.GetString("nickname", "Unknown");
            if (lblWelcome != null)
            {
                lblWelcome.text = $"Welcome, {currentNickname}!";
            }

            if (lblServerError != null)
            {
                lblServerError.style.display = DisplayStyle.None;
            }

            PopulateServerDropdown();
            serverSelectPanel?.RemoveFromClassList("hidden");
        }

        private void PopulateServerDropdown()
        {
            if (dropdownServer == null || serverListSO == null) return;

            serverDropdownChoices.Clear();
            foreach (var server in serverListSO.servers)
            {
                serverDropdownChoices.Add($"{server.displayName} ({server.CongestionLabel})");
            }

            dropdownServer.choices = serverDropdownChoices;

            string lastServerId = playerData != null ? playerData.GetSelectedServerId() : PlayerPrefs.GetString("lastServer", string.Empty);
            int indexToSelect = 0;

            if (!string.IsNullOrEmpty(lastServerId))
            {
                int foundIndex = serverListSO.servers.FindIndex(s => s.serverId == lastServerId);
                if (foundIndex >= 0) indexToSelect = foundIndex;
            }

            if (serverDropdownChoices.Count > 0)
            {
                indexToSelect = Mathf.Clamp(indexToSelect, 0, serverDropdownChoices.Count - 1);
                dropdownServer.index = indexToSelect;
                dropdownServer.value = serverDropdownChoices[indexToSelect];
                dropdownServer.SetEnabled(true);
            }
            else
            {
                dropdownServer.index = -1;
                dropdownServer.value = string.Empty;
                dropdownServer.SetEnabled(false);
            }
        }

        // --- Login Actions ---

        private void OnGoogleLogin() => ProcessLogin("google");
        private void OnAppleLogin() => ProcessLogin("apple");
        private void OnGuestLogin()
        {
            Debug.Log("[LoginManager] Guest 버튼 클릭 감지.");
            ProcessLogin("guest");
        }

        private void ProcessLogin(string provider)
        {
            Debug.Log($"[LoginManager] Login requested with provider: {provider}");

            // Check whether a UID already exists for this account.
            // 새 유저 여부 판단 단계에서는 UID를 자동 생성하지 않습니다.
            string currentUid = playerData != null ? playerData.uid : PlayerPrefs.GetString("uid", string.Empty);
            if (string.IsNullOrEmpty(currentUid))
            {
                // New user flow.
                Debug.Log("[LoginManager] No registered UID was found. Starting the new-user flow.");
                string newUid = Guid.NewGuid().ToString();
                PlayerPrefs.SetString("uid_temp", newUid); // Temporary UID until nickname creation is completed.
                ShowNicknamePanel();
            }
            else
            {
                // Returning user flow.
                Debug.Log("[LoginManager] Existing UID detected. Proceeding to the title flow.");
                OnLoginComplete();
            }
        }

        private void OnLoginComplete()
        {
            HideAllPanels();
            // 로그인 완료 후 서버 선택 단계로 이동합니다.
            ShowServerSelectPanel();
            Debug.Log("[LoginManager] Login completed. Showing server select panel.");

            // 로그인이 끝났으므로 TitleManager 에게 타이틀 버튼을 노출하도록 요청
            if (ProjectFirst.Bootstrap.TitleManager.Instance != null)
            {
                ProjectFirst.Bootstrap.TitleManager.Instance.ShowTitleButtons();
            }
        }

        // --- Nickname ---

        private void OnCreateNicknameClicked()
        {
            string nick = inputNickname?.value?.Trim();
            ValidateAndCreateNicknameAsync(nick).Forget();
        }

        private async UniTaskVoid ValidateAndCreateNicknameAsync(string nickname)
        {
            if (btnCreateNickname != null) btnCreateNickname.SetEnabled(false);

            try
            {
                // 1. Validate nickname length (1 to 8 characters).
                if (string.IsNullOrEmpty(nickname) || nickname.Length > 8)
                {
                    ShowNicknameError("닉네임은 1자 이상 8자 이하로 입력해주세요.");
                    return;
                }

                // 2. Reject whitespace and special characters.
                if (!Regex.IsMatch(nickname, @"^[?-?a-zA-Z0-9]+$"))
                {
                    ShowNicknameError("닉네임은 한글, 영문, 숫자만 사용할 수 있습니다.");
                    return;
                }

                // 3. Check the local profanity list.
                if (badWordData != null && badWordData.ContainsBadWord(nickname))
                {
                    ShowNicknameError("사용할 수 없는 단어가 포함되어 있습니다.");
                    return;
                }

                // 4. Check duplicate nickname through the mock API.
                bool isAvailable = await nicknameAPI.CheckDuplicateAsync(nickname);
                if (!isAvailable)
                {
                    ShowNicknameError("이미 사용 중인 닉네임입니다.");
                    return;
                }

                // 5. Finalize the account and persist it.
                string allocatedUid = PlayerPrefs.GetString("uid_temp", Guid.NewGuid().ToString());
                
                PlayerPrefs.SetString("uid", allocatedUid);
                PlayerPrefs.SetString("nickname", nickname);
                playerData?.SetUidValue(allocatedUid);
                playerData?.SetNicknameValue(nickname);
                PlayerPrefs.SetInt("isNewUser", 0); // false
                PlayerPrefs.DeleteKey("uid_temp");
                PlayerPrefs.Save();

                Debug.Log($"[LoginManager] Nickname created successfully: {nickname}");
                OnLoginComplete();
            }
            finally
            {
                if (btnCreateNickname != null) btnCreateNickname.SetEnabled(true);
            }
        }

        private void ShowNicknameError(string msg)
        {
            if (lblNicknameError == null) return;
            lblNicknameError.text = msg;
            lblNicknameError.style.display = DisplayStyle.Flex;
        }

        // --- Server Select ---

        public void ShowServerSelectPopup()
        {
            ShowServerSelectPanel();
        }

        private void OnConfirmServerClicked()
        {
            if (dropdownServer == null || serverListSO == null || serverListSO.servers.Count == 0)
            {
                ShowServerError("서버 목록을 불러오지 못했습니다.");
                return;
            }

            int selectedIndex = dropdownServer.index;
            if (selectedIndex < 0 || selectedIndex >= serverListSO.servers.Count)
            {
                ShowServerError("서버 연결에 실패했습니다.");
                return;
            }

            var selectedServer = serverListSO.servers[selectedIndex];

            PlayerPrefs.SetString("lastServer", selectedServer.serverId);
            PlayerPrefs.Save();
            playerData?.SetSelectedServerId(selectedServer.serverId);

            Debug.Log($"[LoginManager] Server selected: {selectedServer.displayName}");
            ConnectToSelectedServer();
        }

        private void OnCloseServerSelectClicked()
        {
            if (lblServerError != null)
            {
                lblServerError.style.display = DisplayStyle.None;
            }

            serverSelectPanel?.AddToClassList("hidden");
        }

        public void ConnectToSelectedServer()
        {
            string lastServerId = playerData != null ? playerData.GetSelectedServerId() : PlayerPrefs.GetString("lastServer", string.Empty);
            
            ServerData targetServer = null;
            if (serverListSO != null && serverListSO.servers.Count > 0)
            {
                targetServer = serverListSO.servers.FirstOrDefault(s => s.serverId == lastServerId) ?? serverListSO.servers[0];
            }

            if (targetServer != null)
            {
                // IServerConnectionAPI uses its own ServerInfo type, so map it here.
                var apiServerInfo = new ServerInfo
                {
                    serverId = targetServer.serverId,
                    serverName = targetServer.displayName,
                    // Mock data properties
                    serverIP = "127.0.0.1",
                    port = 8080,
                    status = targetServer.LoadRatio < 0.5f ? ServerStatus.Smooth : 
                             targetServer.LoadRatio < 0.85f ? ServerStatus.Busy : ServerStatus.Full
                };
                ConnectToServerAsync(apiServerInfo).Forget();
            }
            else
            {
                Debug.LogError("[LoginManager] No valid server information was found for connection.");
            }
        }

        private async UniTaskVoid ConnectToServerAsync(ServerInfo serverInfo)
        {
            if (btnConnect != null) btnConnect.SetEnabled(false);
            if (lblServerError != null) lblServerError.style.display = DisplayStyle.None;

            try
            {
                bool success = await serverConnectionAPI.ConnectToAsync(serverInfo);
                
                if (success)
                {
                    // Save the most recently connected server.
                    PlayerPrefs.SetString("lastServer", serverInfo.serverId);
                    PlayerPrefs.Save();
                    playerData?.SetSelectedServerId(serverInfo.serverId);

                    Debug.Log("[LoginManager] Connection succeeded. Moving to the Lobby scene.");

                    if (AsyncSceneLoader.Instance != null)
                    {
                        AsyncSceneLoader.Instance.LoadSceneAsync("Lobby", UnityEngine.SceneManagement.LoadSceneMode.Single);
                    }
                    else
                    {
                        UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby");
                    }
                }
                else
                {
                    if (lblServerError != null)
                    {
                        lblServerError.text = "서버 목록을 불러오지 못했습니다. 잠시 후 다시 시도해주세요.";
                        lblServerError.style.display = DisplayStyle.Flex;
                    }
                }
            }
            finally
            {
                if (btnConnect != null) btnConnect.SetEnabled(true);
            }
        }
        private void ShowServerError(string message)
        {
            if (lblServerError == null) return;
            lblServerError.text = message;
            lblServerError.style.display = DisplayStyle.Flex;
        }
    }
}








