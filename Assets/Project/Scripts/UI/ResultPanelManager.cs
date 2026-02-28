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
        if (uiDoc == null || uiDoc.visualTreeAsset == null)
        {
            // visualTreeAsset 자체가 등록 안되어 있으면 어쩔 수 없이 실패
            return;
        }

        VisualElement docRoot = uiDoc.rootVisualElement;
        if (docRoot == null) return;

        // ── root 탐색 ─────────────────────────────────────────────────────
        // UXML이 로드되면 TemplateContainer 하위에 생성되므로 트리 전체에서 검색합니다.
        foreach (string n in RootCandidates)
        {
            root = docRoot.Q<VisualElement>(n);
            if (root != null) break;
        }

        // 클래스명으로도 한 번 더 찾아봄 (UXML에 등록된 class="result-root")
        if (root == null)
        {
            root = docRoot.Q<VisualElement>(className: "result-root");
        }

        // 이름 매칭 실패 → 첫 번째 유효한 자식 VisualElement 사용
        if (root == null && docRoot.childCount > 0)
        {
            // TemplateContainer 내부의 첫번째 요소를 찾음
            if (docRoot[0].childCount > 0)
                root = docRoot[0][0];
            else
                root = docRoot[0];
            Debug.Log($"[ResultPanelManager] root 이름 매칭 실패. 대체 요소를 사용합니다: '{root.name}'");
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

    public bool ShowWin()  => Show(winTitleText,  winSubtitleText);
    public bool ShowLose() => Show(loseTitleText, loseSubtitleText);

    // ── 내부 ─────────────────────────────────────────────────────────────────

    private bool Show(string title, string subtitle)
    {
        if (!isInitialized) TryInit();

        if (root == null)
        {
            Debug.LogWarning("[ResultPanelManager] UIDocument에 연결된 UXML(Source Asset)이 설정되지 않거나 비어 있어 패널을 표시할 수 없습니다. (fallback uGUI가 대신 사용됩니다.)", this);
            return false;
        }

        if (titleLabel != null) titleLabel.text = title;
        if (descLabel  != null) descLabel.text  = subtitle;

        SetVisible(true);
        return true;
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
