using System;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace ProjectFirst.Data
{
    /// <summary>
    /// 웨이브 데이터 행
    /// </summary>
    [Serializable]
#if ODIN_INSPECTOR
    [HideLabel]
#endif
    public class WaveRow
    {
        private const int DefaultMonsterId = 1;

#if ODIN_INSPECTOR
        [HorizontalGroup("기본", 0.5f)]
        [BoxGroup("기본/웨이브")]
        [LabelText("웨이브 번호")]
#endif
        public int wave;

#if ODIN_INSPECTOR
        [HorizontalGroup("기본", 0.5f)]
        [BoxGroup("기본/스폰")]
        [LabelText("스폰 수량")]
#endif
        public int spawnCount;

#if ODIN_INSPECTOR
        [HorizontalGroup("간격", 0.5f)]
        [BoxGroup("간격/스폰")]
        [LabelText("스폰 간격")]
        [SuffixLabel("초", true)]
#endif
        public float spawnInterval;

#if ODIN_INSPECTOR
        [HorizontalGroup("배율", 0.33f)]
        [BoxGroup("배율/HP")]
        [LabelText("HP 배율")]
        [ProgressBar(0.5, 5, ColorGetter = "GetHpMulColor")]
#endif
        public float enemyHpMul;

#if ODIN_INSPECTOR
        [HorizontalGroup("배율", 0.33f)]
        [BoxGroup("배율/속도")]
        [LabelText("속도 배율")]
        [ProgressBar(0.5, 3, ColorGetter = "GetSpeedMulColor")]
#endif
        public float enemySpeedMul;

#if ODIN_INSPECTOR
        [HorizontalGroup("배율", 0.34f)]
        [BoxGroup("배율/데미지")]
        [LabelText("데미지 배율")]
        [ProgressBar(0.5, 5, ColorGetter = "GetDmgMulColor")]
#endif
        public float enemyDamageMul;

#if ODIN_INSPECTOR
        [HorizontalGroup("특수", 0.5f)]
        [BoxGroup("특수/엘리트")]
        [LabelText("엘리트 주기")]
        [SuffixLabel("마리마다", true)]
#endif
        public int eliteEvery;

#if ODIN_INSPECTOR
        [HorizontalGroup("특수", 0.5f)]
        [BoxGroup("특수/보스")]
        [LabelText("보스")]
        [ToggleLeft]
#endif
        public bool boss;

#if ODIN_INSPECTOR
        [HorizontalGroup("보상", 0.5f)]
        [BoxGroup("보상/골드")]
        [LabelText("보상 골드")]
#endif
        public int rewardGold;

#if ODIN_INSPECTOR
        [HorizontalGroup("몬스터", 0.5f)]
        [BoxGroup("몬스터/Enemy")]
        [LabelText("Enemy ID")]
#endif
        public int enemyId = DefaultMonsterId;

#if ODIN_INSPECTOR
        [HorizontalGroup("몬스터", 0.5f)]
        [BoxGroup("몬스터/Monster")]
        [LabelText("Monster ID")]
#endif
        public int monsterId = 0;

#if ODIN_INSPECTOR
        private static Color GetHpMulColor() => new Color(1f, 0.3f, 0.3f);
        private static Color GetSpeedMulColor() => new Color(0.3f, 0.6f, 1f);
        private static Color GetDmgMulColor() => new Color(1f, 0.6f, 0.2f);
#endif

        public int GetMonsterIdOrFallback()
        {
            if (enemyId > 0)
            {
                return enemyId;
            }

            if (monsterId > 0)
            {
                return monsterId;
            }

            return DefaultMonsterId;
        }
    }
}
