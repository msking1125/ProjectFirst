using UnityEngine;

namespace ProjectFirst.Data
{
    /// <summary>
    /// 방치 보상 수치 설정 ScriptableObject.
    /// 생성: Create → Soul Ark/Idle Reward Config
    /// 권장 경로: Assets/Project/Data/IdleRewardConfig.asset
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Idle Reward Config", fileName = "IdleRewardConfig")]
    public class IdleRewardConfig : ScriptableObject
    {
        [Header("시간당 보상 (1시간 기준)")]
        [Tooltip("1시간당 지급 골드")]
        public int goldPerHour = 100;

        [Tooltip("1시간당 지급 티켓")]
        public int ticketPerHour = 0;

        [Tooltip("1시간당 지급 다이아")]
        public int diamondPerHour = 0;

        [Header("제한")]
        [Tooltip("보상이 쌓이는 최대 오프라인 시간 (시간 단위). 이 이상은 보상이 증가하지 않습니다.")]
        [Min(1f)]
        public float maxOfflineHours = 12f;

        [Tooltip("팝업 자동 표시 최소 경과 시간 (초). 이 시간 미만이면 자동 팝업이 뜨지 않습니다.")]
        [Min(0f)]
        public float minElapsedSecondsForPopup = 60f;

        [Tooltip("보상 연출 재생 시간 (초)")]
        [Min(0f)]
        public float animDuration = 1.5f;
    }
}
