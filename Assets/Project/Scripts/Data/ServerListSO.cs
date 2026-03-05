using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 서버 목록을 관리하는 ScriptableObject.
/// 생성: Project 우클릭 → Create → Game/Server List
/// 권장 경로: Assets/Project/Data/ServerList.asset
/// </summary>
[CreateAssetMenu(menuName = "Game/Server List", fileName = "ServerList")]
public class ServerListSO : ScriptableObject
{
    [Tooltip("서버 목록. Inspector에서 추가/수정하세요.")]
    public List<ServerData> servers = new();

    /// <summary>ID로 서버 검색. 없으면 null.</summary>
    public ServerData GetById(string id)
    {
        foreach (var s in servers)
            if (s.serverId == id) return s;
        return null;
    }
}
