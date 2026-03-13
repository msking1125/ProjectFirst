using System;
using UnityEngine;

namespace ProjectFirst.Data
{
    [Serializable]
    public class ServerData
    {
        public string serverId;
        public string displayName;
        public int maxPlayers = 3000;
        public int currentPlayers;
        public float LoadRatio => maxPlayers > 0 ? Mathf.Clamp01((float)currentPlayers / maxPlayers) : 0f;
        public string CongestionLabel =>
            LoadRatio < 0.5f ? "원활" :
            LoadRatio < 0.85f ? "보통" : "혼잡";
    }
}


