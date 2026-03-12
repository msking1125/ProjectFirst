using System;
using UnityEngine;

/// <summary>
/// Documentation cleaned.
/// Documentation cleaned.
/// </summary>
[CreateAssetMenu(
    menuName = "Events/Void Event Channel",
    fileName = "NewVoidEventChannel")]
public class VoidEventChannelSO : ScriptableObject
{
    /// Documentation cleaned.
    public event Action OnEventRaised;

    /// Documentation cleaned.
    public void RaiseEvent()
    {
        OnEventRaised?.Invoke();
    }
}
