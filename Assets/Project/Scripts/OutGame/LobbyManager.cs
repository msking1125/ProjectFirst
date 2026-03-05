using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// 로비 씬 메인 관리자.
///
/// [Inspector 연결 가이드]
/// ┌ Data
/// │  └ playerData          : PlayerData.asset
/// ├ Center
/// │  ├ backgroundImage     : 배경 전체를 채우는 Image
/// │  ├ characterImage      : 중앙 캐릭터 스프라이트 Image
/// │  ├ stageBackgrounds[]  : 스테이지 인덱스 순 배경 Sprite 배열
/// │  └ characterSprites[]  : 캐릭터 인덱스 순 캐릭터 Sprite 배열
/// ├ Top Bar
/// │  ├ myInfoButton        : 좌상단 '내 정보' 버튼
/// │  ├ ticketText          : 티켓 수치 TMP
/// │  ├ goldText            : 골드 수치 TMP
/// │  ├ diamondText         : 다이아 수치 TMP
/// │  ├ ticketPlusButton    : 티켓 '+' 버튼
/// │  ├ goldPlusButton      : 골드 '+' 버튼
/// │  ├ diamondPlusButton   : 다이아 '+' 버튼
/// │  ├ mailButton          : 우편 버튼
/// │  └ settingsButton      : 설정 버튼
/// ├ Bottom Navigation
/// │  ├ enterGameButton     : 게임 진입 버튼
/// │  ├ characterManageButton: 캐릭터 관리 버튼
/// │  ├ shopButton          : 상점 버튼
/// │  └ petManageButton     : 펫 관리 버튼
/// └ Side Buttons
///    ├ missionButton       : 미션 버튼 (우측)
///    └ idleRewardButton    : 방치 보상 버튼 (우측)
/// </summary>
[DisallowMultipleComponent]
public class LobbyManager : MonoBehaviour
{
    // ── Data ──────────────────────────────────────────────────

    [Header("Data")]
    [SerializeField] private PlayerData playerData;

    // ── 중앙 영역 ─────────────────────────────────────────────

    [Header("Center")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image characterImage;

    [Tooltip("스테이지 인덱스 순서대로 배경 Sprite를 등록하세요 (PlayerData.currentStageIndex 기준으로 선택됩니다).")]
    [SerializeField] private Sprite[] stageBackgrounds;

    [Tooltip("캐릭터 인덱스 순서대로 캐릭터 Sprite를 등록하세요 (PlayerData.currentAgentIndex 기준으로 선택됩니다).")]
    [SerializeField] private Sprite[] characterSprites;

    // ── 탑바 ─────────────────────────────────────────────────

    [Header("Top Bar")]
    [SerializeField] private Button myInfoButton;

    [SerializeField] private TMP_Text ticketText;
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private TMP_Text diamondText;

    [SerializeField] private Button ticketPlusButton;
    [SerializeField] private Button goldPlusButton;
    [SerializeField] private Button diamondPlusButton;

    [SerializeField] private Button mailButton;
    [SerializeField] private Button settingsButton;

    // ── 하단 네비 ─────────────────────────────────────────────

    [Header("Bottom Navigation")]
    [SerializeField] private Button enterGameButton;
    [SerializeField] private Button characterManageButton;
    [SerializeField] private Button shopButton;
    [SerializeField] private Button petManageButton;

    // ── 우측 사이드 ───────────────────────────────────────────

    [Header("Side Buttons")]
    [SerializeField] private Button missionButton;
    [SerializeField] private Button idleRewardButton;

    // ── 씬 이름 ───────────────────────────────────────────────

    [Header("Scene Names")]
    [SerializeField] private string battleSceneName = "Battle_Test";

    // ── 이벤트 채널 ───────────────────────────────────────────

    [Header("Events (Optional)")]
    [SerializeField] private VoidEventChannelSO onMyInfoClicked;
    [SerializeField] private VoidEventChannelSO onMailClicked;
    [SerializeField] private VoidEventChannelSO onSettingsClicked;
    [SerializeField] private VoidEventChannelSO onMissionClicked;
    [SerializeField] private VoidEventChannelSO onIdleRewardClaimed;

    // ─────────────────────────────────────────────────────────

    private void Awake()
    {
        BindButtons();
    }

    private void OnEnable()
    {
        if (playerData != null)
            playerData.OnCurrencyChanged += RefreshCurrencyUI;
    }

    private void OnDisable()
    {
        if (playerData != null)
            playerData.OnCurrencyChanged -= RefreshCurrencyUI;
    }

    private void Start()
    {
        RefreshCurrencyUI();
        RefreshCenterUI();
    }

    // ── UI 갱신 ───────────────────────────────────────────────

    private void RefreshCurrencyUI()
    {
        if (playerData == null)
        {
            Debug.LogWarning("[LobbyManager] PlayerData가 할당되지 않았습니다.");
            return;
        }

        if (ticketText  != null) ticketText.text  = FormatNumber(playerData.ticket);
        if (goldText    != null) goldText.text    = FormatNumber(playerData.gold);
        if (diamondText != null) diamondText.text = FormatNumber(playerData.diamond);
    }

    private void RefreshCenterUI()
    {
        if (playerData == null) return;

        if (backgroundImage != null && stageBackgrounds != null && stageBackgrounds.Length > 0)
        {
            int idx = Mathf.Clamp(playerData.currentStageIndex, 0, stageBackgrounds.Length - 1);
            backgroundImage.sprite = stageBackgrounds[idx];
        }

        if (characterImage != null && characterSprites != null && characterSprites.Length > 0)
        {
            int idx = Mathf.Clamp(playerData.currentAgentIndex, 0, characterSprites.Length - 1);
            characterImage.sprite = characterSprites[idx];
        }
    }

    // ── 버튼 바인딩 ───────────────────────────────────────────

    private void BindButtons()
    {
        // 탑바
        myInfoButton?.onClick.AddListener(OnMyInfoClicked);
        ticketPlusButton?.onClick.AddListener(() => OnCurrencyPlusClicked(CurrencyType.Ticket));
        goldPlusButton?.onClick.AddListener(() => OnCurrencyPlusClicked(CurrencyType.Gold));
        diamondPlusButton?.onClick.AddListener(() => OnCurrencyPlusClicked(CurrencyType.Diamond));
        mailButton?.onClick.AddListener(OnMailClicked);
        settingsButton?.onClick.AddListener(OnSettingsClicked);

        // 하단 네비
        enterGameButton?.onClick.AddListener(OnEnterGameClicked);
        characterManageButton?.onClick.AddListener(OnCharacterManageClicked);
        shopButton?.onClick.AddListener(OnShopClicked);
        petManageButton?.onClick.AddListener(OnPetManageClicked);

        // 우측 사이드
        missionButton?.onClick.AddListener(OnMissionClicked);
        idleRewardButton?.onClick.AddListener(OnIdleRewardClicked);
    }

    // ── 버튼 핸들러: 탑바 ────────────────────────────────────

    private void OnMyInfoClicked()
    {
        Debug.Log("[LobbyManager] 내 정보 클릭");
        onMyInfoClicked?.RaiseEvent();
    }

    private void OnCurrencyPlusClicked(CurrencyType type)
    {
        // 각 재화별 상점/충전 패널로 연동 예정
        Debug.Log($"[LobbyManager] {type} + 버튼 클릭");
    }

    private void OnMailClicked()
    {
        Debug.Log("[LobbyManager] 우편 클릭");
        onMailClicked?.RaiseEvent();
    }

    private void OnSettingsClicked()
    {
        Debug.Log("[LobbyManager] 설정 클릭");
        onSettingsClicked?.RaiseEvent();
    }

    // ── 버튼 핸들러: 하단 네비 ───────────────────────────────

    private void OnEnterGameClicked()
    {
        Debug.Log($"[LobbyManager] 게임 진입 → {battleSceneName}");

        if (AsyncSceneLoader.Instance != null)
            AsyncSceneLoader.Instance.LoadSceneAsync(battleSceneName, LoadSceneMode.Single);
        else
            SceneManager.LoadScene(battleSceneName);
    }

    private void OnCharacterManageClicked()
    {
        Debug.Log("[LobbyManager] 캐릭터 관리 클릭");
        // TODO: 캐릭터 관리 패널 열기
    }

    private void OnShopClicked()
    {
        Debug.Log("[LobbyManager] 상점 클릭");
        // TODO: 상점 패널 열기
    }

    private void OnPetManageClicked()
    {
        Debug.Log("[LobbyManager] 펫 관리 클릭");
        // TODO: 펫 관리 패널 열기
    }

    // ── 버튼 핸들러: 우측 사이드 ─────────────────────────────

    private void OnMissionClicked()
    {
        Debug.Log("[LobbyManager] 미션 클릭");
        onMissionClicked?.RaiseEvent();
    }

    private void OnIdleRewardClicked()
    {
        if (playerData == null) return;

        System.TimeSpan elapsed = playerData.GetIdleElapsed();
        Debug.Log($"[LobbyManager] 방치 보상 클릭 — 경과 시간: {elapsed.TotalMinutes:F1}분");

        // TODO: elapsed 기반으로 보상 계산 후 지급
        playerData.MarkIdleRewardClaimed();
        onIdleRewardClaimed?.RaiseEvent();
    }

    // ── 유틸 ─────────────────────────────────────────────────

    /// <summary>큰 숫자를 K / M 단위로 줄여서 반환합니다.</summary>
    private static string FormatNumber(int value)
    {
        if (value >= 1_000_000) return $"{value / 1_000_000f:F1}M";
        if (value >= 1_000)     return $"{value / 1_000f:F1}K";
        return value.ToString();
    }

    private enum CurrencyType { Ticket, Gold, Diamond }
}
