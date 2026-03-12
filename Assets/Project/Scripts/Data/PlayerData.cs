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
    /// ?뚮젅?댁뼱??怨꾩젙 ?뺣낫쨌?ы솕쨌吏꾪뻾?꽷룸갑移?蹂댁긽 ??꾩뒪?ы봽瑜?蹂닿??섎뒗 ScriptableObject ?먯뀑.
    /// ?앹꽦: Project ?고겢由???Create/Game/Player Data
    /// 沅뚯옣 寃쎈줈: Assets/Project/Data/PlayerData.asset
    ///
    /// [?ы솕 援ъ“]
    ///   ?좉퇋 ??gold(long), gem(long), stamina/staminaMax(int)
    ///   ?덇굅????ticket(int), diamond(int)  ??IdleRewardManager ??湲곗〈 ?쒖뒪???명솚??
    /// </summary>
#if ODIN_INSPECTOR
    [CreateAssetMenu(menuName = "Soul Ark/Player Data", fileName = "PlayerData")]
#else
    [CreateAssetMenu(menuName = "Game/Player Data", fileName = "PlayerData")]
#endif
    public class PlayerData : ScriptableObject
    {
        // ?? 怨꾩젙 ??????????????????????????????????????????????????
#if ODIN_INSPECTOR
        [Title("怨꾩젙 ?뺣낫", TitleAlignment = TitleAlignments.Left)]
        [BoxGroup("怨꾩젙")]
        [HorizontalGroup("怨꾩젙/Info", 0.5f)]
        [LabelText("UID")]
        [ReadOnly]
#endif
        public string uid;

#if ODIN_INSPECTOR
        [HorizontalGroup("怨꾩젙/Info", 0.5f)]
        [LabelText("?됰꽕??)]
#endif
        public string nickname;

#if ODIN_INSPECTOR
        [HorizontalGroup("怨꾩젙/Level", 0.5f)]
        [LabelText("怨꾩젙 ?덈꺼")]
        [ProgressBar(1, 100, ColorGetter = "GetLevelColor")]
#endif
        public int accountLevel;

#if ODIN_INSPECTOR
        [HorizontalGroup("怨꾩젙/Level", 0.5f)]
        [LabelText("寃쏀뿕移?)]
        [ProgressBar(0, 100, ColorGetter = "GetExpColor")]
#endif
        public int accountExp;

#if ODIN_INSPECTOR
        [BoxGroup("怨꾩젙")]
        [LabelText("理쒕? 寃쏀뿕移?)]
        [HideIf("@accountExpMax <= 0")]
#endif
        public int accountExpMax = 100;

#if ODIN_INSPECTOR
        [BoxGroup("怨꾩젙")]
        [LabelText("?좏깮???쒕쾭")]
#endif
        public string selectedServerId;

        // ?? 罹먮┃??????????????????????????????????????????????????
#if ODIN_INSPECTOR
        [Title("罹먮┃??, TitleAlignment = TitleAlignments.Left)]
        [BoxGroup("罹먮┃??)]
        [LabelText("硫붿씤 罹먮┃??ID")]
        [Tooltip("濡쒕퉬?먯꽌 ?쒖떆??硫붿씤 罹먮┃??ID (AgentTable.id 湲곗?).")]
#endif
        public int mainCharacterId;

        // ?? 吏꾪뻾??????????????????????????????????????????????????
#if ODIN_INSPECTOR
        [Title("吏꾪뻾??, TitleAlignment = TitleAlignments.Left)]
        [HorizontalGroup("吏꾪뻾??, 0.33f)]
        [BoxGroup("吏꾪뻾??梨뺥꽣")]
        [LabelText("?꾩옱 梨뺥꽣")]
#endif
        public int currentChapter;

#if ODIN_INSPECTOR
        [HorizontalGroup("吏꾪뻾??, 0.33f)]
        [BoxGroup("吏꾪뻾???ㅽ뀒?댁?")]
        [LabelText("?꾩옱 ?ㅽ뀒?댁?")]
#endif
        public int currentStage;

#if ODIN_INSPECTOR
        [HorizontalGroup("吏꾪뻾??, 0.34f)]
        [BoxGroup("吏꾪뻾??吏꾪뻾瑜?)]
        [LabelText("?꾩껜 吏꾪뻾??)]
        [ProgressBar(0, 100)]
        [Tooltip("0~100 ?꾩껜 ?ㅽ뀒?댁? 吏꾪뻾?? 濡쒕퉬 諛곌꼍 ?좏깮???ъ슜?⑸땲??(index = stageProgress / 10).")]
#endif
        public int stageProgress;

#if ODIN_INSPECTOR
        [BoxGroup("吏꾪뻾??)]
        [LabelText("?ㅽ뀒?댁? ?몃뜳??)]
        [HideInInspector]
#endif
        [Tooltip("?꾩옱 ?꾨떖???ㅽ뀒?댁? ?몃뜳??(0-based, ?덇굅??. 湲곗〈 RefreshCenterUI ?명솚??")]
        public int currentStageIndex;

#if ODIN_INSPECTOR
        [BoxGroup("吏꾪뻾??)]
        [LabelText("罹먮┃???몃뜳??)]
        [HideInInspector]
#endif
        [Tooltip("?꾩옱 ?좏깮??罹먮┃???몃뜳??(0-based, ?덇굅??.")]
        public int currentAgentIndex;

        // ?? ?ㅽ깭誘몃굹 ??????????????????????????????????????????????
#if ODIN_INSPECTOR
        [Title("?ㅽ깭誘몃굹", TitleAlignment = TitleAlignments.Left)]
        [HorizontalGroup("?ㅽ깭誘몃굹", 0.5f)]
        [BoxGroup("?ㅽ깭誘몃굹/?꾩옱")]
        [LabelText("?꾩옱 ?ㅽ깭誘몃굹")]
        [ProgressBar(0, "staminaMax", ColorGetter = "GetStaminaColor")]
#endif
        public int stamina;

#if ODIN_INSPECTOR
        [HorizontalGroup("?ㅽ깭誘몃굹", 0.5f)]
        [BoxGroup("?ㅽ깭誘몃굹/理쒕?")]
        [LabelText("理쒕? ?ㅽ깭誘몃굹")]
#endif
        public int staminaMax;

        // ?? ?ы솕 (?좉퇋) ???????????????????????????????????????????
#if ODIN_INSPECTOR
        [Title("?ы솕", TitleAlignment = TitleAlignments.Left)]
        [HorizontalGroup("?ы솕", 0.5f)]
        [BoxGroup("?ы솕/怨⑤뱶")]
        [LabelText("怨⑤뱶")]
        [SuffixLabel("G", true)]
#endif
        public long gold;

#if ODIN_INSPECTOR
        [HorizontalGroup("?ы솕", 0.5f)]
        [BoxGroup("?ы솕/??)]
        [LabelText("??)]
        [SuffixLabel("GEM", true)]
        [GUIColor(0.8f, 0.4f, 0.9f)]
#endif
        public long gem;

        // ?? ?ы솕 (?덇굅?? ??IdleRewardManager ??湲곗〈 ?명솚 ????????
#if ODIN_INSPECTOR
        [FoldoutGroup("?덇굅???ы솕")]
        [LabelText("?곗폆")]
        [Tooltip("湲곗〈 ?쒖뒪??IdleRewardManager ???먯꽌 ?ъ슜?섎뒗 ?곗폆.")]
#endif
        [Header("?ы솕 (?덇굅??")]
        public int ticket;

#if ODIN_INSPECTOR
        [FoldoutGroup("?덇굅???ы솕")]
        [LabelText("?ㅼ씠??)]
        [Tooltip("湲곗〈 ?쒖뒪?쒖뿉???ъ슜?섎뒗 ?ㅼ씠?? ?좉퇋 肄붾뱶??gem ?ъ슜.")]
#endif
        public int diamond;

        // ?? 蹂댁쑀 紐⑸줉 ?????????????????????????????????????????????
#if ODIN_INSPECTOR
        [Title("蹂댁쑀 紐⑸줉", TitleAlignment = TitleAlignments.Left)]
        [BoxGroup("蹂댁쑀")]
        [LabelText("蹂댁쑀 罹먮┃??ID")]
        [ListDrawerSettings(Expanded = false, ShowPaging = true)]
#endif
        [Header("蹂댁쑀 紐⑸줉")]
        public List<int> ownedCharacterIds = new List<int>();

#if ODIN_INSPECTOR
        [BoxGroup("蹂댁쑀")]
        [LabelText("蹂댁쑀 ??ID")]
        [ListDrawerSettings(Expanded = false, ShowPaging = true)]
#endif
        public List<int> ownedPetIds = new List<int>();

#if ODIN_INSPECTOR
        [BoxGroup("蹂댁쑀")]
        [LabelText("蹂댁쑀 ?λ퉬 ID")]
        [ListDrawerSettings(Expanded = false, ShowPaging = true)]
#endif
        public List<int> ownedEquipmentIds = new List<int>();

        // ?? 罹먮┃???깆옣 ???????????????????????????????????????????
#if ODIN_INSPECTOR
        [Title("罹먮┃???깆옣", TitleAlignment = TitleAlignments.Left)]
        [BoxGroup("?깆옣")]
        [LabelText("罹먮┃??吏꾪뻾 湲곕줉")]
        [TableList(ShowIndexLabels = true, AlwaysExpanded = false)]
#endif
        [Header("罹먮┃???깆옣")]
        public List<CharacterProgressRecord> characterProgressRecords = new List<CharacterProgressRecord>();

#if ODIN_INSPECTOR
        [BoxGroup("?깆옣")]
        [LabelText("寃쏀뿕移??꾩씠??)]
        [TableList(ShowIndexLabels = true, AlwaysExpanded = false)]
#endif
        public List<ExpItemInventoryRecord> expItemInventoryRecords = new List<ExpItemInventoryRecord>();

        // ?? 誘몄뀡 ?곹깭 ?????????????????????????????????????????????
#if ODIN_INSPECTOR
        [Title("誘몄뀡 ?곹깭", TitleAlignment = TitleAlignments.Left)]
        [BoxGroup("誘몄뀡")]
        [LabelText("誘몄뀡 吏꾪뻾 湲곕줉")]
        [TableList(ShowIndexLabels = true, AlwaysExpanded = false)]
#endif
        [Header("誘몄뀡 ?곹깭")]
        public List<MissionProgressRecord> missionProgressRecords = new List<MissionProgressRecord>();

#if ODIN_INSPECTOR
        [BoxGroup("誘몄뀡")]
        [LabelText("誘몄뀡 ?곗뼱 蹂댁긽")]
        [TableList(ShowIndexLabels = true, AlwaysExpanded = false)]
#endif
        public List<MissionTierClaimRecord> missionTierClaimRecords = new List<MissionTierClaimRecord>();

#if ODIN_INSPECTOR
        [BoxGroup("誘몄뀡")]
        [LabelText("?쇱씪 誘몄뀡 由ъ뀑")]
        [ReadOnly]
#endif
        public string lastDailyMissionResetUtc;

#if ODIN_INSPECTOR
        [BoxGroup("誘몄뀡")]
        [LabelText("二쇨컙 誘몄뀡 由ъ뀑")]
        [ReadOnly]
#endif
        public string lastWeeklyMissionResetUtc;

        // ?? ?쒗넗由ъ뼹 ????????????????????????????????????????????????
#if ODIN_INSPECTOR
        [Title("?쒗넗由ъ뼹", TitleAlignment = TitleAlignments.Left)]
        [BoxGroup("?쒗넗由ъ뼹")]
        [LabelText("?쒗넗由ъ뼹 ?뚮옒洹?)]
        [Tooltip("triggerKey ???꾨즺 ?щ?. TutorialManager?먯꽌 愿由ы빀?덈떎.")]
#endif
        [Header("?쒗넗由ъ뼹")]
        [HideInInspector]
        public List<TutorialFlagEntry> tutorialFlagEntries = new List<TutorialFlagEntry>();

        /// <summary>?고????꾩슜 ?뺤뀛?덈━. Awake/Load ??tutorialFlagEntries濡쒕???援ъ꽦?⑸땲??</summary>
        [NonSerialized]
        public Dictionary<string, bool> TutorialFlags = new Dictionary<string, bool>();

        /// <summary>tutorialFlagEntries ??TutorialFlags ?뺤뀛?덈━濡?蹂?섑빀?덈떎.</summary>
        public void RebuildTutorialFlags()
        {
            TutorialFlags.Clear();
            foreach (TutorialFlagEntry entry in tutorialFlagEntries)
            {
                if (!string.IsNullOrEmpty(entry.key))
                    TutorialFlags[entry.key] = entry.done;
            }
        }

        /// <summary>TutorialFlags ?뺤뀛?덈━ ??tutorialFlagEntries 由ъ뒪?몃줈 ?숆린?뷀빀?덈떎.</summary>
        public void SyncTutorialFlagEntries()
        {
            tutorialFlagEntries.Clear();
            foreach (var kvp in TutorialFlags)
                tutorialFlagEntries.Add(new TutorialFlagEntry { key = kvp.Key, done = kvp.Value });
        }

        // ?? 諛⑹튂 蹂댁긽 ?????????????????????????????????????????????
#if ODIN_INSPECTOR
        [Title("諛⑹튂 蹂댁긽", TitleAlignment = TitleAlignments.Left)]
        [BoxGroup("諛⑹튂")]
        [LabelText("留덉?留??섎졊 ?쒓컙")]
        [ReadOnly]
        [Tooltip("留덉?留됱쑝濡?諛⑹튂 蹂댁긽???섎졊??UTC ?쒓컖 (ISO 8601). ?고??꾩뿉 ?먮룞 媛깆떊?⑸땲??")]
#endif
        [Header("諛⑹튂 蹂댁긽")]
        public string lastIdleRewardTime;

        // ?? ?대깽??梨꾨꼸 (VoidEventChannelSO) ?????????????????????
#if ODIN_INSPECTOR
        [Title("?대깽??梨꾨꼸", TitleAlignment = TitleAlignments.Left)]
        [BoxGroup("?대깽??)]
        [LabelText("?ы솕 蹂寃?)]
        [AssetsOnly]
        [Tooltip("?ы솕 蹂寃???諛쒗뻾. LobbyManager ??UIToolkit 湲곕컲 酉곌? 援щ룆?⑸땲??")]
#endif
        [Header("?대깽??梨꾨꼸")]
        public VoidEventChannelSO onCurrencyChanged;

#if ODIN_INSPECTOR
        [BoxGroup("?대깽??)]
        [LabelText("罹먮┃??蹂寃?)]
        [AssetsOnly]
        [Tooltip("罹먮┃??蹂寃???諛쒗뻾.")]
#endif
        public VoidEventChannelSO onCharacterChanged;

#if ODIN_INSPECTOR
        private static Color GetLevelColor() => new Color(0.3f, 0.7f, 1f);
        private static Color GetExpColor() => new Color(0.3f, 0.8f, 0.3f);
        private static Color GetStaminaColor() => new Color(1f, 0.6f, 0.2f);
#endif

        // ?? ?덇굅???대깽??(Action, 湲곗〈 ?쒖뒪???명솚) ?????????????

        /// <summary>?ы솕 ?섏튂媛 蹂寃쎈맆 ??諛쒗뻾?⑸땲??(湲곗〈 UGUI ?쒖뒪???명솚??.</summary>
        public event Action<CurrencyType> OnCurrencyChanged;

    // ?? ?좉퇋 ?ы솕 議곗옉 ????????????????????????????????????????

    /// <summary>怨⑤뱶瑜?異붽??섍퀬 ?대깽?몃? 諛쒗뻾?⑸땲??</summary>
    public void AddGold(long amount)
    {
        gold = Math.Max(0L, gold + amount);
        RaiseCurrencyEvents(CurrencyType.Gold);
    }

    /// <summary>?ъ쓣 異붽??섍퀬 ?대깽?몃? 諛쒗뻾?⑸땲??</summary>
    public void AddGem(long amount)
    {
        gem = Math.Max(0L, gem + amount);
        RaiseCurrencyEvents(CurrencyType.Diamond);
    }

    /// <summary>怨⑤뱶 李④컧. ?붿븸 遺議???false 諛섑솚.</summary>
    public bool SpendGold(long amount)
    {
        if (gold < amount) return false;
        gold -= amount;
        RaiseCurrencyEvents(CurrencyType.Gold);
        return true;
    }

    // ?? ?덇굅??踰붿슜 ?ы솕 議곗옉 ?????????????????????????????????

    /// <summary>?ы솕 ??낆뿉 ?곕씪 amount留뚰겮 異붽??⑸땲??(?덇굅???명솚).</summary>
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

    /// <summary>?ы솕 ??낆뿉 ?대떦?섎뒗 ?꾩옱 蹂댁쑀?됱쓣 諛섑솚?⑸땲??(?덇굅???명솚).</summary>
    public int GetCurrency(CurrencyType type) => type switch
    {
        CurrencyType.Gold    => (int)Math.Min(gold, int.MaxValue),
        CurrencyType.Diamond => diamond,
        CurrencyType.Ticket  => ticket,
        _                    => 0,
    };

    // ?? ?덇굅??媛쒕퀎 ?ы솕 議곗옉 ?????????????????????????????????

    public void AddTicket(int amount)  => AddCurrency(CurrencyType.Ticket,  amount);
    public void AddDiamond(int amount) => AddCurrency(CurrencyType.Diamond, amount);

    // ?? ?ㅽ깭誘몃굹 議곗옉 ?????????????????????????????????????????

    /// <summary>?ㅽ깭誘몃굹瑜?李④컧?⑸땲?? ?붿븸 遺議???false瑜?諛섑솚?섎ŉ 李④컧?섏? ?딆뒿?덈떎.</summary>
    public bool SpendStamina(int amount)
    {
        if (stamina < amount) return false;
        stamina -= amount;
        return true;
    }

    // ?? 罹먮┃???깆옣 議곗옉 ??????????????????????????????????????

    /// <summary>?뱀젙 罹먮┃?곗쓽 ?꾩옱 ?덈꺼??諛섑솚?⑸땲?? 湲곕줉???놁쑝硫?1.</summary>
    public int GetCharacterLevel(int agentId)
    {
        CharacterProgressRecord rec = characterProgressRecords?.FirstOrDefault(r => r.agentId == agentId);
        return rec?.level ?? 1;
    }

    /// <summary>?뱀젙 罹먮┃?곗쓽 ?꾩옱 寃쏀뿕移섎? 諛섑솚?⑸땲?? 湲곕줉???놁쑝硫?0.</summary>
    public int GetCharacterExp(int agentId)
    {
        CharacterProgressRecord rec = characterProgressRecords?.FirstOrDefault(r => r.agentId == agentId);
        return rec?.exp ?? 0;
    }

    /// <summary>?뱀젙 罹먮┃?곗쓽 ?덈꺼怨?寃쏀뿕移섎? ??ν빀?덈떎.</summary>
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

    // ?? 寃쏀뿕移??꾩씠???몃깽?좊━ ????????????????????????????????

    /// <summary>?뱀젙 ??낆쓽 寃쏀뿕移??꾩씠??蹂댁쑀 ?섎웾??諛섑솚?⑸땲??</summary>
    public int GetExpItemCount(ExpItemType type)
    {
        ExpItemInventoryRecord rec = expItemInventoryRecords?.FirstOrDefault(r => r.type == type);
        return rec?.count ?? 0;
    }

    /// <summary>?뱀젙 ??낆쓽 寃쏀뿕移??꾩씠??蹂댁쑀 ?섎웾???ㅼ젙?⑸땲??</summary>
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

    /// <summary>紐⑤뱺 寃쏀뿕移??꾩씠?쒖쓽 珥??섎웾??諛섑솚?⑸땲??</summary>
    public int GetTotalExpItemCount()
    {
        if (expItemInventoryRecords == null) return 0;
        return expItemInventoryRecords.Sum(r => r.count);
    }

    // ?? 怨꾩젙 ?뺣낫 議곗옉 ????????????????????????????????????????

    /// <summary>?됰꽕?꾩쓣 諛섑솚?⑸땲?? 鍮꾩뼱?덉쑝硫?defaultValue瑜?諛섑솚?⑸땲??</summary>
    public string GetNicknameOrDefault(string defaultValue = "Player")
        => string.IsNullOrEmpty(nickname) ? defaultValue : nickname;

    /// <summary>怨꾩젙 ?덈꺼??諛섑솚?⑸땲?? 0 ?댄븯?대㈃ defaultValue瑜?諛섑솚?⑸땲??</summary>
    public int GetAccountLevel(int defaultValue = 1)
        => accountLevel > 0 ? accountLevel : defaultValue;

    /// <summary>怨꾩젙 寃쏀뿕移섎? 諛섑솚?⑸땲??</summary>
    public int GetAccountExp() => accountExp;

    /// <summary>怨꾩젙 理쒕? 寃쏀뿕移섎? 諛섑솚?⑸땲?? 0 ?댄븯?대㈃ defaultValue瑜?諛섑솚?⑸땲??</summary>
    public int GetAccountExpMax(int defaultValue = 100)
        => accountExpMax > 0 ? accountExpMax : defaultValue;

    /// <summary>怨꾩젙 ?덈꺼쨌寃쏀뿕移샕룹턀? 寃쏀뿕移섎? ?쇨큵 ??ν빀?덈떎.</summary>
    public void SetAccountStats(int level, int exp, int expMax)
    {
        accountLevel  = Mathf.Max(1, level);
        accountExp    = Mathf.Max(0, exp);
        accountExpMax = Mathf.Max(1, expMax);
    }

    /// <summary>?됰꽕?꾩쓣 ??ν빀?덈떎.</summary>
    public void SetNicknameValue(string value)
        => nickname = value ?? string.Empty;

    // ?? 濡쒓렇??/ ?몄뀡 ?????????????????????????????????????????

    /// <summary>UID瑜?諛섑솚?⑸땲?? 鍮꾩뼱 ?덉쑝硫???GUID瑜??앹꽦???????諛섑솚?⑸땲??</summary>
    public string GetUidOrCreate()
    {
        if (string.IsNullOrEmpty(uid))
            uid = Guid.NewGuid().ToString();
        return uid;
    }

    /// <summary>UID瑜???ν빀?덈떎.</summary>
    public void SetUidValue(string value)
        => uid = value ?? string.Empty;

    /// <summary>留덉?留됱쑝濡??좏깮???쒕쾭 ID瑜?諛섑솚?⑸땲??</summary>
    public string GetSelectedServerId() => selectedServerId ?? string.Empty;

    /// <summary>?좏깮???쒕쾭 ID瑜???ν빀?덈떎.</summary>
    public void SetSelectedServerId(string value)
        => selectedServerId = value ?? string.Empty;

    /// <summary>濡쒓렇??愿??怨꾩젙 ?뺣낫瑜?珥덇린?뷀빀?덈떎 (濡쒓렇?꾩썐 ???몄텧).</summary>
    public void ClearLoginState()
    {
        uid              = string.Empty;
        nickname         = string.Empty;
        selectedServerId = string.Empty;
    }

    // ?? 蹂댁긽 吏湲??????????????????????????????????????????????

    /// <summary>
    /// RewardItem??PlayerData???곸슜?⑸땲??
    /// ?????녿뒗 itemId/itemName?대㈃ false瑜?諛섑솚?⑸땲??
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

    // ?? 諛⑹튂 蹂댁긽 ?좏떥 ????????????????????????????????????????

    /// <summary>留덉?留??섎졊 ?댄썑 寃쎄낵 ?쒓컙??諛섑솚?⑸땲?? 湲곕줉???놁쑝硫?TimeSpan.Zero.</summary>
    public TimeSpan GetIdleElapsed()
    {
        if (string.IsNullOrEmpty(lastIdleRewardTime))
            return TimeSpan.Zero;

        if (DateTime.TryParse(lastIdleRewardTime, null,
                System.Globalization.DateTimeStyles.RoundtripKind, out DateTime last))
            return DateTime.UtcNow - last;

        return TimeSpan.Zero;
    }

    /// <summary>諛⑹튂 蹂댁긽 ?섎졊 ?쒓컖???꾩옱 UTC濡?湲곕줉?⑸땲??</summary>
    public void MarkIdleRewardClaimed()
    {
        lastIdleRewardTime = DateTime.UtcNow.ToString("o");
    }

    // ?? ?대? ?ы띁 ????????????????????????????????????????????

    private void RaiseCurrencyEvents(CurrencyType type)
    {
        onCurrencyChanged?.RaiseEvent();
        OnCurrencyChanged?.Invoke(type);
    }

    // ?? ?쒗넗由ъ뼹 ?뚮옒洹?吏곷젹?????????????????????????????????

    /// <summary>?쒗넗由ъ뼹 ?뚮옒洹몄쓽 吏곷젹?붿슜 ?뷀듃由? Dictionary ???List濡???ν빀?덈떎.</summary>
    [Serializable]
    public class TutorialFlagEntry
    {
        public string key;
        public bool done;
    }
}
}

