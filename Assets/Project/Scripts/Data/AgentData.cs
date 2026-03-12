using UnityEngine;

namespace ProjectFirst.Data
{
    /// <summary>
    ///
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Agent Data", fileName = "AgentData_New")]
    public class AgentData : ScriptableObject
    {
        public int agentId = 1;

        public string displayName;

        public Sprite portrait;

        public GameObject normalAttackVfxPrefab;

        public Vector3 normalAttackVfxOffset = new Vector3(0f, 1f, 1f);

        public float normalAttackVfxLifetime = 2f;

        [Range(0f, 1f)]
        public float hitTiming = 0.3f;

        public int characterSkillId;

        public Sprite characterSkillIcon;

        public GameObject characterSkillVfxPrefab;

    }
}

