using UnityEngine;
using UnityEngine.Playables;

/// <summary>
/// 상태 진입 시 부모에서 PlayableDirector를 찾아 재생.
/// </summary>
public class PlayTimelineBehaviour : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        PlayableDirector director = animator.GetComponentInParent<PlayableDirector>();

        if (director != null)
        {
            director.Play();
        }
        else
        {
            Debug.LogError($"[PlayTimelineBehaviour] PlayableDirector not found from parent of {animator.name}");
        }
    }
}
