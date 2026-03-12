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
                Debug.LogError("[MockAPI] ServerInfo媛 null?낅땲??");
                return false;
            }

            Debug.Log($"[MockAPI] {serverInfo.serverName} ({serverInfo.serverIP}:{serverInfo.port}) ???묒냽 ?쒕룄 以?..");
            
            // ?ㅽ듃?뚰겕 ?쒕젅???됰궡
            await UniTask.Delay(TimeSpan.FromSeconds(1.0f));

            if (serverInfo.status == ServerStatus.Full)
            {
                Debug.LogWarning($"[MockAPI] ?묒냽 ?ㅽ뙣: {serverInfo.serverName} ?쒕쾭媛 ?ы솕 ?곹깭?낅땲??");
                return false;
            }

            Debug.Log($"[MockAPI] {serverInfo.serverName} ?쒕쾭 ?묒냽 ?깃났!");
            return true;
        }
    }
}
