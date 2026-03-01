using UnityEngine;

// 이 스크립트는 MonoBehaviour가 아니라 StateMachineBehaviour를 상속받습니다!
public class SkillEffectTrigger : StateMachineBehaviour
{
    [Tooltip("스킬 애니메이션이 시작되고 몇 % 시점에 발사할지 결정 (0 = 시작 즉시, 0.5 = 중간)")]
    [Range(0f, 1f)] public float triggerTime = 0f;

    private bool hasFired = false;

    // 상태(애니메이션)에 진입할 때 1회 실행
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        hasFired = false;

        // 0초 즉시 발동 설정이라면 바로 발사
        if (triggerTime <= 0f)
        {
            Fire(animator);
        }
    }

    // 상태(애니메이션)가 재생되는 동안 매 프레임 실행
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 설정한 시간이 지났고, 아직 안 쐈다면 발사
        if (!hasFired && triggerTime > 0f && stateInfo.normalizedTime >= triggerTime)
        {
            Fire(animator);
        }
    }

    private void Fire(Animator animator)
    {
        // Firerailgun 스크립트를 찾아서 스킬 발사 함수 실행!
        Firerailgun railgun = animator.GetComponent<Firerailgun>();
        if (railgun != null)
        {
            railgun.FireSkillRailgun();
        }
        hasFired = true;
    }
}
