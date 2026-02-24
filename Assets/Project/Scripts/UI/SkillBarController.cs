using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillBarController : MonoBehaviour
{
    [SerializeField] private bool enableSlotsOnStartForTest;
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

        Button slotButton = GetSlotButton(slotIndex);
        EnsureSlotButtonInteractive(slotButton, true);
    }

    public void SetSlot(int slotIndex, string skillName)
    {
        TMP_Text slotText = GetSlotText(slotIndex);
        if (slotText != null)
        {
            slotText.text = string.IsNullOrWhiteSpace(skillName) ? $"EMPTY {slotIndex + 1}" : skillName;
        }

        Button slotButton = GetSlotButton(slotIndex);
        EnsureSlotButtonInteractive(slotButton, !string.IsNullOrWhiteSpace(skillName));
    }

    public void ClearSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slotSkills.Length)
        {
            return;
        }

        slotSkills[slotIndex] = null;

        TMP_Text slotText = GetSlotText(slotIndex);
        if (slotText != null)
        {
            slotText.text = $"EMPTY {slotIndex + 1}";
        }

        Button slotButton = GetSlotButton(slotIndex);
        EnsureSlotButtonInteractive(slotButton, false);
    }

    public void CastSlot1()
    {
        Debug.Log("CastSlot1", this);
        CastSlot(0);
    }

    public void CastSlot2()
    {
        Debug.Log("CastSlot2", this);
        CastSlot(1);
    }

    public void CastSlot3()
    {
        Debug.Log("CastSlot3", this);
        CastSlot(2);
    }

    private void CastSlot(int index)
    {
        if (skillSystem == null)
        {
            return;
        }

        if (index < 0 || index >= slotSkills.Length || slotSkills[index] == null)
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
            bool hasSkillSystem = skillSystem != null;
            button.interactable = enableSlotsOnStartForTest
                ? hasSkillSystem
                : hasSkillSystem && skill != null;
        }

        if (text != null)
        {
            text.text = skill != null ? skill.name : $"EMPTY {index + 1}";
        }
    }


    private Button GetSlotButton(int slotIndex)
    {
        return slotIndex switch
        {
            0 => slotBtn1,
            1 => slotBtn2,
            2 => slotBtn3,
            _ => null
        };
    }

    private TMP_Text GetSlotText(int slotIndex)
    {
        return slotIndex switch
        {
            0 => slotTxt1,
            1 => slotTxt2,
            2 => slotTxt3,
            _ => null
        };
    }

    private void EnsureSlotButtonInteractive(Button button, bool interactable)
    {
        if (button == null)
        {
            return;
        }

        button.interactable = interactable;

        Image image = button.GetComponent<Image>();
        if (image != null)
        {
            image.raycastTarget = true;
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
