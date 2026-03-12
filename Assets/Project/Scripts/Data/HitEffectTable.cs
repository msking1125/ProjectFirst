using UnityEngine;

namespace ProjectFirst.Data
{
    /// <summary>
    /// Documentation cleaned.
    /// Documentation cleaned.
    ///
    /// Documentation cleaned.
    /// Documentation cleaned.
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
        /// Documentation cleaned.
        /// Documentation cleaned.
        /// Documentation cleaned.
        /// </summary>
        public GameObject Resolve(bool isCrit, ElementType element, bool isSkillHit)
        {
            // Note: cleaned comment.
            if (isCrit && critHitVfx != null)
                return critHitVfx;

            // Note: cleaned comment.
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
