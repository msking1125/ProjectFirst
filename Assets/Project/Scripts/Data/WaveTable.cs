using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Wave Table")]
public class WaveTable : ScriptableObject
{
    public List<WaveRow> wave = new();
}