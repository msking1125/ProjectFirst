using UnityEngine;

namespace Project
{

/// <summary>
/// ?덉씪嫄?諛쒖궗 而⑦듃濡ㅻ윭
/// </summary>
    public class Firerailgun : MonoBehaviour
    {
        [Header("怨듯넻 ?ㅼ젙")]
        public Transform firePoint;
        public float searchRange = 10f;
        public float forwardOffset = 0.5f;

        // ?? ?됲? ????????????????????????????????????????????????????????????????
        [Header("?됲? (湲곕낯 怨듦꺽)")]
        public GameObject railgunPrefab;
        public float launchSpeed = 30f;
        public Vector3 normalRotationOffset = new Vector3(0, 90, 0);
        public float normalDestroyTime = 2f;

        // ?? ?ㅽ궗 ????????????????????????????????????????????????????????????????
        [Header("?ㅽ궗 (沅곴레湲?")]
        public GameObject skillPrefab;
        public float skillLaunchSpeed = 30f;
        public Vector3 skillRotationOffset = new Vector3(90, 0, 0);
        public float skillDestroyTime = 3f;

    // ?? ?됲? 諛쒖궗 ??
    public void FireRailgun()
    {
        SpawnProjectile(railgunPrefab, launchSpeed, normalRotationOffset, normalDestroyTime);
    }

    // ?? ?ㅽ궗 諛쒖궗 ??
    public void FireSkillRailgun()
    {
        SpawnProjectile(skillPrefab, skillLaunchSpeed, skillRotationOffset, skillDestroyTime);
    }

    // ?? 諛쒖궗 諛???젣 怨듯넻 濡쒖쭅 ??
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
                    // 紐ъ뒪?곗쓽 以묒떖(?댁쭩 ?꾩そ) 議곗?
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

            // [?듭떖] ?ㅼ젙???쒓컙(destroyTime)??吏?섎㈃ ?대줎???꾨꼍?섍쾶 ?뚭눼?⑸땲??
            Destroy(projectile, destroyTime);
        }
        else
        {
            Debug.LogWarning("[Firerailgun] ?꾨━?뱀씠??FirePoint媛 鍮꾩뼱?덉뼱 諛쒖궗?????놁뒿?덈떎!");
        }
    }
}
} // namespace Project


