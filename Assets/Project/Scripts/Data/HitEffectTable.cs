using UnityEngine;

namespace ProjectFirst.Data
{
    /// <summary>
    ///
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Hit Effect Table", fileName = "HitEffectTable")]
    public class HitEffectTable : ScriptableObject
    {
        public GameObject normalHitVfx;

        public GameObject critHitVfx;

        public GameObject passionHitVfx;

        public GameObject intuitionHitVfx;

        public GameObject reasonHitVfx;
        public GameObject Resolve(bool isCrit, ElementType element, bool isSkillHit)
        {
            if (isCrit && critHitVfx != null)
                return critHitVfx;
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

