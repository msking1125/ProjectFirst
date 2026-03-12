using System.Collections.Generic;
using UnityEngine;

namespace ProjectFirst.Data
{
    [CreateAssetMenu(menuName = "Game/Wave Table")]
    public class WaveTable : ScriptableObject
    {
        public List<WaveRow> wave = new();
    }
}

