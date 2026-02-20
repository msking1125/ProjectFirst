using TMPro;
using UnityEngine;

public class BaseHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private TMP_Text hpText;

    private float currentHealth;
    private BattleGameManager gameManager;

    private void Awake()
    {
        currentHealth = Mathf.Max(1f, maxHealth);
        UpdateUI();
    }

    public void BindGameManager(BattleGameManager manager)
    {
        gameManager = manager;
    }

    public void TakeDamage(float damage)
    {
        if (currentHealth <= 0f)
        {
            return;
        }

        currentHealth -= Mathf.Max(0f, damage);
        currentHealth = Mathf.Max(0f, currentHealth);
        UpdateUI();

        if (currentHealth <= 0f)
        {
            if (gameManager != null)
            {
                gameManager.HandleDefeat();
            }
            else
            {
                BattleGameManager.ReportBaseDestroyed();
            }
        }
    }

    private void UpdateUI()
    {
        if (hpText != null)
        {
            hpText.text = $"BaseHP: {Mathf.CeilToInt(currentHealth)}";
        }
    }
}
