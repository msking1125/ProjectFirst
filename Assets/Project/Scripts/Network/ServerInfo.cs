using System;

namespace ProjectFirst.Network
{
    /// <summary>
    /// Documentation cleaned.
    /// </summary>
    public enum ServerStatus
    {
        /// Documentation cleaned.
        Smooth,
        /// Documentation cleaned.
        Busy,
        /// Documentation cleaned.
        Full
    }

    /// <summary>
    /// Documentation cleaned.
    /// </summary>
    [Serializable]
    public class ServerInfo
    {
        /// Documentation cleaned.
        public string serverId;

        /// Documentation cleaned.
        public string serverName;

        /// Documentation cleaned.
        public string serverIP;

        /// Documentation cleaned.
        public int port;

        /// Documentation cleaned.
        public ServerStatus status;

        public override string ToString()
        {
            return $"{serverName} ({serverIP}:{port}) - {status}";
        }
    }
}
