using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project
{
    /// <summary>
    /// 레벨, 경험치, 골드를 각각 분리된 영역으로 표시하는 HUD 컴포넌트입니다.
    /// 인스펙터 연결이 비어 있으면 Awake에서 자식 오브젝트를 자동 탐색합니다.
    /// </summary>
    public class StatusHudView : MonoBehaviour
    {
        [Header("레벨")]
        [Tooltip("레벨 숫자 뒤에 표시되는 프레임 이미지입니다. 선택 항목입니다.")]
        [SerializeField] private Image levelFrame;
        [Tooltip("레벨 숫자 텍스트입니다.")]
        [SerializeField] private TMP_Text levelText;

        [Header("경험치")]
        [Tooltip("EXP 아이콘 이미지입니다. 선택 항목입니다.")]
        [SerializeField] private Image expIcon;
        [Tooltip("경험치 게이지 이미지입니다. Image Type은 Filled를 사용합니다.")]
        [SerializeField] private Image expGauge;
        [Tooltip("경험치 숫자 텍스트입니다. 예: 0 / 10")]
        [SerializeField] private TMP_Text expText;

        [Header("골드")]
        [Tooltip("골드 아이콘 이미지입니다. 선택 항목입니다.")]
        [SerializeField] private Image goldIcon;
        [Tooltip("골드 숫자 텍스트입니다.")]
        [SerializeField] private TMP_Text goldText;

        [Header("자동 생성 설정")]
        [SerializeField] private float levelFontSize = 36f;
        [SerializeField] private float expFontSize = 22f;
        [SerializeField] private float goldFontSize = 28f;
        [SerializeField] private Color gaugeColor = new Color(0.2f, 0.8f, 1f, 1f);
        [SerializeField] private Color gaugeBgColor = new Color(0.1f, 0.1f, 0.1f, 0.7f);
        [SerializeField] private Color goldTextColor = new Color(1f, 0.85f, 0.2f, 1f);
        [SerializeField] private Color levelTextColor = Color.white;

        public TMP_Text LevelText => levelText;
        public TMP_Text GoldText => goldText;
        public TMP_Text ExpText => expText;
        public Image ExpGauge => expGauge;

        private void Awake()
        {
            AutoBind();
        }

        private void AutoBind()
        {
            TMP_Text FindText(string name) => transform.Find(name)?.GetComponent<TMP_Text>();
            Image FindImage(string name) => transform.Find(name)?.GetComponent<Image>();

            levelText ??= FindText("LevelText") ?? FindText("Level");
            levelFrame ??= FindImage("LevelFrame") ?? FindImage("LevelBG");
            expIcon ??= FindImage("ExpIcon") ?? FindImage("EXPIcon");
            expGauge ??= FindImage("ExpGauge") ?? FindImage("EXPGauge") ?? FindImage("ExpFill");
            expText ??= FindText("ExpText") ?? FindText("EXPText");
            goldIcon ??= FindImage("GoldIcon");
            goldText ??= FindText("GoldText") ?? FindText("Gold");

            if (levelText == null) BuildLevelUI();
            if (expGauge == null) BuildExpUI();
            if (goldText == null) BuildGoldUI();
        }

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
                goldText.text = gold.ToString("N0");
        }

        private void BuildLevelUI()
        {
            GameObject frameGo = new GameObject("LevelFrame", typeof(RectTransform), typeof(Image));
            frameGo.transform.SetParent(transform, false);
            RectTransform frameRt = frameGo.GetComponent<RectTransform>();
            frameRt.anchorMin = new Vector2(0f, 1f);
            frameRt.anchorMax = new Vector2(0f, 1f);
            frameRt.pivot = new Vector2(0f, 1f);
            frameRt.anchoredPosition = new Vector2(10f, -10f);
            frameRt.sizeDelta = new Vector2(80f, 80f);

            levelFrame = frameGo.GetComponent<Image>();
            levelFrame.color = new Color(0.1f, 0.1f, 0.2f, 0.85f);
            levelFrame.raycastTarget = false;

            GameObject textGo = new GameObject("LevelText", typeof(RectTransform), typeof(TextMeshProUGUI));
            textGo.transform.SetParent(frameGo.transform, false);
            RectTransform textRt = textGo.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(4f, 4f);
            textRt.offsetMax = new Vector2(-4f, -4f);

            levelText = textGo.GetComponent<TextMeshProUGUI>();
            levelText.text = "1";
            levelText.fontSize = levelFontSize;
            levelText.fontStyle = FontStyles.Bold;
            levelText.color = levelTextColor;
            levelText.alignment = TextAlignmentOptions.Center;
            levelText.raycastTarget = false;

            GameObject labelGo = new GameObject("LvLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelGo.transform.SetParent(frameGo.transform, false);
            RectTransform labelRt = labelGo.GetComponent<RectTransform>();
            labelRt.anchorMin = new Vector2(0f, 0.65f);
            labelRt.anchorMax = new Vector2(1f, 1f);
            labelRt.offsetMin = Vector2.zero;
            labelRt.offsetMax = Vector2.zero;

            TextMeshProUGUI label = labelGo.GetComponent<TextMeshProUGUI>();
            label.text = "Lv";
            label.fontSize = 16f;
            label.fontStyle = FontStyles.Bold;
            label.color = new Color(0.7f, 0.9f, 1f, 1f);
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;

            textRt.anchorMin = new Vector2(0f, 0f);
            textRt.anchorMax = new Vector2(1f, 0.65f);
        }

        private void BuildExpUI()
        {
            GameObject container = new GameObject("ExpContainer", typeof(RectTransform));
            container.transform.SetParent(transform, false);
            RectTransform cRt = container.GetComponent<RectTransform>();
            cRt.anchorMin = new Vector2(0f, 1f);
            cRt.anchorMax = new Vector2(0f, 1f);
            cRt.pivot = new Vector2(0f, 1f);
            cRt.anchoredPosition = new Vector2(10f, -100f);
            cRt.sizeDelta = new Vector2(220f, 22f);

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

            GameObject fillGo = new GameObject("ExpGauge", typeof(RectTransform), typeof(Image));
            fillGo.transform.SetParent(container.transform, false);
            RectTransform fillRt = fillGo.GetComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.offsetMin = new Vector2(2f, 2f);
            fillRt.offsetMax = new Vector2(-2f, -2f);

            expGauge = fillGo.GetComponent<Image>();
            expGauge.color = gaugeColor;
            expGauge.type = Image.Type.Filled;
            expGauge.fillMethod = Image.FillMethod.Horizontal;
            expGauge.fillAmount = 0f;
            expGauge.raycastTarget = false;

            GameObject textGo = new GameObject("ExpText", typeof(RectTransform), typeof(TextMeshProUGUI));
            textGo.transform.SetParent(container.transform, false);
            RectTransform textRt = textGo.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(4f, 0f);
            textRt.offsetMax = new Vector2(-4f, 0f);

            expText = textGo.GetComponent<TextMeshProUGUI>();
            expText.text = "0 / 10";
            expText.fontSize = expFontSize;
            expText.fontStyle = FontStyles.Bold;
            expText.color = Color.white;
            expText.alignment = TextAlignmentOptions.Center;
            expText.raycastTarget = false;
        }

        private void BuildGoldUI()
        {
            GameObject container = new GameObject("GoldContainer", typeof(RectTransform));
            container.transform.SetParent(transform, false);
            RectTransform cRt = container.GetComponent<RectTransform>();
            cRt.anchorMin = new Vector2(0f, 1f);
            cRt.anchorMax = new Vector2(0f, 1f);
            cRt.pivot = new Vector2(0f, 1f);
            cRt.anchoredPosition = new Vector2(10f, -132f);
            cRt.sizeDelta = new Vector2(160f, 36f);

            GameObject iconGo = new GameObject("GoldIcon", typeof(RectTransform), typeof(Image));
            iconGo.transform.SetParent(container.transform, false);
            RectTransform iconRt = iconGo.GetComponent<RectTransform>();
            iconRt.anchorMin = new Vector2(0f, 0f);
            iconRt.anchorMax = new Vector2(0f, 1f);
            iconRt.pivot = new Vector2(0f, 0.5f);
            iconRt.offsetMin = Vector2.zero;
            iconRt.offsetMax = new Vector2(36f, 0f);

            goldIcon = iconGo.GetComponent<Image>();
            goldIcon.color = new Color(1f, 0.8f, 0.1f, 1f);
            goldIcon.raycastTarget = false;

            GameObject textGo = new GameObject("GoldText", typeof(RectTransform), typeof(TextMeshProUGUI));
            textGo.transform.SetParent(container.transform, false);
            RectTransform textRt = textGo.GetComponent<RectTransform>();
            textRt.anchorMin = new Vector2(0f, 0f);
            textRt.anchorMax = new Vector2(1f, 1f);
            textRt.offsetMin = new Vector2(42f, 0f);
            textRt.offsetMax = Vector2.zero;

            goldText = textGo.GetComponent<TextMeshProUGUI>();
            goldText.text = "0";
            goldText.fontSize = goldFontSize;
            goldText.fontStyle = FontStyles.Bold;
            goldText.color = goldTextColor;
            goldText.alignment = TextAlignmentOptions.Left;
            goldText.raycastTarget = false;
        }
    }
}
