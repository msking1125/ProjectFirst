using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectFirst.Data
{
    [Serializable]
    public class ChapterRow
    {
        public int id;

        public string name;

        [TextArea(2, 4)]
        public string description;

        public float worldMapX;

        public float worldMapY;

        public bool isUnlocked;
        public Vector2 WorldMapPosition => new Vector2(worldMapX, worldMapY);
    }
}

