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
    [SerializeField] private Image dimImage;
    [SerializeField] private CanvasGroup dimCanvasGroup;

    private readonly List<SkillRow> currentOptions = new List<SkillRow>(3);
    private Action<SkillRow> onPicked;
    private bool hasLoggedMissingBindings;

    private void Awake()
    {
        AutoBind();
        WarnIfMissingBindings();

        BindButtons();

        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }

        SetDimRaycast(false);
    }

    private void AutoBind()
    {
        panelRoot ??= gameObject;

        Transform optionBtn1Transform = transform.Find("Skill_Layer/Options/OptionButton_1");
        Transform optionBtn2Transform = transform.Find("Skill_Layer/Options/OptionButton_2");
        Transform optionBtn3Transform = transform.Find("Skill_Layer/Options/OptionButton_3");

        if (optionBtn1 == null && optionBtn1Transform != null)
        {
            optionBtn1 = optionBtn1Transform.GetComponent<Button>();
        }
        if (optionBtn2 == null && optionBtn2Transform != null)
        {
            optionBtn2 = optionBtn2Transform.GetComponent<Button>();
        }
        if (optionBtn3 == null && optionBtn3Transform != null)
        {
            optionBtn3 = optionBtn3Transform.GetComponent<Button>();
        }

        if (optionTxt1 == null && optionBtn1 != null)
        {
            optionTxt1 = optionBtn1.transform.Find("Name")?.GetComponent<TMP_Text>();
        }
        if (optionTxt2 == null && optionBtn2 != null)
        {
            optionTxt2 = optionBtn2.transform.Find("Name")?.GetComponent<TMP_Text>();
        }
        if (optionTxt3 == null && optionBtn3 != null)
        {
            optionTxt3 = optionBtn3.transform.Find("Name")?.GetComponent<TMP_Text>();
        }

        if (dimImage == null)
        {
            dimImage = transform.Find("Dim")?.GetComponent<Image>();
        }

        if (dimCanvasGroup == null && dimImage != null)
        {
            dimCanvasGroup = dimImage.GetComponent<CanvasGroup>();
        }
    }

    private void WarnIfMissingBindings()
    {
        if (hasLoggedMissingBindings)
        {
            return;
        }

        List<string> missingFields = new List<string>();
        CheckField(nameof(panelRoot), panelRoot, missingFields);
        CheckField(nameof(optionBtn1), optionBtn1, missingFields);
        CheckField(nameof(optionBtn2), optionBtn2, missingFields);
        CheckField(nameof(optionBtn3), optionBtn3, missingFields);
        CheckField(nameof(optionTxt1), optionTxt1, missingFields);
        CheckField(nameof(optionTxt2), optionTxt2, missingFields);
        CheckField(nameof(optionTxt3), optionTxt3, missingFields);
        CheckField(nameof(dimImage), dimImage, missingFields);

        if (missingFields.Count == 0)
        {
            return;
        }

        hasLoggedMissingBindings = true;
        Debug.LogWarning($"[SkillSelectPanelController] Missing serialized references after AutoBind: {string.Join(", ", missingFields)}", this);
    }

    private static void CheckField(string fieldName, UnityEngine.Object obj, List<string> missingFields)
    {
        if (obj == null)
        {
            missingFields.Add(fieldName);
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
        AutoBind();
        WarnIfMissingBindings();

        BindButtons();

        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }

        SetDimRaycast(false);
    }

    public void ShowOptions(List<SkillRow> candidates, Action<SkillRow> onSelected)
    {
        AutoBind();
        BindButtons();

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

        SetDimRaycast(currentOptions.Count > 0);

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

        SetDimRaycast(false);

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

    [ContextMenu("Show Dummy Options For Test")]
    public void ShowDummyOptionsForTest()
    {
        AutoBind();
        WarnIfMissingBindings();

        if (panelRoot != null)
        {
            panelRoot.gameObject.SetActive(true);
        }

        SetDimRaycast(true);

        currentOptions.Clear();
        onPicked = null;

        SetDummyOption(optionBtn1, optionTxt1, "Skill 1", "Option1 clicked");
        SetDummyOption(optionBtn2, optionTxt2, "Skill 2", "Option2 clicked");
        SetDummyOption(optionBtn3, optionTxt3, "Skill 3", "Option3 clicked");
    }

    private void SetDummyOption(Button button, TMP_Text text, string label, string logMessage)
    {
        if (button != null)
        {
            button.gameObject.SetActive(true);
            button.interactable = true;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                Debug.Log(logMessage);

                if (panelRoot != null)
                {
                    panelRoot.SetActive(false);
                }

                SetDimRaycast(false);
            });
        }

        if (text != null)
        {
            text.text = label;
        }
    }

    private void SetDimRaycast(bool isEnabled)
    {
        if (dimImage != null)
        {
            dimImage.raycastTarget = isEnabled;
        }

        if (dimCanvasGroup != null)
        {
            dimCanvasGroup.blocksRaycasts = isEnabled;
            dimCanvasGroup.interactable = isEnabled;
        }
    }
}
