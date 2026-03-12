using UnityEngine;

namespace ProjectFirst.Data
{
    /// <summary>
    /// 캐릭터 1명당 1개씩 만드는 데이터 에셋.
    /// 공격 VFX, 히트 타이밍, 고유 액티브 스킬 등을 관리합니다.
    ///
    /// 생성: Project 우클릭 → Create → Soul Ark/Agent Data
    /// 경로 권장: Assets/Project/Data/Agents/AgentData_캐릭터명.asset
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
