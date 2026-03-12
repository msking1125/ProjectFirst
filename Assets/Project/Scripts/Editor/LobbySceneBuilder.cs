#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using ProjectFirst.Data;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// 로비 씬 자동 빌더.
/// 메뉴: Tools → Game → Build Lobby Scene
///
/// 실행하면:
///  1. Assets/Project/Scenes/Lobby.unity 씬을 생성 (기존 파일 있으면 덮어쓰기 여부 확인)
///  2. Canvas / EventSystem / 전체 UI 계층 생성
///  3. LobbyManager 컴포넌트를 추가하고 모든 레퍼런스를 자동 연결
///  4. PlayerData.asset 이 없으면 Assets/Project/Data/ 에 생성
///  5. 씬 저장
/// </summary>
public static class LobbySceneBuilder
{
    private const string ScenePath          = "Assets/Project/Scenes/Lobby.unity";
    private const string PlayerDataPath     = "Assets/Project/Data/PlayerData.asset";
    private const string IdleConfigPath     = "Assets/Project/Data/IdleRewardConfig.asset";
    private const string MailBoxPath        = "Assets/Project/Data/MailBox.asset";

    // 레퍼런스 해상도 (9:16 모바일)
    private static readonly Vector2 RefResolution = new Vector2(1080f, 1920f);

    // ── 팔레트 ──────────────────────────────────────────────
    private static readonly Color ColTopBar      = new Color(0.08f, 0.08f, 0.12f, 0.92f);
    private static readonly Color ColBottomBar   = new Color(0.08f, 0.08f, 0.12f, 0.95f);
    private static readonly Color ColSideBtn     = new Color(0.15f, 0.55f, 0.85f, 0.90f);
    private static readonly Color ColNavBtn      = new Color(0.18f, 0.18f, 0.22f, 0.95f);
    private static readonly Color ColNavActive   = new Color(0.20f, 0.60f, 0.95f, 1.00f);
    private static readonly Color ColPlusBtn     = new Color(0.30f, 0.75f, 0.40f, 1.00f);
    private static readonly Color ColCurrencyBg  = new Color(0.05f, 0.05f, 0.08f, 0.85f);

    // ── 진입점 ───────────────────────────────────────────────

    [MenuItem("Tools/Game/Build Lobby Scene")]
    public static void Build()
    {
        // 저장되지 않은 씬 변경사항 확인
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        // 기존 Lobby 씬 파일 존재 시 덮어쓰기 확인
        if (System.IO.File.Exists(ScenePath))
        {
            bool overwrite = EditorUtility.DisplayDialog(
                "Lobby 씬 빌더",
                $"{ScenePath} 이(가) 이미 존재합니다.\n덮어쓰시겠습니까?",
                "덮어쓰기", "취소");

            if (!overwrite) return;
        }

        // 새 씬 생성
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // 에셋 준비
        PlayerData playerData       = EnsurePlayerData();
        IdleRewardConfig idleConfig = EnsureIdleRewardConfig();
        MailBox mailBox             = EnsureMailBox();

        // UI 계층 빌드
        var refs = BuildSceneHierarchy();

        // LobbyManager 배치 및 연결
        WireLobbyManager(refs, playerData);

        // IdleRewardManager 배치 및 연결
        WireIdleRewardManager(refs, playerData, idleConfig, mailBox);

        // 씬 저장
        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.Refresh();

        Debug.Log($"[LobbySceneBuilder] 씬 빌드 완료 → {ScenePath}");
        EditorUtility.DisplayDialog("완료", $"Lobby 씬이 생성되었습니다.\n{ScenePath}", "확인");
    }

    // ── PlayerData 에셋 ───────────────────────────────────────

    private static PlayerData EnsurePlayerData()
    {
        PlayerData data = AssetDatabase.LoadAssetAtPath<PlayerData>(PlayerDataPath);
        if (data == null)
        {
            data = ScriptableObject.CreateInstance<PlayerData>();
            data.ticket  = 10;
            data.gold    = 1000;
            data.diamond = 50;
            AssetDatabase.CreateAsset(data, PlayerDataPath);
            AssetDatabase.SaveAssets();
            Debug.Log($"[LobbySceneBuilder] PlayerData.asset 생성 → {PlayerDataPath}");
        }
        return data;
    }

    // ── 씬 계층 빌드 ─────────────────────────────────────────

    private struct SceneRefs
    {
        public Canvas       canvas;
        public Image        backgroundImage;
        public Image        characterImage;
        public Button       myInfoButton;
        public TMP_Text     ticketText;
        public TMP_Text     goldText;
        public TMP_Text     diamondText;
        public Button       ticketPlusButton;
        public Button       goldPlusButton;
        public Button       diamondPlusButton;
        public Button       mailButton;
        public Button       settingsButton;
        public Button       enterGameButton;
        public Button       characterManageButton;
        public Button       shopButton;
        public Button       petManageButton;
        public Button       missionButton;
        public Button       idleRewardButton;
        // 방치보상 팝업
        public GameObject   idlePopupRoot;
        public TMP_Text     idleElapsedText;
        public TMP_Text     idleGoldText;
        public TMP_Text     idleTicketText;
        public TMP_Text     idleDiamondText;
        public Button       idleClaimButton;
        public Button       idleCloseButton;
        public GameObject   idleRewardAnimRoot;
    }

    private static SceneRefs BuildSceneHierarchy()
    {
        SceneRefs refs = default;

        // ── EventSystem ──────────────────────────────────────
        var esSgo = new GameObject("EventSystem",
            typeof(EventSystem), typeof(StandaloneInputModule));

        // ── Canvas ───────────────────────────────────────────
        var canvasGo = new GameObject("Canvas",
            typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));

        refs.canvas = canvasGo.GetComponent<Canvas>();
        refs.canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        refs.canvas.sortingOrder = 0;

        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode          = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution  = RefResolution;
        scaler.screenMatchMode      = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight   = 0.5f;

        Transform canvasTr = canvasGo.transform;

        // ── 배경 이미지 ───────────────────────────────────────
        var bgGo = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bgGo.transform.SetParent(canvasTr, false);
        refs.backgroundImage = bgGo.GetComponent<Image>();
        refs.backgroundImage.color = new Color(0.12f, 0.14f, 0.20f, 1f);
        StretchFull(bgGo.GetComponent<RectTransform>());

        // ── 캐릭터 이미지 ─────────────────────────────────────
        var charGo = new GameObject("CharacterImage", typeof(RectTransform), typeof(Image));
        charGo.transform.SetParent(canvasTr, false);
        refs.characterImage = charGo.GetComponent<Image>();
        refs.characterImage.color = new Color(1f, 1f, 1f, 0f); // 스프라이트 미연결 시 투명
        refs.characterImage.preserveAspect = true;
        var charRt = charGo.GetComponent<RectTransform>();
        charRt.anchorMin       = new Vector2(0.5f, 0.5f);
        charRt.anchorMax       = new Vector2(0.5f, 0.5f);
        charRt.pivot           = new Vector2(0.5f, 0.5f);
        charRt.anchoredPosition = new Vector2(0f, -60f);
        charRt.sizeDelta       = new Vector2(540f, 900f);

        // ── 탑바 ─────────────────────────────────────────────
        BuildTopBar(canvasTr, ref refs);

        // ── 하단 네비 ─────────────────────────────────────────
        BuildBottomNav(canvasTr, ref refs);

        // ── 우측 사이드 ───────────────────────────────────────
        BuildSidePanel(canvasTr, ref refs);

        // ── 방치 보상 팝업 (초기 비활성) ─────────────────────
        BuildIdleRewardPopup(canvasTr, ref refs);

        return refs;
    }

    // ── 탑바 ─────────────────────────────────────────────────

    private static void BuildTopBar(Transform canvas, ref SceneRefs refs)
    {
        // 탑바 컨테이너 (전체 너비 × 110px, 상단 고정)
        var barGo = new GameObject("TopBar", typeof(RectTransform), typeof(Image));
        barGo.transform.SetParent(canvas, false);
        var barImg = barGo.GetComponent<Image>();
        barImg.color = ColTopBar;
        var barRt = barGo.GetComponent<RectTransform>();
        barRt.anchorMin       = new Vector2(0f, 1f);
        barRt.anchorMax       = new Vector2(1f, 1f);
        barRt.pivot           = new Vector2(0.5f, 1f);
        barRt.anchoredPosition = Vector2.zero;
        barRt.sizeDelta       = new Vector2(0f, 110f);

        Transform barTr = barGo.transform;

        // 내 정보 버튼 (좌상단)
        refs.myInfoButton = CreateIconButton(barTr, "Btn_MyInfo", "내 정보",
            new Vector2(75f, -55f), new Vector2(130f, 80f), ColNavBtn);

        // 재화 그룹 (중앙)
        BuildCurrencyGroup(barTr, ref refs);

        // 우편 버튼
        refs.mailButton = CreateIconButton(barTr, "Btn_Mail", "우편",
            new Vector2(-170f, -55f), new Vector2(110f, 80f), ColNavBtn,
            anchor: new Vector2(1f, 1f));

        // 설정 버튼
        refs.settingsButton = CreateIconButton(barTr, "Btn_Settings", "설정",
            new Vector2(-50f, -55f), new Vector2(90f, 80f), ColNavBtn,
            anchor: new Vector2(1f, 1f));
    }

    private static void BuildCurrencyGroup(Transform barTr, ref SceneRefs refs)
    {
        // 재화 그룹 컨테이너 (중앙)
        var groupGo = new GameObject("CurrencyGroup", typeof(RectTransform));
        groupGo.transform.SetParent(barTr, false);
        var groupRt = groupGo.GetComponent<RectTransform>();
        groupRt.anchorMin       = new Vector2(0.5f, 1f);
        groupRt.anchorMax       = new Vector2(0.5f, 1f);
        groupRt.pivot           = new Vector2(0.5f, 1f);
        groupRt.anchoredPosition = new Vector2(0f, 0f);
        groupRt.sizeDelta       = new Vector2(540f, 110f);

        // 티켓 슬롯 (좌)
        BuildCurrencySlot(groupGo.transform, "Ticket", "티켓", -185f,
            out refs.ticketText, out refs.ticketPlusButton);

        // 골드 슬롯 (중)
        BuildCurrencySlot(groupGo.transform, "Gold", "골드", 0f,
            out refs.goldText, out refs.goldPlusButton);

        // 다이아 슬롯 (우)
        BuildCurrencySlot(groupGo.transform, "Diamond", "다이아", 185f,
            out refs.diamondText, out refs.diamondPlusButton);
    }

    private static void BuildCurrencySlot(Transform parent, string id, string label, float offsetX,
        out TMP_Text valueText, out Button plusButton)
    {
        // 슬롯 배경
        var slotGo = new GameObject($"Slot_{id}", typeof(RectTransform), typeof(Image));
        slotGo.transform.SetParent(parent, false);
        slotGo.GetComponent<Image>().color = ColCurrencyBg;
        var slotRt = slotGo.GetComponent<RectTransform>();
        slotRt.anchorMin       = new Vector2(0.5f, 0.5f);
        slotRt.anchorMax       = new Vector2(0.5f, 0.5f);
        slotRt.pivot           = new Vector2(0.5f, 0.5f);
        slotRt.anchoredPosition = new Vector2(offsetX, -55f);
        slotRt.sizeDelta       = new Vector2(165f, 52f);

        // 라벨
        var labelGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelGo.transform.SetParent(slotGo.transform, false);
        var labelRt = labelGo.GetComponent<RectTransform>();
        labelRt.anchorMin = new Vector2(0f, 0f);
        labelRt.anchorMax = new Vector2(0f, 1f);
        labelRt.pivot     = new Vector2(0f, 0.5f);
        labelRt.offsetMin = new Vector2(6f, 0f);
        labelRt.offsetMax = new Vector2(52f, 0f);
        var labelTmp = labelGo.GetComponent<TextMeshProUGUI>();
        labelTmp.text      = label;
        labelTmp.fontSize  = 18f;
        labelTmp.color     = new Color(0.7f, 0.7f, 0.7f, 1f);
        labelTmp.alignment = TextAlignmentOptions.MidlineLeft;
        labelTmp.enableWordWrapping = false;

        // 수치 텍스트
        var valGo = new GameObject("Value", typeof(RectTransform), typeof(TextMeshProUGUI));
        valGo.transform.SetParent(slotGo.transform, false);
        var valRt = valGo.GetComponent<RectTransform>();
        valRt.anchorMin = new Vector2(0f, 0f);
        valRt.anchorMax = new Vector2(1f, 1f);
        valRt.offsetMin = new Vector2(54f, 0f);
        valRt.offsetMax = new Vector2(-32f, 0f);
        var valTmp = valGo.GetComponent<TextMeshProUGUI>();
        valTmp.text      = "0";
        valTmp.fontSize  = 26f;
        valTmp.fontStyle = FontStyles.Bold;
        valTmp.color     = Color.white;
        valTmp.alignment = TextAlignmentOptions.MidlineRight;
        valTmp.enableWordWrapping = false;
        valueText = valTmp;

        // '+' 버튼
        var plusGo = new GameObject("Btn_Plus", typeof(RectTransform), typeof(Image), typeof(Button));
        plusGo.transform.SetParent(slotGo.transform, false);
        plusGo.GetComponent<Image>().color = ColPlusBtn;
        var plusRt = plusGo.GetComponent<RectTransform>();
        plusRt.anchorMin = new Vector2(1f, 0.5f);
        plusRt.anchorMax = new Vector2(1f, 0.5f);
        plusRt.pivot     = new Vector2(1f, 0.5f);
        plusRt.anchoredPosition = Vector2.zero;
        plusRt.sizeDelta = new Vector2(28f, 52f);
        var plusTxtGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        plusTxtGo.transform.SetParent(plusGo.transform, false);
        StretchFull(plusTxtGo.GetComponent<RectTransform>());
        var plusTmp = plusTxtGo.GetComponent<TextMeshProUGUI>();
        plusTmp.text      = "+";
        plusTmp.fontSize  = 24f;
        plusTmp.fontStyle = FontStyles.Bold;
        plusTmp.color     = Color.white;
        plusTmp.alignment = TextAlignmentOptions.Center;
        plusButton = plusGo.GetComponent<Button>();
    }

    // ── 하단 네비 ─────────────────────────────────────────────

    private static void BuildBottomNav(Transform canvas, ref SceneRefs refs)
    {
        var barGo = new GameObject("BottomNav", typeof(RectTransform), typeof(Image));
        barGo.transform.SetParent(canvas, false);
        barGo.GetComponent<Image>().color = ColBottomBar;
        var barRt = barGo.GetComponent<RectTransform>();
        barRt.anchorMin       = new Vector2(0f, 0f);
        barRt.anchorMax       = new Vector2(1f, 0f);
        barRt.pivot           = new Vector2(0.5f, 0f);
        barRt.anchoredPosition = Vector2.zero;
        barRt.sizeDelta       = new Vector2(0f, 130f);

        Transform barTr = barGo.transform;
        float btnW = 250f;
        float btnH = 100f;
        float y    = 65f;

        // 4등분 배치: 게임진입(강조) / 캐릭터 / 상점 / 펫
        refs.enterGameButton      = CreateNavButton(barTr, "Btn_EnterGame",  "게임진입",  -382f, y, btnW, btnH, ColNavActive);
        refs.characterManageButton = CreateNavButton(barTr, "Btn_Character", "캐릭터",   -127f, y, btnW, btnH, ColNavBtn);
        refs.shopButton            = CreateNavButton(barTr, "Btn_Shop",      "상점",      127f, y, btnW, btnH, ColNavBtn);
        refs.petManageButton       = CreateNavButton(barTr, "Btn_Pet",       "펫관리",    382f, y, btnW, btnH, ColNavBtn);
    }

    // ── 우측 사이드 ───────────────────────────────────────────

    private static void BuildSidePanel(Transform canvas, ref SceneRefs refs)
    {
        var panelGo = new GameObject("SidePanel", typeof(RectTransform));
        panelGo.transform.SetParent(canvas, false);
        var panelRt = panelGo.GetComponent<RectTransform>();
        panelRt.anchorMin       = new Vector2(1f, 0.5f);
        panelRt.anchorMax       = new Vector2(1f, 0.5f);
        panelRt.pivot           = new Vector2(1f, 0.5f);
        panelRt.anchoredPosition = new Vector2(0f, 80f);
        panelRt.sizeDelta       = new Vector2(110f, 280f);

        Transform panelTr = panelGo.transform;
        refs.missionButton    = CreateIconButton(panelTr, "Btn_Mission",    "미션",    new Vector2(0f,  100f), new Vector2(110f, 110f), ColSideBtn);
        refs.idleRewardButton = CreateIconButton(panelTr, "Btn_IdleReward", "방치보상", new Vector2(0f, -30f), new Vector2(110f, 110f), ColSideBtn);
    }

    // ── 에셋 생성 헬퍼 ───────────────────────────────────────

    private static IdleRewardConfig EnsureIdleRewardConfig()
    {
        IdleRewardConfig cfg = AssetDatabase.LoadAssetAtPath<IdleRewardConfig>(IdleConfigPath);
        if (cfg == null)
        {
            cfg = ScriptableObject.CreateInstance<IdleRewardConfig>();
            // 기본값: goldPerHour=100, maxOfflineHours=12 (클래스 기본값 사용)
            AssetDatabase.CreateAsset(cfg, IdleConfigPath);
            AssetDatabase.SaveAssets();
            Debug.Log($"[LobbySceneBuilder] IdleRewardConfig.asset 생성 → {IdleConfigPath}");
        }
        return cfg;
    }

    private static MailBox EnsureMailBox()
    {
        MailBox mb = AssetDatabase.LoadAssetAtPath<MailBox>(MailBoxPath);
        if (mb == null)
        {
            mb = ScriptableObject.CreateInstance<MailBox>();
            AssetDatabase.CreateAsset(mb, MailBoxPath);
            AssetDatabase.SaveAssets();
            Debug.Log($"[LobbySceneBuilder] MailBox.asset 생성 → {MailBoxPath}");
        }
        return mb;
    }

    // ── 방치 보상 팝업 빌드 ───────────────────────────────────

    private static void BuildIdleRewardPopup(Transform canvas, ref SceneRefs refs)
    {
        var colPopupBg    = new Color(0f,    0f,    0f,    0.75f);
        var colPanel      = new Color(0.10f, 0.12f, 0.18f, 1.00f);
        var colDivider    = new Color(0.30f, 0.30f, 0.40f, 1.00f);
        var colRowBg      = new Color(0.06f, 0.06f, 0.10f, 0.90f);
        var colClaimBtn   = new Color(0.20f, 0.60f, 0.95f, 1.00f);
        var colCloseBtn   = new Color(0.35f, 0.35f, 0.40f, 1.00f);

        // ── 팝업 루트 (풀스크린, 초기 비활성) ─────────────────
        var popupGo = new GameObject("IdleRewardPopup",
            typeof(RectTransform), typeof(Image), typeof(Button));
        popupGo.transform.SetParent(canvas, false);
        popupGo.GetComponent<Image>().color = colPopupBg;
        // 뒷배경 터치 시 팝업 닫기 (CloseButton 역할 겸용, 실제 연결은 WireIdleRewardManager)
        StretchFull(popupGo.GetComponent<RectTransform>());
        popupGo.SetActive(false);
        refs.idlePopupRoot = popupGo;

        // ── 카드 패널 ─────────────────────────────────────────
        var panelGo = new GameObject("Panel", typeof(RectTransform), typeof(Image));
        panelGo.transform.SetParent(popupGo.transform, false);
        panelGo.GetComponent<Image>().color = colPanel;
        var panelRt = panelGo.GetComponent<RectTransform>();
        panelRt.anchorMin        = new Vector2(0.5f, 0.5f);
        panelRt.anchorMax        = new Vector2(0.5f, 0.5f);
        panelRt.pivot            = new Vector2(0.5f, 0.5f);
        panelRt.anchoredPosition = Vector2.zero;
        panelRt.sizeDelta        = new Vector2(700f, 760f);
        Transform panelTr = panelGo.transform;

        // 제목
        var titleGo = MakeText(panelTr, "Title", "방치 보상",
            new Vector2(0f, 320f), new Vector2(600f, 80f),
            fontSize: 48f, bold: true, color: Color.white);

        // 경과 시간
        var elapsedGo = MakeText(panelTr, "ElapsedText", "0분",
            new Vector2(0f, 240f), new Vector2(600f, 54f),
            fontSize: 32f, bold: false, color: new Color(0.75f, 0.85f, 1f, 1f));
        refs.idleElapsedText = elapsedGo.GetComponent<TextMeshProUGUI>();

        // 구분선
        var divGo = new GameObject("Divider", typeof(RectTransform), typeof(Image));
        divGo.transform.SetParent(panelTr, false);
        divGo.GetComponent<Image>().color = colDivider;
        var divRt = divGo.GetComponent<RectTransform>();
        divRt.anchorMin = new Vector2(0.5f, 0.5f);
        divRt.anchorMax = new Vector2(0.5f, 0.5f);
        divRt.pivot     = new Vector2(0.5f, 0.5f);
        divRt.anchoredPosition = new Vector2(0f, 175f);
        divRt.sizeDelta = new Vector2(620f, 2f);

        // 보상 행: 골드 / 티켓 / 다이아
        refs.idleGoldText    = BuildRewardRow(panelTr, "GoldRow",    "골드",   colRowBg, 100f);
        refs.idleTicketText  = BuildRewardRow(panelTr, "TicketRow",  "티켓",   colRowBg,   0f);
        refs.idleDiamondText = BuildRewardRow(panelTr, "DiamondRow", "다이아", colRowBg, -100f);

        // 받기 버튼
        var claimGo = new GameObject("Btn_Claim",
            typeof(RectTransform), typeof(Image), typeof(Button));
        claimGo.transform.SetParent(panelTr, false);
        claimGo.GetComponent<Image>().color = colClaimBtn;
        var claimRt = claimGo.GetComponent<RectTransform>();
        claimRt.anchorMin = new Vector2(0.5f, 0.5f);
        claimRt.anchorMax = new Vector2(0.5f, 0.5f);
        claimRt.pivot     = new Vector2(0.5f, 0.5f);
        claimRt.anchoredPosition = new Vector2(0f, -260f);
        claimRt.sizeDelta = new Vector2(540f, 100f);
        MakeText(claimGo.transform, "Text", "받기",
            Vector2.zero, Vector2.zero, 40f, true, Color.white, stretch: true);
        refs.idleClaimButton = claimGo.GetComponent<Button>();

        // 닫기 버튼 (✕)
        var closeGo = new GameObject("Btn_Close",
            typeof(RectTransform), typeof(Image), typeof(Button));
        closeGo.transform.SetParent(panelTr, false);
        closeGo.GetComponent<Image>().color = colCloseBtn;
        var closeRt = closeGo.GetComponent<RectTransform>();
        closeRt.anchorMin = new Vector2(1f, 1f);
        closeRt.anchorMax = new Vector2(1f, 1f);
        closeRt.pivot     = new Vector2(0.5f, 0.5f);
        closeRt.anchoredPosition = new Vector2(-20f, -20f);
        closeRt.sizeDelta = new Vector2(60f, 60f);
        MakeText(closeGo.transform, "Text", "✕",
            Vector2.zero, Vector2.zero, 30f, true, Color.white, stretch: true);
        refs.idleCloseButton = closeGo.GetComponent<Button>();

        // 보상 연출 루트 (비활성 — Animator / Particle 추가용)
        var animGo = new GameObject("RewardAnim", typeof(RectTransform));
        animGo.transform.SetParent(panelTr, false);
        var animRt = animGo.GetComponent<RectTransform>();
        animRt.anchorMin = new Vector2(0.5f, 0.5f);
        animRt.anchorMax = new Vector2(0.5f, 0.5f);
        animRt.pivot     = new Vector2(0.5f, 0.5f);
        animRt.anchoredPosition = Vector2.zero;
        animRt.sizeDelta = new Vector2(400f, 400f);
        animGo.SetActive(false);
        refs.idleRewardAnimRoot = animGo;
    }

    private static TMP_Text BuildRewardRow(Transform parent, string rowName,
        string label, Color rowBg, float yOffset)
    {
        var rowGo = new GameObject(rowName, typeof(RectTransform), typeof(Image));
        rowGo.transform.SetParent(parent, false);
        rowGo.GetComponent<Image>().color = rowBg;
        var rowRt = rowGo.GetComponent<RectTransform>();
        rowRt.anchorMin = new Vector2(0.5f, 0.5f);
        rowRt.anchorMax = new Vector2(0.5f, 0.5f);
        rowRt.pivot     = new Vector2(0.5f, 0.5f);
        rowRt.anchoredPosition = new Vector2(0f, yOffset);
        rowRt.sizeDelta = new Vector2(620f, 76f);

        // 라벨
        MakeText(rowGo.transform, "Label", label,
            new Vector2(-180f, 0f), new Vector2(200f, 76f),
            fontSize: 28f, bold: false, color: new Color(0.7f, 0.7f, 0.7f, 1f));

        // 값 텍스트
        var valGo = MakeText(rowGo.transform, "Value", "+0",
            new Vector2(90f, 0f), new Vector2(320f, 76f),
            fontSize: 36f, bold: true, color: new Color(1f, 0.85f, 0.3f, 1f));
        return valGo.GetComponent<TextMeshProUGUI>();
    }

    // ── IdleRewardManager 연결 ────────────────────────────────

    private static void WireIdleRewardManager(SceneRefs refs,
        PlayerData playerData, IdleRewardConfig config, MailBox mailBox)
    {
        var mgrGo = new GameObject("IdleRewardManager");
        var mgr   = mgrGo.AddComponent<IdleRewardManager>();

        var so = new SerializedObject(mgr);
        so.FindProperty("playerData")        .objectReferenceValue = playerData;
        so.FindProperty("config")            .objectReferenceValue = config;
        so.FindProperty("mailBox")           .objectReferenceValue = mailBox;
        so.FindProperty("popupRoot")         .objectReferenceValue = refs.idlePopupRoot;
        so.FindProperty("elapsedTimeText")   .objectReferenceValue = refs.idleElapsedText;
        so.FindProperty("rewardGoldText")    .objectReferenceValue = refs.idleGoldText;
        so.FindProperty("rewardTicketText")  .objectReferenceValue = refs.idleTicketText;
        so.FindProperty("rewardDiamondText") .objectReferenceValue = refs.idleDiamondText;
        so.FindProperty("claimButton")       .objectReferenceValue = refs.idleClaimButton;
        so.FindProperty("closeButton")       .objectReferenceValue = refs.idleCloseButton;
        so.FindProperty("rewardAnimRoot")    .objectReferenceValue = refs.idleRewardAnimRoot;
        so.ApplyModifiedPropertiesWithoutUndo();

        // LobbyManager에 idleRewardManager 연결
        var lobbyMgr = UnityEngine.Object.FindObjectOfType<LobbyManager>();
        if (lobbyMgr != null)
        {
            var lobbySo = new SerializedObject(lobbyMgr);
            lobbySo.FindProperty("idleRewardManager").objectReferenceValue = mgr;
            lobbySo.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    // ── LobbyManager 연결 ─────────────────────────────────────

    private static void WireLobbyManager(SceneRefs refs, PlayerData playerData)
    {
        var mgrGo = new GameObject("LobbyManager");
        var mgr   = mgrGo.AddComponent<LobbyManager>();

        var so = new SerializedObject(mgr);

        so.FindProperty("playerData")             .objectReferenceValue = playerData;
        so.FindProperty("backgroundImage")         .objectReferenceValue = refs.backgroundImage;
        so.FindProperty("characterImage")          .objectReferenceValue = refs.characterImage;
        so.FindProperty("myInfoButton")            .objectReferenceValue = refs.myInfoButton;
        so.FindProperty("ticketText")              .objectReferenceValue = refs.ticketText;
        so.FindProperty("goldText")                .objectReferenceValue = refs.goldText;
        so.FindProperty("diamondText")             .objectReferenceValue = refs.diamondText;
        so.FindProperty("ticketPlusButton")        .objectReferenceValue = refs.ticketPlusButton;
        so.FindProperty("goldPlusButton")          .objectReferenceValue = refs.goldPlusButton;
        so.FindProperty("diamondPlusButton")       .objectReferenceValue = refs.diamondPlusButton;
        so.FindProperty("mailButton")              .objectReferenceValue = refs.mailButton;
        so.FindProperty("settingsButton")          .objectReferenceValue = refs.settingsButton;
        so.FindProperty("enterGameButton")         .objectReferenceValue = refs.enterGameButton;
        so.FindProperty("characterManageButton")   .objectReferenceValue = refs.characterManageButton;
        so.FindProperty("shopButton")              .objectReferenceValue = refs.shopButton;
        so.FindProperty("petManageButton")         .objectReferenceValue = refs.petManageButton;
        so.FindProperty("missionButton")           .objectReferenceValue = refs.missionButton;
        so.FindProperty("idleRewardButton")        .objectReferenceValue = refs.idleRewardButton;
        // idleRewardManager 는 WireIdleRewardManager() 에서 별도 연결

        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // ── 공통 생성 헬퍼 ────────────────────────────────────────

    /// <summary>TMP_Text GameObject 생성. stretch=true 이면 부모를 가득 채웁니다.</summary>
    private static GameObject MakeText(Transform parent, string name, string text,
        Vector2 pos, Vector2 size, float fontSize, bool bold,
        Color color, bool stretch = false)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        if (stretch)
        {
            StretchFull(rt);
        }
        else
        {
            rt.anchorMin        = new Vector2(0.5f, 0.5f);
            rt.anchorMax        = new Vector2(0.5f, 0.5f);
            rt.pivot            = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta        = size;
        }
        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text               = text;
        tmp.fontSize           = fontSize;
        tmp.fontStyle          = bold ? FontStyles.Bold : FontStyles.Normal;
        tmp.color              = color;
        tmp.alignment          = TextAlignmentOptions.Center;
        tmp.enableWordWrapping = false;
        return go;
    }

    /// <summary>아이콘(텍스트) 버튼 생성. anchor 기본값은 좌상단 (0,1).</summary>
    private static Button CreateIconButton(Transform parent, string name, string label,
        Vector2 pos, Vector2 size, Color color, Vector2 anchor = default)
    {
        if (anchor == default) anchor = new Vector2(0f, 1f);

        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        go.GetComponent<Image>().color = color;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin       = anchor;
        rt.anchorMax       = anchor;
        rt.pivot           = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta       = size;

        var txtGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtGo.transform.SetParent(go.transform, false);
        StretchFull(txtGo.GetComponent<RectTransform>());
        var tmp = txtGo.GetComponent<TextMeshProUGUI>();
        tmp.text      = label;
        tmp.fontSize  = 24f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color     = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableWordWrapping = false;

        return go.GetComponent<Button>();
    }

    /// <summary>하단 네비 버튼 생성. 앵커 중앙 하단.</summary>
    private static Button CreateNavButton(Transform parent, string name, string label,
        float x, float y, float w, float h, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        go.GetComponent<Image>().color = color;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin       = new Vector2(0.5f, 0f);
        rt.anchorMax       = new Vector2(0.5f, 0f);
        rt.pivot           = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta       = new Vector2(w, h);

        var txtGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtGo.transform.SetParent(go.transform, false);
        StretchFull(txtGo.GetComponent<RectTransform>());
        var tmp = txtGo.GetComponent<TextMeshProUGUI>();
        tmp.text      = label;
        tmp.fontSize  = 28f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color     = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableWordWrapping = false;

        return go.GetComponent<Button>();
    }

    private static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
#endif