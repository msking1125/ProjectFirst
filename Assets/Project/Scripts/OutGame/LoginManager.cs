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
            if (uiDocument == null) return;

            // LoginView.uxml??uGUI ScreenSpaceOverlay Canvas ?꾩뿉 ?뚮뜑留곷릺?꾨줉 sortingOrder ?ㅼ젙
            if (uiDocument.panelSettings != null)
                uiDocument.panelSettings.sortingOrder = 200;

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
            // 紐⑥쓽 API ?몄뒪?댁뒪??
            nicknameAPI = new MockNicknameCheckAPI();
            serverConnectionAPI = new MockServerConnectionAPI();

            // 湲덉튃???곗씠??濡쒕뱶
            if (badWordsCSV != null)
            {
                badWordData = new BadWordData(badWordsCSV);
            }
            else
            {
                Debug.LogWarning("[LoginManager] BadWords CSV ?뚯씪???좊떦?섏? ?딆븯?듬땲?? 湲덉튃??寃?щ? 嫄대꼫?곷땲??");
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
        private void OnGuestLogin() => ProcessLogin("guest");

        private void ProcessLogin(string provider)
        {
            Debug.Log($"[LoginManager] {provider} ?뚮옯?쇱쑝濡?濡쒓렇???쒕룄");

            // ?좎? UID 泥댄겕
            string currentUid = playerData != null ? playerData.GetUidOrCreate() : PlayerPrefs.GetString("uid", string.Empty);
            if (string.IsNullOrEmpty(currentUid))
            {
                // ?좉퇋 ?좎?
                Debug.Log("[LoginManager] ?깅줉??UID媛 ?놁뒿?덈떎. ?좉퇋 ?좎? 媛???덉감瑜?吏꾪뻾?⑸땲??");
                string newUid = Guid.NewGuid().ToString();
                PlayerPrefs.SetString("uid_temp", newUid); // ?꾩쭅 ?뺤젙 ??
                ShowNicknamePanel();
            }
            else
            {
                // 湲곗〈 ?좎?
                Debug.Log("[LoginManager] 湲곗〈 UID瑜??뺤씤?덉뒿?덈떎. ??댄? ?붾㈃?쇰줈 吏꾩엯?⑸땲??");
                OnLoginComplete();
            }
        }

        private void OnLoginComplete()
        {
            HideAllPanels();
            // ??댄? 踰꾪듉?ㅼ? TitleManager?먯꽌 ?먮룞?쇰줈 ?쒖떆??
            Debug.Log("[LoginManager] 濡쒓렇???꾨즺 - ??댄? ?붾㈃?쇰줈 ?꾪솚");
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
                // 1. ?뺤떇 ?좏슚??寃??(湲몄씠 1~8)
                if (string.IsNullOrEmpty(nickname) || nickname.Length > 8)
                {
                    ShowNicknameError("?됰꽕?꾩? 1~8???ъ씠?ъ빞 ?⑸땲??");
                    return;
                }

                // 2. ?뺢퇋??寃??(?쒓?/?곷Ц/?レ옄, 怨듬갚 諛??밸Ц ?쒖쇅)
                if (!Regex.IsMatch(nickname, @"^[媛-?즑-zA-Z0-9]+$"))
                {
                    ShowNicknameError("怨듬갚 諛??뱀닔臾몄옄???ъ슜?????놁뒿?덈떎.");
                    return;
                }

                // 3. 濡쒖뺄 湲덉튃??寃??
                if (badWordData != null && badWordData.ContainsBadWord(nickname))
                {
                    ShowNicknameError("?ъ슜?????녿뒗 ?⑥뼱媛 ?ы븿?섏뼱 ?덉뒿?덈떎.");
                    return;
                }

                // 4. 紐⑥쓽 ?쒕쾭 API 以묐났 寃??
                bool isAvailable = await nicknameAPI.CheckDuplicateAsync(nickname);
                if (!isAvailable)
                {
                    ShowNicknameError("?대? ?ъ슜 以묒씤 ?됰꽕?꾩엯?덈떎.");
                    return;
                }

                // 5. ?꾨즺 諛????
                string allocatedUid = PlayerPrefs.GetString("uid_temp", Guid.NewGuid().ToString());
                
                PlayerPrefs.SetString("uid", allocatedUid);
                PlayerPrefs.SetString("nickname", nickname);
                playerData?.SetUidValue(allocatedUid);
                playerData?.SetNicknameValue(nickname);
                PlayerPrefs.SetInt("isNewUser", 0); // false
                PlayerPrefs.DeleteKey("uid_temp");
                PlayerPrefs.Save();

                Debug.Log($"[LoginManager] ?됰꽕??'{nickname}' ?꾨줈???앹꽦 ?꾨즺");
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
                ShowServerError("?쒕쾭 紐⑸줉??遺덈윭?ㅼ? 紐삵뻽?듬땲??");
                return;
            }

            int selectedIndex = dropdownServer.index;
            if (selectedIndex < 0 || selectedIndex >= serverListSO.servers.Count)
            {
                ShowServerError("?묒냽???쒕쾭瑜??좏깮??二쇱꽭??");
                return;
            }

            var selectedServer = serverListSO.servers[selectedIndex];

            PlayerPrefs.SetString("lastServer", selectedServer.serverId);
            PlayerPrefs.Save();
            playerData?.SetSelectedServerId(selectedServer.serverId);

            Debug.Log($"[LoginManager] ?쒕쾭 ?좏깮 ?꾨즺: {selectedServer.displayName}");
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
                Debug.LogError("[LoginManager] ?묒냽 媛?ν븳 ?쒕쾭 ?뺣낫瑜?李얠쓣 ???놁뒿?덈떎.");
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
                    // 理쒓렐 ?묒냽 ?쒕쾭 ???
                    PlayerPrefs.SetString("lastServer", serverInfo.serverId);
                    PlayerPrefs.Save();
                    playerData?.SetSelectedServerId(serverInfo.serverId);

                    Debug.Log("[LoginManager] ?묒냽 ?깃났, Lobby ?ъ쑝濡??대룞?⑸땲??");

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
                        lblServerError.text = "?쒕쾭 ?묒냽???먰솢?섏? ?딆뒿?덈떎. (?ы솕 ?곹깭)";
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







