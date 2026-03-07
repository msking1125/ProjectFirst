using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ProjectFirst.Network
{
    public class MockNicknameCheckAPI : INicknameCheckAPI
    {
        // 흉내내기 위해 특정 닉네임은 중복되었다고 가정합니다.
        private readonly string[] duplicateNicknames = { "tester", "admin", "player1" };

        public async UniTask<bool> CheckDuplicateAsync(string nickname)
        {
            // 네트워크 딜레이 흉내
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f));

            if (nickname == null) return false;

            string lowerNick = nickname.ToLower();
            foreach (var duplicate in duplicateNicknames)
            {
                if (lowerNick == duplicate)
                {
                    Debug.Log($"[MockAPI] '{nickname}' 은(는) 이미 존재하는 닉네임입니다.");
                    return false;
                }
            }

            return true;
        }
    }
}
