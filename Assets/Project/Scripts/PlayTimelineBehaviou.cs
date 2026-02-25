using UnityEngine;
using UnityEngine.Playables;

public class PlayTimelineBehaviour : StateMachineBehaviour
{
    // 노드에 진입할 때 실행
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 1. 부모 오브젝트에서 PlayableDirector 찾기
        PlayableDirector director = animator.GetComponentInParent<PlayableDirector>();

        if (director != null)
        {
            Debug.Log("타임라인 실행 명령 전달됨: " + director.name);
            director.Play();
        }
        else
        {
            Debug.LogError("PlayableDirector를 부모 오브젝트에서 찾을 수 없습니다!");
        }
    }
}