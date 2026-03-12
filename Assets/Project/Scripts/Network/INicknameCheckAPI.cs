using Cysharp.Threading.Tasks;

namespace ProjectFirst.Network
{
    public interface INicknameCheckAPI
    {
        UniTask<bool> CheckDuplicateAsync(string nickname);
    }
}

