using Cysharp.Threading.Tasks;

namespace ProjectFirst.Network
{
    public interface INicknameCheckAPI
    {
        /// <summary>
        /// Documentation cleaned.
        /// Documentation cleaned.
        /// Documentation cleaned.
        /// </summary>
        UniTask<bool> CheckDuplicateAsync(string nickname);
    }
}
