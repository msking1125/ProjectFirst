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

        [Header("Projectiles (optional, overrides prefab defaults)")]
        [Tooltip("일반 공격 발사체 프리팹. 비어 있으면 캐릭터 프리팹의 ProjectileShooter 설정 사용")]
        public GameObject normalAttackProjectilePrefab;

        [Tooltip("스킬 발사체 프리팹. 비어 있으면 캐릭터 프리팹의 ProjectileShooter 설정 사용")]
        public GameObject skillProjectilePrefab;

        [Range(0f, 1f)]
        public float hitTiming = 0.3f;

        public int characterSkillId;

        public Sprite characterSkillIcon;

        public GameObject characterSkillVfxPrefab;

    }
}

