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

    [Header("슬롯 레이아웃 (Inspector에서 수정 가능)")]
    [Tooltip("아이콘 패딩 (버튼 가장자리에서 아이콘까지 거리)")]
    [SerializeField] private float iconPadding = 2f;
    [Tooltip("하단 이름 텍스트 영역 높이 비율 (0~1). 0.36 = 하단 36%")]
    [Range(0.2f, 0.6f)]
    [SerializeField] private float nameAreaRatio = 0.36f;
    [Tooltip("이름 텍스트 최대 폰트 크기")]
    [SerializeField] private float nameMaxFontSize = 13f;
    [Tooltip("쿨타임 텍스트 폰트 크기")]
    [SerializeField] private float cooldownFontSize = 28f;

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
            cdTxt.gameObject.SetActive(onCD);
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
            cdTxt.gameObject.SetActive(false);
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

        // 이름으로 자식 탐색 (Inspector 또는 이전에 생성된 경우)
        Transform namedChild = btn.transform.Find(childName);
        if (namedChild != null)
        {
            Image img = namedChild.GetComponent<Image>();
            if (img != null) return img;
        }

        // SkillSlot은 100×100 정사각형
        // → 아이콘이 버튼 전체를 채우고, Name 텍스트는 하단 오버레이로 표시
        GameObject go = new GameObject(childName, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(btn.transform, false);
        go.transform.SetAsFirstSibling(); // 텍스트보다 아래 레이어

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(iconPadding, iconPadding);
        rt.offsetMax = new Vector2(-iconPadding, -iconPadding);

        Image newImg = go.GetComponent<Image>();
        newImg.raycastTarget = false;
        newImg.preserveAspect = true;
        newImg.enabled = false;

        // Name 텍스트를 하단 오버레이로 재배치
        TMP_Text nameText = btn.GetComponentInChildren<TMP_Text>();
        if (nameText != null)
        {
            // 텍스트 하단 반투명 배경
            GameObject bg = new GameObject("NameBG", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(btn.transform, false);

            RectTransform bgRt = bg.GetComponent<RectTransform>();
            bgRt.anchorMin = new Vector2(0f, 0f);
            bgRt.anchorMax = new Vector2(1f, nameAreaRatio);
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;

            Image bgImg = bg.GetComponent<Image>();
            bgImg.color = new Color(0f, 0f, 0f, 0.6f);
            bgImg.raycastTarget = false;

            nameText.transform.SetAsLastSibling();

            RectTransform txtRt = nameText.GetComponent<RectTransform>();
            if (txtRt != null)
            {
                txtRt.anchorMin = new Vector2(0f, 0f);
                txtRt.anchorMax = new Vector2(1f, nameAreaRatio);
                txtRt.offsetMin = new Vector2(2f, 1f);
                txtRt.offsetMax = new Vector2(-2f, 0f);
            }
            nameText.fontSize           = Mathf.Min(nameText.fontSize, nameMaxFontSize);
            nameText.alignment          = TMPro.TextAlignmentOptions.Center;
            nameText.enableWordWrapping = true;
        }

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

        Transform found = btn.transform.Find("CooldownText");
        if (found != null) return found.GetComponent<TMP_Text>();

        GameObject go = new GameObject("CooldownText", typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(btn.transform, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.alignment     = TextAlignmentOptions.Center;
        tmp.fontSize      = cooldownFontSize;
        tmp.fontStyle     = FontStyles.Bold;
        tmp.color         = Color.white;
        tmp.raycastTarget = false;

        // 반투명 어두운 배경은 별도 자식 오브젝트로 생성
        // (TextMeshProUGUI와 Image는 같은 GameObject에 공존 불가)
        GameObject bgGo = new GameObject("CooldownBG", typeof(RectTransform), typeof(Image));
        bgGo.transform.SetParent(go.transform, false);
        bgGo.transform.SetAsFirstSibling();

        RectTransform bgRt = bgGo.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;

        Image bg = bgGo.GetComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.55f);
        bg.raycastTarget = false;

        go.SetActive(false);
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
