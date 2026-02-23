using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BattleGameManager : MonoBehaviour
{
    public static BattleGameManager Instance { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoCreate()
    {
        if (Instance != null)
        {
            return;
        }

        GameObject managerObject = new GameObject("BattleGameManager");
        managerObject.AddComponent<BattleGameManager>();
    }

    [Header("Base")]
    [SerializeField] private BaseHealth baseHealth;
    [SerializeField] private string baseObjectName = "Ark_Base";

    [Header("Data")]
    [SerializeField] private MonsterTable monsterTable;
    [SerializeField] private SkillTable skillTable;
    [SerializeField] private Agent playerAgent;

    [Header("HUD")]
    [SerializeField] private Canvas targetCanvas;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private SkillBarController skillBarController;
    [SerializeField] private SkillSelectPanelController skillSelectPanelController;
    [SerializeField] private BattleHUD battleHudPrefab;
    [SerializeField] private BattleHUD battleHudInstance;
    [SerializeField] private SkillBarController skillBarPrefab;
    [SerializeField] private SkillSelectPanelController skillSelectPanelPrefab;
    

    [Header("Result UI")]
    [SerializeField] private GameObject resultPanel;

    [Header("Scene")]
    [SerializeField] private string titleSceneName = "Title";

    private TMP_Text resultText;
    private bool gameEnded;

    private RunSession runSession;
    private SkillSystem skillSystem;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Time.timeScale = 1f;

        InitializeRunSession();
        EnsureBaseHealth();
        EnsureHUD();
        EnsureResultUI();
        SetResultUI(false, string.Empty);
        RefreshStatusUI();
    }

    private void OnEnable()
    {
        Enemy.EnemyKilled += HandleEnemyKilled;
    }

    private void OnDisable()
    {
        Enemy.EnemyKilled -= HandleEnemyKilled;
    }

    private void OnDestroy()
    {
        if (runSession != null)
        {
            runSession.OnReachedSkillPickLevel -= HandleReachedSkillPickLevel;
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void HandleVictory() => EndGame("Victory");

    public void HandleDefeat() => EndGame("Defeat");

    public static void EndVictoryFallback()
    {
        BattleGameManager manager = Instance;
        if (manager == null)
        {
            GameObject managerObject = new GameObject("BattleGameManager");
            manager = managerObject.AddComponent<BattleGameManager>();
        }

        manager.HandleVictory();
    }

    public static void ReportBaseDestroyed()
    {
        if (Instance == null)
        {
            GameObject managerObject = new GameObject("BattleGameManager");
            managerObject.AddComponent<BattleGameManager>();
        }

        if (Instance != null)
        {
            Instance.HandleDefeat();
        }
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.name);
    }

    public void BackToTitle()
    {
        Time.timeScale = 1f;
        if (Application.CanStreamedLevelBeLoaded(titleSceneName))
        {
            SceneManager.LoadScene(titleSceneName);
            return;
        }

        SceneManager.LoadScene("Battle_Test");
    }

    private void InitializeRunSession()
    {
        if (runSession == null)
        {
            runSession = new RunSession();
        }

        runSession.Reset();
        if (playerAgent == null)
        {
            playerAgent = FindFirstObjectByType<Agent>();
        }

        skillSystem = new SkillSystem(skillTable, playerAgent);
        runSession.OnReachedSkillPickLevel += HandleReachedSkillPickLevel;
    }

    private void HandleEnemyKilled(string monsterId, MonsterGrade grade)
    {
        if (gameEnded || runSession == null || monsterTable == null)
        {
            return;
        }

        MonsterRow row = monsterTable.GetByIdAndGrade(monsterId, grade) ?? monsterTable.GetById(monsterId);
        if (row == null)
        {
            return;
        }

        runSession.AddGold(row.goldReward);
        runSession.AddExp(row.expReward);
        RefreshStatusUI();
    }

    private void HandleReachedSkillPickLevel(int level)
    {
        OpenSkillSelectPanel();
    }

    private void OpenSkillSelectPanel()
    {
        if (skillSystem == null || skillSelectPanelController == null)
        {
            return;
        }

        List<SkillRow> candidates = skillSystem.GetRandomCandidates(3);
        if (candidates.Count <= 0)
        {
            return;
        }

        skillSelectPanelController.ShowOptions(candidates, selectedSkill =>
        {
            int equippedSlot = skillSystem.EquipToFirstEmpty(selectedSkill);
            if (equippedSlot >= 0 && skillBarController != null)
            {
                skillBarController.SetSlot(equippedSlot, selectedSkill);
            }
        });
    }

    private void EndGame(string message)
    {
        if (gameEnded)
        {
            return;
        }

        gameEnded = true;
        Time.timeScale = 0f;
        SetResultUI(true, message);
    }

    private void EnsureBaseHealth()
    {
        if (baseHealth != null)
        {
            baseHealth.BindGameManager(this);
            return;
        }

        GameObject baseObject = GameObject.Find(baseObjectName) ?? GameObject.Find("Base");
        if (baseObject == null)
        {
            return;
        }

        baseHealth = baseObject.GetComponent<BaseHealth>() ?? baseObject.AddComponent<BaseHealth>();
        baseHealth.BindGameManager(this);
    }

    private void EnsureHUD()
    {
        // 1) BattleHUD 프리팹/인스턴스를 우선 사용
        if (battleHudInstance == null)
        {
            battleHudInstance = FindFirstObjectByType<BattleHUD>();
        }

        if (battleHudInstance == null && battleHudPrefab != null)
        {
            battleHudInstance = Instantiate(battleHudPrefab);
        }

        if (battleHudInstance != null)
        {
            targetCanvas = battleHudInstance.Canvas;
            statusText = battleHudInstance.StatusText;
            skillBarController = battleHudInstance.SkillBarController;
            skillSelectPanelController = battleHudInstance.SkillSelectPanelController;
            resultPanel = battleHudInstance.ResultPanel;
            resultText = battleHudInstance.ResultText;

            if (skillBarController != null)
            {
                skillBarController.Setup(skillSystem);
            }

            return;
        }

        // 2) BattleHUD가 없으면 이전과 같은 방식으로 동적 생성 (레거시 경로)
        if (targetCanvas == null)
        {
            targetCanvas = FindFirstObjectByType<Canvas>();
        }

        if (targetCanvas == null)
        {
            GameObject canvasObject = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            targetCanvas = canvasObject.GetComponent<Canvas>();
            targetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        if (statusText == null)
        {
            statusText = CreateText("RunStatusText", targetCanvas.transform, 36f, new Vector2(-430f, 260f), 28f);
            statusText.alignment = TextAlignmentOptions.Left;
        }

        if (skillBarController == null)
        {
            CreateDefaultSkillBar();
        }

        if (skillBarController != null)
        {
            skillBarController.Setup(skillSystem);
        }

        if (skillSelectPanelController == null)
        {
            CreateDefaultSkillSelectPanel();
        }
    }

    private void CreateDefaultSkillBar()
    {
        GameObject barObject = new GameObject("SkillBar", typeof(RectTransform), typeof(SkillBarController));
        barObject.transform.SetParent(targetCanvas.transform, false);

        RectTransform barRect = barObject.GetComponent<RectTransform>();
        barRect.anchorMin = new Vector2(1f, 0f);
        barRect.anchorMax = new Vector2(1f, 0f);
        barRect.pivot = new Vector2(1f, 0f);
        barRect.anchoredPosition = new Vector2(-20f, 20f);
        barRect.sizeDelta = new Vector2(740f, 90f);

        Button[] slotButtons = new Button[3];
        TMP_Text[] slotLabels = new TMP_Text[3];
        for (int i = 0; i < 3; i++)
        {
            GameObject slotObject = new GameObject($"SkillSlot_{i}", typeof(RectTransform), typeof(Image), typeof(Button));
            slotObject.transform.SetParent(barObject.transform, false);

            RectTransform slotRect = slotObject.GetComponent<RectTransform>();
            slotRect.anchorMin = new Vector2(0f, 0f);
            slotRect.anchorMax = new Vector2(0f, 0f);
            slotRect.pivot = new Vector2(0f, 0f);
            slotRect.anchoredPosition = new Vector2(i * 240f, 0f);
            slotRect.sizeDelta = new Vector2(220f, 68f);

            slotButtons[i] = slotObject.GetComponent<Button>();
            slotLabels[i] = CreateText($"SkillSlotLabel_{i}", slotObject.transform, 40f, Vector2.zero, 24f);
            slotLabels[i].alignment = TextAlignmentOptions.Center;
        }

        skillBarController = barObject.GetComponent<SkillBarController>();
        skillBarController.Configure(slotButtons[0], slotButtons[1], slotButtons[2], slotLabels[0], slotLabels[1], slotLabels[2]);
    }

    private void CreateDefaultSkillSelectPanel()
    {
        GameObject panelObject = new GameObject("SkillSelectPanel", typeof(RectTransform), typeof(Image), typeof(SkillSelectPanelController));
        panelObject.transform.SetParent(targetCanvas.transform, false);
        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(640f, 240f);

        Image panelImage = panelObject.GetComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.86f);

        Button[] optionButtons = new Button[3];
        TMP_Text[] optionLabels = new TMP_Text[3];
        for (int i = 0; i < 3; i++)
        {
            GameObject option = new GameObject($"Option_{i}", typeof(RectTransform), typeof(Image), typeof(Button));
            option.transform.SetParent(panelObject.transform, false);

            RectTransform rect = option.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(180f, 120f);
            rect.anchoredPosition = new Vector2(-210f + (210f * i), 0f);

            optionButtons[i] = option.GetComponent<Button>();
            optionLabels[i] = CreateText($"OptionLabel_{i}", option.transform, 24f, Vector2.zero, 24f);
            optionLabels[i].alignment = TextAlignmentOptions.Center;
        }

        skillSelectPanelController = panelObject.GetComponent<SkillSelectPanelController>();
        skillSelectPanelController.Configure(panelObject, optionButtons[0], optionButtons[1], optionButtons[2], optionLabels[0], optionLabels[1], optionLabels[2]);
    }

    private void EnsureResultUI()
    {
        // BattleHUD가 있다면 그 안의 ResultPanel/ResultText를 사용
        if (battleHudInstance != null)
        {
            if (resultPanel == null)
            {
                resultPanel = battleHudInstance.ResultPanel;
            }

            if (resultText == null)
            {
                resultText = battleHudInstance.ResultText;
            }

            return;
        }

        // BattleHUD가 없으면 이전 방식대로 동적 생성
        if (resultPanel == null)
        {
            resultPanel = new GameObject("ResultPanel", typeof(RectTransform), typeof(Image));
            resultPanel.transform.SetParent(targetCanvas.transform, false);

            RectTransform panelRect = resultPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(560f, 320f);

            Image panelImage = resultPanel.GetComponent<Image>();
            panelImage.color = new Color(0f, 0f, 0f, 0.85f);

            resultText = CreateText("ResultText", resultPanel.transform, 64f, new Vector2(0f, 96f), 56f);
            CreateButton("RestartButton", "Restart", new Vector2(0f, -12f), Restart);
            CreateButton("BackButton", "Back To Title", new Vector2(0f, -102f), BackToTitle);
        }
        else if (resultText == null)
        {
            resultText = resultPanel.GetComponentInChildren<TMP_Text>(true);
        }
    }

    private TMP_Text CreateText(string objectName, Transform parent, float width, Vector2 pos, float fontSize)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.sizeDelta = new Vector2(width * 6f, 90f);
        textRect.anchoredPosition = pos;

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = fontSize;
        text.color = Color.white;
        return text;
    }

    private void CreateButton(string objectName, string label, Vector2 pos, UnityEngine.Events.UnityAction onClick)
    {
        GameObject buttonObject = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(resultPanel.transform, false);

        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(300f, 64f);
        buttonRect.anchoredPosition = pos;

        buttonObject.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 1f);
        Button button = buttonObject.GetComponent<Button>();
        button.onClick.AddListener(onClick);

        TMP_Text buttonText = CreateText($"{objectName}_Label", buttonObject.transform, 48f, Vector2.zero, 34f);
        buttonText.text = label;
    }

    private void RefreshStatusUI()
    {
        if (statusText == null || runSession == null)
        {
            return;
        }

        statusText.text = $"Lv {runSession.Level}  EXP {runSession.Exp}/{runSession.ExpToNextLevel}\nGold {runSession.Gold}";
    }

    private void SetResultUI(bool active, string message)
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(active);
        }

        if (resultText != null)
        {
            resultText.text = message;
        }
    }
}
