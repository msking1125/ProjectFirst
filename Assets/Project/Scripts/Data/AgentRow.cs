using UnityEngine;
using System;

namespace ProjectFirst.Data
{
    /// <summary>
    /// Documentation cleaned.
    /// </summary>
    [Serializable]
    public class AgentRow
    {
        public int id;
        public string name;
        public CombatStats stats;
        public ElementType element = ElementType.Reason;
        public Sprite portrait;

        public CombatStats ToCombatStats()
        {
            return stats.Sanitized();
        }
    }
}

