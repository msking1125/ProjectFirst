using UnityEngine;

namespace Project
{
    /// <summary>
    /// 소문자 animator parameter/tag 규칙용 공용 브리지.
    /// </summary>
    public class AgentAnimatorBridge : MonoBehaviour
    {
        [SerializeField] private Animator animator;

        private static readonly int AttackHash = Animator.StringToHash("attack");
        private static readonly int ActiveSkillHash = Animator.StringToHash("activeskill");
        private static readonly int UltimateHash = Animator.StringToHash("ultimate");

        private void Awake()
        {
            if (animator == null)
                animator = GetComponentInChildren<Animator>(true);
        }

        public Animator CachedAnimator => animator;

        public void TriggerAttack()
        {
            if (animator == null) return;
            animator.SetTrigger(AttackHash);
        }

        public void TriggerActiveSkill()
        {
            if (animator == null) return;
            animator.SetTrigger(ActiveSkillHash);
        }

        public void TriggerUltimate()
        {
            if (animator == null) return;
            animator.SetTrigger(UltimateHash);
        }

        public bool IsInSkillState()
        {
            if (animator == null) return false;
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            return stateInfo.IsTag("skill");
        }

        public bool IsInAttackState()
        {
            if (animator == null) return false;
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            return stateInfo.IsTag("attack");
        }

        public bool IsBusy()
        {
            return IsInAttackState() || IsInSkillState();
        }
    }
}
