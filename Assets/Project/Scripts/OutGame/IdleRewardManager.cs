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
/// Documentation cleaned.
///
/// Documentation cleaned.
/// Documentation cleaned.
/// Documentation cleaned.
/// Documentation cleaned.
/// Documentation cleaned.
///
/// Documentation cleaned.
/// Documentation cleaned.
/// Documentation cleaned.
/// Documentation cleaned.
///
/// Documentation cleaned.
///   Data      : playerData, config, mailBox
///   Popup UI  : popupRoot, elapsedTimeText, rewardGoldText,
///               rewardTicketText, rewardDiamondText,
///               claimButton, closeButton
/// Documentation cleaned.
/// </summary>
[DisallowMultipleComponent]
public class IdleRewardManager : MonoBehaviour
{
    // Note: cleaned comment.
    private const string PrefKey = "IdleReward_LastTime";

    // Note: cleaned comment.

    [Header("Data")]
    [SerializeField] private PlayerData playerData;
    [SerializeField] private IdleRewardConfig config;
    [SerializeField] private MailBox mailBox;

    // Note: cleaned comment.

    [Header("Popup UI")]
    [Tooltip("Configured in inspector.")]
    [SerializeField] private GameObject popupRoot;
    [SerializeField] private TMP_Text elapsedTimeText;
    [SerializeField] private TMP_Text rewardGoldText;
    [SerializeField] private TMP_Text rewardTicketText;
    [SerializeField] private TMP_Text rewardDiamondText;
    [SerializeField] private Button claimButton;
    [SerializeField] private Button closeButton;

    // Note: cleaned comment.

    [Header("Animation")]
    [Tooltip("Effect object played when the reward is claimed (Animator or ParticleSystem can be included). " +
             "It should start inactive and will be disabled automatically after the effect duration.")]
    [SerializeField] private GameObject rewardAnimRoot;

    // Note: cleaned comment.

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

    // Note: cleaned comment.

    private void Awake()
    {
        if (popupRoot      != null) popupRoot.SetActive(false);
        if (rewardAnimRoot != null) rewardAnimRoot.SetActive(false);

        claimButton?.onClick.AddListener(() => ClaimRewardAsync().Forget());
        closeButton?.onClick.AddListener(ClosePopup);
    }

    private void Start()
    {
        // Note: cleaned comment.
        if (string.IsNullOrEmpty(LoadStoredTime()))
            SaveCurrentTime();

        // Note: cleaned comment.
        TimeSpan elapsed = CalcElapsed();
        float minSec = config != null ? config.minElapsedSecondsForPopup : 60f;
        if (elapsed.TotalSeconds >= minSec)
            OpenPopup();
    }

    private void OnApplicationPause(bool pausing)
    {
        // Note: cleaned comment.
        if (pausing) SaveCurrentTime();
    }

    private void OnApplicationQuit()
    {
        SaveCurrentTime();
    }

    // Note: cleaned comment.

    /// Documentation cleaned.
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

    // Note: cleaned comment.

    /// <summary>
    /// Documentation cleaned.
    /// Documentation cleaned.
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

    // Note: cleaned comment.

    private async UniTaskVoid ClaimRewardAsync()
    {
        if (_isClaiming) return;
        _isClaiming = true;
        if (claimButton != null) claimButton.interactable = false;

        // Note: cleaned comment.
        await PlayAnimationAsync();

        // Note: cleaned comment.
        DeliverToMailBox(_pending);

        // Note: cleaned comment.
        SaveCurrentTime();

        _isClaiming = false;
        ClosePopup();
    }

    private void DeliverToMailBox(RewardResult r)
    {
        if (mailBox != null)
        {
            // Note: cleaned comment.
            mailBox.AddMail(
                title:   "방치 보상",
                body:    BuildMailBody(r),
                gold:    r.gold,
                ticket:  r.ticket,
                diamond: r.diamond);

            Debug.Log("[Log] Message cleaned.");
            // Note: removed broken continuation.
        }
        else
        {
            // Note: cleaned comment.
            playerData?.AddGold(r.gold);
            playerData?.AddTicket(r.ticket);
            playerData?.AddDiamond(r.diamond);
            Debug.LogWarning("[Log] Warning message cleaned.");
        }
    }

    // Note: cleaned comment.

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

    // Note: cleaned comment.

    private void RefreshPopupUI()
    {
        if (elapsedTimeText   != null) elapsedTimeText.text   = FormatElapsed(_pending.elapsed);
        if (rewardGoldText    != null) rewardGoldText.text    = $"+{_pending.gold:N0}";
        if (rewardTicketText  != null) rewardTicketText.text  = $"+{_pending.ticket:N0}";
        if (rewardDiamondText != null) rewardDiamondText.text = $"+{_pending.diamond:N0}";

        if (claimButton != null)
            claimButton.interactable = !_pending.IsEmpty;
    }

    // Note: cleaned comment.

    /// <summary>
    /// Documentation cleaned.
    /// </summary>
    private void SaveCurrentTime()
    {
        string now = DateTime.UtcNow.ToString("o");
        PlayerPrefs.SetString(PrefKey, now);
        PlayerPrefs.Save();

        if (playerData != null)
            playerData.lastIdleRewardTime = now;
    }

    /// Documentation cleaned.
    private string LoadStoredTime()
    {
        string stored = PlayerPrefs.GetString(PrefKey, string.Empty);
        if (string.IsNullOrEmpty(stored) && playerData != null)
            stored = playerData.lastIdleRewardTime;
        return stored;
    }

    /// Documentation cleaned.
    private TimeSpan CalcElapsed()
    {
        string stored = LoadStoredTime();
        if (string.IsNullOrEmpty(stored))
            return TimeSpan.Zero;

        if (DateTime.TryParse(stored, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime last))
            return DateTime.UtcNow - last;

        return TimeSpan.Zero;
    }

    // Note: cleaned comment.

    private static string FormatElapsed(TimeSpan t)
    {
        if (t.TotalHours >= 1) return $"{(int)t.TotalHours}h {t.Minutes}m";
        if (t.TotalMinutes >= 1) return $"{(int)t.TotalMinutes}m";
        return "Just now";
    }

    private static string BuildMailBody(RewardResult r)
    {
        var sb = new StringBuilder($"Idle reward accumulated for {FormatElapsed(r.elapsed)}.\n");
        if (r.gold    > 0) sb.AppendLine($"怨⑤뱶   +{r.gold:N0}");
        if (r.ticket  > 0) sb.AppendLine($"?곗폆   +{r.ticket:N0}");
        if (r.diamond > 0) sb.AppendLine($"?ㅼ씠??+{r.diamond:N0}");
        return sb.ToString().TrimEnd();
    }
}






