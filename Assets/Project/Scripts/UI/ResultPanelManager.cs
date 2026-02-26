using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// ResultUI 오브젝트에 부착. UIDocument의 result-popup-root를 찾아 승/패 표시.
/// Inspector에서 텍스트를 직접 수정 가능합니다.
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class ResultPanelManager : MonoBehaviour
{
    [Header("Texts (Optional Override)")]
    [SerializeField] private string winTitleText     = "승리";
    [SerializeField] private string winSubtitleText  = "기지를 지켜냈습니다!";
    [SerializeField] private string loseTitleText    = "패배";
    [SerializeField] private string loseSubtitleText = "기지가 파괴되었습니다...";

    private VisualElement root;
    private Label   titleLabel;
    private Label   descriptionLabel;
    private Button  retryButton;
    private Button  titleButton;
    private Button  continueButton;
    private Button  closeButton;

    private void Awake()
    {
        InitRoot();
    }

    private void InitRoot()
    {
        if (root != null) return;

        UIDocument doc = GetComponent<UIDocument>();
        if (doc == null)
        {
            Debug.LogError("[ResultPanelManager] UIDocument 컴포넌트가 없습니다.", this);
            return;
        }

        // rootVisualElement는 Start 이후 안정적으로 접근 가능
        // Awake에서 null이면 Start에서 재시도
        VisualElement docRoot = doc.rootVisualElement;
        if (docRoot == null) return;

        root = docRoot.Q<VisualElement>("result-popup-root");
        if (root == null)
        {
            Debug.LogError("[ResultPanelManager] 'result-popup-root' 요소를 찾지 못했습니다. ResultPopup.uxml을 확인하세요.", this);
            return;
        }

        titleLabel       = root.Q<Label>("result-title");
        descriptionLabel = root.Q<Label>("result-description");
        retryButton      = root.Q<Button>("retry-button");
        titleButton      = root.Q<Button>("title-button");
        continueButton   = root.Q<Button>("continue-button");
        closeButton      = root.Q<Button>("close-button");

        // 버튼 이벤트 연결
        retryButton?.RegisterCallback<ClickEvent>(_ =>
        {
            Hide();
            var mgr = BattleGameManager.Instance;
            if (mgr != null) mgr.Restart();
        });

        titleButton?.RegisterCallback<ClickEvent>(_ =>
        {
            Hide();
            var mgr = BattleGameManager.Instance;
            if (mgr != null) mgr.BackToTitle();
        });

        continueButton?.RegisterCallback<ClickEvent>(_ => Hide());
        closeButton?.RegisterCallback<ClickEvent>(_ => Hide());

        // 초기 숨김
        Hide();
    }

    private void Start()
    {
        // Awake에서 rootVisualElement가 아직 없던 경우 재시도
        if (root == null)
            InitRoot();
    }

    public void ShowWin()
    {
        Show(winTitleText, winSubtitleText);
    }

    public void ShowLose()
    {
        Show(loseTitleText, loseSubtitleText);
    }

    private void Show(string title, string subtitle)
    {
        if (root == null)
        {
            InitRoot();
            if (root == null)
            {
                Debug.LogWarning("[ResultPanelManager] root가 설정되지 않아 결과 패널을 표시할 수 없습니다.", this);
                return;
            }
        }

        if (titleLabel != null)       titleLabel.text       = title;
        if (descriptionLabel != null) descriptionLabel.text  = subtitle;

        root.style.display = DisplayStyle.Flex;
        root.style.opacity = 1f;
    }

    private void Hide()
    {
        if (root == null) return;
        root.style.display = DisplayStyle.None;
        root.style.opacity = 0f;
    }
}
