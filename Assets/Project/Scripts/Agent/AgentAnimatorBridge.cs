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
            animator = ResolveAnimator(animator);
        }

        public Animator CachedAnimator => ResolveAnimator(animator);

        public void TriggerAttack()
        {
            animator = ResolveAnimator(animator);
            if (animator == null) return;
            animator.SetTrigger(AttackHash);
        }

        public void TriggerActiveSkill()
        {
            animator = ResolveAnimator(animator);
            if (animator == null) return;
            animator.SetTrigger(ActiveSkillHash);
        }

        public void TriggerUltimate()
        {
            animator = ResolveAnimator(animator);
            if (animator == null) return;
            animator.SetTrigger(UltimateHash);
        }

        public bool IsInSkillState()
        {
            animator = ResolveAnimator(animator);
            if (animator == null) return false;
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            return stateInfo.IsTag("skill");
        }

        public bool IsInAttackState()
        {
            animator = ResolveAnimator(animator);
            if (animator == null) return false;
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            return stateInfo.IsTag("attack");
        }

        public bool IsBusy()
        {
            return IsInAttackState() || IsInSkillState();
        }

        private Animator ResolveAnimator(Animator preferred)
        {
            if (HasController(preferred))
                return preferred;

            Animator[] animators = GetComponentsInChildren<Animator>(true);
            foreach (Animator candidate in animators)
            {
                if (HasController(candidate))
                    return candidate;
            }

            return preferred != null ? preferred : (animators.Length > 0 ? animators[0] : null);
        }

        private static bool HasController(Animator candidate)
        {
            return candidate != null && candidate.runtimeAnimatorController != null;
        }
    }
}
