using DG.Tweening;
using TMPro;
using UnityEngine;

public class DamageText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI valueText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float moveY = 1.5f;
    [SerializeField] private float duration = 0.7f;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color critColor = Color.red;

    public void Init(int damage, bool isCrit)
    {
        if (valueText == null)
        {
            valueText = GetComponentInChildren<TextMeshProUGUI>();
        }

        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        if (valueText != null)
        {
            valueText.text = damage.ToString();
            valueText.color = isCrit ? critColor : normalColor;
            valueText.transform.localScale = Vector3.one;
            if (isCrit)
            {
                valueText.transform.DOScale(1.35f, duration * 0.4f).SetEase(Ease.OutBack);
            }
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.DOFade(0f, duration);
        }

        transform.DOMoveY(transform.position.y + moveY, duration).SetEase(Ease.OutQuad)
            .OnComplete(() => Destroy(gameObject));
    }
}
