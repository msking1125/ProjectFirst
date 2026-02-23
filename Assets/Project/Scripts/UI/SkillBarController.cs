using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillBarController : MonoBehaviour
{
    [SerializeField] private Button slotBtn1;
    [SerializeField] private Button slotBtn2;
    [SerializeField] private Button slotBtn3;
    [SerializeField] private TMP_Text slotTxt1;
    [SerializeField] private TMP_Text slotTxt2;
    [SerializeField] private TMP_Text slotTxt3;

    private readonly SkillRow[] equipped = new SkillRow[3];
    private SkillSystem skillSystem;

    private void Update()
    {
        RefreshUI();
    }

    public void Setup(SkillSystem system)
    {
        skillSystem = system;
        RefreshUI();
    }

    public bool EquipToFirstEmptySlot(SkillRow skill)
    {
        if (skill == null)
        {
            return false;
        }

        for (int i = 0; i < equipped.Length; i++)
        {
            if (equipped[i] != null)
            {
                continue;
            }

            equipped[i] = skill;
            RefreshUI();
            return true;
        }

        equipped[0] = skill;
        RefreshUI();
        return true;
    }

    public IReadOnlyList<SkillRow> GetEquippedSkills()
    {
        return equipped;
    }

    public void CastSlot1() => CastSlot(0);

    public void CastSlot2() => CastSlot(1);

    public void CastSlot3() => CastSlot(2);

    private void CastSlot(int index)
    {
        if (index < 0 || index >= equipped.Length)
        {
            return;
        }

        SkillRow skill = equipped[index];
        if (skill == null)
        {
            return;
        }

        if (skillSystem == null)
        {
            Debug.LogWarning($"SkillBarController: SkillSystem missing for slot {index + 1}");
            return;
        }

        bool casted = skillSystem.Cast(skill);
        Debug.Log($"SkillBarController: Cast slot {index + 1} ({skill.name}) success={casted}");
        RefreshUI();
    }

    private void RefreshUI()
    {
        RefreshSlot(0, slotBtn1, slotTxt1);
        RefreshSlot(1, slotBtn2, slotTxt2);
        RefreshSlot(2, slotBtn3, slotTxt3);
    }

    private void RefreshSlot(int index, Button button, TMP_Text text)
    {
        SkillRow skill = equipped[index];
        bool hasSkill = skill != null;

        if (button != null)
        {
            button.interactable = hasSkill && (skillSystem == null || skillSystem.IsSkillReady(skill));
        }

        if (text == null)
        {
            return;
        }

        if (!hasSkill)
        {
            text.text = "Empty";
            return;
        }

        if (skillSystem == null)
        {
            text.text = skill.name;
            return;
        }

        float remaining = skillSystem.GetRemainingCooldown(skill);
        text.text = remaining > 0f ? $"{skill.name} ({remaining:F1}s)" : skill.name;
    }
}
