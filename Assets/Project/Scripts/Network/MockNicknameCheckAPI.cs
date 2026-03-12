using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ProjectFirst.Network
{
    public class MockNicknameCheckAPI : INicknameCheckAPI
    {
        private readonly string[] duplicateNicknames = { "tester", "admin", "player1" };

        public async UniTask<bool> CheckDuplicateAsync(string nickname)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f));

            if (nickname == null) return false;

            string lowerNick = nickname.ToLower();
            foreach (var duplicate in duplicateNicknames)
            {
                if (lowerNick == duplicate)
                {
                    Debug.Log("[Log] 상태가 갱신되었습니다.");
                    return false;
                }
            }

            return true;
        }
    }
}

