using System;
using System.Collections;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 로그인 씬 메인 관리자.
///
/// ── 흐름 ──────────────────────────────────────────────────────
///  [로그인 팝업] 구글/애플/게스트 선택
///       ↓ 인증 성공
///  [서버 선택 팝업] 서버 리스트 표시
///       ↓ 서버 선택
///  기존 유저 → 로비 씬 이동
///  신규 유저 → [닉네임 입력 팝업] → 중복 확인 → 로비 씬 이동
///
/// ── Inspector 연결 가이드 ─────────────────────────────────────
/// [Data]
///   playerData        : PlayerData.asset
///   serverList        : ServerList.asset
///   lobbySceneName    : "Lobby" (씬 이름)
///
/// [Login Popup]
///   loginPopup        : 로그인 선택 팝업 루트 GameObject
///   googleButton      : 구글 로그인 Button
///   appleButton       : 애플 로그인 Button
///   guestButton       : 게스트 로그인 Button
///
/// [Server Popup]
///   serverPopup       : 서버 선택 팝업 루트 GameObject
///   serverItemPrefab  : 서버 항목 프리팹 (ServerListItem 컴포넌트 필요)
///   serverListContent : 서버 항목이 추가될 Scroll View Content Transform
///   serverConfirmBtn  : 서버 선택 확인 Button
///   refreshButton     : 서버 목록 새로고침 Button
///
/// [Nickname Popup]
///   nicknamePopup     : 닉네임 입력 팝업 루트 GameObject
///   nicknameInput     : TMP_InputField (최대 8자 자동 제한)
///   charCountText     : "0/8" 형태로 글자 수 표시하는 TMP_Text
///   duplicateCheckBtn : 중복 확인 Button
///   nicknameConfirmBtn: 완료 Button
///   nicknameStatusText: 중복 결과 안내 TMP_Text
/// </summary>
[DisallowMultipleComponent]
public class LoginManager : MonoBehaviour
{
    // ── 상수 ──────────────────────────────────────────────────

    private const int NicknameMaxLength = 8;
    // 한글(완성형+자모)+영문+숫자+언더스코어 허용
    private static readonly Regex NicknameRegex = new(@"^[가-힣ㄱ-ㅎㅏ-ㅣa-zA-Z0-9_]+$");

    // ── Data ──────────────────────────────────────────────────

    [Header("Data")]
    [SerializeField] private PlayerData playerData;
    [SerializeField] private ServerListSO serverList;
    [SerializeField] private string lobbySceneName = "Lobby";

    // ── Login Popup ───────────────────────────────────────────

    [Header("Login Popup")]
    [SerializeField] private GameObject loginPopup;
    [SerializeField] private Button googleButton;
    [SerializeField] private Button appleButton;
    [SerializeField] private Button guestButton;

    // ── Server Popup ──────────────────────────────────────────

    [Header("Server List Popup")]
    [SerializeField] private GameObject serverPopup;
    [SerializeField] private ServerListItem serverItemPrefab;
    [SerializeField] private Transform serverListContent;
    [SerializeField] private Button serverConfirmBtn;
    [SerializeField] private Button refreshButton;

    // ── Nickname Popup ────────────────────────────────────────

    [Header("Nickname Popup")]
    [SerializeField] private GameObject nicknamePopup;
    [SerializeField] private TMP_InputField nicknameInput;
    [SerializeField] private TMP_Text charCountText;
    [SerializeField] private Button duplicateCheckBtn;
    [SerializeField] private Button nicknameConfirmBtn;
    [SerializeField] private TMP_Text nicknameStatusText;

    // ── 런타임 상태 ───────────────────────────────────────────

    private string selectedServerId;
    private bool isDuplicateChecked;
    private bool isDuplicateOk;
    private LoginProvider currentProvider;

    public enum LoginProvider { Google, Apple, Guest }

    // ── 생명주기 ──────────────────────────────────────────────

    private void Awake()
    {
        BindButtons();
        ShowPopup(loginPopup);
        HidePopup(serverPopup);
        HidePopup(nicknamePopup);
    }

    // ── 버튼 바인딩 ───────────────────────────────────────────

    private void BindButtons()
    {
        googleButton?.onClick.AddListener(() => OnLoginClicked(LoginProvider.Google));
        appleButton?.onClick.AddListener(()  => OnLoginClicked(LoginProvider.Apple));
        guestButton?.onClick.AddListener(()  => OnLoginClicked(LoginProvider.Guest));

        serverConfirmBtn?.onClick.AddListener(OnServerConfirmed);
        refreshButton?.onClick.AddListener(RefreshServerList);

        duplicateCheckBtn?.onClick.AddListener(OnDuplicateCheckClicked);
        nicknameConfirmBtn?.onClick.AddListener(OnNicknameConfirmed);

        if (nicknameInput != null)
        {
            nicknameInput.characterLimit = NicknameMaxLength;
            nicknameInput.onValueChanged.AddListener(OnNicknameInputChanged);
        }

        // 확인 버튼 초기 비활성화
        if (serverConfirmBtn)   serverConfirmBtn.interactable   = false;
        if (nicknameConfirmBtn) nicknameConfirmBtn.interactable = false;
    }

    // ── 로그인 ────────────────────────────────────────────────

    private void OnLoginClicked(LoginProvider provider)
    {
        currentProvider = provider;
        Debug.Log($"[LoginManager] 로그인 시도: {provider}");

        // TODO: 실제 SDK 인증 호출 (Firebase Auth 등)
        // 아래는 즉시 성공 처리 (더미)
        SimulateAuthentication(provider);
    }

    /// <summary>
    /// 인증 성공 후 처리. 실제 구현 시 SDK 콜백에서 호출하세요.
    /// </summary>
    public void OnAuthSuccess(bool isNewUser)
    {
        Debug.Log($"[LoginManager] 인증 성공 (신규: {isNewUser})");
        HidePopup(loginPopup);
        ShowServerListPopup();

        // isNewUser 정보를 서버 선택 이후에 사용하기 위해 저장
        PlayerPrefs.SetInt("IsNewUser", isNewUser ? 1 : 0);
    }

    private void SimulateAuthentication(LoginProvider provider)
    {
        // 에디터 더미: 게스트는 항상 신규 유저, 나머지는 기존 유저
        bool isNew = provider == LoginProvider.Guest;
        OnAuthSuccess(isNew);
    }

    // ── 서버 선택 ─────────────────────────────────────────────

    private void ShowServerListPopup()
    {
        ShowPopup(serverPopup);
        RefreshServerList();
    }

    private void RefreshServerList()
    {
        if (serverListContent == null || serverItemPrefab == null) return;

        // 기존 목록 제거
        foreach (Transform child in serverListContent)
            Destroy(child.gameObject);

        selectedServerId = null;
        if (serverConfirmBtn) serverConfirmBtn.interactable = false;

        if (serverList == null || serverList.servers == null) return;

        foreach (var data in serverList.servers)
        {
            ServerListItem item = Instantiate(serverItemPrefab, serverListContent);
            item.Setup(data, OnServerItemSelected);
        }
    }

    private void OnServerItemSelected(string serverId)
    {
        selectedServerId = serverId;
        if (serverConfirmBtn) serverConfirmBtn.interactable = !string.IsNullOrEmpty(serverId);
        Debug.Log($"[LoginManager] 서버 선택: {serverId}");
    }

    private void OnServerConfirmed()
    {
        if (string.IsNullOrEmpty(selectedServerId)) return;

        Debug.Log($"[LoginManager] 서버 확정: {selectedServerId}");
        HidePopup(serverPopup);

        bool isNewUser = PlayerPrefs.GetInt("IsNewUser", 0) == 1;
        if (isNewUser)
            ShowNicknamePopup();
        else
            GoToLobby();
    }

    // ── 닉네임 입력 ───────────────────────────────────────────

    private void ShowNicknamePopup()
    {
        ShowPopup(nicknamePopup);
        isDuplicateChecked = false;
        isDuplicateOk      = false;
        if (nicknameInput)       nicknameInput.text = string.Empty;
        if (nicknameStatusText)  nicknameStatusText.text = string.Empty;
        if (nicknameConfirmBtn)  nicknameConfirmBtn.interactable = false;
        UpdateCharCount(string.Empty);
    }

    private void OnNicknameInputChanged(string value)
    {
        // 입력이 바뀌면 중복 확인 초기화
        isDuplicateChecked = false;
        isDuplicateOk      = false;
        if (nicknameConfirmBtn) nicknameConfirmBtn.interactable = false;
        if (nicknameStatusText) nicknameStatusText.text = string.Empty;

        UpdateCharCount(value);
    }

    private void UpdateCharCount(string value)
    {
        if (charCountText)
            charCountText.text = $"{value.Length}/{NicknameMaxLength}";
    }

    private void OnDuplicateCheckClicked()
    {
        string nickname = nicknameInput != null ? nicknameInput.text.Trim() : string.Empty;

        if (string.IsNullOrEmpty(nickname))
        {
            SetNicknameStatus("닉네임을 입력해주세요.", Color.red);
            return;
        }

        if (nickname.Length < 2)
        {
            SetNicknameStatus("닉네임은 2자 이상이어야 합니다.", Color.red);
            return;
        }

        if (!NicknameRegex.IsMatch(nickname))
        {
            SetNicknameStatus("한글, 영문, 숫자, '_'만 사용 가능합니다.", Color.red);
            return;
        }

        Debug.Log($"[LoginManager] 중복 확인 요청: {nickname}");
        StartCoroutine(RequestDuplicateCheck(nickname));
    }

    /// <summary>
    /// 중복 확인 API 호출 구조.
    /// 실제 서버 연동 시 이 코루틴 안에서 UnityWebRequest 또는 HttpClient를 사용하세요.
    /// </summary>
    private IEnumerator RequestDuplicateCheck(string nickname)
    {
        if (duplicateCheckBtn) duplicateCheckBtn.interactable = false;
        SetNicknameStatus("확인 중...", Color.gray);

        // ── 실제 API 호출 위치 ──────────────────────────────
        // string url = $"https://your-api.com/nickname/check?name={nickname}";
        // using var req = UnityWebRequest.Get(url);
        // yield return req.SendWebRequest();
        // bool available = /* JSON 파싱 */ req.downloadHandler.text == "{\"available\":true}";
        // ────────────────────────────────────────────────────

        // 더미: 0.5초 대기 후 항상 사용 가능
        yield return new WaitForSeconds(0.5f);
        bool available = true;

        isDuplicateChecked = true;
        isDuplicateOk      = available;

        if (available)
        {
            SetNicknameStatus("사용 가능한 닉네임입니다.", new Color(0.2f, 0.8f, 0.3f));
            if (nicknameConfirmBtn) nicknameConfirmBtn.interactable = true;
        }
        else
        {
            SetNicknameStatus("이미 사용 중인 닉네임입니다.", Color.red);
            if (nicknameConfirmBtn) nicknameConfirmBtn.interactable = false;
        }

        if (duplicateCheckBtn) duplicateCheckBtn.interactable = true;
    }

    private void OnNicknameConfirmed()
    {
        if (!isDuplicateChecked || !isDuplicateOk)
        {
            SetNicknameStatus("중복 확인을 먼저 해주세요.", Color.red);
            return;
        }

        string nickname = nicknameInput != null ? nicknameInput.text.Trim() : string.Empty;
        if (string.IsNullOrEmpty(nickname)) return;

        // PlayerData 또는 PlayerPrefs에 닉네임 저장
        PlayerPrefs.SetString("PlayerNickname", nickname);
        PlayerPrefs.SetInt("IsNewUser", 0);
        PlayerPrefs.Save();

        Debug.Log($"[LoginManager] 닉네임 확정: {nickname}");
        HidePopup(nicknamePopup);
        GoToLobby();
    }

    // ── 씬 전환 ───────────────────────────────────────────────

    private void GoToLobby()
    {
        Debug.Log($"[LoginManager] 로비 씬 이동: {lobbySceneName}");
        SceneManager.LoadScene(lobbySceneName);
    }

    // ── 유틸 ──────────────────────────────────────────────────

    private void ShowPopup(GameObject popup)
    {
        if (popup != null) popup.SetActive(true);
    }

    private void HidePopup(GameObject popup)
    {
        if (popup != null) popup.SetActive(false);
    }

    private void SetNicknameStatus(string message, Color color)
    {
        if (nicknameStatusText == null) return;
        nicknameStatusText.text  = message;
        nicknameStatusText.color = color;
    }
}
