using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;

/// <summary>
/// 타이틀 씬 UI 관리자 (Cyberpunk Dark Neon Style - 9:16 모바일 최적화)
/// 빌드 시 버튼이 비정상적으로 출력/작동하는 문제를 우회 및 방지하도록 수정.
/// 빌드 시 버튼이 비정상적으로 출력/작동하는 문제를 우회 및 방지하도록 수정.
/// (공통 Sprite 사용 및 런타임 생성 시 빌드 호환성 보장)
/// </summary>
using ProjectFirst.OutGame;

public class TitleManager : MonoBehaviour
{
    public static TitleManager Instance { get; private set; }
    private const string PrefKeyBgmVolume = "setting.sound.bgm";
    private const string PrefKeySfxVolume = "setting.sound.sfx";
    private const string PrefKeyMute = "setting.sound.mute";

    [Header("Scene Settings")]
    [SerializeField] private string gameSceneName = "Lobby";

    [Header("UI Texts")]
    [SerializeField] private TMP_FontAsset customFont;
    [SerializeField] private string startButtonText = "게임 시작";
    [SerializeField] private string serverSelectButtonText = "서버 선택";
    [SerializeField] private string settingsButtonText = "설정";

    [Header("Events (Optional)")]
    [SerializeField] private VoidEventChannelSO startButtonEvent;
    [SerializeField] private VoidEventChannelSO settingsButtonEvent;
    [SerializeField] private VoidEventChannelSO quitButtonEvent;

    [Header("Button Sprite (빌드 환경도 지원)")]
    [Tooltip("유니티 빌드에도 포함되는 Sprite. 반드시 Sprite(2D)/UI/Default 임포트 타입이어야 함.")]
    [SerializeField] private Sprite builtinButtonSprite;

    private Canvas targetCanvas;

    // 런타임 설정 패널(기존 SettingPanel의 PlayerPrefs 키/동작을 타이틀에서도 동일 적용)
    private GameObject settingsPanel;
    private Slider bgmSlider;
    private Slider sfxSlider;
    private Toggle muteToggle;
    private TMP_Text bgmValueText;
    private TMP_Text sfxValueText;

    private GameObject buttonGroup;
    private GameObject settingsBtnGo;

    private void Awake()
    {
        Instance = this;
        EnsureCanvasAndUI();
    }

    private void EnsureCanvasAndUI()
    {
        // 최상단 Canvas 찾기
        targetCanvas = FindObjectOfType<Canvas>();

        if (targetCanvas == null)
        {
            GameObject canvasObj = new GameObject("TitleCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            targetCanvas = canvasObj.GetComponent<Canvas>();
            targetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f); // 9:16 모바일 기준
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f; // 너비/높이 중간 매칭

            // UI 생성
            BuildUI();
        }
        else
        {
            if (targetCanvas.transform.Find("CyberpunkOverlay") == null)
            {
                CanvasScaler scaler = targetCanvas.GetComponent<CanvasScaler>();
                if (scaler != null)
                {
                    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    scaler.referenceResolution = new Vector2(1080f, 1920f);
                    scaler.matchWidthOrHeight = 0.5f;
                }
                BuildUI();
            }
        }

        // TitleView.uxml UIDocument만 비활성화 — LoginUI 등 타 UIDocument는 건드리지 않음
        var ownUIDoc = GetComponent<UnityEngine.UIElements.UIDocument>();
        if (ownUIDoc != null) ownUIDoc.enabled = false;

        // 유니티 UI(uGUI)의 버튼 클릭 이벤트를 처리하기 위해서는 EventSystem이 필수적입니다.
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));
        }
    }

    private void BuildUI()
    {
        // 1. 다크 오버레이
        GameObject overlay = new GameObject("CyberpunkOverlay", typeof(RectTransform), typeof(Image));
        overlay.transform.SetParent(targetCanvas.transform, false);
        SetFullScreenStretch(overlay.GetComponent<RectTransform>());
        Image overlayImg = overlay.GetComponent<Image>();
        overlayImg.color = new Color(0.02f, 0.02f, 0.05f, 0.75f); // 딥 다크 블루 + 반투명

        // 2. 로고 영역 생성 (상단 중심)
        GameObject logoRoot = new GameObject("LogoRoot", typeof(RectTransform));
        logoRoot.transform.SetParent(targetCanvas.transform, false);
        RectTransform logoRt = logoRoot.GetComponent<RectTransform>();
        logoRt.anchorMin = new Vector2(0.5f, 0.7f);
        logoRt.anchorMax = new Vector2(0.5f, 0.7f);
        logoRt.anchoredPosition = Vector2.zero;

        // 3. 메인 버튼 레이아웃 컨테이너 (시작, 서버선택) - 로그인 완료 전까지 숨김
        buttonGroup = new GameObject("ButtonGroup", typeof(RectTransform));
        buttonGroup.transform.SetParent(targetCanvas.transform, false);
        RectTransform groupRt = buttonGroup.GetComponent<RectTransform>();
        groupRt.anchorMin = new Vector2(0.5f, 0.2f);
        groupRt.anchorMax = new Vector2(0.5f, 0.2f);
        groupRt.anchoredPosition = Vector2.zero;
        buttonGroup.SetActive(false); // 처음에는 숨김

        // 버튼 생성 (Sprite가 할당되지 않으면 사용자에게 경고)
        if (builtinButtonSprite == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning("[TitleManager] 'builtinButtonSprite'가 할당되지 않았습니다. Button이 정상 출력되지 않을 수 있습니다.\n" +
                "최소한 UI/Skin/UISprite 등 빌드 포함되는 Sprite를 수동 할당해야 합니다.");
#endif
        }

        CreateCyberpunkButton("Btn_Start", buttonGroup.transform, startButtonText, new Vector2(0f, 80f), new Color(0.1f, 0.5f, 0.9f, 0.9f), OnStartClicked);
        CreateCyberpunkButton("Btn_ServerSelect", buttonGroup.transform, serverSelectButtonText, new Vector2(0f, -80f), new Color(0.1f, 0.8f, 0.5f, 0.9f), OnServerSelectClicked);

        // 4. 설정 버튼 (우측 상단) - 로그인 완료 전까지 숨김
        settingsBtnGo = new GameObject("Btn_Settings", typeof(RectTransform));
        settingsBtnGo.transform.SetParent(targetCanvas.transform, false);
        RectTransform setRt = settingsBtnGo.GetComponent<RectTransform>();
        setRt.anchorMin = new Vector2(1f, 1f);
        setRt.anchorMax = new Vector2(1f, 1f);
        setRt.pivot = new Vector2(1f, 1f);
        setRt.anchoredPosition = new Vector2(-40f, -40f);
        // 로그인과 무관하게 설정은 항상 접근 가능
        settingsBtnGo.SetActive(true);

        CreateCyberpunkButton("Btn_Settings_Inner", settingsBtnGo.transform, settingsButtonText, Vector2.zero, new Color(0.3f, 0.3f, 0.35f, 0.9f), OnSettingsClicked, new Vector2(250f, 100f), 30f);

        BuildSettingsPanel();
    }

    public void ShowTitleButtons()
    {
        if (buttonGroup != null) buttonGroup.SetActive(true);
        if (settingsBtnGo != null) settingsBtnGo.SetActive(true);
    }

    private void BuildSettingsPanel()
    {
        settingsPanel = new GameObject("SettingPanel_Runtime", typeof(RectTransform), typeof(Image));
        settingsPanel.transform.SetParent(targetCanvas.transform, false);

        RectTransform panelRt = settingsPanel.GetComponent<RectTransform>();
        SetFullScreenStretch(panelRt);
        Image panelDim = settingsPanel.GetComponent<Image>();
        panelDim.color = new Color(0f, 0f, 0f, 0.68f);

        GameObject window = new GameObject("Window", typeof(RectTransform), typeof(Image));
        window.transform.SetParent(settingsPanel.transform, false);
        RectTransform windowRt = window.GetComponent<RectTransform>();
        windowRt.anchorMin = windowRt.anchorMax = new Vector2(0.5f, 0.5f);
        windowRt.sizeDelta = new Vector2(860f, 840f);
        window.GetComponent<Image>().color = new Color(0.08f, 0.1f, 0.16f, 0.95f);

        CreatePanelLabel(window.transform, "Title", "설정", new Vector2(0f, 345f), 56f, true);

        CreatePanelLabel(window.transform, "BgmLabel", "BGM", new Vector2(-280f, 190f), 38f, true, TextAlignmentOptions.MidlineLeft);
        bgmSlider = CreatePanelSlider(window.transform, "BgmSlider", new Vector2(90f, 190f));
        bgmValueText = CreatePanelLabel(window.transform, "BgmValue", "80", new Vector2(335f, 190f), 34f, true);

        CreatePanelLabel(window.transform, "SfxLabel", "SFX", new Vector2(-280f, 95f), 38f, true, TextAlignmentOptions.MidlineLeft);
        sfxSlider = CreatePanelSlider(window.transform, "SfxSlider", new Vector2(90f, 95f));
        sfxValueText = CreatePanelLabel(window.transform, "SfxValue", "80", new Vector2(335f, 95f), 34f, true);

        muteToggle = CreatePanelToggle(window.transform, "MuteToggle", "음소거", new Vector2(0f, -20f));

        Button closeButton = CreatePanelButton(window.transform, "CloseButton", "닫기", new Vector2(0f, -300f), new Vector2(340f, 100f));
        closeButton.onClick.AddListener(CloseSettingsPanel);

        if (bgmSlider != null)
        {
            bgmSlider.minValue = 0f;
            bgmSlider.maxValue = 100f;
            bgmSlider.wholeNumbers = true;
            bgmSlider.onValueChanged.AddListener(OnBgmChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.minValue = 0f;
            sfxSlider.maxValue = 100f;
            sfxSlider.wholeNumbers = true;
            sfxSlider.onValueChanged.AddListener(OnSfxChanged);
        }

        if (muteToggle != null)
            muteToggle.onValueChanged.AddListener(OnMuteChanged);

        settingsPanel.SetActive(false);
    }

    private void CreateCyberpunkButton(string objName, Transform parent, string label, Vector2 pos, Color neonColor, UnityEngine.Events.UnityAction onClick, Vector2? customSize = null, float fontSize = 40f)
    {
        // 버튼 본체
        GameObject btnGo = new GameObject(objName, typeof(RectTransform), typeof(Image), typeof(Button), typeof(TitleUIButton), typeof(Shadow));
        btnGo.transform.SetParent(parent, false);

        RectTransform btnRt = btnGo.GetComponent<RectTransform>();
        btnRt.sizeDelta = customSize ?? new Vector2(760f, 110f);
        btnRt.anchoredPosition = pos;

        Image btnImg = btnGo.GetComponent<Image>();

        // 유니티 에디터에서는 editor용, 빌드에서는 Assign된 Sprite 사용
        Sprite useSprite = builtinButtonSprite;
#if UNITY_EDITOR
        if (useSprite == null)
            useSprite = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
#endif
        btnImg.sprite = useSprite;
        btnImg.type = (btnImg.sprite != null) ? Image.Type.Sliced : Image.Type.Simple;
        btnImg.color = neonColor;

        // 그림자 글로우
        Shadow cyberShadow = btnGo.GetComponent<Shadow>();
        cyberShadow.effectColor = new Color(neonColor.r, neonColor.g, neonColor.b, 0.5f);
        cyberShadow.effectDistance = new Vector2(4f, -4f);

        Button btn = btnGo.GetComponent<Button>();
        btn.onClick.AddListener(onClick);

        // 데코 바
        GameObject decoLine = new GameObject("DecoLine", typeof(RectTransform), typeof(Image));
        decoLine.transform.SetParent(btnGo.transform, false);
        RectTransform decoRt = decoLine.GetComponent<RectTransform>();
        decoRt.anchorMin = new Vector2(0f, 0f);
        decoRt.anchorMax = new Vector2(0f, 1f);
        decoRt.pivot = new Vector2(0f, 0.5f);
        decoRt.sizeDelta = new Vector2(10f, 0f);
        decoLine.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.8f);

        // 버튼 텍스트
        GameObject txtGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtGo.transform.SetParent(btnGo.transform, false);
        SetFullScreenStretch(txtGo.GetComponent<RectTransform>());

        TextMeshProUGUI tmp = txtGo.GetComponent<TextMeshProUGUI>();
        tmp.text = label.Replace(" ", "<space=0.4em>");

        if (customFont != null) tmp.font = customFont;
        tmp.fontSize = fontSize;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.verticalAlignment = VerticalAlignmentOptions.Middle;
        tmp.enableWordWrapping = false;
        tmp.enableAutoSizing = false;
        tmp.overflowMode = TextOverflowModes.Overflow;
    }

    private TMP_Text CreatePanelLabel(Transform parent, string name, string text, Vector2 pos, float size, bool bold,
        TextAlignmentOptions alignment = TextAlignmentOptions.Center)
    {
        GameObject labelGo = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        labelGo.transform.SetParent(parent, false);
        RectTransform rt = labelGo.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(760f, 64f);

        TextMeshProUGUI tmp = labelGo.GetComponent<TextMeshProUGUI>();
        if (customFont != null) tmp.font = customFont;
        tmp.text = text;
        tmp.fontSize = size;
        tmp.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
        tmp.color = Color.white;
        tmp.alignment = alignment;
        tmp.enableWordWrapping = false;
        return tmp;
    }

    private Slider CreatePanelSlider(Transform parent, string name, Vector2 pos)
    {
        GameObject sliderGo = new GameObject(name, typeof(RectTransform), typeof(Slider));
        sliderGo.transform.SetParent(parent, false);
        RectTransform sliderRt = sliderGo.GetComponent<RectTransform>();
        sliderRt.anchorMin = sliderRt.anchorMax = new Vector2(0.5f, 0.5f);
        sliderRt.anchoredPosition = pos;
        sliderRt.sizeDelta = new Vector2(430f, 36f);

        GameObject bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bg.transform.SetParent(sliderGo.transform, false);
        SetFullScreenStretch(bg.GetComponent<RectTransform>());
        bg.GetComponent<Image>().color = new Color(0.2f, 0.25f, 0.35f, 1f);

        GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(sliderGo.transform, false);
        SetFullScreenStretch(fillArea.GetComponent<RectTransform>());

        GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fill.transform.SetParent(fillArea.transform, false);
        SetFullScreenStretch(fill.GetComponent<RectTransform>());
        fill.GetComponent<Image>().color = new Color(0.1f, 0.65f, 1f, 1f);

        GameObject handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
        handleArea.transform.SetParent(sliderGo.transform, false);
        SetFullScreenStretch(handleArea.GetComponent<RectTransform>());

        GameObject handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
        handle.transform.SetParent(handleArea.transform, false);
        RectTransform handleRt = handle.GetComponent<RectTransform>();
        handleRt.sizeDelta = new Vector2(30f, 48f);
        handle.GetComponent<Image>().color = Color.white;

        Slider slider = sliderGo.GetComponent<Slider>();
        slider.fillRect = fill.GetComponent<RectTransform>();
        slider.handleRect = handleRt;
        slider.targetGraphic = handle.GetComponent<Image>();
        slider.direction = Slider.Direction.LeftToRight;

        return slider;
    }

    private Toggle CreatePanelToggle(Transform parent, string name, string label, Vector2 pos)
    {
        GameObject toggleGo = new GameObject(name, typeof(RectTransform), typeof(Toggle));
        toggleGo.transform.SetParent(parent, false);
        RectTransform rt = toggleGo.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(300f, 60f);

        GameObject bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bg.transform.SetParent(toggleGo.transform, false);
        RectTransform bgRt = bg.GetComponent<RectTransform>();
        bgRt.anchorMin = new Vector2(0f, 0.5f);
        bgRt.anchorMax = new Vector2(0f, 0.5f);
        bgRt.sizeDelta = new Vector2(40f, 40f);
        bgRt.anchoredPosition = new Vector2(30f, 0f);
        bg.GetComponent<Image>().color = new Color(0.15f, 0.2f, 0.3f, 1f);

        GameObject check = new GameObject("Checkmark", typeof(RectTransform), typeof(Image));
        check.transform.SetParent(bg.transform, false);
        SetFullScreenStretch(check.GetComponent<RectTransform>());
        check.GetComponent<Image>().color = new Color(0.1f, 0.8f, 0.95f, 1f);

        CreatePanelLabel(toggleGo.transform, "Label", label, new Vector2(80f, 0f), 34f, true, TextAlignmentOptions.MidlineLeft);

        Toggle toggle = toggleGo.GetComponent<Toggle>();
        toggle.graphic = check.GetComponent<Image>();
        toggle.targetGraphic = bg.GetComponent<Image>();
        return toggle;
    }

    private Button CreatePanelButton(Transform parent, string name, string text, Vector2 pos, Vector2 size)
    {
        GameObject btnGo = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        btnGo.transform.SetParent(parent, false);

        RectTransform rt = btnGo.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        btnGo.GetComponent<Image>().color = new Color(0.12f, 0.55f, 0.9f, 0.95f);
        CreatePanelLabel(btnGo.transform, "Text", text, Vector2.zero, 36f, true);

        return btnGo.GetComponent<Button>();
    }

    private void SetFullScreenStretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    // ── 버튼 핸들러 ──────────────────────────────────────────

    private void OnStartClicked()
    {
        Debug.Log("[TitleManager] 게임 시작 클릭 -> LoginManager.Instance.ConnectToSelectedServer() 실행");
        startButtonEvent?.RaiseEvent();
        LoginManager.Instance.ConnectToSelectedServer();
    }

    private void OnServerSelectClicked()
    {
        Debug.Log("[TitleManager] 서버 선택 클릭");
        LoginManager.Instance.ShowServerSelectPopup();
    }

    private void OnSettingsClicked()
    {
        Debug.Log("[TitleManager] 설정 클릭");
        settingsButtonEvent?.RaiseEvent();
        OpenSettingsPanel();
    }

    private void OpenSettingsPanel()
    {
        if (settingsPanel == null)
            return;

        LoadSettingPrefs();
        settingsPanel.SetActive(true);
    }

    private void CloseSettingsPanel()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    private void LoadSettingPrefs()
    {
        int bgm = PlayerPrefs.GetInt(PrefKeyBgmVolume, 80);
        int sfx = PlayerPrefs.GetInt(PrefKeySfxVolume, 80);
        bool mute = PlayerPrefs.GetInt(PrefKeyMute, 0) == 1;

        if (bgmSlider != null) bgmSlider.SetValueWithoutNotify(Mathf.Clamp(bgm, 0, 100));
        if (sfxSlider != null) sfxSlider.SetValueWithoutNotify(Mathf.Clamp(sfx, 0, 100));
        if (muteToggle != null) muteToggle.SetIsOnWithoutNotify(mute);

        RefreshSoundTexts();
        AudioListener.pause = mute;
    }

    private void OnBgmChanged(float value)
    {
        PlayerPrefs.SetInt(PrefKeyBgmVolume, Mathf.RoundToInt(value));
        PlayerPrefs.Save();
        RefreshSoundTexts();
    }

    private void OnSfxChanged(float value)
    {
        PlayerPrefs.SetInt(PrefKeySfxVolume, Mathf.RoundToInt(value));
        PlayerPrefs.Save();
        RefreshSoundTexts();
    }

    private void OnMuteChanged(bool isMuted)
    {
        PlayerPrefs.SetInt(PrefKeyMute, isMuted ? 1 : 0);
        PlayerPrefs.Save();
        AudioListener.pause = isMuted;
    }

    private void RefreshSoundTexts()
    {
        if (bgmValueText != null && bgmSlider != null)
            bgmValueText.text = Mathf.RoundToInt(bgmSlider.value).ToString();

        if (sfxValueText != null && sfxSlider != null)
            sfxValueText.text = Mathf.RoundToInt(sfxSlider.value).ToString();
    }

    // ── Public/Async 메서드 ─────────────────────────────────────────

    /// <summary>
    /// Addressables 기반 Async 씬 로딩 (빌드/에디터 모두 지원)
    /// </summary>
    private async UniTaskVoid LoadGameSceneAsync()
    {
        if (string.IsNullOrEmpty(gameSceneName))
        {
            Debug.LogError("[TitleManager] gameSceneName이 비어 있습니다.");
            return;
        }

        // Addressables로 씬 비동기 로드 (빌드 세팅 미포함/번들 문제 방지)
        var handle = Addressables.LoadSceneAsync(gameSceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
        while (!handle.IsDone)
        {
            // UI 업데이트하려면 진행 바 노출 추가 가능
            await UniTask.Yield();
        }
        // 로딩 완료 후 UI 등 처리 위치 (별도 화면 없음)
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
