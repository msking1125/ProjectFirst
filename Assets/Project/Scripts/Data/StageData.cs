using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProjectFirst.Data
{
    /// <summary>
    /// Documentation cleaned.
    /// Documentation cleaned.
    /// </summary>
    [CreateAssetMenu(fileName = "StageData", menuName = "MindArk/Data/StageData")]
    public class StageData : ScriptableObject
    {
        [System.Serializable]
        public class StageInfo
        {
            public int stageId;

            public int chapterId;

            public int stageNumber;

            public string stageName;

            [TextArea(2, 4)]
            public string description;

            public int recommendedPower;

            public ElementType enemyElement;

            public int staminaCost;

            [Range(0, 3)]
            public int clearStars;

            public bool isUnlocked;

            public List<RewardItem> previewRewards = new List<RewardItem>();

        }

        public List<StageInfo> stages = new List<StageInfo>();

        /// <summary>
        /// Documentation cleaned.
        /// </summary>
        public List<StageInfo> GetByChapter(int chapterId) =>
            stages.Where(s => s.chapterId == chapterId)
                  .OrderBy(s => s.stageNumber)
                  .ToList();
    }
}
