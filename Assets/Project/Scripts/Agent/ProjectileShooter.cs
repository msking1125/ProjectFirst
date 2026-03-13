using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Project
{
    /// <summary>
    /// 일반 공격/스킬용 투사체를 발사하는 공용 컴포넌트.
    /// </summary>
#if ODIN_INSPECTOR
    [HideMonoScript]
#endif
    public class ProjectileShooter : MonoBehaviour
    {
#if ODIN_INSPECTOR
        [Title("공통 설정", TitleAlignment = TitleAlignments.Left)]
        [HorizontalGroup("common", 0.5f)]
        [BoxGroup("common/spawn")]
        [LabelText("발사 지점")]
        [SceneObjectsOnly]
#endif
        [Header("common")]
        [SerializeField] private Transform firePoint;

#if ODIN_INSPECTOR
        [HorizontalGroup("common", 0.5f)]
        [BoxGroup("common/search")]
        [LabelText("탐색 범위")]
        [PropertyRange(1f, 50f)]
        [SuffixLabel("m", true)]
#endif
        [SerializeField] private float searchRange = 10f;

#if ODIN_INSPECTOR
        [BoxGroup("common/spawn")]
        [LabelText("전진 오프셋")]
        [PropertyRange(0f, 2f)]
#endif
        [SerializeField] private float forwardOffset = 0.5f;

#if ODIN_INSPECTOR
        [Title("일반 공격", TitleAlignment = TitleAlignments.Left)]
        [BoxGroup("normal")]
        [LabelText("일반 공격 프리팹")]
        [AssetsOnly]
#endif
        [Header("normal")]
        [SerializeField] private GameObject normalAttackPrefab;

#if ODIN_INSPECTOR
        [HorizontalGroup("normalsettings", 0.5f)]
        [BoxGroup("normalsettings/speed")]
        [LabelText("일반 공격 속도")]
        [PropertyRange(1f, 100f)]
        [SuffixLabel("m/s", true)]
#endif
        [SerializeField] private float normalAttackSpeed = 30f;

#if ODIN_INSPECTOR
        [HorizontalGroup("normalsettings", 0.5f)]
        [BoxGroup("normalsettings/rotation")]
        [LabelText("일반 공격 회전 오프셋")]
#endif
        [SerializeField] private Vector3 normalAttackRotationOffset = new Vector3(0f, 90f, 0f);

#if ODIN_INSPECTOR
        [BoxGroup("normal")]
        [LabelText("일반 공격 지속 시간")]
        [PropertyRange(0.1f, 10f)]
        [SuffixLabel("초", true)]
#endif
        [SerializeField] private float normalAttackDestroyTime = 2f;

#if ODIN_INSPECTOR
        [Title("스킬 공격", TitleAlignment = TitleAlignments.Left)]
        [BoxGroup("skill")]
        [LabelText("스킬 공격 프리팹")]
        [AssetsOnly]
#endif
        [Header("skill")]
        [SerializeField] private GameObject skillAttackPrefab;

#if ODIN_INSPECTOR
        [HorizontalGroup("skillsettings", 0.5f)]
        [BoxGroup("skillsettings/speed")]
        [LabelText("스킬 공격 속도")]
        [PropertyRange(1f, 100f)]
        [SuffixLabel("m/s", true)]
#endif
        [SerializeField] private float skillAttackSpeed = 30f;

#if ODIN_INSPECTOR
        [HorizontalGroup("skillsettings", 0.5f)]
        [BoxGroup("skillsettings/rotation")]
        [LabelText("스킬 공격 회전 오프셋")]
#endif
        [SerializeField] private Vector3 skillAttackRotationOffset = new Vector3(90f, 0f, 0f);

#if ODIN_INSPECTOR
        [BoxGroup("skill")]
        [LabelText("스킬 공격 지속 시간")]
        [PropertyRange(0.1f, 10f)]
        [SuffixLabel("초", true)]
#endif
        [SerializeField] private float skillAttackDestroyTime = 3f;

        public bool CanFireNormalAttack() => IsConfigured(normalAttackPrefab);

        public bool CanFireSkillAttack() => IsConfigured(skillAttackPrefab);

        public bool FireNormalAttack()
        {
            return SpawnProjectile(
                normalAttackPrefab,
                normalAttackSpeed,
                normalAttackRotationOffset,
                normalAttackDestroyTime
            );
        }

        public bool FireSkillAttack()
        {
            return SpawnProjectile(
                skillAttackPrefab,
                skillAttackSpeed,
                skillAttackRotationOffset,
                skillAttackDestroyTime
            );
        }

        private bool IsConfigured(GameObject prefab)
        {
            return prefab != null && firePoint != null;
        }

        private bool SpawnProjectile(GameObject prefab, float speed, Vector3 rotationOffset, float destroyTime)
        {
            if (prefab == null)
            {
                Debug.LogWarning($"[ProjectileShooter] prefab is missing on {name}");
                return false;
            }

            if (firePoint == null)
            {
                Debug.LogWarning($"[ProjectileShooter] firePoint is missing on {name}");
                return false;
            }

            Vector3 shootDirection = firePoint.forward;

            if (EnemyManager.Instance != null)
            {
                Enemy target = EnemyManager.Instance.GetClosest(transform.position, searchRange);
                if (target != null)
                {
                    Vector3 targetPos = target.transform.position + Vector3.up * 1f;
                    shootDirection = (targetPos - firePoint.position).normalized;
                }
            }

            Vector3 spawnPosition = firePoint.position + (shootDirection * forwardOffset);

            Quaternion lookRotation = Quaternion.LookRotation(shootDirection);
            Quaternion correctionRotation = Quaternion.Euler(rotationOffset);

            GameObject projectile = Instantiate(
                prefab,
                spawnPosition,
                lookRotation * correctionRotation
            );

            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
#if UNITY_6000_0_OR_NEWER
                rb.linearVelocity = shootDirection * speed;
#else
                rb.velocity = shootDirection * speed;
#endif
            }

            Destroy(projectile, destroyTime);
            return true;
        }
    }
}
