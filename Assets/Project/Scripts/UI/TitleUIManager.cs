using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

/// <summary>
/// 타이틀 UI 생성 및 관리 클래스
/// TitleManager에서 UI 생성 로직을 분리
/// </summary>
public class TitleUIManager : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private TMP_FontAsset customFont;
    [SerializeField] private Sprite builtinButtonSprite;

    private Canvas targetCanvas;
    private GameObject buttonGroup;
    private GameObject settingsBtnGo;

    // 버튼 이벤트 핸들러들
    public UnityAction OnStartClicked { get; set; }
    public UnityAction OnServerSelectClicked { get; set; }
    public UnityAction OnSettingsClicked { get; set; }

    public void Initialize()
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
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            scaler.matchWidthOrHeight = 0.5f;

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
                    scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
                    scaler.matchWidthOrHeight = 0.5f;
                }
                BuildUI();
            }
        }

        // EventSystem 확인
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
        overlayImg.color = new Color(0.02f, 0.02f, 0.05f, 0.75f);
        overlayImg.raycastTarget = false;

        // 2. 로고 영역 (선택사항)
        GameObject logoRoot = new GameObject("LogoRoot", typeof(RectTransform));
        logoRoot.transform.SetParent(targetCanvas.transform, false);
        RectTransform logoRt = logoRoot.GetComponent<RectTransform>();
        logoRt.anchorMin = new Vector2(0.5f, 0.7f);
        logoRt.anchorMax = new Vector2(0.5f, 0.7f);
        logoRt.anchoredPosition = Vector2.zero;

        // 3. 메인 버튼 그룹
        CreateButtonGroup();

        // 4. 설정 버튼
        CreateSettingsButton();
    }

    private void CreateButtonGroup()
    {
        buttonGroup = new GameObject("ButtonGroup", typeof(RectTransform), typeof(VerticalLayoutGroup));
        buttonGroup.transform.SetParent(targetCanvas.transform, false);
        RectTransform groupRt = buttonGroup.GetComponent<RectTransform>();
        groupRt.anchorMin = new Vector2(0.5f, 0.15f);
        groupRt.anchorMax = new Vector2(0.5f, 0.15f);
        groupRt.anchoredPosition = Vector2.zero;
        groupRt.sizeDelta = new Vector2(800f, 300f);

        VerticalLayoutGroup layoutGroup = buttonGroup.GetComponent<VerticalLayoutGroup>();
        layoutGroup.spacing = 20f;
        layoutGroup.childAlignment = TextAnchor.MiddleCenter;
        layoutGroup.childControlWidth = true;
        layoutGroup.childControlHeight = true;

        buttonGroup.SetActive(false);
    }

    private void CreateSettingsButton()
    {
        settingsBtnGo = new GameObject("Btn_Settings", typeof(RectTransform));
        settingsBtnGo.transform.SetParent(targetCanvas.transform, false);
        RectTransform setRt = settingsBtnGo.GetComponent<RectTransform>();
        setRt.anchorMin = new Vector2(1f, 1f);
        setRt.anchorMax = new Vector2(1f, 1f);
        setRt.pivot = new Vector2(1f, 1f);
        setRt.anchoredPosition = new Vector2(-40f, -40f);

        CreateCyberpunkButton("Btn_Settings_Inner", settingsBtnGo.transform, "설정", Vector2.zero,
            new Color(0.3f, 0.3f, 0.35f, 0.9f), OnSettingsClicked, new Vector2(200f, 80f), 30f);

        settingsBtnGo.SetActive(true);
    }

    public void ShowTitleButtons()
    {
        // 런타임 UI 버튼 표시 (기존 방식 유지)
        if (buttonGroup != null)
        {
            // 버튼들이 없으면 생성
            if (buttonGroup.transform.childCount == 0)
            {
                CreateCyberpunkButton("Btn_ServerSelect", buttonGroup.transform, "서버 선택", Vector2.zero,
                    new Color(0.1f, 0.8f, 0.5f, 0.9f), OnServerSelectClicked);
                CreateCyberpunkButton("Btn_Start", buttonGroup.transform, "게임 시작", Vector2.zero,
                    new Color(0.1f, 0.5f, 0.9f, 0.9f), OnStartClicked);
            }
            buttonGroup.SetActive(true);
        }

        // UXML UI도 활성화 (선택사항)
        var uiDoc = FindObjectOfType<UnityEngine.UIElements.UIDocument>();
        if (uiDoc != null && uiDoc.gameObject.name.Contains("Title"))
        {
            uiDoc.enabled = true;
        }
    }

    public void HideTitleButtons()
    {
        if (buttonGroup != null) buttonGroup.SetActive(false);
    }

    private void CreateCyberpunkButton(string objName, Transform parent, string label, Vector2 pos, Color neonColor,
        UnityAction onClick, Vector2? customSize = null, float fontSize = 35f)
    {
        GameObject btnGo = new GameObject(objName, typeof(RectTransform), typeof(Image), typeof(Button), typeof(TitleUIButton), typeof(Shadow));
        btnGo.transform.SetParent(parent, false);

        RectTransform btnRt = btnGo.GetComponent<RectTransform>();
        btnRt.sizeDelta = customSize ?? new Vector2(600f, 80f);
        btnRt.anchoredPosition = pos;

        Image btnImg = btnGo.GetComponent<Image>();
        Sprite useSprite = builtinButtonSprite;
#if UNITY_EDITOR
        if (useSprite == null)
            useSprite = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
#endif
        btnImg.sprite = useSprite;
        btnImg.type = (btnImg.sprite != null) ? Image.Type.Sliced : Image.Type.Simple;
        btnImg.color = neonColor;

        Shadow cyberShadow = btnGo.GetComponent<Shadow>();
        cyberShadow.effectColor = new Color(neonColor.r, neonColor.g, neonColor.b, 0.5f);
        cyberShadow.effectDistance = new Vector2(4f, -4f);

        Button btn = btnGo.GetComponent<Button>();
        btn.onClick.AddListener(onClick);

        // 데코 라인
        GameObject decoLine = new GameObject("DecoLine", typeof(RectTransform), typeof(Image));
        decoLine.transform.SetParent(btnGo.transform, false);
        RectTransform decoRt = decoLine.GetComponent<RectTransform>();
        decoRt.anchorMin = new Vector2(0f, 0f);
        decoRt.anchorMax = new Vector2(0f, 1f);
        decoRt.pivot = new Vector2(0f, 0.5f);
        decoRt.sizeDelta = new Vector2(8f, 0f);
        decoLine.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.8f);

        // 텍스트
        GameObject txtGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtGo.transform.SetParent(btnGo.transform, false);
        SetFullScreenStretch(txtGo.GetComponent<RectTransform>());

        TextMeshProUGUI tmp = txtGo.GetComponent<TextMeshProUGUI>();
        tmp.text = label;
        if (customFont != null) tmp.font = customFont;
        tmp.fontSize = fontSize;
        tmp.fontSizeMin = 20f;
        tmp.fontSizeMax = fontSize;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.verticalAlignment = VerticalAlignmentOptions.Middle;
        tmp.enableWordWrapping = false;
        tmp.enableAutoSizing = true;
        tmp.overflowMode = TextOverflowModes.Overflow;
    }

    private void SetFullScreenStretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}