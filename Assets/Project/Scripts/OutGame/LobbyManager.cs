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
using ProjectFirst.OutGame.UI;
/// <summary>
/// Main lobby controller built on UI Toolkit.
/// Wire the PlayerData, UIDocument, optional character preview, and side systems in the inspector.
/// </summary>
[DisallowMultipleComponent]
public class LobbyManager : MonoBehaviour
{
    // Data

    [Header("Data")]
    [SerializeField] private PlayerData playerData;

    // UI

    [Header("UI")]
    [SerializeField] private UIDocument uiDocument;

    // Character

    [Header("Character")]
    [SerializeField] private Transform characterSpawnPoint;
    [SerializeField] private AgentTable agentTable;

    // Background

    [Header("Background")]
    [Tooltip("Background sprites split into 10-stage ranges. The index is selected with stageProgress / 10.")]
    [SerializeField] private Sprite[] backgroundSprites;

    // Side systems

    [Header("Side Systems")]
    [SerializeField] private IdleRewardManager idleRewardManager;
    [SerializeField] private SettingPanel settingPanel;

    // Optional events

    [Header("Events (Optional)")]
    [SerializeField] private VoidEventChannelSO onMyInfoClicked;
    [SerializeField] private VoidEventChannelSO onMailClicked;
    [SerializeField] private VoidEventChannelSO onSettingsClicked;
    [SerializeField] private VoidEventChannelSO onMissionClicked;
    [SerializeField] private VoidEventChannelSO onIdleRewardClaimed;

    // Scene names

    [Header("Scene Names")]
    [SerializeField] private string mapChapterSceneName   = "MapChapterScene";
    [SerializeField] private string characterSceneName    = "CharacterManageScene";
    [SerializeField] private string shopSceneName         = "ShopScene";
    [SerializeField] private string petSceneName          = "PetManageScene";

    // Cached UI references

    // Top bar labels
    private Label         _staminaLabel;
    private Label         _goldLabel;
    private Label         _gemLabel;

    // Top bar buttons
    private Button        _myInfoBtn;
    private Button        _mailBtn;
    private Button        _settingsBtn;
    private VisualElement _mailRedDot;

    // Currency buttons
    private Button        _staminaPlus;
    private Button        _goldPlus;
    private Button        _gemPlus;

    // Bottom navigation
    private Button        _gameStartBtn;
    private Button        _characterBtn;
    private Button        _shopBtn;
    private Button        _petBtn;

    // Shortcut menu
    private Button        _specialShopBtn;
    private Button        _agentBtn;
    private Button        _missionBtn;
    private Button        _eventBtn;
    private Button        _contractBtn;
    private VisualElement _missionRedDot;

    // Side button
    private Button        _idleRewardBtn;

    // Background
    private VisualElement _backgroundImg;

    // Spawned character preview instance
    private GameObject    _spawnedCharacter;
    private System.Action<CurrencyType> _currencyChangedHandler;

    // Lifecycle

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

    // UI binding

    private void BindUI()
    {
        if (uiDocument == null)
        {
            Debug.LogError("[LobbyManager] UIDocument is not assigned.");
            return;
        }

        var root = uiDocument.rootVisualElement;

        // Background
        _backgroundImg  = root.Q<VisualElement>("background-img");

        // Top bar labels
        _staminaLabel   = root.Q<Label>("stamina-label");
        _goldLabel      = root.Q<Label>("gold-label");
        _gemLabel       = root.Q<Label>("gem-label");

        // Top bar buttons
        _myInfoBtn      = root.Q<Button>("myinfo-btn");
        _mailBtn        = root.Q<Button>("mail-btn");
        _settingsBtn    = root.Q<Button>("settings-btn");
        _mailRedDot     = root.Q<VisualElement>("mail-reddot");

        // Currency buttons
        _staminaPlus    = root.Q<Button>("stamina-plus");
        _goldPlus       = root.Q<Button>("gold-plus");
        _gemPlus        = root.Q<Button>("gem-plus");

        // Bottom navigation
        _gameStartBtn   = root.Q<Button>("gamestart-btn");
        _characterBtn   = root.Q<Button>("character-btn");
        _shopBtn        = root.Q<Button>("shop-btn");
        _petBtn         = root.Q<Button>("pet-btn");

        // Shortcut menu
        _specialShopBtn = root.Q<Button>("special-shop-btn");
        _agentBtn       = root.Q<Button>("agent-btn");
        _missionBtn     = root.Q<Button>("mission-btn");
        _eventBtn       = root.Q<Button>("event-btn");
        _contractBtn    = root.Q<Button>("contract-btn");
        _missionRedDot  = root.Q<VisualElement>("mission-reddot");

        // Side button
        _idleRewardBtn  = root.Q<Button>("idle-reward-btn");

        // Wire button events
        _myInfoBtn?.RegisterCallback<ClickEvent>(_   => OnMyInfoClickedHandler());
        _mailBtn?.RegisterCallback<ClickEvent>(_     => OnMailClickedHandler());
        _settingsBtn?.RegisterCallback<ClickEvent>(_ => OnSettingsClickedHandler());

        _staminaPlus?.RegisterCallback<ClickEvent>(_ => Debug.Log("[LobbyManager] TODO: Open the stamina recharge popup."));
        _goldPlus?.RegisterCallback<ClickEvent>(_    => LoadScene(shopSceneName));
        _gemPlus?.RegisterCallback<ClickEvent>(_     => LoadScene(shopSceneName));

        _gameStartBtn?.RegisterCallback<ClickEvent>(_ => LoadScene(mapChapterSceneName));
        _characterBtn?.RegisterCallback<ClickEvent>(_ => LoadScene(characterSceneName));
        _shopBtn?.RegisterCallback<ClickEvent>(_      => LoadScene(shopSceneName));
        _petBtn?.RegisterCallback<ClickEvent>(_       => LoadScene(petSceneName));

        _specialShopBtn?.RegisterCallback<ClickEvent>(_ => LoadScene(shopSceneName));
        _agentBtn?.RegisterCallback<ClickEvent>(_       => LoadScene(characterSceneName));
        _missionBtn?.RegisterCallback<ClickEvent>(_     => OnMissionClickedHandler());
        _eventBtn?.RegisterCallback<ClickEvent>(_       => Debug.Log("[LobbyManager] TODO: Open the event panel."));
        _contractBtn?.RegisterCallback<ClickEvent>(_    => Debug.Log("[LobbyManager] TODO: Open the contract panel."));

        _idleRewardBtn?.RegisterCallback<ClickEvent>(_ => OnIdleRewardClickedHandler());
    }

    // Event subscriptions

    private void RegisterEvents()
    {
        if (playerData == null) return;

        if (playerData.onCurrencyChanged != null)
            playerData.onCurrencyChanged.OnEventRaised += RefreshCurrency;

        if (playerData.onCharacterChanged != null)
            playerData.onCharacterChanged.OnEventRaised += RefreshCharacter;

        // Keep compatibility with direct PlayerData currency updates.
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

    // Full refresh

    private void RefreshAll()
    {
        RefreshCurrency();
        RefreshBackground();
        RefreshCharacter();
    }

    // Currency refresh

    private void RefreshCurrency()
    {
        if (playerData == null)
        {
            Debug.LogWarning("[LobbyManager] PlayerData is not assigned.");
            return;
        }

        if (_staminaLabel != null)
            _staminaLabel.text = $"{playerData.stamina}/{playerData.staminaMax}";

        if (_goldLabel != null)
            _goldLabel.text = FormatNumber(playerData.gold);

        if (_gemLabel != null)
            _gemLabel.text = FormatNumber(playerData.gem);
    }

    // Background refresh

    private void RefreshBackground()
    {
        if (_backgroundImg == null || backgroundSprites == null || backgroundSprites.Length == 0)
            return;

        int idx = Mathf.Clamp(playerData.stageProgress / 10, 0, backgroundSprites.Length - 1);
        if (backgroundSprites[idx] != null)
            _backgroundImg.style.backgroundImage = new StyleBackground(backgroundSprites[idx]);
    }

    // Character refresh

    private void RefreshCharacter()
    {
        if (_spawnedCharacter != null)
            Destroy(_spawnedCharacter);

        if (playerData == null) return;

        if (agentTable == null)
        {
            Debug.LogWarning("[LobbyManager] AgentTable is not assigned. Character preview spawning will be skipped.");
            return;
        }

        // When AgentRow gains a prefab field, instantiate it here.
        // For now AgentRow only stores portrait data, so spawning is skipped.
        AgentRow row = agentTable.GetById(playerData.mainCharacterId);
        if (row == null)
        {
            Debug.LogWarning($"[LobbyManager] No AgentRow was found for mainCharacterId({playerData.mainCharacterId}).");
            return;
        }

        // TODO: Remove the comment below when AgentRow gets a prefab field.
        // if (row.prefab != null && characterSpawnPoint != null)
        //     _spawnedCharacter = Instantiate(row.prefab, characterSpawnPoint.position, characterSpawnPoint.rotation);
    }

    // Scene loading

    private void LoadScene(string sceneName)
    {
        if (AsyncSceneLoader.Instance != null)
            AsyncSceneLoader.Instance.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        else
            SceneManager.LoadScene(sceneName);
    }

    // Auto-resolve SettingPanel

    private void ResolveSettingPanel()
    {
        if (settingPanel != null) return;

        SettingPanel[] panels = FindObjectsOfType<SettingPanel>(true);
        if (panels != null && panels.Length > 0)
            settingPanel = panels[0];
    }

    // Button handlers

    private void OnMyInfoClickedHandler()
    {
        Debug.Log("[LobbyManager] My Info clicked.");
        onMyInfoClicked?.RaiseEvent();
    }

    private void OnMailClickedHandler()
    {
        Debug.Log("[LobbyManager] Mail clicked.");

        if (MailboxPanel.Instance != null)
            MailboxPanel.Instance.Show();
        else
            Debug.LogWarning("[LobbyManager] MailboxPanel.Instance is missing.");

        onMailClicked?.RaiseEvent();
    }

    private void OnSettingsClickedHandler()
    {
        Debug.Log("[LobbyManager] Settings clicked.");

        if (settingPanel != null)
            settingPanel.OpenPanel();
        else
            Debug.LogWarning("[LobbyManager] SettingPanel reference is missing.");

        onSettingsClicked?.RaiseEvent();
    }

    private void OnMissionClickedHandler()
    {
        Debug.Log("[LobbyManager] Mission clicked.");
        onMissionClicked?.RaiseEvent();
    }

    private void OnIdleRewardClickedHandler()
    {
        if (idleRewardManager != null)
            idleRewardManager.OpenPopup();
        else
            Debug.LogWarning("[LobbyManager] IdleRewardManager is not connected.");

        onIdleRewardClaimed?.RaiseEvent();
    }

    // Helpers

    /// <summary>Formats large numbers using K and M suffixes.</summary>
    private static string FormatNumber(long n)
    {
        if (n >= 1_000_000L) return $"{n / 1_000_000f:F1}M";
        if (n >= 1_000L)     return $"{n / 1_000f:F1}K";
        return n.ToString();
    }
}







