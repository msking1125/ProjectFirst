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

namespace Project
{

/// <summary>
/// Documentation cleaned.
///
/// Documentation cleaned.
/// Documentation cleaned.
/// Documentation cleaned.
/// Documentation cleaned.
/// Documentation cleaned.
/// Documentation cleaned.
/// Documentation cleaned.
///
/// Documentation cleaned.
/// Documentation cleaned.
/// </summary>
    public class CharUltimateController : MonoBehaviour
    {
        // Note: cleaned comment.
        [Header("UI References")]
        [SerializeField] private Button   ultButton;       // CharActive_1
        [SerializeField] private Image    skillIcon;       // SkillIcon
        [SerializeField] private Image    cooldownGauge;   // CoolTimeDim
        [SerializeField] private TMP_Text cooldownText;    // CoolTime
        [SerializeField] private Image    charIcon;        // CharIcon
        [Header("Visual State")]
        [SerializeField] private Color readyColor    = Color.white;
        [SerializeField] private Color cooldownColor = new Color(0f, 0f, 0f, 0.6f);

    // Note: cleaned comment.
    private SkillRow boundSkill;
    private float    cooldownDuration;
    private float    cooldownEndTime;  // Time.unscaledTime 湲곗?

    public bool  IsReady   => Time.unscaledTime >= cooldownEndTime;
    public float Remaining => Mathf.Max(0f, cooldownEndTime - Time.unscaledTime);

    /// Documentation cleaned.
    public event System.Action<SkillRow> OnUltimateRequested;

    // Note: cleaned comment.

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

    // Note: cleaned comment.

    /// <summary>
    /// Documentation cleaned.
    /// Documentation cleaned.
    /// </summary>
    public void Setup(AgentData agentData, SkillTable skillTable)
    {
        if (agentData == null)
        {
            Debug.LogWarning("[CharUltimate] AgentData is null. Assign AgentData in the inspector.", this);
            SetInteractable(false);
            return;
        }

        // Note: cleaned comment.
        if (charIcon != null && agentData.characterSkillIcon != null)
        {
            charIcon.sprite  = agentData.characterSkillIcon;
            charIcon.enabled = true;
        }

        // Note: cleaned comment.
        if (agentData.characterSkillId <= 0 || skillTable == null)
        {
            Debug.LogWarning($"[CharUltimate] characterSkillId is invalid or SkillTable is missing. ({agentData.name})", this);
            SetInteractable(false);
            return;
        }

        boundSkill = skillTable.GetById(agentData.characterSkillId);
        if (boundSkill == null)
        {
            Debug.LogWarning($"[CharUltimate] Could not find skill id '{agentData.characterSkillId}' in SkillTable.", this);
            SetInteractable(false);
            return;
        }

        cooldownDuration = boundSkill.cooldown;

        // Note: cleaned comment.
        if (skillIcon != null)
        {
            Sprite icon = boundSkill.icon != null ? boundSkill.icon : agentData.characterSkillIcon;
            skillIcon.sprite  = icon;
            skillIcon.color   = Color.white;
            skillIcon.enabled = (icon != null);
        }

        // Note: cleaned comment.
        if (cooldownGauge != null)
        {
            cooldownGauge.type  = Image.Type.Simple;
            cooldownGauge.color = Color.clear;
        }

        cooldownEndTime = 0f;
        SetInteractable(true);
        UpdateCooldownUI();

        Debug.Log($"[CharUltimate] Setup complete: {agentData.displayName} -> '{boundSkill.name}' (Cooldown {cooldownDuration}s)");
    }

    /// <summary>
    /// Documentation cleaned.
    /// </summary>
    public void StartCooldown()
    {
        if (cooldownDuration <= 0f) return;
        cooldownEndTime = Time.unscaledTime + cooldownDuration;
        UpdateCooldownUI();
    }

    // Note: cleaned comment.

    private void OnButtonClicked()
    {
        if (boundSkill == null || !IsReady) return;
        OnUltimateRequested?.Invoke(boundSkill);
    }

    private void UpdateCooldownUI()
    {
        bool  ready     = IsReady;
        float remaining = Remaining;

        // Note: cleaned comment.
        if (cooldownGauge != null)
        {
            cooldownGauge.type  = Image.Type.Simple;
            cooldownGauge.color = ready ? Color.clear : cooldownColor;
        }

        // Note: cleaned comment.
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

    // Note: cleaned comment.

    private void AutoBind()
    {
        // Note: cleaned comment.
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

        // Note: cleaned comment.
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





