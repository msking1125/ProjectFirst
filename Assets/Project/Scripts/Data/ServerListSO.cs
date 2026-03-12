using System.Collections.Generic;
using UnityEngine;

namespace ProjectFirst.Data
{
    /// <summary>
    /// Documentation cleaned.
    /// Documentation cleaned.
    /// Documentation cleaned.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Server List", fileName = "ServerList")]
    public class ServerListSO : ScriptableObject
    {
        public List<ServerData> servers = new();

        /// Documentation cleaned.
        public ServerData GetById(string id)
        {
            foreach (var s in servers)
                if (s.serverId == id) return s;
            return null;
        }
    }
}
