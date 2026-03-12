using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectFirst.Data
{
    /// <summary>
    /// 우편 보상 아이템 1건을 나타내는 데이터 클래스.
    /// </summary>
    [Serializable]
    public class RewardItem
    {
        public int itemId;

        public string itemName;

        public Sprite icon;

        public int amount;
    }

    /// <summary>
    /// 우편함의 개별 우편 데이터.
    ///
    /// 서버 연동 전에는 MailboxPanel.LoadMockMails() 에서 더미 인스턴스를 생성하여 테스트합니다.
    /// 서버 연동 후에는 JSON 역직렬화 또는 DTO 매핑으로 생성합니다.
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

        // ── 편의 프로퍼티 ───────────────────────────────────────

        /// <summary>만료 여부</summary>
        public bool IsExpired => DateTime.Now > expireDate;

        /// <summary>보상 수령 가능 여부 (미수령 + 미만료)</summary>
        public bool CanClaim => !isClaimed && !IsExpired;

        /// <summary>만료까지 남은 일수 (0 이상)</summary>
        public int DaysUntilExpiry => Mathf.Max(0, (int)(expireDate - DateTime.Now).TotalDays);
    }
}
