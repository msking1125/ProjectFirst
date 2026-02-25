using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// BattleGameManager: 전투 게임의 주요 상태 및 UI를 관리하는 싱글턴 클래스.
/// </summary>
public class BattleGameManager : MonoBehaviour
{
    public static BattleGameManager Instance { get; private set; }
    private static bool hasLoggedDuplicateManagerWarning;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoCreate()
    {
        if (Instance != null) return;

#if UNITY_2022_2_OR_NEWER
        BattleGameManager existingManager = FindFirstObjectByType<BattleGameManager>();
#else
        BattleGameManager existingManager = GameObject.FindObjectOfType<BattleGameManager>();
#endif

        if (existingManager != null)
        {
            Instance = existingManager;
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
    [SerializeField] private ResultPanelManager resultPanelManager;

    [Header("Scene")]
    [SerializeField] private string titleSceneName = "Title";

    private TMP_Text resultText;
    private bool gameEnded;

    private RunSession runSession;
    private SkillSystem skillSystem;
    private bool hasLoggedZeroRewardWarning;
    private bool isEnemyKilledSubscribed;

    private void Awake()
    {
        int managerCount = FindObjectsOfType<BattleGameManager>().Length;
        if (managerCount > 1 && !hasLoggedDuplicateManagerWarning)
        {
            Debug.LogWarning($"[BGM] Multiple BattleGameManager detected in scene: count={managerCount}", this);
            hasLoggedDuplicateManagerWarning = true;
        }

        // 싱글턴 중복 방지
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
        EnsureResultPanelManager();
        SetResultUI(false, string.Empty);
        RefreshStatusUI();
    }

    private void OnEnable()
    {
        EnsureEnemyKilledSubscription();
    }

    private void Start()
    {
        EnsureEnemyKilledSubscription();
    }

    private void OnDisable()
    {
        if (!isEnemyKilledSubscribed)
        {
            return;
        }

        Enemy.EnemyKilled -= HandleEnemyKilled;
        isEnemyKilledSubscribed = false;
        Debug.Log("[BGM] Unsubscribed EnemyKilled");
    }

    private void OnDestroy()
    {
        if (runSession != null)
        {
            runSession.OnLevelChanged -= HandleLevelChanged;
            runSession.OnReachedSkillPickLevel -= HandleReachedSkillPickLevel;
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void HandleVictory() => EndGame("Victory");

    public void HandleDefeat() => EndGame("Defeat");

    /// <summary>
    /// (예외 상황에서) 인스턴스가 없으면 새로 생성 후 승리 처리 (씬 전환 없이도 작동하기 위해)
    /// </summary>
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

    /// <summary>
    /// 기지가 파괴됐을 때 호출, 인스턴스 없을 시 자동 생성해 패배 처리
    /// </summary>
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

    /// <summary>
    /// 현재 씬을 다시 로드 (재시작)
    /// </summary>
    public void Restart()
    {
        Time.timeScale = 1f;
        var activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.name);
    }

    /// <summary>
    /// 타이틀 씬으로 돌아가기
    /// </summary>
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

    /// <summary>
    /// 런(게임 진행) 세션 초기화 및 SkillSystem 등 연결
    /// </summary>
    private void InitializeRunSession()
    {
        if (runSession == null)
        {
            runSession = new RunSession();
        }

        runSession.Reset();

        // 플레이어 Agent를 할당 (없으면 씬에서 탐색)
        if (playerAgent == null)
        {
#if UNITY_2022_2_OR_NEWER
            playerAgent = FindFirstObjectByType<Agent>();
#else
            playerAgent = GameObject.FindObjectOfType<Agent>();
#endif
        }

        skillSystem = new SkillSystem(skillTable, playerAgent);

        // 이벤트 중복 연결 방지 후 다시 연결
        runSession.OnLevelChanged -= HandleLevelChanged;
        runSession.OnReachedSkillPickLevel -= HandleReachedSkillPickLevel;
        runSession.OnLevelChanged += HandleLevelChanged;
        runSession.OnReachedSkillPickLevel += HandleReachedSkillPickLevel;
    }

    /// <summary>
    /// 적 처치시 골드/경험치 지급, 상태 UI 새로고침
    /// 테스트 가이드:
    /// Play -> 몬스터 1마리 사망 -> [Enemy] Die 로그 확인
    /// 이어서 [BGM] Subscribed -> [BGM] HandleEnemyKilled -> [RunSession] AddExp 로그 순서 확인
    /// </summary>
    private void HandleEnemyKilled(Enemy enemy)
    {
        if (gameEnded || monsterTable == null || enemy == null)
        {
            return;
        }

        if (runSession == null)
        {
            InitializeRunSession();
            if (runSession == null)
            {
                return;
            }
        }

        string monsterId = enemy.MonsterId;
        MonsterGrade grade = enemy.Grade;
        Debug.Log($"[BGM] HandleEnemyKilled start monsterId={monsterId} grade={grade}");
        MonsterRow row = monsterTable.GetByIdAndGrade(monsterId, grade) ?? monsterTable.GetById(monsterId);
        if (row == null)
        {
            Debug.LogWarning($"[BGM] HandleEnemyKilled row miss: monsterId={monsterId} grade={grade}");
            return;
        }

        int expReward = row.expReward;
        int goldReward = row.goldReward;
        Debug.Log($"[BGM] HandleEnemyKilled monsterId={monsterId} grade={grade} expReward={expReward} goldReward={goldReward}");

        if (expReward == 0 && goldReward == 0 && !hasLoggedZeroRewardWarning)
        {
            Debug.LogWarning($"[BattleGameManager] 보상 데이터 0: monsterId={monsterId}, grade={grade}");
            hasLoggedZeroRewardWarning = true;
        }

        Debug.Log($"[BGM] Calling RunSession.AddExp amount={expReward}");
        runSession.AddExp(expReward);
        runSession.AddGold(goldReward);

        Debug.Log($"[BattleGameManager] Rewards applied. after Lv={runSession.Level} Exp={runSession.Exp}/{runSession.ExpToNextLevel} Gold={runSession.Gold}");
        RefreshStatusUI();
    }

    private void EnsureEnemyKilledSubscription()
    {
        if (isEnemyKilledSubscribed)
        {
            return;
        }

        Enemy.EnemyKilled -= HandleEnemyKilled;
        Enemy.EnemyKilled += HandleEnemyKilled;
        isEnemyKilledSubscribed = true;
        Debug.Log("[BGM] Subscribed EnemyKilled");
    }

    /// <summary>
    /// 레벨이 변경되었을 때 호출되어 UI를 갱신.
    /// </summary>
    private void HandleLevelChanged(int level)
    {
        Debug.Log($"[BattleGameManager] Level changed to {level}. Exp={runSession?.Exp}/{runSession?.ExpToNextLevel}", this);
        RefreshStatusUI();
    }

    /// <summary>
    /// 스킬 선택 레벨(예: 3, 6, 9...)에 도달했을 때 스킬 선택 패널 표시.
    /// </summary>
    private void HandleReachedSkillPickLevel(int level)
    {
        Debug.Log($"[BattleGameManager] Reached skill-pick level {level}. Opening skill select panel.", this);
        OpenSkillSelectPanel();
    }

    /// <summary>
    /// 스킬 선택 UI 생성 및 핸들러 연결
    /// </summary>
    private void OpenSkillSelectPanel()
    {
        if (skillSystem == null || skillSelectPanelController == null) return;

        List<SkillRow> candidates = skillSystem.GetRandomCandidates(3);
        if (candidates == null || candidates.Count == 0) return;

        skillSelectPanelController.ShowOptions(candidates, selectedSkill =>
        {
            int equippedSlot = skillSystem.EquipToFirstEmpty(selectedSkill);
            if (equippedSlot >= 0 && skillBarController != null)
            {
                skillBarController.SetSlot(equippedSlot, selectedSkill);
            }
        });
    }

    /// <summary>
    /// 게임 종료 처리 및 결과 UI 활성화
    /// </summary>
    private void EndGame(string message)
    {
        if (gameEnded) return;
        gameEnded = true;
        Time.timeScale = 0f;

        // UI Toolkit 풀스크린 패널 우선 사용
        EnsureResultPanelManager();
        if (resultPanelManager != null)
        {
            if (message == "Victory")
            {
                resultPanelManager.ShowWin();
            }
            else
            {
                resultPanelManager.ShowLose();
            }
            return;
        }

        // 없으면 기존 uGUI 패널 사용
        SetResultUI(true, message);
    }

    /// <summary>
    /// ResultPanelManager(UI Toolkit)를 찾아 연결
    /// </summary>
    private void EnsureResultPanelManager()
    {
        if (resultPanelManager != null) return;

#if UNITY_2022_2_OR_NEWER
        resultPanelManager = FindFirstObjectByType<ResultPanelManager>();
#else
        resultPanelManager = GameObject.FindObjectOfType<ResultPanelManager>();
#endif
    }

    /// <summary>
    /// BaseHealth 오브젝트를 씬에서 찾아 연결, 없으면 새로 생성
    /// </summary>
    private void EnsureBaseHealth()
    {
        if (baseHealth != null)
        {
            baseHealth.BindGameManager(this);
            return;
        }

        GameObject baseObject = GameObject.Find(baseObjectName);
        if (baseObject == null)
            baseObject = GameObject.Find("Base");
        if (baseObject == null)
            return;

        baseHealth = baseObject.GetComponent<BaseHealth>();
        if (baseHealth == null)
            baseHealth = baseObject.AddComponent<BaseHealth>();
        baseHealth.BindGameManager(this);
    }

    /// <summary>
    /// HUD 오브젝트 생성 및 연결. 프리팹 우선, 없으면 동적 생성.
    /// </summary>
    private void EnsureHUD()
    {
        // 1) BattleHUD 프리팹/인스턴스 우선
        if (battleHudInstance == null)
        {
#if UNITY_2022_2_OR_NEWER
            battleHudInstance = FindFirstObjectByType<BattleHUD>();
#else
            battleHudInstance = GameObject.FindObjectOfType<BattleHUD>();
#endif
        }

        if (battleHudInstance == null && battleHudPrefab != null)
        {
            battleHudInstance = Instantiate(battleHudPrefab);
        }

        if (battleHudInstance != null)
        {
            // BattleHUD에서 전부 받아 옴
            targetCanvas = battleHudInstance.Canvas;
            statusText = battleHudInstance.StatusText;
            skillBarController = battleHudInstance.SkillBarController;
            skillSelectPanelController = battleHudInstance.SkillSelectPanelController;
            resultPanel = battleHudInstance.ResultPanel;
            resultText = battleHudInstance.ResultText;
        }

        TryResolveHudReferencesFromCanvas();

        SetupHudIfNeeded();

        if (skillBarController != null)
        {
            skillBarController.Setup(skillSystem);
        }
    }

    private void TryResolveHudReferencesFromCanvas()
    {
        if (targetCanvas == null)
        {
            return;
        }

        if (skillSelectPanelController == null)
        {
            SkillSelectPanelController[] panels = targetCanvas.GetComponentsInChildren<SkillSelectPanelController>(true);
            if (panels != null && panels.Length > 0)
            {
                skillSelectPanelController = panels[0];
            }
            WarnDuplicateControllers("SkillSelectPanelController", panels);
        }

        if (skillBarController == null)
        {
            SkillBarController[] bars = targetCanvas.GetComponentsInChildren<SkillBarController>(true);
            if (bars != null && bars.Length > 0)
            {
                skillBarController = bars[0];
            }
            WarnDuplicateControllers("SkillBarController", bars);
        }

        if (statusText == null)
        {
            TMP_Text[] texts = targetCanvas.GetComponentsInChildren<TMP_Text>(true);
            List<TMP_Text> matchedTexts = new List<TMP_Text>();
            foreach (TMP_Text text in texts)
            {
                if (text != null && text.name.Contains("RunStatusText"))
                {
                    matchedTexts.Add(text);
                }
            }

            if (matchedTexts.Count > 0)
            {
                statusText = matchedTexts[0];
            }

            if (matchedTexts.Count >= 2)
            {
                List<string> names = new List<string>(matchedTexts.Count);
                foreach (TMP_Text text in matchedTexts)
                {
                    names.Add(text.name);
                }

                Debug.LogWarning($"[BattleGameManager] Found {matchedTexts.Count} RunStatusText TMP_Text instances in canvas hierarchy: {string.Join(", ", names)}", this);
            }
        }
    }

    private void SetupHudIfNeeded()
    {
        GameObject searchRoot = battleHudInstance != null
            ? battleHudInstance.gameObject
            : targetCanvas != null
                ? targetCanvas.gameObject
                : null;

        if (searchRoot != null)
        {
            SkillSelectPanelController[] selectPanels = searchRoot.GetComponentsInChildren<SkillSelectPanelController>(true);
            if (selectPanels != null && selectPanels.Length > 0 && skillSelectPanelController == null)
            {
                skillSelectPanelController = selectPanels[0];
            }
            WarnDuplicateControllers("SkillSelectPanelController", selectPanels);

            SkillBarController[] skillBars = searchRoot.GetComponentsInChildren<SkillBarController>(true);
            if (skillBars != null && skillBars.Length > 0 && skillBarController == null)
            {
                skillBarController = skillBars[0];
            }
            WarnDuplicateControllers("SkillBarController", skillBars);

            if (statusText == null)
            {
                TMP_Text[] texts = searchRoot.GetComponentsInChildren<TMP_Text>(true);
                foreach (TMP_Text text in texts)
                {
                    if (text != null && text.name.Contains("RunStatusText"))
                    {
                        statusText = text;
                        break;
                    }
                }
            }
        }

        // BattleHUD가 없으면 직접 생성
        if (targetCanvas == null)
        {
#if UNITY_2022_2_OR_NEWER
            targetCanvas = FindFirstObjectByType<Canvas>();
#else
            targetCanvas = GameObject.FindObjectOfType<Canvas>();
#endif
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

        if (skillSelectPanelController == null)
        {
            CreateDefaultSkillSelectPanel();
        }
    }

    private void WarnDuplicateControllers<T>(string typeName, T[] controllers) where T : Component
    {
        if (controllers == null || controllers.Length < 2)
        {
            return;
        }

        List<string> names = new List<string>(controllers.Length);
        foreach (T controller in controllers)
        {
            if (controller != null)
            {
                names.Add(controller.name);
            }
        }

        Debug.LogWarning($"[BattleGameManager] Found {controllers.Length} {typeName} instances in HUD hierarchy: {string.Join(", ", names)}", this);
    }

    /// <summary>
    /// 기본 스킬바 오브젝트 생성 및 연결
    /// </summary>
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

    /// <summary>
    /// 기본 스킬 선택 패널 생성
    /// </summary>
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

    /// <summary>
    /// 결과 패널 및 텍스트 연결/생성
    /// </summary>
    private void EnsureResultUI()
    {
        // BattleHUD 내장 경우 우선 사용
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

        // 없으면 동적으로 생성
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
#if UNITY_2022_2_OR_NEWER
            resultText = resultPanel.GetComponentInChildren<TMP_Text>(true);
#else
            TMP_Text[] children = resultPanel.GetComponentsInChildren<TMP_Text>(true);
            if (children != null && children.Length > 0)
                resultText = children[0];
#endif
        }
    }

    /// <summary>
    /// 텍스트 객체 생성 헬퍼
    /// </summary>
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

    /// <summary>
    /// 버튼 객체 생성 헬퍼
    /// </summary>
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

    /// <summary>
    /// 플레이어 진행 상태 UI 새로고침
    /// </summary>
    private void RefreshStatusUI()
    {
        if (runSession == null) return;

        if (statusText == null)
        {
            TryResolveStatusTextFallback();
            if (statusText == null)
            {
                Debug.LogWarning("[BattleGameManager] statusText is null. Unable to update run status UI.", this);
                return;
            }
        }

        statusText.text = $"Lv {runSession.Level}  EXP {runSession.Exp}/{runSession.ExpToNextLevel}\nGold {runSession.Gold}";
    }


    private void TryResolveStatusTextFallback()
    {
        GameObject runStatusObject = GameObject.Find("RunStatusText");
        if (runStatusObject != null)
        {
            statusText = runStatusObject.GetComponent<TMP_Text>();
            if (statusText != null)
            {
                return;
            }
        }

#if UNITY_2022_2_OR_NEWER
        statusText = FindFirstObjectByType<TMP_Text>();
#else
        statusText = GameObject.FindObjectOfType<TMP_Text>();
#endif
    }

    /// <summary>
    /// 결과 UI 활성화 및 메시지 표시
    /// </summary>
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
