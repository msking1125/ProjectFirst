using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProjectFirst.Data
{
        /// <summary>
    /// 플레이어 계정, 재화, 진행도, 캐릭터 성장 상태를 저장하는 ScriptableObject입니다.
    /// 로비와 각종 UI 시스템이 공통으로 참조하는 런타임 플레이어 데이터입니다.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Player Data", fileName = "PlayerData")]
    public class PlayerData : ScriptableObject
    {
        public string uid;

        public string nickname;

        public int accountLevel;

        public int accountExp;

        public int accountExpMax = 100;

        public string selectedServerId;
        public int mainCharacterId;
        public int currentChapter;

        public int currentStage;

        public int stageProgress;

        [Tooltip("인스펙터에서 설정합니다.")]
        public int currentStageIndex;

        [Tooltip("인스펙터에서 설정합니다.")]
        public int currentAgentIndex;
        public int stamina;

        public int staminaMax;
        public long gold;

        public long gem;
        [Header("재화 (레거시)")]
        public int ticket;

        public int diamond;
        [Header("보유 목록")]
        public List<int> ownedCharacterIds = new List<int>();

        public List<int> ownedPetIds = new List<int>();

        public List<int> ownedEquipmentIds = new List<int>();
        [Header("캐릭터 성장")]
        public List<CharacterProgressRecord> characterProgressRecords = new List<CharacterProgressRecord>();

        public List<ExpItemInventoryRecord> expItemInventoryRecords = new List<ExpItemInventoryRecord>();
        [Header("미션 상태")]
        public List<MissionProgressRecord> missionProgressRecords = new List<MissionProgressRecord>();

        public List<MissionTierClaimRecord> missionTierClaimRecords = new List<MissionTierClaimRecord>();

        public string lastDailyMissionResetUtc;

        public string lastWeeklyMissionResetUtc;
        [Header("튜토리얼")]
        [HideInInspector]
        public List<TutorialFlagEntry> tutorialFlagEntries = new List<TutorialFlagEntry>();
        [NonSerialized]
        public Dictionary<string, bool> TutorialFlags = new Dictionary<string, bool>();
        public void RebuildTutorialFlags()
        {
            TutorialFlags.Clear();
            foreach (TutorialFlagEntry entry in tutorialFlagEntries)
            {
                if (!string.IsNullOrEmpty(entry.key))
                    TutorialFlags[entry.key] = entry.done;
            }
        }
        public void SyncTutorialFlagEntries()
        {
            tutorialFlagEntries.Clear();
            foreach (var kvp in TutorialFlags)
                tutorialFlagEntries.Add(new TutorialFlagEntry { key = kvp.Key, done = kvp.Value });
        }
        [Header("방치 보상")]
        public string lastIdleRewardTime;
        [Header("이벤트")]
        public VoidEventChannelSO onCurrencyChanged;

        public VoidEventChannelSO onCharacterChanged;
        public event Action<CurrencyType> OnCurrencyChanged;
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
    public int GetCurrency(CurrencyType type) => type switch
    {
        CurrencyType.Gold    => (int)Math.Min(gold, int.MaxValue),
        CurrencyType.Diamond => diamond,
        CurrencyType.Ticket  => ticket,
        _                    => 0,
    };

    public void AddTicket(int amount)  => AddCurrency(CurrencyType.Ticket,  amount);
    public void AddDiamond(int amount) => AddCurrency(CurrencyType.Diamond, amount);
    public bool SpendStamina(int amount)
    {
        if (stamina < amount) return false;
        stamina -= amount;
        return true;
    }
    public int GetCharacterLevel(int agentId)
    {
        CharacterProgressRecord rec = characterProgressRecords?.FirstOrDefault(r => r.agentId == agentId);
        return rec?.level ?? 1;
    }
    public int GetCharacterExp(int agentId)
    {
        CharacterProgressRecord rec = characterProgressRecords?.FirstOrDefault(r => r.agentId == agentId);
        return rec?.exp ?? 0;
    }
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
    public int GetExpItemCount(ExpItemType type)
    {
        ExpItemInventoryRecord rec = expItemInventoryRecords?.FirstOrDefault(r => r.type == type);
        return rec?.count ?? 0;
    }
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
    public int GetTotalExpItemCount()
    {
        if (expItemInventoryRecords == null) return 0;
        return expItemInventoryRecords.Sum(r => r.count);
    }
    public string GetNicknameOrDefault(string defaultValue = "플레이어")
        => string.IsNullOrEmpty(nickname) ? defaultValue : nickname;
    public int GetAccountLevel(int defaultValue = 1)
        => accountLevel > 0 ? accountLevel : defaultValue;
    public int GetAccountExp() => accountExp;
    public int GetAccountExpMax(int defaultValue = 100)
        => accountExpMax > 0 ? accountExpMax : defaultValue;
    public void SetAccountStats(int level, int exp, int expMax)
    {
        accountLevel  = Mathf.Max(1, level);
        accountExp    = Mathf.Max(0, exp);
        accountExpMax = Mathf.Max(1, expMax);
    }
    public void SetNicknameValue(string value)
        => nickname = value ?? string.Empty;
    public string GetUidOrCreate()
    {
        if (string.IsNullOrEmpty(uid))
            uid = Guid.NewGuid().ToString();
        return uid;
    }
    public void SetUidValue(string value)
        => uid = value ?? string.Empty;
    public string GetSelectedServerId() => selectedServerId ?? string.Empty;
    public void SetSelectedServerId(string value)
        => selectedServerId = value ?? string.Empty;
    public void ClearLoginState()
    {
        uid              = string.Empty;
        nickname         = string.Empty;
        selectedServerId = string.Empty;
    }

        /// <summary>
    /// 플레이어 계정, 재화, 진행도, 캐릭터 성장 상태를 저장하는 ScriptableObject입니다.
    /// 로비와 각종 UI 시스템이 공통으로 참조하는 런타임 플레이어 데이터입니다.
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
        if (reward.itemId == 2001 || name.Contains("gold") || name.Contains("골드"))
        {
            AddGold(reward.amount);
            return true;
        }
        if (name.Contains("ticket") || name.Contains("티켓"))
        {
            AddTicket(reward.amount);
            return true;
        }
        if (name.Contains("stamina") || name.Contains("스태미나"))
        {
            stamina = Mathf.Min(stamina + reward.amount, staminaMax > 0 ? staminaMax : 999);
            return true;
        }

        return false;
    }
    public TimeSpan GetIdleElapsed()
    {
        if (string.IsNullOrEmpty(lastIdleRewardTime))
            return TimeSpan.Zero;

        if (DateTime.TryParse(lastIdleRewardTime, null,
                System.Globalization.DateTimeStyles.RoundtripKind, out DateTime last))
            return DateTime.UtcNow - last;

        return TimeSpan.Zero;
    }
    public void MarkIdleRewardClaimed()
    {
        lastIdleRewardTime = DateTime.UtcNow.ToString("o");
    }

    private void RaiseCurrencyEvents(CurrencyType type)
    {
        onCurrencyChanged?.RaiseEvent();
        OnCurrencyChanged?.Invoke(type);
    }
    [Serializable]
    public class TutorialFlagEntry
    {
        public string key;
        public bool done;
    }
}
}






