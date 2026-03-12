using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ?멸쾶???고렪??ScriptableObject.
/// 諛⑹튂 蹂댁긽쨌?대깽??蹂댁긽 ?깆씠 ???고렪?⑥뿉 ?곸옱?⑸땲??
/// ?앹꽦: Create ??Game/Mail Box
/// 沅뚯옣 寃쎈줈: Assets/Project/Data/MailBox.asset
/// </summary>
[CreateAssetMenu(menuName = "Game/Mail Box", fileName = "MailBox")]
public class MailBox : ScriptableObject
{
    public List<MailItem> items = new List<MailItem>();

    /// <summary>???고렪??異붽?????諛쒗뻾?⑸땲??(?고렪 UI 諭껋? 媛깆떊??.</summary>
    public event Action OnMailReceived;

    /// <summary>?고렪??異붽??⑸땲??</summary>
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

    /// <summary>?섎졊?섏? ?딆? ?고렪 ?섎? 諛섑솚?⑸땲??</summary>
    public int UnclaimedCount()
    {
        int count = 0;
        foreach (var item in items)
            if (!item.isClaimed) count++;
        return count;
    }
}

/// <summary>?고렪 1嫄??곗씠??</summary>
[Serializable]
public struct MailItem
{
    [Tooltip("?고렪 ?쒕ぉ")]
    public string title;

    [Tooltip("?고렪 蹂몃Ц")]
    public string body;

    [Tooltip("泥⑤? 怨⑤뱶")]
    public int gold;

    [Tooltip("泥⑤? ?곗폆")]
    public int ticket;

    [Tooltip("Claim diamond")]
    public int diamond;

    [Tooltip("諛쒖넚 UTC ?쒓컖 (ISO 8601)")]
    public string sentTime;

    [Tooltip("?섎졊 ?щ?")]
    public bool isClaimed;
}

