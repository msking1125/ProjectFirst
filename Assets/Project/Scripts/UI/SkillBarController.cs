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

    private readonly SkillRow[] slotSkills = new SkillRow[3];
    private SkillSystem skillSystem;

    private void Awake()
    {
        BindButtons();
        Refresh();
    }

    private void Start()
    {
        List<string> missingFields = null;
        void CheckField(string fieldName, UnityEngine.Object obj)
        {
            if (obj == null)
            {
                missingFields ??= new List<string>();
                missingFields.Add(fieldName);
            }
        }

        CheckField(nameof(slotBtn1), slotBtn1);
        CheckField(nameof(slotBtn2), slotBtn2);
        CheckField(nameof(slotBtn3), slotBtn3);
        CheckField(nameof(slotTxt1), slotTxt1);
        CheckField(nameof(slotTxt2), slotTxt2);
        CheckField(nameof(slotTxt3), slotTxt3);

        if (missingFields != null)
        {
            Debug.LogWarning($"[SkillBarController] Missing serialized references: {string.Join(", ", missingFields)}", this);
        }
    }

    public void Setup(SkillSystem system)
    {
        skillSystem = system;
        Refresh();
    }

    public void SetSlot(int slotIndex, SkillRow skill)
    {
        if (slotIndex < 0 || slotIndex >= slotSkills.Length)
        {
            return;
        }

        slotSkills[slotIndex] = skill;
        Refresh();
    }

    public void CastSlot1() => CastSlot(0);
    public void CastSlot2() => CastSlot(1);
    public void CastSlot3() => CastSlot(2);

    private void CastSlot(int index)
    {
        if (skillSystem == null)
        {
            return;
        }

        skillSystem.Cast(index);
    }

    private void Refresh()
    {
        Bind(0, slotBtn1, slotTxt1);
        Bind(1, slotBtn2, slotTxt2);
        Bind(2, slotBtn3, slotTxt3);
    }

    private void Bind(int index, Button button, TMP_Text text)
    {
        SkillRow skill = slotSkills[index];

        if (button != null)
        {
            button.interactable = skillSystem != null && skill != null;
        }

        if (text != null)
        {
            text.text = skill != null ? skill.name : $"EMPTY {index + 1}";
        }
    }

    private void BindButtons()
    {
        if (slotBtn1 != null)
        {
            slotBtn1.onClick.RemoveListener(CastSlot1);
            slotBtn1.onClick.AddListener(CastSlot1);
        }

        if (slotBtn2 != null)
        {
            slotBtn2.onClick.RemoveListener(CastSlot2);
            slotBtn2.onClick.AddListener(CastSlot2);
        }

        if (slotBtn3 != null)
        {
            slotBtn3.onClick.RemoveListener(CastSlot3);
            slotBtn3.onClick.AddListener(CastSlot3);
        }
    }
}
