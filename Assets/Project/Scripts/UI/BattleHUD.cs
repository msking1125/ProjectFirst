using TMPro;
using UnityEngine;
using Project;

[DisallowMultipleComponent]
public class BattleHUD : MonoBehaviour
{
    [Header("Core")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private TMP_Text statusText;

    [Header("Skill UI")]
    [SerializeField] private SkillBarController skillBarController;
    [SerializeField] private SkillSelectPanelController skillSelectPanelController;

    [Header("Result UI")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TMP_Text resultText;

    public Canvas Canvas => canvas;
    public TMP_Text StatusText => statusText;
    public SkillBarController SkillBarController => skillBarController;
    public SkillSelectPanelController SkillSelectPanelController => skillSelectPanelController;
    public GameObject ResultPanel => resultPanel;
    public TMP_Text ResultText => resultText;

    private void Awake()
    {
        // Note: cleaned comment.
        if (canvas == null)
        {
            canvas = GetComponentInChildren<Canvas>(true);
        }

        if (skillBarController == null)
        {
            skillBarController = GetComponentInChildren<SkillBarController>(true);
        }

        if (skillSelectPanelController == null)
        {
            skillSelectPanelController = GetComponentInChildren<SkillSelectPanelController>(true);
        }
    }
}

