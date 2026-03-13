using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ProjectFirst.Data;

namespace Project
{
    public class BattleGameManager : MonoBehaviour
    {
        public static BattleGameManager Instance { get; private set; }

        [Header("Base")]
        [SerializeField] private BaseHealth baseHealth;

        [SerializeField] private string baseObjectName = "Ark_Base";

        [Header("Data")]
        [SerializeField] private MonsterTable monsterTable;

        [SerializeField] private SkillTable skillTable;

        [SerializeField] private Agent playerAgent;

        [Header("Player Data")]
        [SerializeField] private PlayerData playerData;

        [Header("Player Spawn")]
        [Tooltip("체크 시 전투 시작 시 플레이어 위치를 아래 값으로 고정 (캐릭터가 안 보일 때 사용)")]
        [SerializeField] private bool overridePlayerSpawnPosition;
        [SerializeField] private Vector3 playerSpawnPosition = new Vector3(0f, 0f, 0f);

        [Range(0f, 1f)]
        [SerializeField] private float defeatGoldRatio = 0f;

        [Header("HUD")]
        [SerializeField] private Canvas targetCanvas;

        [SerializeField] private CanvasGroup uiCanvasGroup;

        [SerializeField] private StatusHudView statusHudView;

        [SerializeField] private CharUltimateController charUltimateController;

        [SerializeField] private SkillBarController skillBarController;

        [SerializeField] private SkillSelectPanelController skillSelectPanelController;

        [SerializeField] private SkillBarController skillBarPrefab;

        [SerializeField] private SkillSelectPanelController skillSelectPanelPrefab;

        [Header("Result UI")]
        [SerializeField] private GameObject resultPanel;

        [SerializeField] private ResultPanelManager resultPanelManager;

        [Header("Scene")]
        [SerializeField] private string titleSceneName = "Title";

    private TMP_Text resultText;
    private bool gameEnded;
    [SerializeField] private RunSession runSession;
    private SkillSystem skillSystem;
    private Agent[] allAgents;
    private bool isEnemyKilledSubscribed;
    private bool hasLoggedZeroRewardWarning;
    private Animator cachedPlayerAnimator;
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

    private void InitializeRunSession()
    {
        if (runSession == null)
        {
            Debug.LogError("[BattleGameManager] runSession is null! Creating a temporary instance.");
            runSession = ScriptableObject.CreateInstance<RunSession>();
        }
        runSession.Reset();

        if (playerAgent == null)
        {
#if UNITY_2022_2_OR_NEWER
            playerAgent = FindFirstObjectByType<Agent>();
#else
            playerAgent = FindObjectOfType<Agent>();
#endif
        }

        if (overridePlayerSpawnPosition && playerAgent != null)
            playerAgent.transform.position = playerSpawnPosition;

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

    private void HandleEnemyKilled(Enemy enemy)
    {
        if (gameEnded || monsterTable == null || enemy == null) return;
        if (runSession == null) { InitializeRunSession(); if (runSession == null) return; }

        MonsterRow row = monsterTable.GetByIdAndGrade(enemy.MonsterId, enemy.Grade)
                      ?? monsterTable.GetById(enemy.MonsterId);
        if (row == null) return;

        if (row.expReward == 0 && row.goldReward == 0 && !hasLoggedZeroRewardWarning)
        {
            Debug.LogWarning("[Log] 寃쎄퀬媛 諛쒖깮?덉뒿?덈떎.");
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

    private void EndGame(string message)
    {
        if (gameEnded) return;
        gameEnded = true;
        GrantBattleGold(message == "Victory");

        pendingResultMessage = message;
        Invoke("ShowResultDelayed", 0.3f);
    }
    private void GrantBattleGold(bool isVictory)
    {
        if (playerData == null || runSession == null)
        {
            Debug.LogWarning("[Log] 寃쎄퀬媛 諛쒖깮?덉뒿?덈떎.");
            return;
        }

        int earnedGold = runSession.Gold;
        if (earnedGold <= 0) return;

        int grantedGold = isVictory
            ? earnedGold
            : Mathf.FloorToInt(earnedGold * defeatGoldRatio);

        if (grantedGold <= 0) return;

        playerData.AddGold(grantedGold);
        Debug.Log("[Log] ?곹깭媛 媛깆떊?섏뿀?듬땲??");
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

    private void Update()
    {
        CheckCinematicVisibility();
    }

    private void CheckCinematicVisibility()
    {
        if (playerAgent == null) return;
        if (cachedPlayerAnimator == null) CachePlayerAnimator();

        bool inCinematic = false;
        if (cachedPlayerAnimator != null)
        {
            AnimatorStateInfo si = cachedPlayerAnimator.GetCurrentAnimatorStateInfo(0);
            inCinematic = si.IsTag("skill");
        }

        if (inCinematic == isCinematicActive) return;
        isCinematicActive = inCinematic;
        SetUIVisible(!inCinematic);
    }

    private void CachePlayerAnimator()
    {
        cachedPlayerAnimator = null;
        if (playerAgent == null)
            return;

        AgentAnimatorBridge bridge = playerAgent.GetComponentInChildren<AgentAnimatorBridge>(true);
        if (bridge != null && bridge.CachedAnimator != null)
        {
            cachedPlayerAnimator = bridge.CachedAnimator;
            return;
        }

        Animator[] animators = playerAgent.GetComponentsInChildren<Animator>(true);
        foreach (Animator candidate in animators)
        {
            if (candidate != null && candidate.runtimeAnimatorController != null)
            {
                cachedPlayerAnimator = candidate;
                return;
            }
        }

        cachedPlayerAnimator = animators.Length > 0 ? animators[0] : null;
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
        if (statusHudView == null)
            statusHudView = targetCanvas.GetComponentInChildren<StatusHudView>(true);
        if (charUltimateController == null)
            charUltimateController = targetCanvas.GetComponentInChildren<CharUltimateController>(true);
        if (skillBarController == null)
            skillBarController = targetCanvas.GetComponentInChildren<SkillBarController>(true);
        if (skillBarController == null && skillBarPrefab != null)
            skillBarController = Instantiate(skillBarPrefab, targetCanvas.transform);
        if (skillBarController == null)
            skillBarController = CreateDefaultSkillBar();
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
        charUltimateController.OnUltimateRequested -= CastCharacterUltimate;
        charUltimateController.OnUltimateRequested += CastCharacterUltimate;
    }

    private void CastCharacterUltimate(SkillRow skill)
    {
        if (skill == null || playerAgent == null) return;
        EnemyManager em = EnemyManager.Instance;
        if (em == null) return;
        GameObject vfxOverride = playerAgent.AgentData?.characterSkillVfxPrefab;
        skillSystem.CastDirect(skill, vfxOverride);

        charUltimateController.StartCooldown();
        Debug.Log("[Log] ?곹깭媛 媛깆떊?섏뿀?듬땲??");
    }

    private void RefreshStatusUI()
    {
        if (runSession == null) return;
        if (statusHudView == null && targetCanvas != null)
            statusHudView = targetCanvas.GetComponentInChildren<StatusHudView>(true);

        if (statusHudView != null)
        {
            statusHudView.Refresh(runSession.Level, runSession.Exp, runSession.ExpToNextLevel, runSession.Gold);
            return;
        }

        Debug.LogWarning("[Log] 寃쎄퀬媛 諛쒖깮?덉뒿?덈떎.");
    }

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

    private void EnsureBaseHealth()
    {
        if (baseHealth != null) { baseHealth.BindGameManager(this); return; }
        GameObject obj = GameObject.Find(baseObjectName) ?? GameObject.Find("Base");
        if (obj == null) return;
        baseHealth = obj.GetComponent<BaseHealth>() ?? obj.AddComponent<BaseHealth>();
        baseHealth.BindGameManager(this);
    }

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



