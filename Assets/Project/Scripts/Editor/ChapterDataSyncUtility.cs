#if UNITY_EDITOR
using System.Collections.Generic;
using ProjectFirst.Data;
using UnityEditor;
using UnityEngine;

namespace ProjectFirst.Editor
{
    /// <summary>
    /// ChapterTable 데이터를 ChapterData 에셋으로 동기화하는 유틸리티입니다.
    /// </summary>
    public static class ChapterDataSyncUtility
    {
        private const string ChapterTablePath = "Assets/Project/Data/ChapterTable.asset";
        private const string ChapterDataPath  = "Assets/Project/Data/ChapterData.asset";

        /// <summary>
        /// CSV에서 가져온 ChapterTable을 기반으로 ChapterData를 생성/갱신합니다.
        /// </summary>
        [MenuItem("Soul Ark/Data/Sync ChapterData From Table")]
        public static void SyncChapterDataFromTable()
        {
            ChapterTable chapterTable = AssetDatabase.LoadAssetAtPath<ChapterTable>(ChapterTablePath);
            if (chapterTable == null)
            {
                Debug.LogError($"[ChapterDataSyncUtility] ChapterTable 에셋을 찾을 수 없습니다: {ChapterTablePath}");
                return;
            }

            ChapterData chapterData = AssetDatabase.LoadAssetAtPath<ChapterData>(ChapterDataPath);
            if (chapterData == null)
            {
                chapterData = ScriptableObject.CreateInstance<ChapterData>();
                AssetDatabase.CreateAsset(chapterData, ChapterDataPath);
            }

            if (chapterData.chapters == null)
                chapterData.chapters = new List<ChapterData.ChapterInfo>();

            chapterData.chapters.Clear();

            foreach (ChapterRow row in chapterTable.rows)
            {
                if (row == null) continue;

                var info = new ChapterData.ChapterInfo
                {
                    chapterId        = row.id,
                    chapterName      = row.name,
                    description      = row.description,
                    worldMapPosition = new Vector2(row.worldMapX, row.worldMapY),
                    clearStars       = 0,
                    isUnlocked       = row.isUnlocked,
                    stageIds         = new List<int>()
                };

                chapterData.chapters.Add(info);
            }

            EditorUtility.SetDirty(chapterData);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[ChapterDataSyncUtility] ChapterData 동기화 완료 - {chapterData.chapters.Count}개 챕터");
        }
    }
}
#endif

