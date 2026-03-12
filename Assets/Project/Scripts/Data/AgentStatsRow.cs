using System;
using UnityEngine;

namespace ProjectFirst.Data
{
    /// <summary>
    /// Documentation cleaned.
    /// </summary>
    [Serializable]
    public class AgentStatsRow
    {
        public int id;
        public string name;
        public CombatStats stats;
        public ElementType element = ElementType.Reason;

        /// <summary>
        /// Documentation cleaned.
        /// </summary>
        public CombatStats ToCombatStats() => stats.Sanitized();
    }
}

