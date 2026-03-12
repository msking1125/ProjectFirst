using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Project
{
    /// <summary>
    /// BattleGameManager: 전투 씬의 게임 흐름, 보상, UI를 관리하는 싱글턴.
    /// </summary>
#if ODIN_INSPECTOR
    [HideMonoScript]
#endif
    public class BattleGameManager : MonoBehaviour
    {
        public static BattleGameManager Instance { get; private set; }

        // ── Inspector 필드 ────────────────────────────────────────────────────────

#if ODIN_INSPECTOR
        [Title("기지 및 데이터", TitleAlignment = TitleAlignments.Left)]
        [HorizontalGroup("Base", 0.5f)]
        [BoxGroup("Base/기지")]
        [LabelText("기지 체력")]
        [Required]
        [Tooltip("기지(Ark)의 체력 컴포넌트")]
#endif
        [Header("Base")]
        [SerializeField] private BaseHealth baseHealth;

#if ODIN_INSPECTOR
        [BoxGroup("Base/이름")]
        [LabelText("기지 오브젝트명")]
        [Tooltip("Scene에서 기지 오브젝트 이름")]
#endif
        [SerializeField] private string baseObjectName = "Ark_Base";

#if ODIN_INSPECTOR
        [Title("테이블 데이터", TitleAlignment = TitleAlignments.Left)]
        [HorizontalGroup("Data", 0.5f)]
        [BoxGroup("Data/몬스터")]
        [LabelText("몬스터 테이블")]
        [Required]
        [AssetsOnly]
        [Tooltip("몬스터 데이터 테이블 SO")]
#endif
        [Header("Data")]
        [SerializeField] private MonsterTable monsterTable;

#if ODIN_INSPECTOR
        [HorizontalGroup("Data", 0.5f)]
        [BoxGroup("Data/스킬")]
        [LabelText("스킬 테이블")]
        [Required]
        [AssetsOnly]
        [Tooltip("스킬 데이터 테이블 SO")]
#endif
        [SerializeField] private SkillTable skillTable;

#if ODIN_INSPECTOR
        [BoxGroup("Data/플레이어")]
        [LabelText("플레이어 에이전트")]
        [Required]
        [SceneObjectsOnly]
        [Tooltip("플레이어 캐릭터 Agent 컴포넌트")]
#endif
        [SerializeField] private Agent playerAgent;

#if ODIN_INSPECTOR
        [Title("플레이어 데이터", TitleAlignment = TitleAlignments.Left)]
        [HorizontalGroup("Player", 0.5f)]
        [BoxGroup("Player/SO")]
        [LabelText("PlayerData")]
        [Required]
        [AssetsOnly]
        [Tooltip("승리 보상 골드를 귀속시킬 PlayerData 에셋")]
#endif
        [Header("Player Data")]
        [Tooltip("승리 보상 골드를 귀속시킬 PlayerData 에셋을 연결하세요.")]
        [SerializeField] private PlayerData playerData;

#if ODIN_INSPECTOR
        [HorizontalGroup("Player", 0.5f)]
        [BoxGroup("Player/비율")]
        [LabelText("패배 보상 비율")]
        [PropertyRange(0f, 1f)]
        [SuffixLabel("%", true)]
        [Tooltip("패배 시에도 획득 골드의 이 비율만큼 지급 (0 = 미지급, 0.5 = 50%)")]
#endif
        [Tooltip("패배 시에도 획득 골드의 이 비율만큼 지급합니다. (0 = 미지급, 0.5 = 50%)")]
        [Range(0f, 1f)]
        [SerializeField] private float defeatGoldRatio = 0f;

#if ODIN_INSPECTOR
        [Title("HUD 연결", TitleAlignment = TitleAlignments.Left)]
        [HorizontalGroup("HUD", 0.5f)]
        [BoxGroup("HUD/캔버스")]
        [LabelText("타겟 캔버스")]
        [SceneObjectsOnly]
#endif
        [Header("HUD")]
        [SerializeField] private Canvas targetCanvas;

#if ODIN_INSPECTOR
        [BoxGroup("HUD/캔버스")]
        [LabelText("UI CanvasGroup")]
        [Tooltip("시네마틱 중 숨길 UI CanvasGroup (비우면 targetCanvas에 자동 추가)")]
        [SceneObjectsOnly]
#endif
        [Tooltip("시네마틱 중 숨길 UI CanvasGroup (비우면 targetCanvas에 자동 추가)")]
        [SerializeField] private CanvasGroup uiCanvasGroup;

#if ODIN_INSPECTOR
        [HorizontalGroup("HUD", 0.5f)]
        [BoxGroup("HUD/뷰")]
        [LabelText("Status HUD")]
        [Tooltip("레벨/경험치/골드 표시 HUD")]
        [SceneObjectsOnly]
#endif
        [SerializeField] private StatusHudView statusHudView;

#if ODIN_INSPECTOR
        [BoxGroup("HUD/뷰")]
        [LabelText("궁극기 컨트롤러")]
        [Tooltip("캐릭터 고유 스킬 버튼 컨트롤러")]
        [SceneObjectsOnly]
#endif
        [SerializeField] private CharUltimateController charUltimateController;

#if ODIN_INSPECTOR
        [HorizontalGroup("HUD/스킬", 0.5f)]
        [BoxGroup("HUD/스킬/바")]
        [LabelText("스킬 바")]
        [Tooltip("장착된 스킬 표시 바")]
        [SceneObjectsOnly]
#endif
        [SerializeField] private SkillBarController skillBarController;

#if ODIN_INSPECTOR
        [HorizontalGroup("HUD/스킬", 0.5f)]
        [BoxGroup("HUD/스킬/선택")]
        [LabelText("스킬 선택 패널")]
        [Tooltip("레벨업 시 스킬 선택 패널")]
        [SceneObjectsOnly]
#endif
        [SerializeField] private SkillSelectPanelController skillSelectPanelController;

#if ODIN_INSPECTOR
        [BoxGroup("HUD/프리팹")]
        [LabelText("스킬 바 프리팹")]
        [AssetsOnly]
        [Tooltip("동적 생성용 스킬 바 프리팹")]
#endif
        [SerializeField] private SkillBarController skillBarPrefab;

#if ODIN_INSPECTOR
        [BoxGroup("HUD/프리팹")]
        [LabelText("스킬 선택 프리팹")]
        [AssetsOnly]
        [Tooltip("동적 생성용 스킬 선택 패널 프리팹")]
#endif
        [SerializeField] private SkillSelectPanelController skillSelectPanelPrefab;

#if ODIN_INSPECTOR
        [Title("결과 UI", TitleAlignment = TitleAlignments.Left)]
        [HorizontalGroup("Result", 0.5f)]
        [BoxGroup("Result/패널")]
        [LabelText("결과 패널")]
        [Tooltip("승리/패배 결과 표시 패널 GameObject")]
        [SceneObjectsOnly]
#endif
        [Header("Result UI")]
        [SerializeField] private GameObject resultPanel;

#if ODIN_INSPECTOR
        [HorizontalGroup("Result", 0.5f)]
        [BoxGroup("Result/매니저")]
        [LabelText("결과 매니저")]
        [Tooltip("ResultPanelManager 컴포넌트")]
        [SceneObjectsOnly]
#endif
        [SerializeField] private ResultPanelManager resultPanelManager;

#if ODIN_INSPECTOR
        [Title("씬 설정", TitleAlignment = TitleAlignments.Left)]
        [BoxGroup("씬")]
        [LabelText("타이틀 씬 이름")]
        [Tooltip("BackToTitle()로 이동할 씬 이름")]
#endif
        [Header("Scene")]
        [SerializeField] private string titleSceneName = "Title";

    // ── 런타임 ───────────────────────────────────────────────────────────────

    private TMP_Text resultText;
    private bool gameEnded;
    private RunSession runSession;
    private SkillSystem skillSystem;
    private Agent[] allAgents;
    private bool isEnemyKilledSubscribed;
    private bool hasLoggedZeroRewardWarning;
    private Animator cachedPlayerAnimator;

    // ── 생명주기 ─────────────────────────────────────────────────────────────

    // 시네마틱 상태
    private bool isCinematicActive;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
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

    private void OnEnable()  => EnsureEnemyKilledSubscription();
    private void Start()     => EnsureEnemyKilledSubscription();

    private void OnDisable()
    {
        if (!isEnemyKilledSubscribed) return;
        Enemy.EnemyKilled -= HandleEnemyKilled;
        isEnemyKilledSubscribed = false;
    }

    private void OnDestroy()
    {
        if (runSession != null)
        {
            runSession.OnLevelChanged -= HandleLevelChanged;
            runSession.OnReachedSkillPickLevel -= HandleReachedSkillPickLevel;
        }
        if (Instance == this) Instance = null;
    }

    // ── 공개 API ─────────────────────────────────────────────────────────────

    public void HandleVictory() => EndGame("Victory");
    public void HandleDefeat()  => EndGame("Defeat");

    public static void EndVictoryFallback()
    {
        BattleGameManager manager = ResolveInstanceFromScene();
        if (manager != null)
        {
            manager.HandleVictory();
            return;
        }

        Debug.LogError("[BattleGameManager] Victory was reported, but no BattleGameManager exists in the active scene.");
    }

    public static void ReportBaseDestroyed()
    {
        BattleGameManager manager = ResolveInstanceFromScene();
        if (manager != null)
        {
            manager.HandleDefeat();
            return;
        }

        Debug.LogError("[BattleGameManager] Base destruction was reported, but no BattleGameManager exists in the active scene.");
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void BackToTitle()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(Application.CanStreamedLevelBeLoaded(titleSceneName) ? titleSceneName : "Battle_Test");
    }

    // ── 초기화 ───────────────────────────────────────────────────────────────

    private void InitializeRunSession()
    {
        if (runSession == null) runSession = new RunSession();
        runSession.Reset();

        if (playerAgent == null)
        {
#if UNITY_2022_2_OR_NEWER
            playerAgent = FindFirstObjectByType<Agent>();
#else
            playerAgent = FindObjectOfType<Agent>();
#endif
        }

        CachePlayerAnimator();
        skillSystem = new SkillSystem(skillTable, playerAgent);
        SetupCharUltimate();

#if UNITY_2022_2_OR_NEWER
        allAgents = FindObjectsByType<Agent>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
#else
        allAgents = FindObjectsOfType<Agent>();
#endif
        foreach (Agent a in allAgents)
            if (a != null) a.StartCombat();

        runSession.OnLevelChanged -= HandleLevelChanged;
        runSession.OnReachedSkillPickLevel -= HandleReachedSkillPickLevel;
        runSession.OnLevelChanged += HandleLevelChanged;
        runSession.OnReachedSkillPickLevel += HandleReachedSkillPickLevel;
    }

    // ── 이벤트 핸들러 ────────────────────────────────────────────────────────

    private void HandleEnemyKilled(Enemy enemy)
    {
        if (gameEnded || monsterTable == null || enemy == null) return;
        if (runSession == null) { InitializeRunSession(); if (runSession == null) return; }

        MonsterRow row = monsterTable.GetByIdAndGrade(enemy.MonsterId, enemy.Grade)
                      ?? monsterTable.GetById(enemy.MonsterId);
        if (row == null) return;

        if (row.expReward == 0 && row.goldReward == 0 && !hasLoggedZeroRewardWarning)
        {
            Debug.LogWarning($"[BGM] 보상 데이터 0: id={enemy.MonsterId}");
            hasLoggedZeroRewardWarning = true;
        }

        runSession.AddExp(row.expReward);
        runSession.AddGold(row.goldReward);
        RefreshStatusUI();
    }

    private void EnsureEnemyKilledSubscription()
    {
        if (isEnemyKilledSubscribed) return;
        Enemy.EnemyKilled -= HandleEnemyKilled;
        Enemy.EnemyKilled += HandleEnemyKilled;
        isEnemyKilledSubscribed = true;
    }

    private void HandleLevelChanged(int level)      => RefreshStatusUI();
    private void HandleReachedSkillPickLevel(int _) => OpenSkillSelectPanel();

    // ── 스킬 선택 ────────────────────────────────────────────────────────────

    private void OpenSkillSelectPanel()
    {
        if (skillSystem == null || skillSelectPanelController == null) return;

        List<SkillRow> candidates = skillSystem.GetRandomCandidates(3);
        if (candidates == null || candidates.Count == 0) return;

        EnsureSkillPanelFullscreen();
        SetAgentsCombat(false);

        skillSelectPanelController.ShowOptions(candidates, selectedSkill =>
        {
            int slot = skillSystem.EquipToFirstEmpty(selectedSkill);
            if (slot >= 0 && skillBarController != null)
                skillBarController.SetSlot(slot, selectedSkill);
            SetAgentsCombat(true);
        });
    }

    private void EnsureSkillPanelFullscreen()
    {
        if (skillSelectPanelController == null) return;
        RectTransform rt = skillSelectPanelController.GetComponent<RectTransform>();
        if (rt == null) return;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        rt.SetAsLastSibling();
    }

    private void SetAgentsCombat(bool active)
    {
        if (allAgents == null) return;
        foreach (Agent a in allAgents)
        {
            if (a == null) continue;
            if (active) a.ResumeCombat(); else a.PauseCombat();
        }
    }

    private string pendingResultMessage;

    // ── 게임 종료 ────────────────────────────────────────────────────────────

    private void EndGame(string message)
    {
        if (gameEnded) return;
        gameEnded = true;

        // 배틀 보상 골드를 PlayerData에 귀속
        GrantBattleGold(message == "Victory");

        pendingResultMessage = message;
        Invoke("ShowResultDelayed", 0.3f);
    }

    /// <summary>
    /// 배틀 결과에 따라 획득한 골드를 PlayerData에 저장합니다.
    /// </summary>
    private void GrantBattleGold(bool isVictory)
    {
        if (playerData == null || runSession == null)
        {
            Debug.LogWarning("[BattleGameManager] PlayerData 또는 RunSession이 null — 골드 귀속 스킵.");
            return;
        }

        int earnedGold = runSession.Gold;
        if (earnedGold <= 0) return;

        int grantedGold = isVictory
            ? earnedGold
            : Mathf.FloorToInt(earnedGold * defeatGoldRatio);

        if (grantedGold <= 0) return;

        playerData.AddGold(grantedGold);
        Debug.Log($"[BattleGameManager] 골드 귀속: {grantedGold} ({(isVictory ? "승리" : "패배")}) -> PlayerData.gold={playerData.gold}");
    }

    private void ShowResultDelayed()
    {
        Time.timeScale = 0f;

        EnsureResultPanelManager();
        if (resultPanelManager != null)
        {
            if (!resultPanelManager.gameObject.activeSelf)
            {
                resultPanelManager.gameObject.SetActive(true);
            }

            if (pendingResultMessage == "Victory") resultPanelManager.ShowWin();
            else                                   resultPanelManager.ShowLose();
            
            pendingResultMessage = null;
            return;
        }
        
        SetResultUI(true, pendingResultMessage);
        pendingResultMessage = null;
    }

    // ── HUD 초기화 ───────────────────────────────────────────────────────────

    private void Update()
    {
        CheckCinematicVisibility();
    }

    // ── 시네마틱 UI 숨김 ────────────────────────────────────────────────────────

    private void CheckCinematicVisibility()
    {
        if (playerAgent == null) return;
        if (cachedPlayerAnimator == null) CachePlayerAnimator();

        bool inCinematic = false;
        if (cachedPlayerAnimator != null)
        {
            AnimatorStateInfo si = cachedPlayerAnimator.GetCurrentAnimatorStateInfo(0);
            inCinematic = si.IsName("Tabi_skill_action") || si.IsName("Tabi_skill");
        }

        if (inCinematic == isCinematicActive) return;
        isCinematicActive = inCinematic;
        SetUIVisible(!inCinematic);
    }

    private void CachePlayerAnimator()
    {
        cachedPlayerAnimator = playerAgent != null
            ? playerAgent.GetComponentInChildren<Animator>(true)
            : null;
    }

    private static BattleGameManager ResolveInstanceFromScene()
    {
        if (Instance != null)
            return Instance;

#if UNITY_2022_2_OR_NEWER
        return FindFirstObjectByType<BattleGameManager>();
#else
        return FindObjectOfType<BattleGameManager>();
#endif
    }

    /// <summary>UI Canvas 전체를 즉시 표시/숨깁니다.</summary>
    public void SetUIVisible(bool visible)
    {
        if (uiCanvasGroup == null && targetCanvas != null)
        {
            uiCanvasGroup = targetCanvas.GetComponent<CanvasGroup>();
            if (uiCanvasGroup == null)
                uiCanvasGroup = targetCanvas.gameObject.AddComponent<CanvasGroup>();
        }
        if (uiCanvasGroup == null) return;

        uiCanvasGroup.alpha          = visible ? 1f : 0f;
        uiCanvasGroup.interactable   = visible;
        uiCanvasGroup.blocksRaycasts = visible;
    }

    private void EnsureHUD()
    {
        // Canvas 탐색/생성
        if (targetCanvas == null)
        {
#if UNITY_2022_2_OR_NEWER
            targetCanvas = FindFirstObjectByType<Canvas>();
#else
            targetCanvas = FindObjectOfType<Canvas>();
#endif
        }
        if (targetCanvas == null)
        {
            var go = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            targetCanvas = go.GetComponent<Canvas>();
            targetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        // StatusHudView 자동 탐색
        if (statusHudView == null)
            statusHudView = targetCanvas.GetComponentInChildren<StatusHudView>(true);

        // CharUltimateController 자동 탐색
        if (charUltimateController == null)
            charUltimateController = targetCanvas.GetComponentInChildren<CharUltimateController>(true);

        // SkillBarController: Inspector → Canvas 탐색 → 프리팹 인스턴스 → 동적 생성
        if (skillBarController == null)
            skillBarController = targetCanvas.GetComponentInChildren<SkillBarController>(true);
        if (skillBarController == null && skillBarPrefab != null)
            skillBarController = Instantiate(skillBarPrefab, targetCanvas.transform);
        if (skillBarController == null)
            skillBarController = CreateDefaultSkillBar();

        // SkillSelectPanelController: Inspector → Canvas 탐색 → 프리팹 → 씬 전체 → 동적 생성
        if (skillSelectPanelController == null)
            skillSelectPanelController = targetCanvas.GetComponentInChildren<SkillSelectPanelController>(true);
        if (skillSelectPanelController == null && skillSelectPanelPrefab != null)
            skillSelectPanelController = Instantiate(skillSelectPanelPrefab, targetCanvas.transform);
        if (skillSelectPanelController == null)
        {
#if UNITY_2022_2_OR_NEWER
            skillSelectPanelController = FindFirstObjectByType<SkillSelectPanelController>(FindObjectsInactive.Include);
#else
            skillSelectPanelController = FindObjectOfType<SkillSelectPanelController>(true);
#endif
        }
        if (skillSelectPanelController == null)
            skillSelectPanelController = CreateDefaultSkillSelectPanel();

        if (skillBarController != null)
            skillBarController.Setup(skillSystem);
    }

    // ── 캐릭터 고유 스킬 ────────────────────────────────────────────────────────

    private void SetupCharUltimate()
    {
        if (charUltimateController == null)
        {
            if (targetCanvas != null)
                charUltimateController = targetCanvas.GetComponentInChildren<CharUltimateController>(true);
#if UNITY_2022_2_OR_NEWER
            if (charUltimateController == null)
                charUltimateController = FindFirstObjectByType<CharUltimateController>(FindObjectsInactive.Include);
#else
            if (charUltimateController == null)
                charUltimateController = FindObjectOfType<CharUltimateController>();
#endif
        }
        if (charUltimateController == null) return;

        charUltimateController.Setup(playerAgent?.AgentData, skillTable);
        charUltimateController.OnUltimateRequested += CastCharacterUltimate;
    }

    private void CastCharacterUltimate(SkillRow skill)
    {
        if (skill == null || playerAgent == null) return;
        EnemyManager em = EnemyManager.Instance;
        if (em == null) return;

        // AgentData.characterSkillVfxPrefab 우선, 없으면 SkillRow.castVfxPrefab 사용
        GameObject vfxOverride = playerAgent.AgentData?.characterSkillVfxPrefab;

        // SkillSystem에 직접 캐스트 위임 (VFX + 데미지 + 버프/디버프 로직 재사용)
        skillSystem.CastDirect(skill, vfxOverride);

        charUltimateController.StartCooldown();
        Debug.Log($"[BGM] 캐릭터 스킬 발동: {skill.name}");
    }

    // ── 상태 UI ──────────────────────────────────────────────────────────────

    private void RefreshStatusUI()
    {
        if (runSession == null) return;

        // StatusHudView 자동 탐색
        if (statusHudView == null && targetCanvas != null)
            statusHudView = targetCanvas.GetComponentInChildren<StatusHudView>(true);

        if (statusHudView != null)
        {
            statusHudView.Refresh(runSession.Level, runSession.Exp, runSession.ExpToNextLevel, runSession.Gold);
            return;
        }

        Debug.LogWarning("[BGM] StatusHudView가 없습니다. Canvas → StatusHud 오브젝트에 StatusHudView를 부착하세요.");
    }

    // ── 결과 UI ──────────────────────────────────────────────────────────────

    private void EnsureResultPanelManager()
    {
        if (resultPanelManager != null) return;
#if UNITY_2022_2_OR_NEWER
        resultPanelManager = FindFirstObjectByType<ResultPanelManager>();
#else
        resultPanelManager = FindObjectOfType<ResultPanelManager>();
#endif
    }

    private void EnsureResultUI()
    {
        if (resultPanel != null)
        {
            resultText ??= resultPanel.GetComponentInChildren<TMP_Text>(true);
            return;
        }
        if (targetCanvas == null) return;

        resultPanel = new GameObject("ResultPanel", typeof(RectTransform), typeof(Image));
        resultPanel.transform.SetParent(targetCanvas.transform, false);
        var rt = resultPanel.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(560f, 320f);
        resultPanel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.85f);

        resultText = CreateText("ResultText", resultPanel.transform, 64f, new Vector2(0f, 96f), 56f);
        CreateButton("RestartButton",  "Restart",       new Vector2(0f, -12f),  Restart);
        CreateButton("BackButton",     "Back To Title", new Vector2(0f, -102f), BackToTitle);
    }

    private void SetResultUI(bool active, string message)
    {
        resultPanel?.SetActive(active);
        if (resultText != null) resultText.text = message;
    }

    // ── 기지 ─────────────────────────────────────────────────────────────────

    private void EnsureBaseHealth()
    {
        if (baseHealth != null) { baseHealth.BindGameManager(this); return; }
        GameObject obj = GameObject.Find(baseObjectName) ?? GameObject.Find("Base");
        if (obj == null) return;
        baseHealth = obj.GetComponent<BaseHealth>() ?? obj.AddComponent<BaseHealth>();
        baseHealth.BindGameManager(this);
    }

    // ── 동적 UI 생성 헬퍼 ────────────────────────────────────────────────────

    private SkillBarController CreateDefaultSkillBar()
    {
        var go = new GameObject("SkillBar", typeof(RectTransform), typeof(SkillBarController));
        go.transform.SetParent(targetCanvas.transform, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 0f); rt.anchorMax = new Vector2(1f, 0f);
        rt.pivot = new Vector2(1f, 0f);
        rt.anchoredPosition = new Vector2(-20f, 20f);
        rt.sizeDelta = new Vector2(740f, 90f);

        var btns = new Button[3]; var lbls = new TMP_Text[3];
        for (int i = 0; i < 3; i++)
        {
            var slot = new GameObject($"SkillSlot_{i}", typeof(RectTransform), typeof(Image), typeof(Button));
            slot.transform.SetParent(go.transform, false);
            var sr = slot.GetComponent<RectTransform>();
            sr.anchorMin = sr.anchorMax = sr.pivot = Vector2.zero;
            sr.anchoredPosition = new Vector2(i * 240f, 0f);
            sr.sizeDelta = new Vector2(220f, 68f);
            btns[i] = slot.GetComponent<Button>();
            lbls[i] = CreateText($"Label_{i}", slot.transform, 40f, Vector2.zero, 24f);
            lbls[i].alignment = TextAlignmentOptions.Center;
        }
        var bar = go.GetComponent<SkillBarController>();
        bar.Configure(btns[0], btns[1], btns[2], lbls[0], lbls[1], lbls[2]);
        return bar;
    }

    private SkillSelectPanelController CreateDefaultSkillSelectPanel()
    {
        var go = new GameObject("SkillSelectPanel", typeof(RectTransform), typeof(Image), typeof(SkillSelectPanelController));
        go.transform.SetParent(targetCanvas.transform, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(640f, 240f);
        go.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.86f);

        var btns = new Button[3]; var lbls = new TMP_Text[3];
        for (int i = 0; i < 3; i++)
        {
            var opt = new GameObject($"Option_{i}", typeof(RectTransform), typeof(Image), typeof(Button));
            opt.transform.SetParent(go.transform, false);
            var or2 = opt.GetComponent<RectTransform>();
            or2.sizeDelta = new Vector2(180f, 120f);
            or2.anchoredPosition = new Vector2(-210f + 210f * i, 0f);
            btns[i] = opt.GetComponent<Button>();
            lbls[i] = CreateText($"OptionLabel_{i}", opt.transform, 24f, Vector2.zero, 24f);
            lbls[i].alignment = TextAlignmentOptions.Center;
        }
        var panel = go.GetComponent<SkillSelectPanelController>();
        panel.Configure(go, btns[0], btns[1], btns[2], lbls[0], lbls[1], lbls[2]);
        return panel;
    }

    private TMP_Text CreateText(string objName, Transform parent, float width, Vector2 pos, float fontSize)
    {
        var go = new GameObject(objName, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(width * 6f, 90f);
        rt.anchoredPosition = pos;
        var t = go.GetComponent<TextMeshProUGUI>();
        t.alignment = TextAlignmentOptions.Center;
        t.fontSize = fontSize;
        t.color = Color.white;
        return t;
    }

    private void CreateButton(string objName, string label, Vector2 pos, UnityEngine.Events.UnityAction onClick)
    {
        var go = new GameObject(objName, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(resultPanel.transform, false);
        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(300f, 64f);
        rt.anchoredPosition = pos;
        go.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 1f);
        go.GetComponent<Button>().onClick.AddListener(onClick);
        var t = CreateText($"{objName}_Label", go.transform, 48f, Vector2.zero, 34f);
        t.text = label;
    }
}
} // namespace Project
