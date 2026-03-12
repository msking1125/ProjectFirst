using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using ProjectFirst.Data;

/// <summary>
/// UIToolkit(UIDocument) 湲곕컲 ?ㅼ젙 ?⑤꼸.
///
/// [Inspector ?곌껐 媛?대뱶]
/// ??uiDocument        : ??GameObject??UIDocument 而댄룷?뚰듃
/// ??audioMixer        : ?꾨줈?앺듃 AudioMixer (BGMVolume / SFXVolume ?뚮씪誘명꽣 ?꾩슂)
/// ??playerData        : PlayerData.asset (怨꾩젙 ??UID ?쒖떆?? ?좏깮)
/// ??defaultLoginMethod: "Guest" ??濡쒓렇??諛⑹떇 湲곕낯媛?
///
/// [LobbyManager ?곕룞]
/// LobbyManager.settingPanel ????而댄룷?뚰듃瑜??곌껐?섎㈃
/// ?ㅼ젙 踰꾪듉 ?대┃ ??OpenPanel()???먮룞 ?몄텧?⑸땲??
///
/// [AudioMixer ?뚮씪誘명꽣]
/// Exposed Parameter ?대쫫??諛섎뱶??"BGMVolume", "SFXVolume" ?쇰줈 吏?뺥븯?몄슂.
/// </summary>
[DisallowMultipleComponent]
public class SettingPanel : MonoBehaviour
{
    public static SettingPanel Instance { get; private set; }

    // ?? Inspector ??????????????????????????????????????????

    [Header("UI")]
    [SerializeField] private UIDocument uiDocument;

    [Header("Audio")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("Account (?좏깮)")]
    [SerializeField] private PlayerData playerData;
    [SerializeField] private string defaultLoginMethod = "Guest";
    [SerializeField] private GameSettingsData settingsData;

    // ?? PlayerPrefs ???????????????????????????????????????

    private const string KEY_BGM_VOL  = "bgmVol";
    private const string KEY_SFX_VOL  = "sfxVol";
    private const string KEY_BGM_MUTE = "bgmMute";
    private const string KEY_SFX_MUTE = "sfxMute";
    private const string KEY_FRAME    = "frameQuality";
    private const string KEY_SHAKE    = "shakeOn";
    private const string KEY_BLOOM    = "bloomOn";
    private const string KEY_BLUR     = "blurOn";

    // AudioMixer Exposed Parameter ?대쫫
    private const string MIXER_BGM = "BGMVolume";
    private const string MIXER_SFX = "SFXVolume";

    // ?? UI 罹먯떆 ????????????????????????????????????????????

    // 諛곌꼍/猷⑦듃
    private VisualElement _backdrop;

    // ??踰꾪듉
    private Button _tabGraphicsBtn;
    private Button _tabSoundBtn;
    private Button _tabAccountBtn;

    // ??肄섑뀗痢?
    private VisualElement _graphicsTab;
    private VisualElement _soundTab;
    private VisualElement _accountTab;

    // 洹몃옒?? ?꾨젅???좏깮 (RadioButtonGroup ??value=0:??1:以?2:??
    private RadioButtonGroup _frameGroup;
    private Toggle           _shakeToggle;
    private Toggle           _bloomToggle;
    private Toggle           _blurToggle;

    // AudioMixer ?뚮씪誘명꽣 ?ъ슜 媛???щ? (Exposed 誘몄꽕????false)
    private bool _audioReady;

    // ?ъ슫??
    private Slider _bgmSlider;
    private Slider _sfxSlider;
    private Label  _bgmValueLabel;
    private Label  _sfxValueLabel;
    private Toggle _bgmMuteToggle;
    private Toggle _sfxMuteToggle;

    // ?뚯냼嫄???蹂쇰ⅷ 罹먯떆 (?щ씪?대뜑 ?섏튂 ?좎?, 異쒕젰留?0)
    private float _bgmVolCache;
    private float _sfxVolCache;

    // 怨꾩젙
    private Label  _uidLabel;
    private Label  _loginMethodLabel;
    private Button _logoutBtn;
    private Button _copyBtn;

    // 濡쒓렇?꾩썐 ?뺤씤 ?앹뾽
    private VisualElement _confirmOverlay;
    private Button        _confirmYesBtn;
    private Button        _confirmNoBtn;

    // ?????????????????????????????????????????????????????????

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        BindUI();
        ValidateAudio();
        Hide();
    }

    // ?? 怨듦컻 API ?????????????????????????????????????????????

    /// <summary>?ㅼ젙 ?⑤꼸???쒖떆?섍퀬 ??λ맂 媛믪쑝濡?珥덇린?뷀빀?덈떎.</summary>
    public void Show()
    {
        if (_backdrop == null) return;

        // ?ㅻⅨ UIDocument蹂대떎 ?꾩뿉 ?뚮뜑留곷릺?꾨줉 Sort Order ?곗꽑 ?ㅼ젙
        if (uiDocument != null)
            uiDocument.sortingOrder = 100;

        _backdrop.style.display = DisplayStyle.Flex;
        LoadSettings();
        SwitchTab(0);
    }

    /// <summary>紐⑤뱺 ?ㅼ젙????ν븯怨??⑤꼸???④퉩?덈떎.</summary>
    public void Hide()
    {
        if (_backdrop == null) return;

        SaveAllSettings();
        _backdrop.style.display = DisplayStyle.None;
    }

    /// <summary>LobbyManager ?명솚 ??Show()? ?숈씪?⑸땲??</summary>
    public void OpenPanel() => Show();

    /// <summary>LobbyManager ?명솚 ??Hide()? ?숈씪?⑸땲??</summary>
    public void ClosePanel() => Hide();

    // ?? UI 諛붿씤???????????????????????????????????????????????

    private void BindUI()
    {
        if (uiDocument == null)
        {
            Debug.LogError("[SettingPanel] UIDocument媛 ?좊떦?섏? ?딆븯?듬땲??");
            return;
        }

        var root = uiDocument.rootVisualElement;

        _backdrop = root.Q<VisualElement>("setting-backdrop");

        // ?ㅻ뜑 ?リ린
        root.Q<Button>("close-btn")?.RegisterCallback<ClickEvent>(_ => Hide());

        // ??踰꾪듉
        _tabGraphicsBtn = root.Q<Button>("tab-graphics");
        _tabSoundBtn    = root.Q<Button>("tab-sound");
        _tabAccountBtn  = root.Q<Button>("tab-account");

        _tabGraphicsBtn?.RegisterCallback<ClickEvent>(_ => SwitchTab(0));
        _tabSoundBtn?.RegisterCallback<ClickEvent>(_    => SwitchTab(1));
        _tabAccountBtn?.RegisterCallback<ClickEvent>(_  => SwitchTab(2));

        // ??肄섑뀗痢?
        _graphicsTab = root.Q<VisualElement>("graphics-tab");
        _soundTab    = root.Q<VisualElement>("sound-tab");
        _accountTab  = root.Q<VisualElement>("account-tab");

        // ?? 洹몃옒????????????????????????????????????????????

        _frameGroup = root.Q<RadioButtonGroup>("frame-group");
        _frameGroup?.RegisterValueChangedCallback(e => ApplyGraphicsSettings(e.newValue));

        _shakeToggle = root.Q<Toggle>("shake-toggle");
        _bloomToggle = root.Q<Toggle>("bloom-toggle");
        _blurToggle  = root.Q<Toggle>("blur-toggle");

        _shakeToggle?.RegisterValueChangedCallback(e => PlayerPrefs.SetInt(KEY_SHAKE, e.newValue ? 1 : 0));
        _bloomToggle?.RegisterValueChangedCallback(e => PlayerPrefs.SetInt(KEY_BLOOM, e.newValue ? 1 : 0));
        _blurToggle?.RegisterValueChangedCallback(e  => PlayerPrefs.SetInt(KEY_BLUR,  e.newValue ? 1 : 0));

        // ?? ?ъ슫????????????????????????????????????????????

        _bgmSlider     = root.Q<Slider>("bgm-slider");
        _sfxSlider     = root.Q<Slider>("sfx-slider");
        _bgmValueLabel = root.Q<Label>("bgm-value-label");
        _sfxValueLabel = root.Q<Label>("sfx-value-label");
        _bgmMuteToggle = root.Q<Toggle>("bgm-mute-toggle");
        _sfxMuteToggle = root.Q<Toggle>("sfx-mute-toggle");

        _bgmSlider?.RegisterValueChangedCallback(OnBgmSliderChanged);
        _sfxSlider?.RegisterValueChangedCallback(OnSfxSliderChanged);
        _bgmMuteToggle?.RegisterValueChangedCallback(OnBgmMuteToggled);
        _sfxMuteToggle?.RegisterValueChangedCallback(OnSfxMuteToggled);

        // ?? 怨꾩젙 ????????????????????????????????????????????

        _uidLabel         = root.Q<Label>("uid-label");
        _loginMethodLabel = root.Q<Label>("login-method-label");
        _logoutBtn        = root.Q<Button>("logout-btn");
        _copyBtn          = root.Q<Button>("copy-btn");

        _logoutBtn?.RegisterCallback<ClickEvent>(_ => ShowConfirmDialog());
        _copyBtn?.RegisterCallback<ClickEvent>(_   => CopyUid());

        // ?? ?뺤씤 ?앹뾽 ????????????????????????????????????????

        _confirmOverlay = root.Q<VisualElement>("confirm-overlay");
        _confirmYesBtn  = root.Q<Button>("confirm-yes-btn");
        _confirmNoBtn   = root.Q<Button>("confirm-no-btn");

        _confirmYesBtn?.RegisterCallback<ClickEvent>(_ => OnLogoutConfirmed());
        _confirmNoBtn?.RegisterCallback<ClickEvent>(_  => HideConfirmDialog());
    }

    // ?? ???꾪솚 ??????????????????????????????????????????????

    /// <param name="tabIndex">0=洹몃옒?? 1=?ъ슫?? 2=怨꾩젙</param>
    private void SwitchTab(int tabIndex)
    {
        SetTabVisible(_graphicsTab, tabIndex == 0);
        SetTabVisible(_soundTab,    tabIndex == 1);
        SetTabVisible(_accountTab,  tabIndex == 2);

        SetTabActive(_tabGraphicsBtn, tabIndex == 0);
        SetTabActive(_tabSoundBtn,    tabIndex == 1);
        SetTabActive(_tabAccountBtn,  tabIndex == 2);

        if (tabIndex == 2)
            RefreshAccountInfo();
    }

    private static void SetTabVisible(VisualElement el, bool visible)
    {
        if (el != null)
            el.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private static void SetTabActive(Button btn, bool active)
    {
        if (btn == null) return;
        if (active) btn.AddToClassList("tab-active");
        else        btn.RemoveFromClassList("tab-active");
    }

    // ?? ?ㅼ젙 濡쒕뱶 ?????????????????????????????????????????????

    private void LoadSettings()
    {
        settingsData?.LoadLegacyPrefs();

        float bgm = settingsData != null ? settingsData.bgmVolume : PlayerPrefs.GetFloat(KEY_BGM_VOL, 80f);
        float sfx = settingsData != null ? settingsData.sfxVolume : PlayerPrefs.GetFloat(KEY_SFX_VOL, 80f);
        bool bgmMute = settingsData != null ? settingsData.bgmMute : PlayerPrefs.GetInt(KEY_BGM_MUTE, 0) == 1;
        bool sfxMute = settingsData != null ? settingsData.sfxMute : PlayerPrefs.GetInt(KEY_SFX_MUTE, 0) == 1;
        int frame = settingsData != null ? settingsData.frameQuality : Mathf.Clamp(PlayerPrefs.GetInt(KEY_FRAME, 1), 0, 2);
        bool shake = settingsData != null ? settingsData.shake : PlayerPrefs.GetInt(KEY_SHAKE, 1) == 1;
        bool bloom = settingsData != null ? settingsData.bloom : PlayerPrefs.GetInt(KEY_BLOOM, 1) == 1;
        bool blur = settingsData != null ? settingsData.blur : PlayerPrefs.GetInt(KEY_BLUR, 0) == 1;

        _bgmVolCache = bgm;
        _sfxVolCache = sfx;
        _bgmSlider?.SetValueWithoutNotify(bgm);
        _sfxSlider?.SetValueWithoutNotify(sfx);
        UpdateBgmLabel(bgm);
        UpdateSfxLabel(sfx);
        _bgmMuteToggle?.SetValueWithoutNotify(bgmMute);
        _sfxMuteToggle?.SetValueWithoutNotify(sfxMute);
        _frameGroup?.SetValueWithoutNotify(frame);
        _shakeToggle?.SetValueWithoutNotify(shake);
        _bloomToggle?.SetValueWithoutNotify(bloom);
        _blurToggle?.SetValueWithoutNotify(blur);

        ApplyBgmVolume(bgmMute ? 0f : bgm);
        ApplySfxVolume(sfxMute ? 0f : sfx);
        ApplyGraphicsSettingsSilent(frame);
    }

    // ?? ?ㅼ젙 ????????????????????????????????????????????????

    private void SaveAllSettings()
    {
        if (settingsData != null)
        {
            if (_bgmSlider != null) settingsData.bgmVolume = _bgmSlider.value;
            if (_sfxSlider != null) settingsData.sfxVolume = _sfxSlider.value;
            if (_bgmMuteToggle != null) settingsData.bgmMute = _bgmMuteToggle.value;
            if (_sfxMuteToggle != null) settingsData.sfxMute = _sfxMuteToggle.value;
            if (_frameGroup != null) settingsData.frameQuality = _frameGroup.value;
            if (_shakeToggle != null) settingsData.shake = _shakeToggle.value;
            if (_bloomToggle != null) settingsData.bloom = _bloomToggle.value;
            if (_blurToggle != null) settingsData.blur = _blurToggle.value;
            settingsData.SaveLegacyPrefs();
            return;
        }

        if (_bgmSlider != null) PlayerPrefs.SetFloat(KEY_BGM_VOL, _bgmSlider.value);
        if (_sfxSlider != null) PlayerPrefs.SetFloat(KEY_SFX_VOL, _sfxSlider.value);
        if (_bgmMuteToggle != null) PlayerPrefs.SetInt(KEY_BGM_MUTE, _bgmMuteToggle.value ? 1 : 0);
        if (_sfxMuteToggle != null) PlayerPrefs.SetInt(KEY_SFX_MUTE, _sfxMuteToggle.value ? 1 : 0);
        if (_shakeToggle != null) PlayerPrefs.SetInt(KEY_SHAKE, _shakeToggle.value ? 1 : 0);
        if (_bloomToggle != null) PlayerPrefs.SetInt(KEY_BLOOM, _bloomToggle.value ? 1 : 0);
        if (_blurToggle != null) PlayerPrefs.SetInt(KEY_BLUR, _blurToggle.value ? 1 : 0);
        if (_frameGroup != null) PlayerPrefs.SetInt(KEY_FRAME, _frameGroup.value);
        PlayerPrefs.Save();
    }

    // ?? ?ъ슫???몃뱾???????????????????????????????????????????

    private void OnBgmSliderChanged(ChangeEvent<float> evt)
    {
        if (_bgmMuteToggle == null || !_bgmMuteToggle.value)
            _bgmVolCache = evt.newValue;

        if (settingsData != null)
            settingsData.bgmVolume = evt.newValue;

        ApplyBgmVolume(_bgmMuteToggle != null && _bgmMuteToggle.value ? 0f : evt.newValue);
        UpdateBgmLabel(evt.newValue);
    }

    private void OnSfxSliderChanged(ChangeEvent<float> evt)
    {
        if (_sfxMuteToggle == null || !_sfxMuteToggle.value)
            _sfxVolCache = evt.newValue;

        if (settingsData != null)
            settingsData.sfxVolume = evt.newValue;

        ApplySfxVolume(_sfxMuteToggle != null && _sfxMuteToggle.value ? 0f : evt.newValue);
        UpdateSfxLabel(evt.newValue);
    }

    private void OnBgmMuteToggled(ChangeEvent<bool> evt)
    {
        if (settingsData != null)
            settingsData.bgmMute = evt.newValue;

        ApplyBgmVolume(evt.newValue ? 0f : _bgmVolCache);
    }

    private void OnSfxMuteToggled(ChangeEvent<bool> evt)
    {
        if (settingsData != null)
            settingsData.sfxMute = evt.newValue;

        ApplySfxVolume(evt.newValue ? 0f : _sfxVolCache);
    }

    /// <summary>
    /// AudioMixer Exposed Parameter 議댁옱 ?щ?瑜???踰덈쭔 寃利앺빀?덈떎.
    /// AudioMixer媛 ?녾굅???뚮씪誘명꽣媛 ?몄텧?섏? ?딆븯?쇰㈃ 蹂쇰ⅷ ?쒖뼱瑜?嫄대꼫?곷땲??
    /// </summary>
    private void ValidateAudio()
    {
        if (audioMixer == null) return;

        bool bgmOk = audioMixer.GetFloat(MIXER_BGM, out _);
        bool sfxOk = audioMixer.GetFloat(MIXER_SFX, out _);
        _audioReady = bgmOk && sfxOk;

        if (!_audioReady)
            Debug.LogWarning("[SettingPanel] AudioMixer??'BGMVolume' / 'SFXVolume' " +
                             "Exposed Parameter瑜??ㅼ젙?섏꽭?? 蹂쇰ⅷ ?쒖뼱媛 鍮꾪솢?깊솕?⑸땲??");
    }

    private void ApplyBgmVolume(float vol)
    {
        if (!_audioReady) return;
        float db = vol > 0f ? Mathf.Log10(vol / 100f) * 20f : -80f;
        audioMixer.SetFloat(MIXER_BGM, db);
    }

    private void ApplySfxVolume(float vol)
    {
        if (!_audioReady) return;
        float db = vol > 0f ? Mathf.Log10(vol / 100f) * 20f : -80f;
        audioMixer.SetFloat(MIXER_SFX, db);
    }

    private void UpdateBgmLabel(float val)
    {
        if (_bgmValueLabel != null)
            _bgmValueLabel.text = Mathf.RoundToInt(val).ToString();
    }

    private void UpdateSfxLabel(float val)
    {
        if (_sfxValueLabel != null)
            _sfxValueLabel.text = Mathf.RoundToInt(val).ToString();
    }

    // ?? 洹몃옒???몃뱾???????????????????????????????????????????

    private void ApplyGraphicsSettings(int frameLevel)
    {
        if (settingsData != null)
            settingsData.frameQuality = frameLevel;
        else
            PlayerPrefs.SetInt(KEY_FRAME, frameLevel);

        ApplyGraphicsSettingsSilent(frameLevel);
    }

    private static void ApplyGraphicsSettingsSilent(int frameLevel)
    {
        // 0=??60fps), 1=以?30fps), 2=??30fps + 理쒖? ?덉쭏)
        Application.targetFrameRate = frameLevel == 0 ? 60 : 30;
        QualitySettings.SetQualityLevel(frameLevel == 2 ? 0 : 2, true);
    }

    // ?? 怨꾩젙 ?????????????????????????????????????????????????

    private void RefreshAccountInfo()
    {
        string uid = playerData != null && !string.IsNullOrEmpty(playerData.uid)
            ? playerData.uid
            : "UID-000000";

        if (_uidLabel         != null) _uidLabel.text         = uid;
        if (_loginMethodLabel != null) _loginMethodLabel.text = defaultLoginMethod;
    }

    private void CopyUid()
    {
        if (_uidLabel == null) return;
        GUIUtility.systemCopyBuffer = _uidLabel.text;
        Debug.Log($"[SettingPanel] UID 蹂듭궗: {_uidLabel.text}");
    }

    // ?? 濡쒓렇?꾩썐 ?뺤씤 ?앹뾽 ???????????????????????????????????

    private void ShowConfirmDialog()
    {
        if (_confirmOverlay != null)
            _confirmOverlay.style.display = DisplayStyle.Flex;
    }

    private void HideConfirmDialog()
    {
        if (_confirmOverlay != null)
            _confirmOverlay.style.display = DisplayStyle.None;
    }

    private void OnLogoutConfirmed()
    {
        HideConfirmDialog();
        Debug.Log("[SettingPanel] Logout confirmed - clearing account keys only.");

        playerData?.ClearLoginState();

        PlayerPrefs.DeleteKey("uid");
        PlayerPrefs.DeleteKey("uid_temp");
        PlayerPrefs.DeleteKey("nickname");
        PlayerPrefs.DeleteKey("lastServer");
        PlayerPrefs.DeleteKey("isNewUser");
        PlayerPrefs.Save();

        if (AsyncSceneLoader.Instance != null)
            AsyncSceneLoader.Instance.LoadSceneAsync("Title", LoadSceneMode.Single);
        else
            SceneManager.LoadScene("Title");
    }
}
