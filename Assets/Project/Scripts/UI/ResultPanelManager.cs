using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// ResultUI 오브젝트에 부착.
/// UIDocument에서 result-popup-root(또는 첫 번째 VisualElement)를 찾아 승/패 표시합니다.
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class ResultPanelManager : MonoBehaviour
{
    [Header("Texts (Optional Override)")]
    [SerializeField] private string winTitleText     = "승리";
    [SerializeField] private string winSubtitleText  = "기지를 지켜냈습니다!";
    [SerializeField] private string loseTitleText    = "패배";
    [SerializeField] private string loseSubtitleText = "기지가 파괴되었습니다...";

    // ── 내부 상태 ────────────────────────────────────────────────────────────
    private UIDocument  uiDoc;
    private VisualElement root;          // 최상위 패널 (show/hide 대상)
    private Label        titleLabel;
    private Label        descLabel;
    private bool         isInitialized;

    // ── 요소 이름 후보 목록 (어떤 UXML이든 매칭) ─────────────────────────────
    private static readonly string[] RootCandidates   = { "result-popup-root", "result-root", "root", "ResultRoot", "panel" };
    private static readonly string[] TitleCandidates  = { "result-title",    "title",    "Title",    "resultTitle"   };
    private static readonly string[] DescCandidates   = { "result-description", "result-subtitle", "subtitle", "description", "Subtitle" };
    private static readonly string[] RetryCandidates  = { "retry-button",    "continue-button", "restart-button", "RetryButton" };
    private static readonly string[] TitleBtnCandidates = { "title-button",  "back-button",  "TitleButton" };
    private static readonly string[] CloseCandidates  = { "close-button",   "CloseButton" };

    // ────────────────────────────────────────────────────────────────────────

    private void Awake()  => TryInit();
    private void Start()  => TryInit();   // Awake에서 rootVisualElement가 아직 없는 경우 대비

    private void TryInit()
    {
        if (isInitialized) return;

        uiDoc = GetComponent<UIDocument>();
        if (uiDoc == null) return;

        VisualElement docRoot = uiDoc.rootVisualElement;
        if (docRoot == null) return;

        // ── root 탐색 ─────────────────────────────────────────────────────
        root = QueryFirst(docRoot, RootCandidates);

        // 이름 매칭 실패 → 첫 번째 자식 VisualElement 사용
        if (root == null && docRoot.childCount > 0)
        {
            root = docRoot[0];
            Debug.Log($"[ResultPanelManager] root 이름 매칭 실패. 첫 번째 자식 '{root.name}' 을 사용합니다.");
        }

        if (root == null)
        {
            Debug.LogError("[ResultPanelManager] UIDocument에서 표시할 root VisualElement를 찾지 못했습니다.", this);
            return;
        }

        // ── 자식 요소 탐색 ────────────────────────────────────────────────
        titleLabel = QueryFirst<Label>(root, TitleCandidates);
        descLabel  = QueryFirst<Label>(root, DescCandidates);

        // 버튼 바인딩 (없어도 무방)
        BindButton(root, RetryCandidates, OnRetry);
        BindButton(root, TitleBtnCandidates, OnTitle);
        BindButton(root, CloseCandidates, OnClose);

        // 초기 숨김
        SetVisible(false);

        isInitialized = true;
        Debug.Log($"[ResultPanelManager] 초기화 완료. root='{root.name}', title={titleLabel?.name}, desc={descLabel?.name}", this);
    }

    // ── 공개 API ─────────────────────────────────────────────────────────────

    public void ShowWin()  => Show(winTitleText,  winSubtitleText);
    public void ShowLose() => Show(loseTitleText, loseSubtitleText);

    // ── 내부 ─────────────────────────────────────────────────────────────────

    private void Show(string title, string subtitle)
    {
        if (!isInitialized) TryInit();

        if (root == null)
        {
            Debug.LogWarning("[ResultPanelManager] root가 설정되지 않아 결과 패널을 표시할 수 없습니다.", this);
            return;
        }

        if (titleLabel != null) titleLabel.text = title;
        if (descLabel  != null) descLabel.text  = subtitle;

        SetVisible(true);
    }

    private void SetVisible(bool visible)
    {
        if (root == null) return;
        root.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        root.style.opacity = visible ? 1f : 0f;
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

    private static VisualElement QueryFirst(VisualElement parent, string[] names)
    {
        foreach (string n in names)
        {
            var e = parent.Q<VisualElement>(n);
            if (e != null) return e;
        }
        return null;
    }

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
