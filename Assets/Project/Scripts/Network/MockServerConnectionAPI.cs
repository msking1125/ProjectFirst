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
                Debug.LogError("[Log] Error message cleaned.");
                return false;
            }

            Debug.Log("[Log] Message cleaned.");
            
            // Note: cleaned comment.
            await UniTask.Delay(TimeSpan.FromSeconds(1.0f));

            if (serverInfo.status == ServerStatus.Full)
            {
                Debug.LogWarning("[Log] Warning message cleaned.");
                return false;
            }

            Debug.Log("[Log] Message cleaned.");
            return true;
        }
    }
}
