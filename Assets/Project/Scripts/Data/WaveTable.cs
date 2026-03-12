using System.Collections.Generic;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace ProjectFirst.Data
{
    /// <summary>
    /// 웨이브 데이터 테이블
    /// </summary>
#if ODIN_INSPECTOR
    [CreateAssetMenu(menuName = "Soul Ark/Wave Table")]
#else
    [CreateAssetMenu(menuName = "Game/Wave Table")]
#endif
    public class WaveTable : ScriptableObject
    {
#if ODIN_INSPECTOR
        [Title("웨이브 목록", TitleAlignment = TitleAlignments.Centered)]
        [TableList(ShowIndexLabels = true, AlwaysExpanded = true)]
        [Searchable]
#endif
        public List<WaveRow> wave = new();
    }
}