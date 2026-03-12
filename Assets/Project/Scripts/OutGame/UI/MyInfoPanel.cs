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
/// Documentation cleaned.
///
/// Documentation cleaned.
/// Documentation cleaned.
/// Documentation cleaned.
/// Documentation cleaned.
/// Documentation cleaned.
///
/// Documentation cleaned.
/// Documentation cleaned.
/// Documentation cleaned.
/// Documentation cleaned.
/// Documentation cleaned.
/// Documentation cleaned.
/// Documentation cleaned.
/// Documentation cleaned.
/// Documentation cleaned.
/// Documentation cleaned.
/// Documentation cleaned.
/// Documentation cleaned.
/// Documentation cleaned.
/// Documentation cleaned.
/// Documentation cleaned.
/// Documentation cleaned.
/// Documentation cleaned.
/// Documentation cleaned.
/// Documentation cleaned.
/// Documentation cleaned.
/// Documentation cleaned.
/// Documentation cleaned.
/// Documentation cleaned.
/// Documentation cleaned.
/// Documentation cleaned.
/// </summary>
[DisallowMultipleComponent]
public class MyInfoPanel : MonoBehaviour
{
    // Note: cleaned comment.

    /// Documentation cleaned.
    [Serializable]
    private class CharacterEntry
    {
        [Tooltip("Configured in inspector.")]
        public AgentData agentData;

        [Tooltip("Configured in inspector.")]
        public Sprite portrait;

        [Tooltip("Configured in inspector.")]
        public int[] skillIds = new int[3];
    }

    // Note: cleaned comment.

    private const string PrefKeyNickname = "player.nickname";
    private const string PrefKeyLevel    = "player.level";
    private const string PrefKeyExp      = "player.exp";
    private const string PrefKeyExpMax   = "player.expmax";

    // Note: cleaned comment.

    [Header("Data")]
    [SerializeField] private PlayerData playerData;
    [SerializeField] private SkillTable skillTable;
    [Tooltip("Configured in inspector.")]
    [SerializeField] private CharacterEntry[] characters;

    [Header("Profile")]
    [SerializeField] private Image       representativePortrait;
    [SerializeField] private TMP_Text    nicknameText;
    [SerializeField] private TMP_Text    levelText;
    [SerializeField] private Slider      expBar;
    [SerializeField] private TMP_Text    expText;

    [Header("Character List")]
    [Tooltip("Configured in inspector.")]
    [SerializeField] private Transform   characterListContent;
    [Tooltip("Configured in inspector.")]
    [SerializeField] private GameObject  characterSlotPrefab;

    [Header("Settings")]
    [Tooltip("Configured in inspector.")]
    [SerializeField] private Image[]  skillIconImages  = new Image[3];
    [Tooltip("Configured in inspector.")]
    [SerializeField] private Button[] skillIconButtons = new Button[3];

    [Header("Tooltip")]
    [Tooltip("Configured in inspector.")]
    [SerializeField] private GameObject tooltipRoot;
    [SerializeField] private TMP_Text   tooltipTitleText;
    [SerializeField] private TMP_Text   tooltipDescText;

    [Header("Close")]
    [SerializeField] private Button closeButton;

    [Header("Events (Optional)")]
    [SerializeField] private VoidEventChannelSO onRepresentativeAgentChanged;

    // Note: cleaned comment.

    private int                _selectedIndex;
    private readonly List<Button> _slotButtons = new();
    private readonly SkillRow[]   _skillRows   = new SkillRow[3];
    private int                _tooltipOpenSlot = -1;

    // Note: cleaned comment.

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

    // Note: cleaned comment.

    /// Documentation cleaned.
    public void Open() => gameObject.SetActive(true);

    /// Documentation cleaned.
    public void Close() => gameObject.SetActive(false);

    /// Documentation cleaned.
    public void SetAccountStats(int level, int exp, int expMax)
    {
        if (playerData != null)
            playerData.SetAccountStats(level, exp, expMax);

        ApplyLevelExp(level, exp, expMax);
    }

    /// Documentation cleaned.
    public void SetNickname(string nickname)
    {
        if (playerData != null)
            playerData.SetNicknameValue(nickname);

        if (nicknameText != null) nicknameText.text = nickname;
    }

    // Note: cleaned comment.

    private void RefreshProfile()
    {
        string nick = playerData != null ? playerData.GetNicknameOrDefault("Player") : PlayerPrefs.GetString(PrefKeyNickname, "Player");
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

    // Note: cleaned comment.

    private void BuildCharacterList()
    {
        if (characterListContent == null || characterSlotPrefab == null || characters == null) return;

        // Note: cleaned comment.
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

            // Note: cleaned comment.
            if (img != null)
            {
                Sprite portrait = entry.portrait != null ? entry.portrait : entry.agentData?.portrait;
                if (portrait != null)
                    img.sprite = portrait;
            }

            // Note: cleaned comment.
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

        Debug.Log("[Log] Message cleaned.");
    }


    /// Documentation cleaned.
    private void UpdateSlotHighlights()
    {
        for (int i = 0; i < _slotButtons.Count; i++)
        {
            if (_slotButtons[i] == null) continue;
            _slotButtons[i].interactable = (i != _selectedIndex);
        }
    }

    // Note: cleaned comment.

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

            // Note: cleaned comment.
            if (slot < skillIconImages.Length && skillIconImages[slot] != null)
            {
                Color c = skillIconImages[slot].color;
                c.a = row != null ? 1f : 0.3f;
                skillIconImages[slot].color = c;
            }
        }
    }

    // Note: cleaned comment.

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
            // Note: cleaned comment.
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




