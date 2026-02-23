using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillSelectPanelController : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Button optionBtn1;
    [SerializeField] private Button optionBtn2;
    [SerializeField] private Button optionBtn3;
    [SerializeField] private TMP_Text optionTxt1;
    [SerializeField] private TMP_Text optionTxt2;
    [SerializeField] private TMP_Text optionTxt3;

    private readonly List<SkillRow> currentOptions = new List<SkillRow>(3);
    private Action<SkillRow> onPicked;

    private void Awake()
    {
        if (panelRoot == null)
        {
            panelRoot = gameObject;
        }

        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
    }

    public void ShowOptions(IReadOnlyList<SkillRow> options, Action<SkillRow> onPick)
    {
        currentOptions.Clear();
        onPicked = onPick;

        for (int i = 0; i < 3; i++)
        {
            SkillRow row = options != null && i < options.Count ? options[i] : null;
            currentOptions.Add(row);
            SetOptionUI(i, row);
        }

        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }
    }

    public void ShowDummyOptionsForTest()
    {
        ShowOptions(new[]
        {
            new SkillRow { id = "dummy_a", name = "Skill A" },
            new SkillRow { id = "dummy_b", name = "Skill B" },
            new SkillRow { id = "dummy_c", name = "Skill C" }
        }, null);
    }

    public void Pick0() => HandlePick(0);

    public void Pick1() => HandlePick(1);

    public void Pick2() => HandlePick(2);

    private void HandlePick(int index)
    {
        if (index < 0 || index >= currentOptions.Count)
        {
            return;
        }

        SkillRow picked = currentOptions[index];
        if (picked == null)
        {
            return;
        }

        Debug.Log($"SkillSelectPanelController: Picked {picked.id} ({picked.name})");

        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }

        onPicked?.Invoke(picked);
    }

    private void SetOptionUI(int index, SkillRow skill)
    {
        Button button = GetButton(index);
        TMP_Text text = GetText(index);
        bool hasSkill = skill != null;

        if (button != null)
        {
            button.interactable = hasSkill;
            button.gameObject.SetActive(hasSkill);
        }

        if (text != null)
        {
            text.text = hasSkill ? skill.name : string.Empty;
        }
    }

    private Button GetButton(int index)
    {
        return index switch
        {
            0 => optionBtn1,
            1 => optionBtn2,
            2 => optionBtn3,
            _ => null
        };
    }

    private TMP_Text GetText(int index)
    {
        return index switch
        {
            0 => optionTxt1,
            1 => optionTxt2,
            2 => optionTxt3,
            _ => null
        };
    }
}
