using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace ProjectFirst.Data
{
    /// <summary>
    /// 방치 보상 수치 설정 ScriptableObject.
    /// 생성: Create → Soul Ark/Idle Reward Config
    /// 권장 경로: Assets/Project/Data/IdleRewardConfig.asset
    /// </summary>
#if ODIN_INSPECTOR
    [CreateAssetMenu(menuName = "Soul Ark/Idle Reward Config", fileName = "IdleRewardConfig")]
#else
    [CreateAssetMenu(menuName = "Game/Idle Reward Config", fileName = "IdleRewardConfig")]
#endif
    public class IdleRewardConfig : ScriptableObject
    {
#if ODIN_INSPECTOR
        [Title("시간당 보상 (1시간 기준)", TitleAlignment = TitleAlignments.Left)]
        [HorizontalGroup("시간당", 0.33f)]
        [BoxGroup("시간당/골드")]
        [LabelText("골드")]
        [ProgressBar(0, 1000)]
        [SuffixLabel("/시간", true)]
#endif
        [Header("시간당 보상 (1시간 기준)")]
        [Tooltip("1시간당 지급 골드")]
        public int goldPerHour = 100;

#if ODIN_INSPECTOR
        [HorizontalGroup("시간당", 0.33f)]
        [BoxGroup("시간당/티켓")]
        [LabelText("티켓")]
        [SuffixLabel("/시간", true)]
#endif
        [Tooltip("1시간당 지급 티켓")]
        public int ticketPerHour = 0;

#if ODIN_INSPECTOR
        [HorizontalGroup("시간당", 0.34f)]
        [BoxGroup("시간당/다이아")]
        [LabelText("다이아")]
        [SuffixLabel("/시간", true)]
        [GUIColor(0.8f, 0.4f, 0.9f)]
#endif
        [Tooltip("1시간당 지급 다이아")]
        public int diamondPerHour = 0;

#if ODIN_INSPECTOR
        [Title("제한", TitleAlignment = TitleAlignments.Left)]
        [HorizontalGroup("제한", 0.5f)]
        [BoxGroup("제한/시간")]
        [LabelText("최대 오프라인 시간")]
        [SuffixLabel("시간", true)]
#endif
        [Header("제한")]
        [Tooltip("보상이 쌓이는 최대 오프라인 시간 (시간 단위). 이 이상은 보상이 증가하지 않습니다.")]
        [Min(1f)]
        public float maxOfflineHours = 12f;

#if ODIN_INSPECTOR
        [HorizontalGroup("제한", 0.5f)]
        [BoxGroup("제한/팝업")]
        [LabelText("팝업 최소 시간")]
        [SuffixLabel("초", true)]
#endif
        [Tooltip("팝업 자동 표시 최소 경과 시간 (초). 이 시간 미만이면 자동 팝업이 뜨지 않습니다.")]
        [Min(0f)]
        public float minElapsedSecondsForPopup = 60f;

#if ODIN_INSPECTOR
        [BoxGroup("제한")]
        [LabelText("연출 시간")]
        [SuffixLabel("초", true)]
#endif
        [Tooltip("보상 연출 재생 시간 (초)")]
        [Min(0f)]
        public float animDuration = 1.5f;
    }
}
