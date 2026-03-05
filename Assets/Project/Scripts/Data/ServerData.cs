using System;
using UnityEngine;

/// <summary>
/// 서버 한 항목의 데이터. ServerListSO의 rows에 등록합니다.
/// </summary>
[Serializable]
public class ServerData
{
    [Tooltip("서버 고유 ID (서버 선택 시 전달됩니다)")]
    public string serverId;

    [Tooltip("유저에게 보여줄 서버 이름")]
    public string displayName;

    [Tooltip("최대 수용 인원")]
    public int maxPlayers = 3000;

    [Tooltip("현재 접속 인원 (런타임에 API로 갱신 예정)")]
    public int currentPlayers;

    /// <summary>접속자 비율 (0~1)</summary>
    public float LoadRatio => maxPlayers > 0 ? Mathf.Clamp01((float)currentPlayers / maxPlayers) : 0f;

    /// <summary>접속자 비율에 따른 혼잡도 문자열</summary>
    public string CongestionLabel =>
        LoadRatio < 0.5f ? "원활" :
        LoadRatio < 0.85f ? "보통" : "혼잡";
}
