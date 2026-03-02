using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// ResultUI 오브젝트에 부착.
/// UIDocument에서 root VisualElement를 찾아 승/패 결과를 표시합니다.
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class ResultPanelManager : MonoBehaviour
{
    [Header("Texts (Optional Override)")]
    [SerializeField] private string winTitleText     = "승리";
    [SerializeField] private string winSubtitleText  = "기지를 지켜냈습니다!";
    [SerializeField] private string loseTitleText    = "패배";
    [SerializeField] private string loseSubtitleText = "기지가 파괴되었습니다...";

    [Header("Sort Order (다른 UI 위에 표시)")]
    [Tooltip("Canvas 등 다른 UI보다 높게 설정하세요. (기본 100)")]
    [SerializeField] private int sortOrder = 100;

    // ── 내부 상태 ────────────────────────────────────────────────────────────
    private UIDocument    uiDoc;
    private VisualElement root;
    private Label         titleLabel;
    private Label         descLabel;
    private bool          isInitialized;

    // Show 요청이 init 전에 왔을 때 대기
    private bool   pendingShow;
    private string pendingTitle;
    private string pendingSubtitle;

    // ── 요소 이름 후보 목록 ──────────────────────────────────────────────────
    private static readonly string[] RootCandidates     = { "result-popup-root", "result-root", "root", "ResultRoot", "panel", "container" };
    private static readonly string[] TitleCandidates    = { "result-title",   "title",   "Title",   "resultTitle"   };
    private static readonly string[] DescCandidates     = { "result-description", "result-subtitle", "subtitle", "description", "Subtitle" };
    private static readonly string[] RetryCandidates    = { "retry-button",   "continue-button", "restart-button" };
    private static readonly string[] TitleBtnCandidates = { "title-button",   "back-button",   "TitleButton" };
    private static readonly string[] CloseCandidates    = { "close-button",   "CloseButton" };

    // ────────────────────────────────────────────────────────────────────────

    private void Awake() => TryInit();

    private void Start()
    {
        TryInit();
        ApplySortOrder();
    }

    private void Update()
    {
        // rootVisualElement가 Awake/Start에서 아직 준비되지 않은 경우 매 프레임 재시도
        if (isInitialized) return;

        TryInit();

        if (isInitialized && pendingShow)
        {
            pendingShow = false;
            ShowInternal(pendingTitle, pendingSubtitle);
        }
    }

    private void ApplySortOrder()
    {
        if (uiDoc != null && uiDoc.panelSettings != null)
            uiDoc.panelSettings.sortingOrder = sortOrder;
    }

    private void TryInit()
    {
        if (isInitialized) return;

        uiDoc = GetComponent<UIDocument>();
        if (uiDoc == null) return;

        if (uiDoc.visualTreeAsset == null)
        {
            Debug.LogWarning("[ResultPanelManager] UIDocument에 Source Asset(UXML)이 없습니다.", this);
            return;
        }

        VisualElement docRoot = uiDoc.rootVisualElement;
        if (docRoot == null) return; // 아직 준비 안 됨 → Update에서 재시도

        // ── root 탐색 ─────────────────────────────────────────────────────
        foreach (string n in RootCandidates)
        {
            root = docRoot.Q<VisualElement>(n);
            if (root != null) break;
        }

        if (root == null)
            root = docRoot.Q<VisualElement>(className: "result-root")
                ?? docRoot.Q<VisualElement>(className: "result-popup");

        // 이름 매칭 실패 → TemplateContainer 하위 첫 번째 요소 사용
        if (root == null)
        {
            VisualElement container = docRoot.childCount > 0 ? docRoot[0] : null;
            root = (container?.childCount > 0) ? container[0] : container;
            if (root != null)
                Debug.Log($"[ResultPanelManager] root 이름 매칭 실패. 대체 사용: '{root.name}'", this);
        }

        if (root == null)
        {
            Debug.LogError("[ResultPanelManager] root VisualElement를 찾지 못했습니다. UXML 구조를 확인하세요.", this);
            return;
        }

        // ── 자식 요소 탐색 ────────────────────────────────────────────────
        titleLabel = QueryFirst<Label>(root, TitleCandidates);
        descLabel  = QueryFirst<Label>(root, DescCandidates);

        BindButton(root, RetryCandidates,    OnRetry);
        BindButton(root, TitleBtnCandidates, OnTitle);
        BindButton(root, CloseCandidates,    OnClose);

        SetVisible(false);

        isInitialized = true;
        Debug.Log($"[ResultPanelManager] 초기화 완료. root='{root.name}' title={titleLabel?.name} desc={descLabel?.name}", this);
    }

    // ── 공개 API ─────────────────────────────────────────────────────────────

    public bool ShowWin()  => Show(winTitleText,  winSubtitleText);
    public bool ShowLose() => Show(loseTitleText, loseSubtitleText);

    // ── 내부 ─────────────────────────────────────────────────────────────────

    private bool Show(string title, string subtitle)
    {
        if (!isInitialized) TryInit();

        if (!isInitialized || root == null)
        {
            // 초기화 미완 → Update에서 대기 처리
            pendingShow     = true;
            pendingTitle    = title;
            pendingSubtitle = subtitle;
            Debug.LogWarning("[ResultPanelManager] 초기화 미완. 대기 후 재시도합니다.", this);
            return false;
        }

        ShowInternal(title, subtitle);
        return true;
    }

    private void ShowInternal(string title, string subtitle)
    {
        ApplySortOrder();

        if (titleLabel != null) titleLabel.text = title;
        if (descLabel  != null) descLabel.text  = subtitle;

        SetVisible(true);
    }

    private void SetVisible(bool visible)
    {
        if (root == null) return;
        root.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        root.style.opacity = visible ? 1f : 0f;
        root.pickingMode   = visible ? PickingMode.Position : PickingMode.Ignore;
    }

    private void OnRetry()
    {
        SetVisible(false);
        BattleGameManager.Instance?.Restart();
    }

    private void OnTitle()
    {
        SetVisible(false);
        BattleGameManager.Instance?.BackToTitle();
    }

    private void OnClose() => SetVisible(false);

    // ── 유틸 ─────────────────────────────────────────────────────────────────

    private static T QueryFirst<T>(VisualElement parent, string[] names) where T : VisualElement
    {
        foreach (string n in names)
        {
            var e = parent.Q<T>(n);
            if (e != null) return e;
        }
        return null;
    }

    private static void BindButton(VisualElement parent, string[] names, System.Action onClick)
    {
        foreach (string n in names)
        {
            var btn = parent.Q<Button>(n);
            if (btn != null)
            {
                btn.RegisterCallback<ClickEvent>(_ => onClick?.Invoke());
                return;
            }
        }
    }
}
