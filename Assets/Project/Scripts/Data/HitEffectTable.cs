using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace ProjectFirst.Data
{
    /// <summary>
    /// 피격 이펙트 프리팹을 한 곳에서 관리하는 테이블.
    /// Enemy Inspector의 개별 슬롯 대신 이 테이블 하나만 연결하면 됩니다.
    ///
    /// 생성: Project 우클릭 → Create → Soul Ark/Hit Effect Table
    /// 경로 권장: Assets/Project/Data/HitEffectTable.asset
    /// </summary>
#if ODIN_INSPECTOR
    [CreateAssetMenu(menuName = "Soul Ark/Hit Effect Table", fileName = "HitEffectTable")]
#else
    [CreateAssetMenu(menuName = "Game/Hit Effect Table", fileName = "HitEffectTable")]
#endif
    public class HitEffectTable : ScriptableObject
    {
#if ODIN_INSPECTOR
        [Title("일반 피격", TitleAlignment = TitleAlignments.Left)]
        [HorizontalGroup("일반", 0.5f)]
        [BoxGroup("일반/기본")]
        [LabelText("일반 피격")]
        [AssetsOnly]
        [PreviewField(60, ObjectFieldAlignment.Left)]
        [Tooltip("일반 공격 피격 이펙트\nAssets/Project/Art/VFX/toss/Hit_normal.prefab")]
#endif
        public GameObject normalHitVfx;

#if ODIN_INSPECTOR
        [HorizontalGroup("일반", 0.5f)]
        [BoxGroup("일반/치명타")]
        [LabelText("치명타 피격")]
        [AssetsOnly]
        [PreviewField(60, ObjectFieldAlignment.Left)]
        [Tooltip("치명타 공격 피격 이펙트\nAssets/Project/Art/VFX/toss/Hit_critical.prefab")]
#endif
        public GameObject critHitVfx;

#if ODIN_INSPECTOR
        [Title("스킬 속성별 피격", TitleAlignment = TitleAlignments.Left)]
        [HorizontalGroup("속성", 0.33f)]
        [BoxGroup("속성/Passion")]
        [LabelText("Passion")]
        [AssetsOnly]
        [PreviewField(60, ObjectFieldAlignment.Left)]
        [GUIColor(1f, 0.3f, 0.3f)]
        [Tooltip("Passion 속성 스킬 피격\nAssets/Project/Art/VFX/toss/Hit_passion.prefab")]
#endif
        public GameObject passionHitVfx;

#if ODIN_INSPECTOR
        [HorizontalGroup("속성", 0.33f)]
        [BoxGroup("속성/Intuition")]
        [LabelText("Intuition")]
        [AssetsOnly]
        [PreviewField(60, ObjectFieldAlignment.Left)]
        [GUIColor(1f, 0.8f, 0.3f)]
        [Tooltip("Intuition 속성 스킬 피격\nAssets/Project/Art/VFX/toss/Hit_intuition.prefab")]
#endif
        public GameObject intuitionHitVfx;

#if ODIN_INSPECTOR
        [HorizontalGroup("속성", 0.34f)]
        [BoxGroup("속성/Reason")]
        [LabelText("Reason")]
        [AssetsOnly]
        [PreviewField(60, ObjectFieldAlignment.Left)]
        [GUIColor(0.3f, 0.6f, 1f)]
        [Tooltip("Reason 속성 스킬 피격\nAssets/Project/Art/VFX/toss/Hit_reason.prefab")]
#endif
        public GameObject reasonHitVfx;

        /// <summary>
        /// 상황에 맞는 VFX 프리팹을 반환합니다.
        /// isCrit=true 이면 critHitVfx 우선, 없으면 normalHitVfx.
        /// element가 있으면 속성 VFX 우선, 없으면 normalHitVfx.
        /// </summary>
        public GameObject Resolve(bool isCrit, ElementType element, bool isSkillHit)
        {
            // 치명타 우선
            if (isCrit && critHitVfx != null)
                return critHitVfx;

            // 스킬 피격이면 속성별 VFX
            if (isSkillHit)
            {
                GameObject elementVfx = element switch
                {
                    ElementType.Passion   => passionHitVfx,
                    ElementType.Intuition => intuitionHitVfx,
                    ElementType.Reason    => reasonHitVfx,
                    _                    => null
                };
                if (elementVfx != null)
                    return elementVfx;
            }

            return normalHitVfx;
        }
    }
}
