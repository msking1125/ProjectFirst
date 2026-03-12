using System;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Game/Mail Box", fileName = "MailBox")]
public class MailBox : ScriptableObject
{
    public List<MailItem> items = new List<MailItem>();
    public event Action OnMailReceived;
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
    public int UnclaimedCount()
    {
        int count = 0;
        foreach (var item in items)
            if (!item.isClaimed) count++;
        return count;
    }
}
[Serializable]
public struct MailItem
{
    [Tooltip("인스펙터에서 설정합니다.")]
    public string title;

    [Tooltip("인스펙터에서 설정합니다.")]
    public string body;

    [Tooltip("인스펙터에서 설정합니다.")]
    public int gold;

    [Tooltip("인스펙터에서 설정합니다.")]
    public int ticket;

    [Tooltip("Claim diamond")]
    public int diamond;

    [Tooltip("인스펙터에서 설정합니다.")]
    public string sentTime;

    [Tooltip("인스펙터에서 설정합니다.")]
    public bool isClaimed;
}


