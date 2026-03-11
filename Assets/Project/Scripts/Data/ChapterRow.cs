using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CSV에서 임포트되는 챕터 1행 데이터.
/// </summary>
[Serializable]
public class ChapterRow
{
    public int id;
    public string name;
    public string description;
    public float worldMapX;
    public float worldMapY;
    public bool isUnlocked;

    /// <summary>
    /// worldMapX/Y를 Vector2로 변환합니다.
    /// </summary>
    public Vector2 WorldMapPosition => new Vector2(worldMapX, worldMapY);
}
