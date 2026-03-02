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

    [Header("쿨타임 딤 오버레이 (없으면 자동 생성)")]
    [SerializeField] private Image dimOverlay1;
    [SerializeField] private Image dimOverlay2;
    [SerializeField] private Image dimOverlay3;

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
        AutoBind();
        EnsureIconComponents();
        EnsureCooldownComponents();
        EnsureDimOverlays();
        EnsureNameTextLayout();
        BindButtons();
        Refresh();
    }

    /// <summary>
    /// Inspector 연결이 비어 있을 때 자식 오브젝트 이름으로 자동 탐색합니다.
    /// SkillSlot_1 / SkillSlot_2 / SkillSlot_3 → Button, Name(TMP_Text), Icon(Image), CooldownText(TMP_Text)
    /// </summary>
    private void AutoBind()
    {
        // 자식 이름으로 슬롯 탐색
        string[] slotNames = { "SkillSlot_1", "SkillSlot_2", "SkillSlot_3" };

        for (int i = 0; i < slotNames.Length; i++)
        {
            Transform slot = transform.Find(slotNames[i]);
            if (slot == null) continue;

            // Button
            if (GetSlotButton(i) == null)
            {
                Button btn = slot.GetComponent<Button>();
                switch (i) { case 0: slotBtn1 = btn; break; case 1: slotBtn2 = btn; break; case 2: slotBtn3 = btn; break; }
            }

            // TMP_Text (Name 자식)
            if (GetSlotText(i) == null)
            {
                TMP_Text txt = slot.Find("Name")?.GetComponent<TMP_Text>()
                            ?? slot.GetComponentInChildren<TMP_Text>(true);
                switch (i) { case 0: slotTxt1 = txt; break; case 1: slotTxt2 = txt; break; case 2: slotTxt3 = txt; break; }
            }

            // Icon Image (Icon 자식) - 배경으로 사용하므로 sibling 0으로 내림
            if (GetSlotIcon(i) == null)
            {
                Transform iconTr = slot.Find("Icon");
                Image icon = iconTr?.GetComponent<Image>();
                if (icon != null)
                {
                    // Name 텍스트가 아이콘 위에 렌더링되도록 Icon을 첫 번째 자식(배경)으로 이동
                    iconTr.SetSiblingIndex(0);
                    icon.raycastTarget = false;
                }
                switch (i) { case 0: slotIcon1 = icon; break; case 1: slotIcon2 = icon; break; case 2: slotIcon3 = icon; break; }
            }

            // CooldownText
            if (GetCooldownText(i) == null)
            {
                TMP_Text cd = slot.Find("CooldownText")?.GetComponent<TMP_Text>()
                           ?? slot.Find("CoolTime")?.GetComponent<TMP_Text>();
                switch (i) { case 0: cooldownTxt1 = cd; break; case 1: cooldownTxt2 = cd; break; case 2: cooldownTxt3 = cd; break; }
            }
        }
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
        EnsureDimOverlays();
        EnsureNameTextLayout();
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

        // 버튼 상호작용 차단
        if (btn != null)
            btn.interactable = !onCD;

        // 딤 오버레이로 어둡게 처리
        Image dim = GetDimOverlay(index);
        if (dim != null)
            dim.gameObject.SetActive(onCD);

        // 쿨타임 숫자 텍스트
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
        if (existing != null)
        {
            // 기존 아이콘을 배경으로 내려 Name 텍스트가 위에 렌더링되도록 보장
            existing.transform.SetSiblingIndex(0);
            existing.raycastTarget = false;
            return existing;
        }
        if (btn == null) return null;

        GameObject go = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(btn.transform, false);
        go.transform.SetSiblingIndex(0); // 배경으로 배치

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

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

    private Button   GetSlotButton(int i)    => i switch { 0 => slotBtn1,      1 => slotBtn2,      2 => slotBtn3,      _ => null };
    private TMP_Text GetSlotText(int i)      => i switch { 0 => slotTxt1,      1 => slotTxt2,      2 => slotTxt3,      _ => null };
    private Image    GetSlotIcon(int i)      => i switch { 0 => slotIcon1,     1 => slotIcon2,     2 => slotIcon3,     _ => null };
    private TMP_Text GetCooldownText(int i)  => i switch { 0 => cooldownTxt1,  1 => cooldownTxt2,  2 => cooldownTxt3,  _ => null };
    private Image    GetDimOverlay(int i)    => i switch { 0 => dimOverlay1,   1 => dimOverlay2,   2 => dimOverlay3,   _ => null };

    // ── 딤 오버레이 생성 ────────────────────────────────────────────────────────

    private void EnsureDimOverlays()
    {
        dimOverlay1 = EnsureDim(slotBtn1, dimOverlay1);
        dimOverlay2 = EnsureDim(slotBtn2, dimOverlay2);
        dimOverlay3 = EnsureDim(slotBtn3, dimOverlay3);
    }

    private Image EnsureDim(Button btn, Image existing)
    {
        if (existing != null) return existing;
        if (btn == null) return null;

        // "CooldownDim" 이름 자식 탐색
        Transform found = btn.transform.Find("CooldownDim");
        if (found != null) return found.GetComponent<Image>();

        // 새로 생성: 버튼 전체를 덮는 반투명 검정 오버레이
        GameObject go = new GameObject("CooldownDim", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(btn.transform, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        Image img = go.GetComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0.6f); // 60% 불투명 검정
        img.raycastTarget = false;

        go.SetActive(false); // 평소에는 숨김

        // CooldownText, Icon 위에 렌더링되도록 최상위 sibling으로
        go.transform.SetAsLastSibling();

        return img;
    }

    // ── Name 텍스트 하단 배치 ────────────────────────────────────────────────────

    /// <summary>
    /// Name 텍스트를 슬롯 하단에 배치합니다. (anchorMin.y=0, anchorMax.y=0.35)
    /// 아이콘은 슬롯 전체를 채우고 이름은 하단 35% 영역에 반투명 배경과 함께 표시됩니다.
    /// </summary>
    private void EnsureNameTextLayout()
    {
        ApplyBottomLayout(slotBtn1, slotTxt1);
        ApplyBottomLayout(slotBtn2, slotTxt2);
        ApplyBottomLayout(slotBtn3, slotTxt3);
    }

    private static void ApplyBottomLayout(Button btn, TMP_Text txt)
    {
        if (btn == null || txt == null) return;

        RectTransform rt = txt.GetComponent<RectTransform>();
        if (rt == null) return;

        // 하단 35% 영역에 배치
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(1f, 0.38f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.pivot     = new Vector2(0.5f, 0f);

        txt.alignment       = TextAlignmentOptions.Center;
        txt.fontSize        = 22f;
        txt.enableWordWrapping = true;
        txt.color           = Color.white;

        // 텍스트 가독성을 위한 반투명 배경 추가 (없을 때만)
        Transform bgTr = btn.transform.Find("NameBg");
        if (bgTr == null)
        {
            GameObject bg = new GameObject("NameBg", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(btn.transform, false);

            RectTransform bgRt = bg.GetComponent<RectTransform>();
            bgRt.anchorMin = new Vector2(0f, 0f);
            bgRt.anchorMax = new Vector2(1f, 0.38f);
            bgRt.offsetMin = bgRt.offsetMax = Vector2.zero;
            bgRt.pivot     = new Vector2(0.5f, 0f);

            Image bgImg = bg.GetComponent<Image>();
            bgImg.color = new Color(0f, 0f, 0f, 0.55f);
            bgImg.raycastTarget = false;

            // txt 바로 아래(배경으로)
            bg.transform.SetSiblingIndex(txt.transform.GetSiblingIndex());
        }

        // txt는 NameBg 위에
        txt.transform.SetAsLastSibling();
    }
}