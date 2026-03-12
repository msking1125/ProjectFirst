/// <summary>
/// 게임 내 재화 종류를 명확하게 구분하는 열거형.
/// PlayerData.AddCurrency / GetCurrency 에서 사용합니다.
/// </summary>
public enum CurrencyType
{
    /// <summary>PvE 컨텐츠(배틀 클리어, 퀘스트 등)에서 획득하는 기본 재화.</summary>
    Gold,

    /// <summary>프리미엄 재화. 뽑기·아이템 구매 등에 사용.</summary>
    Diamond,

    /// <summary>배틀 입장권. 소모 후 재충전 방식.</summary>
    Ticket,
}
