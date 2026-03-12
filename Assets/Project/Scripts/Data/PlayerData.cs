using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어의 계정 정보·재화·진행도·방치 보상 타임스탬프를 보관하는 ScriptableObject 에셋.
/// 생성: Project 우클릭 → Create/Game/Player Data
/// 권장 경로: Assets/Project/Data/PlayerData.asset
///
/// [재화 구조]
///   신규 — gold(long), gem(long), stamina/staminaMax(int)
///   레거시 — ticket(int), diamond(int)  ← IdleRewardManager 등 기존 시스템 호환용
/// </summary>
[CreateAssetMenu(menuName = "Game/Player Data", fileName = "PlayerData")]
public class PlayerData : ScriptableObject
{
    // ── 계정 ──────────────────────────────────────────────────

    [Header("계정")]
    public string uid;
    public string nickname;
    public int accountLevel;
    public int accountExp;

    // ── 캐릭터 ────────────────────────────────────────────────

    [Header("캐릭터")]
    [Tooltip("로비에서 표시할 메인 캐릭터 ID (AgentTable.id 기준).")]
    public int mainCharacterId;

    // ── 진행도 ────────────────────────────────────────────────

    [Header("진행도")]
    [Tooltip("0~100 전체 스테이지 진행도. 로비 배경 선택에 사용됩니다 (index = stageProgress / 10).")]
    public int stageProgress;
    public int currentChapter;
    public int currentStage;

    [Tooltip("현재 도달한 스테이지 인덱스 (0-based, 레거시). 기존 RefreshCenterUI 호환용.")]
    public int currentStageIndex;
    [Tooltip("현재 선택된 캐릭터 인덱스 (0-based, 레거시).")]
    public int currentAgentIndex;

    // ── 스태미나 ──────────────────────────────────────────────

    [Header("스태미나")]
    public int stamina;
    public int staminaMax;

    // ── 재화 (신규) ───────────────────────────────────────────

    [Header("재화 (신규)")]
    public long gold;
    public long gem;

    // ── 재화 (레거시) — IdleRewardManager 등 기존 호환 ────────

    [Header("재화 (레거시)")]
    [Tooltip("기존 시스템(IdleRewardManager 등)에서 사용하는 티켓.")]
    public int ticket;
    [Tooltip("기존 시스템에서 사용하는 다이아. 신규 코드는 gem 사용.")]
    public int diamond;

    // ── 보유 목록 ─────────────────────────────────────────────

    [Header("보유 목록")]
    public List<int> ownedCharacterIds  = new List<int>();
    public List<int> ownedPetIds        = new List<int>();
    public List<int> ownedEquipmentIds  = new List<int>();

    // ── 방치 보상 ─────────────────────────────────────────────

    [Header("방치 보상")]
    [Tooltip("마지막으로 방치 보상을 수령한 UTC 시각 (ISO 8601). 런타임에 자동 갱신됩니다.")]
    public string lastIdleRewardTime;

    // ── 이벤트 채널 (VoidEventChannelSO) ─────────────────────

    [Header("이벤트 채널")]
    [Tooltip("재화 변경 시 발행. LobbyManager 등 UIToolkit 기반 뷰가 구독합니다.")]
    public VoidEventChannelSO onCurrencyChanged;
    [Tooltip("캐릭터 변경 시 발행.")]
    public VoidEventChannelSO onCharacterChanged;

    // ── 레거시 이벤트 (Action, 기존 시스템 호환) ─────────────

    /// <summary>재화 수치가 변경될 때 발행됩니다 (기존 UGUI 시스템 호환용).</summary>
    public event Action<CurrencyType> OnCurrencyChanged;

    // ── 신규 재화 조작 ────────────────────────────────────────

    /// <summary>골드를 추가하고 이벤트를 발행합니다.</summary>
    public void AddGold(long amount)
    {
        gold = Math.Max(0L, gold + amount);
        RaiseCurrencyEvents(CurrencyType.Gold);
    }

    /// <summary>젬을 추가하고 이벤트를 발행합니다.</summary>
    public void AddGem(long amount)
    {
        gem = Math.Max(0L, gem + amount);
        RaiseCurrencyEvents(CurrencyType.Diamond);
    }

    /// <summary>골드 차감. 잔액 부족 시 false 반환.</summary>
    public bool SpendGold(long amount)
    {
        if (gold < amount) return false;
        gold -= amount;
        RaiseCurrencyEvents(CurrencyType.Gold);
        return true;
    }

    // ── 레거시 범용 재화 조작 ─────────────────────────────────

    /// <summary>재화 타입에 따라 amount만큼 추가합니다 (레거시 호환).</summary>
    public void AddCurrency(CurrencyType type, int amount)
    {
        switch (type)
        {
            case CurrencyType.Gold:
                gold    = Math.Max(0L, gold + amount);
                break;
            case CurrencyType.Diamond:
                diamond = Mathf.Max(0, diamond + amount);
                break;
            case CurrencyType.Ticket:
                ticket  = Mathf.Max(0, ticket + amount);
                break;
        }
        RaiseCurrencyEvents(type);
    }

    /// <summary>재화 타입에 해당하는 현재 보유량을 반환합니다 (레거시 호환).</summary>
    public int GetCurrency(CurrencyType type) => type switch
    {
        CurrencyType.Gold    => (int)Math.Min(gold, int.MaxValue),
        CurrencyType.Diamond => diamond,
        CurrencyType.Ticket  => ticket,
        _                    => 0,
    };

    // ── 레거시 개별 재화 조작 ─────────────────────────────────

    public void AddTicket(int amount)  => AddCurrency(CurrencyType.Ticket,  amount);
    public void AddDiamond(int amount) => AddCurrency(CurrencyType.Diamond, amount);

    // ── 스태미나 조작 ─────────────────────────────────────────

    /// <summary>스태미나를 차감합니다. 잔액 부족 시 false를 반환하며 차감하지 않습니다.</summary>
    public bool SpendStamina(int amount)
    {
        if (stamina < amount) return false;
        stamina -= amount;
        return true;
    }

    // ── 방치 보상 유틸 ────────────────────────────────────────

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

    // ── 내부 헬퍼 ────────────────────────────────────────────

    private void RaiseCurrencyEvents(CurrencyType type)
    {
        onCurrencyChanged?.RaiseEvent();
        OnCurrencyChanged?.Invoke(type);
    }
}
