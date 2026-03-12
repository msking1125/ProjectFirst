using System;
using UnityEngine;

namespace ProjectFirst.Data
{
    [Serializable]
    public class AgentStatsRow
    {
        public int id;
        public string name;
        public CombatStats stats;
        public ElementType element = ElementType.Reason;
        public CombatStats ToCombatStats() => stats.Sanitized();
    }
}


