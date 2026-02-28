using System;
using TMPro;
using UnityEngine;

public class BaseHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private TMP_Text hpText;

    private int currentHealth;
    private BattleGameManager gameManager;

    public event Action<int, int> OnHealthChanged;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => Mathf.Max(1, maxHealth);

    private void Awake()
    {
        currentHealth = MaxHealth;
        NotifyHealthChanged();
    }

    public void BindGameManager(BattleGameManager manager)
    {
        gameManager = manager;
    }

    public void TakeDamage(float damage)
    {
        if (currentHealth <= 0)
        {
            return;
        }

        int appliedDamage = Mathf.CeilToInt(Mathf.Max(0f, damage));
        currentHealth = Mathf.Max(0, currentHealth - appliedDamage);
        NotifyHealthChanged();

        if (currentHealth <= 0)
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

    private void NotifyHealthChanged()
    {
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);

        if (hpText != null)
        {
            hpText.text = $"BaseHP: {CurrentHealth}/{MaxHealth}";
        }
    }
}
