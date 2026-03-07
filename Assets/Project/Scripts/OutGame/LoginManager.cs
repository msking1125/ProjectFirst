using System;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Cysharp.Threading.Tasks;

using ProjectFirst.OutGame.Data;
using ProjectFirst.Network;

namespace ProjectFirst.OutGame
{
    public class LoginManager : MonoBehaviour
    {
        public static LoginManager Instance { get; private set; }

        [Header("UI Document")]
        [SerializeField] private UIDocument uiDocument;

        [Header("Data References")]
        [SerializeField] private global::ServerListSO serverListSO;
        [SerializeField] private TextAsset badWordsCSV;

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
            if (uiDocument == null) return;
            var root = uiDocument.rootVisualElement;

            // 1. Login Panel
            loginPanel = root.Q<VisualElement>("LoginPanel");
            btnGoogle = root.Q<Button>("BtnGoogle");
            btnApple = root.Q<Button>("BtnApple");
            btnGuest = root.Q<Button>("BtnGuest");

            if (btnGoogle != null) btnGoogle.clicked += OnGoogleLogin;
            if (btnApple != null) btnApple.clicked += OnAppleLogin;
            if (btnGuest != null) btnGuest.clicked += OnGuestLogin;

            // 2. Nickname Panel
            nicknamePanel = root.Q<VisualElement>("NicknamePanel");
            inputNickname = root.Q<TextField>("InputNickname");
            lblNicknameError = root.Q<Label>("LblNicknameError");
            btnCreateNickname = root.Q<Button>("BtnCreateNickname");
            btnBackToLogin = root.Q<Button>("BtnBackToLogin");

            if (btnCreateNickname != null) btnCreateNickname.clicked += OnCreateNicknameClicked;
            if (btnBackToLogin != null) btnBackToLogin.clicked += ShowLoginPanel;
            
            // 3. Server Select Panel
            serverSelectPanel = root.Q<VisualElement>("ServerSelectPanel");
            lblWelcome = root.Q<Label>("LblWelcome");
            dropdownServer = root.Q<DropdownField>("DropdownServer");
            lblServerError = root.Q<Label>("LblServerError");
            btnConnect = root.Q<Button>("BtnConnect");
            btnLogout = root.Q<Button>("BtnLogout");

            if (btnConnect != null) btnConnect.clicked += OnConfirmServerClicked;
            if (btnLogout != null) btnLogout.clicked += HideAllPanels; // "닫기" 버튼

            ShowLoginPanel();
        }

        private void InitializeDependencies()
        {
            // 모의 API 인스턴스화
            nicknameAPI = new MockNicknameCheckAPI();
            serverConnectionAPI = new MockServerConnectionAPI();

            // 금칙어 데이터 로드
            if (badWordsCSV != null)
            {
                badWordData = new BadWordData(badWordsCSV);
            }
            else
            {
                Debug.LogWarning("[LoginManager] BadWords CSV 파일이 할당되지 않았습니다. 금칙어 검사를 건너뜁니다.");
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

            string currentNickname = PlayerPrefs.GetString("nickname", "Unknown");
            if (lblWelcome != null)
            {
                lblWelcome.text = $"환영합니다, {currentNickname}님";
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

            List<string> serverNames = new List<string>();
            foreach (var server in serverListSO.servers)
            {
                serverNames.Add($"{server.displayName} ({server.CongestionLabel})");
            }

            dropdownServer.choices = serverNames;
            
            string lastServerId = PlayerPrefs.GetString("lastServer", string.Empty);
            int indexToSelect = 0;
            
            if (!string.IsNullOrEmpty(lastServerId))
            {
                int foundIndex = serverListSO.servers.FindIndex(s => s.serverId == lastServerId);
                if (foundIndex >= 0) indexToSelect = foundIndex;
            }

            if (serverNames.Count > 0)
            {
                dropdownServer.index = indexToSelect;
            }
        }

        // --- Login Actions ---

        private void OnGoogleLogin() => ProcessLogin("google");
        private void OnAppleLogin() => ProcessLogin("apple");
        private void OnGuestLogin() => ProcessLogin("guest");

        private void ProcessLogin(string provider)
        {
            Debug.Log($"[LoginManager] {provider} 플랫폼으로 로그인 시도");

            // 유저 UID 체크
            if (!PlayerPrefs.HasKey("uid"))
            {
                // 신규 유저
                Debug.Log("[LoginManager] 등록된 UID가 없습니다. 신규 유저 가입 절차를 진행합니다.");
                string newUid = Guid.NewGuid().ToString();
                PlayerPrefs.SetString("uid_temp", newUid); // 아직 확정 전
                ShowNicknamePanel();
            }
            else
            {
                // 기존 유저
                Debug.Log("[LoginManager] 기존 UID를 확인했습니다. 타이틀 화면으로 진입합니다.");
                OnLoginComplete();
            }
        }

        private void OnLoginComplete()
        {
            HideAllPanels();
            if (TitleManager.Instance != null)
            {
                TitleManager.Instance.ShowTitleButtons();
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
                // 1. 형식 유효성 검사 (길이 1~8)
                if (string.IsNullOrEmpty(nickname) || nickname.Length > 8)
                {
                    ShowNicknameError("닉네임은 1~8자 사이여야 합니다.");
                    return;
                }

                // 2. 정규식 검사 (한글/영문/숫자, 공백 및 특문 제외)
                if (!Regex.IsMatch(nickname, @"^[가-힣a-zA-Z0-9]+$"))
                {
                    ShowNicknameError("공백 및 특수문자는 사용할 수 없습니다.");
                    return;
                }

                // 3. 로컬 금칙어 검사
                if (badWordData != null && badWordData.ContainsBadWord(nickname))
                {
                    ShowNicknameError("사용할 수 없는 단어가 포함되어 있습니다.");
                    return;
                }

                // 4. 모의 서버 API 중복 검사
                bool isAvailable = await nicknameAPI.CheckDuplicateAsync(nickname);
                if (!isAvailable)
                {
                    ShowNicknameError("이미 사용 중인 닉네임입니다.");
                    return;
                }

                // 5. 완료 및 저장
                string allocatedUid = PlayerPrefs.GetString("uid_temp", Guid.NewGuid().ToString());
                
                PlayerPrefs.SetString("uid", allocatedUid);
                PlayerPrefs.SetString("nickname", nickname);
                PlayerPrefs.SetInt("isNewUser", 0); // false
                PlayerPrefs.DeleteKey("uid_temp");
                PlayerPrefs.Save();

                Debug.Log($"[LoginManager] 닉네임 '{nickname}' 프로필 생성 완료");
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
            if (dropdownServer == null || serverListSO == null) return;

            int selectedIndex = dropdownServer.index;
            if (selectedIndex < 0 || selectedIndex >= serverListSO.servers.Count) return;

            var selectedServer = serverListSO.servers[selectedIndex];
            
            // 선택한 서버 저장
            PlayerPrefs.SetString("lastServer", selectedServer.serverId);
            PlayerPrefs.Save();
            
            Debug.Log($"[LoginManager] 서버 선택 완료: {selectedServer.displayName}");
            HideAllPanels();
        }

        public void ConnectToSelectedServer()
        {
            string lastServerId = PlayerPrefs.GetString("lastServer", string.Empty);
            
            ServerData targetServer = null;
            if (serverListSO != null && serverListSO.servers.Count > 0)
            {
                targetServer = serverListSO.servers.FirstOrDefault(s => s.serverId == lastServerId) ?? serverListSO.servers[0];
            }

            if (targetServer != null)
            {
                // IServerConnectionAPI uses its own ServerInfo, map it
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
                Debug.LogError("[LoginManager] 접속 가능한 서버 정보를 찾을 수 없습니다.");
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
                    // 최근 접속 서버 저장
                    PlayerPrefs.SetString("lastServer", serverInfo.serverId);
                    PlayerPrefs.Save();

                    Debug.Log("[LoginManager] 접속 성공, Lobby 씬으로 이동합니다.");
                    
                    // 싱글톤 어드레서블 씬 로더 활용 (Lobby)
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
                        lblServerError.text = "서버 접속이 원활하지 않습니다. (포화 상태)";
                        lblServerError.style.display = DisplayStyle.Flex;
                    }
                }
            }
            finally
            {
                if (btnConnect != null) btnConnect.SetEnabled(true);
            }
        }
    }
}
