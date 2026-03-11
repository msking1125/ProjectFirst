using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 스테이지 정보를 보관하는 ScriptableObject 에셋.
/// 생성: Project 우클릭 → Create/MindArk/Data/StageData
/// </summary>
[CreateAssetMenu(fileName = "StageData", menuName = "MindArk/Data/StageData")]
public class StageData : ScriptableObject
{
    [System.Serializable]
    public class StageInfo
    {
        [Tooltip("스테이지 고유 ID")]
        public int stageId;

        [Tooltip("소속 챕터 ID")]
        public int chapterId;

        [Tooltip("챕터 내 순번 (1부터 시작)")]
        public int stageNumber;

        public string stageName;

        [TextArea(2, 4)]
        public string description;

        [Tooltip("권장 전투력")]
        public int recommendedPower;

        [Tooltip("주요 적 속성")]
        public ElementType enemyElement;

        [Tooltip("소모 스태미나")]
        public int staminaCost;

        [Range(0, 3)]
        [Tooltip("클리어 별점 (0~3)")]
        public int clearStars;

        public bool isUnlocked;

        [Tooltip("보상 미리보기 목록")]
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
