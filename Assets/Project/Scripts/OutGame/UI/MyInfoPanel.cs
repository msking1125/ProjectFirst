using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ProjectFirst.Data;
/// <summary>
/// 내 정보 패널입니다.
///
/// 기능:
/// - 대표 캐릭터 이미지 표시
/// - 닉네임 / 계정 레벨 / 경험치 바 갱신
/// - 보유 캐릭터 목록 선택 시 대표 캐릭터 변경
/// - 스킬 아이콘 3종 표시 및 툴팁 토글
/// </summary>
[DisallowMultipleComponent]
public class MyInfoPanel : MonoBehaviour
{
    /// <summary>보유 캐릭터 1명에 대응하는 인스펙터 설정 묶음.</summary>
    [Serializable]
    private class CharacterEntry
    {
        [Tooltip("AgentData.asset (agentId, displayName, characterSkillId 등)")]
        public AgentData agentData;

        [Tooltip("캐릭터 초상화 스프라이트")]
        public Sprite portrait;

        [Tooltip("이 캐릭터의 스킬 ID 3개 (SkillTable에서 조회)")]
        public int[] skillIds = new int[3];
    }

    private const string PrefKeyNickname = "player.nickname";
    private const string PrefKeyLevel    = "player.level";
    private const string PrefKeyExp      = "player.exp";
    private const string PrefKeyExpMax   = "player.expmax";

    [Header("Data")]
    [SerializeField] private PlayerData playerData;
    [SerializeField] private SkillTable skillTable;
    [Tooltip("보유 캐릭터 목록. 인덱스는 PlayerData.currentAgentIndex와 일치해야 합니다.")]
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
    [Tooltip("슬롯 프리팹. 루트에 Button, 하위에 Image가 있어야 합니다.")]
    [SerializeField] private GameObject  characterSlotPrefab;

    [Header("Skill Icons (3 슬롯)")]
    [Tooltip("스킬 아이콘을 표시할 Image 3개")]
    [SerializeField] private Image[]  skillIconImages  = new Image[3];
    [Tooltip("아이콘 터치 감지용 Button 3개")]
    [SerializeField] private Button[] skillIconButtons = new Button[3];

    [Header("Tooltip")]
    [Tooltip("툴팁 루트 오브젝트. 비활성 상태로 시작합니다.")]
    [SerializeField] private GameObject tooltipRoot;
    [SerializeField] private TMP_Text   tooltipTitleText;
    [SerializeField] private TMP_Text   tooltipDescText;

    [Header("Close")]
    [SerializeField] private Button closeButton;

    [Header("Events (선택)")]
    [SerializeField] private VoidEventChannelSO onRepresentativeAgentChanged;

    private int                _selectedIndex;
    private readonly List<Button> _slotButtons = new();
    private readonly SkillRow[]   _skillRows   = new SkillRow[3];
    private int                _tooltipOpenSlot = -1;

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
    /// <summary>패널을 엽니다.</summary>
    public void Open() => gameObject.SetActive(true);
    /// <summary>패널을 닫습니다.</summary>
    public void Close() => gameObject.SetActive(false);
    /// <summary>계정 레벨과 경험치를 갱신합니다.</summary>
    public void SetAccountStats(int level, int exp, int expMax)
    {
        if (playerData != null)
            playerData.SetAccountStats(level, exp, expMax);

        ApplyLevelExp(level, exp, expMax);
    }
    /// <summary>닉네임을 갱신합니다.</summary>
    public void SetNickname(string nickname)
    {
        if (playerData != null)
            playerData.SetNicknameValue(nickname);

        if (nicknameText != null) nicknameText.text = nickname;
    }

    private void RefreshProfile()
    {
        string nick = playerData != null ? playerData.GetNicknameOrDefault("모험가") : PlayerPrefs.GetString(PrefKeyNickname, "모험가");
        int level = playerData != null ? playerData.GetAccountLevel(1) : PlayerPrefs.GetInt(PrefKeyLevel, 1);
        int exp = playerData != null ? playerData.GetAccountExp() : PlayerPrefs.GetInt(PrefKeyExp, 0);
        int expMax = playerData != null ? playerData.GetAccountExpMax(100) : PlayerPrefs.GetInt(PrefKeyExpMax, 100);

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

    private void BuildCharacterList()
    {
        if (characterListContent == null || characterSlotPrefab == null || characters == null) return;
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
            if (img != null)
            {
                Sprite portrait = entry.portrait != null ? entry.portrait : entry.agentData?.portrait;
                if (portrait != null)
                    img.sprite = portrait;
            }
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

        Debug.Log("[MyInfoPanel] 대표 캐릭터가 변경되었습니다.");
    }
    private void UpdateSlotHighlights()
    {
        for (int i = 0; i < _slotButtons.Count; i++)
        {
            if (_slotButtons[i] == null) continue;
            _slotButtons[i].interactable = (i != _selectedIndex);
        }
    }

    private void RefreshSkillIcons()
    {
        if (characters == null || characters.Length == 0) return;

        int agentIdx = Mathf.Clamp(_selectedIndex, 0, characters.Length - 1);
        int[] ids = characters[agentIdx].skillIds;

        for (int slot = 0; slot < 3; slot++)
        {
            SkillRow row = null;

            if (skillTable != null && ids != null && slot < ids.Length && ids[slot] > 0)
                row = skillTable.GetById(ids[slot]);

            _skillRows[slot] = row;

            if (slot < skillIconImages.Length && skillIconImages[slot] != null)
                skillIconImages[slot].sprite = row?.icon;
            if (slot < skillIconImages.Length && skillIconImages[slot] != null)
            {
                Color c = skillIconImages[slot].color;
                c.a = row != null ? 1f : 0.3f;
                skillIconImages[slot].color = c;
            }
        }
    }

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









