using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectFirst.Data
{
    [Serializable]
    public class RewardItem
    {
        public int itemId;

        public string itemName;

        public Sprite icon;

        public int amount;
    }

    /// <summary>
    ///
    /// </summary>
    [Serializable]
    public class MailData
    {
        public string mailId;

        public string title;

        public string senderName;

        [TextArea(2, 5)]
        public string body;

        public DateTime sendDate;

        public DateTime expireDate;

        public List<RewardItem> rewards = new List<RewardItem>();

        public bool isRead;

        public bool isClaimed;
        public bool IsExpired => DateTime.Now > expireDate;
        public bool CanClaim => !isClaimed && !IsExpired;
        public int DaysUntilExpiry => Mathf.Max(0, (int)(expireDate - DateTime.Now).TotalDays);
    }
}

