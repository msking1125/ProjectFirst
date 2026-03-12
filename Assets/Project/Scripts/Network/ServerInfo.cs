using System;

namespace ProjectFirst.Network
{
    public enum ServerStatus
    {
        Smooth,
        Busy,
        Full
    }
    [Serializable]
    public class ServerInfo
    {
        public string serverId;
        public string serverName;
        public string serverIP;
        public int port;
        public ServerStatus status;

        public override string ToString()
        {
            return $"{serverName} ({serverIP}:{port}) - {status}";
        }
    }
}

