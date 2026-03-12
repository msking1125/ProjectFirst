using UnityEngine;

namespace Project
{
    public class AgentAnimationEvents : MonoBehaviour
    {
        [SerializeField] private Agent agent;

        private void Awake()
        {
            if (agent == null)
                agent = GetComponentInParent<Agent>();
        }

        public void ApplyAttackHit_01()
        {
            agent?.ApplyAttackHit_01();
        }

        public void ApplyAttackHit_02()
        {
            agent?.ApplyAttackHit_02();
        }

        public void EndAttackCombo()
        {
            agent?.EndAttackCombo();
        }

        public void FireNormalAttackEvent()
        {
            agent?.FireNormalAttack();
        }
    }
}
