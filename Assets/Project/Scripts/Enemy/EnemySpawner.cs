using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EnemySpawner
/// - spawnInterval(초)마다 spawnPoints 중 임의 위치에서 EnemyPool.Get으로 적 생성
/// </summary>
[AddComponentMenu("Enemy/Enemy Spawner")]
public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Pool (필수)")]
    [Tooltip("EnemyPool 컴포넌트를 연결하세요.")]
    public EnemyPool enemyPool;

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

    private float spawnTimer;

    void Awake()
    {
        if (enemyPool == null)
        {
            enemyPool = EnemyPool.Instance;
        }
    }

    void Update()
    {
        if (!IsSpawnerReady()) return;

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            SpawnEnemy();
            spawnTimer = 0f;
        }
    }

    private bool IsSpawnerReady()
    {
        if (enemyPool == null)
        {
            Debug.LogError("[EnemySpawner] enemyPool이 할당되지 않았습니다.");
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

    private void SpawnEnemy()
    {
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

        int idx = Random.Range(0, validSpawnPoints.Count);
        Transform spawnPoint = validSpawnPoints[idx];

        Enemy enemy = enemyPool.Get(spawnPoint.position, Quaternion.identity, arkTarget);
        if (enemy == null)
        {
            Debug.LogError("[EnemySpawner] EnemyPool.Get 실패로 적 스폰에 실패했습니다.");
        }
    }
}
