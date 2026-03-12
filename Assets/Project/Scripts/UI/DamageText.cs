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

        // ?띿뒪???꾩튂 臾댁옉???붾뱾由??먮뒗 ?ㅽ봽??珥덇린??蹂댁옣
        if (valueText != null && valueText.rectTransform != null)
        {
            valueText.rectTransform.anchoredPosition = Vector2.zero;
        }

        if (valueText != null)
        {
            // ?꾩뿭 湲곕낯 ?고듃(TMP Settings)媛 ?덈떎硫??먮룞?쇰줈 ?곸슜?⑸땲??
            if (TMP_Settings.defaultFontAsset != null)
            {
                valueText.font = TMP_Settings.defaultFontAsset;
            }

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

        // 鍮뚮낫??泥섎━ (??긽 移대찓?쇰? 諛붾씪蹂대룄濡?
        if (cam != null)
        {
            transform.forward = cam.transform.forward;
        }

        transform.DOMoveY(transform.position.y + moveY, duration).SetEase(Ease.OutQuad)
            .OnComplete(() => Destroy(gameObject));
    }

    private void LateUpdate()
    {
        // ?띿뒪?멸? ?꾨줈 ?щ씪媛???숈븞?먮룄 鍮뚮낫???좎?
        if (cam != null)
        {
            transform.forward = cam.transform.forward;
        }
    }
}
