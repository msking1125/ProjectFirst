using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Project
{

/// <summary>
/// 레일건 발사 컨트롤러
/// </summary>
#if ODIN_INSPECTOR
    [HideMonoScript]
#endif
    public class Firerailgun : MonoBehaviour
    {
#if ODIN_INSPECTOR
        [Title("공통 설정", TitleAlignment = TitleAlignments.Left)]
        [HorizontalGroup("공통", 0.5f)]
        [BoxGroup("공통/발사")]
        [LabelText("발사 지점")]
        [Tooltip("투사체가 생성될 위치")]
        [SceneObjectsOnly]
#endif
        [Header("공통 설정")]
        public Transform firePoint;

#if ODIN_INSPECTOR
        [HorizontalGroup("공통", 0.5f)]
        [BoxGroup("공통/범위")]
        [LabelText("탐색 범위")]
        [PropertyRange(1f, 50f)]
        [SuffixLabel("m", true)]
        [Tooltip("적 탐색 범위")]
#endif
        public float searchRange = 10f;

#if ODIN_INSPECTOR
        [BoxGroup("공통")]
        [LabelText("전진 오프셋")]
        [Tooltip("발사 위치 전진 오프셋")]
        [PropertyRange(0f, 2f)]
#endif
        public float forwardOffset = 0.5f;

        // ── 평타 ────────────────────────────────────────────────────────────────
#if ODIN_INSPECTOR
        [Title("평타 (기본 공격)", TitleAlignment = TitleAlignments.Left)]
        [BoxGroup("평타")]
        [LabelText("평타 프리팹")]
        [AssetsOnly]
        [PreviewField(60, ObjectFieldAlignment.Left)]
#endif
        [Header("평타 (기본 공격)")]
        public GameObject railgunPrefab;

#if ODIN_INSPECTOR
        [HorizontalGroup("평타설정", 0.5f)]
        [BoxGroup("평타설정/속도")]
        [LabelText("발사 속도")]
        [PropertyRange(1f, 100f)]
        [SuffixLabel("m/s", true)]
#endif
        public float launchSpeed = 30f;

#if ODIN_INSPECTOR
        [HorizontalGroup("평타설정", 0.5f)]
        [BoxGroup("평타설정/회전")]
        [LabelText("회전 오프셋")]
        [Tooltip("평타 이펙트 회전 오프셋")]
#endif
        public Vector3 normalRotationOffset = new Vector3(0, 90, 0);

#if ODIN_INSPECTOR
        [BoxGroup("평타")]
        [LabelText("지속 시간")]
        [Tooltip("평타 이펙트가 하이라키에서 지워지는 시간(초)")]
        [SuffixLabel("초", true)]
        [PropertyRange(0.1f, 10f)]
#endif
        [Tooltip("평타 이펙트가 하이라키에서 지워지는 시간(초)입니다.")]
        public float normalDestroyTime = 2f;

        // ── 스킬 ────────────────────────────────────────────────────────────────
#if ODIN_INSPECTOR
        [Title("스킬 (궁극기)", TitleAlignment = TitleAlignments.Left)]
        [BoxGroup("스킬")]
        [LabelText("스킬 프리팹")]
        [AssetsOnly]
        [PreviewField(60, ObjectFieldAlignment.Left)]
#endif
        [Header("스킬 (궁극기)")]
        public GameObject skillPrefab;

#if ODIN_INSPECTOR
        [HorizontalGroup("스킬설정", 0.5f)]
        [BoxGroup("스킬설정/속도")]
        [LabelText("발사 속도")]
        [PropertyRange(1f, 100f)]
        [SuffixLabel("m/s", true)]
#endif
        public float skillLaunchSpeed = 30f;

#if ODIN_INSPECTOR
        [HorizontalGroup("스킬설정", 0.5f)]
        [BoxGroup("스킬설정/회전")]
        [LabelText("회전 오프셋")]
        [Tooltip("스킬 이펙트 회전 오프셋")]
#endif
        public Vector3 skillRotationOffset = new Vector3(90, 0, 0);

#if ODIN_INSPECTOR
        [BoxGroup("스킬")]
        [LabelText("지속 시간")]
        [Tooltip("스킬 이펙트가 하이라키에서 지워지는 시간(초)")]
        [SuffixLabel("초", true)]
        [PropertyRange(0.1f, 10f)]
        [GUIColor(1f, 0.6f, 0.2f)]
#endif
        [Tooltip("스킬 이펙트가 하이라키에서 지워지는 시간(초)입니다.")]
        public float skillDestroyTime = 3f;

    // ── 평타 발사 ──
    public void FireRailgun()
    {
        SpawnProjectile(railgunPrefab, launchSpeed, normalRotationOffset, normalDestroyTime);
    }

    // ── 스킬 발사 ──
    public void FireSkillRailgun()
    {
        SpawnProjectile(skillPrefab, skillLaunchSpeed, skillRotationOffset, skillDestroyTime);
    }

    // ── 발사 및 삭제 공통 로직 ──
    private void SpawnProjectile(GameObject prefab, float speed, Vector3 rotationOffset, float destroyTime)
    {
        if (prefab != null && firePoint != null)
        {
            Vector3 shootDirection = firePoint.forward;

            if (EnemyManager.Instance != null)
            {
                Enemy target = EnemyManager.Instance.GetClosest(transform.position, searchRange);
                if (target != null)
                {
                    // 몬스터의 중심(살짝 위쪽) 조준
                    Vector3 targetPos = target.transform.position + Vector3.up * 1f;
                    shootDirection = (targetPos - firePoint.position).normalized;
                }
            }

            Vector3 spawnPos = firePoint.position + (shootDirection * forwardOffset);

            Quaternion targetRotation = Quaternion.LookRotation(shootDirection);
            Quaternion correction = Quaternion.Euler(rotationOffset);

            GameObject projectile = Instantiate(prefab, spawnPos, targetRotation * correction);

            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = shootDirection * speed;
            }

            // [핵심] 설정된 시간(destroyTime)이 지나면 클론을 완벽하게 파괴합니다!
            Destroy(projectile, destroyTime);
        }
        else
        {
            Debug.LogWarning("[Firerailgun] 프리팹이나 FirePoint가 비어있어 발사할 수 없습니다!");
        }
    }
}
} // namespace Project
