using UnityEngine;

namespace Project
{
    public class Firerailgun : MonoBehaviour
    {
        [Header("Settings")]
        public Transform firePoint;
        public float searchRange = 10f;
        public float forwardOffset = 0.5f;
        [Header("Settings")]
        public GameObject railgunPrefab;
        public float launchSpeed = 30f;
        public Vector3 normalRotationOffset = new Vector3(0, 90, 0);
        public float normalDestroyTime = 2f;
        [Header("Settings")]
        public GameObject skillPrefab;
        public float skillLaunchSpeed = 30f;
        public Vector3 skillRotationOffset = new Vector3(90, 0, 0);
        public float skillDestroyTime = 3f;
    public void FireRailgun()
    {
        SpawnProjectile(railgunPrefab, launchSpeed, normalRotationOffset, normalDestroyTime);
    }
    public void FireSkillRailgun()
    {
        SpawnProjectile(skillPrefab, skillLaunchSpeed, skillRotationOffset, skillDestroyTime);
    }
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
            Destroy(projectile, destroyTime);
        }
        else
        {
            Debug.LogWarning("[Log] 경고가 발생했습니다.");
        }
    }
}
} // namespace Project



