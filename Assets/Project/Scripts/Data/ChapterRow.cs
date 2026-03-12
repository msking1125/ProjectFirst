using System;
using System.Collections.Generic;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace ProjectFirst.Data
{
    /// <summary>
    /// CSV에서 임포트되는 챕터 1행 데이터.
    /// </summary>
    [Serializable]
#if ODIN_INSPECTOR
    [HideLabel]
#endif
    public class ChapterRow
    {
#if ODIN_INSPECTOR
        [HorizontalGroup("기본", 0.5f)]
        [BoxGroup("기본/ID")]
        [LabelText("챕터 ID")]
#endif
        public int id;

#if ODIN_INSPECTOR
        [HorizontalGroup("기본", 0.5f)]
        [BoxGroup("기본/이름")]
        [LabelText("챕터명")]
#endif
        public string name;

#if ODIN_INSPECTOR
        [BoxGroup("정보")]
        [LabelText("설명")]
        [MultiLineProperty(2)]
#else
        [TextArea(2, 4)]
#endif
        public string description;

#if ODIN_INSPECTOR
        [BoxGroup("월드맵")]
        [LabelText("월드맵 위치")]
#endif
        public float worldMapX;

#if ODIN_INSPECTOR
        [BoxGroup("월드맵")]
        [LabelText("")]
        [HideLabel]
#endif
        public float worldMapY;

#if ODIN_INSPECTOR
        [BoxGroup("상태")]
        [LabelText("잠금 해제")]
        [ToggleLeft]
#endif
        public bool isUnlocked;

        /// <summary>
        /// worldMapX/Y를 Vector2로 변환합니다.
        /// </summary>
        public Vector2 WorldMapPosition => new Vector2(worldMapX, worldMapY);
    }
}
