using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectFirst.Data;

public class TitleSettingsManager : MonoBehaviour
{
    [Header("Settings Panel")]
    [SerializeField] private TMP_FontAsset customFont;
    [SerializeField] private GameSettingsData settingsData;

    private GameObject settingsPanel;
    private Slider bgmSlider;
    private Slider sfxSlider;
    private Toggle muteToggle;
    private TMP_Text bgmValueText;
    private TMP_Text sfxValueText;
    private Canvas targetCanvas;

    public void Initialize()
    {
        targetCanvas = FindObjectOfType<Canvas>();
        settingsData?.LoadLegacyPrefs();
        if (targetCanvas != null)
        {
            BuildSettingsPanel();
        }
    }

    public void OpenSettingsPanel()
    {
        if (settingsPanel == null)
        {
            BuildSettingsPanel();
        }

        if (settingsPanel != null)
        {
            LoadSettings();
            settingsPanel.SetActive(true);
        }
    }

    public void CloseSettingsPanel()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    private void BuildSettingsPanel()
    {
        if (settingsPanel != null) return;

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
        windowRt.sizeDelta = new Vector2(700f, 600f);
        window.GetComponent<Image>().color = new Color(0.08f, 0.1f, 0.16f, 0.95f);

        CreatePanelLabel(window.transform, "Title", "Settings", new Vector2(0f, 270f), 48f, true);
        CreatePanelLabel(window.transform, "BgmLabel", "BGM", new Vector2(-250f, 150f), 32f, true, TextAlignmentOptions.MidlineLeft);
        bgmSlider = CreatePanelSlider(window.transform, "BgmSlider", new Vector2(70f, 150f));
        bgmValueText = CreatePanelLabel(window.transform, "BgmValue", "80", new Vector2(315f, 150f), 28f, true);
        CreatePanelLabel(window.transform, "SfxLabel", "SFX", new Vector2(-250f, 50f), 32f, true, TextAlignmentOptions.MidlineLeft);
        sfxSlider = CreatePanelSlider(window.transform, "SfxSlider", new Vector2(70f, 50f));
        sfxValueText = CreatePanelLabel(window.transform, "SfxValue", "80", new Vector2(315f, 50f), 28f, true);
        muteToggle = CreatePanelToggle(window.transform, "MuteToggle", "Mute", new Vector2(0f, -50f));
        Button closeButton = CreatePanelButton(window.transform, "CloseButton", "Close", new Vector2(0f, -250f), new Vector2(300f, 80f));
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
        {
            muteToggle.onValueChanged.AddListener(OnMuteChanged);
        }

        settingsPanel.SetActive(false);
    }

    private TMP_Text CreatePanelLabel(Transform parent, string name, string text, Vector2 pos, float size, bool bold, TextAlignmentOptions alignment = TextAlignmentOptions.Center)
    {
        GameObject labelGo = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        labelGo.transform.SetParent(parent, false);
        RectTransform rt = labelGo.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(600f, 50f);

        TextMeshProUGUI tmp = labelGo.GetComponent<TextMeshProUGUI>();
        if (customFont != null) tmp.font = customFont;
        tmp.text = text;
        tmp.fontSize = size;
        tmp.fontSizeMin = size * 0.8f;
        tmp.fontSizeMax = size;
        tmp.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
        tmp.color = Color.white;
        tmp.alignment = alignment;
        tmp.enableWordWrapping = false;
        tmp.enableAutoSizing = true;
        return tmp;
    }

    private Slider CreatePanelSlider(Transform parent, string name, Vector2 pos)
    {
        GameObject sliderGo = new GameObject(name, typeof(RectTransform), typeof(Slider));
        sliderGo.transform.SetParent(parent, false);
        RectTransform sliderRt = sliderGo.GetComponent<RectTransform>();
        sliderRt.anchorMin = sliderRt.anchorMax = new Vector2(0.5f, 0.5f);
        sliderRt.anchoredPosition = pos;
        sliderRt.sizeDelta = new Vector2(380f, 30f);

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
        handleRt.sizeDelta = new Vector2(25f, 40f);
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
        rt.sizeDelta = new Vector2(300f, 50f);

        GameObject bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bg.transform.SetParent(toggleGo.transform, false);
        RectTransform bgRt = bg.GetComponent<RectTransform>();
        bgRt.anchorMin = new Vector2(0f, 0.5f);
        bgRt.anchorMax = new Vector2(0f, 0.5f);
        bgRt.sizeDelta = new Vector2(35f, 35f);
        bgRt.anchoredPosition = new Vector2(25f, 0f);
        bg.GetComponent<Image>().color = new Color(0.15f, 0.2f, 0.3f, 1f);

        GameObject check = new GameObject("Checkmark", typeof(RectTransform), typeof(Image));
        check.transform.SetParent(bg.transform, false);
        SetFullScreenStretch(check.GetComponent<RectTransform>());
        check.GetComponent<Image>().color = new Color(0.1f, 0.8f, 0.95f, 1f);

        CreatePanelLabel(toggleGo.transform, "Label", label, new Vector2(70f, 0f), 28f, true, TextAlignmentOptions.MidlineLeft);

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
        CreatePanelLabel(btnGo.transform, "Text", text, Vector2.zero, 32f, true);
        return btnGo.GetComponent<Button>();
    }

    private void SetFullScreenStretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private void LoadSettings()
    {
        settingsData?.LoadLegacyPrefs();
        int bgm = settingsData != null ? Mathf.RoundToInt(settingsData.bgmVolume) : PlayerPrefs.GetInt("setting.sound.bgm", 80);
        int sfx = settingsData != null ? Mathf.RoundToInt(settingsData.sfxVolume) : PlayerPrefs.GetInt("setting.sound.sfx", 80);
        bool mute = settingsData != null ? settingsData.globalMute : PlayerPrefs.GetInt("setting.sound.mute", 0) == 1;

        if (bgmSlider != null) bgmSlider.SetValueWithoutNotify(Mathf.Clamp(bgm, 0, 100));
        if (sfxSlider != null) sfxSlider.SetValueWithoutNotify(Mathf.Clamp(sfx, 0, 100));
        if (muteToggle != null) muteToggle.SetIsOnWithoutNotify(mute);
        RefreshSoundTexts();
        AudioListener.pause = mute;
    }

    private void OnBgmChanged(float value)
    {
        if (settingsData != null)
        {
            settingsData.bgmVolume = value;
            settingsData.SaveLegacyPrefs();
        }
        else
        {
            PlayerPrefs.SetInt("setting.sound.bgm", Mathf.RoundToInt(value));
            PlayerPrefs.Save();
        }

        RefreshSoundTexts();
    }

    private void OnSfxChanged(float value)
    {
        if (settingsData != null)
        {
            settingsData.sfxVolume = value;
            settingsData.SaveLegacyPrefs();
        }
        else
        {
            PlayerPrefs.SetInt("setting.sound.sfx", Mathf.RoundToInt(value));
            PlayerPrefs.Save();
        }

        RefreshSoundTexts();
    }

    private void OnMuteChanged(bool isMuted)
    {
        if (settingsData != null)
        {
            settingsData.globalMute = isMuted;
            settingsData.SaveLegacyPrefs();
        }
        else
        {
            PlayerPrefs.SetInt("setting.sound.mute", isMuted ? 1 : 0);
            PlayerPrefs.Save();
        }

        AudioListener.pause = isMuted;
    }

    private void RefreshSoundTexts()
    {
        if (bgmValueText != null && bgmSlider != null)
            bgmValueText.text = Mathf.RoundToInt(bgmSlider.value).ToString();

        if (sfxValueText != null && sfxSlider != null)
            sfxValueText.text = Mathf.RoundToInt(sfxSlider.value).ToString();
    }
}