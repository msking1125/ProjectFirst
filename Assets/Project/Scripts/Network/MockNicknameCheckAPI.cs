using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ProjectFirst.Network
{
    public class MockNicknameCheckAPI : INicknameCheckAPI
    {
        // Note: cleaned comment.
        private readonly string[] duplicateNicknames = { "tester", "admin", "player1" };

        public async UniTask<bool> CheckDuplicateAsync(string nickname)
        {
            // Note: cleaned comment.
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f));

            if (nickname == null) return false;

            string lowerNick = nickname.ToLower();
            foreach (var duplicate in duplicateNicknames)
            {
                if (lowerNick == duplicate)
                {
                    Debug.Log("[Log] Message cleaned.");
                    return false;
                }
            }

            return true;
        }
    }
}
