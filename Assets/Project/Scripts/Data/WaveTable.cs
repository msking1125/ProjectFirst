using System.Collections.Generic;
using UnityEngine;

namespace ProjectFirst.Data
{
    /// <summary>
    /// Documentation cleaned.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Wave Table")]
    public class WaveTable : ScriptableObject
    {
        public List<WaveRow> wave = new();
    }
}
