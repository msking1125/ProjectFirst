using System;
using UnityEngine;

namespace ProjectFirst.Data
{
    /// <summary>
    /// Documentation cleaned.
    /// </summary>
    [Serializable]
    public class ServerData
    {
        public string serverId;
        public string displayName;
        public int maxPlayers = 3000;
        public int currentPlayers;
        /// Documentation cleaned.
        public float LoadRatio => maxPlayers > 0 ? Mathf.Clamp01((float)currentPlayers / maxPlayers) : 0f;
        /// Documentation cleaned.
        public string CongestionLabel =>
            LoadRatio < 0.5f ? "?먰솢" :
            LoadRatio < 0.85f ? "보통" : "혼잡";
    }
}

