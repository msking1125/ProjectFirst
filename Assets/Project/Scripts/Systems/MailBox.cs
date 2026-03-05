using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 인게임 우편함 ScriptableObject.
/// 방치 보상·이벤트 보상 등이 이 우편함에 적재됩니다.
/// 생성: Create → Game/Mail Box
/// 권장 경로: Assets/Project/Data/MailBox.asset
/// </summary>
[CreateAssetMenu(menuName = "Game/Mail Box", fileName = "MailBox")]
public class MailBox : ScriptableObject
{
    public List<MailItem> items = new List<MailItem>();

    /// <summary>새 우편이 추가될 때 발행됩니다 (우편 UI 뱃지 갱신용).</summary>
    public event Action OnMailReceived;

    /// <summary>우편을 추가합니다.</summary>
    public void AddMail(string title, string body,
        int gold = 0, int ticket = 0, int diamond = 0)
    {
        items.Add(new MailItem
        {
            title     = title,
            body      = body,
            gold      = gold,
            ticket    = ticket,
            diamond   = diamond,
            sentTime  = DateTime.UtcNow.ToString("o"),
            isClaimed = false,
        });

        OnMailReceived?.Invoke();
    }

    /// <summary>수령되지 않은 우편 수를 반환합니다.</summary>
    public int UnclaimedCount()
    {
        int count = 0;
        foreach (var item in items)
            if (!item.isClaimed) count++;
        return count;
    }
}

/// <summary>우편 1건 데이터.</summary>
[Serializable]
public struct MailItem
{
    [Tooltip("우편 제목")]
    public string title;

    [Tooltip("우편 본문")]
    public string body;

    [Tooltip("첨부 골드")]
    public int gold;

    [Tooltip("첨부 티켓")]
    public int ticket;

    [Tooltip("첨부 다이아")]
    public int diamond;

    [Tooltip("발송 UTC 시각 (ISO 8601)")]
    public string sentTime;

    [Tooltip("수령 여부")]
    public bool isClaimed;
}
