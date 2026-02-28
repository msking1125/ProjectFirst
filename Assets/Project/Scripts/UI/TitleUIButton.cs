using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
    
    // Fallback animation state if DOTween is not used
    private bool isHovered = false;
    private bool isPressed = false;
    private float currentScaleVelocity;
    private Color currentColorVelocity;
    private Color currentColor;

    private void Awake()
    {
        button = GetComponent<Button>();
        targetImage = button.targetGraphic as Image;
        
        originalScale = transform.localScale;
        
        if (targetImage != null)
        {
            originalColor = targetImage.color;
            currentColor = originalColor;
        }
    }

    private void OnDisable()
    {
        transform.localScale = originalScale;
        if (targetImage != null)
        {
            targetImage.color = originalColor;
            currentColor = originalColor;
        }
        isHovered = false;
        isPressed = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!button.interactable) return;
        isHovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!button.interactable) return;
        isHovered = false;
        isPressed = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!button.interactable) return;
        isPressed = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!button.interactable) return;
        isPressed = false;
    }

    private void Update()
    {
        if (!button.interactable) return;

        // Determine target scale and color
        float targetScaleMult = 1f;
        Color targetColor = originalColor;

        if (isPressed)
        {
            targetScaleMult = 0.95f; // shrink slightly on press
            targetColor = originalColor * 0.8f; // darken slightly
            targetColor.a = originalColor.a;
        }
        else if (isHovered)
        {
            targetScaleMult = hoverScale;
            // Add brightness
            targetColor = originalColor + hoverColorOverlay;
            targetColor.r = Mathf.Clamp01(targetColor.r);
            targetColor.g = Mathf.Clamp01(targetColor.g);
            targetColor.b = Mathf.Clamp01(targetColor.b);
            targetColor.a = originalColor.a;
        }

        Vector3 targetScale = originalScale * targetScaleMult;

        // Smoothly interpolate
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * (1f / animationDuration));
        
        if (targetImage != null)
        {
            currentColor = Color.Lerp(currentColor, targetColor, Time.unscaledDeltaTime * (1f / animationDuration));
            targetImage.color = currentColor;
        }
    }
}
