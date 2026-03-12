using UnityEngine;
using Project;

// Animation state event bridge that fires the railgun skill once at a configured normalized time.
public class SkillEffectTrigger : StateMachineBehaviour
{
    [Tooltip("Normalized animation time that triggers the skill effect. 0 fires immediately on state enter.")]
    [Range(0f, 1f)] public float triggerTime = 0f;

    private bool hasFired;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        hasFired = false;

        if (triggerTime <= 0f)
        {
            Fire(animator);
        }
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!hasFired && triggerTime > 0f && stateInfo.normalizedTime >= triggerTime)
        {
            Fire(animator);
        }
    }

    private void Fire(Animator animator)
    {
        Firerailgun railgun = animator.GetComponent<Firerailgun>();
        if (railgun != null)
        {
            railgun.FireSkillRailgun();
        }

        hasFired = true;
    }
}