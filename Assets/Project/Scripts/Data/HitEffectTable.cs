using UnityEngine;

namespace ProjectFirst.Data
{
    /// <summary>
    /// 피격 이펙트 프리팹을 한 곳에서 관리하는 테이블.
    /// Enemy Inspector의 개별 슬롯 대신 이 테이블 하나만 연결하면 됩니다.
    ///
    /// 생성: Project 우클릭 → Create → Soul Ark/Hit Effect Table
    /// 경로 권장: Assets/Project/Data/HitEffectTable.asset
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Hit Effect Table", fileName = "HitEffectTable")]
    public class HitEffectTable : ScriptableObject
    {
        public GameObject normalHitVfx;

        public GameObject critHitVfx;

        public GameObject passionHitVfx;

        public GameObject intuitionHitVfx;

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
