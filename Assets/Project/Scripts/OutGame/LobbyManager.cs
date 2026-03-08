using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

/// <summary>
/// ·Îºñ ¾À ¸ÞÀÎ °ü¸®ÀÚ ? UI Toolkit(UIDocument) ±â¹Ý.
///
/// [Inspector ¿¬°á °¡ÀÌµå]
/// ¦£ Data
/// ¦¢  ¦¦ playerData         : PlayerData.asset
/// ¦§ UI
/// ¦¢  ¦¦ uiDocument         : Scene ÀÇ UIDocument ÄÄÆ÷³ÍÆ®
/// ¦§ Character
/// ¦¢  ¦§ characterSpawnPoint: Ä³¸¯ÅÍ ÇÁ¸®ÆÕÀ» ÀÎ½ºÅÏ½ºÈ­ÇÒ Transform
/// ¦¢  ¦¦ agentTable         : AgentTable.asset (mainCharacterId ·è¾÷¿ë)
/// ¦§ Background
/// ¦¢  ¦¦ backgroundSprites[]: ½ºÅ×ÀÌÁö ÁøÇàµµ 10´Ü°è ¹è°æ Sprite ¹è¿­ (10°³)
/// ¦§ Side Systems
/// ¦¢  ¦§ idleRewardManager  : IdleRewardManager ÄÄÆ÷³ÍÆ®
/// ¦¢  ¦¦ settingPanel       : SettingPanel ÄÄÆ÷³ÍÆ® (¼±ÅÃ, ÀÚµ¿ Å½»ö °¡´É)
/// ¦¦ Events (Optional)
///    ¦§ onMyInfoClicked
///    ¦§ onMailClicked
///    ¦§ onSettingsClicked
///    ¦§ onMissionClicked
///    ¦¦ onIdleRewardClaimed
/// </summary>
[DisallowMultipleComponent]
public class LobbyManager : MonoBehaviour
{
    // ¦¡¦¡ Data ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡

    [Header("Data")]
    [SerializeField] private PlayerData playerData;

    // ¦¡¦¡ UI ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡

    [Header("UI")]
    [SerializeField] private UIDocument uiDocument;

    // ¦¡¦¡ Character ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡

    [Header("Character")]
    [SerializeField] private Transform characterSpawnPoint;
    [SerializeField] private AgentTable agentTable;

    // ¦¡¦¡ Background ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡

    [Header("Background")]
    [Tooltip("½ºÅ×ÀÌÁö ÁøÇàµµ¸¦ 10±¸°£À¸·Î ³ª´« ¹è°æ Sprite (ÃÖ´ë 10°³). " +
             "ÀÎµ¦½º = stageProgress / 10À¸·Î ¼±ÅÃµË´Ï´Ù.")]
    [SerializeField] private Sprite[] backgroundSprites;

    // ¦¡¦¡ Side Systems ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡

    [Header("Side Systems")]
    [SerializeField] private IdleRewardManager idleRewardManager;
    [SerializeField] private SettingPanel settingPanel;

    // ¦¡¦¡ Events (Optional) ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡

    [Header("Events (Optional)")]
    [SerializeField] private VoidEventChannelSO onMyInfoClicked;
    [SerializeField] private VoidEventChannelSO onMailClicked;
    [SerializeField] private VoidEventChannelSO onSettingsClicked;
    [SerializeField] private VoidEventChannelSO onMissionClicked;
    [SerializeField] private VoidEventChannelSO onIdleRewardClaimed;

    // ¦¡¦¡ ¾À ÀÌ¸§ ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡

    [Header("Scene Names")]
    [SerializeField] private string mapChapterSceneName   = "MapChapterScene";
    [SerializeField] private string characterSceneName    = "CharacterManageScene";
    [SerializeField] private string shopSceneName         = "ShopScene";
    [SerializeField] private string petSceneName          = "PetManageScene";

    // ¦¡¦¡ UI ¿ä¼Ò Ä³½Ã ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡

    // Top-bar ÀçÈ­
    private Label         _staminaLabel;
    private Label         _goldLabel;
    private Label         _gemLabel;

    // Top-bar ¹öÆ°
    private Button        _myInfoBtn;
    private Button        _mailBtn;
    private Button        _settingsBtn;
    private VisualElement _mailRedDot;

    // ÀçÈ­ + ¹öÆ°
    private Button        _staminaPlus;
    private Button        _goldPlus;
    private Button        _gemPlus;

    // ÇÏ´Ü ³×ºñ
    private Button        _gameStartBtn;
    private Button        _characterBtn;
    private Button        _shopBtn;
    private Button        _petBtn;

    // ¿ìÃø Äü¸Þ´º
    private Button        _specialShopBtn;
    private Button        _agentBtn;
    private Button        _missionBtn;
    private Button        _eventBtn;
    private Button        _contractBtn;
    private VisualElement _missionRedDot;

    // ÁÂÃø »çÀÌµå
    private Button        _idleRewardBtn;

    // ¹è°æ
    private VisualElement _backgroundImg;

    // ½ºÆùµÈ Ä³¸¯ÅÍ ÀÎ½ºÅÏ½º
    private GameObject    _spawnedCharacter;

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡

    private void Awake()
    {
        ResolveSettingPanel();
    }

    private void OnEnable()
    {
        BindUI();
        RegisterEvents();
        RefreshAll();
    }

    private void OnDisable()
    {
        UnregisterEvents();
    }

    // ¦¡¦¡ UI ¹ÙÀÎµù ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡

    private void BindUI()
    {
        if (uiDocument == null)
        {
            Debug.LogError("[LobbyManager] UIDocument°¡ ÇÒ´çµÇÁö ¾Ê¾Ò½À´Ï´Ù.");
            return;
        }

        var root = uiDocument.rootVisualElement;

        // ¹è°æ
        _backgroundImg  = root.Q<VisualElement>("background-img");

        // Å¾¹Ù ÀçÈ­
        _staminaLabel   = root.Q<Label>("stamina-label");
        _goldLabel      = root.Q<Label>("gold-label");
        _gemLabel       = root.Q<Label>("gem-label");

        // Å¾¹Ù ¹öÆ°
        _myInfoBtn      = root.Q<Button>("myinfo-btn");
        _mailBtn        = root.Q<Button>("mail-btn");
        _settingsBtn    = root.Q<Button>("settings-btn");
        _mailRedDot     = root.Q<VisualElement>("mail-reddot");

        // ÀçÈ­ + ¹öÆ°
        _staminaPlus    = root.Q<Button>("stamina-plus");
        _goldPlus       = root.Q<Button>("gold-plus");
        _gemPlus        = root.Q<Button>("gem-plus");

        // ÇÏ´Ü ³×ºñ
        _gameStartBtn   = root.Q<Button>("gamestart-btn");
        _characterBtn   = root.Q<Button>("character-btn");
        _shopBtn        = root.Q<Button>("shop-btn");
        _petBtn         = root.Q<Button>("pet-btn");

        // ¿ìÃø Äü¸Þ´º
        _specialShopBtn = root.Q<Button>("special-shop-btn");
        _agentBtn       = root.Q<Button>("agent-btn");
        _missionBtn     = root.Q<Button>("mission-btn");
        _eventBtn       = root.Q<Button>("event-btn");
        _contractBtn    = root.Q<Button>("contract-btn");
        _missionRedDot  = root.Q<VisualElement>("mission-reddot");

        // ÁÂÃø »çÀÌµå
        _idleRewardBtn  = root.Q<Button>("idle-reward-btn");

        // ¹öÆ° ÀÌº¥Æ® ¿¬°á
        _myInfoBtn?.RegisterCallback<ClickEvent>(_   => OnMyInfoClickedHandler());
        _mailBtn?.RegisterCallback<ClickEvent>(_     => OnMailClickedHandler());
        _settingsBtn?.RegisterCallback<ClickEvent>(_ => OnSettingsClickedHandler());

        _staminaPlus?.RegisterCallback<ClickEvent>(_ => Debug.Log("[LobbyManager] TODO: ½ºÅÂ¹Ì³ª ÃæÀü ÆË¾÷"));
        _goldPlus?.RegisterCallback<ClickEvent>(_    => LoadScene(shopSceneName));
        _gemPlus?.RegisterCallback<ClickEvent>(_     => LoadScene(shopSceneName));

        _gameStartBtn?.RegisterCallback<ClickEvent>(_ => LoadScene(mapChapterSceneName));
        _characterBtn?.RegisterCallback<ClickEvent>(_ => LoadScene(characterSceneName));
        _shopBtn?.RegisterCallback<ClickEvent>(_      => LoadScene(shopSceneName));
        _petBtn?.RegisterCallback<ClickEvent>(_       => LoadScene(petSceneName));

        _specialShopBtn?.RegisterCallback<ClickEvent>(_ => LoadScene(shopSceneName));
        _agentBtn?.RegisterCallback<ClickEvent>(_       => LoadScene(characterSceneName));
        _missionBtn?.RegisterCallback<ClickEvent>(_     => OnMissionClickedHandler());
        _eventBtn?.RegisterCallback<ClickEvent>(_       => Debug.Log("[LobbyManager] TODO: ÀÌº¥Æ® ÆÐ³Î"));
        _contractBtn?.RegisterCallback<ClickEvent>(_    => Debug.Log("[LobbyManager] TODO: °è¾à ÆÐ³Î"));

        _idleRewardBtn?.RegisterCallback<ClickEvent>(_ => OnIdleRewardClickedHandler());
    }

    // ¦¡¦¡ ÀÌº¥Æ® Ã¤³Î ±¸µ¶ ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡

    private void RegisterEvents()
    {
        if (playerData == null) return;

        if (playerData.onCurrencyChanged != null)
            playerData.onCurrencyChanged.OnEventRaised += RefreshCurrency;

        if (playerData.onCharacterChanged != null)
            playerData.onCharacterChanged.OnEventRaised += RefreshCharacter;

        // ·¹°Å½Ã ÀÌº¥Æ®µµ ±¸µ¶ (PlayerData¸¦ Á÷Á¢ int·Î ¼öÁ¤ÇÏ´Â ±âÁ¸ ÄÚµå È£È¯)
        playerData.OnCurrencyChanged += _ => RefreshCurrency();
    }

    private void UnregisterEvents()
    {
        if (playerData == null) return;

        if (playerData.onCurrencyChanged != null)
            playerData.onCurrencyChanged.OnEventRaised -= RefreshCurrency;

        if (playerData.onCharacterChanged != null)
            playerData.onCharacterChanged.OnEventRaised -= RefreshCharacter;

        playerData.OnCurrencyChanged -= _ => RefreshCurrency();
    }

    // ¦¡¦¡ ÀüÃ¼ °»½Å ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡

    private void RefreshAll()
    {
        RefreshCurrency();
        RefreshBackground();
        RefreshCharacter();
    }

    // ¦¡¦¡ ÀçÈ­ UI °»½Å ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡

    private void RefreshCurrency()
    {
        if (playerData == null)
        {
            Debug.LogWarning("[LobbyManager] PlayerData°¡ ÇÒ´çµÇÁö ¾Ê¾Ò½À´Ï´Ù.");
            return;
        }

        if (_staminaLabel != null)
            _staminaLabel.text = $"{playerData.stamina}/{playerData.staminaMax}";

        if (_goldLabel != null)
            _goldLabel.text = FormatNumber(playerData.gold);

        if (_gemLabel != null)
            _gemLabel.text = FormatNumber(playerData.gem);
    }

    // ¦¡¦¡ ¹è°æ °»½Å ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡

    private void RefreshBackground()
    {
        if (_backgroundImg == null || backgroundSprites == null || backgroundSprites.Length == 0)
            return;

        int idx = Mathf.Clamp(playerData.stageProgress / 10, 0, backgroundSprites.Length - 1);
        if (backgroundSprites[idx] != null)
            _backgroundImg.style.backgroundImage = new StyleBackground(backgroundSprites[idx]);
    }

    // ¦¡¦¡ Ä³¸¯ÅÍ °»½Å ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡

    private void RefreshCharacter()
    {
        if (_spawnedCharacter != null)
            Destroy(_spawnedCharacter);

        if (playerData == null) return;

        if (agentTable == null)
        {
            Debug.LogWarning("[LobbyManager] AgentTableÀÌ ÇÒ´çµÇÁö ¾Ê¾Ò½À´Ï´Ù. Ä³¸¯ÅÍ ½ºÆùÀ» °Ç³Ê¶Ý´Ï´Ù.");
            return;
        }

        // AgentRow¿¡ ÇÁ¸®ÆÕ ÇÊµå°¡ Ãß°¡µÇ¸é ¿©±â¼­ Instantiate Ã³¸®
        // ÇöÀç AgentRow´Â ÀüÅõ ½ºÅÈ¸¸ º¸À¯ÇÏ¹Ç·Î ½ºÆù »ý·«
        AgentRow row = agentTable.GetById(playerData.mainCharacterId.ToString());
        if (row == null)
        {
            Debug.LogWarning($"[LobbyManager] mainCharacterId({playerData.mainCharacterId})¿¡ ÇØ´çÇÏ´Â AgentRow¸¦ Ã£À» ¼ö ¾ø½À´Ï´Ù.");
            return;
        }

        // TODO: AgentRow¿¡ prefab ÇÊµå Ãß°¡ ÈÄ ¾Æ·¡ ÁÖ¼® ÇØÁ¦
        // if (row.prefab != null && characterSpawnPoint != null)
        //     _spawnedCharacter = Instantiate(row.prefab, characterSpawnPoint.position, characterSpawnPoint.rotation);
    }

    // ¦¡¦¡ ¾À ÀÌµ¿ ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡

    private void LoadScene(string sceneName)
    {
        if (AsyncSceneLoader.Instance != null)
            AsyncSceneLoader.Instance.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        else
            SceneManager.LoadScene(sceneName);
    }

    // ¦¡¦¡ SettingPanel ÀÚµ¿ Å½»ö ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡

    private void ResolveSettingPanel()
    {
        if (settingPanel != null) return;

        SettingPanel[] panels = FindObjectsOfType<SettingPanel>(true);
        if (panels != null && panels.Length > 0)
            settingPanel = panels[0];
    }

    // ¦¡¦¡ ¹öÆ° ÇÚµé·¯ ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡

    private void OnMyInfoClickedHandler()
    {
        Debug.Log("[LobbyManager] ³» Á¤º¸ Å¬¸¯");
        onMyInfoClicked?.RaiseEvent();
    }

    private void OnMailClickedHandler()
    {
        Debug.Log("[LobbyManager] ¿ìÆí Å¬¸¯");

        if (MailboxPanel.Instance != null)
            MailboxPanel.Instance.Show();
        else
            Debug.LogWarning("[LobbyManager] MailboxPanel.Instance°¡ ¾ø½À´Ï´Ù.");

        onMailClicked?.RaiseEvent();
    }

    private void OnSettingsClickedHandler()
    {
        Debug.Log("[LobbyManager] ¼³Á¤ Å¬¸¯");

        if (settingPanel != null)
            settingPanel.OpenPanel();
        else
            Debug.LogWarning("[LobbyManager] SettingPanel ÂüÁ¶°¡ ¾ø½À´Ï´Ù.");

        onSettingsClicked?.RaiseEvent();
    }

    private void OnMissionClickedHandler()
    {
        Debug.Log("[LobbyManager] ¹Ì¼Ç Å¬¸¯");
        onMissionClicked?.RaiseEvent();
    }

    private void OnIdleRewardClickedHandler()
    {
        if (idleRewardManager != null)
            idleRewardManager.OpenPopup();
        else
            Debug.LogWarning("[LobbyManager] IdleRewardManager°¡ ¿¬°áµÇÁö ¾Ê¾Ò½À´Ï´Ù.");

        onIdleRewardClaimed?.RaiseEvent();
    }

    // ¦¡¦¡ À¯Æ¿ ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡

    /// <summary>Å« ¼ýÀÚ¸¦ K / M ´ÜÀ§·Î ÁÙ¿©¼­ ¹ÝÈ¯ÇÕ´Ï´Ù.</summary>
    private static string FormatNumber(long n)
    {
        if (n >= 1_000_000L) return $"{n / 1_000_000f:F1}M";
        if (n >= 1_000L)     return $"{n / 1_000f:F1}K";
        return n.ToString();
    }
}
