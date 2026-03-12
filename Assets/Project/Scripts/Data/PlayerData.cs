using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProjectFirst.Data
{
    /// <summary>
    /// Documentation cleaned.
    /// Documentation cleaned.
    /// Documentation cleaned.
    ///
    /// Documentation cleaned.
    /// Documentation cleaned.
    /// Documentation cleaned.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Player Data", fileName = "PlayerData")]
    public class PlayerData : ScriptableObject
    {
        // Note: cleaned comment.
        public string uid;

        public string nickname;

        public int accountLevel;

        public int accountExp;

        public int accountExpMax = 100;

        public string selectedServerId;

        // Note: cleaned comment.
        public int mainCharacterId;

        // Note: cleaned comment.
        public int currentChapter;

        public int currentStage;

        public int stageProgress;

        [Tooltip("Configured in inspector.")]
        public int currentStageIndex;

        [Tooltip("Configured in inspector.")]
        public int currentAgentIndex;

        // Note: cleaned comment.
        public int stamina;

        public int staminaMax;

        // Note: cleaned comment.
        public long gold;

        public long gem;

        // Note: cleaned comment.
        [Header("Settings")]
        public int ticket;

        public int diamond;

        // Note: cleaned comment.
        [Header("Settings")]
        public List<int> ownedCharacterIds = new List<int>();

        public List<int> ownedPetIds = new List<int>();

        public List<int> ownedEquipmentIds = new List<int>();

        // Note: cleaned comment.
        [Header("Settings")]
        public List<CharacterProgressRecord> characterProgressRecords = new List<CharacterProgressRecord>();

        public List<ExpItemInventoryRecord> expItemInventoryRecords = new List<ExpItemInventoryRecord>();

        // Note: cleaned comment.
        [Header("Settings")]
        public List<MissionProgressRecord> missionProgressRecords = new List<MissionProgressRecord>();

        public List<MissionTierClaimRecord> missionTierClaimRecords = new List<MissionTierClaimRecord>();

        public string lastDailyMissionResetUtc;

        public string lastWeeklyMissionResetUtc;

        // Note: cleaned comment.
        [Header("Settings")]
        [HideInInspector]
        public List<TutorialFlagEntry> tutorialFlagEntries = new List<TutorialFlagEntry>();

        /// Documentation cleaned.
        [NonSerialized]
        public Dictionary<string, bool> TutorialFlags = new Dictionary<string, bool>();

        /// Documentation cleaned.
        public void RebuildTutorialFlags()
        {
            TutorialFlags.Clear();
            foreach (TutorialFlagEntry entry in tutorialFlagEntries)
            {
                if (!string.IsNullOrEmpty(entry.key))
                    TutorialFlags[entry.key] = entry.done;
            }
        }

        /// Documentation cleaned.
        public void SyncTutorialFlagEntries()
        {
            tutorialFlagEntries.Clear();
            foreach (var kvp in TutorialFlags)
                tutorialFlagEntries.Add(new TutorialFlagEntry { key = kvp.Key, done = kvp.Value });
        }

        // Note: cleaned comment.
        [Header("Settings")]
        public string lastIdleRewardTime;

        // Note: cleaned comment.
        [Header("Settings")]
        public VoidEventChannelSO onCurrencyChanged;

        public VoidEventChannelSO onCharacterChanged;


        // Note: cleaned comment.

        /// Documentation cleaned.
        public event Action<CurrencyType> OnCurrencyChanged;

    // Note: cleaned comment.

    /// Documentation cleaned.
    public void AddGold(long amount)
    {
        gold = Math.Max(0L, gold + amount);
        RaiseCurrencyEvents(CurrencyType.Gold);
    }

    /// Documentation cleaned.
    public void AddGem(long amount)
    {
        gem = Math.Max(0L, gem + amount);
        RaiseCurrencyEvents(CurrencyType.Diamond);
    }

    /// Documentation cleaned.
    public bool SpendGold(long amount)
    {
        if (gold < amount) return false;
        gold -= amount;
        RaiseCurrencyEvents(CurrencyType.Gold);
        return true;
    }

    // Note: cleaned comment.

    /// Documentation cleaned.
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

    /// Documentation cleaned.
    public int GetCurrency(CurrencyType type) => type switch
    {
        CurrencyType.Gold    => (int)Math.Min(gold, int.MaxValue),
        CurrencyType.Diamond => diamond,
        CurrencyType.Ticket  => ticket,
        _                    => 0,
    };

    // Note: cleaned comment.

    public void AddTicket(int amount)  => AddCurrency(CurrencyType.Ticket,  amount);
    public void AddDiamond(int amount) => AddCurrency(CurrencyType.Diamond, amount);

    // Note: cleaned comment.

    /// Documentation cleaned.
    public bool SpendStamina(int amount)
    {
        if (stamina < amount) return false;
        stamina -= amount;
        return true;
    }

    // Note: cleaned comment.

    /// Documentation cleaned.
    public int GetCharacterLevel(int agentId)
    {
        CharacterProgressRecord rec = characterProgressRecords?.FirstOrDefault(r => r.agentId == agentId);
        return rec?.level ?? 1;
    }

    /// Documentation cleaned.
    public int GetCharacterExp(int agentId)
    {
        CharacterProgressRecord rec = characterProgressRecords?.FirstOrDefault(r => r.agentId == agentId);
        return rec?.exp ?? 0;
    }

    /// Documentation cleaned.
    public void SetCharacterProgress(int agentId, int level, int exp)
    {
        if (characterProgressRecords == null)
            characterProgressRecords = new List<CharacterProgressRecord>();

        CharacterProgressRecord rec = characterProgressRecords.FirstOrDefault(r => r.agentId == agentId);
        if (rec == null)
        {
            rec = new CharacterProgressRecord { agentId = agentId };
            characterProgressRecords.Add(rec);
        }
        rec.level = level;
        rec.exp   = exp;
    }

    // Note: cleaned comment.

    /// Documentation cleaned.
    public int GetExpItemCount(ExpItemType type)
    {
        ExpItemInventoryRecord rec = expItemInventoryRecords?.FirstOrDefault(r => r.type == type);
        return rec?.count ?? 0;
    }

    /// Documentation cleaned.
    public void SetExpItemCount(ExpItemType type, int count)
    {
        if (expItemInventoryRecords == null)
            expItemInventoryRecords = new List<ExpItemInventoryRecord>();

        ExpItemInventoryRecord rec = expItemInventoryRecords.FirstOrDefault(r => r.type == type);
        if (rec == null)
        {
            rec = new ExpItemInventoryRecord { type = type };
            expItemInventoryRecords.Add(rec);
        }
        rec.count = Mathf.Max(0, count);
    }

    /// Documentation cleaned.
    public int GetTotalExpItemCount()
    {
        if (expItemInventoryRecords == null) return 0;
        return expItemInventoryRecords.Sum(r => r.count);
    }

    // Note: cleaned comment.

    /// Documentation cleaned.
    public string GetNicknameOrDefault(string defaultValue = "Player")
        => string.IsNullOrEmpty(nickname) ? defaultValue : nickname;

    /// Documentation cleaned.
    public int GetAccountLevel(int defaultValue = 1)
        => accountLevel > 0 ? accountLevel : defaultValue;

    /// Documentation cleaned.
    public int GetAccountExp() => accountExp;

    /// Documentation cleaned.
    public int GetAccountExpMax(int defaultValue = 100)
        => accountExpMax > 0 ? accountExpMax : defaultValue;

    /// Documentation cleaned.
    public void SetAccountStats(int level, int exp, int expMax)
    {
        accountLevel  = Mathf.Max(1, level);
        accountExp    = Mathf.Max(0, exp);
        accountExpMax = Mathf.Max(1, expMax);
    }

    /// Documentation cleaned.
    public void SetNicknameValue(string value)
        => nickname = value ?? string.Empty;

    // Note: cleaned comment.

    /// Documentation cleaned.
    public string GetUidOrCreate()
    {
        if (string.IsNullOrEmpty(uid))
            uid = Guid.NewGuid().ToString();
        return uid;
    }

    /// Documentation cleaned.
    public void SetUidValue(string value)
        => uid = value ?? string.Empty;

    /// Documentation cleaned.
    public string GetSelectedServerId() => selectedServerId ?? string.Empty;

    /// Documentation cleaned.
    public void SetSelectedServerId(string value)
        => selectedServerId = value ?? string.Empty;

    /// Documentation cleaned.
    public void ClearLoginState()
    {
        uid              = string.Empty;
        nickname         = string.Empty;
        selectedServerId = string.Empty;
    }

    // Note: cleaned comment.

    /// <summary>
    /// Documentation cleaned.
    /// Documentation cleaned.
    /// </summary>
    public bool TryGrantReward(RewardItem reward)
    {
        if (reward == null) return false;

        string name = reward.itemName?.ToLower() ?? string.Empty;

        if (reward.itemId == 1001 || name.Contains("gem") || name.Contains("diamond"))
        {
            AddGem(reward.amount);
            return true;
        }
        if (reward.itemId == 2001 || name.Contains("gold") || name.Contains("怨⑤뱶"))
        {
            AddGold(reward.amount);
            return true;
        }
        if (name.Contains("ticket") || name.Contains("?곗폆"))
        {
            AddTicket(reward.amount);
            return true;
        }
        if (name.Contains("stamina") || name.Contains("?ㅽ깭誘몃굹"))
        {
            stamina = Mathf.Min(stamina + reward.amount, staminaMax > 0 ? staminaMax : 999);
            return true;
        }

        return false;
    }

    // Note: cleaned comment.

    /// Documentation cleaned.
    public TimeSpan GetIdleElapsed()
    {
        if (string.IsNullOrEmpty(lastIdleRewardTime))
            return TimeSpan.Zero;

        if (DateTime.TryParse(lastIdleRewardTime, null,
                System.Globalization.DateTimeStyles.RoundtripKind, out DateTime last))
            return DateTime.UtcNow - last;

        return TimeSpan.Zero;
    }

    /// Documentation cleaned.
    public void MarkIdleRewardClaimed()
    {
        lastIdleRewardTime = DateTime.UtcNow.ToString("o");
    }

    // Note: cleaned comment.

    private void RaiseCurrencyEvents(CurrencyType type)
    {
        onCurrencyChanged?.RaiseEvent();
        OnCurrencyChanged?.Invoke(type);
    }

    // Note: cleaned comment.

    /// Documentation cleaned.
    [Serializable]
    public class TutorialFlagEntry
    {
        public string key;
        public bool done;
    }
}
}

