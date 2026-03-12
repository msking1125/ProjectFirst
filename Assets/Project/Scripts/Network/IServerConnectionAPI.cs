using Cysharp.Threading.Tasks;
using ProjectFirst.OutGame.Data;

namespace ProjectFirst.Network
{
    public interface IServerConnectionAPI
    {
        /// <summary>
        /// ?좏깮???쒕쾭 ?뺣낫濡??묒냽???쒕룄?⑸땲??
        /// 諛섑솚媛믪씠 true?대㈃ ?묒냽 ?깃났, false?대㈃ ?ㅽ뙣.
        /// </summary>
        UniTask<bool> ConnectToAsync(ServerInfo serverInfo);
    }
}
