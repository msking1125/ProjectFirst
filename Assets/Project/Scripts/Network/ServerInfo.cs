using System;

namespace ProjectFirst.Network
{
    /// <summary>
    /// ?쒕쾭 ?곹깭 enum
    /// </summary>
    public enum ServerStatus
    {
        /// <summary>?먰솢 (?묒냽 媛??</summary>
        Smooth,
        /// <summary>蹂댄넻 (?쎄컙 ?쇱옟)</summary>
        Busy,
        /// <summary>?ы솕 (?묒냽 遺덇?)</summary>
        Full
    }

    /// <summary>
    /// ?쒕쾭 ?곌껐???꾩슂???뺣낫
    /// </summary>
    [Serializable]
    public class ServerInfo
    {
        /// <summary>?쒕쾭 怨좎쑀 ID</summary>
        public string serverId;

        /// <summary>?쒕쾭 ?쒖떆 ?대쫫</summary>
        public string serverName;

        /// <summary>?쒕쾭 IP 二쇱냼</summary>
        public string serverIP;

        /// <summary>?쒕쾭 ?ы듃</summary>
        public int port;

        /// <summary>?쒕쾭 ?곹깭</summary>
        public ServerStatus status;

        public override string ToString()
        {
            return $"{serverName} ({serverIP}:{port}) - {status}";
        }
    }
}
