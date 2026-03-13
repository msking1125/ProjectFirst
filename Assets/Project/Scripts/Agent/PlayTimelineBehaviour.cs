using UnityEngine;
using UnityEngine.Playables;

namespace Project
{
    /// <summary>
    /// 궁극기 연출용 Timeline을 재생하는 StateMachineBehaviour.
    /// - ultimatecutscene 같은 시네마틱 상태에 붙여서 사용합니다.
    /// - 상태 진입 시 지정된 Timeline을 재생하고, 상태 종료 시 정지합니다.
    /// </summary>
    public class PlayTimelineBehaviour : StateMachineBehaviour
    {
        [Tooltip("재생할 Timeline (PlayableDirector). 씬 또는 프리팹 상의 Director를 Drag & Drop 하세요.")]
        public PlayableDirector timelineToPlay;

        [Tooltip("상태 진입 시 자동으로 Timeline 재생 시작")]
        public bool playOnEnter = true;

        [Tooltip("상태 종료 시 Timeline 정지")]
        public bool stopOnExit = true;

        private bool wasPlaying;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (!playOnEnter || timelineToPlay == null)
                return;

            // 이미 재생 중이면 처음부터 다시 시작
            if (timelineToPlay.state == PlayState.Playing)
                timelineToPlay.Stop();

            timelineToPlay.Play();
            wasPlaying = true;

            Debug.Log("[PlayTimelineBehaviour] Timeline 재생 시작");
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (!stopOnExit || timelineToPlay == null || !wasPlaying)
                return;

            timelineToPlay.Stop();
            wasPlaying = false;

            Debug.Log("[PlayTimelineBehaviour] Timeline 정지");
        }

        /// <summary>
        /// 코드에서 Timeline을 교체하고 싶을 때 사용.
        /// </summary>
        public void SetTimeline(PlayableDirector timeline)
        {
            timelineToPlay = timeline;
        }
    }
}

