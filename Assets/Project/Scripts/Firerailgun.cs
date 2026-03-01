using UnityEngine;

public class Firerailgun : MonoBehaviour
{
    [Header("공통 설정")]
    public Transform firePoint;
    public float searchRange = 10f;
    public float forwardOffset = 0.5f;

    [Header("평타 (기본 공격)")]
    public GameObject railgunPrefab;
    public float launchSpeed = 30f;
    public Vector3 normalRotationOffset = new Vector3(0, 90, 0);
    [Tooltip("평타 이펙트가 하이라키에서 지워지는 시간(초)입니다.")]
    public float normalDestroyTime = 2f;

    [Header("스킬 (궁극기)")]
    public GameObject skillPrefab;
    public float skillLaunchSpeed = 30f;
    public Vector3 skillRotationOffset = new Vector3(90, 0, 0);
    [Tooltip("스킬 이펙트가 하이라키에서 지워지는 시간(초)입니다.")]
    public float skillDestroyTime = 3f; // 👉 스킬 전용 삭제 시간!

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