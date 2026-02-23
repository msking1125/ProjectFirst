using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillSelectPanel : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private Button[] optionButtons;
    [SerializeField] private TMP_Text[] optionLabels;

    private Action<SkillRow> onSkillSelected;
    private readonly List<SkillRow> currentCandidates = new List<SkillRow>();

    private void Awake()
    {
        if (root == null)
        {
            root = gameObject;
        }

        if (optionButtons != null)
        {
            for (int i = 0; i < optionButtons.Length; i++)
            {
                int captured = i;
                optionButtons[i].onClick.AddListener(() => Select(captured));
            }
        }

        if (root != null)
        {
            root.SetActive(false);
        }
    }

    public void Configure(GameObject targetRoot, Button[] buttons, TMP_Text[] labels)
    {
        root = targetRoot;
        optionButtons = buttons;
        optionLabels = labels;

        if (optionButtons != null)
        {
            for (int i = 0; i < optionButtons.Length; i++)
            {
                int captured = i;
                optionButtons[i].onClick.RemoveAllListeners();
                optionButtons[i].onClick.AddListener(() => Select(captured));
            }
        }

        if (root != null)
        {
            root.SetActive(false);
        }
    }

    public void Open(List<SkillRow> candidates, Action<SkillRow> onSelected)
    {
        currentCandidates.Clear();
        currentCandidates.AddRange(candidates);
        onSkillSelected = onSelected;

        for (int i = 0; i < optionButtons.Length; i++)
        {
            bool hasOption = i < currentCandidates.Count;
            optionButtons[i].gameObject.SetActive(hasOption);
            if (hasOption)
            {
                optionLabels[i].text = currentCandidates[i].name;
            }
        }

        root.SetActive(true);
        Time.timeScale = 0f;
    }

    private void Select(int index)
    {
        if (index < 0 || index >= currentCandidates.Count)
        {
            return;
        }

        SkillRow selected = currentCandidates[index];
        root.SetActive(false);
        Time.timeScale = 1f;
        onSkillSelected?.Invoke(selected);
    }
}
