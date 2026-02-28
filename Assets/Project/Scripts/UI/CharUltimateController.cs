using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 캐릭터 고유 액티브 스킬 버튼 컨트롤러.
///
/// ── Inspector 연결 가이드 ─────────────────────────────────────────────────
/// CharUltimate 오브젝트에 부착하고 아래 항목을 연결합니다.
/// 비워두면 자식 오브젝트 이름으로 자동 탐색합니다.
///
/// [CharUltimate]  ← 이 오브젝트에 CharUltimateController 부착
///   ├── UltButton       (Button)    ← 탭 시 스킬 발동
///   ├── SkillIcon       (Image)     ← 스킬 아이콘
///   ├── CooldownGauge   (Image, Filled) ← 쿨타임 게이지 (꽉 차면 사용 가능)
///   ├── CooldownText    (TMP_Text)  ← 남은 쿨타임 숫자 (선택)
///   └── CharIcon        (Image)    ← 캐릭터 초상화 (선택)
/// </summary>
public class CharUltimateController : MonoBehaviour
{
    // ── Inspector 연결 ────────────────────────────────────────────────────────
    [Header("UI 연결 (비우면 자동 탐색)")]
    [SerializeField] private Button    ultButton;
    [SerializeField] private Image     skillIcon;
    [SerializeField] private Image     cooldownGauge;    // Image Type: Filled
    [SerializeField] private TMP_Text  cooldownText;
    [SerializeField] private Image     charIcon;

    [Header("불가 상태 표시")]
    [SerializeField] private Color readyColor    = Color.white;
    [SerializeField] private Color cooldownColor = new Color(0.4f, 0.4f, 0.4f, 0.8f);

    // ── 런타임 ───────────────────────────────────────────────────────────────
    private SkillRow      boundSkill;
    private float         cooldownDuration;
    private float         cooldownEndTime;   // Time.unscaledTime 기준

    public bool IsReady   => Time.unscaledTime >= cooldownEndTime;
    public float Remaining => Mathf.Max(0f, cooldownEndTime - Time.unscaledTime);

    // 외부(BattleGameManager)에서 발동 시 호출할 콜백
    public event System.Action<SkillRow> OnUltimateRequested;

    // ────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        AutoBind();

        if (ultButton != null)
            ultButton.onClick.AddListener(OnButtonClicked);
    }

    private void AutoBind()
    {
        Image   Find(string n) => transform.Find(n)?.GetComponent<Image>();
        Button  FindBtn(string n) => transform.Find(n)?.GetComponent<Button>();
        TMP_Text FindTxt(string n) => transform.Find(n)?.GetComponent<TMP_Text>();

        ultButton     ??= FindBtn("UltButton")  ?? GetComponent<Button>();
        skillIcon     ??= Find("SkillIcon");
        cooldownGauge ??= Find("CooldownGauge") ?? Find("CooldownFill");
        cooldownText  ??= FindTxt("CooldownText");
        charIcon      ??= Find("CharIcon");

        // cooldownGauge가 없으면 이 오브젝트의 Image를 게이지로 사용
        if (cooldownGauge == null)
            cooldownGauge = GetComponent<Image>();

        if (cooldownGauge != null)
        {
            cooldownGauge.type       = Image.Type.Filled;
            cooldownGauge.fillMethod = Image.FillMethod.Radial360;
            cooldownGauge.fillAmount = 1f;
        }
    }

    private void Update()
    {
        if (boundSkill == null) return;
        UpdateCooldownUI();
    }

    // ── 외부 API ─────────────────────────────────────────────────────────────

    /// <summary>
    /// BattleGameManager에서 초기화 시 호출.
    /// AgentData에서 스킬 정보를 연결합니다.
    /// </summary>
    public void Setup(AgentData agentData, SkillTable skillTable)
    {
        if (agentData == null)
        {
            Debug.LogWarning("[CharUltimate] AgentData가 null입니다. Inspector에서 연결하세요.");
            SetInteractable(false);
            return;
        }

        // 캐릭터 초상화 적용
        if (charIcon != null && agentData.characterSkillIcon != null)
            charIcon.sprite = agentData.characterSkillIcon;

        // 스킬 탐색
        if (string.IsNullOrWhiteSpace(agentData.characterSkillId) || skillTable == null)
        {
            Debug.LogWarning($"[CharUltimate] characterSkillId가 비어있거나 SkillTable이 없습니다. AgentData: {agentData.name}");
            SetInteractable(false);
            return;
        }

        boundSkill = skillTable.GetById(agentData.characterSkillId);
        if (boundSkill == null)
        {
            Debug.LogWarning($"[CharUltimate] SkillTable에서 '{agentData.characterSkillId}'를 찾지 못했습니다.");
            SetInteractable(false);
            return;
        }

        cooldownDuration = boundSkill.cooldown;

        // 스킬 아이콘 적용
        if (skillIcon != null)
        {
            skillIcon.sprite  = boundSkill.icon != null ? boundSkill.icon
                              : agentData.characterSkillIcon;
            skillIcon.enabled = skillIcon.sprite != null;
        }

        cooldownEndTime = 0f; // 게임 시작 시 바로 사용 가능
        SetInteractable(true);
        UpdateCooldownUI();

        Debug.Log($"[CharUltimate] 설정 완료: {agentData.displayName} → 스킬 '{boundSkill.name}' (쿨타임 {cooldownDuration}s)");
    }

    /// <summary>
    /// 외부에서 강제로 쿨타임 시작 (BattleGameManager에서 실제 발동 처리 후 호출)
    /// </summary>
    public void StartCooldown()
    {
        if (cooldownDuration <= 0f) return;
        cooldownEndTime = Time.unscaledTime + cooldownDuration;
    }

    // ── 내부 처리 ────────────────────────────────────────────────────────────

    private void OnButtonClicked()
    {
        if (boundSkill == null) return;

        if (!IsReady)
        {
            Debug.Log($"[CharUltimate] 쿨타임 중. 남은 시간: {Remaining:F1}s");
            return;
        }

        OnUltimateRequested?.Invoke(boundSkill);
    }

    private void UpdateCooldownUI()
    {
        bool ready = IsReady;
        float remaining = Remaining;

        // 게이지: 쿨타임 비율 (1 = 준비됨, 0 = 방금 사용)
        if (cooldownGauge != null)
        {
            float fill = cooldownDuration > 0f
                ? 1f - (remaining / cooldownDuration)
                : 1f;
            cooldownGauge.fillAmount = ready ? 1f : fill;
            cooldownGauge.color = ready ? readyColor : cooldownColor;
        }

        // 텍스트: 남은 쿨타임 또는 준비 표시
        if (cooldownText != null)
        {
            cooldownText.text    = ready ? string.Empty : $"{remaining:F1}";
            cooldownText.enabled = !ready;
        }

        // 버튼 상호작용
        SetInteractable(ready);
    }

    private void SetInteractable(bool value)
    {
        if (ultButton != null)
            ultButton.interactable = value;
    }
}
