using System;
using UnityEngine;

/// <summary>
/// 파라미터 없는 이벤트 채널 (ScriptableObject 기반).
/// </summary>
[CreateAssetMenu(menuName = "ProjectFirst/Events/Void Event Channel")]
public class VoidEventChannelSO : ScriptableObject
{
    public event Action EventRaised;

    public void Raise()
    {
        EventRaised?.Invoke();
    }

    public void Register(Action listener)
    {
        EventRaised += listener;
    }

    public void Unregister(Action listener)
    {
        EventRaised -= listener;
    }
}

