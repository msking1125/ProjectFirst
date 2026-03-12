using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProjectFirst.Data
{
    /// <summary>
    /// 스테이지 정보를 보관하는 ScriptableObject 에셋.
    /// 생성: Project 우클릭 → Create/Soul Ark/Data/StageData
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
        /// 특정 챕터에 속한 스테이지를 stageNumber 오름차순으로 반환합니다.
        /// </summary>
        public List<StageInfo> GetByChapter(int chapterId) =>
            stages.Where(s => s.chapterId == chapterId)
                  .OrderBy(s => s.stageNumber)
                  .ToList();
    }
}
