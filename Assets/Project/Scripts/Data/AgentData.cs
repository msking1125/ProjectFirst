using UnityEngine;

/// <summary>
/// 캐릭터 1명당 1개씩 만드는 데이터 에셋.
/// 공격 VFX, 히트 타이밍, 고유 액티브 스킬 등을 관리합니다.
///
/// 생성: Project 우클릭 → Create → Game/Agent Data
/// 경로 권장: Assets/Project/Data/Agents/AgentData_캐릭터명.asset
/// </summary>
[CreateAssetMenu(menuName = "Game/Agent Data", fileName = "AgentData_New")]
public class AgentData : ScriptableObject
{
    [Header("기본 정보")]
    [Tooltip("Agent.agentId와 일치해야 합니다.")]
    public string agentId;
    public string displayName;

    [Header("기본 공격 VFX")]
    [Tooltip("기본 공격 시 스폰할 이펙트 프리팹")]
    public GameObject normalAttackVfxPrefab;

    [Tooltip("VFX 스폰 위치 오프셋 (캐릭터 기준)")]
    public Vector3 normalAttackVfxOffset = new Vector3(0f, 1f, 1f);

    [Tooltip("VFX 자동 소멸 시간 (초). 0이면 소멸 안 함")]
    public float normalAttackVfxLifetime = 2f;

    [Tooltip("공격 모션에서 실제 타격이 발생하는 타이밍 비율 (0=시작, 1=끝). 0.3이면 애니의 30% 지점에 데미지")]
    [Range(0f, 1f)]
    public float hitTiming = 0.3f;

    [Header("액티브 스킬 (캐릭터 고유)")]
    [Tooltip("이 캐릭터 전용 액티브 스킬 ID. SkillTable에서 탐색합니다.")]
    public string characterSkillId;

    [Tooltip("캐릭터 고유 스킬 아이콘 (UI 버튼에 표시)")]
    public Sprite characterSkillIcon;

    [Tooltip("캐릭터 고유 스킬 VFX 프리팹")]
    public GameObject characterSkillVfxPrefab;
}
