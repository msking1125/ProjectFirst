using System.Collections.Generic;
using UnityEngine;

namespace ProjectFirst.Data
{
    /// <summary>
    /// 웨이브 데이터 테이블
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Wave Table")]
    public class WaveTable : ScriptableObject
    {
        public List<WaveRow> wave = new();
    }
}
