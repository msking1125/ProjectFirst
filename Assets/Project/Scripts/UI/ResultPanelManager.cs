using UnityEngine;
using UnityEngine.UIElements;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Project
{
    /// <summary>
    /// ResultUI ?ㅻ툕?앺듃??遺李?
    /// UIDocument?먯꽌 root VisualElement瑜?李얠븘 ????寃곌낵瑜??쒖떆?⑸땲??
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
#if ODIN_INSPECTOR
    [HideMonoScript]
#endif
    public class ResultPanelManager : MonoBehaviour
    {
#if ODIN_INSPECTOR
        [Title("?띿뒪???ㅼ젙", TitleAlignment = TitleAlignments.Left)]
        [HorizontalGroup("?띿뒪??, 0.5f)]
        [BoxGroup("?띿뒪???밸━")]
        [LabelText("?밸━ ??댄?")]
        [Tooltip("?밸━ ???쒖떆????댄?")]
#endif
        [Header("Texts (Optional Override)")]
        [SerializeField] private string winTitleText     = "?밸━";

#if ODIN_INSPECTOR
        [BoxGroup("?띿뒪???밸━")]
        [LabelText("?밸━ ?ㅻ챸")]
        [Tooltip("?밸━ ???쒖떆???ㅻ챸")]
#endif
        [SerializeField] private string winSubtitleText  = "湲곗?瑜?吏耳쒕깉?듬땲??";

#if ODIN_INSPECTOR
        [HorizontalGroup("?띿뒪??, 0.5f)]
        [BoxGroup("?띿뒪???⑤같")]
        [LabelText("?⑤같 ??댄?")]
        [GUIColor(1f, 0.4f, 0.4f)]
        [Tooltip("?⑤같 ???쒖떆????댄?")]
#endif
        [SerializeField] private string loseTitleText    = "?⑤같";

#if ODIN_INSPECTOR
        [BoxGroup("?띿뒪???⑤같")]
        [LabelText("?⑤같 ?ㅻ챸")]
        [GUIColor(1f, 0.4f, 0.4f)]
        [Tooltip("?⑤같 ???쒖떆???ㅻ챸")]
#endif
        [SerializeField] private string loseSubtitleText = "湲곗?媛 ?뚭눼?섏뿀?듬땲??..";

#if ODIN_INSPECTOR
        [Title("罹붾쾭???ㅼ젙", TitleAlignment = TitleAlignments.Left)]
        [BoxGroup("罹붾쾭??)]
        [LabelText("Sort Order")]
        [Tooltip("Canvas ???ㅻⅨ UI蹂대떎 ?믨쾶 ?ㅼ젙?섏꽭?? (湲곕낯 100)")]
        [PropertyRange(0, 999)]
#endif
        [Header("Sort Order (?ㅻⅨ UI ?꾩뿉 ?쒖떆)")]
        [SerializeField] private int sortOrder = 100;

        // ?? ?대? ?곹깭 ????????????????????????????????????????????????????????????
        private UIDocument    uiDoc;
        private VisualElement root;
        private Label         titleLabel;
        private Label         descLabel;
        private bool          isInitialized;

        // Show ?붿껌??init ?꾩뿉 ?붿쓣 ???湲?
        private bool   pendingShow;
        private string pendingTitle;
        private string pendingSubtitle;
        
        // 珥덇린???ъ떆??愿由?
        private int initRetryCount;
        private const int MaxInitRetries = 20;

    // ?? ?붿냼 ?대쫫 ?꾨낫 紐⑸줉 ??????????????????????????????????????????????????
    private static readonly string[] RootCandidates     = { "result-popup-root", "result-root", "root", "ResultRoot", "panel", "container" };
    private static readonly string[] TitleCandidates    = { "result-title",   "title",   "Title",   "resultTitle"   };
    private static readonly string[] DescCandidates     = { "result-description", "result-subtitle", "subtitle", "description", "Subtitle" };
    private static readonly string[] RetryCandidates    = { "retry-button",   "continue-button", "restart-button" };
    private static readonly string[] TitleBtnCandidates = { "title-button",   "back-button",   "TitleButton" };
    private static readonly string[] CloseCandidates    = { "close-button",   "CloseButton" };

    // ????????????????????????????????????????????????????????????????????????

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

        // UI Toolkit???먮룞?쇰줈 UXML??遺숈뿬 二쇱? 紐삵븯??寃쎌슦 ?鍮?
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

        // ?? root ?먯깋 ?????????????????????????????????????????????????????
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

        // ?대쫫 留ㅼ묶 ?ㅽ뙣 ??TemplateContainer ?섏쐞 泥?踰덉㎏ ?붿냼 ?ъ슜
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
                Debug.LogError("[ResultPanelManager] 珥덇린??誘몄셿?쇰줈 寃곌낵李??쒖떆瑜??ъ떆?꾪뻽?쇰굹 ?ㅽ뙣?덉뒿?덈떎 (理쒕? ?잛닔 ?꾨떖). UXML 援ъ“瑜??뺤씤?섏꽭??", this);
                pendingShow = false;
            }
            return;
        }

        // ?? ?먯떇 ?붿냼 ?먯깋 ????????????????????????????????????????????????
        titleLabel = QueryFirst<Label>(root, TitleCandidates);
        descLabel  = QueryFirst<Label>(root, DescCandidates);

        BindButton(root, RetryCandidates,    OnRetry);
        BindButton(root, TitleBtnCandidates, OnTitle);
        BindButton(root, CloseCandidates,    OnClose);

        SetVisible(false);

        isInitialized = true;
        CancelInvoke(nameof(TryInit));
        ApplySortOrder();
        
        Debug.Log($"[ResultPanelManager] 珥덇린???꾨즺. root='{root.name}' title={titleLabel?.name} desc={descLabel?.name}", this);
    }

    // ?? 怨듦컻 API ?????????????????????????????????????????????????????????????

    public bool ShowWin()  => Show(winTitleText,  winSubtitleText);
    public bool ShowLose() => Show(loseTitleText, loseSubtitleText);

    // ?? ?대? ?????????????????????????????????????????????????????????????????

    private bool Show(string title, string subtitle)
    {
        if (!isInitialized) TryInit();

        if (uiDoc == null || uiDoc.visualTreeAsset == null)
        {
            Debug.Log("[ResultPanelManager] UXML 誘명븷?? ?泥?fallback) UI瑜??ъ슜?⑸땲??");
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

    // ?? ?좏떥 ?????????????????????????????????????????????????????????????????

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

