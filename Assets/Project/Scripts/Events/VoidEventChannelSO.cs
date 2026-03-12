using System;
using UnityEngine;

/// <summary>
/// ?뚮씪誘명꽣 ?녿뒗 ScriptableObject ?대깽??梨꾨꼸
/// 踰꾪듉 ?대깽?????⑥닚 ?좏샇 ?꾨떖???ъ슜
/// </summary>
[CreateAssetMenu(
    menuName = "Events/Void Event Channel",
    fileName = "NewVoidEventChannel")]
public class VoidEventChannelSO : ScriptableObject
{
    /// <summary>?대깽?몃? ?섏떊??由ъ뒪?덈뱾</summary>
    public event Action OnEventRaised;

    /// <summary>?대깽??諛쒗뻾 (踰꾪듉 ?대┃ ?깆뿉???몄텧)</summary>
    public void RaiseEvent()
    {
        OnEventRaised?.Invoke();
    }
}
