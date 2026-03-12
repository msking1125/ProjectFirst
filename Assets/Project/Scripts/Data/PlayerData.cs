using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace ProjectFirst.Data
{
    /// <summary>
    /// 플레이어의 계정 정보·재화·진행도·방치 보상 타임스탬프를 보관하는 ScriptableObject 에셋.
    /// 생성: Project 우클릭 → Create/Game/Player Data
    /// 권장 경로: Assets/Project/Data/PlayerData.asset
    ///
    /// [재화 구조]
    ///   신규 — gold(long), gem(long), stamina/staminaMax(int)
    ///   레거시 — ticket(int), diamond(int)  ← IdleRewardManager 등 기존 시스템 호환용
    /// </summary>
#if ODIN_INSPECTOR
    [CreateAssetMenu(menuName = "Soul Ark/Player Data", fileName = "PlayerData")]
#else
    [CreateAssetMenu(menuName = "Game/Player Data", fileName = "PlayerData")]
#endif
    public class PlayerData : ScriptableObject
    {
        // ── 계정 ──────────────────────────────────────────────────
#if ODIN_INSPECTOR
        [Title("계정 정보", TitleAlignment = TitleAlignments.Left)]
        [BoxGroup("계정")]
        [HorizontalGroup("계정/Info", 0.5f)]
        [LabelText("UID")]
        [ReadOnly]
#endif
        public string uid;

#if ODIN_INSPECTOR
        [HorizontalGroup("계정/Info", 0.5f)]
        [LabelText("닉네임")]
#endif
        public string nickname;

#if ODIN_INSPECTOR
        [HorizontalGroup("계정/Level", 0.5f)]
        [LabelText("계정 레벨")]
        [ProgressBar(1, 100, ColorGetter = "GetLevelColor")]
#endif
        public int accountLevel;

#if ODIN_INSPECTOR
        [HorizontalGroup("계정/Level", 0.5f)]
        [LabelText("경험치")]
        [ProgressBar(0, 100, ColorGetter = "GetExpColor")]
#endif
        public int accountExp;

#if ODIN_INSPECTOR
        [BoxGroup("계정")]
        [LabelText("최대 경험치")]
        [HideIf("@accountExpMax <= 0")]
#endif
        public int accountExpMax = 100;

#if ODIN_INSPECTOR
        [BoxGroup("계정")]
        [LabelText("선택한 서버")]
#endif
        public string selectedServerId;

        // ── 캐릭터 ────────────────────────────────────────────────
#if ODIN_INSPECTOR
        [Title("캐릭터", TitleAlignment = TitleAlignments.Left)]
        [BoxGroup("캐릭터")]
        [LabelText("메인 캐릭터 ID")]
        [Tooltip("로비에서 표시할 메인 캐릭터 ID (AgentTable.id 기준).")]
#endif
        public int mainCharacterId;

        // ── 진행도 ────────────────────────────────────────────────
#if ODIN_INSPECTOR
        [Title("진행도", TitleAlignment = TitleAlignments.Left)]
        [HorizontalGroup("진행도", 0.33f)]
        [BoxGroup("진행도/챕터")]
        [LabelText("현재 챕터")]
#endif
        public int currentChapter;

#if ODIN_INSPECTOR
        [HorizontalGroup("진행도", 0.33f)]
        [BoxGroup("진행도/스테이지")]
        [LabelText("현재 스테이지")]
#endif
        public int currentStage;

#if ODIN_INSPECTOR
        [HorizontalGroup("진행도", 0.34f)]
        [BoxGroup("진행도/진행률")]
        [LabelText("전체 진행도")]
        [ProgressBar(0, 100)]
        [Tooltip("0~100 전체 스테이지 진행도. 로비 배경 선택에 사용됩니다 (index = stageProgress / 10).")]
#endif
        public int stageProgress;

#if ODIN_INSPECTOR
        [BoxGroup("진행도")]
        [LabelText("스테이지 인덱스")]
        [HideInInspector]
#endif
        [Tooltip("현재 도달한 스테이지 인덱스 (0-based, 레거시). 기존 RefreshCenterUI 호환용.")]
        public int currentStageIndex;

#if ODIN_INSPECTOR
        [BoxGroup("진행도")]
        [LabelText("캐릭터 인덱스")]
        [HideInInspector]
#endif
        [Tooltip("현재 선택된 캐릭터 인덱스 (0-based, 레거시).")]
        public int currentAgentIndex;

        // ── 스태미나 ──────────────────────────────────────────────
#if ODIN_INSPECTOR
        [Title("스태미나", TitleAlignment = TitleAlignments.Left)]
        [HorizontalGroup("스태미나", 0.5f)]
        [BoxGroup("스태미나/현재")]
        [LabelText("현재 스태미나")]
        [ProgressBar(0, "staminaMax", ColorGetter = "GetStaminaColor")]
#endif
        public int stamina;

#if ODIN_INSPECTOR
        [HorizontalGroup("스태미나", 0.5f)]
        [BoxGroup("스태미나/최대")]
        [LabelText("최대 스태미나")]
#endif
        public int staminaMax;

        // ── 재화 (신규) ───────────────────────────────────────────
#if ODIN_INSPECTOR
        [Title("재화", TitleAlignment = TitleAlignments.Left)]
        [HorizontalGroup("재화", 0.5f)]
        [BoxGroup("재화/골드")]
        [LabelText("골드")]
        [SuffixLabel("G", true)]
#endif
        public long gold;

#if ODIN_INSPECTOR
        [HorizontalGroup("재화", 0.5f)]
        [BoxGroup("재화/젬")]
        [LabelText("젬")]
        [SuffixLabel("GEM", true)]
        [GUIColor(0.8f, 0.4f, 0.9f)]
#endif
        public long gem;

        // ── 재화 (레거시) — IdleRewardManager 등 기존 호환 ────────
#if ODIN_INSPECTOR
        [FoldoutGroup("레거시 재화")]
        [LabelText("티켓")]
        [Tooltip("기존 시스템(IdleRewardManager 등)에서 사용하는 티켓.")]
#endif
        [Header("재화 (레거시)")]
        public int ticket;

#if ODIN_INSPECTOR
        [FoldoutGroup("레거시 재화")]
        [LabelText("다이아")]
        [Tooltip("기존 시스템에서 사용하는 다이아. 신규 코드는 gem 사용.")]
#endif
        [Tooltip("기존 시스템에서 사용하는 다이아. 신규 코드는 gem 사용.")]
        public int diamond;

        // ── 보유 목록 ─────────────────────────────────────────────
#if ODIN_INSPECTOR
        [Title("보유 목록", TitleAlignment = TitleAlignments.Left)]
        [BoxGroup("보유")]
        [LabelText("보유 캐릭터 ID")]
        [ListDrawerSettings(Expanded = false, ShowPaging = true)]
#endif
        [Header("보유 목록")]
        public List<int> ownedCharacterIds = new List<int>();

#if ODIN_INSPECTOR
        [BoxGroup("보유")]
        [LabelText("보유 펫 ID")]
        [ListDrawerSettings(Expanded = false, ShowPaging = true)]
#endif
        public List<int> ownedPetIds = new List<int>();

#if ODIN_INSPECTOR
        [BoxGroup("보유")]
        [LabelText("보유 장비 ID")]
        [ListDrawerSettings(Expanded = false, ShowPaging = true)]
#endif
        public List<int> ownedEquipmentIds = new List<int>();

        // ── 캐릭터 성장 ───────────────────────────────────────────
#if ODIN_INSPECTOR
        [Title("캐릭터 성장", TitleAlignment = TitleAlignments.Left)]
        [BoxGroup("성장")]
        [LabelText("캐릭터 진행 기록")]
        [TableList(ShowIndexLabels = true, AlwaysExpanded = false)]
#endif
        [Header("캐릭터 성장")]
        public List<CharacterProgressRecord> characterProgressRecords = new List<CharacterProgressRecord>();

#if ODIN_INSPECTOR
        [BoxGroup("성장")]
        [LabelText("경험치 아이템")]
        [TableList(ShowIndexLabels = true, AlwaysExpanded = false)]
#endif
        public List<ExpItemInventoryRecord> expItemInventoryRecords = new List<ExpItemInventoryRecord>();

        // ── 미션 상태 ─────────────────────────────────────────────
#if ODIN_INSPECTOR
        [Title("미션 상태", TitleAlignment = TitleAlignments.Left)]
        [BoxGroup("미션")]
        [LabelText("미션 진행 기록")]
        [TableList(ShowIndexLabels = true, AlwaysExpanded = false)]
#endif
        [Header("미션 상태")]
        public List<MissionProgressRecord> missionProgressRecords = new List<MissionProgressRecord>();

#if ODIN_INSPECTOR
        [BoxGroup("미션")]
        [LabelText("미션 티어 보상")]
        [TableList(ShowIndexLabels = true, AlwaysExpanded = false)]
#endif
        public List<MissionTierClaimRecord> missionTierClaimRecords = new List<MissionTierClaimRecord>();

#if ODIN_INSPECTOR
        [BoxGroup("미션")]
        [LabelText("일일 미션 리셋")]
        [ReadOnly]
#endif
        public string lastDailyMissionResetUtc;

#if ODIN_INSPECTOR
        [BoxGroup("미션")]
        [LabelText("주간 미션 리셋")]
        [ReadOnly]
#endif
        public string lastWeeklyMissionResetUtc;

        // ── 방치 보상 ─────────────────────────────────────────────
#if ODIN_INSPECTOR
        [Title("방치 보상", TitleAlignment = TitleAlignments.Left)]
        [BoxGroup("방치")]
        [LabelText("마지막 수령 시간")]
        [ReadOnly]
        [Tooltip("마지막으로 방치 보상을 수령한 UTC 시각 (ISO 8601). 런타임에 자동 갱신됩니다.")]
#endif
        [Header("방치 보상")]
        public string lastIdleRewardTime;

        // ── 이벤트 채널 (VoidEventChannelSO) ─────────────────────
#if ODIN_INSPECTOR
        [Title("이벤트 채널", TitleAlignment = TitleAlignments.Left)]
        [BoxGroup("이벤트")]
        [LabelText("재화 변경")]
        [AssetsOnly]
        [Tooltip("재화 변경 시 발행. LobbyManager 등 UIToolkit 기반 뷰가 구독합니다.")]
#endif
        [Header("이벤트 채널")]
        public VoidEventChannelSO onCurrencyChanged;

#if ODIN_INSPECTOR
        [BoxGroup("이벤트")]
        [LabelText("캐릭터 변경")]
        [AssetsOnly]
        [Tooltip("캐릭터 변경 시 발행.")]
#endif
        public VoidEventChannelSO onCharacterChanged;

#if ODIN_INSPECTOR
        private static Color GetLevelColor() => new Color(0.3f, 0.7f, 1f);
        private static Color GetExpColor() => new Color(0.3f, 0.8f, 0.3f);
        private static Color GetStaminaColor() => new Color(1f, 0.6f, 0.2f);
#endif

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

    // ── 캐릭터 성장 조작 ──────────────────────────────────────

    /// <summary>특정 캐릭터의 현재 레벨을 반환합니다. 기록이 없으면 1.</summary>
    public int GetCharacterLevel(int agentId)
    {
        CharacterProgressRecord rec = characterProgressRecords?.FirstOrDefault(r => r.agentId == agentId);
        return rec?.level ?? 1;
    }

    /// <summary>특정 캐릭터의 현재 경험치를 반환합니다. 기록이 없으면 0.</summary>
    public int GetCharacterExp(int agentId)
    {
        CharacterProgressRecord rec = characterProgressRecords?.FirstOrDefault(r => r.agentId == agentId);
        return rec?.exp ?? 0;
    }

    /// <summary>특정 캐릭터의 레벨과 경험치를 저장합니다.</summary>
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

    // ── 경험치 아이템 인벤토리 ────────────────────────────────

    /// <summary>특정 타입의 경험치 아이템 보유 수량을 반환합니다.</summary>
    public int GetExpItemCount(ExpItemType type)
    {
        ExpItemInventoryRecord rec = expItemInventoryRecords?.FirstOrDefault(r => r.type == type);
        return rec?.count ?? 0;
    }

    /// <summary>특정 타입의 경험치 아이템 보유 수량을 설정합니다.</summary>
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

    /// <summary>모든 경험치 아이템의 총 수량을 반환합니다.</summary>
    public int GetTotalExpItemCount()
    {
        if (expItemInventoryRecords == null) return 0;
        return expItemInventoryRecords.Sum(r => r.count);
    }

    // ── 계정 정보 조작 ────────────────────────────────────────

    /// <summary>닉네임을 반환합니다. 비어있으면 defaultValue를 반환합니다.</summary>
    public string GetNicknameOrDefault(string defaultValue = "Player")
        => string.IsNullOrEmpty(nickname) ? defaultValue : nickname;

    /// <summary>계정 레벨을 반환합니다. 0 이하이면 defaultValue를 반환합니다.</summary>
    public int GetAccountLevel(int defaultValue = 1)
        => accountLevel > 0 ? accountLevel : defaultValue;

    /// <summary>계정 경험치를 반환합니다.</summary>
    public int GetAccountExp() => accountExp;

    /// <summary>계정 최대 경험치를 반환합니다. 0 이하이면 defaultValue를 반환합니다.</summary>
    public int GetAccountExpMax(int defaultValue = 100)
        => accountExpMax > 0 ? accountExpMax : defaultValue;

    /// <summary>계정 레벨·경험치·최대 경험치를 일괄 저장합니다.</summary>
    public void SetAccountStats(int level, int exp, int expMax)
    {
        accountLevel  = Mathf.Max(1, level);
        accountExp    = Mathf.Max(0, exp);
        accountExpMax = Mathf.Max(1, expMax);
    }

    /// <summary>닉네임을 저장합니다.</summary>
    public void SetNicknameValue(string value)
        => nickname = value ?? string.Empty;

    // ── 로그인 / 세션 ─────────────────────────────────────────

    /// <summary>UID를 반환합니다. 비어 있으면 새 GUID를 생성해 저장 후 반환합니다.</summary>
    public string GetUidOrCreate()
    {
        if (string.IsNullOrEmpty(uid))
            uid = Guid.NewGuid().ToString();
        return uid;
    }

    /// <summary>UID를 저장합니다.</summary>
    public void SetUidValue(string value)
        => uid = value ?? string.Empty;

    /// <summary>마지막으로 선택한 서버 ID를 반환합니다.</summary>
    public string GetSelectedServerId() => selectedServerId ?? string.Empty;

    /// <summary>선택한 서버 ID를 저장합니다.</summary>
    public void SetSelectedServerId(string value)
        => selectedServerId = value ?? string.Empty;

    /// <summary>로그인 관련 계정 정보를 초기화합니다 (로그아웃 시 호출).</summary>
    public void ClearLoginState()
    {
        uid              = string.Empty;
        nickname         = string.Empty;
        selectedServerId = string.Empty;
    }

    // ── 보상 지급 ─────────────────────────────────────────────

    /// <summary>
    /// RewardItem을 PlayerData에 적용합니다.
    /// 알 수 없는 itemId/itemName이면 false를 반환합니다.
    /// </summary>
    public bool TryGrantReward(RewardItem reward)
    {
        if (reward == null) return false;

        string name = reward.itemName?.ToLower() ?? string.Empty;

        if (reward.itemId == 1001 || name.Contains("gem") || name.Contains("다이아"))
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
    }   // class PlayerData
}       // namespace ProjectFirst.Data
