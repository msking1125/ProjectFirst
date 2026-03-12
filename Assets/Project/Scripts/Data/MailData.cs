using System;
using System.Collections.Generic;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace ProjectFirst.Data
{
    /// <summary>
    /// 우편 보상 아이템 1건을 나타내는 데이터 클래스.
    /// </summary>
    [Serializable]
#if ODIN_INSPECTOR
    [HideLabel]
#endif
    public class RewardItem
    {
#if ODIN_INSPECTOR
        [HorizontalGroup("기본", 0.5f)]
        [BoxGroup("기본/ID")]
        [LabelText("아이템 ID")]
        [Tooltip("아이템 고유 ID (서버 연동 시 사용)")]
#endif
        public int itemId;

#if ODIN_INSPECTOR
        [HorizontalGroup("기본", 0.5f)]
        [BoxGroup("기본/이름")]
        [LabelText("이름")]
        [Tooltip("표시용 아이템 이름")]
#endif
        public string itemName;

#if ODIN_INSPECTOR
        [HorizontalGroup("리소스", 0.5f)]
        [BoxGroup("리소스/아이콘")]
        [LabelText("아이콘")]
        [Tooltip("인벤토리·보상 팝업에서 사용할 아이콘")]
        [PreviewField(50, ObjectFieldAlignment.Left)]
#endif
        public Sprite icon;

#if ODIN_INSPECTOR
        [HorizontalGroup("리소스", 0.5f)]
        [BoxGroup("리소스/수량")]
        [LabelText("수량")]
        [ProgressBar(1, 10000)]
#endif
        public int amount;
    }

    /// <summary>
    /// 우편함의 개별 우편 데이터.
    ///
    /// 서버 연동 전에는 MailboxPanel.LoadMockMails() 에서 더미 인스턴스를 생성하여 테스트합니다.
    /// 서버 연동 후에는 JSON 역직렬화 또는 DTO 매핑으로 생성합니다.
    /// </summary>
    [Serializable]
#if ODIN_INSPECTOR
    [HideLabel]
#endif
    public class MailData
    {
#if ODIN_INSPECTOR
        [HorizontalGroup("기본", 0.5f)]
        [BoxGroup("기본/ID")]
        [LabelText("우편 ID")]
        [ReadOnly]
        [Tooltip("우편 고유 ID (서버에서 발급)")]
#endif
        public string mailId;

#if ODIN_INSPECTOR
        [HorizontalGroup("기본", 0.5f)]
        [BoxGroup("기본/제목")]
        [LabelText("제목")]
#endif
        public string title;

#if ODIN_INSPECTOR
        [BoxGroup("발신")]
        [LabelText("보낸이")]
#endif
        public string senderName;

#if ODIN_INSPECTOR
        [BoxGroup("내용")]
        [LabelText("본문")]
        [MultiLineProperty(3)]
#else
        [TextArea(2, 5)]
#endif
        public string body;

#if ODIN_INSPECTOR
        [HorizontalGroup("시간", 0.5f)]
        [BoxGroup("시간/발송")]
        [LabelText("발송일")]
        [ReadOnly]
#endif
        public DateTime sendDate;

#if ODIN_INSPECTOR
        [HorizontalGroup("시간", 0.5f)]
        [BoxGroup("시간/만료")]
        [LabelText("만료일")]
        [ReadOnly]
        [GUIColor(1f, 0.4f, 0.4f)]
#endif
        public DateTime expireDate;

#if ODIN_INSPECTOR
        [BoxGroup("보상")]
        [LabelText("보상 목록")]
        [TableList(ShowIndexLabels = true, AlwaysExpanded = false)]
#endif
        public List<RewardItem> rewards = new List<RewardItem>();

#if ODIN_INSPECTOR
        [HorizontalGroup("상태", 0.5f)]
        [BoxGroup("상태/읽음")]
        [LabelText("읽음")]
        [ToggleLeft]
#endif
        public bool isRead;

#if ODIN_INSPECTOR
        [HorizontalGroup("상태", 0.5f)]
        [BoxGroup("상태/수령")]
        [LabelText("수령 완료")]
        [ToggleLeft]
        [GUIColor(0.3f, 0.8f, 0.3f)]
#endif
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
