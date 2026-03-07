using System;

namespace ProjectFirst.Network
{
    /// <summary>
    /// 서버 상태 enum
    /// </summary>
    public enum ServerStatus
    {
        /// <summary>원활 (접속 가능)</summary>
        Smooth,
        /// <summary>보통 (약간 혼잡)</summary>
        Busy,
        /// <summary>포화 (접속 불가)</summary>
        Full
    }

    /// <summary>
    /// 서버 연결에 필요한 정보
    /// </summary>
    [Serializable]
    public class ServerInfo
    {
        /// <summary>서버 고유 ID</summary>
        public string serverId;

        /// <summary>서버 표시 이름</summary>
        public string serverName;

        /// <summary>서버 IP 주소</summary>
        public string serverIP;

        /// <summary>서버 포트</summary>
        public int port;

        /// <summary>서버 상태</summary>
        public ServerStatus status;

        public override string ToString()
        {
            return $"{serverName} ({serverIP}:{port}) - {status}";
        }
    }
}