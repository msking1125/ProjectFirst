using UnityEngine;

public class Firerailgun : MonoBehaviour
{
    public GameObject railgunPrefab;
    public Transform firePoint;
    public float launchSpeed = 30f;

    public void FireRailgun()
    {
        if (railgunPrefab != null && firePoint != null)
        {
            // [위치 보정] 0.5f에서 1.5f로 값을 키웠습니다. 
            // 여전히 몸을 뚫는다면 이 숫자를 2.0f, 2.5f 식으로 더 키우세요.
            Vector3 spawnPos = firePoint.position + (firePoint.forward * 1.5f);

            // [회전 보정] X축으로 누워있는 이펙트 방향 수정
            Quaternion correction = Quaternion.Euler(0, 90, 0);
            GameObject projectile = Instantiate(railgunPrefab, spawnPos, firePoint.rotation * correction);

            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = firePoint.forward * launchSpeed;
            }
        }
    }
} // 괄호 개수를 맞춰서 CS1513 에러를 해결했습니다.