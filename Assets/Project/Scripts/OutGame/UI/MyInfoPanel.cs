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
/// ???뺣낫 ?⑤꼸.
///
/// 湲곕뒫:
///   - ???罹먮┃???대?吏
///   - ?됰꽕??/ 怨꾩젙 ?덈꺼 / 寃쏀뿕移?諛?
///   - 蹂댁쑀 罹먮┃??由ъ뒪?????좏깮 ??濡쒕퉬 ???罹먮┃??蹂寃?
///   - 李⑹슜 罹먮┃???ㅽ궗 ?꾩씠肄?3醫?(?곗튂 ???댄똻 ?좉?)
///
/// [Inspector ?곌껐 媛?대뱶]
/// ??Data
/// ?? ??playerData        : PlayerData.asset
/// ?? ??skillTable        : SkillTable.asset
/// ?? ??characters[]     : 蹂댁쑀 罹먮┃?곕퀎 CharacterEntry (AgentData + (?좏깮)珥덉긽???ㅻ쾭?쇱씠??+ ?ㅽ궗 ID 3媛?
/// ??Profile
/// ?? ??representativePortrait : 以묒븰 ???罹먮┃??Image
/// ?? ??nicknameText           : ?됰꽕??TMP_Text
/// ?? ??levelText              : "Lv.N" TMP_Text
/// ?? ??expBar                 : 寃쏀뿕移?Slider
/// ?? ??expText                : "N / M" TMP_Text
/// ??Character List
/// ?? ??characterListContent   : ScrollRect Content Transform
/// ?? ??characterSlotPrefab    : ?щ’ ?꾨━??(猷⑦듃??Button, ?섏쐞??Image ?ы븿)
/// ??Skill Icons (3 ?щ’)
/// ?? ??skillIconImages[0??]   : ?ㅽ궗 ?꾩씠肄?Image
/// ?? ??skillIconButtons[0??]  : ?꾩씠肄???Button (?곗튂 媛먯?)
/// ??Tooltip
/// ?? ??tooltipRoot            : ?댄똻 猷⑦듃 GameObject (湲곕낯 鍮꾪솢??
/// ?? ??tooltipTitleText       : ?ㅽ궗 ?대쫫 TMP_Text
/// ?? ??tooltipDescText        : ?ㅽ궗 ?ㅻ챸 TMP_Text
/// ??Close
/// ?? ??closeButton            : ?⑤꼸 ?リ린 踰꾪듉
/// ??Events (Optional)
///    ??onRepresentativeAgentChanged : ???罹먮┃??蹂寃???諛쒗뻾
/// </summary>
[DisallowMultipleComponent]
public class MyInfoPanel : MonoBehaviour
{
    // ?? ?대? 罹먮┃???ㅼ젙 ?????????????????????????????????????

    /// <summary>蹂댁쑀 罹먮┃??1紐낆뿉 ??묓븯???몄뒪?숉꽣 ?ㅼ젙 臾띠쓬.</summary>
    [Serializable]
    private class CharacterEntry
    {
        [Tooltip("AgentData.asset (agentId, displayName, characterSkillId ??")]
        public AgentData agentData;

        [Tooltip("罹먮┃??珥덉긽???ㅽ봽?쇱씠???좏깮). 鍮꾩썙?먮㈃ AgentData.portrait瑜??ъ슜?⑸땲??")]
        public Sprite portrait;

        [Tooltip("??罹먮┃?곗쓽 ?ㅽ궗 ID 3媛?(SkillTable?먯꽌 議고쉶). 鍮?移몄? 鍮??щ’?쇰줈 ?쒖떆?⑸땲??")]
        public int[] skillIds = new int[3];
    }

    // ?? PlayerPrefs ??????????????????????????????????????????

    private const string PrefKeyNickname = "player.nickname";
    private const string PrefKeyLevel    = "player.level";
    private const string PrefKeyExp      = "player.exp";
    private const string PrefKeyExpMax   = "player.expmax";

    // ?? ?몄뒪?숉꽣 ?????????????????????????????????????????????

    [Header("Data")]
    [SerializeField] private PlayerData playerData;
    [SerializeField] private SkillTable skillTable;
    [Tooltip("蹂댁쑀 罹먮┃??紐⑸줉. ?몃뜳?ㅻ뒗 PlayerData.currentAgentIndex ? ?쇱튂?댁빞 ?⑸땲??")]
    [SerializeField] private CharacterEntry[] characters;

    [Header("Profile")]
    [SerializeField] private Image       representativePortrait;
    [SerializeField] private TMP_Text    nicknameText;
    [SerializeField] private TMP_Text    levelText;
    [SerializeField] private Slider      expBar;
    [SerializeField] private TMP_Text    expText;

    [Header("Character List")]
    [Tooltip("ScrollRect??Content Transform. ?щ’?????섏쐞???앹꽦?⑸땲??")]
    [SerializeField] private Transform   characterListContent;
    [Tooltip("?щ’ ?꾨━?? 猷⑦듃??Button, ?섏쐞??Image 而댄룷?뚰듃媛 ?덉뼱???⑸땲??")]
    [SerializeField] private GameObject  characterSlotPrefab;

    [Header("Skill Icons (3 ?щ’)")]
    [Tooltip("?ㅽ궗 ?꾩씠肄섏쓣 ?쒖떆??Image 3媛?(0=?щ’1, 1=?щ’2, 2=?щ’3)")]
    [SerializeField] private Image[]  skillIconImages  = new Image[3];
    [Tooltip("?꾩씠肄??곗튂 媛먯???Button 3媛? ?숈씪 ?쒖꽌濡??곌껐?섏꽭??")]
    [SerializeField] private Button[] skillIconButtons = new Button[3];

    [Header("Tooltip")]
    [Tooltip("?댄똻 猷⑦듃 ?ㅻ툕?앺듃. 鍮꾪솢???곹깭濡??쒖옉?⑸땲??")]
    [SerializeField] private GameObject tooltipRoot;
    [SerializeField] private TMP_Text   tooltipTitleText;
    [SerializeField] private TMP_Text   tooltipDescText;

    [Header("Close")]
    [SerializeField] private Button closeButton;

    [Header("Events (Optional)")]
    [SerializeField] private VoidEventChannelSO onRepresentativeAgentChanged;

    // ?? ?대? ?곹깭 ?????????????????????????????????????????????

    private int                _selectedIndex;
    private readonly List<Button> _slotButtons = new();
    private readonly SkillRow[]   _skillRows   = new SkillRow[3];
    private int                _tooltipOpenSlot = -1;

    // ?????????????????????????????????????????????????????????

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

    // ?? Public API ????????????????????????????????????????????

    /// <summary>?⑤꼸???쎈땲??</summary>
    public void Open() => gameObject.SetActive(true);

    /// <summary>?⑤꼸???レ뒿?덈떎.</summary>
    public void Close() => gameObject.SetActive(false);

    /// <summary>?덈꺼쨌寃쏀뿕移섎? ?몃??먯꽌 媛깆떊?섍퀬 ??ν빀?덈떎.</summary>
    public void SetAccountStats(int level, int exp, int expMax)
    {
        if (playerData != null)
            playerData.SetAccountStats(level, exp, expMax);

        ApplyLevelExp(level, exp, expMax);
    }

    /// <summary>?됰꽕?꾩쓣 ?몃??먯꽌 蹂寃쏀븯怨???ν빀?덈떎.</summary>
    public void SetNickname(string nickname)
    {
        if (playerData != null)
            playerData.SetNicknameValue(nickname);

        if (nicknameText != null) nicknameText.text = nickname;
    }

    // ?? ?꾨줈??媛깆떊 ???????????????????????????????????????????

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

    // ?? 罹먮┃??由ъ뒪???????????????????????????????????????????

    private void BuildCharacterList()
    {
        if (characterListContent == null || characterSlotPrefab == null || characters == null) return;

        // 湲곗〈 ?щ’ ?쒓굅
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

            // 珥덉긽???곸슜
            if (img != null)
            {
                Sprite portrait = entry.portrait != null ? entry.portrait : entry.agentData?.portrait;
                if (portrait != null)
                    img.sprite = portrait;
            }

            // ?먯씠?꾪듃 ?대쫫 (TMP_Text媛 ?덉쑝硫??쒖떆)
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

        Debug.Log($"[MyInfoPanel] ???罹먮┃??蹂寃????몃뜳??{_selectedIndex}" +
                  $" ({(characters[_selectedIndex].agentData != null ? characters[_selectedIndex].agentData.displayName : "?")})");
    }

    /// <summary>?꾩옱 ?좏깮???щ’留?鍮꾪솢?깊솕(?좏깮 媛뺤“)?섍퀬 ?섎㉧吏???쒖꽦?뷀빀?덈떎.</summary>
    private void UpdateSlotHighlights()
    {
        for (int i = 0; i < _slotButtons.Count; i++)
        {
            if (_slotButtons[i] == null) continue;
            _slotButtons[i].interactable = (i != _selectedIndex);
        }
    }

    // ?? ?ㅽ궗 ?꾩씠肄????????????????????????????????????????????

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

            // ?꾩씠肄??뚰뙆: ?ㅽ궗 誘몄꽕???щ’? 諛섑닾紐?泥섎━
            if (slot < skillIconImages.Length && skillIconImages[slot] != null)
            {
                Color c = skillIconImages[slot].color;
                c.a = row != null ? 1f : 0.3f;
                skillIconImages[slot].color = c;
            }
        }
    }

    // ?? ?댄똻 ?????????????????????????????????????????????????

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
            // 媛숈? ?щ’ ?ы꽣移????댄똻 ?リ린
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




