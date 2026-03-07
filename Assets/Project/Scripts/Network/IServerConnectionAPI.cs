using Cysharp.Threading.Tasks;
using ProjectFirst.OutGame.Data;

namespace ProjectFirst.Network
{
    public interface IServerConnectionAPI
    {
        /// <summary>
        /// 선택한 서버 정보로 접속을 시도합니다.
        /// 반환값이 true이면 접속 성공, false이면 실패.
        /// </summary>
        UniTask<bool> ConnectToAsync(ServerInfo serverInfo);
    }
}
