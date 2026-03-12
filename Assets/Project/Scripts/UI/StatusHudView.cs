using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project
{

/// <summary>
/// ?덈꺼 쨌 寃쏀뿕移?쨌 怨⑤뱶瑜?媛곴컖 遺꾨━???곸뿭?쇰줈 ?쒖떆?섎뒗 HUD 而댄룷?뚰듃.
///
/// ?? Inspector ?곌껐 媛?대뱶 ?????????????????????????????????????????????????
/// 吏곸젒 ?곌껐?섍굅?? 鍮꾩썙?먮㈃ Awake?먯꽌 ?먯떇 ?ㅻ툕?앺듃瑜??먮룞 ?먯깋?⑸땲??
///
/// [?덈꺼]
///   Level Frame   : Image  - ?덈꺼 ?レ옄 ?ㅼ뿉 源붾━???꾨젅???대?吏
///   Level Text    : TMP_Text - "1" "2" ?뺥깭濡??쒖떆
///
/// [寃쏀뿕移?
///   Exp Icon      : Image  - 寃쏀뿕移??꾩씠肄?(EXP 留덊겕)
///   Exp Gauge     : Image  - fillAmount 諛⑹떇 (Image Type = Filled)
///   Exp Text      : TMP_Text - "0 / 10" (?좏깮)
///
/// [怨⑤뱶]
///   Gold Icon     : Image  - 怨⑤뱶 肄붿씤 ?꾩씠肄?
///   Gold Text     : TMP_Text - "1,250" ?뺥깭濡??쒖떆
/// </summary>
public class StatusHudView : MonoBehaviour
{
    // ?? ?덈꺼 ?????????????????????????????????????????????????????????????????
    [Header("?덈꺼")]
    [Tooltip("?덈꺼 ?レ옄 ?ㅼ뿉 ?쒖떆?섎뒗 ?꾨젅???대?吏 (?좏깮)")]
    [SerializeField] private Image     levelFrame;
    [Tooltip("Level text")]
    [SerializeField] private TMP_Text  levelText;

    // ?? 寃쏀뿕移????????????????????????????????????????????????????????????????
    [Header("EXP")]
    [Tooltip("EXP ?꾩씠肄??대?吏 (?좏깮)")]
    [SerializeField] private Image     expIcon;
    [Tooltip("寃쏀뿕移?寃뚯씠吏 (Image Type: Filled, Fill Method: Horizontal)")]
    [SerializeField] private Image     expGauge;
    [Tooltip("寃쏀뿕移??レ옄 ?띿뒪??(0/10 ?뺥깭, ?좏깮)")]
    [SerializeField] private TMP_Text  expText;

    // ?? 怨⑤뱶 ?????????????????????????????????????????????????????????????????
    [Header("怨⑤뱶")]
    [Tooltip("怨⑤뱶 肄붿씤 ?꾩씠肄??대?吏 (?좏깮)")]
    [SerializeField] private Image     goldIcon;
    [Tooltip("Gold text")]
    [SerializeField] private TMP_Text  goldText;

    // ?? ?먮룞 ?앹꽦 ?ㅼ젙 ????????????????????????????????????????????????????????
    [Header("?먮룞 ?앹꽦 ?ㅼ젙 (Inspector ?곌껐 ?놁쓣 ??")]
    [SerializeField] private float levelFontSize  = 36f;
    [SerializeField] private float expFontSize    = 22f;
    [SerializeField] private float goldFontSize   = 28f;
    [SerializeField] private Color gaugeColor     = new Color(0.2f, 0.8f, 1f, 1f);   // ?쒖븞 怨꾩뿴
    [SerializeField] private Color gaugeBgColor   = new Color(0.1f, 0.1f, 0.1f, 0.7f);
    [SerializeField] private Color goldTextColor  = new Color(1f, 0.85f, 0.2f, 1f);  // 怨⑤뱶 ?됱긽
    [SerializeField] private Color levelTextColor = Color.white;

    // ?? 怨듦컻 ?꾨줈?쇳떚 (?몃? 李몄“?? ??????????????????????????????????????????
    public TMP_Text LevelText => levelText;
    public TMP_Text GoldText  => goldText;
    public TMP_Text ExpText   => expText;
    public Image    ExpGauge  => expGauge;

    // ????????????????????????????????????????????????????????????????????????

    private void Awake()
    {
        AutoBind();
    }

    private void AutoBind()
    {
        // ?대쫫?쇰줈 ?먯떇 ?먯깋 (怨듯넻 ?ы띁)
        TMP_Text  FindText(string n)  => transform.Find(n)?.GetComponent<TMP_Text>();
        Image     FindImage(string n) => transform.Find(n)?.GetComponent<Image>();

        levelText  ??= FindText("LevelText")   ?? FindText("Level");
        levelFrame ??= FindImage("LevelFrame") ?? FindImage("LevelBG");
        expIcon    ??= FindImage("ExpIcon")    ?? FindImage("EXPIcon");
        expGauge   ??= FindImage("ExpGauge")   ?? FindImage("EXPGauge") ?? FindImage("ExpFill");
        expText    ??= FindText("ExpText")     ?? FindText("EXPText");
        goldIcon   ??= FindImage("GoldIcon");
        goldText   ??= FindText("GoldText")    ?? FindText("Gold");

        // 紐⑤몢 ?놁쑝硫?肄붾뱶濡??먮룞 ?앹꽦
        if (levelText == null) BuildLevelUI();
        if (expGauge  == null) BuildExpUI();
        if (goldText  == null) BuildGoldUI();
    }

    // ?? ?곗씠??媛깆떊 (BattleGameManager?먯꽌 ?몄텧) ?????????????????????????????

    /// <summary>
    /// ?덈꺼, 寃쏀뿕移? 怨⑤뱶瑜???踰덉뿉 媛깆떊?⑸땲??
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
            goldText.text = gold.ToString("N0"); // 1,250 ?뺥깭
    }

    // ?? ?먮룞 UI ?앹꽦 ??????????????????????????????????????????????????????????

    /// <summary>
    /// ?덈꺼 ?꾨젅??+ ?띿뒪???먮룞 ?앹꽦.
    /// 醫뚯긽?? ?꾨젅??諛곌꼍 Image ?꾩뿉 ?덈꺼 ?띿뒪??
    /// </summary>
    private void BuildLevelUI()
    {
        // ?꾨젅??諛곌꼍 (80횞80)
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

        // ?덈꺼 ?띿뒪??(?꾨젅???먯떇)
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

        // "Lv" ?덉씠釉?(?꾨젅???곷떒)
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

        // ?덈꺼 ?レ옄瑜??섎떒?쇰줈
        textRt.anchorMin = new Vector2(0f, 0f);
        textRt.anchorMax = new Vector2(1f, 0.65f);
    }

    /// <summary>
    /// 寃쏀뿕移?寃뚯씠吏 + ?꾩씠肄?+ ?띿뒪???먮룞 ?앹꽦.
    /// ?덈꺼 ?꾨젅???꾨옒??諛곗튂.
    /// </summary>
    private void BuildExpUI()
    {
        // 而⑦뀒?대꼫 (寃쏀뿕移?寃뚯씠吏 ?곸뿭)
        GameObject container = new GameObject("ExpContainer", typeof(RectTransform));
        container.transform.SetParent(transform, false);
        RectTransform cRt = container.GetComponent<RectTransform>();
        cRt.anchorMin = new Vector2(0f, 1f);
        cRt.anchorMax = new Vector2(0f, 1f);
        cRt.pivot     = new Vector2(0f, 1f);
        cRt.anchoredPosition = new Vector2(10f, -100f); // ?덈꺼 ?꾨젅??80) + ?щ갚(10)
        cRt.sizeDelta = new Vector2(220f, 22f);

        // 寃뚯씠吏 諛곌꼍
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

        // 寃뚯씠吏 Fill
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

        // EXP ?띿뒪??(寃뚯씠吏 ???ㅻ쾭?덉씠)
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

        // EXP ?쇰꺼 (寃뚯씠吏 ?쇱そ ?곷떒)
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
    /// 怨⑤뱶 ?꾩씠肄?+ ?띿뒪???먮룞 ?앹꽦.
    /// 寃쏀뿕移?寃뚯씠吏 ?꾨옒??諛곗튂.
    /// </summary>
    private void BuildGoldUI()
    {
        // 而⑦뀒?대꼫
        GameObject container = new GameObject("GoldContainer", typeof(RectTransform));
        container.transform.SetParent(transform, false);
        RectTransform cRt = container.GetComponent<RectTransform>();
        cRt.anchorMin = new Vector2(0f, 1f);
        cRt.anchorMax = new Vector2(0f, 1f);
        cRt.pivot     = new Vector2(0f, 1f);
        cRt.anchoredPosition = new Vector2(10f, -132f); // EXP 而⑦뀒?대꼫 ?꾨옒
        cRt.sizeDelta = new Vector2(160f, 36f);

        // 怨⑤뱶 ?꾩씠肄?(?먰삎 ?몃? 諛곌꼍?쇰줈 ?泥?
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

        // "G" ?덉씠釉?(?꾩씠肄???
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

        // 怨⑤뱶 ?띿뒪??
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
} // namespace Project

