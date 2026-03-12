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
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Project
{

/// <summary>
/// 罹먮┃??怨좎쑀 ?≫떚釉??ㅽ궗 踰꾪듉 而⑦듃濡ㅻ윭.
///
/// ?? Hierarchy 援ъ“ ????????????????????????????????????????????????????????
/// SkillChar  [CharUltimateController 遺李?
///   ?쒋?? CharActive_1 (Button)     ??Ult Button
///   ??    ?쒋?? SkillIcon  (Image)  ???ㅽ궗 ?꾩씠肄?
///   ??    ?쒋?? CoolTimeDim(Image, Filled) ??荑⑦???寃뚯씠吏 ?ㅻ쾭?덉씠
///   ??    ?붴?? CoolTime   (TMP_Text) ???⑥? ?쒓컙 ?띿뒪??
///   ?붴?? CharIcon (Image)          ??罹먮┃??珥덉긽??
///
/// ?? Inspector 媛?대뱶 ?????????????????????????????????????????????????????
/// 鍮꾩썙?먮㈃ ?먯떇 ?ㅻ툕?앺듃 ?대쫫?쇰줈 ?먮룞 ?먯깋?⑸땲??
/// </summary>
#if ODIN_INSPECTOR
    [HideMonoScript]
#endif
    public class CharUltimateController : MonoBehaviour
    {
        // ?? Inspector ?곌껐 ????????????????????????????????????????????????????????
#if ODIN_INSPECTOR
        [Title("UI ?곌껐 (鍮꾩슦硫??먮룞 ?먯깋)", TitleAlignment = TitleAlignments.Left)]
        [HorizontalGroup("UI", 0.33f)]
        [BoxGroup("UI/踰꾪듉")]
        [LabelText("Ult 踰꾪듉")]
        [Tooltip("CharActive_1")]
        [SceneObjectsOnly]
#endif
        [Header("UI ?곌껐 (鍮꾩슦硫??먮룞 ?먯깋)")]
        [SerializeField] private Button   ultButton;       // CharActive_1

#if ODIN_INSPECTOR
        [HorizontalGroup("UI", 0.33f)]
        [BoxGroup("UI/?꾩씠肄?)]
        [LabelText("?ㅽ궗 ?꾩씠肄?)]
        [Tooltip("SkillIcon")]
        [PreviewField(50, ObjectFieldAlignment.Left)]
#endif
        [SerializeField] private Image    skillIcon;       // SkillIcon

#if ODIN_INSPECTOR
        [HorizontalGroup("UI", 0.34f)]
        [BoxGroup("UI/寃뚯씠吏")]
        [LabelText("荑⑦???寃뚯씠吏")]
        [Tooltip("CoolTimeDim (Image.Type.Filled)")]
        [PreviewField(50, ObjectFieldAlignment.Left)]
#endif
        [SerializeField] private Image    cooldownGauge;   // CoolTimeDim

#if ODIN_INSPECTOR
        [HorizontalGroup("UI2", 0.5f)]
        [BoxGroup("UI2/?띿뒪??)]
        [LabelText("荑⑦????띿뒪??)]
        [Tooltip("CoolTime")]
#endif
        [SerializeField] private TMP_Text cooldownText;    // CoolTime

#if ODIN_INSPECTOR
        [HorizontalGroup("UI2", 0.5f)]
        [BoxGroup("UI2/罹먮┃??)]
        [LabelText("罹먮┃???꾩씠肄?)]
        [Tooltip("CharIcon")]
        [PreviewField(50, ObjectFieldAlignment.Left)]
#endif
        [SerializeField] private Image    charIcon;        // CharIcon

#if ODIN_INSPECTOR
        [Title("遺덇? ?곹깭 ?쒖떆", TitleAlignment = TitleAlignments.Left)]
        [HorizontalGroup("?곹깭", 0.5f)]
        [BoxGroup("?곹깭/以鍮?)]
        [LabelText("以鍮??됱긽")]
        [Tooltip("荑⑦????꾨즺 ??踰꾪듉 ?됱긽")]
#endif
        [Header("遺덇? ?곹깭 ?쒖떆")]
        [SerializeField] private Color readyColor    = Color.white;

#if ODIN_INSPECTOR
        [HorizontalGroup("?곹깭", 0.5f)]
        [BoxGroup("?곹깭/荑⑦???)]
        [LabelText("荑⑦????됱긽")]
        [GUIColor(0.2f, 0.2f, 0.2f)]
        [Tooltip("荑⑦???以?踰꾪듉 ?됱긽")]
#endif
        [SerializeField] private Color cooldownColor = new Color(0f, 0f, 0f, 0.6f);

    // ?? ?고??????????????????????????????????????????????????????????????????
    private SkillRow boundSkill;
    private float    cooldownDuration;
    private float    cooldownEndTime;  // Time.unscaledTime 湲곗?

    public bool  IsReady   => Time.unscaledTime >= cooldownEndTime;
    public float Remaining => Mathf.Max(0f, cooldownEndTime - Time.unscaledTime);

    /// <summary>踰꾪듉 ????BattleGameManager媛 援щ룆?⑸땲??</summary>
    public event System.Action<SkillRow> OnUltimateRequested;

    // ????????????????????????????????????????????????????????????????????????

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

    // ?? ?몃? API ??????????????????????????????????????????????????????????????

    /// <summary>
    /// BattleGameManager 珥덇린?????몄텧.
    /// AgentData?먯꽌 ?ㅽ궗???먯깋?섍퀬 ?꾩씠肄??깆쓣 ?ㅼ젙?⑸땲??
    /// </summary>
    public void Setup(AgentData agentData, SkillTable skillTable)
    {
        if (agentData == null)
        {
            Debug.LogWarning("[CharUltimate] AgentData媛 null?낅땲?? Agent Inspector?먯꽌 AgentData瑜??곌껐?섏꽭??", this);
            SetInteractable(false);
            return;
        }

        // 罹먮┃??珥덉긽??
        if (charIcon != null && agentData.characterSkillIcon != null)
        {
            charIcon.sprite  = agentData.characterSkillIcon;
            charIcon.enabled = true;
        }

        // ?ㅽ궗 ?먯깋
        if (agentData.characterSkillId <= 0 || skillTable == null)
        {
            Debug.LogWarning($"[CharUltimate] characterSkillId媛 0?댄븯?닿굅??SkillTable???놁뒿?덈떎. ({agentData.name})", this);
            SetInteractable(false);
            return;
        }

        boundSkill = skillTable.GetById(agentData.characterSkillId);
        if (boundSkill == null)
        {
            Debug.LogWarning($"[CharUltimate] SkillTable?먯꽌 '{agentData.characterSkillId}'瑜?李얠? 紐삵뻽?듬땲??", this);
            SetInteractable(false);
            return;
        }

        cooldownDuration = boundSkill.cooldown;

        // ?ㅽ궗 ?꾩씠肄?(SkillRow.icon ?곗꽑, ?놁쑝硫?AgentData.characterSkillIcon)
        if (skillIcon != null)
        {
            Sprite icon = boundSkill.icon != null ? boundSkill.icon : agentData.characterSkillIcon;
            skillIcon.sprite  = icon;
            skillIcon.color   = Color.white;
            skillIcon.enabled = (icon != null);
        }

        // 荑⑦??????ㅻ쾭?덉씠 珥덇린??(Simple ???- ?꾩씠肄??꾩껜瑜?洹좎씪?섍쾶 ??쓬)
        if (cooldownGauge != null)
        {
            cooldownGauge.type  = Image.Type.Simple;
            cooldownGauge.color = Color.clear;
        }

        cooldownEndTime = 0f;
        SetInteractable(true);
        UpdateCooldownUI();

        Debug.Log($"[CharUltimate] ?ㅼ젙 ?꾨즺: {agentData.displayName} ??'{boundSkill.name}' (荑⑦???{cooldownDuration}s)");
    }

    /// <summary>
    /// ?ㅽ궗 諛쒕룞 ??BattleGameManager媛 ?몄텧?섏뿬 荑⑦??꾩쓣 ?쒖옉?⑸땲??
    /// </summary>
    public void StartCooldown()
    {
        if (cooldownDuration <= 0f) return;
        cooldownEndTime = Time.unscaledTime + cooldownDuration;
        UpdateCooldownUI();
    }

    // ?? ?대? 泥섎━ ?????????????????????????????????????????????????????????????

    private void OnButtonClicked()
    {
        if (boundSkill == null || !IsReady) return;
        OnUltimateRequested?.Invoke(boundSkill);
    }

    private void UpdateCooldownUI()
    {
        bool  ready     = IsReady;
        float remaining = Remaining;

        // ???ㅻ쾭?덉씠: 荑⑦???以??꾩씠肄??꾩껜瑜?諛섑닾紐?寃?뺤쑝濡???쓬
        if (cooldownGauge != null)
        {
            cooldownGauge.type  = Image.Type.Simple;
            cooldownGauge.color = ready ? Color.clear : cooldownColor;
        }

        // 荑⑦????띿뒪??
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

    // ?? ?먮룞 ?먯깋 ?????????????????????????????????????????????????????????????

    private void AutoBind()
    {
        // Button: ?먯떇 以?泥?踰덉㎏ Button (CharActive_1 ??
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

        // Simple ??낆쑝濡??ㅼ젙 (?꾩씠肄??꾩껜瑜?洹좎씪?섍쾶 ??쓬)
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




