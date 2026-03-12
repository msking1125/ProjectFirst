#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using ProjectFirst.Data;
/// <summary>
/// 濡쒕퉬 ???먮룞 鍮뚮뜑.
/// 硫붾돱: Tools ??Game ??Build Lobby Scene
///
/// ?ㅽ뻾?섎㈃:
///  1. Assets/Project/Scenes/Lobby.unity ?ъ쓣 ?앹꽦 (湲곗〈 ?뚯씪 ?덉쑝硫???뼱?곌린 ?щ? ?뺤씤)
///  2. Canvas / EventSystem / ?꾩껜 UI 怨꾩링 ?앹꽦
///  3. LobbyManager 而댄룷?뚰듃瑜?異붽??섍퀬 紐⑤뱺 ?덊띁?곗뒪瑜??먮룞 ?곌껐
///  4. PlayerData.asset ???놁쑝硫?Assets/Project/Data/ ???앹꽦
///  5. ?????
/// </summary>
public static class LobbySceneBuilder
{
    private const string ScenePath          = "Assets/Project/Scenes/Lobby.unity";
    private const string PlayerDataPath     = "Assets/Project/Data/PlayerData.asset";
    private const string IdleConfigPath     = "Assets/Project/Data/IdleRewardConfig.asset";
    private const string MailBoxPath        = "Assets/Project/Data/MailBox.asset";

    // ?덊띁?곗뒪 ?댁긽??(9:16 紐⑤컮??
    private static readonly Vector2 RefResolution = new Vector2(1080f, 1920f);

    // ?? ?붾젅????????????????????????????????????????????????
    private static readonly Color ColTopBar      = new Color(0.08f, 0.08f, 0.12f, 0.92f);
    private static readonly Color ColBottomBar   = new Color(0.08f, 0.08f, 0.12f, 0.95f);
    private static readonly Color ColSideBtn     = new Color(0.15f, 0.55f, 0.85f, 0.90f);
    private static readonly Color ColNavBtn      = new Color(0.18f, 0.18f, 0.22f, 0.95f);
    private static readonly Color ColNavActive   = new Color(0.20f, 0.60f, 0.95f, 1.00f);
    private static readonly Color ColPlusBtn     = new Color(0.30f, 0.75f, 0.40f, 1.00f);
    private static readonly Color ColCurrencyBg  = new Color(0.05f, 0.05f, 0.08f, 0.85f);

    // ?? 吏꾩엯?????????????????????????????????????????????????

    [MenuItem("Tools/Game/Build Lobby Scene")]
    public static void Build()
    {
        // ??λ릺吏 ?딆? ??蹂寃쎌궗???뺤씤
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        // 湲곗〈 Lobby ???뚯씪 議댁옱 ????뼱?곌린 ?뺤씤
        if (System.IO.File.Exists(ScenePath))
        {
            bool overwrite = EditorUtility.DisplayDialog(
                "Lobby ??鍮뚮뜑",
                $"{ScenePath} ??媛) ?대? 議댁옱?⑸땲??\n??뼱?곗떆寃좎뒿?덇퉴?",
                "??뼱?곌린", "痍⑥냼");

            if (!overwrite) return;
        }

        // ?????앹꽦
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // ?먯뀑 以鍮?
        PlayerData playerData       = EnsurePlayerData();
        IdleRewardConfig idleConfig = EnsureIdleRewardConfig();
        MailBox mailBox             = EnsureMailBox();

        // UI 怨꾩링 鍮뚮뱶
        var refs = BuildSceneHierarchy();

        // LobbyManager 諛곗튂 諛??곌껐
        WireLobbyManager(refs, playerData);

        // IdleRewardManager 諛곗튂 諛??곌껐
        WireIdleRewardManager(refs, playerData, idleConfig, mailBox);

        // ?????
        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.Refresh();

        Debug.Log($"[LobbySceneBuilder] ??鍮뚮뱶 ?꾨즺 ??{ScenePath}");
        EditorUtility.DisplayDialog("?꾨즺", $"Lobby ?ъ씠 ?앹꽦?섏뿀?듬땲??\n{ScenePath}", "?뺤씤");
    }

    // ?? PlayerData ?먯뀑 ???????????????????????????????????????

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
            Debug.Log($"[LobbySceneBuilder] PlayerData.asset ?앹꽦 ??{PlayerDataPath}");
        }
        return data;
    }

    // ?? ??怨꾩링 鍮뚮뱶 ?????????????????????????????????????????

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
        // 諛⑹튂蹂댁긽 ?앹뾽
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

        // ?? EventSystem ??????????????????????????????????????
        var esSgo = new GameObject("EventSystem",
            typeof(EventSystem), typeof(StandaloneInputModule));

        // ?? Canvas ???????????????????????????????????????????
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

        // ?? 諛곌꼍 ?대?吏 ???????????????????????????????????????
        var bgGo = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bgGo.transform.SetParent(canvasTr, false);
        refs.backgroundImage = bgGo.GetComponent<Image>();
        refs.backgroundImage.color = new Color(0.12f, 0.14f, 0.20f, 1f);
        StretchFull(bgGo.GetComponent<RectTransform>());

        // ?? 罹먮┃???대?吏 ?????????????????????????????????????
        var charGo = new GameObject("CharacterImage", typeof(RectTransform), typeof(Image));
        charGo.transform.SetParent(canvasTr, false);
        refs.characterImage = charGo.GetComponent<Image>();
        refs.characterImage.color = new Color(1f, 1f, 1f, 0f); // ?ㅽ봽?쇱씠??誘몄뿰寃????щ챸
        refs.characterImage.preserveAspect = true;
        var charRt = charGo.GetComponent<RectTransform>();
        charRt.anchorMin       = new Vector2(0.5f, 0.5f);
        charRt.anchorMax       = new Vector2(0.5f, 0.5f);
        charRt.pivot           = new Vector2(0.5f, 0.5f);
        charRt.anchoredPosition = new Vector2(0f, -60f);
        charRt.sizeDelta       = new Vector2(540f, 900f);

        // ?? ?묐컮 ?????????????????????????????????????????????
        BuildTopBar(canvasTr, ref refs);

        // ?? ?섎떒 ?ㅻ퉬 ?????????????????????????????????????????
        BuildBottomNav(canvasTr, ref refs);

        // ?? ?곗륫 ?ъ씠?????????????????????????????????????????
        BuildSidePanel(canvasTr, ref refs);

        // ?? 諛⑹튂 蹂댁긽 ?앹뾽 (珥덇린 鍮꾪솢?? ?????????????????????
        BuildIdleRewardPopup(canvasTr, ref refs);

        return refs;
    }

    // ?? ?묐컮 ?????????????????????????????????????????????????

    private static void BuildTopBar(Transform canvas, ref SceneRefs refs)
    {
        // ?묐컮 而⑦뀒?대꼫 (?꾩껜 ?덈퉬 횞 110px, ?곷떒 怨좎젙)
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

        // ???뺣낫 踰꾪듉 (醫뚯긽??
        refs.myInfoButton = CreateIconButton(barTr, "Btn_MyInfo", "???뺣낫",
            new Vector2(75f, -55f), new Vector2(130f, 80f), ColNavBtn);

        // ?ы솕 洹몃９ (以묒븰)
        BuildCurrencyGroup(barTr, ref refs);

        // ?고렪 踰꾪듉
        refs.mailButton = CreateIconButton(barTr, "Btn_Mail", "?고렪",
            new Vector2(-170f, -55f), new Vector2(110f, 80f), ColNavBtn,
            anchor: new Vector2(1f, 1f));

        // ?ㅼ젙 踰꾪듉
        refs.settingsButton = CreateIconButton(barTr, "Btn_Settings", "?ㅼ젙",
            new Vector2(-50f, -55f), new Vector2(90f, 80f), ColNavBtn,
            anchor: new Vector2(1f, 1f));
    }

    private static void BuildCurrencyGroup(Transform barTr, ref SceneRefs refs)
    {
        // ?ы솕 洹몃９ 而⑦뀒?대꼫 (以묒븰)
        var groupGo = new GameObject("CurrencyGroup", typeof(RectTransform));
        groupGo.transform.SetParent(barTr, false);
        var groupRt = groupGo.GetComponent<RectTransform>();
        groupRt.anchorMin       = new Vector2(0.5f, 1f);
        groupRt.anchorMax       = new Vector2(0.5f, 1f);
        groupRt.pivot           = new Vector2(0.5f, 1f);
        groupRt.anchoredPosition = new Vector2(0f, 0f);
        groupRt.sizeDelta       = new Vector2(540f, 110f);

        // ?곗폆 ?щ’ (醫?
        BuildCurrencySlot(groupGo.transform, "Ticket", "?곗폆", -185f,
            out refs.ticketText, out refs.ticketPlusButton);

        // 怨⑤뱶 ?щ’ (以?
        BuildCurrencySlot(groupGo.transform, "Gold", "怨⑤뱶", 0f,
            out refs.goldText, out refs.goldPlusButton);

        // ?ㅼ씠???щ’ (??
        BuildCurrencySlot(groupGo.transform, "Diamond", "Diamond", 185f,
            out refs.diamondText, out refs.diamondPlusButton);
    }

    private static void BuildCurrencySlot(Transform parent, string id, string label, float offsetX,
        out TMP_Text valueText, out Button plusButton)
    {
        // ?щ’ 諛곌꼍
        var slotGo = new GameObject($"Slot_{id}", typeof(RectTransform), typeof(Image));
        slotGo.transform.SetParent(parent, false);
        slotGo.GetComponent<Image>().color = ColCurrencyBg;
        var slotRt = slotGo.GetComponent<RectTransform>();
        slotRt.anchorMin       = new Vector2(0.5f, 0.5f);
        slotRt.anchorMax       = new Vector2(0.5f, 0.5f);
        slotRt.pivot           = new Vector2(0.5f, 0.5f);
        slotRt.anchoredPosition = new Vector2(offsetX, -55f);
        slotRt.sizeDelta       = new Vector2(165f, 52f);

        // ?쇰꺼
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

        // ?섏튂 ?띿뒪??
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

        // '+' 踰꾪듉
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

    // ?? ?섎떒 ?ㅻ퉬 ?????????????????????????????????????????????

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

        // 4?깅텇 諛곗튂: 寃뚯엫吏꾩엯(媛뺤“) / 罹먮┃??/ ?곸젏 / ??
        refs.enterGameButton      = CreateNavButton(barTr, "Btn_EnterGame",  "寃뚯엫吏꾩엯",  -382f, y, btnW, btnH, ColNavActive);
        refs.characterManageButton = CreateNavButton(barTr, "Btn_Character", "Character", -127f, y, btnW, btnH, ColNavBtn);
        refs.shopButton            = CreateNavButton(barTr, "Btn_Shop",      "?곸젏",      127f, y, btnW, btnH, ColNavBtn);
        refs.petManageButton       = CreateNavButton(barTr, "Btn_Pet",       "Pet",       382f, y, btnW, btnH, ColNavBtn);
    }

    // ?? ?곗륫 ?ъ씠?????????????????????????????????????????????

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
        refs.missionButton    = CreateIconButton(panelTr, "Btn_Mission",    "誘몄뀡",    new Vector2(0f,  100f), new Vector2(110f, 110f), ColSideBtn);
        refs.idleRewardButton = CreateIconButton(panelTr, "Btn_IdleReward", "諛⑹튂蹂댁긽", new Vector2(0f, -30f), new Vector2(110f, 110f), ColSideBtn);
    }

    // ?? ?먯뀑 ?앹꽦 ?ы띁 ???????????????????????????????????????

    private static IdleRewardConfig EnsureIdleRewardConfig()
    {
        IdleRewardConfig cfg = AssetDatabase.LoadAssetAtPath<IdleRewardConfig>(IdleConfigPath);
        if (cfg == null)
        {
            cfg = ScriptableObject.CreateInstance<IdleRewardConfig>();
            // 湲곕낯媛? goldPerHour=100, maxOfflineHours=12 (?대옒??湲곕낯媛??ъ슜)
            AssetDatabase.CreateAsset(cfg, IdleConfigPath);
            AssetDatabase.SaveAssets();
            Debug.Log($"[LobbySceneBuilder] IdleRewardConfig.asset ?앹꽦 ??{IdleConfigPath}");
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
            Debug.Log($"[LobbySceneBuilder] MailBox.asset ?앹꽦 ??{MailBoxPath}");
        }
        return mb;
    }

    // ?? 諛⑹튂 蹂댁긽 ?앹뾽 鍮뚮뱶 ???????????????????????????????????

    private static void BuildIdleRewardPopup(Transform canvas, ref SceneRefs refs)
    {
        var colPopupBg    = new Color(0f,    0f,    0f,    0.75f);
        var colPanel      = new Color(0.10f, 0.12f, 0.18f, 1.00f);
        var colDivider    = new Color(0.30f, 0.30f, 0.40f, 1.00f);
        var colRowBg      = new Color(0.06f, 0.06f, 0.10f, 0.90f);
        var colClaimBtn   = new Color(0.20f, 0.60f, 0.95f, 1.00f);
        var colCloseBtn   = new Color(0.35f, 0.35f, 0.40f, 1.00f);

        // ?? ?앹뾽 猷⑦듃 (??ㅽ겕由? 珥덇린 鍮꾪솢?? ?????????????????
        var popupGo = new GameObject("IdleRewardPopup",
            typeof(RectTransform), typeof(Image), typeof(Button));
        popupGo.transform.SetParent(canvas, false);
        popupGo.GetComponent<Image>().color = colPopupBg;
        // ?룸같寃??곗튂 ???앹뾽 ?リ린 (CloseButton ??븷 寃몄슜, ?ㅼ젣 ?곌껐? WireIdleRewardManager)
        StretchFull(popupGo.GetComponent<RectTransform>());
        popupGo.SetActive(false);
        refs.idlePopupRoot = popupGo;

        // ?? 移대뱶 ?⑤꼸 ?????????????????????????????????????????
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

        // ?쒕ぉ
        var titleGo = MakeText(panelTr, "Title", "諛⑹튂 蹂댁긽",
            new Vector2(0f, 320f), new Vector2(600f, 80f),
            fontSize: 48f, bold: true, color: Color.white);

        // 寃쎄낵 ?쒓컙
        var elapsedGo = MakeText(panelTr, "ElapsedText", "0 min",
            new Vector2(0f, 240f), new Vector2(600f, 54f),
            fontSize: 32f, bold: false, color: new Color(0.75f, 0.85f, 1f, 1f));
        refs.idleElapsedText = elapsedGo.GetComponent<TextMeshProUGUI>();

        // 援щ텇??
        var divGo = new GameObject("Divider", typeof(RectTransform), typeof(Image));
        divGo.transform.SetParent(panelTr, false);
        divGo.GetComponent<Image>().color = colDivider;
        var divRt = divGo.GetComponent<RectTransform>();
        divRt.anchorMin = new Vector2(0.5f, 0.5f);
        divRt.anchorMax = new Vector2(0.5f, 0.5f);
        divRt.pivot     = new Vector2(0.5f, 0.5f);
        divRt.anchoredPosition = new Vector2(0f, 175f);
        divRt.sizeDelta = new Vector2(620f, 2f);

        // 蹂댁긽 ?? 怨⑤뱶 / ?곗폆 / ?ㅼ씠??
        refs.idleGoldText    = BuildRewardRow(panelTr, "GoldRow",    "怨⑤뱶",   colRowBg, 100f);
        refs.idleTicketText  = BuildRewardRow(panelTr, "TicketRow",  "?곗폆",   colRowBg,   0f);
        refs.idleDiamondText = BuildRewardRow(panelTr, "DiamondRow", "Diamond", colRowBg, -100f);

        // 諛쏄린 踰꾪듉
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
        MakeText(claimGo.transform, "Text", "諛쏄린",
            Vector2.zero, Vector2.zero, 40f, true, Color.white, stretch: true);
        refs.idleClaimButton = claimGo.GetComponent<Button>();

        // ?リ린 踰꾪듉 (??
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
        MakeText(closeGo.transform, "Text", "X",
            Vector2.zero, Vector2.zero, 30f, true, Color.white, stretch: true);
        refs.idleCloseButton = closeGo.GetComponent<Button>();

        // 蹂댁긽 ?곗텧 猷⑦듃 (鍮꾪솢????Animator / Particle 異붽???
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

        // ?쇰꺼
        MakeText(rowGo.transform, "Label", label,
            new Vector2(-180f, 0f), new Vector2(200f, 76f),
            fontSize: 28f, bold: false, color: new Color(0.7f, 0.7f, 0.7f, 1f));

        // 媛??띿뒪??
        var valGo = MakeText(rowGo.transform, "Value", "+0",
            new Vector2(90f, 0f), new Vector2(320f, 76f),
            fontSize: 36f, bold: true, color: new Color(1f, 0.85f, 0.3f, 1f));
        return valGo.GetComponent<TextMeshProUGUI>();
    }

    // ?? IdleRewardManager ?곌껐 ????????????????????????????????

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

        // LobbyManager??idleRewardManager ?곌껐
        var lobbyMgr = UnityEngine.Object.FindObjectOfType<LobbyManager>();
        if (lobbyMgr != null)
        {
            var lobbySo = new SerializedObject(lobbyMgr);
            lobbySo.FindProperty("idleRewardManager").objectReferenceValue = mgr;
            lobbySo.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    // ?? LobbyManager ?곌껐 ?????????????????????????????????????

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
        // idleRewardManager ??WireIdleRewardManager() ?먯꽌 蹂꾨룄 ?곌껐

        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // ?? 怨듯넻 ?앹꽦 ?ы띁 ????????????????????????????????????????

    /// <summary>TMP_Text GameObject ?앹꽦. stretch=true ?대㈃ 遺紐⑤? 媛??梨꾩썎?덈떎.</summary>
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

    /// <summary>?꾩씠肄??띿뒪?? 踰꾪듉 ?앹꽦. anchor 湲곕낯媛믪? 醫뚯긽??(0,1).</summary>
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

    /// <summary>?섎떒 ?ㅻ퉬 踰꾪듉 ?앹꽦. ?듭빱 以묒븰 ?섎떒.</summary>
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


