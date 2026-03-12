using UnityEngine;

namespace Project
{

/// <summary>
/// Documentation cleaned.
/// </summary>
    public class Firerailgun : MonoBehaviour
    {
        [Header("Settings")]
        public Transform firePoint;
        public float searchRange = 10f;
        public float forwardOffset = 0.5f;

        // Note: cleaned comment.
        [Header("Settings")]
        public GameObject railgunPrefab;
        public float launchSpeed = 30f;
        public Vector3 normalRotationOffset = new Vector3(0, 90, 0);
        public float normalDestroyTime = 2f;

        // Note: cleaned comment.
        [Header("Settings")]
        public GameObject skillPrefab;
        public float skillLaunchSpeed = 30f;
        public Vector3 skillRotationOffset = new Vector3(90, 0, 0);
        public float skillDestroyTime = 3f;

    // Note: cleaned comment.
    public void FireRailgun()
    {
        SpawnProjectile(railgunPrefab, launchSpeed, normalRotationOffset, normalDestroyTime);
    }

    // Note: cleaned comment.
    public void FireSkillRailgun()
    {
        SpawnProjectile(skillPrefab, skillLaunchSpeed, skillRotationOffset, skillDestroyTime);
    }

    // Note: cleaned comment.
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
                    // Note: cleaned comment.
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

            // Note: cleaned comment.
            Destroy(projectile, destroyTime);
        }
        else
        {
            Debug.LogWarning("[Log] Warning message cleaned.");
        }
    }
}
} // namespace Project


