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

    private readonly SkillRow[] equippedSkills = new SkillRow[3];
    private SkillSystem skillSystem;

    private void Awake()
    {
        BindButtons();

        Refresh();
    }

    public void Configure(Button button1, Button button2, Button button3, TMP_Text text1, TMP_Text text2, TMP_Text text3)
    {
        slotBtn1 = button1;
        slotBtn2 = button2;
        slotBtn3 = button3;
        slotTxt1 = text1;
        slotTxt2 = text2;
        slotTxt3 = text3;

        BindButtons();
        Refresh();
    }

    private void Update()
    {
        Refresh();
    }

    public void Setup(SkillSystem system)
    {
        skillSystem = system;
        Refresh();
    }

    public bool EquipToNextEmpty(SkillRow skill)
    {
        if (skill == null)
        {
            return false;
        }

        for (int i = 0; i < equippedSkills.Length; i++)
        {
            if (equippedSkills[i] == null)
            {
                equippedSkills[i] = skill;
                Refresh();
                return true;
            }
        }

        equippedSkills[equippedSkills.Length - 1] = skill;
        Refresh();
        return true;
    }

    public void Pick0() => CastSlot(0);
    public void Pick1() => CastSlot(1);
    public void Pick2() => CastSlot(2);

    private void CastSlot(int index)
    {
        if (index < 0 || index >= equippedSkills.Length || skillSystem == null)
        {
            return;
        }

        SkillRow selected = equippedSkills[index];
        if (selected == null)
        {
            return;
        }

        skillSystem.Cast(selected);
        Refresh();
    }

    private void Refresh()
    {
        Bind(0, slotBtn1, slotTxt1);
        Bind(1, slotBtn2, slotTxt2);
        Bind(2, slotBtn3, slotTxt3);
    }

    private void Bind(int index, Button button, TMP_Text text)
    {
        SkillRow skill = equippedSkills[index];
        bool available = skill != null && skillSystem != null;

        if (button != null)
        {
            button.interactable = available && skillSystem.IsSkillReady(skill);
        }

        if (text != null)
        {
            if (!available)
            {
                text.text = $"Skill {index + 1}";
                return;
            }

            float remain = skillSystem.GetRemainingCooldown(skill);
            text.text = remain <= 0f ? skill.name : $"{skill.name} ({remain:F1}s)";
        }
    }

    private void BindButtons()
    {
        if (slotBtn1 != null)
        {
            slotBtn1.onClick.RemoveListener(Pick0);
            slotBtn1.onClick.AddListener(Pick0);
        }

        if (slotBtn2 != null)
        {
            slotBtn2.onClick.RemoveListener(Pick1);
            slotBtn2.onClick.AddListener(Pick1);
        }

        if (slotBtn3 != null)
        {
            slotBtn3.onClick.RemoveListener(Pick2);
            slotBtn3.onClick.AddListener(Pick2);
        }
    }
}
