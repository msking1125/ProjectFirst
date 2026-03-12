using UnityEngine;
using UnityEngine.Playables;

public class PlayTimelineBehaviour : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        PlayableDirector director = animator.GetComponentInParent<PlayableDirector>();

        if (director != null)
        {
            Debug.Log("[Log] 상태가 갱신되었습니다.");
            director.Play();
        }
        else
        {
            Debug.LogError("[Log] 오류가 발생했습니다.");
        }
    }
}

