using Cysharp.Threading.Tasks;

namespace ProjectFirst.Network
{
    public interface INicknameCheckAPI
    {
        /// <summary>
        /// 서버에 닉네임 중복 여부를 질의합니다.
        /// 반환값이 true이면 닉네임 사용 가능 (중복 아님),
        /// false이면 닉네임 중복.
        /// </summary>
        UniTask<bool> CheckDuplicateAsync(string nickname);
    }
}
