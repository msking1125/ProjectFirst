using UnityEngine;
using Project;

/// <summary>
/// 상태 진입 후 지정된 normalized time에 한 번만 스킬 발사.
/// </summary>
public class SkillEffectTrigger : StateMachineBehaviour
{
    [Tooltip("0이면 상태 진입 직후 발사, 0~1이면 해당 normalized time에서 발사")]
    [Range(0f, 1f)]
    public float triggerTime = 0f;

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
        if (hasFired) return;

        if (triggerTime > 0f && stateInfo.normalizedTime >= triggerTime)
        {
            Fire(animator);
        }
    }

    private void Fire(Animator animator)
    {
        ProjectileShooter shooter = animator.GetComponent<ProjectileShooter>();
        if (shooter == null)
            shooter = animator.GetComponentInChildren<ProjectileShooter>(true);

        if (shooter != null)
        {
            shooter.FireSkillAttack();
        }
        else
        {
            Debug.LogWarning($"[SkillEffectTrigger] ProjectileShooter not found on {animator.name}");
        }

        hasFired = true;
    }
}

