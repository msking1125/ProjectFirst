using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Project
{

/// <summary>
/// 캐릭터 고유 액티브 스킬 버튼 컨트롤러.
///
/// ── Hierarchy 구조 ────────────────────────────────────────────────────────
/// SkillChar  [CharUltimateController 부착]
///   ├── CharActive_1 (Button)     ← Ult Button
///   │     ├── SkillIcon  (Image)  ← 스킬 아이콘
///   │     ├── CoolTimeDim(Image, Filled) ← 쿨타임 게이지 오버레이
///   │     └── CoolTime   (TMP_Text) ← 남은 시간 텍스트
///   └── CharIcon (Image)          ← 캐릭터 초상화
///
/// ── Inspector 가이드 ─────────────────────────────────────────────────────
/// 비워두면 자식 오브젝트 이름으로 자동 탐색합니다.
/// </summary>
#if ODIN_INSPECTOR
    [HideMonoScript]
#endif
    public class CharUltimateController : MonoBehaviour
    {
        // ── Inspector 연결 ────────────────────────────────────────────────────────
#if ODIN_INSPECTOR
        [Title("UI 연결 (비우면 자동 탐색)", TitleAlignment = TitleAlignments.Left)]
        [HorizontalGroup("UI", 0.33f)]
        [BoxGroup("UI/버튼")]
        [LabelText("Ult 버튼")]
        [Tooltip("CharActive_1")]
        [SceneObjectsOnly]
#endif
        [Header("UI 연결 (비우면 자동 탐색)")]
        [SerializeField] private Button   ultButton;       // CharActive_1

#if ODIN_INSPECTOR
        [HorizontalGroup("UI", 0.33f)]
        [BoxGroup("UI/아이콘")]
        [LabelText("스킬 아이콘")]
        [Tooltip("SkillIcon")]
        [PreviewField(50, ObjectFieldAlignment.Left)]
#endif
        [SerializeField] private Image    skillIcon;       // SkillIcon

#if ODIN_INSPECTOR
        [HorizontalGroup("UI", 0.34f)]
        [BoxGroup("UI/게이지")]
        [LabelText("쿨타임 게이지")]
        [Tooltip("CoolTimeDim (Image.Type.Filled)")]
        [PreviewField(50, ObjectFieldAlignment.Left)]
#endif
        [SerializeField] private Image    cooldownGauge;   // CoolTimeDim

#if ODIN_INSPECTOR
        [HorizontalGroup("UI2", 0.5f)]
        [BoxGroup("UI2/텍스트")]
        [LabelText("쿨타임 텍스트")]
        [Tooltip("CoolTime")]
#endif
        [SerializeField] private TMP_Text cooldownText;    // CoolTime

#if ODIN_INSPECTOR
        [HorizontalGroup("UI2", 0.5f)]
        [BoxGroup("UI2/캐릭터")]
        [LabelText("캐릭터 아이콘")]
        [Tooltip("CharIcon")]
        [PreviewField(50, ObjectFieldAlignment.Left)]
#endif
        [SerializeField] private Image    charIcon;        // CharIcon

#if ODIN_INSPECTOR
        [Title("불가 상태 표시", TitleAlignment = TitleAlignments.Left)]
        [HorizontalGroup("상태", 0.5f)]
        [BoxGroup("상태/준비")]
        [LabelText("준비 색상")]
        [Tooltip("쿨타임 완료 시 버튼 색상")]
#endif
        [Header("불가 상태 표시")]
        [SerializeField] private Color readyColor    = Color.white;

#if ODIN_INSPECTOR
        [HorizontalGroup("상태", 0.5f)]
        [BoxGroup("상태/쿨타임")]
        [LabelText("쿨타임 색상")]
        [GUIColor(0.2f, 0.2f, 0.2f)]
        [Tooltip("쿨타임 중 버튼 색상")]
#endif
        [SerializeField] private Color cooldownColor = new Color(0f, 0f, 0f, 0.6f);

    // ── 런타임 ───────────────────────────────────────────────────────────────
    private SkillRow boundSkill;
    private float    cooldownDuration;
    private float    cooldownEndTime;  // Time.unscaledTime 기준

    public bool  IsReady   => Time.unscaledTime >= cooldownEndTime;
    public float Remaining => Mathf.Max(0f, cooldownEndTime - Time.unscaledTime);

    /// <summary>버튼 탭 시 BattleGameManager가 구독합니다.</summary>
    public event System.Action<SkillRow> OnUltimateRequested;

    // ────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        AutoBind();

        if (ultButton != null)
            ultButton.onClick.AddListener(OnButtonClicked);

        SetInteractable(false);
    }

    private void Update()
    {
        if (boundSkill == null) return;
        UpdateCooldownUI();
    }

    // ── 외부 API ──────────────────────────────────────────────────────────────

    /// <summary>
    /// BattleGameManager 초기화 시 호출.
    /// AgentData에서 스킬을 탐색하고 아이콘 등을 설정합니다.
    /// </summary>
    public void Setup(AgentData agentData, SkillTable skillTable)
    {
        if (agentData == null)
        {
            Debug.LogWarning("[CharUltimate] AgentData가 null입니다. Agent Inspector에서 AgentData를 연결하세요.", this);
            SetInteractable(false);
            return;
        }

        // 캐릭터 초상화
        if (charIcon != null && agentData.characterSkillIcon != null)
        {
            charIcon.sprite  = agentData.characterSkillIcon;
            charIcon.enabled = true;
        }

        // 스킬 탐색
        if (agentData.characterSkillId <= 0 || skillTable == null)
        {
            Debug.LogWarning($"[CharUltimate] characterSkillId가 0이하이거나 SkillTable이 없습니다. ({agentData.name})", this);
            SetInteractable(false);
            return;
        }

        boundSkill = skillTable.GetById(agentData.characterSkillId);
        if (boundSkill == null)
        {
            Debug.LogWarning($"[CharUltimate] SkillTable에서 '{agentData.characterSkillId}'를 찾지 못했습니다.", this);
            SetInteractable(false);
            return;
        }

        cooldownDuration = boundSkill.cooldown;

        // 스킬 아이콘 (SkillRow.icon 우선, 없으면 AgentData.characterSkillIcon)
        if (skillIcon != null)
        {
            Sprite icon = boundSkill.icon != null ? boundSkill.icon : agentData.characterSkillIcon;
            skillIcon.sprite  = icon;
            skillIcon.color   = Color.white;
            skillIcon.enabled = (icon != null);
        }

        // 쿨타임 딤 오버레이 초기화 (Simple 타입 - 아이콘 전체를 균일하게 덮음)
        if (cooldownGauge != null)
        {
            cooldownGauge.type  = Image.Type.Simple;
            cooldownGauge.color = Color.clear;
        }

        cooldownEndTime = 0f;
        SetInteractable(true);
        UpdateCooldownUI();

        Debug.Log($"[CharUltimate] 설정 완료: {agentData.displayName} → '{boundSkill.name}' (쿨타임 {cooldownDuration}s)");
    }

    /// <summary>
    /// 스킬 발동 후 BattleGameManager가 호출하여 쿨타임을 시작합니다.
    /// </summary>
    public void StartCooldown()
    {
        if (cooldownDuration <= 0f) return;
        cooldownEndTime = Time.unscaledTime + cooldownDuration;
        UpdateCooldownUI();
    }

    // ── 내부 처리 ─────────────────────────────────────────────────────────────

    private void OnButtonClicked()
    {
        if (boundSkill == null || !IsReady) return;
        OnUltimateRequested?.Invoke(boundSkill);
    }

    private void UpdateCooldownUI()
    {
        bool  ready     = IsReady;
        float remaining = Remaining;

        // 딤 오버레이: 쿨타임 중 아이콘 전체를 반투명 검정으로 덮음
        if (cooldownGauge != null)
        {
            cooldownGauge.type  = Image.Type.Simple;
            cooldownGauge.color = ready ? Color.clear : cooldownColor;
        }

        // 쿨타임 텍스트
        if (cooldownText != null)
        {
            if (ready)
            {
                cooldownText.text    = string.Empty;
                cooldownText.enabled = false;
            }
            else
            {
                cooldownText.text    = remaining >= 10f
                    ? $"{Mathf.CeilToInt(remaining)}"
                    : $"{remaining:F1}";
                cooldownText.enabled = true;
            }
        }

        SetInteractable(ready);
    }

    private void SetInteractable(bool value)
    {
        if (ultButton != null)
            ultButton.interactable = value;
    }

    // ── 자동 탐색 ─────────────────────────────────────────────────────────────

    private void AutoBind()
    {
        // Button: 자식 중 첫 번째 Button (CharActive_1 등)
        if (ultButton == null)
        {
            foreach (Button btn in GetComponentsInChildren<Button>(true))
            {
                ultButton = btn;
                break;
            }
        }

        skillIcon     ??= FindImage("SkillIcon",   "Skill_Icon",   "Icon");
        cooldownGauge ??= FindImage("CoolTimeDim", "CooldownGauge","CooldownFill", "GaugeFill");
        cooldownText  ??= FindText ("CoolTime",    "CooldownText", "Cooldown");
        charIcon      ??= FindImage("CharIcon",    "CharacterIcon","Portrait");

        if (cooldownGauge == null)
            cooldownGauge = GetComponent<Image>();

        // Simple 타입으로 설정 (아이콘 전체를 균일하게 덮음)
        if (cooldownGauge != null)
            cooldownGauge.type = Image.Type.Simple;
    }

    private Image FindImage(params string[] names)
    {
        foreach (string n in names)
        {
            Transform t = FindDeep(n);
            if (t != null) { Image c = t.GetComponent<Image>(); if (c != null) return c; }
        }
        return null;
    }

    private TMP_Text FindText(params string[] names)
    {
        foreach (string n in names)
        {
            Transform t = FindDeep(n);
            if (t != null) { TMP_Text c = t.GetComponent<TMP_Text>(); if (c != null) return c; }
        }
        return null;
    }

    private Transform FindDeep(string targetName)
    {
        foreach (Transform t in GetComponentsInChildren<Transform>(true))
        {
            if (string.Equals(t.name, targetName, System.StringComparison.OrdinalIgnoreCase))
                return t;
        }
        return null;
    }
}
} // namespace Project
