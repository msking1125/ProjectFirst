using System;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace ProjectFirst.Data
{
    /// <summary>
    /// 서버 한 항목의 데이터. ServerListSO의 rows에 등록합니다.
    /// </summary>
    [Serializable]
#if ODIN_INSPECTOR
    [HideLabel]
#endif
    public class ServerData
    {
#if ODIN_INSPECTOR
        [HorizontalGroup("기본", 0.5f)]
        [BoxGroup("기본/ID")]
        [LabelText("서버 ID")]
        [Tooltip("서버 고유 ID (서버 선택 시 전달됩니다)")]
#endif
        public string serverId;

#if ODIN_INSPECTOR
        [HorizontalGroup("기본", 0.5f)]
        [BoxGroup("기본/이름")]
        [LabelText("표시 이름")]
        [Tooltip("유저에게 보여줄 서버 이름")]
#endif
        public string displayName;

#if ODIN_INSPECTOR
        [HorizontalGroup("접속", 0.5f)]
        [BoxGroup("접속/최대")]
        [LabelText("최대 인원")]
        [ProgressBar(0, 5000)]
        [Tooltip("최대 수용 인원")]
#endif
        public int maxPlayers = 3000;

#if ODIN_INSPECTOR
        [HorizontalGroup("접속", 0.5f)]
        [BoxGroup("접속/현재")]
        [LabelText("현재 인원")]
        [ProgressBar(0, "maxPlayers", ColorGetter = "GetLoadColor")]
        [ReadOnly]
        [Tooltip("현재 접속 인원 (런타임에 API로 갱신 예정)")]
#endif
        public int currentPlayers;

#if ODIN_INSPECTOR
        [BoxGroup("상태")]
        [ShowInInspector, ReadOnly]
        [LabelText("접속률")]
        [ProgressBar(0, 1, ColorGetter = "GetLoadRatioColor")]
        [SuffixLabel("%", true)]
#endif
        /// <summary>접속자 비율 (0~1)</summary>
        public float LoadRatio => maxPlayers > 0 ? Mathf.Clamp01((float)currentPlayers / maxPlayers) : 0f;

#if ODIN_INSPECTOR
        [BoxGroup("상태")]
        [ShowInInspector, ReadOnly]
        [LabelText("혼잡도")]
        [GUIColor("GetCongestionColor")]
#endif
        /// <summary>접속자 비율에 따른 혼잡도 문자열</summary>
        public string CongestionLabel =>
            LoadRatio < 0.5f ? "원활" :
            LoadRatio < 0.85f ? "보통" : "혼잡";

#if ODIN_INSPECTOR
        private static Color GetLoadColor() => new Color(0.3f, 0.7f, 1f);
        private static Color GetLoadRatioColor() => new Color(1f, 0.6f, 0.2f);
        private Color GetCongestionColor()
        {
            if (LoadRatio < 0.5f) return new Color(0.3f, 0.8f, 0.3f); // 원활 - 녹색
            if (LoadRatio < 0.85f) return new Color(1f, 0.8f, 0.2f); // 보통 - 노랑
            return new Color(1f, 0.3f, 0.3f); // 혼잡 - 빨강
        }
#endif
    }
}
