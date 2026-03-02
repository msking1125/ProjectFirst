using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 타이틀 버튼: 에디터/실행/빌드 환경에서 모두 일관적으로 동작하도록 처리.
/// 일부 모바일/빌드 환경에서 Button.targetGraphic가 Image가 아닌 경우 등 버그 예방.
/// </summary>
[RequireComponent(typeof(Button))]
public class TitleUIButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Hover Settings")]
    [SerializeField] private float hoverScale = 1.05f;
    [SerializeField] private float animationDuration = 0.15f;
    [SerializeField] private Color hoverColorOverlay = new Color(1f, 1f, 1f, 0.2f); // Add brightness

    private Button button;
    private Image targetImage;
    private Vector3 originalScale;
    private Color originalColor;
    private bool hasOriginalColor = false;

    // Fallback animation state if DOTween is not used
    private bool isHovered = false;
    private bool isPressed = false;
    private Color currentColor;

    private void Awake()
    {
        button = GetComponent<Button>();

        // 보통 Button.targetGraphic이 Image지만, 환경 따라 NULL이거나 다른 타입일 수 있으므로 예외 처리
        targetImage = GetComponent<Image>();
        if (targetImage == null && button.targetGraphic is Image tgImg)
        {
            targetImage = tgImg;
        }

        originalScale = transform.localScale;

        if (targetImage != null)
        {
            originalColor = targetImage.color;
            currentColor = originalColor;
            hasOriginalColor = true;
        }
        else
        {
            hasOriginalColor = false;
        }
    }

    private void OnEnable()
    {
        // 빌드 환경에서 Enable 타이밍 재보장
        ResetState();
    }

    private void OnDisable()
    {
        ResetState();
    }

    private void ResetState()
    {
        // 스케일과 색상 원래대로
        transform.localScale = originalScale;
        isHovered = false;
        isPressed = false;
        if (targetImage != null && hasOriginalColor)
        {
            targetImage.color = originalColor;
            currentColor = originalColor;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!button || !button.interactable) return;
        isHovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!button || !button.interactable) return;
        isHovered = false;
        isPressed = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!button || !button.interactable) return;
        isPressed = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!button || !button.interactable) return;
        isPressed = false;
    }

    private void Update()
    {
        if (!button || !button.interactable)
        {
            // 비활성화 시 초기화 보장
            ResetState();
            return;
        }

        float targetScaleMult = 1f;
        Color targetColor = hasOriginalColor ? originalColor : Color.white;

        if (isPressed)
        {
            targetScaleMult = 0.95f;
            targetColor = targetColor * 0.8f;
            targetColor.a = (hasOriginalColor ? originalColor.a : targetColor.a);
        }
        else if (isHovered)
        {
            targetScaleMult = hoverScale;
            targetColor = targetColor + hoverColorOverlay;
            targetColor.r = Mathf.Clamp01(targetColor.r);
            targetColor.g = Mathf.Clamp01(targetColor.g);
            targetColor.b = Mathf.Clamp01(targetColor.b);
            targetColor.a = (hasOriginalColor ? originalColor.a : targetColor.a);
        }

        Vector3 targetScale = originalScale * targetScaleMult;

        // Lerp 스케일
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime / Mathf.Max(0.0001f, animationDuration));

        if (targetImage != null && hasOriginalColor)
        {
            currentColor = Color.Lerp(currentColor, targetColor, Time.unscaledDeltaTime / Mathf.Max(0.0001f, animationDuration));
            targetImage.color = currentColor;
        }
    }
}
