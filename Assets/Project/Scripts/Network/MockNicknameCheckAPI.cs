using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ProjectFirst.Network
{
    public class MockNicknameCheckAPI : INicknameCheckAPI
    {
        // ?됰궡?닿린 ?꾪빐 ?뱀젙 ?됰꽕?꾩? 以묐났?섏뿀?ㅺ퀬 媛?뺥빀?덈떎.
        private readonly string[] duplicateNicknames = { "tester", "admin", "player1" };

        public async UniTask<bool> CheckDuplicateAsync(string nickname)
        {
            // ?ㅽ듃?뚰겕 ?쒕젅???됰궡
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f));

            if (nickname == null) return false;

            string lowerNick = nickname.ToLower();
            foreach (var duplicate in duplicateNicknames)
            {
                if (lowerNick == duplicate)
                {
                    Debug.Log($"[MockAPI] '{nickname}' ?(?? ?대? 議댁옱?섎뒗 ?됰꽕?꾩엯?덈떎.");
                    return false;
                }
            }

            return true;
        }
    }
}
