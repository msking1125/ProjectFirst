using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace ProjectFirst.Data
{
    /// <summary>
    /// 캐릭터 1명당 1개씩 만드는 데이터 에셋.
    /// 공격 VFX, 히트 타이밍, 고유 액티브 스킬 등을 관리합니다.
    ///
    /// 생성: Project 우클릭 → Create → Soul Ark/Agent Data
    /// 경로 권장: Assets/Project/Data/Agents/AgentData_캐릭터명.asset
    /// </summary>
#if ODIN_INSPECTOR
    [CreateAssetMenu(menuName = "Soul Ark/Agent Data", fileName = "AgentData_New")]
#else
    [CreateAssetMenu(menuName = "Game/Agent Data", fileName = "AgentData_New")]
#endif
    public class AgentData : ScriptableObject
    {
#if ODIN_INSPECTOR
        [Title("기본 정보", TitleAlignment = TitleAlignments.Left)]
        [HorizontalGroup("기본", 0.5f)]
        [BoxGroup("기본/ID")]
        [LabelText("Agent ID")]
        [Tooltip("Agent.agentId와 일치해야 합니다.")]
#endif
        public int agentId = 1;

#if ODIN_INSPECTOR
        [HorizontalGroup("기본", 0.5f)]
        [BoxGroup("기본/이름")]
        [LabelText("표시 이름")]
#endif
        public string displayName;

#if ODIN_INSPECTOR
        [BoxGroup("기본")]
        [LabelText("초상화")]
        [Tooltip("캐릭터 초상화(프로필/로비 UI 등에서 사용)")]
        [PreviewField(80, ObjectFieldAlignment.Left)]
#endif
        public Sprite portrait;

#if ODIN_INSPECTOR
        [Title("기본 공격 VFX", TitleAlignment = TitleAlignments.Left)]
        [BoxGroup("VFX")]
        [LabelText("VFX 프리팹")]
        [Tooltip("기본 공격 시 스폰할 이펙트 프리팹")]
        [AssetsOnly]
        [PreviewField(60, ObjectFieldAlignment.Left)]
#endif
        public GameObject normalAttackVfxPrefab;

#if ODIN_INSPECTOR
        [BoxGroup("VFX")]
        [LabelText("스폰 오프셋")]
        [Tooltip("VFX 스폰 위치 오프셋 (캐릭터 기준)")]
#endif
        public Vector3 normalAttackVfxOffset = new Vector3(0f, 1f, 1f);

#if ODIN_INSPECTOR
        [HorizontalGroup("VFX설정", 0.5f)]
        [BoxGroup("VFX설정/시간")]
        [LabelText("VFX 지속시간")]
        [SuffixLabel("초", true)]
        [Tooltip("VFX 자동 소멸 시간 (초). 0이면 소멸 안 함")]
#endif
        public float normalAttackVfxLifetime = 2f;

#if ODIN_INSPECTOR
        [HorizontalGroup("VFX설정", 0.5f)]
        [BoxGroup("VFX설정/타이밍")]
        [LabelText("히트 타이밍")]
        [ProgressBar(0, 1, ColorGetter = "GetHitTimingColor")]
        [Tooltip("공격 모션에서 실제 타격이 발생하는 타이밍 비율 (0=시작, 1=끝). 0.3이면 애니의 30% 지점에 데미지")]
#endif
        [Range(0f, 1f)]
        public float hitTiming = 0.3f;

#if ODIN_INSPECTOR
        [Title("액티브 스킬", TitleAlignment = TitleAlignments.Left)]
        [HorizontalGroup("스킬", 0.5f)]
        [BoxGroup("스킬/ID")]
        [LabelText("스킬 ID")]
        [Tooltip("이 캐릭터 전용 액티브 스킬 ID. SkillTable에서 탐색합니다.")]
#endif
        public int characterSkillId;

#if ODIN_INSPECTOR
        [HorizontalGroup("스킬", 0.5f)]
        [BoxGroup("스킬/아이콘")]
        [LabelText("스킬 아이콘")]
        [Tooltip("캐릭터 고유 스킬 아이콘 (UI 버튼에 표시)")]
        [PreviewField(60, ObjectFieldAlignment.Left)]
#endif
        public Sprite characterSkillIcon;

#if ODIN_INSPECTOR
        [BoxGroup("스킬")]
        [LabelText("스킬 VFX")]
        [Tooltip("캐릭터 고유 스킬 VFX 프리팹")]
        [AssetsOnly]
        [PreviewField(60, ObjectFieldAlignment.Left)]
#endif
        public GameObject characterSkillVfxPrefab;

#if ODIN_INSPECTOR
        private static Color GetHitTimingColor() => new Color(1f, 0.6f, 0.2f);
#endif
    }
}
