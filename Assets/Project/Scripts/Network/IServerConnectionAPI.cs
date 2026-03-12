using Cysharp.Threading.Tasks;
using ProjectFirst.OutGame.Data;

namespace ProjectFirst.Network
{
    public interface IServerConnectionAPI
    {
        /// <summary>
        /// Documentation cleaned.
        /// Documentation cleaned.
        /// </summary>
        UniTask<bool> ConnectToAsync(ServerInfo serverInfo);
    }
}
