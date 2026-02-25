using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// 승리/패배 결과를 UI Toolkit 풀스크린 패널로 표시하는 매니저.
/// 게임 씬 위에만 덮어 씌우며 씬 전환은 하지 않습니다.
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class ResultPanelManager : MonoBehaviour
{
    [Header("Texts (Optional Override)")]
    [SerializeField] private string winTitleText = "승리";
    [SerializeField] private string winSubtitleText = "기지를 지켜냈습니다!";
    [SerializeField] private string loseTitleText = "패배";
    [SerializeField] private string loseSubtitleText = "기지가 파괴되었습니다...";

    private UIDocument _uiDocument;
    private VisualElement _root;
    private Label _titleLabel;
    private Label _subtitleLabel;
    private Button _restartButton;
    private Button _titleButton;

    private void Awake()
    {
        _uiDocument = GetComponent<UIDocument>();
    }

    private void OnEnable()
    {
        SetupUI();
        Hide();
    }

    private void OnDisable()
    {
        TeardownUI();
    }

    private void SetupUI()
    {
        if (_uiDocument == null)
        {
            Debug.LogWarning("[ResultPanelManager] UIDocument 가 설정되지 않았습니다.", this);
            return;
        }

        _root = _uiDocument.rootVisualElement?.Q<VisualElement>("result-root");
        if (_root == null)
        {
            Debug.LogWarning("[ResultPanelManager] result-root 요소를 찾을 수 없습니다.", this);
            return;
        }

        _titleLabel = _root.Q<Label>("result-title");
        _subtitleLabel = _root.Q<Label>("result-subtitle");
        _restartButton = _root.Q<Button>("restart-button");
        _titleButton = _root.Q<Button>("title-button");

        if (_restartButton != null)
        {
            _restartButton.clicked += HandleRestartClicked;
        }

        if (_titleButton != null)
        {
            _titleButton.clicked += HandleTitleClicked;
        }
    }

    private void TeardownUI()
    {
        if (_restartButton != null)
        {
            _restartButton.clicked -= HandleRestartClicked;
        }

        if (_titleButton != null)
        {
            _titleButton.clicked -= HandleTitleClicked;
        }
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
        if (_root == null)
        {
            Debug.LogWarning("[ResultPanelManager] root 가 설정되지 않아 결과 패널을 표시할 수 없습니다.", this);
            return;
        }

        if (_titleLabel != null)
        {
            _titleLabel.text = title;
        }

        if (_subtitleLabel != null)
        {
            _subtitleLabel.text = subtitle;
        }

        _root.style.display = DisplayStyle.Flex;
    }

    public void Hide()
    {
        if (_root == null) return;
        _root.style.display = DisplayStyle.None;
    }

    private void HandleRestartClicked()
    {
        Hide();

        var manager = BattleGameManager.Instance;
        if (manager != null)
        {
            manager.Restart();
        }
    }

    private void HandleTitleClicked()
    {
        Hide();

        var manager = BattleGameManager.Instance;
        if (manager != null)
        {
            manager.BackToTitle();
        }
    }
}

