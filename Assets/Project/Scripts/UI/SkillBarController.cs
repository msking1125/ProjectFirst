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

    public void CastSlot1()
    {
        LogCast(1, slotTxt1);
    }

    public void CastSlot2()
    {
        LogCast(2, slotTxt2);
    }

    public void CastSlot3()
    {
        LogCast(3, slotTxt3);
    }

    private void LogCast(int slotIndex, TMP_Text slotText)
    {
        string slotName = slotText != null && !string.IsNullOrWhiteSpace(slotText.text)
            ? slotText.text
            : $"Slot {slotIndex}";

        Debug.Log($"SkillBarController: Cast slot {slotIndex} ({slotName})");
    }
}
