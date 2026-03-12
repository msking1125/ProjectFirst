using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Documentation cleaned.
/// Documentation cleaned.
/// Documentation cleaned.
/// Documentation cleaned.
/// </summary>
[CreateAssetMenu(menuName = "Game/Mail Box", fileName = "MailBox")]
public class MailBox : ScriptableObject
{
    public List<MailItem> items = new List<MailItem>();

    /// Documentation cleaned.
    public event Action OnMailReceived;

    /// Documentation cleaned.
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

    /// Documentation cleaned.
    public int UnclaimedCount()
    {
        int count = 0;
        foreach (var item in items)
            if (!item.isClaimed) count++;
        return count;
    }
}

/// Documentation cleaned.
[Serializable]
public struct MailItem
{
    [Tooltip("Configured in inspector.")]
    public string title;

    [Tooltip("Configured in inspector.")]
    public string body;

    [Tooltip("Configured in inspector.")]
    public int gold;

    [Tooltip("Configured in inspector.")]
    public int ticket;

    [Tooltip("Claim diamond")]
    public int diamond;

    [Tooltip("Configured in inspector.")]
    public string sentTime;

    [Tooltip("Configured in inspector.")]
    public bool isClaimed;
}

