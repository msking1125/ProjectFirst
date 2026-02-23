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

    public void ShowDummyOptionsForTest()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }

        if (optionTxt1 != null)
        {
            optionTxt1.text = "Skill A";
        }

        if (optionTxt2 != null)
        {
            optionTxt2.text = "Skill B";
        }

        if (optionTxt3 != null)
        {
            optionTxt3.text = "Skill C";
        }

        if (optionBtn1 != null)
        {
            optionBtn1.interactable = true;
        }

        if (optionBtn2 != null)
        {
            optionBtn2.interactable = true;
        }

        if (optionBtn3 != null)
        {
            optionBtn3.interactable = true;
        }
    }

    public void Pick0()
    {
        HandlePick(0, optionTxt1);
    }

    public void Pick1()
    {
        HandlePick(1, optionTxt2);
    }

    public void Pick2()
    {
        HandlePick(2, optionTxt3);
    }

    private void HandlePick(int index, TMP_Text optionText)
    {
        string pickedName = optionText != null ? optionText.text : $"Option {index}";
        Debug.Log($"SkillSelectPanelController: Picked index {index} ({pickedName})");

        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
    }
}
