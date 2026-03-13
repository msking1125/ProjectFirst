#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using ProjectFirst.Data;
using UnityEditor;
using UnityEngine;

namespace ProjectFirst.Editor
{
    /// <summary>
    /// StageTable 데이터를 StageData 에셋으로 동기화하는 유틸리티입니다.
    /// </summary>
    public static class StageDataSyncUtility
    {
        private const string StageTablePath = "Assets/Project/Data/StageTable.asset";
        private const string StageDataPath  = "Assets/Project/Data/StageData.asset";

        /// <summary>
        /// CSV에서 가져온 StageTable을 기반으로 StageData를 생성/갱신합니다.
        /// </summary>
        [MenuItem("Soul Ark/Data/Sync StageData From Table")]
        public static void SyncStageDataFromTable()
        {
            StageTable stageTable = AssetDatabase.LoadAssetAtPath<StageTable>(StageTablePath);
            if (stageTable == null)
            {
                Debug.LogError($"[StageDataSyncUtility] StageTable 에셋을 찾을 수 없습니다: {StageTablePath}");
                return;
            }

            StageData stageData = AssetDatabase.LoadAssetAtPath<StageData>(StageDataPath);
            if (stageData == null)
            {
                stageData = ScriptableObject.CreateInstance<StageData>();
                AssetDatabase.CreateAsset(stageData, StageDataPath);
            }

            if (stageData.stages == null)
                stageData.stages = new List<StageData.StageInfo>();

            stageData.stages.Clear();

            foreach (StageRow row in stageTable.rows)
            {
                if (row == null) continue;

                var info = new StageData.StageInfo
                {
                    stageId          = row.id,
                    chapterId        = row.chapterId,
                    stageNumber      = row.stageNumber,
                    stageName        = row.name,
                    description      = row.description,
                    recommendedPower = row.recommendedPower,
                    enemyElement     = row.enemyElement,
                    staminaCost      = row.staminaCost,
                    clearStars       = 0,
                    isUnlocked       = false
                };

                // 기본 보상은 StageRow의 rewardGold/Exp를 이용해 간단히 구성
                info.previewRewards = new List<RewardItem>();
                if (row.rewardGold > 0)
                {
                    info.previewRewards.Add(new RewardItem
                    {
                        itemName = "Gold",
                        amount   = row.rewardGold
                    });
                }

                if (row.rewardExp > 0)
                {
                    info.previewRewards.Add(new RewardItem
                    {
                        itemName = "Exp",
                        amount   = row.rewardExp
                    });
                }

                stageData.stages.Add(info);
            }

            EditorUtility.SetDirty(stageData);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[StageDataSyncUtility] StageData 동기화 완료 - {stageData.stages.Count}개 스테이지");
        }
    }
}
#endif

