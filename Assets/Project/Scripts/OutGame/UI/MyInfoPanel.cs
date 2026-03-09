using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 내 정보 패널.
///
/// 기능:
///   - 대표 캐릭터 이미지
///   - 닉네임 / 계정 레벨 / 경험치 바
///   - 보유 캐릭터 리스트 → 선택 시 로비 대표 캐릭터 변경
///   - 착용 캐릭터 스킬 아이콘 3종 (터치 시 툴팁 토글)
///
/// [Inspector 연결 가이드]
/// ┌ Data
/// │  ├ playerData        : PlayerData.asset
/// │  ├ skillTable        : SkillTable.asset
/// │  └ characters[]     : 보유 캐릭터별 CharacterEntry (AgentData + (선택)초상화 오버라이드 + 스킬 ID 3개)
/// ├ Profile
/// │  ├ representativePortrait : 중앙 대표 캐릭터 Image
/// │  ├ nicknameText           : 닉네임 TMP_Text
/// │  ├ levelText              : "Lv.N" TMP_Text
/// │  ├ expBar                 : 경험치 Slider
/// │  └ expText                : "N / M" TMP_Text
/// ├ Character List
/// │  ├ characterListContent   : ScrollRect Content Transform
/// │  └ characterSlotPrefab    : 슬롯 프리팹 (루트에 Button, 하위에 Image 포함)
/// ├ Skill Icons (3 슬롯)
/// │  ├ skillIconImages[0‥2]   : 스킬 아이콘 Image
/// │  └ skillIconButtons[0‥2]  : 아이콘 위 Button (터치 감지)
/// ├ Tooltip
/// │  ├ tooltipRoot            : 툴팁 루트 GameObject (기본 비활성)
/// │  ├ tooltipTitleText       : 스킬 이름 TMP_Text
/// │  └ tooltipDescText        : 스킬 설명 TMP_Text
/// ├ Close
/// │  └ closeButton            : 패널 닫기 버튼
/// └ Events (Optional)
///    └ onRepresentativeAgentChanged : 대표 캐릭터 변경 시 발행
/// </summary>
[DisallowMultipleComponent]
public class MyInfoPanel : MonoBehaviour
{
    // ── 내부 캐릭터 설정 ─────────────────────────────────────

    /// <summary>보유 캐릭터 1명에 대응하는 인스펙터 설정 묶음.</summary>
    [Serializable]
    private class CharacterEntry
    {
        [Tooltip("AgentData.asset (agentId, displayName, characterSkillId 등)")]
        public AgentData agentData;

        [Tooltip("캐릭터 초상화 스프라이트(선택). 비워두면 AgentData.portrait를 사용합니다.")]
        public Sprite portrait;

        [Tooltip("이 캐릭터의 스킬 ID 3개 (SkillTable에서 조회). 빈 칸은 빈 슬롯으로 표시됩니다.")]
        public string[] skillIds = new string[3];
    }

    // ── PlayerPrefs 키 ────────────────────────────────────────

    private const string PrefKeyNickname = "player.nickname";
    private const string PrefKeyLevel    = "player.level";
    private const string PrefKeyExp      = "player.exp";
    private const string PrefKeyExpMax   = "player.expmax";

    // ── 인스펙터 ─────────────────────────────────────────────

    [Header("Data")]
    [SerializeField] private PlayerData playerData;
    [SerializeField] private SkillTable skillTable;
    [Tooltip("보유 캐릭터 목록. 인덱스는 PlayerData.currentAgentIndex 와 일치해야 합니다.")]
    [SerializeField] private CharacterEntry[] characters;

    [Header("Profile")]
    [SerializeField] private Image       representativePortrait;
    [SerializeField] private TMP_Text    nicknameText;
    [SerializeField] private TMP_Text    levelText;
    [SerializeField] private Slider      expBar;
    [SerializeField] private TMP_Text    expText;

    [Header("Character List")]
    [Tooltip("ScrollRect의 Content Transform. 슬롯이 이 하위에 생성됩니다.")]
    [SerializeField] private Transform   characterListContent;
    [Tooltip("슬롯 프리팹. 루트에 Button, 하위에 Image 컴포넌트가 있어야 합니다.")]
    [SerializeField] private GameObject  characterSlotPrefab;

    [Header("Skill Icons (3 슬롯)")]
    [Tooltip("스킬 아이콘을 표시할 Image 3개 (0=슬롯1, 1=슬롯2, 2=슬롯3)")]
    [SerializeField] private Image[]  skillIconImages  = new Image[3];
    [Tooltip("아이콘 터치 감지용 Button 3개. 동일 순서로 연결하세요.")]
    [SerializeField] private Button[] skillIconButtons = new Button[3];

    [Header("Tooltip")]
    [Tooltip("툴팁 루트 오브젝트. 비활성 상태로 시작합니다.")]
    [SerializeField] private GameObject tooltipRoot;
    [SerializeField] private TMP_Text   tooltipTitleText;
    [SerializeField] private TMP_Text   tooltipDescText;

    [Header("Close")]
    [SerializeField] private Button closeButton;

    [Header("Events (Optional)")]
    [SerializeField] private VoidEventChannelSO onRepresentativeAgentChanged;

    // ── 내부 상태 ─────────────────────────────────────────────

    private int                _selectedIndex;
    private readonly List<Button> _slotButtons = new();
    private readonly SkillRow[]   _skillRows   = new SkillRow[3];
    private int                _tooltipOpenSlot = -1;

    // ─────────────────────────────────────────────────────────

    private void Awake()
    {
        closeButton?.onClick.AddListener(Close);
        if (tooltipRoot != null) tooltipRoot.SetActive(false);
    }

    private void OnEnable()
    {
        _selectedIndex = playerData != null ? playerData.currentAgentIndex : 0;
        _tooltipOpenSlot = -1;
        if (tooltipRoot != null) tooltipRoot.SetActive(false);

        RefreshProfile();
        BuildCharacterList();
        RefreshSkillIcons();
        BindSkillButtons();
    }

    // ── Public API ────────────────────────────────────────────

    /// <summary>패널을 엽니다.</summary>
    public void Open() => gameObject.SetActive(true);

    /// <summary>패널을 닫습니다.</summary>
    public void Close() => gameObject.SetActive(false);

    /// <summary>레벨·경험치를 외부에서 갱신하고 저장합니다.</summary>
    public void SetAccountStats(int level, int exp, int expMax)
    {
        PlayerPrefs.SetInt(PrefKeyLevel,  level);
        PlayerPrefs.SetInt(PrefKeyExp,    exp);
        PlayerPrefs.SetInt(PrefKeyExpMax, expMax);
        PlayerPrefs.Save();
        ApplyLevelExp(level, exp, expMax);
    }

    /// <summary>닉네임을 외부에서 변경하고 저장합니다.</summary>
    public void SetNickname(string nickname)
    {
        PlayerPrefs.SetString(PrefKeyNickname, nickname);
        PlayerPrefs.Save();
        if (nicknameText != null) nicknameText.text = nickname;
    }

    // ── 프로필 갱신 ───────────────────────────────────────────

    private void RefreshProfile()
    {
        string nick   = PlayerPrefs.GetString(PrefKeyNickname, "모험가");
        int    level  = PlayerPrefs.GetInt(PrefKeyLevel,  1);
        int    exp    = PlayerPrefs.GetInt(PrefKeyExp,    0);
        int    expMax = PlayerPrefs.GetInt(PrefKeyExpMax, 100);

        if (nicknameText != null) nicknameText.text = nick;
        ApplyLevelExp(level, exp, expMax);
        RefreshRepresentativePortrait();
    }

    private void ApplyLevelExp(int level, int exp, int expMax)
    {
        if (levelText != null) levelText.text = $"Lv.{level}";

        if (expBar != null)
        {
            expBar.minValue = 0f;
            expBar.maxValue = expMax;
            expBar.SetValueWithoutNotify(Mathf.Clamp(exp, 0, expMax));
        }

        if (expText != null) expText.text = $"{exp:N0} / {expMax:N0}";
    }

    private void RefreshRepresentativePortrait()
    {
        if (representativePortrait == null || characters == null || characters.Length == 0) return;
        int idx = Mathf.Clamp(_selectedIndex, 0, characters.Length - 1);
        CharacterEntry entry = characters[idx];
        Sprite portrait = entry.portrait != null ? entry.portrait : entry.agentData?.portrait;
        representativePortrait.sprite = portrait;
    }

    // ── 캐릭터 리스트 ─────────────────────────────────────────

    private void BuildCharacterList()
    {
        if (characterListContent == null || characterSlotPrefab == null || characters == null) return;

        // 기존 슬롯 제거
        foreach (Button btn in _slotButtons)
            if (btn != null) Destroy(btn.gameObject);
        _slotButtons.Clear();

        for (int i = 0; i < characters.Length; i++)
        {
            int capturedIndex = i;
            CharacterEntry entry = characters[i];

            GameObject go  = Instantiate(characterSlotPrefab, characterListContent);
            Button     btn = go.GetComponent<Button>();
            Image      img = go.GetComponentInChildren<Image>();

            // 초상화 적용
            if (img != null)
            {
                Sprite portrait = entry.portrait != null ? entry.portrait : entry.agentData?.portrait;
                if (portrait != null)
                    img.sprite = portrait;
            }

            // 에이전트 이름 (TMP_Text가 있으면 표시)
            TMP_Text label = go.GetComponentInChildren<TMP_Text>();
            if (label != null && entry.agentData != null)
                label.text = entry.agentData.displayName;

            if (btn != null)
            {
                btn.onClick.AddListener(() => OnCharacterSlotSelected(capturedIndex));
                _slotButtons.Add(btn);
            }
        }

        UpdateSlotHighlights();
    }

    private void OnCharacterSlotSelected(int index)
    {
        if (_selectedIndex == index) return;

        _selectedIndex = index;

        if (playerData != null)
            playerData.currentAgentIndex = _selectedIndex;

        RefreshRepresentativePortrait();
        RefreshSkillIcons();
        BindSkillButtons();
        UpdateSlotHighlights();
        HideTooltip();

        onRepresentativeAgentChanged?.RaiseEvent();

        Debug.Log($"[MyInfoPanel] 대표 캐릭터 변경 → 인덱스 {_selectedIndex}" +
                  $" ({(characters[_selectedIndex].agentData != null ? characters[_selectedIndex].agentData.displayName : "?")})");
    }

    /// <summary>현재 선택된 슬롯만 비활성화(선택 강조)하고 나머지는 활성화합니다.</summary>
    private void UpdateSlotHighlights()
    {
        for (int i = 0; i < _slotButtons.Count; i++)
        {
            if (_slotButtons[i] == null) continue;
            _slotButtons[i].interactable = (i != _selectedIndex);
        }
    }

    // ── 스킬 아이콘 ───────────────────────────────────────────

    private void RefreshSkillIcons()
    {
        if (characters == null || characters.Length == 0) return;

        int agentIdx = Mathf.Clamp(_selectedIndex, 0, characters.Length - 1);
        string[] ids = characters[agentIdx].skillIds;

        for (int slot = 0; slot < 3; slot++)
        {
            SkillRow row = null;

            if (skillTable != null && ids != null && slot < ids.Length && !string.IsNullOrWhiteSpace(ids[slot]))
                row = skillTable.GetById(ids[slot]);

            _skillRows[slot] = row;

            if (slot < skillIconImages.Length && skillIconImages[slot] != null)
                skillIconImages[slot].sprite = row?.icon;

            // 아이콘 알파: 스킬 미설정 슬롯은 반투명 처리
            if (slot < skillIconImages.Length && skillIconImages[slot] != null)
            {
                Color c = skillIconImages[slot].color;
                c.a = row != null ? 1f : 0.3f;
                skillIconImages[slot].color = c;
            }
        }
    }

    // ── 툴팁 ─────────────────────────────────────────────────

    private void BindSkillButtons()
    {
        for (int slot = 0; slot < skillIconButtons.Length && slot < 3; slot++)
        {
            if (skillIconButtons[slot] == null) continue;

            int capturedSlot = slot;
            skillIconButtons[slot].onClick.RemoveAllListeners();
            skillIconButtons[slot].onClick.AddListener(() => OnSkillIconTapped(capturedSlot));
        }
    }

    private void OnSkillIconTapped(int slot)
    {
        if (_tooltipOpenSlot == slot)
        {
            // 같은 슬롯 재터치 → 툴팁 닫기
            HideTooltip();
            return;
        }

        SkillRow row = _skillRows[slot];
        if (row == null)
        {
            HideTooltip();
            return;
        }

        ShowTooltip(slot, row);
    }

    private void ShowTooltip(int slot, SkillRow row)
    {
        _tooltipOpenSlot = slot;

        if (tooltipTitleText != null) tooltipTitleText.text = row.name;
        if (tooltipDescText  != null) tooltipDescText.text  = row.description;

        tooltipRoot?.SetActive(true);
    }

    private void HideTooltip()
    {
        _tooltipOpenSlot = -1;
        tooltipRoot?.SetActive(false);
    }
}
