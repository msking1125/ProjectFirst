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

    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;
    }

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

        // 텍스트 위치 무작위 흔들림 또는 오프셋 초기화 보장
        if (valueText != null && valueText.rectTransform != null)
        {
            valueText.rectTransform.anchoredPosition = Vector2.zero;
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

        // 빌보드 처리 (항상 카메라를 바라보도록)
        if (cam != null)
        {
            transform.forward = cam.transform.forward;
        }

        transform.DOMoveY(transform.position.y + moveY, duration).SetEase(Ease.OutQuad)
            .OnComplete(() => Destroy(gameObject));
    }

    private void LateUpdate()
    {
        // 텍스트가 위로 올라가는 동안에도 빌보드 유지
        if (cam != null)
        {
            transform.forward = cam.transform.forward;
        }
    }
}
