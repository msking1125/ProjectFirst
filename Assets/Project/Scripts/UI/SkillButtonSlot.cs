using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillButtonSlot : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text label;

    private SkillRow equippedSkill;
    private SkillSystem skillSystem;
    private int slotIndex = -1;

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

    public void Setup(SkillSystem system, int index)
    {
        skillSystem = system;
        slotIndex = index;
        Refresh();
    }

    public void Equip(SkillRow skill)
    {
        equippedSkill = skill;
        Refresh();
    }

    private void OnClick()
    {
        if (skillSystem == null || slotIndex < 0)
        {
            return;
        }

        skillSystem.Cast(slotIndex);
    }

    private void Refresh()
    {
        if (button != null)
        {
            button.interactable = equippedSkill != null && skillSystem != null;
        }

        if (label != null)
        {
            label.text = equippedSkill != null ? equippedSkill.name : "Skill";
        }
    }
}
