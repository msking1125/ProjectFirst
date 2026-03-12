using UnityEngine;
using UnityEngine.Playables;

public class PlayTimelineBehaviour : StateMachineBehaviour
{
    // ?몃뱶??吏꾩엯?????ㅽ뻾
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 1. 遺紐??ㅻ툕?앺듃?먯꽌 PlayableDirector 李얘린
        PlayableDirector director = animator.GetComponentInParent<PlayableDirector>();

        if (director != null)
        {
            Debug.Log("??꾨씪???ㅽ뻾 紐낅졊 ?꾨떖?? " + director.name);
            director.Play();
        }
        else
        {
            Debug.LogError("PlayableDirector瑜?遺紐??ㅻ툕?앺듃?먯꽌 李얠쓣 ???놁뒿?덈떎!");
        }
    }
}
