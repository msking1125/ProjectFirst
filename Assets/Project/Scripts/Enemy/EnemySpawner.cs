using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EnemySpawner
/// ---------------------------
/// [사용 가이드]
/// 1. Hierarchy에 EnemySpawner를 빈 오브젝트로 추가하십시오.
/// 2. Inspector에서 enemyPrefab(Enemy 프리팹)과 arkTarget(목표 Transform)을 연결하세요.
/// 3. spawnPoints 배열에 생성될 위치의 Transform들을 추가하세요.
/// 4. spawnInterval을 원하는 스폰 주기로 설정하십시오.
/// 
/// [작동 방식]
/// - spawnInterval(초)마다 spawnPoints 중 임의의 위치에 enemyPrefab을 Instantiate.
/// - Instantiate된 Enemy는 Init(arkTarget)으로 목표를 할당받음.
/// - spawnPoints가 비어있거나 null이면 에러 로그 출력.
/// 
/// [최적화 및 방지 사항]
/// - spawnPoints가 유효한지 항상 확인 (null/0 체크)
/// - enemyPrefab이 Enemy 컴포넌트를 갖고 있는지 확인
/// </summary>
[AddComponentMenu("Enemy/Enemy Spawner")]
public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy 프리팹 (필수)")]
    [Tooltip("생성할 적 프리팹을 등록하세요 (Enemy.cs 필수).")]
    public GameObject enemyPrefab;

    [Header("타겟 (Ark) (필수)")]
    [Tooltip("적이 추격할 타겟(Ark)의 Transform을 지정하세요.")]
    public Transform arkTarget;

    [Header("Spawn Points (필수)")]
    [Tooltip("적이 생성될 위치들의 Transform 배열을 등록하세요.")]
    public Transform[] spawnPoints;

    [Header("Spawn Option")]
    [Tooltip("스폰 주기(초)를 설정하세요.")]
    [Min(0.1f)]
    public float spawnInterval = 2f;

    private float spawnTimer = 0f;

    void Update()
    {
        // 스폰 포인트와 프리팹이 세팅되어 있을 때만 동작
        if (!IsSpawnerReady()) return;

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            SpawnEnemy();
            spawnTimer = 0f;
        }
    }

    /// <summary>
    /// 스폰 포인트 및 프리팹, 타겟 유효성 체크
    /// </summary>
    /// <returns>스폰 가능 여부</returns>
    private bool IsSpawnerReady()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("[EnemySpawner] enemyPrefab이 할당되지 않았습니다.");
            return false;
        }
        if (arkTarget == null)
        {
            Debug.LogError("[EnemySpawner] arkTarget이 할당되지 않았습니다.");
            return false;
        }
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("[EnemySpawner] spawnPoints가 비어있습니다. SpawnPoints를 연결하세요.");
            return false;
        }

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (spawnPoints[i] != null)
            {
                return true;
            }
        }

        Debug.LogError("[EnemySpawner] spawnPoints에 유효한 Transform이 없습니다.");
        return false;
    }

    /// <summary>
    /// 적을 하나 스폰하는 함수.
    /// </summary>
    private void SpawnEnemy()
    {
        // 유효한 스폰 포인트만 후보로 사용
        List<Transform> validSpawnPoints = new List<Transform>();
        foreach (Transform point in spawnPoints)
        {
            if (point != null)
            {
                validSpawnPoints.Add(point);
            }
        }

        if (validSpawnPoints.Count == 0)
        {
            Debug.LogError("[EnemySpawner] spawnPoints에 유효한 Transform이 없습니다.");
            return;
        }

        // 랜덤 스폰 위치 선정
        int idx = Random.Range(0, validSpawnPoints.Count);
        Transform spawnPoint = validSpawnPoints[idx];

        // 적 생성
        GameObject enemyObj = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);

        // Enemy 컴포넌트 확인
        Enemy enemyComponent = enemyObj.GetComponent<Enemy>();
        if (enemyComponent == null)
        {
            Debug.LogError("[EnemySpawner] enemyPrefab에 Enemy 컴포넌트가 없습니다.");
            Destroy(enemyObj);
            return;
        }

        // 타겟 지정 및 초기화
        enemyComponent.Init(arkTarget);
    }
}
