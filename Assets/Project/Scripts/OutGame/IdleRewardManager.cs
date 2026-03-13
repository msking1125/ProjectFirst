using System;
using System.Globalization;
using System.Text;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ProjectFirst.Data;

/// <summary>
/// 방치 보상 관리자.
///
/// 흐름:
///   1. Start()에서 오프라인 경과 시간을 계산합니다.
///   2. 경과 시간이 minElapsedSecondsForPopup 이상이면 팝업을 자동 표시합니다.
///   3. 받기 버튼을 누르면 연출 후 우편함 또는 PlayerData에 보상을 지급합니다.
///   4. 지급이 끝나면 기준 시간을 현재 시각으로 초기화합니다.
///
/// 저장 전략:
///   - PlayerPrefs("IdleReward_LastTime") : 실제 영속 저장
///   - PlayerData.lastIdleRewardTime      : 에디터 편의용 미러 데이터
///   두 값 모두 ISO 8601 UTC 문자열로 기록합니다.
/// </summary>
[DisallowMultipleComponent]
public class IdleRewardManager : MonoBehaviour
{
    private const string PrefKey = "IdleReward_LastTime";

    [Header("데이터")]
    [SerializeField] private PlayerData playerData;
    [SerializeField] private IdleRewardConfig config;
    [SerializeField] private MailBox mailBox;

    [Header("팝업 UI")]
    [Tooltip("팝업 루트 오브젝트입니다. 비활성 상태로 시작합니다.")]
    [SerializeField] private GameObject popupRoot;
    [SerializeField] private TMP_Text elapsedTimeText;
    [SerializeField] private TMP_Text rewardGoldText;
    [SerializeField] private TMP_Text rewardStaminaText;
    [SerializeField] private TMP_Text rewardGemText;
    [SerializeField] private Button claimButton;
    [SerializeField] private Button closeButton;

    [Header("연출")]
    [Tooltip("받기 버튼 클릭 후 재생할 연출 오브젝트입니다. Animator나 ParticleSystem을 포함할 수 있습니다.")]
    [SerializeField] private GameObject rewardAnimRoot;

    private struct RewardResult
    {
        public int gold;
        public int stamina;
        public int gem;
        public TimeSpan elapsed;
        public bool IsEmpty => gold == 0 && stamina == 0 && gem == 0;
    }

    private RewardResult pending;
    private bool isClaiming;

    private void Awake()
    {
        if (popupRoot != null)
            popupRoot.SetActive(false);

        if (rewardAnimRoot != null)
            rewardAnimRoot.SetActive(false);

        claimButton?.onClick.AddListener(() => ClaimRewardAsync().Forget());
        closeButton?.onClick.AddListener(ClosePopup);
    }

    private void Start()
    {
        if (string.IsNullOrEmpty(LoadStoredTime()))
            SaveCurrentTime();

        TimeSpan elapsed = CalcElapsed();
        float minSeconds = config != null ? config.minElapsedSecondsForPopup : 60f;
        if (elapsed.TotalSeconds >= minSeconds)
            OpenPopup();
    }

    private void OnApplicationPause(bool pausing)
    {
        if (pausing)
            SaveCurrentTime();
    }

    private void OnApplicationQuit()
    {
        SaveCurrentTime();
    }

    /// <summary>
    /// 방치 보상 팝업을 엽니다.
    /// </summary>
    public void OpenPopup()
    {
        if (isClaiming)
            return;

        pending = CalculateReward();
        RefreshPopupUI();
        popupRoot?.SetActive(true);
    }

    public void ClosePopup()
    {
        if (isClaiming)
            return;

        popupRoot?.SetActive(false);
    }

    /// <summary>
    /// 오프라인 누적 시간에 비례한 보상을 계산합니다.
    /// 최대 maxOfflineHours까지만 인정합니다.
    /// </summary>
    private RewardResult CalculateReward()
    {
        TimeSpan elapsed = CalcElapsed();
        float maxHours = config != null ? config.maxOfflineHours : 12f;
        float cappedSeconds = Mathf.Min((float)elapsed.TotalSeconds, maxHours * 3600f);
        float hours = cappedSeconds / 3600f;

        int goldPerHour = config != null ? config.goldPerHour : 100;
        int staminaPerHour = config != null ? config.staminaPerHour : 0;
        int gemPerHour = config != null ? config.gemPerHour : 0;

        return new RewardResult
        {
            gold = Mathf.FloorToInt(goldPerHour * hours),
            stamina = Mathf.FloorToInt(staminaPerHour * hours),
            gem = Mathf.FloorToInt(gemPerHour * hours),
            elapsed = elapsed,
        };
    }

    private async UniTaskVoid ClaimRewardAsync()
    {
        if (isClaiming)
            return;

        isClaiming = true;
        if (claimButton != null)
            claimButton.interactable = false;

        await PlayAnimationAsync();
        DeliverToMailBox(pending);
        SaveCurrentTime();

        isClaiming = false;
        ClosePopup();
    }

    private void DeliverToMailBox(RewardResult reward)
    {
        if (mailBox != null)
        {
            mailBox.AddMail(
                title: "방치 보상",
                body: BuildMailBody(reward),
                gold: reward.gold,
                stamina: reward.stamina,
                gem: reward.gem);

            Debug.Log($"[IdleRewardManager] 우편 지급 완료 - 골드 {reward.gold:N0} / 스태미나 {reward.stamina:N0} / 잼 {reward.gem:N0}");
            return;
        }

        playerData?.AddGold(reward.gold);
        playerData?.AddCurrency(CurrencyType.Stamina, reward.stamina);
        playerData?.AddGem(reward.gem);
        Debug.LogWarning("[IdleRewardManager] MailBox가 연결되지 않아 PlayerData에 직접 지급했습니다.");
    }

    private async UniTask PlayAnimationAsync()
    {
        float duration = config != null ? config.animDuration : 1.5f;

        if (rewardAnimRoot != null)
        {
            rewardAnimRoot.SetActive(true);
            await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: destroyCancellationToken);
            rewardAnimRoot.SetActive(false);
            return;
        }

        await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: destroyCancellationToken);
    }

    private void RefreshPopupUI()
    {
        if (elapsedTimeText != null)
            elapsedTimeText.text = FormatElapsed(pending.elapsed);
        if (rewardGoldText != null)
            rewardGoldText.text = $"+{pending.gold:N0}";
        if (rewardStaminaText != null)
            rewardStaminaText.text = $"+{pending.stamina:N0}";
        if (rewardGemText != null)
            rewardGemText.text = $"+{pending.gem:N0}";

        if (claimButton != null)
            claimButton.interactable = !pending.IsEmpty;
    }

    /// <summary>
    /// 현재 UTC 시각을 PlayerPrefs와 PlayerData에 함께 저장합니다.
    /// </summary>
    private void SaveCurrentTime()
    {
        string now = DateTime.UtcNow.ToString("o");
        PlayerPrefs.SetString(PrefKey, now);
        PlayerPrefs.Save();

        if (playerData != null)
            playerData.lastIdleRewardTime = now;
    }

    /// <summary>
    /// 저장된 마지막 방치 보상 기준 시각을 읽어옵니다.
    /// </summary>
    private string LoadStoredTime()
    {
        string stored = PlayerPrefs.GetString(PrefKey, string.Empty);
        if (string.IsNullOrEmpty(stored) && playerData != null)
            stored = playerData.lastIdleRewardTime;
        return stored;
    }

    /// <summary>
    /// 마지막 저장 시각부터 현재까지의 경과 시간을 계산합니다.
    /// </summary>
    private TimeSpan CalcElapsed()
    {
        string stored = LoadStoredTime();
        if (string.IsNullOrEmpty(stored))
            return TimeSpan.Zero;

        if (DateTime.TryParse(stored, null, DateTimeStyles.RoundtripKind, out DateTime last))
            return DateTime.UtcNow - last;

        return TimeSpan.Zero;
    }

    private static string FormatElapsed(TimeSpan time)
    {
        if (time.TotalHours >= 1)
            return $"{(int)time.TotalHours}시간 {time.Minutes}분";
        if (time.TotalMinutes >= 1)
            return $"{(int)time.TotalMinutes}분";
        return "방금";
    }

    private static string BuildMailBody(RewardResult reward)
    {
        var sb = new StringBuilder($"{FormatElapsed(reward.elapsed)} 동안의 방치 보상입니다.\n");
        if (reward.gold > 0)
            sb.AppendLine($"골드   +{reward.gold:N0}");
        if (reward.stamina > 0)
            sb.AppendLine($"스태미나 +{reward.stamina:N0}");
        if (reward.gem > 0)
            sb.AppendLine($"잼   +{reward.gem:N0}");
        return sb.ToString().TrimEnd();
    }
}
