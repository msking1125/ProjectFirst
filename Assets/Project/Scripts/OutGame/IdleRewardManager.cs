using System;
using System.Globalization;
using System.Text;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 방치 보상 관리자.
///
/// 흐름:
///   1. Start() → 오프라인 경과 시간 계산
///   2. 경과 시간 ≥ minElapsedSecondsForPopup → 팝업 자동 표시
///   3. 방치 보상 버튼 터치 → LobbyManager.OnIdleRewardClicked() → OpenPopup()
///   4. 받기 버튼 → ClaimRewardAsync() → 연출 → MailBox 지급 → 시간 초기화
///
/// 저장 전략:
///   - PlayerPrefs ("IdleReward_LastTime") : 빌드 환경에서 세션 간 실제 영속
///   - PlayerData.lastIdleRewardTime       : SO 인스턴스 내 미러 (에디터 편의용)
///   양쪽 모두 ISO 8601 UTC 문자열로 기록합니다.
///
/// [Inspector 연결 가이드]
///   Data      : playerData, config, mailBox
///   Popup UI  : popupRoot, elapsedTimeText, rewardGoldText,
///               rewardTicketText, rewardDiamondText,
///               claimButton, closeButton
///   Animation : rewardAnimRoot (Animator / 파티클 포함 GameObject)
/// </summary>
[DisallowMultipleComponent]
public class IdleRewardManager : MonoBehaviour
{
    // PlayerPrefs 키
    private const string PrefKey = "IdleReward_LastTime";

    // ── Data ──────────────────────────────────────────────────

    [Header("Data")]
    [SerializeField] private PlayerData playerData;
    [SerializeField] private IdleRewardConfig config;
    [SerializeField] private MailBox mailBox;

    // ── Popup UI ──────────────────────────────────────────────

    [Header("Popup UI")]
    [Tooltip("팝업 루트 오브젝트. 비활성 상태로 시작합니다.")]
    [SerializeField] private GameObject popupRoot;
    [SerializeField] private TMP_Text elapsedTimeText;
    [SerializeField] private TMP_Text rewardGoldText;
    [SerializeField] private TMP_Text rewardTicketText;
    [SerializeField] private TMP_Text rewardDiamondText;
    [SerializeField] private Button claimButton;
    [SerializeField] private Button closeButton;

    // ── 보상 연출 ─────────────────────────────────────────────

    [Header("Animation")]
    [Tooltip("받기 클릭 후 재생할 연출 GameObject (Animator / ParticleSystem 등을 포함). " +
             "비활성 상태로 시작하며, 연출 시간 후 자동 비활성화됩니다.")]
    [SerializeField] private GameObject rewardAnimRoot;

    // ── 내부 상태 ─────────────────────────────────────────────

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

    // ─────────────────────────────────────────────────────────

    private void Awake()
    {
        if (popupRoot      != null) popupRoot.SetActive(false);
        if (rewardAnimRoot != null) rewardAnimRoot.SetActive(false);

        claimButton?.onClick.AddListener(() => ClaimRewardAsync().Forget());
        closeButton?.onClick.AddListener(ClosePopup);
    }

    private void Start()
    {
        // 최초 실행이면 현재 시각을 기준으로 초기화
        if (string.IsNullOrEmpty(LoadStoredTime()))
            SaveCurrentTime();

        // 로비 진입 시 보상이 충분히 쌓였으면 자동 팝업
        TimeSpan elapsed = CalcElapsed();
        float minSec = config != null ? config.minElapsedSecondsForPopup : 60f;
        if (elapsed.TotalSeconds >= minSec)
            OpenPopup();
    }

    private void OnApplicationPause(bool pausing)
    {
        // 앱이 백그라운드로 전환될 때 현재 시각 저장
        if (pausing) SaveCurrentTime();
    }

    private void OnApplicationQuit()
    {
        SaveCurrentTime();
    }

    // ── Public API ────────────────────────────────────────────

    /// <summary>LobbyManager 또는 외부에서 팝업을 엽니다.</summary>
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

    // ── 보상 계산 ─────────────────────────────────────────────

    /// <summary>
    /// 계산식: elapsed(초) × (단위보상 / 3600) = 총 보상
    /// 최대 maxOfflineHours 시간까지만 인정합니다.
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

    // ── 수령 ─────────────────────────────────────────────────

    private async UniTaskVoid ClaimRewardAsync()
    {
        if (_isClaiming) return;
        _isClaiming = true;
        if (claimButton != null) claimButton.interactable = false;

        // 보상 연출 (rewardAnimRoot에 Animator / Particle 연결)
        await PlayAnimationAsync();

        // 우편 지급
        DeliverToMailBox(_pending);

        // 기준 시각 초기화 (다음 오프라인 카운트 시작점)
        SaveCurrentTime();

        _isClaiming = false;
        ClosePopup();
    }

    private void DeliverToMailBox(RewardResult r)
    {
        if (mailBox != null)
        {
            // 우편함에 추가 (우편 UI에서 수령 처리)
            mailBox.AddMail(
                title:   "방치 보상",
                body:    BuildMailBody(r),
                gold:    r.gold,
                ticket:  r.ticket,
                diamond: r.diamond);

            Debug.Log($"[IdleRewardManager] 우편 지급 완료 — " +
                      $"골드 {r.gold:N0} / 티켓 {r.ticket:N0} / 다이아 {r.diamond:N0}");
        }
        else
        {
            // MailBox 미연결 시 PlayerData에 직접 지급 (폴백)
            playerData?.AddGold(r.gold);
            playerData?.AddTicket(r.ticket);
            playerData?.AddDiamond(r.diamond);
            Debug.LogWarning("[IdleRewardManager] MailBox가 연결되지 않아 PlayerData에 직접 지급했습니다.");
        }
    }

    // ── 보상 연출 ─────────────────────────────────────────────

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

    // ── UI 갱신 ───────────────────────────────────────────────

    private void RefreshPopupUI()
    {
        if (elapsedTimeText   != null) elapsedTimeText.text   = FormatElapsed(_pending.elapsed);
        if (rewardGoldText    != null) rewardGoldText.text    = $"+{_pending.gold:N0}";
        if (rewardTicketText  != null) rewardTicketText.text  = $"+{_pending.ticket:N0}";
        if (rewardDiamondText != null) rewardDiamondText.text = $"+{_pending.diamond:N0}";

        if (claimButton != null)
            claimButton.interactable = !_pending.IsEmpty;
    }

    // ── 시간 저장 / 로드 ──────────────────────────────────────

    /// <summary>
    /// 현재 UTC 시각을 PlayerPrefs 와 PlayerData 양쪽에 기록합니다.
    /// </summary>
    private void SaveCurrentTime()
    {
        string now = DateTime.UtcNow.ToString("o");
        PlayerPrefs.SetString(PrefKey, now);
        PlayerPrefs.Save();

        if (playerData != null)
            playerData.lastIdleRewardTime = now;
    }

    /// <summary>PlayerPrefs → PlayerData 순서로 저장된 시각을 읽습니다.</summary>
    private string LoadStoredTime()
    {
        string stored = PlayerPrefs.GetString(PrefKey, string.Empty);
        if (string.IsNullOrEmpty(stored) && playerData != null)
            stored = playerData.lastIdleRewardTime;
        return stored;
    }

    /// <summary>마지막 저장 시각부터 현재까지의 경과 시간을 반환합니다.</summary>
    private TimeSpan CalcElapsed()
    {
        string stored = LoadStoredTime();
        if (string.IsNullOrEmpty(stored))
            return TimeSpan.Zero;

        if (DateTime.TryParse(stored, null, DateTimeStyles.RoundtripKind, out DateTime last))
            return DateTime.UtcNow - last;

        return TimeSpan.Zero;
    }

    // ── 유틸 ─────────────────────────────────────────────────

    private static string FormatElapsed(TimeSpan t)
    {
        if (t.TotalHours  >= 1) return $"{(int)t.TotalHours}시간 {t.Minutes}분";
        if (t.TotalMinutes >= 1) return $"{(int)t.TotalMinutes}분";
        return "방금";
    }

    private static string BuildMailBody(RewardResult r)
    {
        var sb = new StringBuilder($"{FormatElapsed(r.elapsed)} 동안의 방치 보상입니다.\n");
        if (r.gold    > 0) sb.AppendLine($"골드   +{r.gold:N0}");
        if (r.ticket  > 0) sb.AppendLine($"티켓   +{r.ticket:N0}");
        if (r.diamond > 0) sb.AppendLine($"다이아 +{r.diamond:N0}");
        return sb.ToString().TrimEnd();
    }
}
