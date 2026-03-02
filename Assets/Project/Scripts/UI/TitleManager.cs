using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// 타이틀 씬 UI 관리자 (Cyberpunk Dark Neon Style - 9:16 모바일 최적화)
/// 빌드 시 버튼이 비정상적으로 출력/작동하는 문제를 우회 및 방지하도록 수정.
/// (공통 Sprite 사용 및 런타임 생성 시 빌드 호환성 보장)
/// </summary>
public class TitleManager : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string gameSceneName = "Battle_Test";

    [Header("UI Texts")]
    [SerializeField] private TMP_FontAsset customFont;
    [SerializeField] private string startButtonText = "게임 시작";
    [SerializeField] private string settingsButtonText = "설정";
    [SerializeField] private string quitButtonText = "종료";

    [Header("Events (Optional)")]
    [SerializeField] private VoidEventChannelSO startButtonEvent;
    [SerializeField] private VoidEventChannelSO settingsButtonEvent;
    [SerializeField] private VoidEventChannelSO quitButtonEvent;

    [Header("Button Sprite (빌드 환경도 지원)")]
    [Tooltip("유니티 빌드에도 포함되는 Sprite. 반드시 Sprite(2D)/UI/Default 임포트 타입이어야 함.")]
    [SerializeField] private Sprite builtinButtonSprite;

    private Canvas targetCanvas;

    private void Awake()
    {
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

        // Hide existing UIDocument if present to remove the old UI at the bottom
        var uiDocuments = FindObjectsOfType<UnityEngine.UIElements.UIDocument>();
        foreach (var uiDoc in uiDocuments)
        {
            if (uiDoc != null)
            {
                uiDoc.gameObject.SetActive(false);
            }
        }

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

        // 3. 버튼 레이아웃 컨테이너
        GameObject buttonGroup = new GameObject("ButtonGroup", typeof(RectTransform));
        buttonGroup.transform.SetParent(targetCanvas.transform, false);
        RectTransform groupRt = buttonGroup.GetComponent<RectTransform>();
        groupRt.anchorMin = new Vector2(0.5f, 0.2f);
        groupRt.anchorMax = new Vector2(0.5f, 0.2f);
        groupRt.anchoredPosition = Vector2.zero;

        // 버튼 생성 (Sprite가 할당되지 않으면 사용자에게 경고)
        if (builtinButtonSprite == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning("[TitleManager] 'builtinButtonSprite'가 할당되지 않았습니다. Button이 정상 출력되지 않을 수 있습니다.\n" +
                "최소한 UI/Skin/UISprite 등 빌드 포함되는 Sprite를 수동 할당해야 합니다.");
#endif
        }

        CreateCyberpunkButton("Btn_Start", buttonGroup.transform, startButtonText, new Vector2(0f, 160f), new Color(0.1f, 0.5f, 0.9f, 0.9f), OnStartClicked);
        CreateCyberpunkButton("Btn_Settings", buttonGroup.transform, settingsButtonText, new Vector2(0f, 0f), new Color(0.3f, 0.3f, 0.35f, 0.9f), OnSettingsClicked);
        CreateCyberpunkButton("Btn_Quit", buttonGroup.transform, quitButtonText, new Vector2(0f, -160f), new Color(0.8f, 0.2f, 0.3f, 0.9f), OnQuitClicked);
    }

    private void CreateCyberpunkButton(string objName, Transform parent, string label, Vector2 pos, Color neonColor, UnityEngine.Events.UnityAction onClick)
    {
        // 버튼 본체
        GameObject btnGo = new GameObject(objName, typeof(RectTransform), typeof(Image), typeof(Button), typeof(TitleUIButton), typeof(Shadow));
        btnGo.transform.SetParent(parent, false);

        RectTransform btnRt = btnGo.GetComponent<RectTransform>();
        btnRt.sizeDelta = new Vector2(760f, 110f);
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
        tmp.fontSize = 40f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.verticalAlignment = VerticalAlignmentOptions.Middle;
        tmp.enableWordWrapping = false;
        tmp.enableAutoSizing = false;
        tmp.overflowMode = TextOverflowModes.Overflow;

        // (빌드 환경에서 버튼이 안 눌리면 GraphicRaycaster/Canvas 설정 확인 필요)
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
        Debug.Log("[TitleManager] 게임 시작 클릭 -> AsyncSceneLoader 실행");
        startButtonEvent?.RaiseEvent();
        LoadGameScene();
    }

    private void OnSettingsClicked()
    {
        Debug.Log("[TitleManager] 설정 클릭 (현재 더미 로그)");
        settingsButtonEvent?.RaiseEvent();
    }

    private void OnQuitClicked()
    {
        Debug.Log("[TitleManager] 종료 클릭");
        quitButtonEvent?.RaiseEvent();
        QuitGame();
    }

    // ── Public 메서드 ─────────────────────────────────────────

    public void LoadGameScene()
    {
        if (string.IsNullOrEmpty(gameSceneName))
        {
            Debug.LogError("[TitleManager] gameSceneName이 비어 있습니다.");
            return;
        }

        SceneManager.LoadScene(gameSceneName);
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
