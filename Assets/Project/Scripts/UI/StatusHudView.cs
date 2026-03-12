using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project
{

/// <summary>
/// Documentation cleaned.
///
/// Documentation cleaned.
/// Documentation cleaned.
///
/// Documentation cleaned.
/// Documentation cleaned.
/// Documentation cleaned.
///
/// Documentation cleaned.
/// Documentation cleaned.
/// Documentation cleaned.
/// Documentation cleaned.
///
/// Documentation cleaned.
/// Documentation cleaned.
/// Documentation cleaned.
/// </summary>
public class StatusHudView : MonoBehaviour
{
    // Note: cleaned comment.
    [Header("Level")]
    [Tooltip("Optional frame image shown behind the level text.")]
    [SerializeField] private Image     levelFrame;
    [Tooltip("Level text")]
    [SerializeField] private TMP_Text  levelText;

    // Note: cleaned comment.
    [Header("EXP")]
    [Tooltip("Optional EXP icon image.")]
    [SerializeField] private Image     expIcon;
    [Tooltip("EXP gauge image. Set Image Type to Filled and Fill Method to Horizontal.")]
    [SerializeField] private Image     expGauge;
    [Tooltip("Optional EXP text such as 0 / 10.")]
    [SerializeField] private TMP_Text  expText;

    // Note: cleaned comment.
    [Header("Gold")]
    [Tooltip("Optional gold coin icon image.")]
    [SerializeField] private Image     goldIcon;
    [Tooltip("Gold text")]
    [SerializeField] private TMP_Text  goldText;

    // Note: cleaned comment.
    [Header("Auto Build Settings")]
    [SerializeField] private float levelFontSize  = 36f;
    [SerializeField] private float expFontSize    = 22f;
    [SerializeField] private float goldFontSize   = 28f;
    [SerializeField] private Color gaugeColor     = new Color(0.2f, 0.8f, 1f, 1f);
    [SerializeField] private Color gaugeBgColor   = new Color(0.1f, 0.1f, 0.1f, 0.7f);
    [SerializeField] private Color goldTextColor  = new Color(1f, 0.85f, 0.2f, 1f);
    [SerializeField] private Color levelTextColor = Color.white;

    // Note: cleaned comment.
    public TMP_Text LevelText => levelText;
    public TMP_Text GoldText  => goldText;
    public TMP_Text ExpText   => expText;
    public Image    ExpGauge  => expGauge;

    // Note: cleaned comment.

    private void Awake()
    {
        AutoBind();
    }

    private void AutoBind()
    {
        // Note: cleaned comment.
        TMP_Text  FindText(string n)  => transform.Find(n)?.GetComponent<TMP_Text>();
        Image     FindImage(string n) => transform.Find(n)?.GetComponent<Image>();

        levelText  ??= FindText("LevelText")   ?? FindText("Level");
        levelFrame ??= FindImage("LevelFrame") ?? FindImage("LevelBG");
        expIcon    ??= FindImage("ExpIcon")    ?? FindImage("EXPIcon");
        expGauge   ??= FindImage("ExpGauge")   ?? FindImage("EXPGauge") ?? FindImage("ExpFill");
        expText    ??= FindText("ExpText")     ?? FindText("EXPText");
        goldIcon   ??= FindImage("GoldIcon");
        goldText   ??= FindText("GoldText")    ?? FindText("Gold");

        // Note: cleaned comment.
        if (levelText == null) BuildLevelUI();
        if (expGauge  == null) BuildExpUI();
        if (goldText  == null) BuildGoldUI();
    }

    // Note: cleaned comment.

    /// <summary>
    /// Documentation cleaned.
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

    // Note: cleaned comment.

    /// <summary>
    /// Documentation cleaned.
    /// Documentation cleaned.
    /// </summary>
    private void BuildLevelUI()
    {
        // Note: cleaned comment.
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

        // Note: cleaned comment.
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

        // Note: cleaned comment.
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

        // Note: cleaned comment.
        textRt.anchorMin = new Vector2(0f, 0f);
        textRt.anchorMax = new Vector2(1f, 0.65f);
    }

    /// <summary>
    /// Documentation cleaned.
    /// Documentation cleaned.
    /// </summary>
    private void BuildExpUI()
    {
        // Note: cleaned comment.
        GameObject container = new GameObject("ExpContainer", typeof(RectTransform));
        container.transform.SetParent(transform, false);
        RectTransform cRt = container.GetComponent<RectTransform>();
        cRt.anchorMin = new Vector2(0f, 1f);
        cRt.anchorMax = new Vector2(0f, 1f);
        cRt.pivot     = new Vector2(0f, 1f);
        cRt.anchoredPosition = new Vector2(10f, -100f); // ?덈꺼 ?꾨젅??80) + ?щ갚(10)
        cRt.sizeDelta = new Vector2(220f, 22f);

        // Note: cleaned comment.
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

        // Note: cleaned comment.
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

        // Note: cleaned comment.
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

        // Note: cleaned comment.
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
    /// Documentation cleaned.
    /// Documentation cleaned.
    /// </summary>
    private void BuildGoldUI()
    {
        // Note: cleaned comment.
        GameObject container = new GameObject("GoldContainer", typeof(RectTransform));
        container.transform.SetParent(transform, false);
        RectTransform cRt = container.GetComponent<RectTransform>();
        cRt.anchorMin = new Vector2(0f, 1f);
        cRt.anchorMax = new Vector2(0f, 1f);
        cRt.pivot     = new Vector2(0f, 1f);
        cRt.anchoredPosition = new Vector2(10f, -132f); // EXP 而⑦뀒?대꼫 ?꾨옒
        cRt.sizeDelta = new Vector2(160f, 36f);

        // Note: cleaned comment.
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

        // Note: cleaned comment.
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

        // Note: cleaned comment.
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

