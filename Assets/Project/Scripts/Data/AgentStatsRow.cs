using System;
using UnityEngine;

namespace ProjectFirst.Data
{
    /// <summary>
    /// ?먯씠?꾪듃(罹먮┃?? ?ㅽ꺈 ?곗씠????
    /// </summary>
    [Serializable]
    public class AgentStatsRow
    {
        public int id;
        public string name;
        public CombatStats stats;
        public ElementType element = ElementType.Reason;

        /// <summary>
        /// CombatStats濡?蹂?섑빀?덈떎.
        /// </summary>
        public CombatStats ToCombatStats() => stats.Sanitized();
    }
}

