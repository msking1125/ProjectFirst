using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 레벨 · 경험치 · 골드를 각각 분리된 영역으로 표시하는 HUD 컴포넌트.
///
/// ── Inspector 연결 가이드 ─────────────────────────────────────────────────
/// 직접 연결하거나, 비워두면 Awake에서 자식 오브젝트를 자동 탐색합니다.
///
/// [레벨]
///   Level Frame   : Image  - 레벨 숫자 뒤에 깔리는 프레임 이미지
///   Level Text    : TMP_Text - "1" "2" 형태로 표시
///
/// [경험치]
///   Exp Icon      : Image  - 경험치 아이콘 (EXP 마크)
///   Exp Gauge     : Image  - fillAmount 방식 (Image Type = Filled)
///   Exp Text      : TMP_Text - "0 / 10" (선택)
///
/// [골드]
///   Gold Icon     : Image  - 골드 코인 아이콘
///   Gold Text     : TMP_Text - "1,250" 형태로 표시
/// </summary>
public class StatusHudView : MonoBehaviour
{
    // ── 레벨 ─────────────────────────────────────────────────────────────────
    [Header("레벨")]
    [Tooltip("레벨 숫자 뒤에 표시되는 프레임 이미지 (선택)")]
    [SerializeField] private Image     levelFrame;
    [Tooltip("레벨 숫자 텍스트")]
    [SerializeField] private TMP_Text  levelText;

    // ── 경험치 ───────────────────────────────────────────────────────────────
    [Header("경험치")]
    [Tooltip("EXP 아이콘 이미지 (선택)")]
    [SerializeField] private Image     expIcon;
    [Tooltip("경험치 게이지 (Image Type: Filled, Fill Method: Horizontal)")]
    [SerializeField] private Image     expGauge;
    [Tooltip("경험치 숫자 텍스트 (0/10 형태, 선택)")]
    [SerializeField] private TMP_Text  expText;

    // ── 골드 ─────────────────────────────────────────────────────────────────
    [Header("골드")]
    [Tooltip("골드 코인 아이콘 이미지 (선택)")]
    [SerializeField] private Image     goldIcon;
    [Tooltip("골드 숫자 텍스트")]
    [SerializeField] private TMP_Text  goldText;

    // ── 자동 생성 설정 ────────────────────────────────────────────────────────
    [Header("자동 생성 설정 (Inspector 연결 없을 때)")]
    [SerializeField] private float levelFontSize  = 36f;
    [SerializeField] private float expFontSize    = 22f;
    [SerializeField] private float goldFontSize   = 28f;
    [SerializeField] private Color gaugeColor     = new Color(0.2f, 0.8f, 1f, 1f);   // 시안 계열
    [SerializeField] private Color gaugeBgColor   = new Color(0.1f, 0.1f, 0.1f, 0.7f);
    [SerializeField] private Color goldTextColor  = new Color(1f, 0.85f, 0.2f, 1f);  // 골드 색상
    [SerializeField] private Color levelTextColor = Color.white;

    // ── 공개 프로퍼티 (외부 참조용) ──────────────────────────────────────────
    public TMP_Text LevelText => levelText;
    public TMP_Text GoldText  => goldText;
    public TMP_Text ExpText   => expText;
    public Image    ExpGauge  => expGauge;

    // ────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        AutoBind();
    }

    private void AutoBind()
    {
        // 이름으로 자식 탐색 (공통 헬퍼)
        TMP_Text  FindText(string n)  => transform.Find(n)?.GetComponent<TMP_Text>();
        Image     FindImage(string n) => transform.Find(n)?.GetComponent<Image>();

        levelText  ??= FindText("LevelText")   ?? FindText("Level");
        levelFrame ??= FindImage("LevelFrame") ?? FindImage("LevelBG");
        expIcon    ??= FindImage("ExpIcon")    ?? FindImage("EXPIcon");
        expGauge   ??= FindImage("ExpGauge")   ?? FindImage("EXPGauge") ?? FindImage("ExpFill");
        expText    ??= FindText("ExpText")     ?? FindText("EXPText");
        goldIcon   ??= FindImage("GoldIcon");
        goldText   ??= FindText("GoldText")    ?? FindText("Gold");

        // 모두 없으면 코드로 자동 생성
        if (levelText == null) BuildLevelUI();
        if (expGauge  == null) BuildExpUI();
        if (goldText  == null) BuildGoldUI();
    }

    // ── 데이터 갱신 (BattleGameManager에서 호출) ─────────────────────────────

    /// <summary>
    /// 레벨, 경험치, 골드를 한 번에 갱신합니다.
    /// </summary>
    public void Refresh(int level, int exp, int expMax, int gold)
    {
        SetLevel(level);
        SetExp(exp, expMax);
        SetGold(gold);
    }

    public void SetLevel(int level)
    {
        if (levelText != null)
            levelText.text = level.ToString();
    }

    public void SetExp(int exp, int expMax)
    {
        float ratio = expMax > 0 ? Mathf.Clamp01((float)exp / expMax) : 0f;

        if (expGauge != null)
            expGauge.fillAmount = ratio;

        if (expText != null)
            expText.text = $"{exp} / {expMax}";
    }

    public void SetGold(int gold)
    {
        if (goldText != null)
            goldText.text = gold.ToString("N0"); // 1,250 형태
    }

    // ── 자동 UI 생성 ──────────────────────────────────────────────────────────

    /// <summary>
    /// 레벨 프레임 + 텍스트 자동 생성.
    /// 좌상단: 프레임 배경 Image 위에 레벨 텍스트.
    /// </summary>
    private void BuildLevelUI()
    {
        // 프레임 배경 (80×80)
        GameObject frameGo = new GameObject("LevelFrame", typeof(RectTransform), typeof(Image));
        frameGo.transform.SetParent(transform, false);
        RectTransform frameRt = frameGo.GetComponent<RectTransform>();
        frameRt.anchorMin = new Vector2(0f, 1f);
        frameRt.anchorMax = new Vector2(0f, 1f);
        frameRt.pivot     = new Vector2(0f, 1f);
        frameRt.anchoredPosition = new Vector2(10f, -10f);
        frameRt.sizeDelta = new Vector2(80f, 80f);

        levelFrame = frameGo.GetComponent<Image>();
        levelFrame.color = new Color(0.1f, 0.1f, 0.2f, 0.85f);
        levelFrame.raycastTarget = false;

        // 레벨 텍스트 (프레임 자식)
        GameObject textGo = new GameObject("LevelText", typeof(RectTransform), typeof(TextMeshProUGUI));
        textGo.transform.SetParent(frameGo.transform, false);
        RectTransform textRt = textGo.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = new Vector2(4f, 4f);
        textRt.offsetMax = new Vector2(-4f, -4f);

        levelText = textGo.GetComponent<TextMeshProUGUI>();
        levelText.text      = "1";
        levelText.fontSize  = levelFontSize;
        levelText.fontStyle = FontStyles.Bold;
        levelText.color     = levelTextColor;
        levelText.alignment = TextAlignmentOptions.Center;
        levelText.raycastTarget = false;

        // "Lv" 레이블 (프레임 상단)
        GameObject lbGo = new GameObject("LvLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
        lbGo.transform.SetParent(frameGo.transform, false);
        RectTransform lbRt = lbGo.GetComponent<RectTransform>();
        lbRt.anchorMin = new Vector2(0f, 0.65f);
        lbRt.anchorMax = new Vector2(1f, 1f);
        lbRt.offsetMin = Vector2.zero;
        lbRt.offsetMax = Vector2.zero;

        TextMeshProUGUI lbTmp = lbGo.GetComponent<TextMeshProUGUI>();
        lbTmp.text      = "Lv";
        lbTmp.fontSize  = 16f;
        lbTmp.fontStyle = FontStyles.Bold;
        lbTmp.color     = new Color(0.7f, 0.9f, 1f, 1f);
        lbTmp.alignment = TextAlignmentOptions.Center;
        lbTmp.raycastTarget = false;

        // 레벨 숫자를 하단으로
        textRt.anchorMin = new Vector2(0f, 0f);
        textRt.anchorMax = new Vector2(1f, 0.65f);
    }

    /// <summary>
    /// 경험치 게이지 + 아이콘 + 텍스트 자동 생성.
    /// 레벨 프레임 아래에 배치.
    /// </summary>
    private void BuildExpUI()
    {
        // 컨테이너 (경험치 게이지 영역)
        GameObject container = new GameObject("ExpContainer", typeof(RectTransform));
        container.transform.SetParent(transform, false);
        RectTransform cRt = container.GetComponent<RectTransform>();
        cRt.anchorMin = new Vector2(0f, 1f);
        cRt.anchorMax = new Vector2(0f, 1f);
        cRt.pivot     = new Vector2(0f, 1f);
        cRt.anchoredPosition = new Vector2(10f, -100f); // 레벨 프레임(80) + 여백(10)
        cRt.sizeDelta = new Vector2(220f, 22f);

        // 게이지 배경
        GameObject bgGo = new GameObject("ExpGaugeBG", typeof(RectTransform), typeof(Image));
        bgGo.transform.SetParent(container.transform, false);
        RectTransform bgRt = bgGo.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;
        Image bgImg = bgGo.GetComponent<Image>();
        bgImg.color = gaugeBgColor;
        bgImg.raycastTarget = false;

        // 게이지 Fill
        GameObject fillGo = new GameObject("ExpGauge", typeof(RectTransform), typeof(Image));
        fillGo.transform.SetParent(container.transform, false);
        RectTransform fillRt = fillGo.GetComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero;
        fillRt.anchorMax = Vector2.one;
        fillRt.offsetMin = new Vector2(2f, 2f);
        fillRt.offsetMax = new Vector2(-2f, -2f);

        expGauge = fillGo.GetComponent<Image>();
        expGauge.color      = gaugeColor;
        expGauge.type       = Image.Type.Filled;
        expGauge.fillMethod = Image.FillMethod.Horizontal;
        expGauge.fillAmount = 0f;
        expGauge.raycastTarget = false;

        // EXP 텍스트 (게이지 위 오버레이)
        GameObject txtGo = new GameObject("ExpText", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtGo.transform.SetParent(container.transform, false);
        RectTransform txtRt = txtGo.GetComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero;
        txtRt.anchorMax = Vector2.one;
        txtRt.offsetMin = new Vector2(4f, 0f);
        txtRt.offsetMax = new Vector2(-4f, 0f);

        expText = txtGo.GetComponent<TextMeshProUGUI>();
        expText.text      = "0 / 10";
        expText.fontSize  = expFontSize;
        expText.fontStyle = FontStyles.Bold;
        expText.color     = Color.white;
        expText.alignment = TextAlignmentOptions.Center;
        expText.raycastTarget = false;

        // EXP 라벨 (게이지 왼쪽 상단)
        GameObject expLabelGo = new GameObject("ExpLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
        expLabelGo.transform.SetParent(transform, false);
        RectTransform expLabelRt = expLabelGo.GetComponent<RectTransform>();
        expLabelRt.anchorMin = new Vector2(0f, 1f);
        expLabelRt.anchorMax = new Vector2(0f, 1f);
        expLabelRt.pivot     = new Vector2(0f, 1f);
        expLabelRt.anchoredPosition = new Vector2(10f, -92f);
        expLabelRt.sizeDelta = new Vector2(40f, 16f);

        TextMeshProUGUI expLabelTmp = expLabelGo.GetComponent<TextMeshProUGUI>();
        expLabelTmp.text      = "EXP";
        expLabelTmp.fontSize  = 14f;
        expLabelTmp.fontStyle = FontStyles.Bold;
        expLabelTmp.color     = new Color(0.6f, 0.9f, 1f, 1f);
        expLabelTmp.alignment = TextAlignmentOptions.Left;
        expLabelTmp.raycastTarget = false;
    }

    /// <summary>
    /// 골드 아이콘 + 텍스트 자동 생성.
    /// 경험치 게이지 아래에 배치.
    /// </summary>
    private void BuildGoldUI()
    {
        // 컨테이너
        GameObject container = new GameObject("GoldContainer", typeof(RectTransform));
        container.transform.SetParent(transform, false);
        RectTransform cRt = container.GetComponent<RectTransform>();
        cRt.anchorMin = new Vector2(0f, 1f);
        cRt.anchorMax = new Vector2(0f, 1f);
        cRt.pivot     = new Vector2(0f, 1f);
        cRt.anchoredPosition = new Vector2(10f, -132f); // EXP 컨테이너 아래
        cRt.sizeDelta = new Vector2(160f, 36f);

        // 골드 아이콘 (원형 노란 배경으로 대체)
        GameObject iconGo = new GameObject("GoldIcon", typeof(RectTransform), typeof(Image));
        iconGo.transform.SetParent(container.transform, false);
        RectTransform iconRt = iconGo.GetComponent<RectTransform>();
        iconRt.anchorMin = new Vector2(0f, 0f);
        iconRt.anchorMax = new Vector2(0f, 1f);
        iconRt.pivot     = new Vector2(0f, 0.5f);
        iconRt.offsetMin = Vector2.zero;
        iconRt.offsetMax = new Vector2(36f, 0f);

        goldIcon = iconGo.GetComponent<Image>();
        goldIcon.color = new Color(1f, 0.8f, 0.1f, 1f);
        goldIcon.raycastTarget = false;

        // "G" 레이블 (아이콘 위)
        GameObject gLabelGo = new GameObject("GLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
        gLabelGo.transform.SetParent(iconGo.transform, false);
        RectTransform gLabelRt = gLabelGo.GetComponent<RectTransform>();
        gLabelRt.anchorMin = Vector2.zero;
        gLabelRt.anchorMax = Vector2.one;
        gLabelRt.offsetMin = Vector2.zero;
        gLabelRt.offsetMax = Vector2.zero;

        TextMeshProUGUI gTmp = gLabelGo.GetComponent<TextMeshProUGUI>();
        gTmp.text      = "G";
        gTmp.fontSize  = 20f;
        gTmp.fontStyle = FontStyles.Bold;
        gTmp.color     = new Color(0.3f, 0.15f, 0f, 1f);
        gTmp.alignment = TextAlignmentOptions.Center;
        gTmp.raycastTarget = false;

        // 골드 텍스트
        GameObject txtGo = new GameObject("GoldText", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtGo.transform.SetParent(container.transform, false);
        RectTransform txtRt = txtGo.GetComponent<RectTransform>();
        txtRt.anchorMin = new Vector2(0f, 0f);
        txtRt.anchorMax = new Vector2(1f, 1f);
        txtRt.offsetMin = new Vector2(42f, 0f);
        txtRt.offsetMax = Vector2.zero;

        goldText = txtGo.GetComponent<TextMeshProUGUI>();
        goldText.text      = "0";
        goldText.fontSize  = goldFontSize;
        goldText.fontStyle = FontStyles.Bold;
        goldText.color     = goldTextColor;
        goldText.alignment = TextAlignmentOptions.Left;
        goldText.raycastTarget = false;
    }
}
