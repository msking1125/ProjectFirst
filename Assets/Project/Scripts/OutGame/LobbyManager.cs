using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using ProjectFirst.Data;
/// <summary>
/// 濡쒕퉬 ??硫붿씤 愿由ъ옄 ? UI Toolkit(UIDocument) 湲곕컲.
///
/// [Inspector ?곌껐 媛?대뱶]
/// ??Data
/// ?? ??playerData         : PlayerData.asset
/// ??UI
/// ?? ??uiDocument         : Scene ??UIDocument 而댄룷?뚰듃
/// ??Character
/// ?? ??characterSpawnPoint: 罹먮┃???꾨━?뱀쓣 ?몄뒪?댁뒪?뷀븷 Transform
/// ?? ??agentTable         : AgentTable.asset (mainCharacterId 猷⑹뾽??
/// ??Background
/// ?? ??backgroundSprites[]: ?ㅽ뀒?댁? 吏꾪뻾??10?④퀎 諛곌꼍 Sprite 諛곗뿴 (10媛?
/// ??Side Systems
/// ?? ??idleRewardManager  : IdleRewardManager 而댄룷?뚰듃
/// ?? ??settingPanel       : SettingPanel 而댄룷?뚰듃 (?좏깮, ?먮룞 ?먯깋 媛??
/// ??Events (Optional)
///    ??onMyInfoClicked
///    ??onMailClicked
///    ??onSettingsClicked
///    ??onMissionClicked
///    ??onIdleRewardClaimed
/// </summary>
[DisallowMultipleComponent]
public class LobbyManager : MonoBehaviour
{
    // ?? Data ??????????????????????????????????????????????????

    [Header("Data")]
    [SerializeField] private PlayerData playerData;

    // ?? UI ????????????????????????????????????????????????????

    [Header("UI")]
    [SerializeField] private UIDocument uiDocument;

    // ?? Character ?????????????????????????????????????????????

    [Header("Character")]
    [SerializeField] private Transform characterSpawnPoint;
    [SerializeField] private AgentTable agentTable;

    // ?? Background ????????????????????????????????????????????

    [Header("Background")]
    [Tooltip("?ㅽ뀒?댁? 吏꾪뻾?꾨? 10援ш컙?쇰줈 ?섎늿 諛곌꼍 Sprite (理쒕? 10媛?. " +
             "?몃뜳??= stageProgress / 10?쇰줈 ?좏깮?⑸땲??")]
    [SerializeField] private Sprite[] backgroundSprites;

    // ?? Side Systems ??????????????????????????????????????????

    [Header("Side Systems")]
    [SerializeField] private IdleRewardManager idleRewardManager;
    [SerializeField] private SettingPanel settingPanel;

    // ?? Events (Optional) ?????????????????????????????????????

    [Header("Events (Optional)")]
    [SerializeField] private VoidEventChannelSO onMyInfoClicked;
    [SerializeField] private VoidEventChannelSO onMailClicked;
    [SerializeField] private VoidEventChannelSO onSettingsClicked;
    [SerializeField] private VoidEventChannelSO onMissionClicked;
    [SerializeField] private VoidEventChannelSO onIdleRewardClaimed;

    // ?? ???대쫫 ???????????????????????????????????????????????

    [Header("Scene Names")]
    [SerializeField] private string mapChapterSceneName   = "MapChapterScene";
    [SerializeField] private string characterSceneName    = "CharacterManageScene";
    [SerializeField] private string shopSceneName         = "ShopScene";
    [SerializeField] private string petSceneName          = "PetManageScene";

    // ?? UI ?붿냼 罹먯떆 ??????????????????????????????????????????

    // Top-bar ?ы솕
    private Label         _staminaLabel;
    private Label         _goldLabel;
    private Label         _gemLabel;

    // Top-bar 踰꾪듉
    private Button        _myInfoBtn;
    private Button        _mailBtn;
    private Button        _settingsBtn;
    private VisualElement _mailRedDot;

    // ?ы솕 + 踰꾪듉
    private Button        _staminaPlus;
    private Button        _goldPlus;
    private Button        _gemPlus;

    // ?섎떒 ?ㅻ퉬
    private Button        _gameStartBtn;
    private Button        _characterBtn;
    private Button        _shopBtn;
    private Button        _petBtn;

    // ?곗륫 ?듬찓??
    private Button        _specialShopBtn;
    private Button        _agentBtn;
    private Button        _missionBtn;
    private Button        _eventBtn;
    private Button        _contractBtn;
    private VisualElement _missionRedDot;

    // 醫뚯륫 ?ъ씠??
    private Button        _idleRewardBtn;

    // 諛곌꼍
    private VisualElement _backgroundImg;

    // ?ㅽ룿??罹먮┃???몄뒪?댁뒪
    private GameObject    _spawnedCharacter;
    private System.Action<CurrencyType> _currencyChangedHandler;

    // ?????????????????????????????????????????????????????????

    private void Awake()
    {
        _currencyChangedHandler = _ => RefreshCurrency();
        ResolveSettingPanel();
    }

    private void OnEnable()
    {
        BindUI();
        RegisterEvents();
        RefreshAll();

        ProjectFirst.OutGame.TutorialManager.Instance?.TryTrigger("first_lobby");
    }

    private void OnDisable()
    {
        UnregisterEvents();
    }

    // ?? UI 諛붿씤???????????????????????????????????????????????

    private void BindUI()
    {
        if (uiDocument == null)
        {
            Debug.LogError("[LobbyManager] UIDocument媛 ?좊떦?섏? ?딆븯?듬땲??");
            return;
        }

        var root = uiDocument.rootVisualElement;

        // 諛곌꼍
        _backgroundImg  = root.Q<VisualElement>("background-img");

        // ?묐컮 ?ы솕
        _staminaLabel   = root.Q<Label>("stamina-label");
        _goldLabel      = root.Q<Label>("gold-label");
        _gemLabel       = root.Q<Label>("gem-label");

        // ?묐컮 踰꾪듉
        _myInfoBtn      = root.Q<Button>("myinfo-btn");
        _mailBtn        = root.Q<Button>("mail-btn");
        _settingsBtn    = root.Q<Button>("settings-btn");
        _mailRedDot     = root.Q<VisualElement>("mail-reddot");

        // ?ы솕 + 踰꾪듉
        _staminaPlus    = root.Q<Button>("stamina-plus");
        _goldPlus       = root.Q<Button>("gold-plus");
        _gemPlus        = root.Q<Button>("gem-plus");

        // ?섎떒 ?ㅻ퉬
        _gameStartBtn   = root.Q<Button>("gamestart-btn");
        _characterBtn   = root.Q<Button>("character-btn");
        _shopBtn        = root.Q<Button>("shop-btn");
        _petBtn         = root.Q<Button>("pet-btn");

        // ?곗륫 ?듬찓??
        _specialShopBtn = root.Q<Button>("special-shop-btn");
        _agentBtn       = root.Q<Button>("agent-btn");
        _missionBtn     = root.Q<Button>("mission-btn");
        _eventBtn       = root.Q<Button>("event-btn");
        _contractBtn    = root.Q<Button>("contract-btn");
        _missionRedDot  = root.Q<VisualElement>("mission-reddot");

        // 醫뚯륫 ?ъ씠??
        _idleRewardBtn  = root.Q<Button>("idle-reward-btn");

        // 踰꾪듉 ?대깽???곌껐
        _myInfoBtn?.RegisterCallback<ClickEvent>(_   => OnMyInfoClickedHandler());
        _mailBtn?.RegisterCallback<ClickEvent>(_     => OnMailClickedHandler());
        _settingsBtn?.RegisterCallback<ClickEvent>(_ => OnSettingsClickedHandler());

        _staminaPlus?.RegisterCallback<ClickEvent>(_ => Debug.Log("[LobbyManager] TODO: ?ㅽ깭誘몃굹 異⑹쟾 ?앹뾽"));
        _goldPlus?.RegisterCallback<ClickEvent>(_    => LoadScene(shopSceneName));
        _gemPlus?.RegisterCallback<ClickEvent>(_     => LoadScene(shopSceneName));

        _gameStartBtn?.RegisterCallback<ClickEvent>(_ => LoadScene(mapChapterSceneName));
        _characterBtn?.RegisterCallback<ClickEvent>(_ => LoadScene(characterSceneName));
        _shopBtn?.RegisterCallback<ClickEvent>(_      => LoadScene(shopSceneName));
        _petBtn?.RegisterCallback<ClickEvent>(_       => LoadScene(petSceneName));

        _specialShopBtn?.RegisterCallback<ClickEvent>(_ => LoadScene(shopSceneName));
        _agentBtn?.RegisterCallback<ClickEvent>(_       => LoadScene(characterSceneName));
        _missionBtn?.RegisterCallback<ClickEvent>(_     => OnMissionClickedHandler());
        _eventBtn?.RegisterCallback<ClickEvent>(_       => Debug.Log("[LobbyManager] TODO: ?대깽???⑤꼸"));
        _contractBtn?.RegisterCallback<ClickEvent>(_    => Debug.Log("[LobbyManager] TODO: 怨꾩빟 ?⑤꼸"));

        _idleRewardBtn?.RegisterCallback<ClickEvent>(_ => OnIdleRewardClickedHandler());
    }

    // ?? ?대깽??梨꾨꼸 援щ룆 ??????????????????????????????????????

    private void RegisterEvents()
    {
        if (playerData == null) return;

        if (playerData.onCurrencyChanged != null)
            playerData.onCurrencyChanged.OnEventRaised += RefreshCurrency;

        if (playerData.onCharacterChanged != null)
            playerData.onCharacterChanged.OnEventRaised += RefreshCharacter;

        // ?덇굅???대깽?몃룄 援щ룆 (PlayerData瑜?吏곸젒 int濡??섏젙?섎뒗 湲곗〈 肄붾뱶 ?명솚)
        playerData.OnCurrencyChanged += _currencyChangedHandler;
    }

    private void UnregisterEvents()
    {
        if (playerData == null) return;

        if (playerData.onCurrencyChanged != null)
            playerData.onCurrencyChanged.OnEventRaised -= RefreshCurrency;

        if (playerData.onCharacterChanged != null)
            playerData.onCharacterChanged.OnEventRaised -= RefreshCharacter;

        playerData.OnCurrencyChanged -= _currencyChangedHandler;
    }

    // ?? ?꾩껜 媛깆떊 ?????????????????????????????????????????????

    private void RefreshAll()
    {
        RefreshCurrency();
        RefreshBackground();
        RefreshCharacter();
    }

    // ?? ?ы솕 UI 媛깆떊 ?????????????????????????????????????????

    private void RefreshCurrency()
    {
        if (playerData == null)
        {
            Debug.LogWarning("[LobbyManager] PlayerData媛 ?좊떦?섏? ?딆븯?듬땲??");
            return;
        }

        if (_staminaLabel != null)
            _staminaLabel.text = $"{playerData.stamina}/{playerData.staminaMax}";

        if (_goldLabel != null)
            _goldLabel.text = FormatNumber(playerData.gold);

        if (_gemLabel != null)
            _gemLabel.text = FormatNumber(playerData.gem);
    }

    // ?? 諛곌꼍 媛깆떊 ?????????????????????????????????????????????

    private void RefreshBackground()
    {
        if (_backgroundImg == null || backgroundSprites == null || backgroundSprites.Length == 0)
            return;

        int idx = Mathf.Clamp(playerData.stageProgress / 10, 0, backgroundSprites.Length - 1);
        if (backgroundSprites[idx] != null)
            _backgroundImg.style.backgroundImage = new StyleBackground(backgroundSprites[idx]);
    }

    // ?? 罹먮┃??媛깆떊 ???????????????????????????????????????????

    private void RefreshCharacter()
    {
        if (_spawnedCharacter != null)
            Destroy(_spawnedCharacter);

        if (playerData == null) return;

        if (agentTable == null)
        {
            Debug.LogWarning("[LobbyManager] AgentTable???좊떦?섏? ?딆븯?듬땲?? 罹먮┃???ㅽ룿??嫄대꼫?곷땲??");
            return;
        }

        // AgentRow???꾨━???꾨뱶媛 異붽??섎㈃ ?ш린??Instantiate 泥섎━
        // ?꾩옱 AgentRow???꾪닾 ?ㅽ꺈留?蹂댁쑀?섎?濡??ㅽ룿 ?앸왂
        AgentRow row = agentTable.GetById(playerData.mainCharacterId);
        if (row == null)
        {
            Debug.LogWarning($"[LobbyManager] mainCharacterId({playerData.mainCharacterId})???대떦?섎뒗 AgentRow瑜?李얠쓣 ???놁뒿?덈떎.");
            return;
        }

        // TODO: AgentRow??prefab ?꾨뱶 異붽? ???꾨옒 二쇱꽍 ?댁젣
        // if (row.prefab != null && characterSpawnPoint != null)
        //     _spawnedCharacter = Instantiate(row.prefab, characterSpawnPoint.position, characterSpawnPoint.rotation);
    }

    // ?? ???대룞 ???????????????????????????????????????????????

    private void LoadScene(string sceneName)
    {
        if (AsyncSceneLoader.Instance != null)
            AsyncSceneLoader.Instance.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        else
            SceneManager.LoadScene(sceneName);
    }

    // ?? SettingPanel ?먮룞 ?먯깋 ????????????????????????????????

    private void ResolveSettingPanel()
    {
        if (settingPanel != null) return;

        SettingPanel[] panels = FindObjectsOfType<SettingPanel>(true);
        if (panels != null && panels.Length > 0)
            settingPanel = panels[0];
    }

    // ?? 踰꾪듉 ?몃뱾?????????????????????????????????????????????

    private void OnMyInfoClickedHandler()
    {
        Debug.Log("[LobbyManager] ???뺣낫 ?대┃");
        onMyInfoClicked?.RaiseEvent();
    }

    private void OnMailClickedHandler()
    {
        Debug.Log("[LobbyManager] ?고렪 ?대┃");

        if (MailboxPanel.Instance != null)
            MailboxPanel.Instance.Show();
        else
            Debug.LogWarning("[LobbyManager] MailboxPanel.Instance媛 ?놁뒿?덈떎.");

        onMailClicked?.RaiseEvent();
    }

    private void OnSettingsClickedHandler()
    {
        Debug.Log("[LobbyManager] ?ㅼ젙 ?대┃");

        if (settingPanel != null)
            settingPanel.OpenPanel();
        else
            Debug.LogWarning("[LobbyManager] SettingPanel 李몄“媛 ?놁뒿?덈떎.");

        onSettingsClicked?.RaiseEvent();
    }

    private void OnMissionClickedHandler()
    {
        Debug.Log("[LobbyManager] 誘몄뀡 ?대┃");
        onMissionClicked?.RaiseEvent();
    }

    private void OnIdleRewardClickedHandler()
    {
        if (idleRewardManager != null)
            idleRewardManager.OpenPopup();
        else
            Debug.LogWarning("[LobbyManager] IdleRewardManager媛 ?곌껐?섏? ?딆븯?듬땲??");

        onIdleRewardClaimed?.RaiseEvent();
    }

    // ?? ?좏떥 ?????????????????????????????????????????????????

    /// <summary>???レ옄瑜?K / M ?⑥쐞濡?以꾩뿬??諛섑솚?⑸땲??</summary>
    private static string FormatNumber(long n)
    {
        if (n >= 1_000_000L) return $"{n / 1_000_000f:F1}M";
        if (n >= 1_000L)     return $"{n / 1_000f:F1}K";
        return n.ToString();
    }
}





