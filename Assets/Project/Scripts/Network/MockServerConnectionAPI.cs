using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ProjectFirst.OutGame.Data;

namespace ProjectFirst.Network
{
    public class MockServerConnectionAPI : IServerConnectionAPI
    {
        public async UniTask<bool> ConnectToAsync(ServerInfo serverInfo)
        {
            if (serverInfo == null)
            {
                Debug.LogError("[MockAPI] ServerInfo가 null입니다.");
                return false;
            }

            Debug.Log($"[MockAPI] {serverInfo.serverName} ({serverInfo.serverIP}:{serverInfo.port}) 에 접속 시도 중...");
            
            // 네트워크 딜레이 흉내
            await UniTask.Delay(TimeSpan.FromSeconds(1.0f));

            if (serverInfo.status == ServerStatus.Full)
            {
                Debug.LogWarning($"[MockAPI] 접속 실패: {serverInfo.serverName} 서버가 포화 상태입니다.");
                return false;
            }

            Debug.Log($"[MockAPI] {serverInfo.serverName} 서버 접속 성공!");
            return true;
        }
    }
}
