using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ProjectFirst.Data;

namespace Project
{
    /// <summary>
    /// 캐릭터 고유 액티브 스킬 버튼 컨트롤러입니다.
    /// 버튼, 아이콘, 쿨타임 게이지와 텍스트를 함께 관리합니다.
    /// </summary>
    public class CharUltimateController : MonoBehaviour
    {
        [Header("UI 연결 (비우면 자동 탐색)")]
        [SerializeField] private Button ultButton;
        [SerializeField] private Image skillIcon;
        [SerializeField] private Image cooldownGauge;
        [SerializeField] private TMP_Text cooldownText;
        [SerializeField] private Image charIcon;

        [Header("불가 상태 표시")]
        [SerializeField] private Color readyColor = Color.white;
        [SerializeField] private Color cooldownColor = new Color(0f, 0f, 0f, 0.6f);

        private SkillRow boundSkill;
        private float cooldownDuration;
        private float cooldownEndTime;

        public bool IsReady => Time.unscaledTime >= cooldownEndTime;
        public float Remaining => Mathf.Max(0f, cooldownEndTime - Time.unscaledTime);

        /// <summary>
        /// 버튼을 눌러 궁극기 사용을 요청할 때 발생합니다.
        /// </summary>
        public event System.Action<SkillRow> OnUltimateRequested;

        private void Awake()
        {
            AutoBind();

            if (ultButton != null)
                ultButton.onClick.AddListener(OnButtonClicked);

            SetInteractable(false);
        }

        private void Update()
        {
            if (boundSkill == null)
                return;

            UpdateCooldownUI();
        }

        /// <summary>
        /// AgentData와 SkillTable을 바인딩하고 버튼 상태를 초기화합니다.
        /// </summary>
        public void Setup(AgentData agentData, SkillTable skillTable)
        {
            if (agentData == null)
            {
                Debug.LogWarning("[CharUltimate] AgentData가 null입니다. Agent Inspector에서 AgentData를 연결하세요.", this);
                SetInteractable(false);
                return;
            }

            if (charIcon != null && agentData.characterSkillIcon != null)
            {
                charIcon.sprite = agentData.characterSkillIcon;
                charIcon.enabled = true;
            }

            if (agentData.characterSkillId <= 0 || skillTable == null)
            {
                Debug.LogWarning($"[CharUltimate] characterSkillId가 0 이하이거나 SkillTable이 없습니다. ({agentData.name})", this);
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

            if (skillIcon != null)
            {
                Sprite icon = boundSkill.icon != null ? boundSkill.icon : agentData.characterSkillIcon;
                skillIcon.sprite = icon;
                skillIcon.color = Color.white;
                skillIcon.enabled = icon != null;
            }

            if (cooldownGauge != null)
            {
                cooldownGauge.type = Image.Type.Simple;
                cooldownGauge.color = Color.clear;
            }

            cooldownEndTime = 0f;
            SetInteractable(true);
            UpdateCooldownUI();

            Debug.Log($"[CharUltimate] 설정 완료: {agentData.displayName} -> '{boundSkill.name}' (쿨타임 {cooldownDuration}s)");
        }

        /// <summary>
        /// 궁극기 사용 후 쿨타임을 시작합니다.
        /// </summary>
        public void StartCooldown()
        {
            if (cooldownDuration <= 0f)
                return;

            cooldownEndTime = Time.unscaledTime + cooldownDuration;
            UpdateCooldownUI();
        }

        private void OnButtonClicked()
        {
            if (boundSkill == null || !IsReady)
                return;

            OnUltimateRequested?.Invoke(boundSkill);
        }

        private void UpdateCooldownUI()
        {
            bool ready = IsReady;
            float remaining = Remaining;

            if (cooldownGauge != null)
            {
                cooldownGauge.type = Image.Type.Simple;
                cooldownGauge.color = ready ? Color.clear : cooldownColor;
            }

            if (cooldownText != null)
            {
                if (ready)
                {
                    cooldownText.text = string.Empty;
                    cooldownText.enabled = false;
                }
                else
                {
                    cooldownText.text = remaining >= 10f
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

        private void AutoBind()
        {
            if (ultButton == null)
            {
                foreach (Button button in GetComponentsInChildren<Button>(true))
                {
                    ultButton = button;
                    break;
                }
            }

            skillIcon ??= FindImage("SkillIcon", "Skill_Icon", "Icon");
            cooldownGauge ??= FindImage("CoolTimeDim", "CooldownGauge", "CooldownFill", "GaugeFill");
            cooldownText ??= FindText("CoolTime", "CooldownText", "Cooldown");
            charIcon ??= FindImage("CharIcon", "CharacterIcon", "Portrait");

            if (cooldownGauge == null)
                cooldownGauge = GetComponent<Image>();

            if (cooldownGauge != null)
                cooldownGauge.type = Image.Type.Simple;
        }

        private Image FindImage(params string[] names)
        {
            foreach (string name in names)
            {
                Transform target = FindDeep(name);
                if (target != null)
                {
                    Image image = target.GetComponent<Image>();
                    if (image != null)
                        return image;
                }
            }

            return null;
        }

        private TMP_Text FindText(params string[] names)
        {
            foreach (string name in names)
            {
                Transform target = FindDeep(name);
                if (target != null)
                {
                    TMP_Text text = target.GetComponent<TMP_Text>();
                    if (text != null)
                        return text;
                }
            }

            return null;
        }

        private Transform FindDeep(string targetName)
        {
            foreach (Transform child in GetComponentsInChildren<Transform>(true))
            {
                if (string.Equals(child.name, targetName, System.StringComparison.OrdinalIgnoreCase))
                    return child;
            }

            return null;
        }
    }
}
