using System;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Game/Mail Box", fileName = "MailBox")]
public class MailBox : ScriptableObject
{
    public List<MailItem> items = new List<MailItem>();
    public event Action OnMailReceived;
    public void AddMail(string title, string body,
        int gold = 0, int stamina = 0, int gem = 0)
    {
        items.Add(new MailItem
        {
            title     = title,
            body      = body,
            gold      = gold,
            stamina   = stamina,
            gem       = gem,
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
    public int stamina;

    [Tooltip("Claim gem")]
    public int gem;

    [Tooltip("인스펙터에서 설정합니다.")]
    public string sentTime;

    [Tooltip("인스펙터에서 설정합니다.")]
    public bool isClaimed;
}


