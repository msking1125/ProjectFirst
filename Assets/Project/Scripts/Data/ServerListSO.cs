using System.Collections.Generic;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace ProjectFirst.Data
{
    /// <summary>
    /// 서버 목록을 관리하는 ScriptableObject.
    /// 생성: Project 우클릭 → Create → Soul Ark/Server List
    /// 권장 경로: Assets/Project/Data/ServerList.asset
    /// </summary>
#if ODIN_INSPECTOR
    [CreateAssetMenu(menuName = "Soul Ark/Server List", fileName = "ServerList")]
#else
    [CreateAssetMenu(menuName = "Game/Server List", fileName = "ServerList")]
#endif
    public class ServerListSO : ScriptableObject
    {
#if ODIN_INSPECTOR
        [Title("서버 목록", TitleAlignment = TitleAlignments.Centered)]
        [TableList(ShowIndexLabels = true, AlwaysExpanded = true)]
        [Searchable]
        [Tooltip("서버 목록. Inspector에서 추가/수정하세요.")]
#endif
        public List<ServerData> servers = new();

        /// <summary>ID로 서버 검색. 없으면 null.</summary>
        public ServerData GetById(string id)
        {
            foreach (var s in servers)
                if (s.serverId == id) return s;
            return null;
        }
    }
}
