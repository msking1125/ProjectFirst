using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Project
{

#if ODIN_INSPECTOR
    [HideMonoScript]
#endif
    public class BaseHealth : MonoBehaviour
    {
#if ODIN_INSPECTOR
        [Title("체력 설정", TitleAlignment = TitleAlignments.Left)]
        [HorizontalGroup("체력", 0.5f)]
        [BoxGroup("체력/최대")]
        [LabelText("최대 체력")]
        [PropertyRange(10, 1000)]
#endif
        [SerializeField] private int maxHealth = 100;

#if ODIN_INSPECTOR
        [HorizontalGroup("체력", 0.5f)]
        [BoxGroup("체력/텍스트")]
        [LabelText("HP 텍스트")]
        [Tooltip("체력 표시 텍스트 컴포넌트")]
        [SceneObjectsOnly]
#endif
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
                hpText.text = $"HP: {CurrentHealth}/{MaxHealth}";
            }
        }
    }
}
