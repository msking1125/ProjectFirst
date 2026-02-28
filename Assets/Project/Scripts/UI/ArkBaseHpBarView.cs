using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ArkBaseHpBarView : MonoBehaviour
{
    [Header("Editor 연결 체크리스트")]
    // 1) Canvas 하위 UI 오브젝트에 ArkBaseHpBarView를 추가합니다.
    // 2) fillImage에는 Image Type이 Filled인 게이지 이미지를 연결합니다.
    // 3) hpText에는 TMP_Text를 연결합니다.
    // 4) 가능하면 target에 Ark_Base의 BaseHealth를 직접 연결합니다(미연결 시 자동 탐색).
    [SerializeField] private BaseHealth target;
    [SerializeField] private Image fillImage;
    [SerializeField] private TMP_Text hpText;

    public BaseHealth Target => target;

    private void Awake()
    {
        ResolveTargetIfNeeded();
    }

    private void OnEnable()
    {
        SubscribeTarget();
        RefreshFromTarget();
    }

    private void OnDisable()
    {
        UnsubscribeTarget();
    }

    public void SetTarget(BaseHealth baseHealth)
    {
        if (target == baseHealth)
        {
            RefreshFromTarget();
            return;
        }

        UnsubscribeTarget();
        target = baseHealth;
        SubscribeTarget();
        RefreshFromTarget();
    }

    private void ResolveTargetIfNeeded()
    {
        if (target != null)
        {
            return;
        }

        GameObject arkObject = GameObject.Find("Ark_Base");
        if (arkObject != null)
        {
            target = arkObject.GetComponent<BaseHealth>();
        }

        if (target == null)
        {
#if UNITY_2022_2_OR_NEWER
            target = FindFirstObjectByType<BaseHealth>();
#else
            target = FindObjectOfType<BaseHealth>();
#endif
        }

        if (target == null)
        {
            Debug.LogWarning("[ArkBaseHpBarView] target(BaseHealth) is not assigned and auto-resolve failed.", this);
            return;
        }

        Debug.LogWarning($"[ArkBaseHpBarView] target was empty. Auto-assigned to '{target.gameObject.name}'.", this);
    }

    private void SubscribeTarget()
    {
        ResolveTargetIfNeeded();

        if (target == null)
        {
            return;
        }

        target.OnHealthChanged -= HandleHealthChanged;
        target.OnHealthChanged += HandleHealthChanged;
    }

    private void UnsubscribeTarget()
    {
        if (target == null)
        {
            return;
        }

        target.OnHealthChanged -= HandleHealthChanged;
    }

    private void RefreshFromTarget()
    {
        if (target == null)
        {
            return;
        }

        HandleHealthChanged(target.CurrentHealth, target.MaxHealth);
    }

    private void HandleHealthChanged(int current, int max)
    {
        int safeMax = Mathf.Max(1, max);

        if (fillImage != null)
        {
            fillImage.fillAmount = current / (float)safeMax;
        }

        if (hpText != null)
        {
            hpText.text = $"BaseHP: {current}/{safeMax}";
        }
    }
}
