using System;
using UnityEngine;

namespace ProjectFirst.Data
{
    /// <summary>
    /// CSV에서 임포트되는 스테이지 1행 데이터.
    /// </summary>
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
