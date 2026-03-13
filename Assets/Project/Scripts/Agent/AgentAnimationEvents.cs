using UnityEngine;

namespace Project
{
    /// <summary>
    /// 애니메이션 이벤트 수신기. Animator와 동일한 GameObject에 있어야 Unity가 이벤트를 전달합니다.
    /// </summary>
    public class AgentAnimationEvents : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Agent agent;
        [SerializeField] private ProjectileShooter projectileShooter;

        private void Awake()
        {
            CacheReferences();
        }

        private void CacheReferences()
        {
            if (agent == null)
                agent = GetComponentInParent<Agent>();
            if (projectileShooter == null)
            {
                projectileShooter = GetComponent<ProjectileShooter>();
                if (projectileShooter == null)
                    projectileShooter = GetComponentInParent<ProjectileShooter>();
            }
        }

        public void Bind(Agent boundAgent)
        {
            agent = boundAgent;
            if (projectileShooter == null)
            {
                projectileShooter = GetComponent<ProjectileShooter>();
                if (projectileShooter == null)
                    projectileShooter = GetComponentInParent<ProjectileShooter>();
            }
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

        /// <summary>
        /// attack_01, attack_02 클립에서 호출. 발사체 생성은 ProjectileShooter에 직접 위임 후 Agent에도 알림.
        /// </summary>
        public void FireNormalAttackEvent()
        {
            if (projectileShooter != null && projectileShooter.CanFireNormalAttack())
                projectileShooter.FireNormalAttack();
            else
                agent?.FireNormalAttack();
        }

        public void FireActiveSkillEvent()
        {
            if (projectileShooter != null && projectileShooter.CanFireSkillAttack())
                projectileShooter.FireSkillAttack();
            else
                agent?.TriggerActiveSkillAnimation();
        }

        public void ApplyActiveSkillHit()
        {
            agent?.ApplyActiveSkillHit();
        }

        public void ApplyUltimateHit()
        {
            agent?.ApplyUltimateHit();
        }
    }
}
