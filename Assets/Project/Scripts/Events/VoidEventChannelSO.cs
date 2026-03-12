using System;
using UnityEngine;
[CreateAssetMenu(
    menuName = "Events/Void Event Channel",
    fileName = "NewVoidEventChannel")]
public class VoidEventChannelSO : ScriptableObject
{
    public event Action OnEventRaised;
    public void RaiseEvent()
    {
        OnEventRaised?.Invoke();
    }
}

