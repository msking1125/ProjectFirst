using System;
using System.Collections.Generic;
using ProjectFirst.Data;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Player Data", fileName = "PlayerData")]
public class PlayerData : ScriptableObject
{
    private const string LegacyNicknameKey = "nickname";
    private const string ProfileNicknameKey = "player.nickname";
    private const string ProfileLevelKey = "player.level";
    private const string ProfileExpKey = "player.exp";
    private const string ProfileExpMaxKey = "player.expmax";

    [Header("Account")]
    public string uid;
    public string nickname;
    public string selectedServerId;
    public int accountLevel;
    public int accountExp;
    public int accountExpMax = 100;

    [Header("Character")]
    [Tooltip("Main character id used in lobby scenes.")]
    public int mainCharacterId;

    [Header("Progress")]
    [Tooltip("Overall stage progress used by lobby background selection.")]
    public int stageProgress;
    public int currentChapter;
    public int currentStage;
    public int currentStageIndex;
    public int currentAgentIndex;

    [Header("Stamina")]
    public int stamina;
    public int staminaMax;

    [Header("Currencies")]
    public long gold;
    public long gem;

    [Header("Legacy Currencies")]
    public int ticket;
    public int diamond;

    [Header("Owned Items")]
    public List<int> ownedCharacterIds = new();
    public List<int> ownedPetIds = new();
    public List<int> ownedEquipmentIds = new();

    [Header("Character Progress")]
    public List<CharacterProgressRecord> characterProgressRecords = new();
    public List<ExpItemInventoryRecord> expItemInventoryRecords = new();

    [Header("Idle Reward")]
    [Tooltip("Last idle reward claim time in UTC ISO-8601.")]
    public string lastIdleRewardTime;

    [Header("Mission Progress")]
    [Tooltip("Last daily mission reset time in UTC ISO-8601.")]
    public string lastDailyMissionResetUtc;
    [Tooltip("Last weekly mission reset time in UTC ISO-8601.")]
    public string lastWeeklyMissionResetUtc;
    public List<MissionProgressRecord> missionProgressRecords = new();
    public List<MissionTierClaimRecord> missionTierClaimRecords = new();

    [Header("Event Channels")]
    public VoidEventChannelSO onCurrencyChanged;
    public VoidEventChannelSO onCharacterChanged;

    public event Action<CurrencyType> OnCurrencyChanged;

    public string GetNicknameOrDefault(string fallback = "Player")
    {
        if (string.IsNullOrWhiteSpace(nickname))
        {
            nickname = PlayerPrefs.GetString(ProfileNicknameKey, PlayerPrefs.GetString(LegacyNicknameKey, fallback));
        }

        return string.IsNullOrWhiteSpace(nickname) ? fallback : nickname;
    }

    public void SetNicknameValue(string value)
    {
        nickname = value ?? string.Empty;
    }

    public string GetUidOrCreate()
    {
        if (string.IsNullOrWhiteSpace(uid))
        {
            uid = PlayerPrefs.GetString("uid", string.Empty);
        }

        return uid ?? string.Empty;
    }

    public void SetUidValue(string value)
    {
        uid = value ?? string.Empty;
    }

    public string GetSelectedServerId()
    {
        if (string.IsNullOrWhiteSpace(selectedServerId))
        {
            selectedServerId = PlayerPrefs.GetString("lastServer", string.Empty);
        }

        return selectedServerId ?? string.Empty;
    }

    public void SetSelectedServerId(string value)
    {
        selectedServerId = value ?? string.Empty;
    }

    public void ClearLoginState()
    {
        uid = string.Empty;
        nickname = string.Empty;
        selectedServerId = string.Empty;
    }

    

    public int GetAccountLevel(int fallback = 1)
    {
        if (accountLevel <= 0)
        {
            accountLevel = Mathf.Max(1, PlayerPrefs.GetInt(ProfileLevelKey, fallback));
        }

        return accountLevel;
    }

    public int GetAccountExp()
    {
        if (accountExp < 0)
        {
            accountExp = 0;
        }
        else if (accountExp == 0)
        {
            accountExp = Mathf.Max(0, PlayerPrefs.GetInt(ProfileExpKey, 0));
        }

        return accountExp;
    }

    public int GetAccountExpMax(int fallback = 100)
    {
        if (accountExpMax <= 0)
        {
            accountExpMax = Mathf.Max(1, PlayerPrefs.GetInt(ProfileExpMaxKey, fallback));
        }

        return accountExpMax;
    }

    public void SetAccountStats(int level, int exp, int expMax)
    {
        accountLevel = Mathf.Max(1, level);
        accountExp = Mathf.Max(0, exp);
        accountExpMax = Mathf.Max(1, expMax);
    }

    public int GetCharacterLevel(int agentId)
    {
        return GetOrCreateCharacterProgressRecord(agentId).level;
    }

    public int GetCharacterExp(int agentId)
    {
        return GetOrCreateCharacterProgressRecord(agentId).exp;
    }

    public void SetCharacterProgress(int agentId, int level, int exp)
    {
        CharacterProgressRecord record = GetOrCreateCharacterProgressRecord(agentId);
        record.level = level;
        record.exp = exp;
    }

    public int GetExpItemCount(ExpItemType type)
    {
        return GetOrCreateExpItemInventoryRecord(type).count;
    }

    public void SetExpItemCount(ExpItemType type, int count)
    {
        ExpItemInventoryRecord record = GetOrCreateExpItemInventoryRecord(type);
        record.count = count;
    }

    public int GetTotalExpItemCount()
    {
        return GetExpItemCount(ExpItemType.Small)
             + GetExpItemCount(ExpItemType.Medium)
             + GetExpItemCount(ExpItemType.Large)
             + GetExpItemCount(ExpItemType.Crystal);
    }

    public void AddGold(long amount)
    {
        gold = Math.Max(0L, gold + amount);
        RaiseCurrencyEvents(CurrencyType.Gold);
    }

    public void AddGem(long amount)
    {
        gem = Math.Max(0L, gem + amount);
        RaiseCurrencyEvents(CurrencyType.Diamond);
    }

    public bool SpendGold(long amount)
    {
        if (gold < amount) return false;
        gold -= amount;
        RaiseCurrencyEvents(CurrencyType.Gold);
        return true;
    }

    public void AddCurrency(CurrencyType type, int amount)
    {
        switch (type)
        {
            case CurrencyType.Gold:
                gold = Math.Max(0L, gold + amount);
                break;
            case CurrencyType.Diamond:
                diamond = Mathf.Max(0, diamond + amount);
                break;
            case CurrencyType.Ticket:
                ticket = Mathf.Max(0, ticket + amount);
                break;
        }

        RaiseCurrencyEvents(type);
    }

    public int GetCurrency(CurrencyType type) => type switch
    {
        CurrencyType.Gold => (int)Math.Min(gold, int.MaxValue),
        CurrencyType.Diamond => diamond,
        CurrencyType.Ticket => ticket,
        _ => 0,
    };

    public void AddTicket(int amount) => AddCurrency(CurrencyType.Ticket, amount);
    public void AddDiamond(int amount) => AddCurrency(CurrencyType.Diamond, amount);

    public bool TryGrantReward(RewardItem reward)
    {
        if (reward == null)
        {
            return false;
        }

        return TryGrantReward(reward.itemName, reward.amount);
    }

    public bool TryGrantReward(string rewardName, int amount)
    {
        switch (NormalizeRewardName(rewardName))
        {
            case "gold":
                AddGold(amount);
                return true;
            case "gem":
                AddGem(amount);
                return true;
            case "ticket":
                AddTicket(amount);
                return true;
            case "diamond":
                AddDiamond(amount);
                return true;
            default:
                return false;
        }
    }

    public TimeSpan GetIdleElapsed()
    {
        if (string.IsNullOrEmpty(lastIdleRewardTime))
        {
            return TimeSpan.Zero;
        }

        if (DateTime.TryParse(lastIdleRewardTime, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime last))
        {
            return DateTime.UtcNow - last;
        }

        return TimeSpan.Zero;
    }

    public void MarkIdleRewardClaimed()
    {
        lastIdleRewardTime = DateTime.UtcNow.ToString("o");
    }

    private CharacterProgressRecord GetOrCreateCharacterProgressRecord(int agentId)
    {
        characterProgressRecords ??= new List<CharacterProgressRecord>();

        for (int i = 0; i < characterProgressRecords.Count; i++)
        {
            if (characterProgressRecords[i].agentId == agentId)
            {
                return characterProgressRecords[i];
            }
        }

        CharacterProgressRecord record = new CharacterProgressRecord
        {
            agentId = agentId,
            level = Mathf.Max(1, PlayerPrefs.GetInt($"agent_lv_{agentId}", 1)),
            exp = Mathf.Max(0, PlayerPrefs.GetInt($"agent_exp_{agentId}", 0)),
        };
        characterProgressRecords.Add(record);
        return record;
    }

    private ExpItemInventoryRecord GetOrCreateExpItemInventoryRecord(ExpItemType type)
    {
        expItemInventoryRecords ??= new List<ExpItemInventoryRecord>();

        for (int i = 0; i < expItemInventoryRecords.Count; i++)
        {
            if (expItemInventoryRecords[i].type == type)
            {
                return expItemInventoryRecords[i];
            }
        }

        ExpItemInventoryRecord record = new ExpItemInventoryRecord
        {
            type = type,
            count = Mathf.Max(0, PlayerPrefs.GetInt($"exp_item_{(int)type}", 0)),
        };
        expItemInventoryRecords.Add(record);
        return record;
    }

    private static string NormalizeRewardName(string rewardName)
    {
        if (string.IsNullOrWhiteSpace(rewardName))
        {
            return string.Empty;
        }

        string normalized = rewardName.Trim().ToLowerInvariant();
        return normalized switch
        {
            "怨⑤뱶" => "gold",
            "gold" => "gold",
            "?? => "gem",
            "gem" => "gem",
            "?곗폆" => "ticket",
            "ticket" => "ticket",
            "?ㅼ씠?? => "diamond",
            "diamond" => "diamond",
            _ => normalized,
        };
    }

    private void RaiseCurrencyEvents(CurrencyType type)
    {
        onCurrencyChanged?.RaiseEvent();
        OnCurrencyChanged?.Invoke(type);
    }
}
