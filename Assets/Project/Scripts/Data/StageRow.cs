using System;
using UnityEngine;

namespace ProjectFirst.Data
{
    [Serializable]
    public class StageRow
    {
        public int id;

        public int chapterId;

        public int stageNumber;

        public string name;

        [TextArea(2, 4)]
        public string description;

        public int recommendedPower;

        public ElementType enemyElement = ElementType.Reason;

        public int staminaCost;

        public int rewardGold;

        public int rewardExp;

        public string waveDataId;
    }
}

