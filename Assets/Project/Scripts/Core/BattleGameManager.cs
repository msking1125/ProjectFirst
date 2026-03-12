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
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Project
{
    /// <summary>
    /// BattleGameManager: ?꾪닾 ?ъ쓽 寃뚯엫 ?먮쫫, 蹂댁긽, UI瑜?愿由ы븯???깃???
    /// </summary>
#if ODIN_INSPECTOR
    [HideMonoScript]
#endif
    public class BattleGameManager : MonoBehaviour
    {
        public static BattleGameManager Instance { get; private set; }

        // ?? Inspector ?꾨뱶 ????????????????????????????????????????????????????????

#if ODIN_INSPECTOR
        [Title("湲곗? 諛??곗씠??, TitleAlignment = TitleAlignments.Left)]
        [HorizontalGroup("Base", 0.5f)]
        [BoxGroup("Base/湲곗?")]
        [LabelText("湲곗? 泥대젰")]
        [Required]
        [Tooltip("湲곗?(Ark)??泥대젰 而댄룷?뚰듃")]
#endif
        [Header("Base")]
        [SerializeField] private BaseHealth baseHealth;

#if ODIN_INSPECTOR
        [BoxGroup("Base/?대쫫")]
        [LabelText("湲곗? ?ㅻ툕?앺듃紐?)]
        [Tooltip("Scene?먯꽌 湲곗? ?ㅻ툕?앺듃 ?대쫫")]
#endif
        [SerializeField] private string baseObjectName = "Ark_Base";

#if ODIN_INSPECTOR
        [Title("?뚯씠釉??곗씠??, TitleAlignment = TitleAlignments.Left)]
        [HorizontalGroup("Data", 0.5f)]
        [BoxGroup("Data/紐ъ뒪??)]
        [LabelText("紐ъ뒪???뚯씠釉?)]
        [Required]
        [AssetsOnly]
        [Tooltip("紐ъ뒪???곗씠???뚯씠釉?SO")]
#endif
        [Header("Data")]
        [SerializeField] private MonsterTable monsterTable;

#if ODIN_INSPECTOR
        [HorizontalGroup("Data", 0.5f)]
        [BoxGroup("Data/?ㅽ궗")]
        [LabelText("?ㅽ궗 ?뚯씠釉?)]
        [Required]
        [AssetsOnly]
        [Tooltip("?ㅽ궗 ?곗씠???뚯씠釉?SO")]
#endif
        [SerializeField] private SkillTable skillTable;

#if ODIN_INSPECTOR
        [BoxGroup("Data/?뚮젅?댁뼱")]
        [LabelText("?뚮젅?댁뼱 ?먯씠?꾪듃")]
        [Required]
        [SceneObjectsOnly]
        [Tooltip("?뚮젅?댁뼱 罹먮┃??Agent 而댄룷?뚰듃")]
#endif
        [SerializeField] private Agent playerAgent;

#if ODIN_INSPECTOR
        [Title("?뚮젅?댁뼱 ?곗씠??, TitleAlignment = TitleAlignments.Left)]
        [HorizontalGroup("Player", 0.5f)]
        [BoxGroup("Player/SO")]
        [LabelText("PlayerData")]
        [Required]
        [AssetsOnly]
        [Tooltip("?밸━ 蹂댁긽 怨⑤뱶瑜?洹?띿떆??PlayerData ?먯뀑")]
#endif
        [Header("Player Data")]
        [SerializeField] private PlayerData playerData;

#if ODIN_INSPECTOR
        [HorizontalGroup("Player", 0.5f)]
        [BoxGroup("Player/鍮꾩쑉")]
        [LabelText("?⑤같 蹂댁긽 鍮꾩쑉")]
        [PropertyRange(0f, 1f)]
        [SuffixLabel("%", true)]
        [Tooltip("?⑤같 ?쒖뿉???띾뱷 怨⑤뱶????鍮꾩쑉留뚰겮 吏湲?(0 = 誘몄?湲? 0.5 = 50%)")]
#endif
        [Range(0f, 1f)]
        [SerializeField] private float defeatGoldRatio = 0f;

#if ODIN_INSPECTOR
        [Title("HUD ?곌껐", TitleAlignment = TitleAlignments.Left)]
        [HorizontalGroup("HUD", 0.5f)]
        [BoxGroup("HUD/罹붾쾭??)]
        [LabelText("?寃?罹붾쾭??)]
        [SceneObjectsOnly]
#endif
        [Header("HUD")]
        [SerializeField] private Canvas targetCanvas;

#if ODIN_INSPECTOR
        [BoxGroup("HUD/罹붾쾭??)]
        [LabelText("UI CanvasGroup")]
        [Tooltip("?쒕꽕留덊떛 以??④만 UI CanvasGroup (鍮꾩슦硫?targetCanvas???먮룞 異붽?)")]
        [SceneObjectsOnly]
#endif
        [SerializeField] private CanvasGroup uiCanvasGroup;

#if ODIN_INSPECTOR
        [HorizontalGroup("HUD", 0.5f)]
        [BoxGroup("HUD/酉?)]
        [LabelText("Status HUD")]
        [Tooltip("?덈꺼/寃쏀뿕移?怨⑤뱶 ?쒖떆 HUD")]
        [SceneObjectsOnly]
#endif
        [SerializeField] private StatusHudView statusHudView;

#if ODIN_INSPECTOR
        [BoxGroup("HUD/酉?)]
        [LabelText("沅곴레湲?而⑦듃濡ㅻ윭")]
        [Tooltip("罹먮┃??怨좎쑀 ?ㅽ궗 踰꾪듉 而⑦듃濡ㅻ윭")]
        [SceneObjectsOnly]
#endif
        [SerializeField] private CharUltimateController charUltimateController;

#if ODIN_INSPECTOR
        [HorizontalGroup("HUD/?ㅽ궗", 0.5f)]
        [BoxGroup("HUD/?ㅽ궗/諛?)]
        [LabelText("?ㅽ궗 諛?)]
        [Tooltip("?μ갑???ㅽ궗 ?쒖떆 諛?)]
        [SceneObjectsOnly]
#endif
        [SerializeField] private SkillBarController skillBarController;

#if ODIN_INSPECTOR
        [HorizontalGroup("HUD/?ㅽ궗", 0.5f)]
        [BoxGroup("HUD/?ㅽ궗/?좏깮")]
        [LabelText("?ㅽ궗 ?좏깮 ?⑤꼸")]
        [Tooltip("?덈꺼?????ㅽ궗 ?좏깮 ?⑤꼸")]
        [SceneObjectsOnly]
#endif
        [SerializeField] private SkillSelectPanelController skillSelectPanelController;

#if ODIN_INSPECTOR
        [BoxGroup("HUD/?꾨━??)]
        [LabelText("?ㅽ궗 諛??꾨━??)]
        [AssetsOnly]
        [Tooltip("?숈쟻 ?앹꽦???ㅽ궗 諛??꾨━??)]
#endif
        [SerializeField] private SkillBarController skillBarPrefab;

#if ODIN_INSPECTOR
        [BoxGroup("HUD/?꾨━??)]
        [LabelText("?ㅽ궗 ?좏깮 ?꾨━??)]
        [AssetsOnly]
        [Tooltip("?숈쟻 ?앹꽦???ㅽ궗 ?좏깮 ?⑤꼸 ?꾨━??)]
#endif
        [SerializeField] private SkillSelectPanelController skillSelectPanelPrefab;

#if ODIN_INSPECTOR
        [Title("寃곌낵 UI", TitleAlignment = TitleAlignments.Left)]
        [HorizontalGroup("Result", 0.5f)]
        [BoxGroup("Result/?⑤꼸")]
        [LabelText("寃곌낵 ?⑤꼸")]
        [Tooltip("?밸━/?⑤같 寃곌낵 ?쒖떆 ?⑤꼸 GameObject")]
        [SceneObjectsOnly]
#endif
        [Header("Result UI")]
        [SerializeField] private GameObject resultPanel;

#if ODIN_INSPECTOR
        [HorizontalGroup("Result", 0.5f)]
        [BoxGroup("Result/留ㅻ땲?")]
        [LabelText("寃곌낵 留ㅻ땲?")]
        [Tooltip("ResultPanelManager 而댄룷?뚰듃")]
        [SceneObjectsOnly]
#endif
        [SerializeField] private ResultPanelManager resultPanelManager;

#if ODIN_INSPECTOR
        [Title("???ㅼ젙", TitleAlignment = TitleAlignments.Left)]
        [BoxGroup("??)]
        [LabelText("??댄? ???대쫫")]
        [Tooltip("BackToTitle()濡??대룞?????대쫫")]
#endif
        [Header("Scene")]
        [SerializeField] private string titleSceneName = "Title";

    // ?? ?고??????????????????????????????????????????????????????????????????

    private TMP_Text resultText;
    private bool gameEnded;
    private RunSession runSession;
    private SkillSystem skillSystem;
    private Agent[] allAgents;
    private bool isEnemyKilledSubscribed;
    private bool hasLoggedZeroRewardWarning;
    private Animator cachedPlayerAnimator;

    // ?? ?앸챸二쇨린 ?????????????????????????????????????????????????????????????

    // ?쒕꽕留덊떛 ?곹깭
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

    // ?? 怨듦컻 API ?????????????????????????????????????????????????????????????

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

    // ?? 珥덇린?????????????????????????????????????????????????????????????????

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

    // ?? ?대깽???몃뱾??????????????????????????????????????????????????????????

    private void HandleEnemyKilled(Enemy enemy)
    {
        if (gameEnded || monsterTable == null || enemy == null) return;
        if (runSession == null) { InitializeRunSession(); if (runSession == null) return; }

        MonsterRow row = monsterTable.GetByIdAndGrade(enemy.MonsterId, enemy.Grade)
                      ?? monsterTable.GetById(enemy.MonsterId);
        if (row == null) return;

        if (row.expReward == 0 && row.goldReward == 0 && !hasLoggedZeroRewardWarning)
        {
            Debug.LogWarning($"[BGM] 蹂댁긽 ?곗씠??0: id={enemy.MonsterId}");
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

    // ?? ?ㅽ궗 ?좏깮 ????????????????????????????????????????????????????????????

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

    // ?? 寃뚯엫 醫낅즺 ????????????????????????????????????????????????????????????

    private void EndGame(string message)
    {
        if (gameEnded) return;
        gameEnded = true;

        // 諛고? 蹂댁긽 怨⑤뱶瑜?PlayerData??洹??
        GrantBattleGold(message == "Victory");

        pendingResultMessage = message;
        Invoke("ShowResultDelayed", 0.3f);
    }

    /// <summary>
    /// 諛고? 寃곌낵???곕씪 ?띾뱷??怨⑤뱶瑜?PlayerData????ν빀?덈떎.
    /// </summary>
    private void GrantBattleGold(bool isVictory)
    {
        if (playerData == null || runSession == null)
        {
            Debug.LogWarning("[BattleGameManager] PlayerData ?먮뒗 RunSession??null ??怨⑤뱶 洹???ㅽ궢.");
            return;
        }

        int earnedGold = runSession.Gold;
        if (earnedGold <= 0) return;

        int grantedGold = isVictory
            ? earnedGold
            : Mathf.FloorToInt(earnedGold * defeatGoldRatio);

        if (grantedGold <= 0) return;

        playerData.AddGold(grantedGold);
        Debug.Log($"[BattleGameManager] 怨⑤뱶 洹?? {grantedGold} ({(isVictory ? "?밸━" : "?⑤같")}) -> PlayerData.gold={playerData.gold}");
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

    // ?? HUD 珥덇린?????????????????????????????????????????????????????????????

    private void Update()
    {
        CheckCinematicVisibility();
    }

    // ?? ?쒕꽕留덊떛 UI ?④? ????????????????????????????????????????????????????????

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

    /// <summary>UI Canvas ?꾩껜瑜?利됱떆 ?쒖떆/?④퉩?덈떎.</summary>
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
        // Canvas ?먯깋/?앹꽦
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

        // StatusHudView ?먮룞 ?먯깋
        if (statusHudView == null)
            statusHudView = targetCanvas.GetComponentInChildren<StatusHudView>(true);

        // CharUltimateController ?먮룞 ?먯깋
        if (charUltimateController == null)
            charUltimateController = targetCanvas.GetComponentInChildren<CharUltimateController>(true);

        // SkillBarController: Inspector ??Canvas ?먯깋 ???꾨━???몄뒪?댁뒪 ???숈쟻 ?앹꽦
        if (skillBarController == null)
            skillBarController = targetCanvas.GetComponentInChildren<SkillBarController>(true);
        if (skillBarController == null && skillBarPrefab != null)
            skillBarController = Instantiate(skillBarPrefab, targetCanvas.transform);
        if (skillBarController == null)
            skillBarController = CreateDefaultSkillBar();

        // SkillSelectPanelController: Inspector ??Canvas ?먯깋 ???꾨━???????꾩껜 ???숈쟻 ?앹꽦
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

    // ?? 罹먮┃??怨좎쑀 ?ㅽ궗 ????????????????????????????????????????????????????????

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

        // AgentData.characterSkillVfxPrefab ?곗꽑, ?놁쑝硫?SkillRow.castVfxPrefab ?ъ슜
        GameObject vfxOverride = playerAgent.AgentData?.characterSkillVfxPrefab;

        // SkillSystem??吏곸젒 罹먯뒪???꾩엫 (VFX + ?곕?吏 + 踰꾪봽/?붾쾭??濡쒖쭅 ?ъ궗??
        skillSystem.CastDirect(skill, vfxOverride);

        charUltimateController.StartCooldown();
        Debug.Log($"[BGM] 罹먮┃???ㅽ궗 諛쒕룞: {skill.name}");
    }

    // ?? ?곹깭 UI ??????????????????????????????????????????????????????????????

    private void RefreshStatusUI()
    {
        if (runSession == null) return;

        // StatusHudView ?먮룞 ?먯깋
        if (statusHudView == null && targetCanvas != null)
            statusHudView = targetCanvas.GetComponentInChildren<StatusHudView>(true);

        if (statusHudView != null)
        {
            statusHudView.Refresh(runSession.Level, runSession.Exp, runSession.ExpToNextLevel, runSession.Gold);
            return;
        }

        Debug.LogWarning("[BGM] StatusHudView媛 ?놁뒿?덈떎. Canvas ??StatusHud ?ㅻ툕?앺듃??StatusHudView瑜?遺李⑺븯?몄슂.");
    }

    // ?? 寃곌낵 UI ??????????????????????????????????????????????????????????????

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

    // ?? 湲곗? ?????????????????????????????????????????????????????????????????

    private void EnsureBaseHealth()
    {
        if (baseHealth != null) { baseHealth.BindGameManager(this); return; }
        GameObject obj = GameObject.Find(baseObjectName) ?? GameObject.Find("Base");
        if (obj == null) return;
        baseHealth = obj.GetComponent<BaseHealth>() ?? obj.AddComponent<BaseHealth>();
        baseHealth.BindGameManager(this);
    }

    // ?? ?숈쟻 UI ?앹꽦 ?ы띁 ????????????????????????????????????????????????????

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




