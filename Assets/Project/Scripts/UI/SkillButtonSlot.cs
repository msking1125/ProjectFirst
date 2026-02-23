using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillButtonSlot : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text label;

    private SkillRow equippedSkill;
    private SkillSystem skillSystem;

    private void Awake()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (button != null)
        {
            button.onClick.AddListener(OnClick);
        }

        Refresh();
    }

    private void Update()
    {
        if (equippedSkill != null)
        {
            Refresh();
        }
    }

    public void Configure(Button targetButton, TMP_Text targetLabel)
    {
        button = targetButton;
        label = targetLabel;
        if (button != null)
        {
            button.onClick.RemoveListener(OnClick);
            button.onClick.AddListener(OnClick);
        }

        Refresh();
    }

    public void Setup(SkillSystem system)
    {
        skillSystem = system;
        Refresh();
    }

    public void Equip(SkillRow skill)
    {
        equippedSkill = skill;
        Refresh();
    }

    private void OnClick()
    {
        if (equippedSkill == null || skillSystem == null)
        {
            return;
        }

        skillSystem.Cast(equippedSkill);
        Refresh();
    }

    private void Refresh()
    {
        bool active = equippedSkill != null && skillSystem != null;
        if (button != null)
        {
            button.interactable = active && skillSystem.IsSkillReady(equippedSkill);
        }

        if (label != null)
        {
            if (!active)
            {
                label.text = "Skill";
            }
            else
            {
                float remaining = skillSystem.GetRemainingCooldown(equippedSkill);
                label.text = remaining <= 0f
                    ? equippedSkill.name
                    : $"{equippedSkill.name} ({remaining:F1}s)";
            }
        }
    }
}
