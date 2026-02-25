using UnityEngine.UIElements;

/// <summary>
/// ResultPopup UXML 뷰를 캡슐화하는 뷰 클래스.
/// 순수 UI Toolkit 요소 참조와 레이아웃 전환만 담당합니다.
/// </summary>
public class ResultPopup
{
    public enum Mode
    {
        Win,
        Lose
    }

    private readonly VisualElement root;
    private readonly Label titleLabel;
    private readonly Label descriptionLabel;
    private readonly Label waveLabel;
    private readonly Label goldLabel;
    private readonly Label prestigeLabel;

    private readonly Button continueButton;
    private readonly Button prestigeButton;
    private readonly Button retryButton;
    private readonly Button titleButton;
    private readonly Button closeButton;

    public VisualElement Root => root;

    public ResultPopup(VisualElement rootElement)
    {
        root = rootElement;

        titleLabel = root.Q<Label>("result-title");
        descriptionLabel = root.Q<Label>("result-description");
        waveLabel = root.Q<Label>("wave-label");
        goldLabel = root.Q<Label>("gold-label");
        prestigeLabel = root.Q<Label>("prestige-label");

        continueButton = root.Q<Button>("continue-button");
        prestigeButton = root.Q<Button>("prestige-button");
        retryButton = root.Q<Button>("retry-button");
        titleButton = root.Q<Button>("title-button");
        closeButton = root.Q<Button>("close-button");
    }

    public void ConfigureWin(int wave, int gold, int prestige)
    {
        if (titleLabel != null)
        {
            titleLabel.text = "승리!";
        }

        if (descriptionLabel != null)
        {
            descriptionLabel.text = "기지를 지켜냈습니다.";
        }

        if (waveLabel != null)
        {
            waveLabel.text = $"도달 웨이브: {wave}";
        }

        if (goldLabel != null)
        {
            goldLabel.text = $"획득 골드: {gold}";
        }

        if (prestigeLabel != null)
        {
            prestigeLabel.text = $"프레스티지 포인트: {prestige}";
            prestigeLabel.style.display = DisplayStyle.Flex;
        }

        if (continueButton != null)
        {
            continueButton.style.display = DisplayStyle.Flex;
        }

        if (prestigeButton != null)
        {
            prestigeButton.style.display = DisplayStyle.Flex;
        }

        if (retryButton != null)
        {
            retryButton.style.display = DisplayStyle.None;
        }

        if (titleButton != null)
        {
            titleButton.style.display = DisplayStyle.None;
        }
    }

    public void ConfigureLose(int wave, int gold)
    {
        if (titleLabel != null)
        {
            titleLabel.text = "패배...";
        }

        if (descriptionLabel != null)
        {
            descriptionLabel.text = "기지가 파괴되었습니다.";
        }

        if (waveLabel != null)
        {
            waveLabel.text = $"생존 웨이브: {wave}";
        }

        if (goldLabel != null)
        {
            goldLabel.text = $"획득 골드: {gold}";
        }

        if (prestigeLabel != null)
        {
            prestigeLabel.style.display = DisplayStyle.None;
        }

        if (continueButton != null)
        {
            continueButton.style.display = DisplayStyle.None;
        }

        if (prestigeButton != null)
        {
            prestigeButton.style.display = DisplayStyle.None;
        }

        if (retryButton != null)
        {
            retryButton.style.display = DisplayStyle.Flex;
        }

        if (titleButton != null)
        {
            titleButton.style.display = DisplayStyle.Flex;
        }
    }

    public void SetVisible(bool visible)
    {
        if (root == null) return;
        root.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    public void SetOpacity(float opacity)
    {
        if (root == null) return;
        root.style.opacity = opacity;
    }

    public void BindButtons(
        System.Action onContinue,
        System.Action onPrestige,
        System.Action onRetry,
        System.Action onTitle,
        System.Action onClose)
    {
        if (continueButton != null)
        {
            continueButton.clicked += () => onContinue?.Invoke();
        }

        if (prestigeButton != null)
        {
            prestigeButton.clicked += () => onPrestige?.Invoke();
        }

        if (retryButton != null)
        {
            retryButton.clicked += () => onRetry?.Invoke();
        }

        if (titleButton != null)
        {
            titleButton.clicked += () => onTitle?.Invoke();
        }

        if (closeButton != null)
        {
            closeButton.clicked += () => onClose?.Invoke();
        }
    }
}

