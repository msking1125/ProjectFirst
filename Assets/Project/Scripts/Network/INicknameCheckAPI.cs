using Cysharp.Threading.Tasks;

namespace ProjectFirst.Network
{
    public interface INicknameCheckAPI
    {
        /// <summary>
        /// ?쒕쾭???됰꽕??以묐났 ?щ?瑜?吏덉쓽?⑸땲??
        /// 諛섑솚媛믪씠 true?대㈃ ?됰꽕???ъ슜 媛??(以묐났 ?꾨떂),
        /// false?대㈃ ?됰꽕??以묐났.
        /// </summary>
        UniTask<bool> CheckDuplicateAsync(string nickname);
    }
}
