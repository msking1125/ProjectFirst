using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using ProjectFirst.Data;

/// <summary>
/// UI Toolkit 기반 우편함 패널입니다.
///
/// 기능:
/// - 수령 가능 / 수령 완료 우편 탭 전환
/// - 우편 목록 및 상세 내용 표시
/// - 개별 수령 / 전체 수령 / 읽은 우편 삭제
/// - 보상 획득 연출 및 확인 팝업 표시
/// </summary>
[DisallowMultipleComponent]
public class MailboxPanel : MonoBehaviour
{
    public static MailboxPanel Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private UIDocument uiDocument;

    [Header("Data")]
    [SerializeField] private PlayerData playerData;
    [SerializeField] private MailCatalogSO mailCatalog;

    private List<MailData> _allMails = new List<MailData>();
    private MailData _selectedMail;
    private bool _showingUnclaimed = true; // true: 수령 가능 우편 표시, false: 수령 완료 우편 표시

    // Root
    private VisualElement _root;
    private VisualElement _mailboxPanel;

    // Header
    private Label  _titleLabel;
    private Button _closeBtn;

    // Tabs
    private Button _unclaimedTabBtn;
    private Button _claimedTabBtn;
    private Label  _unclaimedBadge;

    // List
    private ListView      _mailListView;
    private VisualElement _emptyStateView;
    private Label         _emptyLabel;

    // Detail
    private VisualElement _detailPanel;
    private Label  _detailTitle;
    private Label  _detailSender;
    private Label  _detailDate;
    private Label  _detailBody;
    private VisualElement _detailRewardsRow;
    private Button _detailClaimBtn;

    // Bottom
    private Button _claimAllBtn;
    private Button _deleteReadBtn;

    // Reward Overlay
    private VisualElement _rewardOverlay;
    private VisualElement _rewardCardsContainer;
    private Label         _rewardTitleLabel;
    private Label         _rewardTouchLabel;

    // Confirm Dialog
    private VisualElement _confirmOverlay;
    private Label         _confirmMessage;
    private Button        _confirmYesBtn;
    private Button        _confirmNoBtn;
    private List<MailData> _filteredMails = new List<MailData>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        BindUI();
    }
    //  Public API
    public void Show()
    {
        if (_mailboxPanel == null) BindUI();

        LoadMockMails();
        _showingUnclaimed = true;
        SyncTabButtonStyles();
        RebuildList();
        HideDetailPanel();
        _mailboxPanel.style.display = DisplayStyle.Flex;
    }
    public void Hide()
    {
        if (_mailboxPanel != null)
            _mailboxPanel.style.display = DisplayStyle.None;

        HideRewardOverlay();
        HideConfirmDialog();
        HideDetailPanel();
    }

    private void BindUI()
    {
        if (uiDocument == null)
        {
            Debug.LogError("[MailboxPanel] UIDocument가 할당되지 않았습니다.");
            return;
        }

        _root = uiDocument.rootVisualElement;
        _mailboxPanel = _root.Q<VisualElement>("mailbox-panel");
        _titleLabel = _root.Q<Label>("title-label");
        _closeBtn   = _root.Q<Button>("close-btn");
        _closeBtn?.RegisterCallback<ClickEvent>(_ => Hide());
        _unclaimedTabBtn = _root.Q<Button>("unclaimed-tab-btn");
        _claimedTabBtn   = _root.Q<Button>("claimed-tab-btn");
        _unclaimedBadge  = _root.Q<Label>("unclaimed-badge");

        _unclaimedTabBtn?.RegisterCallback<ClickEvent>(_ =>
        {
            _showingUnclaimed = true;
            SyncTabButtonStyles();
            RebuildList();
            HideDetailPanel();
        });

        _claimedTabBtn?.RegisterCallback<ClickEvent>(_ =>
        {
            _showingUnclaimed = false;
            SyncTabButtonStyles();
            RebuildList();
            HideDetailPanel();
        });
        _mailListView   = _root.Q<ListView>("mail-list");
        _emptyStateView = _root.Q<VisualElement>("empty-state-view");
        _emptyLabel     = _root.Q<Label>("empty-label");

        SetupListView();
        _detailPanel      = _root.Q<VisualElement>("detail-panel");
        _detailTitle      = _root.Q<Label>("detail-title");
        _detailSender     = _root.Q<Label>("detail-sender");
        _detailDate       = _root.Q<Label>("detail-date");
        _detailBody       = _root.Q<Label>("detail-body");
        _detailRewardsRow = _root.Q<VisualElement>("detail-rewards-row");
        _detailClaimBtn   = _root.Q<Button>("detail-claim-btn");

        _detailClaimBtn?.RegisterCallback<ClickEvent>(_ =>
        {
            if (_selectedMail != null) OnClaimClicked(_selectedMail);
        });
        _claimAllBtn   = _root.Q<Button>("claim-all-btn");
        _deleteReadBtn = _root.Q<Button>("delete-read-btn");

        _claimAllBtn?.RegisterCallback<ClickEvent>(_ => OnClaimAllClicked());
        _deleteReadBtn?.RegisterCallback<ClickEvent>(_ => OnDeleteReadClicked());
        _rewardOverlay        = _root.Q<VisualElement>("reward-overlay");
        _rewardCardsContainer = _root.Q<VisualElement>("reward-cards-container");
        _rewardTitleLabel     = _root.Q<Label>("reward-title-label");
        _rewardTouchLabel     = _root.Q<Label>("reward-touch-label");

        _rewardOverlay?.RegisterCallback<ClickEvent>(_ => HideRewardOverlay());
        _confirmOverlay = _root.Q<VisualElement>("confirm-overlay");
        _confirmMessage = _root.Q<Label>("confirm-message");
        _confirmYesBtn  = _root.Q<Button>("confirm-yes-btn");
        _confirmNoBtn   = _root.Q<Button>("confirm-no-btn");

        _confirmNoBtn?.RegisterCallback<ClickEvent>(_ => HideConfirmDialog());
        Hide();
    }

    private void SetupListView()
    {
        if (_mailListView == null) return;

        _mailListView.makeItem = MakeMailCard;
        _mailListView.bindItem = BindMailCard;
        _mailListView.fixedItemHeight = 80;
        _mailListView.selectionType = SelectionType.None;
    }
    private VisualElement MakeMailCard()
    {
        var card = new VisualElement();
        card.AddToClassList("mail-card");

        var icon = new VisualElement();
        icon.name = "item-icon";
        icon.AddToClassList("item-icon");
        card.Add(icon);

        var info = new VisualElement();
        info.AddToClassList("mail-info");

        var titleLabel = new Label();
        titleLabel.name = "mail-title-label";
        titleLabel.AddToClassList("mail-title-label");
        info.Add(titleLabel);

        var senderLabel = new Label();
        senderLabel.name = "sender-label";
        senderLabel.AddToClassList("sender-label");
        info.Add(senderLabel);

        var expireLabel = new Label();
        expireLabel.name = "expire-label";
        expireLabel.AddToClassList("expire-label");
        info.Add(expireLabel);

        card.Add(info);

        var rewardPreview = new VisualElement();
        rewardPreview.name = "reward-preview";
        rewardPreview.AddToClassList("reward-preview");
        card.Add(rewardPreview);

        var claimBtn = new Button();
        claimBtn.name = "claim-btn";
        claimBtn.AddToClassList("claim-btn");
        claimBtn.text = "수령";
        card.Add(claimBtn);

        return card;
    }
    private void BindMailCard(VisualElement card, int index)
    {
        if (index < 0 || index >= _filteredMails.Count) return;

        var mail = _filteredMails[index];

        var titleLabel = card.Q<Label>("mail-title-label");
        if (titleLabel != null)
        {
            titleLabel.text = mail.title;
            titleLabel.EnableInClassList("mail-unread", !mail.isRead);
        }

        var senderLabel = card.Q<Label>("sender-label");
        if (senderLabel != null)
            senderLabel.text = mail.senderName;

        var expireLabel = card.Q<Label>("expire-label");
        if (expireLabel != null)
        {
            int daysLeft = mail.DaysUntilExpiry;
            if (mail.IsExpired)
            {
                expireLabel.text = "만료";
                expireLabel.EnableInClassList("expire-danger", true);
            }
            else
            {
                expireLabel.text = $"D-{daysLeft}";
                expireLabel.EnableInClassList("expire-danger", daysLeft <= 7);
            }
        }

        var itemIcon = card.Q<VisualElement>("item-icon");
        if (itemIcon != null)
        {
            if (mail.rewards.Count > 0 && mail.rewards[0].icon != null)
                itemIcon.style.backgroundImage = new StyleBackground(mail.rewards[0].icon);
            else
                itemIcon.style.backgroundImage = StyleKeyword.None;
        }

        var rewardPreview = card.Q<VisualElement>("reward-preview");
        if (rewardPreview != null)
        {
            rewardPreview.Clear();
            int maxPreview = Mathf.Min(mail.rewards.Count, 3);
            for (int i = 0; i < maxPreview; i++)
            {
                var rewardIcon = new VisualElement();
                rewardIcon.AddToClassList("reward-preview-icon");
                if (mail.rewards[i].icon != null)
                    rewardIcon.style.backgroundImage = new StyleBackground(mail.rewards[i].icon);
                rewardPreview.Add(rewardIcon);
            }

            if (mail.rewards.Count > 3)
            {
                var extra = new Label();
                extra.AddToClassList("reward-extra-label");
                extra.text = $"+{mail.rewards.Count - 3}";
                rewardPreview.Add(extra);
            }
        }

        var claimBtn = card.Q<Button>("claim-btn");
        if (claimBtn != null)
        {
            claimBtn.SetEnabled(mail.CanClaim);
            claimBtn.text = mail.isClaimed ? "수령 완료" : "수령";

            if (mail.isClaimed)
                claimBtn.AddToClassList("claim-btn-done");
            else
                claimBtn.RemoveFromClassList("claim-btn-done");

            claimBtn.clickable = new Clickable(() => OnClaimClicked(mail));
        }

        card.RegisterCallback<ClickEvent>(evt =>
        {
            if (evt.target is Button) return;
            OnMailCardClicked(mail);
        });
    }
    private void LoadMockMails()
    {
        if (mailCatalog != null && mailCatalog.mockMails != null && mailCatalog.mockMails.Count > 0)
        {
            _allMails = mailCatalog.mockMails.Select(CloneMail).ToList();
            return;
        }

        _allMails = new List<MailData>
        {
            new MailData
            {
                mailId = "m001",
                title = "환영합니다!",
                senderName = "오퍼레이터",
                body = "마인드아크에 오신 것을 환영합니다.\n첫 로그인 보상을 수령해 주세요!",
                sendDate = System.DateTime.Now.AddDays(-1),
                expireDate = System.DateTime.Now.AddDays(6),
                rewards = new List<RewardItem> { new RewardItem { itemId = 1001, itemName = "젬", amount = 100 } },
                isRead = false,
                isClaimed = false
            },
            new MailData
            {
                mailId = "m002",
                title = "주간 미션 보상",
                senderName = "시스템",
                body = "주간 미션 보상이 도착했습니다.\n미션을 계속 완료하고 더 많은 보상을 받아 보세요.",
                sendDate = System.DateTime.Now,
                expireDate = System.DateTime.Now.AddDays(14),
                rewards = new List<RewardItem> { new RewardItem { itemId = 2001, itemName = "골드", amount = 5000 } },
                isRead = false,
                isClaimed = false
            },
        };
    }
    private void RebuildList()
    {
        _filteredMails = _showingUnclaimed
            ? _allMails.Where(m => m.CanClaim).ToList()
            : _allMails.Where(m => m.isClaimed).ToList();

        if (_mailListView != null)
        {
            _mailListView.itemsSource = _filteredMails;
            _mailListView.RefreshItems();
        }

        bool isEmpty = _filteredMails.Count == 0;

        if (_emptyStateView != null)
            _emptyStateView.style.display = isEmpty ? DisplayStyle.Flex : DisplayStyle.None;

        if (_mailListView != null)
            _mailListView.style.display = isEmpty ? DisplayStyle.None : DisplayStyle.Flex;

        RefreshUnclaimedBadge();
    }

    private void RefreshUnclaimedBadge()
    {
        int count = _allMails.Count(m => m.CanClaim);
        if (_unclaimedBadge != null)
        {
            _unclaimedBadge.text = count.ToString();
            _unclaimedBadge.style.display = count > 0 ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    private void SyncTabButtonStyles()
    {
        _unclaimedTabBtn?.EnableInClassList("tab-active", _showingUnclaimed);
        _claimedTabBtn?.EnableInClassList("tab-active", !_showingUnclaimed);
    }

    private void OnMailCardClicked(MailData mail)
    {
        _selectedMail = mail;
        mail.isRead = true;
        ShowDetailPanel(mail);
        RebuildList();
    }

    private void OnClaimClicked(MailData mail)
    {
        if (!mail.CanClaim) return;

        mail.isClaimed = true;

        ApplyRewards(mail.rewards);
        ShowRewardOverlay(mail.rewards);
        RebuildList();

        if (_selectedMail == mail)
            ShowDetailPanel(mail);
    }

    private void OnClaimAllClicked()
    {
        var claimable = _allMails.Where(m => m.CanClaim).ToList();
        if (claimable.Count == 0)
        {
            Debug.Log("[MailboxPanel] 수령 가능한 우편이 없습니다.");
            return;
        }

        foreach (var m in claimable)
            m.isClaimed = true;

        var allRewards = claimable.SelectMany(m => m.rewards).ToList();

        ApplyRewards(allRewards);
        ShowRewardOverlay(allRewards);
        RebuildList();
        HideDetailPanel();
    }

    private void OnDeleteReadClicked()
    {
        int count = _allMails.Count(m => m.isClaimed);
        if (count == 0)
        {
            Debug.Log("[MailboxPanel] 삭제할 수령 완료 우편이 없습니다.");
            return;
        }

        ShowConfirmDialog("수령 완료한 우편을 삭제할까요?",
            () =>
            {
                _allMails.RemoveAll(m => m.isClaimed);
                RebuildList();
                HideDetailPanel();
                HideConfirmDialog();
            });
    }

    private void ShowDetailPanel(MailData mail)
    {
        if (_detailPanel == null) return;

        _detailPanel.style.display = DisplayStyle.Flex;

        if (_detailTitle != null) _detailTitle.text = mail.title;
        if (_detailSender != null) _detailSender.text = $"보낸 이: {mail.senderName}";
        if (_detailDate != null) _detailDate.text = $"수신일: {mail.sendDate:yyyy.MM.dd}  만료일: {mail.expireDate:yyyy.MM.dd}";
        if (_detailBody != null) _detailBody.text = mail.body;

        if (_detailRewardsRow != null)
        {
            _detailRewardsRow.Clear();
            foreach (var reward in mail.rewards)
            {
                var rewardCard = CreateRewardCard(reward);
                _detailRewardsRow.Add(rewardCard);
            }
        }

        if (_detailClaimBtn != null)
        {
            _detailClaimBtn.SetEnabled(mail.CanClaim);
            _detailClaimBtn.text = mail.isClaimed ? "수령 완료" : "보상 수령";
        }
    }
    private void HideDetailPanel()
    {
        if (_detailPanel != null)
            _detailPanel.style.display = DisplayStyle.None;
        _selectedMail = null;
    }

    private void ShowRewardOverlay(List<RewardItem> rewards)
    {
        if (_rewardOverlay == null) return;

        _rewardOverlay.style.display = DisplayStyle.Flex;

        if (_rewardTitleLabel != null)
            _rewardTitleLabel.text = "보상을 획득했습니다!";

        if (_rewardCardsContainer != null)
        {
            _rewardCardsContainer.Clear();
            foreach (var r in rewards)
            {
                var card = CreateRewardCard(r);
                _rewardCardsContainer.Add(card);
            }
        }

        if (_rewardTouchLabel != null)
            _rewardTouchLabel.text = "터치하여 계속";
    }

    private void HideRewardOverlay()
    {
        if (_rewardOverlay != null)
            _rewardOverlay.style.display = DisplayStyle.None;
    }

    private void ShowConfirmDialog(string message, System.Action onConfirm)
    {
        if (_confirmOverlay == null) return;

        _confirmOverlay.style.display = DisplayStyle.Flex;

        if (_confirmMessage != null)
            _confirmMessage.text = message;

        if (_confirmYesBtn != null)
        {
            _confirmYesBtn.clickable = new Clickable(() => onConfirm?.Invoke());
        }
    }

    private void HideConfirmDialog()
    {
        if (_confirmOverlay != null)
            _confirmOverlay.style.display = DisplayStyle.None;
    }

    /// <summary>
/// UI Toolkit 기반 우편함 패널입니다.
///
/// 기능:
/// - 수령 가능 / 수령 완료 우편 탭 전환
/// - 우편 목록 및 상세 내용 표시
/// - 개별 수령 / 전체 수령 / 읽은 우편 삭제
/// - 보상 획득 연출 및 확인 팝업 표시
/// </summary>
    private void ApplyRewards(List<RewardItem> rewards)
    {
        if (playerData == null) return;

        foreach (var reward in rewards)
        {
            if (!playerData.TryGrantReward(reward))
            {
                Debug.Log("[MailboxPanel] 수령 가능한 우편이 없습니다.");
            }
        }
    }
    private static MailData CloneMail(MailData source)
    {
        return new MailData
        {
            mailId = source.mailId,
            title = source.title,
            senderName = source.senderName,
            body = source.body,
            sendDate = source.sendDate,
            expireDate = source.expireDate,
            rewards = source.rewards != null ? source.rewards.Select(r => new RewardItem { itemId = r.itemId, itemName = r.itemName, icon = r.icon, amount = r.amount }).ToList() : new List<RewardItem>(),
            isRead = source.isRead,
            isClaimed = source.isClaimed,
        };
    }

    private VisualElement CreateRewardCard(RewardItem reward)
    {
        var card = new VisualElement();
        card.AddToClassList("reward-card");

        var icon = new VisualElement();
        icon.AddToClassList("reward-card-icon");
        if (reward.icon != null)
            icon.style.backgroundImage = new StyleBackground(reward.icon);
        card.Add(icon);

        var nameLabel = new Label();
        nameLabel.AddToClassList("reward-card-name");
        nameLabel.text = reward.itemName;
        card.Add(nameLabel);

        var amountLabel = new Label();
        amountLabel.AddToClassList("reward-card-amount");
        amountLabel.text = $"x{reward.amount}";
        card.Add(amountLabel);

        return card;
    }
}






