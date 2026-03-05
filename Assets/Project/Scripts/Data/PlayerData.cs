using System;
using UnityEngine;

/// <summary>
/// 플레이어의 재화 진행도 방치보상 타임스탬프를 보관하는 ScriptableObject 에셋.
/// 생성: Project 우클릭 Create Game/Player Data
/// 권장 경로: Assets/Project/Data/PlayerData.asset
/// </summary>
[CreateAssetMenu(menuName = "Game/Player Data", fileName = "PlayerData")]
public class PlayerData : ScriptableObject
{
    [Header("재화")]
    public int ticket;
    public int gold;
    public int diamond;

    [Header("진행도")]
    [Tooltip("현재 도달한 스테이지 인덱스 (0-based). 로비 배경 캐릭터 스프라이트 선택에 사용됩니다.")]
    public int currentStageIndex;
    [Tooltip("현재 선택된 캐릭터 인덱스 (0-based).")]
    public int currentAgentIndex;

    [Header("방치 보상")]
    [Tooltip("마지막으로 방치 보상을 수령한 UTC 시각 (ISO 8601). 런타임에 자동 갱신됩니다.")]
    public string lastIdleRewardTime;

    /// <summary>재화 수치가 변경될 때 발행됩니다 (UI 바인딩용). 변경된 재화 타입을 전달합니다.</summary>
    public event Action<CurrencyType> OnCurrencyChanged;

    // ── 범용 재화 조작 ──────────────────────────────────────────

    /// <summary>재화 타입에 따라 amount만큼 추가합니다. 음수면 무시됩니다.</summary>
    public void AddCurrency(CurrencyType type, int amount)
    {
        switch (type)
        {
            case CurrencyType.Gold:    gold    = Mathf.Max(0, gold    + amount); break;
            case CurrencyType.Diamond: diamond = Mathf.Max(0, diamond + amount); break;
            case CurrencyType.Ticket:  ticket  = Mathf.Max(0, ticket  + amount); break;
        }
        OnCurrencyChanged?.Invoke(type);
    }

    /// <summary>재화 타입에 해당하는 현재 보유량을 반환합니다.</summary>
    public int GetCurrency(CurrencyType type) => type switch
    {
        CurrencyType.Gold    => gold,
        CurrencyType.Diamond => diamond,
        CurrencyType.Ticket  => ticket,
        _                    => 0,
    };

    // ── 개별 재화 조작 (하위 호환 유지) ────────────────────────

    public void AddTicket(int amount)  => AddCurrency(CurrencyType.Ticket,  amount);
    public void AddGold(int amount)    => AddCurrency(CurrencyType.Gold,    amount);
    public void AddDiamond(int amount) => AddCurrency(CurrencyType.Diamond, amount);

    // ── 방치 보상 유틸 ─────────────────────────────────────────

    /// <summary>마지막 수령 이후 경과 시간을 반환합니다. 기록이 없으면 TimeSpan.Zero.</summary>
    public TimeSpan GetIdleElapsed()
    {
        if (string.IsNullOrEmpty(lastIdleRewardTime))
            return TimeSpan.Zero;

        if (DateTime.TryParse(lastIdleRewardTime, null,
                System.Globalization.DateTimeStyles.RoundtripKind, out DateTime last))
            return DateTime.UtcNow - last;

        return TimeSpan.Zero;
    }

    /// <summary>방치 보상 수령 시각을 현재 UTC로 기록합니다.</summary>
    public void MarkIdleRewardClaimed()
    {
        lastIdleRewardTime = DateTime.UtcNow.ToString("o");
    }
}
