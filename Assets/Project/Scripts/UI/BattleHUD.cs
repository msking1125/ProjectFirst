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
        // 媛?ν븳 ???먮룞?쇰줈 李몄“瑜?蹂댁셿?섎릺, SerializeField ?곌껐??理쒖슦?좎엯?덈떎.
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

