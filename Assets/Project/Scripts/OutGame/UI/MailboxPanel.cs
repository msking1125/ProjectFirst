using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// UI Toolkit ±â¹Ý ¿ìÆíÇÔ ÆÐ³Î.
///
/// [Inspector ¿¬°á °¡ÀÌµå]
/// ¦£ UI
/// ¦¢  ¦¦ uiDocument   : MailboxView.uxml À» »ç¿ëÇÏ´Â UIDocument ÄÄÆ÷³ÍÆ®
/// ¦¦ Data
///    ¦¦ playerData   : PlayerData.asset  (ÀçÈ­ º¸»ó Àû¿ë ½Ã »ç¿ë)
///
/// LobbyManager ¿¡¼­ OnMailClickedHandler() ¡æ MailboxPanel.Instance.Show() È£Ãâ.
/// </summary>
[DisallowMultipleComponent]
public class MailboxPanel : MonoBehaviour
{
    // ¦¡¦¡ Singleton ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    public static MailboxPanel Instance { get; private set; }

    // ¦¡¦¡ Inspector ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡

    [Header("UI")]
    [SerializeField] private UIDocument uiDocument;

    [Header("Data")]
    [SerializeField] private PlayerData playerData;

    // ¦¡¦¡ »óÅÂ ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡

    private List<MailData> _allMails = new List<MailData>();
    private MailData _selectedMail;
    private bool _showingUnclaimed = true; // true: ¹Ì¼ö·É ÅÇ, false: ¼ö·ÉÀÌ·Â ÅÇ

    // ¦¡¦¡ UI ¿ä¼Ò Ä³½Ã ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡

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

    // ÇöÀç ÇÊÅÍ¸µµÈ ¸ñ·Ï
    private List<MailData> _filteredMails = new List<MailData>();

    // ¦¡¦¡ »ý¸íÁÖ±â ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡

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

    // ?????????????????????????????????????????????????????????
    //  Public API
    // ?????????????????????????????????????????????????????????

    /// <summary>¿ìÆíÇÔ ÆÐ³ÎÀ» ¿­°í ´õ¹Ì µ¥ÀÌÅÍ¸¦ ·ÎµåÇÕ´Ï´Ù.</summary>
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

    /// <summary>¿ìÆíÇÔ ÆÐ³ÎÀ» ¼û±é´Ï´Ù.</summary>
    public void Hide()
    {
        if (_mailboxPanel != null)
            _mailboxPanel.style.display = DisplayStyle.None;

        HideRewardOverlay();
        HideConfirmDialog();
        HideDetailPanel();
    }

    // ?????????????????????????????????????????????????????????
    //  UI ¹ÙÀÎµù
    // ?????????????????????????????????????????????????????????

    private void BindUI()
    {
        if (uiDocument == null)
        {
            Debug.LogError("[MailboxPanel] UIDocument°¡ ÇÒ´çµÇÁö ¾Ê¾Ò½À´Ï´Ù.");
            return;
        }

        _root = uiDocument.rootVisualElement;
        _mailboxPanel = _root.Q<VisualElement>("mailbox-panel");

        // ¦¡¦¡ Header ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
        _titleLabel = _root.Q<Label>("title-label");
        _closeBtn   = _root.Q<Button>("close-btn");
        _closeBtn?.RegisterCallback<ClickEvent>(_ => Hide());

        // ¦¡¦¡ Tabs ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
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

        // ¦¡¦¡ ListView ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
        _mailListView   = _root.Q<ListView>("mail-list");
        _emptyStateView = _root.Q<VisualElement>("empty-state-view");
        _emptyLabel     = _root.Q<Label>("empty-label");

        SetupListView();

        // ¦¡¦¡ Detail Panel ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
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

        // ¦¡¦¡ Bottom Action Bar ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
        _claimAllBtn   = _root.Q<Button>("claim-all-btn");
        _deleteReadBtn = _root.Q<Button>("delete-read-btn");

        _claimAllBtn?.RegisterCallback<ClickEvent>(_ => OnClaimAllClicked());
        _deleteReadBtn?.RegisterCallback<ClickEvent>(_ => OnDeleteReadClicked());

        // ¦¡¦¡ Reward Overlay ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
        _rewardOverlay        = _root.Q<VisualElement>("reward-overlay");
        _rewardCardsContainer = _root.Q<VisualElement>("reward-cards-container");
        _rewardTitleLabel     = _root.Q<Label>("reward-title-label");
        _rewardTouchLabel     = _root.Q<Label>("reward-touch-label");

        _rewardOverlay?.RegisterCallback<ClickEvent>(_ => HideRewardOverlay());

        // ¦¡¦¡ Confirm Dialog ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
        _confirmOverlay = _root.Q<VisualElement>("confirm-overlay");
        _confirmMessage = _root.Q<Label>("confirm-message");
        _confirmYesBtn  = _root.Q<Button>("confirm-yes-btn");
        _confirmNoBtn   = _root.Q<Button>("confirm-no-btn");

        _confirmNoBtn?.RegisterCallback<ClickEvent>(_ => HideConfirmDialog());

        // ±âº» ¼û±è
        Hide();
    }

    // ?????????????????????????????????????????????????????????
    //  ListView ¼³Á¤
    // ?????????????????????????????????????????????????????????

    private void SetupListView()
    {
        if (_mailListView == null) return;

        _mailListView.makeItem = MakeMailCard;
        _mailListView.bindItem = BindMailCard;
        _mailListView.fixedItemHeight = 80;
        _mailListView.selectionType = SelectionType.None;
    }

    /// <summary>¸ÞÀÏ Ä«µå ÇÏ³ª¸¦ »ý¼ºÇÕ´Ï´Ù.</summary>
    private VisualElement MakeMailCard()
    {
        var card = new VisualElement();
        card.AddToClassList("mail-card");

        // ¾ÆÀÌÅÛ ¾ÆÀÌÄÜ (ÁÂÃø)
        var icon = new VisualElement();
        icon.name = "item-icon";
        icon.AddToClassList("item-icon");
        card.Add(icon);

        // ¸ÞÀÏ Á¤º¸ ¿µ¿ª
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

        // º¸»ó ¹Ì¸®º¸±â
        var rewardPreview = new VisualElement();
        rewardPreview.name = "reward-preview";
        rewardPreview.AddToClassList("reward-preview");
        card.Add(rewardPreview);

        // ¹Þ±â ¹öÆ°
        var claimBtn = new Button();
        claimBtn.name = "claim-btn";
        claimBtn.AddToClassList("claim-btn");
        claimBtn.text = "¹Þ±â";
        card.Add(claimBtn);

        return card;
    }

    /// <summary>¸®½ºÆ® Ç×¸ñ¿¡ MailData¸¦ ¹ÙÀÎµùÇÕ´Ï´Ù.</summary>
    private void BindMailCard(VisualElement card, int index)
    {
        if (index < 0 || index >= _filteredMails.Count) return;

        var mail = _filteredMails[index];

        // Å¸ÀÌÆ²
        var titleLabel = card.Q<Label>("mail-title-label");
        if (titleLabel != null)
        {
            titleLabel.text = mail.title;
            titleLabel.EnableInClassList("mail-unread", !mail.isRead);
        }

        // ¹ß½ÅÀÚ
        var senderLabel = card.Q<Label>("sender-label");
        if (senderLabel != null)
            senderLabel.text = mail.senderName;

        // ¸¸·áÀÏ
        var expireLabel = card.Q<Label>("expire-label");
        if (expireLabel != null)
        {
            int daysLeft = mail.DaysUntilExpiry;
            if (mail.IsExpired)
            {
                expireLabel.text = "¸¸·áµÊ";
                expireLabel.EnableInClassList("expire-danger", true);
            }
            else
            {
                expireLabel.text = $"D-{daysLeft}";
                expireLabel.EnableInClassList("expire-danger", daysLeft <= 7);
            }
        }

        // ¾ÆÀÌÅÛ ¾ÆÀÌÄÜ (Ã¹ ¹øÂ° º¸»óÀÇ ¾ÆÀÌÄÜ »ç¿ë)
        var itemIcon = card.Q<VisualElement>("item-icon");
        if (itemIcon != null)
        {
            if (mail.rewards.Count > 0 && mail.rewards[0].icon != null)
                itemIcon.style.backgroundImage = new StyleBackground(mail.rewards[0].icon);
            else
                itemIcon.style.backgroundImage = StyleKeyword.None;
        }

        // º¸»ó ¹Ì¸®º¸±â (ÃÖ´ë 3°³ + ÃÊ°ú ½Ã +N)
        var rewardPreview = card.Q<VisualElement>("reward-preview");
        if (rewardPreview != null)
        {
            rewardPreview.Clear();
            int maxPreview = Mathf.Min(mail.rewards.Count, 3);
            for (int i = 0; i < maxPreview; i++)
            {
                var rIcon = new VisualElement();
                rIcon.AddToClassList("reward-preview-icon");
                if (mail.rewards[i].icon != null)
                    rIcon.style.backgroundImage = new StyleBackground(mail.rewards[i].icon);
                rewardPreview.Add(rIcon);
            }

            if (mail.rewards.Count > 3)
            {
                var extra = new Label();
                extra.AddToClassList("reward-extra-label");
                extra.text = $"+{mail.rewards.Count - 3}";
                rewardPreview.Add(extra);
            }
        }

        // ¹Þ±â ¹öÆ°
        var claimBtn = card.Q<Button>("claim-btn");
        if (claimBtn != null)
        {
            claimBtn.SetEnabled(mail.CanClaim);
            claimBtn.text = mail.isClaimed ? "¿Ï·á" : "¹Þ±â";

            if (mail.isClaimed)
                claimBtn.AddToClassList("claim-btn-done");
            else
                claimBtn.RemoveFromClassList("claim-btn-done");

            claimBtn.clickable = new Clickable(() => OnClaimClicked(mail));
        }

        // Ä«µå ÀüÃ¼ Å¬¸¯ ¡æ µðÅ×ÀÏ ÆÐ³Î
        card.RegisterCallback<ClickEvent>(evt =>
        {
            // ¹öÆ° Å¬¸¯Àº Á¦¿Ü
            if (evt.target is Button) return;
            OnMailCardClicked(mail);
        });
    }

    // ?????????????????????????????????????????????????????????
    //  ´õ¹Ì µ¥ÀÌÅÍ
    // ?????????????????????????????????????????????????????????

    private void LoadMockMails()
    {
        _allMails = new List<MailData>
        {
            new MailData
            {
                mailId     = "m001",
                title      = "È¯¿µÇÕ´Ï´Ù!",
                senderName = "¿î¿µÀÚ",
                body       = "MindArk¿¡ ¿À½Å °ÍÀ» È¯¿µÇÕ´Ï´Ù.\nÃ¹ Á¢¼Ó º¸»óÀ» ¹Þ¾Æ°¡¼¼¿ä!",
                sendDate   = System.DateTime.Now.AddDays(-1),
                expireDate = System.DateTime.Now.AddDays(6),
                rewards    = new List<RewardItem>
                {
                    new RewardItem { itemId = 1001, itemName = "Áª", amount = 100 }
                },
                isRead    = false,
                isClaimed = false
            },
            new MailData
            {
                mailId     = "m002",
                title      = "ÁÖ°£ ¹Ì¼Ç º¸»ó",
                senderName = "½Ã½ºÅÛ",
                body       = "ÁÖ°£ ¹Ì¼Ç ´Þ¼º º¸»óÀÔ´Ï´Ù.\n²ÙÁØÈ÷ ¹Ì¼ÇÀ» Å¬¸®¾îÇØ º¸¼¼¿ä!",
                sendDate   = System.DateTime.Now,
                expireDate = System.DateTime.Now.AddDays(14),
                rewards    = new List<RewardItem>
                {
                    new RewardItem { itemId = 2001, itemName = "°ñµå", amount = 5000 }
                },
                isRead    = false,
                isClaimed = false
            },
        };
    }

    // ?????????????????????????????????????????????????????????
    //  ¸ñ·Ï °»½Å
    // ?????????????????????????????????????????????????????????

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

        // ºó ¸ñ·Ï Ã³¸®
        bool isEmpty = _filteredMails.Count == 0;

        if (_emptyStateView != null)
            _emptyStateView.style.display = isEmpty ? DisplayStyle.Flex : DisplayStyle.None;

        if (_mailListView != null)
            _mailListView.style.display = isEmpty ? DisplayStyle.None : DisplayStyle.Flex;

        // ¹Ì¼ö·É ¹îÁö °»½Å
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

    // ?????????????????????????????????????????????????????????
    //  ÅÇ ½ºÅ¸ÀÏ
    // ?????????????????????????????????????????????????????????

    private void SyncTabButtonStyles()
    {
        _unclaimedTabBtn?.EnableInClassList("tab-active", _showingUnclaimed);
        _claimedTabBtn?.EnableInClassList("tab-active", !_showingUnclaimed);
    }

    // ?????????????????????????????????????????????????????????
    //  ÀÌº¥Æ® ÇÚµé·¯
    // ?????????????????????????????????????????????????????????

    private void OnMailCardClicked(MailData mail)
    {
        _selectedMail = mail;
        mail.isRead = true;
        ShowDetailPanel(mail);
        RebuildList(); // ÀÐÀ½ »óÅÂ ¹Ý¿µ
    }

    private void OnClaimClicked(MailData mail)
    {
        if (!mail.CanClaim) return;

        mail.isClaimed = true;

        // TODO: ¼­¹ö API È£Ãâ ÈÄ ÀÀ´äÀ¸·Î Ã³¸®
        ApplyRewards(mail.rewards);
        ShowRewardOverlay(mail.rewards);
        RebuildList();

        // µðÅ×ÀÏ ÆÐ³Î °»½Å
        if (_selectedMail == mail)
            ShowDetailPanel(mail);
    }

    private void OnClaimAllClicked()
    {
        var claimable = _allMails.Where(m => m.CanClaim).ToList();
        if (claimable.Count == 0)
        {
            Debug.Log("[MailboxPanel] ¼ö·É °¡´ÉÇÑ ¿ìÆíÀÌ ¾ø½À´Ï´Ù.");
            return;
        }

        foreach (var m in claimable)
            m.isClaimed = true;

        var allRewards = claimable.SelectMany(m => m.rewards).ToList();

        // TODO: ¼­¹ö API ÀÏ°ý È£Ãâ
        ApplyRewards(allRewards);
        ShowRewardOverlay(allRewards);
        RebuildList();
        HideDetailPanel();
    }

    private void OnDeleteReadClicked()
    {
        // »èÁ¦ ´ë»ó: ¼ö·É ¿Ï·áµÈ ¿ìÆí¸¸
        int count = _allMails.Count(m => m.isClaimed);
        if (count == 0)
        {
            Debug.Log("[MailboxPanel] »èÁ¦ÇÒ ¼ö·É ¿Ï·á ¿ìÆíÀÌ ¾ø½À´Ï´Ù.");
            return;
        }

        ShowConfirmDialog($"¼ö·É ¿Ï·áµÈ ¿ìÆí {count}°ÇÀ» »èÁ¦ÇÏ½Ã°Ú½À´Ï±î?\n¹Ì¼ö·É ¿ìÆíÀº »èÁ¦µÇÁö ¾Ê½À´Ï´Ù.",
            () =>
            {
                _allMails.RemoveAll(m => m.isClaimed);
                RebuildList();
                HideDetailPanel();
                HideConfirmDialog();
            });
    }

    // ?????????????????????????????????????????????????????????
    //  µðÅ×ÀÏ ÆÐ³Î
    // ?????????????????????????????????????????????????????????

    private void ShowDetailPanel(MailData mail)
    {
        if (_detailPanel == null) return;

        _detailPanel.style.display = DisplayStyle.Flex;

        if (_detailTitle  != null) _detailTitle.text  = mail.title;
        if (_detailSender != null) _detailSender.text = $"º¸³½ »ç¶÷: {mail.senderName}";
        if (_detailDate   != null) _detailDate.text   = $"¼ö½Å: {mail.sendDate:yyyy.MM.dd}  ¡¤  ¸¸·á: {mail.expireDate:yyyy.MM.dd}";
        if (_detailBody   != null) _detailBody.text   = mail.body;

        // º¸»ó ¸ñ·Ï
        if (_detailRewardsRow != null)
        {
            _detailRewardsRow.Clear();
            foreach (var r in mail.rewards)
            {
                var rewardCard = CreateRewardCard(r);
                _detailRewardsRow.Add(rewardCard);
            }
        }

        // ¼ö·É ¹öÆ° »óÅÂ
        if (_detailClaimBtn != null)
        {
            _detailClaimBtn.SetEnabled(mail.CanClaim);
            _detailClaimBtn.text = mail.isClaimed ? "¼ö·É ¿Ï·á" : "º¸»ó ¹Þ±â";
        }
    }

    private void HideDetailPanel()
    {
        if (_detailPanel != null)
            _detailPanel.style.display = DisplayStyle.None;
        _selectedMail = null;
    }

    // ?????????????????????????????????????????????????????????
    //  º¸»ó ¿À¹ö·¹ÀÌ
    // ?????????????????????????????????????????????????????????

    private void ShowRewardOverlay(List<RewardItem> rewards)
    {
        if (_rewardOverlay == null) return;

        _rewardOverlay.style.display = DisplayStyle.Flex;

        if (_rewardTitleLabel != null)
            _rewardTitleLabel.text = "º¸»ó È¹µæ!";

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
            _rewardTouchLabel.text = "TOUCH TO CONTINUE";
    }

    private void HideRewardOverlay()
    {
        if (_rewardOverlay != null)
            _rewardOverlay.style.display = DisplayStyle.None;
    }

    // ?????????????????????????????????????????????????????????
    //  È®ÀÎ ÆË¾÷
    // ?????????????????????????????????????????????????????????

    private void ShowConfirmDialog(string message, System.Action onConfirm)
    {
        if (_confirmOverlay == null) return;

        _confirmOverlay.style.display = DisplayStyle.Flex;

        if (_confirmMessage != null)
            _confirmMessage.text = message;

        // ±âÁ¸ ÀÌº¥Æ® ÇØÁ¦ ÈÄ Àçµî·Ï
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

    // ?????????????????????????????????????????????????????????
    //  º¸»ó Àû¿ë
    // ?????????????????????????????????????????????????????????

    /// <summary>
    /// º¸»óÀ» PlayerData¿¡ ¹Ý¿µÇÕ´Ï´Ù.
    /// ¼­¹ö ¿¬µ¿ Àü±îÁö´Â ¾ÆÀÌÅÛ ÀÌ¸§ ±âÁØÀ¸·Î ÀçÈ­¸¦ Á÷Á¢ Ãß°¡ÇÕ´Ï´Ù.
    /// </summary>
    private void ApplyRewards(List<RewardItem> rewards)
    {
        if (playerData == null) return;

        foreach (var r in rewards)
        {
            switch (r.itemName)
            {
                case "°ñµå":
                    playerData.AddGold(r.amount);
                    break;
                case "Áª":
                    playerData.AddGem(r.amount);
                    break;
                default:
                    Debug.Log($"[MailboxPanel] ¹ÌÃ³¸® º¸»ó: {r.itemName} x{r.amount} (itemId={r.itemId})");
                    break;
            }
        }
    }

    // ?????????????????????????????????????????????????????????
    //  À¯Æ¿
    // ?????????????????????????????????????????????????????????

    /// <summary>º¸»ó Ä«µå VisualElement¸¦ »ý¼ºÇÕ´Ï´Ù (¾ÆÀÌÄÜ + ÀÌ¸§ + ¼ö·®).</summary>
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
