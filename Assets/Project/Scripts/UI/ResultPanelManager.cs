using UnityEngine;
using UnityEngine.UIElements;

namespace Project
{
    /// <summary>
    /// Documentation cleaned.
    /// Documentation cleaned.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class ResultPanelManager : MonoBehaviour
    {
        [Header("Texts (Optional Override)")]
        [SerializeField] private string winTitleText     = "승리";
        [SerializeField] private string winSubtitleText  = "기지를 지켜냈습니다!";
        [SerializeField] private string loseTitleText    = "패배";
        [SerializeField] private string loseSubtitleText = "기지가 파괴되었습니다...";
        [Header("Settings")]
        [SerializeField] private int sortOrder = 100;

        // Note: cleaned comment.
        private UIDocument    uiDoc;
        private VisualElement root;
        private Label         titleLabel;
        private Label         descLabel;
        private bool          isInitialized;

        // Note: cleaned comment.
        private bool   pendingShow;
        private string pendingTitle;
        private string pendingSubtitle;
        
        // Note: cleaned comment.
        private int initRetryCount;
        private const int MaxInitRetries = 20;

    // Note: cleaned comment.
    private static readonly string[] RootCandidates     = { "result-popup-root", "result-root", "root", "ResultRoot", "panel", "container" };
    private static readonly string[] TitleCandidates    = { "result-title",   "title",   "Title",   "resultTitle"   };
    private static readonly string[] DescCandidates     = { "result-description", "result-subtitle", "subtitle", "description", "Subtitle" };
    private static readonly string[] RetryCandidates    = { "retry-button",   "continue-button", "restart-button" };
    private static readonly string[] TitleBtnCandidates = { "title-button",   "back-button",   "TitleButton" };
    private static readonly string[] CloseCandidates    = { "close-button",   "CloseButton" };

    // Note: cleaned comment.

    private void OnEnable()
    {
        TryInit();
        if (!isInitialized)
        {
            InvokeRepeating(nameof(TryInit), 0.1f, 0.1f);
        }
    }

    private void Start()
    {
        ApplySortOrder();
    }

    private void Update()
    {
        if (isInitialized && pendingShow)
        {
            ShowInternal(pendingTitle, pendingSubtitle);
            pendingShow = false;
        }
    }

    private void ApplySortOrder()
    {
        if (uiDoc != null && uiDoc.panelSettings != null)
        {
            uiDoc.panelSettings.sortingOrder = sortOrder;
        }
    }

    private void TryInit()
    {
        if (isInitialized)
        {
            CancelInvoke(nameof(TryInit));
            return;
        }

        if (uiDoc == null)
            uiDoc = GetComponent<UIDocument>();

        if (uiDoc == null)
            return;

        if (uiDoc.visualTreeAsset == null)
            return;

        // Note: cleaned comment.
        VisualElement docRoot = uiDoc.rootVisualElement;
        
        if (docRoot == null && initRetryCount < MaxInitRetries)
        {
            initRetryCount++;
            return;
        }

        if (docRoot == null)
        {
            docRoot = new VisualElement();
        }

        if (docRoot.childCount == 0 && uiDoc.visualTreeAsset != null)
        {
            VisualElement cloned = uiDoc.visualTreeAsset.CloneTree();
            docRoot.Add(cloned);
            docRoot = cloned;
        }

        // Note: cleaned comment.
        foreach (string n in RootCandidates)
        {
            root = docRoot.Q<VisualElement>(n);
            if (root != null) break;
        }

        if (root == null)
        {
            root = docRoot.Q<VisualElement>(className: "result-root")
                ?? docRoot.Q<VisualElement>(className: "result-popup");
        }

        // Note: cleaned comment.
        if (root == null)
        {
            VisualElement container = docRoot.childCount > 0 ? docRoot[0] : null;
            root = (container?.childCount > 0) ? container[0] : container;
        }

        if (root == null)
        {
            initRetryCount++;
            if (initRetryCount >= MaxInitRetries)
            {
                CancelInvoke(nameof(TryInit));
                Debug.LogError("[Log] Error message cleaned.");
                pendingShow = false;
            }
            return;
        }

        // Note: cleaned comment.
        titleLabel = QueryFirst<Label>(root, TitleCandidates);
        descLabel  = QueryFirst<Label>(root, DescCandidates);

        BindButton(root, RetryCandidates,    OnRetry);
        BindButton(root, TitleBtnCandidates, OnTitle);
        BindButton(root, CloseCandidates,    OnClose);

        SetVisible(false);

        isInitialized = true;
        CancelInvoke(nameof(TryInit));
        ApplySortOrder();
        
        Debug.Log("[Log] Message cleaned.");
    }

    // Note: cleaned comment.

    public bool ShowWin()  => Show(winTitleText,  winSubtitleText);
    public bool ShowLose() => Show(loseTitleText, loseSubtitleText);

    // Note: cleaned comment.

    private bool Show(string title, string subtitle)
    {
        if (!isInitialized) TryInit();

        if (uiDoc == null || uiDoc.visualTreeAsset == null)
        {
            Debug.Log("[Log] Message cleaned.");
            return false;
        }

        if (!isInitialized || root == null)
        {
            pendingShow     = true;
            pendingTitle    = title;
            pendingSubtitle = subtitle;
            return true; // Return true as we're handling it via delay
        }

        ShowInternal(title, subtitle);
        return true;
    }

    private void ShowInternal(string title, string subtitle)
    {
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

    // Note: cleaned comment.

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
} // namespace Project


