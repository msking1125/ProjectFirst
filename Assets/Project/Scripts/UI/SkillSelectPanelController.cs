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

        BindButtons();

        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
    }

    public void Configure(GameObject root, Button button1, Button button2, Button button3, TMP_Text text1, TMP_Text text2, TMP_Text text3)
    {
        panelRoot = root;
        optionBtn1 = button1;
        optionBtn2 = button2;
        optionBtn3 = button3;
        optionTxt1 = text1;
        optionTxt2 = text2;
        optionTxt3 = text3;

        BindButtons();

        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
    }

    public void ShowOptions(List<SkillRow> candidates, Action<SkillRow> onSelected)
    {
        currentOptions.Clear();
        onPicked = onSelected;

        if (candidates != null)
        {
            for (int i = 0; i < candidates.Count && i < 3; i++)
            {
                if (candidates[i] != null)
                {
                    currentOptions.Add(candidates[i]);
                }
            }
        }

        BindOption(0, optionBtn1, optionTxt1);
        BindOption(1, optionBtn2, optionTxt2);
        BindOption(2, optionBtn3, optionTxt3);

        if (panelRoot != null)
        {
            panelRoot.SetActive(currentOptions.Count > 0);
        }

        if (currentOptions.Count > 0)
        {
            Time.timeScale = 0f;
        }
    }

    public void Pick0() => HandlePick(0);
    public void Pick1() => HandlePick(1);
    public void Pick2() => HandlePick(2);

    private void BindOption(int index, Button button, TMP_Text text)
    {
        bool hasOption = index < currentOptions.Count;

        if (button != null)
        {
            button.gameObject.SetActive(hasOption);
            button.interactable = hasOption;
        }

        if (text != null)
        {
            text.text = hasOption ? currentOptions[index].name : string.Empty;
        }
    }

    private void HandlePick(int index)
    {
        if (index < 0 || index >= currentOptions.Count)
        {
            return;
        }

        SkillRow selected = currentOptions[index];
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }

        Time.timeScale = 1f;
        onPicked?.Invoke(selected);

        currentOptions.Clear();
        onPicked = null;
    }

    private void BindButtons()
    {
        if (optionBtn1 != null)
        {
            optionBtn1.onClick.RemoveListener(Pick0);
            optionBtn1.onClick.AddListener(Pick0);
        }

        if (optionBtn2 != null)
        {
            optionBtn2.onClick.RemoveListener(Pick1);
            optionBtn2.onClick.AddListener(Pick1);
        }

        if (optionBtn3 != null)
        {
            optionBtn3.onClick.RemoveListener(Pick2);
            optionBtn3.onClick.AddListener(Pick2);
        }
    }
}
