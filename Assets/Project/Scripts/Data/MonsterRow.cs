using System;
using UnityEngine;

namespace ProjectFirst.Data
{
    [Serializable]
    public class MonsterRow
    {
        public int id;
        public string name;
        public MonsterGrade grade = MonsterGrade.Normal;
        public CombatStats stats;
        public ElementType element = ElementType.Reason;
        public float moveSpeed = -1f;
        public int expReward;
        public int goldReward;
        public GameObject prefab;

        public CombatStats ToCombatStats()
        {
            return stats.Sanitized();
        }
    }
}


