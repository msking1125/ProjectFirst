using System;
using UnityEngine;
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

    private Action _onContinue;
    private Action _onPrestige;
    private Action _onRetry;
    private Action _onTitle;
    private Action _onClose;

    public VisualElement Root => root;

    public ResultPopup(VisualElement rootElement)
    {
        root = rootElement;

        titleLabel = root.Q<Label>("result-title");
        descriptionLabel = root.Q<Label>("result-description");
        waveLabel = root.Q<Label>("wave-label");
        goldLabel = root.Q<Label>("gold-label");
        prestigeLabel = root.Q<Label>("prestige-label");

        continueButton = FindButton("continue-button", "계속하기");
        prestigeButton = FindButton("prestige-button", "프레스티지");
        retryButton = FindButton("retry-button", "다시 도전");
        titleButton = FindButton("title-button", "타이틀로");
        closeButton = FindButton("close-button", "✕");
    }

    private Button FindButton(string name, string fallbackText)
    {
        Button button = root.Q<Button>(name);
        if (button != null)
            return button;

        foreach (Button candidate in root.Query<Button>().ToList())
        {
            if (candidate == null)
                continue;

            if (candidate.text == fallbackText)
            {
                Debug.LogWarning($"[ResultPopup] '{name}' 버튼을 찾지 못해 텍스트('{fallbackText}') 기반으로 바인딩했습니다.");
                return candidate;
            }
        }

        Debug.LogError($"[ResultPopup] '{name}' 버튼을 찾지 못했습니다. UXML name 연결을 확인하세요.");
        return null;
    }

    public void ConfigureWin(int wave, int gold, int prestige)
    {
        if (titleLabel != null)
            titleLabel.text = "승리!";

        if (descriptionLabel != null)
            descriptionLabel.text = "기지를 지켜냈습니다.";

        if (waveLabel != null)
            waveLabel.text = $"도달 웨이브: {wave}";

        if (goldLabel != null)
            goldLabel.text = $"획득 골드: {gold}";

        if (prestigeLabel != null)
        {
            prestigeLabel.text = $"프레스티지 포인트: {prestige}";
            prestigeLabel.style.display = DisplayStyle.Flex;
        }

        SetButtonVisible(continueButton, true);
        SetButtonVisible(prestigeButton, true);
        SetButtonVisible(retryButton, false);
        SetButtonVisible(titleButton, false);
    }

    public void ConfigureLose(int wave, int gold)
    {
        if (titleLabel != null)
            titleLabel.text = "패배...";

        if (descriptionLabel != null)
            descriptionLabel.text = "기지가 파괴되었습니다.";

        if (waveLabel != null)
            waveLabel.text = $"생존 웨이브: {wave}";

        if (goldLabel != null)
            goldLabel.text = $"획득 골드: {gold}";

        if (prestigeLabel != null)
            prestigeLabel.style.display = DisplayStyle.None;

        SetButtonVisible(continueButton, false);
        SetButtonVisible(prestigeButton, false);
        SetButtonVisible(retryButton, true);
        SetButtonVisible(titleButton, true);
    }

    public void SetVisible(bool visible)
    {
        if (root == null)
            return;

        root.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        root.pickingMode = visible ? PickingMode.Position : PickingMode.Ignore;
    }

    public void SetOpacity(float opacity)
    {
        if (root == null)
            return;

        root.style.opacity = Mathf.Clamp01(opacity);
    }

    public void BindButtons(
        Action onContinue,
        Action onPrestige,
        Action onRetry,
        Action onTitle,
        Action onClose)
    {
        _onContinue = onContinue;
        _onPrestige = onPrestige;
        _onRetry = onRetry;
        _onTitle = onTitle;
        _onClose = onClose;

        RebindButton(continueButton, HandleContinueClicked, nameof(continueButton));
        RebindButton(prestigeButton, HandlePrestigeClicked, nameof(prestigeButton));
        RebindButton(retryButton, HandleRetryClicked, nameof(retryButton));
        RebindButton(titleButton, HandleTitleClicked, nameof(titleButton));
        RebindButton(closeButton, HandleCloseClicked, nameof(closeButton));
    }

    private void RebindButton(Button button, Action handler, string debugName)
    {
        if (button == null)
        {
            Debug.LogError($"[ResultPopup] {debugName} 바인딩 실패: 버튼 참조가 null 입니다.");
            return;
        }

        button.clicked -= handler;
        button.clicked += handler;
    }

    private void HandleContinueClicked() => _onContinue?.Invoke();
    private void HandlePrestigeClicked() => _onPrestige?.Invoke();
    private void HandleRetryClicked() => _onRetry?.Invoke();
    private void HandleTitleClicked() => _onTitle?.Invoke();
    private void HandleCloseClicked() => _onClose?.Invoke();

    private static void SetButtonVisible(Button button, bool visible)
    {
        if (button == null)
            return;

        button.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        button.SetEnabled(visible);
    }
}
