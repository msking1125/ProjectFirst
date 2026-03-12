using UnityEngine;
using UnityEngine.Playables;

public class PlayTimelineBehaviour : StateMachineBehaviour
{
    // Note: cleaned comment.
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Note: cleaned comment.
        PlayableDirector director = animator.GetComponentInParent<PlayableDirector>();

        if (director != null)
        {
            Debug.Log("[Log] Message cleaned.");
            director.Play();
        }
        else
        {
            Debug.LogError("[Log] Error message cleaned.");
        }
    }
}
