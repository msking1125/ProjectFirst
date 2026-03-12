using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ProjectFirst.Data;
using Cysharp.Threading.Tasks;
/// <summary>
/// 諛⑹튂 蹂댁긽 愿由ъ옄.
///
/// ?먮쫫:
///   1. Start() ???ㅽ봽?쇱씤 寃쎄낵 ?쒓컙 怨꾩궛
///   2. 寃쎄낵 ?쒓컙 ??minElapsedSecondsForPopup ???앹뾽 ?먮룞 ?쒖떆
///   3. 諛⑹튂 蹂댁긽 踰꾪듉 ?곗튂 ??LobbyManager.OnIdleRewardClicked() ??OpenPopup()
///   4. 諛쏄린 踰꾪듉 ??ClaimRewardAsync() ???곗텧 ??MailBox 吏湲????쒓컙 珥덇린??
///
/// ????꾨왂:
///   - PlayerPrefs ("IdleReward_LastTime") : 鍮뚮뱶 ?섍꼍?먯꽌 ?몄뀡 媛??ㅼ젣 ?곸냽
///   - PlayerData.lastIdleRewardTime       : SO ?몄뒪?댁뒪 ??誘몃윭 (?먮뵒???몄쓽??
///   ?묒そ 紐⑤몢 ISO 8601 UTC 臾몄옄?대줈 湲곕줉?⑸땲??
///
/// [Inspector ?곌껐 媛?대뱶]
///   Data      : playerData, config, mailBox
///   Popup UI  : popupRoot, elapsedTimeText, rewardGoldText,
///               rewardTicketText, rewardDiamondText,
///               claimButton, closeButton
///   Animation : rewardAnimRoot (Animator / ?뚰떚???ы븿 GameObject)
/// </summary>
[DisallowMultipleComponent]
public class IdleRewardManager : MonoBehaviour
{
    // PlayerPrefs ??
    private const string PrefKey = "IdleReward_LastTime";

    // ?? Data ??????????????????????????????????????????????????

    [Header("Data")]
    [SerializeField] private PlayerData playerData;
    [SerializeField] private IdleRewardConfig config;
    [SerializeField] private MailBox mailBox;

    // ?? Popup UI ??????????????????????????????????????????????

    [Header("Popup UI")]
    [Tooltip("?앹뾽 猷⑦듃 ?ㅻ툕?앺듃. 鍮꾪솢???곹깭濡??쒖옉?⑸땲??")]
    [SerializeField] private GameObject popupRoot;
    [SerializeField] private TMP_Text elapsedTimeText;
    [SerializeField] private TMP_Text rewardGoldText;
    [SerializeField] private TMP_Text rewardTicketText;
    [SerializeField] private TMP_Text rewardDiamondText;
    [SerializeField] private Button claimButton;
    [SerializeField] private Button closeButton;

    // ?? 蹂댁긽 ?곗텧 ?????????????????????????????????????????????

    [Header("Animation")]
    [Tooltip("諛쏄린 ?대┃ ???ъ깮???곗텧 GameObject (Animator / ParticleSystem ?깆쓣 ?ы븿). " +
             "鍮꾪솢???곹깭濡??쒖옉?섎ŉ, ?곗텧 ?쒓컙 ???먮룞 鍮꾪솢?깊솕?⑸땲??")]
    [SerializeField] private GameObject rewardAnimRoot;

    // ?? ?대? ?곹깭 ?????????????????????????????????????????????

    private struct RewardResult
    {
        public int gold;
        public int ticket;
        public int diamond;
        public TimeSpan elapsed;
        public bool IsEmpty => gold == 0 && ticket == 0 && diamond == 0;
    }

    private RewardResult _pending;
    private bool _isClaiming;

    // ?????????????????????????????????????????????????????????

    private void Awake()
    {
        if (popupRoot      != null) popupRoot.SetActive(false);
        if (rewardAnimRoot != null) rewardAnimRoot.SetActive(false);

        claimButton?.onClick.AddListener(() => ClaimRewardAsync().Forget());
        closeButton?.onClick.AddListener(ClosePopup);
    }

    private void Start()
    {
        // 理쒖큹 ?ㅽ뻾?대㈃ ?꾩옱 ?쒓컖??湲곗??쇰줈 珥덇린??
        if (string.IsNullOrEmpty(LoadStoredTime()))
            SaveCurrentTime();

        // 濡쒕퉬 吏꾩엯 ??蹂댁긽??異⑸텇???볦??쇰㈃ ?먮룞 ?앹뾽
        TimeSpan elapsed = CalcElapsed();
        float minSec = config != null ? config.minElapsedSecondsForPopup : 60f;
        if (elapsed.TotalSeconds >= minSec)
            OpenPopup();
    }

    private void OnApplicationPause(bool pausing)
    {
        // ?깆씠 諛깃렇?쇱슫?쒕줈 ?꾪솚?????꾩옱 ?쒓컖 ???
        if (pausing) SaveCurrentTime();
    }

    private void OnApplicationQuit()
    {
        SaveCurrentTime();
    }

    // ?? Public API ????????????????????????????????????????????

    /// <summary>LobbyManager ?먮뒗 ?몃??먯꽌 ?앹뾽???쎈땲??</summary>
    public void OpenPopup()
    {
        if (_isClaiming) return;

        _pending = CalculateReward();
        RefreshPopupUI();
        popupRoot?.SetActive(true);
    }

    public void ClosePopup()
    {
        if (_isClaiming) return;
        popupRoot?.SetActive(false);
    }

    // ?? 蹂댁긽 怨꾩궛 ?????????????????????????????????????????????

    /// <summary>
    /// 怨꾩궛?? elapsed(珥? 횞 (?⑥쐞蹂댁긽 / 3600) = 珥?蹂댁긽
    /// 理쒕? maxOfflineHours ?쒓컙源뚯?留??몄젙?⑸땲??
    /// </summary>
    private RewardResult CalculateReward()
    {
        TimeSpan elapsed   = CalcElapsed();
        float maxHours     = config != null ? config.maxOfflineHours : 12f;
        float cappedSec    = Mathf.Min((float)elapsed.TotalSeconds, maxHours * 3600f);
        float hours        = cappedSec / 3600f;

        int gph = config != null ? config.goldPerHour    : 100;
        int tph = config != null ? config.ticketPerHour  : 0;
        int dph = config != null ? config.diamondPerHour : 0;

        return new RewardResult
        {
            gold    = Mathf.FloorToInt(gph * hours),
            ticket  = Mathf.FloorToInt(tph * hours),
            diamond = Mathf.FloorToInt(dph * hours),
            elapsed = elapsed,
        };
    }

    // ?? ?섎졊 ?????????????????????????????????????????????????

    private async UniTaskVoid ClaimRewardAsync()
    {
        if (_isClaiming) return;
        _isClaiming = true;
        if (claimButton != null) claimButton.interactable = false;

        // 蹂댁긽 ?곗텧 (rewardAnimRoot??Animator / Particle ?곌껐)
        await PlayAnimationAsync();

        // ?고렪 吏湲?
        DeliverToMailBox(_pending);

        // 湲곗? ?쒓컖 珥덇린??(?ㅼ쓬 ?ㅽ봽?쇱씤 移댁슫???쒖옉??
        SaveCurrentTime();

        _isClaiming = false;
        ClosePopup();
    }

    private void DeliverToMailBox(RewardResult r)
    {
        if (mailBox != null)
        {
            // ?고렪?⑥뿉 異붽? (?고렪 UI?먯꽌 ?섎졊 泥섎━)
            mailBox.AddMail(
                title:   "諛⑹튂 蹂댁긽",
                body:    BuildMailBody(r),
                gold:    r.gold,
                ticket:  r.ticket,
                diamond: r.diamond);

            Debug.Log($"[IdleRewardManager] ?고렪 吏湲??꾨즺 ??" +
                      $"怨⑤뱶 {r.gold:N0} / ?곗폆 {r.ticket:N0} / ?ㅼ씠??{r.diamond:N0}");
        }
        else
        {
            // MailBox 誘몄뿰寃???PlayerData??吏곸젒 吏湲?(?대갚)
            playerData?.AddGold(r.gold);
            playerData?.AddTicket(r.ticket);
            playerData?.AddDiamond(r.diamond);
            Debug.LogWarning("[IdleRewardManager] MailBox媛 ?곌껐?섏? ?딆븘 PlayerData??吏곸젒 吏湲됲뻽?듬땲??");
        }
    }

    // ?? 蹂댁긽 ?곗텧 ?????????????????????????????????????????????

    private async UniTask PlayAnimationAsync()
    {
        float duration = config != null ? config.animDuration : 1.5f;

        if (rewardAnimRoot != null)
        {
            rewardAnimRoot.SetActive(true);
            await UniTask.Delay(
                TimeSpan.FromSeconds(duration),
                cancellationToken: destroyCancellationToken);
            rewardAnimRoot.SetActive(false);
        }
        else
        {
            await UniTask.Delay(
                TimeSpan.FromSeconds(duration),
                cancellationToken: destroyCancellationToken);
        }
    }

    // ?? UI 媛깆떊 ???????????????????????????????????????????????

    private void RefreshPopupUI()
    {
        if (elapsedTimeText   != null) elapsedTimeText.text   = FormatElapsed(_pending.elapsed);
        if (rewardGoldText    != null) rewardGoldText.text    = $"+{_pending.gold:N0}";
        if (rewardTicketText  != null) rewardTicketText.text  = $"+{_pending.ticket:N0}";
        if (rewardDiamondText != null) rewardDiamondText.text = $"+{_pending.diamond:N0}";

        if (claimButton != null)
            claimButton.interactable = !_pending.IsEmpty;
    }

    // ?? ?쒓컙 ???/ 濡쒕뱶 ??????????????????????????????????????

    /// <summary>
    /// ?꾩옱 UTC ?쒓컖??PlayerPrefs ? PlayerData ?묒そ??湲곕줉?⑸땲??
    /// </summary>
    private void SaveCurrentTime()
    {
        string now = DateTime.UtcNow.ToString("o");
        PlayerPrefs.SetString(PrefKey, now);
        PlayerPrefs.Save();

        if (playerData != null)
            playerData.lastIdleRewardTime = now;
    }

    /// <summary>PlayerPrefs ??PlayerData ?쒖꽌濡???λ맂 ?쒓컖???쎌뒿?덈떎.</summary>
    private string LoadStoredTime()
    {
        string stored = PlayerPrefs.GetString(PrefKey, string.Empty);
        if (string.IsNullOrEmpty(stored) && playerData != null)
            stored = playerData.lastIdleRewardTime;
        return stored;
    }

    /// <summary>留덉?留?????쒓컖遺???꾩옱源뚯???寃쎄낵 ?쒓컙??諛섑솚?⑸땲??</summary>
    private TimeSpan CalcElapsed()
    {
        string stored = LoadStoredTime();
        if (string.IsNullOrEmpty(stored))
            return TimeSpan.Zero;

        if (DateTime.TryParse(stored, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime last))
            return DateTime.UtcNow - last;

        return TimeSpan.Zero;
    }

    // ?? ?좏떥 ?????????????????????????????????????????????????

    private static string FormatElapsed(TimeSpan t)
    {
        if (t.TotalHours >= 1) return $"{(int)t.TotalHours}h {t.Minutes}m";
        if (t.TotalMinutes >= 1) return $"{(int)t.TotalMinutes}m";
        return "Just now";
    }

    private static string BuildMailBody(RewardResult r)
    {
        var sb = new StringBuilder($"{FormatElapsed(r.elapsed)} ?숈븞??諛⑹튂 蹂댁긽?낅땲??\n");
        if (r.gold    > 0) sb.AppendLine($"怨⑤뱶   +{r.gold:N0}");
        if (r.ticket  > 0) sb.AppendLine($"?곗폆   +{r.ticket:N0}");
        if (r.diamond > 0) sb.AppendLine($"?ㅼ씠??+{r.diamond:N0}");
        return sb.ToString().TrimEnd();
    }
}






