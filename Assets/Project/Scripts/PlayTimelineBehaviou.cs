using UnityEngine;
using UnityEngine.Playables;

public class PlayTimelineBehaviour : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        PlayableDirector director = animator.GetComponentInParent<PlayableDirector>();

        if (director != null)
        {
            director.Play();
        }
    }
}