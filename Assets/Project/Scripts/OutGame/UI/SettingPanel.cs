using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

/// <summary>
/// UIToolkit(UIDocument) 기반 설정 패널.
///
/// [Inspector 연결 가이드]
/// ┌ uiDocument        : 이 GameObject의 UIDocument 컴포넌트
/// ├ audioMixer        : 프로젝트 AudioMixer (BGMVolume / SFXVolume 파라미터 필요)
/// ├ playerData        : PlayerData.asset (계정 탭 UID 표시용, 선택)
/// └ defaultLoginMethod: "Guest" 등 로그인 방식 기본값
///
/// [LobbyManager 연동]
/// LobbyManager.settingPanel 에 이 컴포넌트를 연결하면
/// 설정 버튼 클릭 시 OpenPanel()이 자동 호출됩니다.
///
/// [AudioMixer 파라미터]
/// Exposed Parameter 이름을 반드시 "BGMVolume", "SFXVolume" 으로 지정하세요.
/// </summary>
[DisallowMultipleComponent]
public class SettingPanel : MonoBehaviour
{
    public static SettingPanel Instance { get; private set; }

    // ── Inspector ──────────────────────────────────────────

    [Header("UI")]
    [SerializeField] private UIDocument uiDocument;

    [Header("Audio")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("Account (선택)")]
    [SerializeField] private PlayerData playerData;
    [SerializeField] private string defaultLoginMethod = "Guest";

    // ── PlayerPrefs 키 ─────────────────────────────────────

    private const string KEY_BGM_VOL  = "bgmVol";
    private const string KEY_SFX_VOL  = "sfxVol";
    private const string KEY_BGM_MUTE = "bgmMute";
    private const string KEY_SFX_MUTE = "sfxMute";
    private const string KEY_FRAME    = "frameQuality";
    private const string KEY_SHAKE    = "shakeOn";
    private const string KEY_BLOOM    = "bloomOn";
    private const string KEY_BLUR     = "blurOn";

    // AudioMixer Exposed Parameter 이름
    private const string MIXER_BGM = "BGMVolume";
    private const string MIXER_SFX = "SFXVolume";

    // ── UI 캐시 ────────────────────────────────────────────

    // 배경/루트
    private VisualElement _backdrop;

    // 탭 버튼
    private Button _tabGraphicsBtn;
    private Button _tabSoundBtn;
    private Button _tabAccountBtn;

    // 탭 콘텐츠
    private VisualElement _graphicsTab;
    private VisualElement _soundTab;
    private VisualElement _accountTab;

    // 그래픽: 프레임 라디오 [0]=상(60fps) [1]=중(30fps) [2]=하(저화질)
    private RadioButton[] _frameButtons;
    private Toggle        _shakeToggle;
    private Toggle        _bloomToggle;
    private Toggle        _blurToggle;

    // 사운드
    private Slider _bgmSlider;
    private Slider _sfxSlider;
    private Label  _bgmValueLabel;
    private Label  _sfxValueLabel;
    private Toggle _bgmMuteToggle;
    private Toggle _sfxMuteToggle;

    // 음소거 전 볼륨 캐시 (슬라이더 수치 유지, 출력만 0)
    private float _bgmVolCache;
    private float _sfxVolCache;

    // 계정
    private Label  _uidLabel;
    private Label  _loginMethodLabel;
    private Button _logoutBtn;
    private Button _copyBtn;

    // 로그아웃 확인 팝업
    private VisualElement _confirmOverlay;
    private Button        _confirmYesBtn;
    private Button        _confirmNoBtn;

    // ─────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        BindUI();
        Hide();
    }

    // ── 공개 API ─────────────────────────────────────────────

    /// <summary>설정 패널을 표시하고 저장된 값으로 초기화합니다.</summary>
    public void Show()
    {
        if (_backdrop == null) return;

        _backdrop.style.display = DisplayStyle.Flex;
        LoadSettings();
        SwitchTab(0);
    }

    /// <summary>모든 설정을 저장하고 패널을 숨깁니다.</summary>
    public void Hide()
    {
        if (_backdrop == null) return;

        SaveAllSettings();
        _backdrop.style.display = DisplayStyle.None;
    }

    /// <summary>LobbyManager 호환 — Show()와 동일합니다.</summary>
    public void OpenPanel() => Show();

    /// <summary>LobbyManager 호환 — Hide()와 동일합니다.</summary>
    public void ClosePanel() => Hide();

    // ── UI 바인딩 ─────────────────────────────────────────────

    private void BindUI()
    {
        if (uiDocument == null)
        {
            Debug.LogError("[SettingPanel] UIDocument가 할당되지 않았습니다.");
            return;
        }

        var root = uiDocument.rootVisualElement;

        _backdrop = root.Q<VisualElement>("setting-backdrop");

        // 헤더 닫기
        root.Q<Button>("close-btn")?.RegisterCallback<ClickEvent>(_ => Hide());

        // 탭 버튼
        _tabGraphicsBtn = root.Q<Button>("tab-graphics");
        _tabSoundBtn    = root.Q<Button>("tab-sound");
        _tabAccountBtn  = root.Q<Button>("tab-account");

        _tabGraphicsBtn?.RegisterCallback<ClickEvent>(_ => SwitchTab(0));
        _tabSoundBtn?.RegisterCallback<ClickEvent>(_    => SwitchTab(1));
        _tabAccountBtn?.RegisterCallback<ClickEvent>(_  => SwitchTab(2));

        // 탭 콘텐츠
        _graphicsTab = root.Q<VisualElement>("graphics-tab");
        _soundTab    = root.Q<VisualElement>("sound-tab");
        _accountTab  = root.Q<VisualElement>("account-tab");

        // ── 그래픽 ──────────────────────────────────────────

        _frameButtons = new RadioButton[]
        {
            root.Q<RadioButton>("frame-high"),
            root.Q<RadioButton>("frame-mid"),
            root.Q<RadioButton>("frame-low"),
        };

        _frameButtons[0]?.RegisterValueChangedCallback(e => { if (e.newValue) ApplyGraphicsSettings(0); });
        _frameButtons[1]?.RegisterValueChangedCallback(e => { if (e.newValue) ApplyGraphicsSettings(1); });
        _frameButtons[2]?.RegisterValueChangedCallback(e => { if (e.newValue) ApplyGraphicsSettings(2); });

        _shakeToggle = root.Q<Toggle>("shake-toggle");
        _bloomToggle = root.Q<Toggle>("bloom-toggle");
        _blurToggle  = root.Q<Toggle>("blur-toggle");

        _shakeToggle?.RegisterValueChangedCallback(e => PlayerPrefs.SetInt(KEY_SHAKE, e.newValue ? 1 : 0));
        _bloomToggle?.RegisterValueChangedCallback(e => PlayerPrefs.SetInt(KEY_BLOOM, e.newValue ? 1 : 0));
        _blurToggle?.RegisterValueChangedCallback(e  => PlayerPrefs.SetInt(KEY_BLUR,  e.newValue ? 1 : 0));

        // ── 사운드 ──────────────────────────────────────────

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

        // ── 계정 ────────────────────────────────────────────

        _uidLabel         = root.Q<Label>("uid-label");
        _loginMethodLabel = root.Q<Label>("login-method-label");
        _logoutBtn        = root.Q<Button>("logout-btn");
        _copyBtn          = root.Q<Button>("copy-btn");

        _logoutBtn?.RegisterCallback<ClickEvent>(_ => ShowConfirmDialog());
        _copyBtn?.RegisterCallback<ClickEvent>(_   => CopyUid());

        // ── 확인 팝업 ────────────────────────────────────────

        _confirmOverlay = root.Q<VisualElement>("confirm-overlay");
        _confirmYesBtn  = root.Q<Button>("confirm-yes-btn");
        _confirmNoBtn   = root.Q<Button>("confirm-no-btn");

        _confirmYesBtn?.RegisterCallback<ClickEvent>(_ => OnLogoutConfirmed());
        _confirmNoBtn?.RegisterCallback<ClickEvent>(_  => HideConfirmDialog());
    }

    // ── 탭 전환 ──────────────────────────────────────────────

    /// <param name="tabIndex">0=그래픽, 1=사운드, 2=계정</param>
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

    // ── 설정 로드 ─────────────────────────────────────────────

    private void LoadSettings()
    {
        float bgm    = PlayerPrefs.GetFloat(KEY_BGM_VOL,  80f);
        float sfx    = PlayerPrefs.GetFloat(KEY_SFX_VOL,  80f);
        bool bgmMute = PlayerPrefs.GetInt(KEY_BGM_MUTE, 0) == 1;
        bool sfxMute = PlayerPrefs.GetInt(KEY_SFX_MUTE, 0) == 1;
        int  frame   = Mathf.Clamp(PlayerPrefs.GetInt(KEY_FRAME, 1), 0, 2);
        bool shake   = PlayerPrefs.GetInt(KEY_SHAKE, 1) == 1;
        bool bloom   = PlayerPrefs.GetInt(KEY_BLOOM, 1) == 1;
        bool blur    = PlayerPrefs.GetInt(KEY_BLUR,  0) == 1;

        _bgmVolCache = bgm;
        _sfxVolCache = sfx;

        // 슬라이더 — 이벤트 없이 세팅
        _bgmSlider?.SetValueWithoutNotify(bgm);
        _sfxSlider?.SetValueWithoutNotify(sfx);
        UpdateBgmLabel(bgm);
        UpdateSfxLabel(sfx);

        // 음소거 토글 — 이벤트 없이 세팅
        _bgmMuteToggle?.SetValueWithoutNotify(bgmMute);
        _sfxMuteToggle?.SetValueWithoutNotify(sfxMute);

        // 프레임 라디오 — 이벤트 없이 세팅
        for (int i = 0; i < _frameButtons.Length; i++)
            _frameButtons[i]?.SetValueWithoutNotify(i == frame);

        // 그래픽 토글 — 이벤트 없이 세팅
        _shakeToggle?.SetValueWithoutNotify(shake);
        _bloomToggle?.SetValueWithoutNotify(bloom);
        _blurToggle?.SetValueWithoutNotify(blur);

        // 실제 적용
        ApplyBgmVolume(bgmMute ? 0f : bgm);
        ApplySfxVolume(sfxMute ? 0f : sfx);
        ApplyGraphicsSettingsSilent(frame);
    }

    // ── 설정 저장 ─────────────────────────────────────────────

    private void SaveAllSettings()
    {
        if (_bgmSlider     != null) PlayerPrefs.SetFloat(KEY_BGM_VOL,  _bgmSlider.value);
        if (_sfxSlider     != null) PlayerPrefs.SetFloat(KEY_SFX_VOL,  _sfxSlider.value);
        if (_bgmMuteToggle != null) PlayerPrefs.SetInt(KEY_BGM_MUTE, _bgmMuteToggle.value ? 1 : 0);
        if (_sfxMuteToggle != null) PlayerPrefs.SetInt(KEY_SFX_MUTE, _sfxMuteToggle.value ? 1 : 0);
        if (_shakeToggle   != null) PlayerPrefs.SetInt(KEY_SHAKE, _shakeToggle.value ? 1 : 0);
        if (_bloomToggle   != null) PlayerPrefs.SetInt(KEY_BLOOM, _bloomToggle.value ? 1 : 0);
        if (_blurToggle    != null) PlayerPrefs.SetInt(KEY_BLUR,  _blurToggle.value  ? 1 : 0);

        for (int i = 0; i < _frameButtons.Length; i++)
        {
            if (_frameButtons[i] != null && _frameButtons[i].value)
            {
                PlayerPrefs.SetInt(KEY_FRAME, i);
                break;
            }
        }

        PlayerPrefs.Save();
    }

    // ── 사운드 핸들러 ─────────────────────────────────────────

    private void OnBgmSliderChanged(ChangeEvent<float> evt)
    {
        // 음소거 중이 아닐 때만 캐시 갱신 (음소거 해제 시 복원용)
        if (_bgmMuteToggle == null || !_bgmMuteToggle.value)
            _bgmVolCache = evt.newValue;

        ApplyBgmVolume(_bgmMuteToggle != null && _bgmMuteToggle.value ? 0f : evt.newValue);
        UpdateBgmLabel(evt.newValue);
    }

    private void OnSfxSliderChanged(ChangeEvent<float> evt)
    {
        if (_sfxMuteToggle == null || !_sfxMuteToggle.value)
            _sfxVolCache = evt.newValue;

        ApplySfxVolume(_sfxMuteToggle != null && _sfxMuteToggle.value ? 0f : evt.newValue);
        UpdateSfxLabel(evt.newValue);
    }

    private void OnBgmMuteToggled(ChangeEvent<bool> evt)
    {
        // ON : 출력만 0 (슬라이더 수치 유지)
        // OFF: 캐시로 복원
        ApplyBgmVolume(evt.newValue ? 0f : _bgmVolCache);
    }

    private void OnSfxMuteToggled(ChangeEvent<bool> evt)
    {
        ApplySfxVolume(evt.newValue ? 0f : _sfxVolCache);
    }

    private void ApplyBgmVolume(float vol)
    {
        float db = vol > 0f ? Mathf.Log10(vol / 100f) * 20f : -80f;
        audioMixer?.SetFloat(MIXER_BGM, db);
    }

    private void ApplySfxVolume(float vol)
    {
        float db = vol > 0f ? Mathf.Log10(vol / 100f) * 20f : -80f;
        audioMixer?.SetFloat(MIXER_SFX, db);
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

    // ── 그래픽 핸들러 ─────────────────────────────────────────

    private void ApplyGraphicsSettings(int frameLevel)
    {
        PlayerPrefs.SetInt(KEY_FRAME, frameLevel);
        ApplyGraphicsSettingsSilent(frameLevel);
    }

    private static void ApplyGraphicsSettingsSilent(int frameLevel)
    {
        // 0=상(60fps), 1=중(30fps), 2=하(30fps + 최저 품질)
        Application.targetFrameRate = frameLevel == 0 ? 60 : 30;
        QualitySettings.SetQualityLevel(frameLevel == 2 ? 0 : 2, true);
    }

    // ── 계정 탭 ───────────────────────────────────────────────

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
        Debug.Log($"[SettingPanel] UID 복사: {_uidLabel.text}");
    }

    // ── 로그아웃 확인 팝업 ───────────────────────────────────

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
        Debug.Log("[SettingPanel] 로그아웃 확인 → PlayerPrefs 초기화 후 Title 씬 이동");

        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        if (AsyncSceneLoader.Instance != null)
            AsyncSceneLoader.Instance.LoadSceneAsync("Title", LoadSceneMode.Single);
        else
            SceneManager.LoadScene("Title");
    }
}
