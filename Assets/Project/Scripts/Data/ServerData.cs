using System;
using UnityEngine;

namespace ProjectFirst.Data
{
    /// <summary>
    /// ?쒕쾭 ????ぉ???곗씠?? ServerListSO??rows???깅줉?⑸땲??
    /// </summary>
    [Serializable]
    public class ServerData
    {
        public string serverId;
        public string displayName;
        public int maxPlayers = 3000;
        public int currentPlayers;
        /// <summary>?묒냽??鍮꾩쑉 (0~1)</summary>
        public float LoadRatio => maxPlayers > 0 ? Mathf.Clamp01((float)currentPlayers / maxPlayers) : 0f;
        /// <summary>?묒냽??鍮꾩쑉???곕Ⅸ ?쇱옟??臾몄옄??/summary>
        public string CongestionLabel =>
            LoadRatio < 0.5f ? "?먰솢" :
            LoadRatio < 0.85f ? "蹂댄넻" : "?쇱옟";
    }
}

