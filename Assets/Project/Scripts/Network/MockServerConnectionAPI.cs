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
                Debug.LogError("[Log] 오류가 발생했습니다.");
                return false;
            }

            Debug.Log("[Log] 상태가 갱신되었습니다.");
            await UniTask.Delay(TimeSpan.FromSeconds(1.0f));

            if (serverInfo.status == ServerStatus.Full)
            {
                Debug.LogWarning("[Log] 경고가 발생했습니다.");
                return false;
            }

            Debug.Log("[Log] 상태가 갱신되었습니다.");
            return true;
        }
    }
}

