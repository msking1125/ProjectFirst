using Cysharp.Threading.Tasks;
using ProjectFirst.OutGame.Data;

namespace ProjectFirst.Network
{
    public interface IServerConnectionAPI
    {
        UniTask<bool> ConnectToAsync(ServerInfo serverInfo);
    }
}

