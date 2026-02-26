using System;
using UnityEngine;

/// <summary>
/// 파라미터 없는 ScriptableObject 이벤트 채널
/// 버튼 이벤트 등 단순 신호 전달에 사용
/// </summary>
[CreateAssetMenu(
    menuName = "Events/Void Event Channel",
    fileName = "NewVoidEventChannel")]
public class VoidEventChannelSO : ScriptableObject
{
    /// <summary>이벤트를 수신할 리스너들</summary>
    public event Action OnEventRaised;

    /// <summary>이벤트 발행 (버튼 클릭 등에서 호출)</summary>
    public void RaiseEvent()
    {
        if (OnEventRaised == null)
        {
            Debug.LogWarning($"[{name}] 구독자가 없습니다.");
            return;
        }
        OnEventRaised.Invoke();
    }
}
