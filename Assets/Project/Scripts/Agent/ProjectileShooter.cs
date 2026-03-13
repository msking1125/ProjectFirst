using UnityEngine;
using ProjectFirst.Data;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Project
{
    /// <summary>
    /// 일반 공격/스킬 투사체를 발사하는 컴포넌트.
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
        [LabelText("발사 위치")]
        [SceneObjectsOnly]
#endif
        [Header("공통")]
        [Tooltip("비워두면 이 트랜스폼을 사용합니다")] // 한글, UTF-8
        [SerializeField] private Transform firePoint;

        private void Awake()
        {
            if (firePoint == null)
                firePoint = transform;
        }

#if ODIN_INSPECTOR
        [HorizontalGroup("common", 0.5f)]
        [BoxGroup("common/search")]
        [LabelText("탐지 범위")]
        [PropertyRange(1f, 50f)]
        [SuffixLabel("m", true)]
#endif
        [SerializeField] private float searchRange = 10f;

#if ODIN_INSPECTOR
        [BoxGroup("common/spawn")]
        [LabelText("전방 오프셋")]
        [PropertyRange(0f, 2f)]
#endif
        [SerializeField] private float forwardOffset = 0.5f;

#if ODIN_INSPECTOR
        [Title("일반 공격", TitleAlignment = TitleAlignments.Left)]
        [BoxGroup("normal")]
        [LabelText("일반 공격 프리팹")]
        [AssetsOnly]
#endif
        [Header("일반 공격")]
        [Tooltip("일반 공격 투사체 프리팹을 지정하세요")] // 한글, UTF-8
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
        [LabelText("일반 공격 소멸 시간")]
        [PropertyRange(0.1f, 10f)]
        [SuffixLabel("초", true)]
#endif
        [SerializeField] private float normalAttackDestroyTime = 2f;

#if ODIN_INSPECTOR
        [Title("스킬 공격", TitleAlignment = TitleAlignments.Left)]
        [BoxGroup("skill")]
        [LabelText("스킬 프리팹")]
        [AssetsOnly]
#endif
        [Header("스킬 공격")]
        [Tooltip("스킬 투사체 프리팹을 지정하세요")] // 한글, UTF-8
        [SerializeField] private GameObject skillAttackPrefab;

#if ODIN_INSPECTOR
        [HorizontalGroup("skillsettings", 0.5f)]
        [BoxGroup("skillsettings/speed")]
        [LabelText("스킬 속도")]
        [PropertyRange(1f, 100f)]
        [SuffixLabel("m/s", true)]
#endif
        [SerializeField] private float skillAttackSpeed = 30f;

#if ODIN_INSPECTOR
        [HorizontalGroup("skillsettings", 0.5f)]
        [BoxGroup("skillsettings/rotation")]
        [LabelText("스킬 회전 오프셋")]
#endif
        [SerializeField] private Vector3 skillAttackRotationOffset = new Vector3(90f, 0f, 0f);

#if ODIN_INSPECTOR
        [BoxGroup("skill")]
        [LabelText("스킬 소멸 시간")]
        [PropertyRange(0.1f, 10f)]
        [SuffixLabel("초", true)]
#endif
        [SerializeField] private float skillAttackDestroyTime = 3f;

        /// <summary>
        /// AgentData에 발사체 프리팹이 있으면 적용. 프리팹에 이미 할당된 값은 비어 있을 때만 덮어씀.
        /// </summary>
        public void SetPrefabsFromAgentData(AgentData data)
        {
            if (data == null) return;
            if (data.normalAttackProjectilePrefab != null && normalAttackPrefab == null)
                normalAttackPrefab = data.normalAttackProjectilePrefab;
            if (data.skillProjectilePrefab != null && skillAttackPrefab == null)
                skillAttackPrefab = data.skillProjectilePrefab;
        }

        public bool CanFireNormalAttack() => firePoint != null && normalAttackPrefab != null;

        public bool CanFireSkillAttack() => firePoint != null && skillAttackPrefab != null;

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

        private bool SpawnProjectile(GameObject prefab, float speed, Vector3 rotationOffset, float destroyTime)
        {
            if (prefab == null)
            {
                Debug.LogWarning($"[ProjectileShooter] 프리팹이 {name}에서 누락되었습니다"); // 한글, UTF-8
                return false;
            }

            Transform spawn = firePoint != null ? firePoint : transform;
            Vector3 shootDirection = spawn.forward;

            if (EnemyManager.Instance != null)
            {
                Enemy target = EnemyManager.Instance.GetClosest(transform.position, searchRange);
                if (target != null)
                {
                    Vector3 targetPos = target.transform.position + Vector3.up * 1f;
                    shootDirection = (targetPos - spawn.position).normalized;
                }
            }

            Vector3 spawnPosition = spawn.position + (shootDirection * forwardOffset);
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
            else
            {
                ProjectileMovement movement = projectile.GetComponent<ProjectileMovement>();
                if (movement == null)
                    movement = projectile.AddComponent<ProjectileMovement>();
                movement.SetDirectionAndSpeed(shootDirection, speed);
            }

            Destroy(projectile, destroyTime);
            return true;
        }
    }

    /// <summary>
    /// Rigidbody 없이 발사체 이동용.
    /// </summary>
    public class ProjectileMovement : MonoBehaviour
    {
        private Vector3 _direction;
        private float _speed;

        public void SetDirectionAndSpeed(Vector3 dir, float spd)
        {
            _direction = dir.normalized;
            _speed = spd;
        }

        private void Update()
        {
            if (_direction != Vector3.zero && _speed > 0f)
                transform.position += _direction * (_speed * Time.deltaTime);
        }
    }
}
