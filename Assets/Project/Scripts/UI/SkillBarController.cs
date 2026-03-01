using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillBarController : MonoBehaviour
{
    [Header("슬롯 버튼")]
    [SerializeField] private Button slotBtn1;
    [SerializeField] private Button slotBtn2;
    [SerializeField] private Button slotBtn3;

    [Header("슬롯 텍스트 (스킬 이름)")]
    [SerializeField] private TMP_Text slotTxt1;
    [SerializeField] private TMP_Text slotTxt2;
    [SerializeField] private TMP_Text slotTxt3;

    [Header("슬롯 아이콘 Image (없으면 자동 생성)")]
    [SerializeField] private Image slotIcon1;
    [SerializeField] private Image slotIcon2;
    [SerializeField] private Image slotIcon3;

    [Header("쿨타임 오버레이 텍스트 (없으면 자동 생성)")]
    [SerializeField] private TMP_Text cooldownTxt1;
    [SerializeField] private TMP_Text cooldownTxt2;
    [SerializeField] private TMP_Text cooldownTxt3;

    [Header("스킬 이펙트")]
    [SerializeField] private GameObject userSkillFlamePrefab;
    [SerializeField] private Transform effectSpawnPoint;
    [SerializeField] private bool useSlotPosition = true;

    [Header("테스트")]
    [SerializeField] private bool enableSlotsOnStartForTest;

    [SerializeField] private Color noIconColor = new Color(0.3f, 0.3f, 0.3f, 0.6f);

    private readonly SkillRow[] slotSkills = new SkillRow[3];
    private SkillSystem skillSystem;

    private void Awake()
    {
        EnsureIconComponents();
        EnsureCooldownComponents();
        BindButtons();
        Refresh();
    }

    private void Update()
    {
        if (skillSystem == null) return;

        UpdateCooldownUI(0, slotBtn1, cooldownTxt1);
        UpdateCooldownUI(1, slotBtn2, cooldownTxt2);
        UpdateCooldownUI(2, slotBtn3, cooldownTxt3);
    }

    // ✅ 🔥 추가된 Configure (에러 해결 핵심)
    public void Configure(Button b1, Button b2, Button b3,
                          TMP_Text t1, TMP_Text t2, TMP_Text t3)
    {
        slotBtn1 = b1;
        slotBtn2 = b2;
        slotBtn3 = b3;

        slotTxt1 = t1;
        slotTxt2 = t2;
        slotTxt3 = t3;

        EnsureIconComponents();
        EnsureCooldownComponents();
        BindButtons();
        Refresh();
    }

    public void Setup(SkillSystem system)
    {
        skillSystem = system;
        Refresh();
    }

    // ===================== 이펙트 =====================

    private void PlaySkillEffect(int index)
    {
        if (userSkillFlamePrefab == null) return;

        Vector3 pos = Vector3.zero;

        if (useSlotPosition)
        {
            Button btn = GetSlotButton(index);
            if (btn != null)
                pos = btn.transform.position;
        }
        else if (effectSpawnPoint != null)
        {
            pos = effectSpawnPoint.position;
        }

        Instantiate(userSkillFlamePrefab, pos, Quaternion.identity);
    }

    // ===================== 스킬 사용 =====================

    public void CastSlot1() => CastSlot(0);
    public void CastSlot2() => CastSlot(1);
    public void CastSlot3() => CastSlot(2);

    private void CastSlot(int index)
    {
        if (skillSystem == null || index < 0 || index >= slotSkills.Length) return;
        if (slotSkills[index] == null) return;

        // 🔥 이펙트 먼저
        PlaySkillEffect(index);

        skillSystem.Cast(index);
    }

    // ===================== 쿨타임 =====================

    private void UpdateCooldownUI(int index, Button btn, TMP_Text cdTxt)
    {
        if (slotSkills[index] == null) return;

        bool onCD = skillSystem.IsOnCooldown(index);
        float remaining = skillSystem.GetRemainingCooldown(index);

        if (btn != null)
            btn.interactable = !onCD;

        if (cdTxt != null)
        {
            cdTxt.gameObject.SetActive(onCD);
            if (onCD)
                cdTxt.text = remaining >= 10f
                    ? $"{Mathf.CeilToInt(remaining)}"
                    : $"{remaining:F1}";
        }
    }

    // ===================== 슬롯 =====================

    public void SetSlot(int index, SkillRow skill)
    {
        if (index < 0 || index >= 3) return;
        slotSkills[index] = skill;
        RefreshSlot(index);
    }

    private void Refresh()
    {
        for (int i = 0; i < 3; i++)
            RefreshSlot(i);
    }

    private void RefreshSlot(int index)
    {
        SkillRow skill = slotSkills[index];

        Button btn = GetSlotButton(index);
        TMP_Text txt = GetSlotText(index);
        Image icon = GetSlotIcon(index);

        if (txt != null)
            txt.text = skill != null ? skill.name : $"EMPTY {index + 1}";

        if (icon != null)
        {
            if (skill?.icon != null)
            {
                icon.sprite = skill.icon;
                icon.color = Color.white;
                icon.enabled = true;
            }
            else
            {
                icon.sprite = null;
                icon.color = noIconColor;
                icon.enabled = skill != null;
            }
        }

        if (btn != null)
            btn.interactable = skillSystem != null && skill != null;
    }

    // ===================== 자동 생성 =====================

    private void EnsureIconComponents()
    {
        slotIcon1 = EnsureIcon(slotBtn1, slotIcon1);
        slotIcon2 = EnsureIcon(slotBtn2, slotIcon2);
        slotIcon3 = EnsureIcon(slotBtn3, slotIcon3);
    }

    private Image EnsureIcon(Button btn, Image existing)
    {
        if (existing != null) return existing;
        if (btn == null) return null;

        GameObject go = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(btn.transform, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;

        Image img = go.GetComponent<Image>();
        img.raycastTarget = false;
        img.enabled = false;

        return img;
    }

    private void EnsureCooldownComponents()
    {
        cooldownTxt1 = EnsureCooldown(slotBtn1, cooldownTxt1);
        cooldownTxt2 = EnsureCooldown(slotBtn2, cooldownTxt2);
        cooldownTxt3 = EnsureCooldown(slotBtn3, cooldownTxt3);
    }

    private TMP_Text EnsureCooldown(Button btn, TMP_Text existing)
    {
        if (existing != null) return existing;
        if (btn == null) return null;

        GameObject go = new GameObject("CooldownText", typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(btn.transform, false);

        TextMeshProUGUI txt = go.GetComponent<TextMeshProUGUI>();
        txt.alignment = TextAlignmentOptions.Center;
        txt.fontSize = 28;
        txt.color = Color.white;

        go.SetActive(false);
        return txt;
    }

    // ===================== 버튼 =====================

    private void BindButtons()
    {
        Bind(slotBtn1, CastSlot1);
        Bind(slotBtn2, CastSlot2);
        Bind(slotBtn3, CastSlot3);
    }

    private void Bind(Button btn, UnityEngine.Events.UnityAction action)
    {
        if (btn == null) return;
        btn.onClick.RemoveListener(action);
        btn.onClick.AddListener(action);
    }

    // ===================== 헬퍼 =====================

    private Button GetSlotButton(int i) => i switch { 0 => slotBtn1, 1 => slotBtn2, 2 => slotBtn3, _ => null };
    private TMP_Text GetSlotText(int i) => i switch { 0 => slotTxt1, 1 => slotTxt2, 2 => slotTxt3, _ => null };
    private Image GetSlotIcon(int i) => i switch { 0 => slotIcon1, 1 => slotIcon2, 2 => slotIcon3, _ => null };
}