using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EnemySpawner
/// - 기본적으로 spawnInterval(초)마다 spawnPoints 중 임의 위치에서 EnemyPool.Get으로 적 생성
/// - WaveManager가 설정하면 해당 웨이브 spawnCount / spawnInterval / 스탯 배수를 사용
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

    [Header("Monster Data")]
    [SerializeField] private MonsterTable monsterTable;

    [Header("Spawn Option")]
    [Tooltip("스폰 주기(초)를 설정하세요.")]
    [Min(0.01f)]
    public float spawnInterval = 2f;

    private float spawnTimer;
    private int waveSpawnCount;
    private int spawnedCount;
    private float enemyHpMul = 1f;
    private float enemySpeedMul = 1f;
    private float enemyDamageMul = 1f;
    private int eliteEvery;
    private bool bossWave;
    private bool useWaveConfig;
    private string currentEnemyId = "slime";

    private bool loggedMissingPool;
    private bool loggedMissingTarget;
    private bool loggedMissingSpawnPoints;

    public bool IsWaveSpawnCompleted => useWaveConfig && spawnedCount >= waveSpawnCount;

    void Awake()
    {
        if (enemyPool == null)
        {
            enemyPool = EnemyPool.Instance;
        }
    }

    void Update()
    {
        if (!IsSpawnerReady())
        {
            return;
        }

        if (useWaveConfig && IsWaveSpawnCompleted)
        {
            return;
        }

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            SpawnEnemy();
            spawnTimer = 0f;
        }
    }

    public void ConfigureWave(int spawnCount, float interval, float hpMul, float speedMul, float damageMul, int eliteEveryCount, bool isBossWave, string enemyId = "slime")
    {
        waveSpawnCount = Mathf.Max(0, spawnCount);
        spawnInterval = Mathf.Max(0.01f, interval);
        enemyHpMul = Mathf.Max(0f, hpMul);
        enemySpeedMul = Mathf.Max(0f, speedMul);
        enemyDamageMul = Mathf.Max(0f, damageMul);
        eliteEvery = Mathf.Max(0, eliteEveryCount);
        bossWave = isBossWave;
        currentEnemyId = string.IsNullOrWhiteSpace(enemyId) ? "slime" : enemyId;
        useWaveConfig = true;
    }

    public void BeginWave()
    {
        spawnTimer = 0f;
        spawnedCount = 0;
    }

    private bool IsSpawnerReady()
    {
        if (enemyPool == null)
        {
            if (!loggedMissingPool)
            {
                Debug.LogError("[EnemySpawner] enemyPool이 할당되지 않았습니다.");
                loggedMissingPool = true;
            }

            return false;
        }

        loggedMissingPool = false;

        if (arkTarget == null)
        {
            if (!loggedMissingTarget)
            {
                Debug.LogError("[EnemySpawner] arkTarget이 할당되지 않았습니다.");
                loggedMissingTarget = true;
            }

            return false;
        }

        loggedMissingTarget = false;

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            if (!loggedMissingSpawnPoints)
            {
                Debug.LogError("[EnemySpawner] spawnPoints가 비어있습니다. SpawnPoints를 연결하세요.");
                loggedMissingSpawnPoints = true;
            }

            return false;
        }

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (spawnPoints[i] != null)
            {
                loggedMissingSpawnPoints = false;
                return true;
            }
        }

        if (!loggedMissingSpawnPoints)
        {
            Debug.LogError("[EnemySpawner] spawnPoints에 유효한 Transform이 없습니다.");
            loggedMissingSpawnPoints = true;
        }

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

        MonsterGrade grade = ResolveGrade();
        WaveMultipliers multipliers = new WaveMultipliers { hp = enemyHpMul, speed = enemySpeedMul, damage = enemyDamageMul };

        MonsterRow row = monsterTable != null ? monsterTable.GetByIdAndGrade(currentEnemyId, grade) : null;
        GameObject prefabOverride = row != null ? row.prefab : null;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        string prefabOverrideName = prefabOverride != null ? prefabOverride.name : "null";
        string defaultPrefabName = enemyPool != null && enemyPool.DefaultEnemyPrefab != null ? enemyPool.DefaultEnemyPrefab.name : "null";
        Debug.Log($"[EnemySpawner] Spawn request. monsterId='{currentEnemyId}', grade='{grade}', prefabOverride='{prefabOverrideName}', defaultPrefab='{defaultPrefabName}'");
#endif
        Enemy enemy = enemyPool.Get(spawnPoint.position, Quaternion.identity, arkTarget, monsterTable, currentEnemyId, grade, multipliers);
        if (enemy == null)
        {
            Debug.LogError("[EnemySpawner] EnemyPool.Get 실패로 적 스폰에 실패했습니다.");
            return;
        }

        if (useWaveConfig)
        {
            spawnedCount++;
        }
    }

    private MonsterGrade ResolveGrade()
    {
        if (bossWave)
        {
            return MonsterGrade.Boss;
        }

        if (eliteEvery > 0 && (spawnedCount + 1) % eliteEvery == 0)
        {
            return MonsterGrade.Elite;
        }

        return MonsterGrade.Normal;
    }
}
