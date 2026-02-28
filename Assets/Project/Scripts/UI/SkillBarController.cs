using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 스킬 바 컨트롤러.
/// - 슬롯당 쿨타임 표시 (남은 초수 오버레이)
/// - 쿨타임 중 버튼 비활성화, 종료 시 재활성화
/// - 슬롯에 스킬 아이콘 이미지 표시
/// </summary>
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

    [Header("테스트")]
    [SerializeField] private bool enableSlotsOnStartForTest;

    // ── 기본 아이콘 색상 (아이콘 없을 때) ────────────────────────────────────
    [SerializeField] private Color noIconColor = new Color(0.3f, 0.3f, 0.3f, 0.6f);

    private readonly SkillRow[] slotSkills = new SkillRow[3];
    private SkillSystem skillSystem;

    // ────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        EnsureIconComponents();
        EnsureCooldownComponents();
        BindButtons();
        Refresh();
    }

    private void Start()
    {
        // 필드 누락 경고
        List<string> missing = null;
        void Check(string n, Object o) { if (o == null) { missing ??= new List<string>(); missing.Add(n); } }
        Check(nameof(slotBtn1), slotBtn1); Check(nameof(slotBtn2), slotBtn2); Check(nameof(slotBtn3), slotBtn3);
        Check(nameof(slotTxt1), slotTxt1); Check(nameof(slotTxt2), slotTxt2); Check(nameof(slotTxt3), slotTxt3);
        if (missing != null)
            Debug.LogWarning($"[SkillBarController] Missing references: {string.Join(", ", missing)}", this);
    }

    private void Update()
    {
        if (skillSystem == null) return;
        UpdateCooldownUI(0, slotBtn1, cooldownTxt1);
        UpdateCooldownUI(1, slotBtn2, cooldownTxt2);
        UpdateCooldownUI(2, slotBtn3, cooldownTxt3);
    }

    // ── 쿨타임 UI 업데이트 ────────────────────────────────────────────────────

    private void UpdateCooldownUI(int index, Button btn, TMP_Text cdTxt)
    {
        if (slotSkills[index] == null) return;

        bool onCD = skillSystem.IsOnCooldown(index);
        float remaining = skillSystem.GetRemainingCooldown(index);

        // 버튼 활성화 상태
        if (btn != null)
            btn.interactable = !onCD;

        // 쿨타임 텍스트 표시
        if (cdTxt != null)
        {
            GameObject toggleGo = cdTxt.transform.parent != null && cdTxt.transform.parent.name == "CooldownOverlay" ? cdTxt.transform.parent.gameObject : cdTxt.gameObject;
            toggleGo.SetActive(onCD);
            if (onCD)
                cdTxt.text = remaining >= 10f
                    ? $"{Mathf.CeilToInt(remaining)}"
                    : $"{remaining:F1}";
        }
    }

    // ── 공개 API ─────────────────────────────────────────────────────────────

    public void Configure(Button b1, Button b2, Button b3, TMP_Text t1, TMP_Text t2, TMP_Text t3)
    {
        slotBtn1 = b1; slotBtn2 = b2; slotBtn3 = b3;
        slotTxt1 = t1; slotTxt2 = t2; slotTxt3 = t3;
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

    public void SetSlot(int slotIndex, SkillRow skill)
    {
        if (slotIndex < 0 || slotIndex >= slotSkills.Length) return;
        slotSkills[slotIndex] = skill;
        RefreshSlot(slotIndex);
        EnsureSlotButtonInteractive(GetSlotButton(slotIndex), skill != null);
    }

    public void ClearSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slotSkills.Length) return;
        slotSkills[slotIndex] = null;
        RefreshSlot(slotIndex);
        EnsureSlotButtonInteractive(GetSlotButton(slotIndex), false);
    }

    // ── 버튼 콜백 ────────────────────────────────────────────────────────────

    public void CastSlot1() { Debug.Log("CastSlot1", this); CastSlot(0); }
    public void CastSlot2() { Debug.Log("CastSlot2", this); CastSlot(1); }
    public void CastSlot3() { Debug.Log("CastSlot3", this); CastSlot(2); }

    private void CastSlot(int index)
    {
        if (skillSystem == null || index < 0 || index >= slotSkills.Length) return;
        if (slotSkills[index] == null) return;
        skillSystem.Cast(index);
    }

    // ── 내부 ─────────────────────────────────────────────────────────────────

    private void Refresh()
    {
        for (int i = 0; i < slotSkills.Length; i++)
            RefreshSlot(i);
    }

    private void RefreshSlot(int index)
    {
        SkillRow skill   = slotSkills[index];
        Button   btn     = GetSlotButton(index);
        TMP_Text txt     = GetSlotText(index);
        Image    icon    = GetSlotIcon(index);
        TMP_Text cdTxt   = GetCooldownText(index);

        // 이름 텍스트
        if (txt != null)
            txt.text = skill != null ? skill.name : $"EMPTY {index + 1}";

        // 아이콘
        if (icon != null)
        {
            if (skill?.icon != null)
            {
                icon.sprite  = skill.icon;
                icon.color   = Color.white;
                icon.enabled = true;
            }
            else
            {
                icon.sprite  = null;
                icon.color   = noIconColor;
                icon.enabled = skill != null; // 스킬 있을 때만 빈 배경
            }
        }

        // 버튼 활성화 (쿨타임은 Update에서 갱신)
        if (btn != null)
        {
            bool hasSystem = skillSystem != null;
            btn.interactable = enableSlotsOnStartForTest
                ? hasSystem
                : hasSystem && skill != null;
        }

        // 쿨타임 텍스트 초기 숨김
        if (cdTxt != null)
        {
            GameObject toggleGo = cdTxt.transform.parent != null && cdTxt.transform.parent.name == "CooldownOverlay" ? cdTxt.transform.parent.gameObject : cdTxt.gameObject;
            toggleGo.SetActive(false);
        }
    }

    // ── 자동 컴포넌트 생성 ────────────────────────────────────────────────────

    /// <summary>아이콘 Image가 없으면 슬롯 버튼 자식에 자동 생성</summary>
    private void EnsureIconComponents()
    {
        slotIcon1 = EnsureIconOnButton(slotBtn1, slotIcon1, "SkillIcon");
        slotIcon2 = EnsureIconOnButton(slotBtn2, slotIcon2, "SkillIcon");
        slotIcon3 = EnsureIconOnButton(slotBtn3, slotIcon3, "SkillIcon");
    }

    private Image EnsureIconOnButton(Button btn, Image existing, string childName)
    {
        if (existing != null) return existing;
        if (btn == null) return null;

        // 1순위: Inspector에서 직접 지정된 경우 이미 반환됨 (existing != null)
        // 2순위: 이름으로 자식 탐색
        Transform namedChild = btn.transform.Find(childName);
        if (namedChild != null)
        {
            Image img = namedChild.GetComponent<Image>();
            if (img != null) return img;
        }

        // 3순위: 버튼 자식 중 Image 컴포넌트가 있는 오브젝트 자동 탐지
        // (슬롯에 이미 아이콘용 Image가 있는 경우 재사용)
        Image[] childImages = btn.GetComponentsInChildren<Image>(true);
        foreach (Image childImg in childImages)
        {
            // 버튼 자신의 Image는 제외
            if (childImg.gameObject == btn.gameObject) continue;
            // TMP_Text가 붙은 오브젝트는 텍스트용이므로 제외
            if (childImg.GetComponent<TMP_Text>() != null) continue;
            if (childImg.GetComponentInParent<TMP_Text>() != null) continue;
            childImg.raycastTarget = false;
            return childImg;
        }

        // 4순위: 없으면 새로 생성
        GameObject go = new GameObject(childName, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(btn.transform, false);
        go.transform.SetAsFirstSibling(); // 텍스트 뒤에 가리지 않도록

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.05f, 0.4f);
        rt.anchorMax = new Vector2(0.45f, 0.95f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image newImg = go.GetComponent<Image>();
        newImg.raycastTarget = false;
        newImg.enabled = false;

        return newImg;
    }

    /// <summary>쿨타임 텍스트가 없으면 슬롯 버튼 자식에 자동 생성</summary>
    private void EnsureCooldownComponents()
    {
        cooldownTxt1 = EnsureCooldownText(slotBtn1, cooldownTxt1);
        cooldownTxt2 = EnsureCooldownText(slotBtn2, cooldownTxt2);
        cooldownTxt3 = EnsureCooldownText(slotBtn3, cooldownTxt3);
    }

    private TMP_Text EnsureCooldownText(Button btn, TMP_Text existing)
    {
        if (existing != null) return existing;
        if (btn == null) return null;

        Transform foundOverlay = btn.transform.Find("CooldownOverlay");
        if (foundOverlay != null) return foundOverlay.GetComponentInChildren<TMP_Text>();

        Transform old = btn.transform.Find("CooldownText");
        if (old != null)
        {
            // 과거 버전 오브젝트를 파괴해서 껍데기를 재사용하지 않게 방지
            Destroy(old.gameObject);
        }

        GameObject overlayGo = new GameObject("CooldownOverlay", typeof(RectTransform), typeof(Image));
        overlayGo.transform.SetParent(btn.transform, false);

        RectTransform overlayRt = overlayGo.GetComponent<RectTransform>();
        overlayRt.anchorMin = Vector2.zero;
        overlayRt.anchorMax = Vector2.one;
        overlayRt.offsetMin = Vector2.zero;
        overlayRt.offsetMax = Vector2.zero;

        Image bg = overlayGo.GetComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.55f);
        bg.raycastTarget = false;

        GameObject textGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textGo.transform.SetParent(overlayGo.transform, false);

        RectTransform textRt = textGo.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = textGo.GetComponent<TextMeshProUGUI>();
        tmp.alignment   = TextAlignmentOptions.Center;
        tmp.fontSize    = 28f;
        tmp.fontStyle   = FontStyles.Bold;
        tmp.color       = Color.white;
        tmp.raycastTarget = false;

        overlayGo.SetActive(false);
        return tmp;
    }

    // ── 헬퍼 ─────────────────────────────────────────────────────────────────

    private Button   GetSlotButton(int i)       => i switch { 0 => slotBtn1,    1 => slotBtn2,    2 => slotBtn3,    _ => null };
    private TMP_Text GetSlotText(int i)         => i switch { 0 => slotTxt1,    1 => slotTxt2,    2 => slotTxt3,    _ => null };
    private Image    GetSlotIcon(int i)         => i switch { 0 => slotIcon1,   1 => slotIcon2,   2 => slotIcon3,   _ => null };
    private TMP_Text GetCooldownText(int i)     => i switch { 0 => cooldownTxt1,1 => cooldownTxt2,2 => cooldownTxt3,_ => null };

    private void EnsureSlotButtonInteractive(Button btn, bool interactable)
    {
        if (btn == null) return;
        btn.interactable = interactable;
        Image img = btn.GetComponent<Image>();
        if (img != null) img.raycastTarget = true;
    }

    private void BindButtons()
    {
        Bind(slotBtn1, CastSlot1);
        Bind(slotBtn2, CastSlot2);
        Bind(slotBtn3, CastSlot3);
    }

    private static void Bind(Button btn, UnityEngine.Events.UnityAction action)
    {
        if (btn == null) return;
        btn.onClick.RemoveListener(action);
        btn.onClick.AddListener(action);
    }
}
