using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 로비/타이틀에서 사용하는 환경설정 팝업 패널입니다.
/// - 탭: 그래픽 / 사운드 / 계정
/// - 설정값은 PlayerPrefs에 저장됩니다.
/// </summary>
[DisallowMultipleComponent]
public class SettingPanel : MonoBehaviour
{
    private const string PrefKeyFrameLevel = "setting.frame.level";
    private const string PrefKeyShake = "setting.graphic.shake";
    private const string PrefKeyBloom = "setting.graphic.bloom";
    private const string PrefKeyBlur = "setting.graphic.blur";

    private const string PrefKeyBgmVolume = "setting.sound.bgm";
    private const string PrefKeySfxVolume = "setting.sound.sfx";
    private const string PrefKeyMute = "setting.sound.mute";

    [Header("Tabs")]
    [SerializeField] private Button graphicTabButton;
    [SerializeField] private Button soundTabButton;
    [SerializeField] private Button accountTabButton;

    [SerializeField] private GameObject graphicTabRoot;
    [SerializeField] private GameObject soundTabRoot;
    [SerializeField] private GameObject accountTabRoot;

    [Header("Graphic")]
    [SerializeField] private Button frameHighButton;
    [SerializeField] private Button frameMidButton;
    [SerializeField] private Button frameLowButton;

    [SerializeField] private Toggle shakeToggle;
    [SerializeField] private Toggle bloomToggle;
    [SerializeField] private Toggle blurToggle;

    [SerializeField] private TMP_Text frameStateText;

    [Header("Sound")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private TMP_Text bgmValueText;
    [SerializeField] private TMP_Text sfxValueText;

    [SerializeField] private Toggle muteToggle;

    [Tooltip("BGM AudioSource(선택). 연결 시 슬라이더 값이 반영됩니다.")]
    [SerializeField] private AudioSource bgmAudioSource;

    [Tooltip("SFX AudioSource(선택). 연결 시 슬라이더 값이 반영됩니다.")]
    [SerializeField] private AudioSource sfxAudioSource;

    [Header("Account")]
    [SerializeField] private TMP_Text uidText;
    [SerializeField] private TMP_Text loginMethodText;
    [SerializeField] private Button logoutButton;

    [SerializeField] private string uid = "UID-000000";
    [SerializeField] private string loginMethod = "Guest";

    [Header("Optional Events")]
    [SerializeField] private VoidEventChannelSO onLogoutClicked;

    private FrameLevel currentFrameLevel = FrameLevel.Mid;

    private void Awake()
    {
        BindTabs();
        BindGraphic();
        BindSound();
        BindAccount();

        ConfigureSliderRange();
        LoadPrefs();

        OpenGraphicTab();
    }

    private void BindTabs()
    {
        graphicTabButton?.onClick.AddListener(OpenGraphicTab);
        soundTabButton?.onClick.AddListener(OpenSoundTab);
        accountTabButton?.onClick.AddListener(OpenAccountTab);
    }

    private void BindGraphic()
    {
        frameHighButton?.onClick.AddListener(SetFrameHigh);
        frameMidButton?.onClick.AddListener(SetFrameMid);
        frameLowButton?.onClick.AddListener(SetFrameLow);

        if (shakeToggle != null)
            shakeToggle.onValueChanged.AddListener(OnShakeChanged);

        if (bloomToggle != null)
            bloomToggle.onValueChanged.AddListener(OnBloomChanged);

        if (blurToggle != null)
            blurToggle.onValueChanged.AddListener(OnBlurChanged);
    }

    private void BindSound()
    {
        if (bgmSlider != null)
            bgmSlider.onValueChanged.AddListener(OnBgmSliderChanged);

        if (sfxSlider != null)
            sfxSlider.onValueChanged.AddListener(OnSfxSliderChanged);

        if (muteToggle != null)
            muteToggle.onValueChanged.AddListener(OnMuteChanged);
    }

    private void BindAccount()
    {
        logoutButton?.onClick.AddListener(OnLogoutClicked);
    }

    private void ConfigureSliderRange()
    {
        if (bgmSlider != null)
        {
            bgmSlider.minValue = 0f;
            bgmSlider.maxValue = 100f;
            bgmSlider.wholeNumbers = true;
        }

        if (sfxSlider != null)
        {
            sfxSlider.minValue = 0f;
            sfxSlider.maxValue = 100f;
            sfxSlider.wholeNumbers = true;
        }
    }

    private void LoadPrefs()
    {
        currentFrameLevel = (FrameLevel)PlayerPrefs.GetInt(PrefKeyFrameLevel, (int)FrameLevel.Mid);
        bool shake = PlayerPrefs.GetInt(PrefKeyShake, 1) == 1;
        bool bloom = PlayerPrefs.GetInt(PrefKeyBloom, 1) == 1;
        bool blur = PlayerPrefs.GetInt(PrefKeyBlur, 0) == 1;

        int bgm = PlayerPrefs.GetInt(PrefKeyBgmVolume, 80);
        int sfx = PlayerPrefs.GetInt(PrefKeySfxVolume, 80);
        bool mute = PlayerPrefs.GetInt(PrefKeyMute, 0) == 1;

        SetToggleSilently(shakeToggle, shake);
        SetToggleSilently(bloomToggle, bloom);
        SetToggleSilently(blurToggle, blur);

        SetSliderSilently(bgmSlider, bgm);
        SetSliderSilently(sfxSlider, sfx);
        SetToggleSilently(muteToggle, mute);

        ApplyFrameLevel(currentFrameLevel, false);
        ApplySoundVolumes(false);
        ApplyMute(mute, false);

        UpdateSoundTexts();
        RefreshAccountTexts();
    }

    // ── 탭 오픈 ───────────────────────────────────────────────

    public void OpenGraphicTab() => SetTab(graphicTabRoot, soundTabRoot, accountTabRoot);
    public void OpenSoundTab() => SetTab(soundTabRoot, graphicTabRoot, accountTabRoot);
    public void OpenAccountTab() => SetTab(accountTabRoot, graphicTabRoot, soundTabRoot);

    private static void SetTab(GameObject openTab, GameObject closeA, GameObject closeB)
    {
        if (openTab != null) openTab.SetActive(true);
        if (closeA != null) closeA.SetActive(false);
        if (closeB != null) closeB.SetActive(false);
    }

    // ── 그래픽 ───────────────────────────────────────────────

    public void SetFrameHigh() => ApplyFrameLevel(FrameLevel.High, true);
    public void SetFrameMid() => ApplyFrameLevel(FrameLevel.Mid, true);
    public void SetFrameLow() => ApplyFrameLevel(FrameLevel.Low, true);

    private void ApplyFrameLevel(FrameLevel level, bool save)
    {
        currentFrameLevel = level;

        switch (level)
        {
            case FrameLevel.High:
                Application.targetFrameRate = 60;
                SetFrameText("상 (60 FPS)");
                break;
            case FrameLevel.Mid:
                Application.targetFrameRate = 45;
                SetFrameText("중 (45 FPS)");
                break;
            case FrameLevel.Low:
                Application.targetFrameRate = 30;
                SetFrameText("하 (30 FPS)");
                break;
        }

        if (save)
        {
            PlayerPrefs.SetInt(PrefKeyFrameLevel, (int)level);
            PlayerPrefs.Save();
        }
    }

    private void OnShakeChanged(bool isOn)
    {
        PlayerPrefs.SetInt(PrefKeyShake, isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void OnBloomChanged(bool isOn)
    {
        PlayerPrefs.SetInt(PrefKeyBloom, isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void OnBlurChanged(bool isOn)
    {
        PlayerPrefs.SetInt(PrefKeyBlur, isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    // ── 사운드 ───────────────────────────────────────────────

    private void OnBgmSliderChanged(float value)
    {
        int saved = Mathf.RoundToInt(value);
        PlayerPrefs.SetInt(PrefKeyBgmVolume, saved);
        PlayerPrefs.Save();

        ApplySoundVolumes(true);
        UpdateSoundTexts();
    }

    private void OnSfxSliderChanged(float value)
    {
        int saved = Mathf.RoundToInt(value);
        PlayerPrefs.SetInt(PrefKeySfxVolume, saved);
        PlayerPrefs.Save();

        ApplySoundVolumes(true);
        UpdateSoundTexts();
    }

    private void OnMuteChanged(bool isMuted)
    {
        PlayerPrefs.SetInt(PrefKeyMute, isMuted ? 1 : 0);
        PlayerPrefs.Save();

        ApplyMute(isMuted, true);
    }

    private void ApplySoundVolumes(bool includeAudioSources)
    {
        float bgm01 = GetBgmVolume01();
        float sfx01 = GetSfxVolume01();

        if (includeAudioSources)
        {
            if (bgmAudioSource != null)
                bgmAudioSource.volume = bgm01;

            if (sfxAudioSource != null)
                sfxAudioSource.volume = sfx01;
        }
    }

    private static void ApplyMute(bool isMuted, bool includeDebugLog)
    {
        // 음소거는 수치(BGM/SFX) 자체를 변경하지 않고 출력만 차단합니다.
        AudioListener.pause = isMuted;

        if (includeDebugLog)
            Debug.Log($"[SettingPanel] 음소거: {(isMuted ? "ON" : "OFF")}");
    }

    private float GetBgmVolume01()
    {
        return (bgmSlider == null ? 80f : bgmSlider.value) / 100f;
    }

    private float GetSfxVolume01()
    {
        return (sfxSlider == null ? 80f : sfxSlider.value) / 100f;
    }

    private void UpdateSoundTexts()
    {
        if (bgmValueText != null && bgmSlider != null)
            bgmValueText.text = Mathf.RoundToInt(bgmSlider.value).ToString();

        if (sfxValueText != null && sfxSlider != null)
            sfxValueText.text = Mathf.RoundToInt(sfxSlider.value).ToString();
    }

    // ── 계정 ────────────────────────────────────────────────

    private void RefreshAccountTexts()
    {
        if (uidText != null) uidText.text = uid;
        if (loginMethodText != null) loginMethodText.text = loginMethod;
    }

    private void OnLogoutClicked()
    {
        Debug.Log("[SettingPanel] 로그아웃 버튼 클릭");
        onLogoutClicked?.RaiseEvent();
    }

    // ── 유틸 ────────────────────────────────────────────────

    private static void SetSliderSilently(Slider slider, int value)
    {
        if (slider == null) return;
        slider.SetValueWithoutNotify(Mathf.Clamp(value, 0, 100));
    }

    private static void SetToggleSilently(Toggle toggle, bool value)
    {
        if (toggle == null) return;
        toggle.SetIsOnWithoutNotify(value);
    }

    private void SetFrameText(string text)
    {
        if (frameStateText != null)
            frameStateText.text = text;
    }

    private enum FrameLevel
    {
        Low = 0,
        Mid = 1,
        High = 2
    }
}
