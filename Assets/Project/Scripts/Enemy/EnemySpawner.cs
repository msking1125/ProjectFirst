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
    private const string DefaultMonsterId = "1";

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

    private float fallbackSpawnTimer;
    private bool useWaveConfig;
    [SerializeField] private string defaultMonsterId = DefaultMonsterId;

    [System.Serializable]
    public class WaveSession
    {
        public int waveSpawnCount;
        public float spawnInterval;
        public float enemyHpMul;
        public float enemySpeedMul;
        public float enemyDamageMul;
        public int eliteEvery;
        public bool bossWave;
        public string currentEnemyId;
        public string lastConfiguredEnemyId;

        public float spawnTimer;
        public int spawnedCount;

        public bool IsCompleted => spawnedCount >= waveSpawnCount;
    }

    private List<WaveSession> waveSessions = new List<WaveSession>();

    private bool loggedMissingPool;
    private bool loggedMissingTarget;
    private bool loggedMissingSpawnPoints;
    private bool loggedMissingMonsterTable;

    public bool IsWaveSpawnCompleted
    {
        get
        {
            if (!useWaveConfig) return false;
            foreach (var session in waveSessions)
            {
                if (!session.IsCompleted) return false;
            }
            return true;
        }
    }

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

        if (useWaveConfig)
        {
            foreach (var session in waveSessions)
            {
                if (session.IsCompleted) continue;

                session.spawnTimer += Time.deltaTime;
                if (session.spawnTimer >= session.spawnInterval)
                {
                    SpawnEnemy(session);
                    session.spawnTimer = 0f;
                }
            }
        }
        else
        {
            fallbackSpawnTimer += Time.deltaTime;
            if (fallbackSpawnTimer >= spawnInterval)
            {
                SpawnEnemyFallback();
                fallbackSpawnTimer = 0f;
            }
        }
    }

    public void ConfigureWave(List<WaveRow> rows)
    {
        waveSessions.Clear();
        useWaveConfig = true;
        
        defaultMonsterId = string.IsNullOrWhiteSpace(defaultMonsterId) ? DefaultMonsterId : defaultMonsterId;

        foreach (var row in rows)
        {
            var session = new WaveSession
            {
                waveSpawnCount = Mathf.Max(0, row.spawnCount),
                spawnInterval = Mathf.Max(0.01f, row.spawnInterval),
                enemyHpMul = Mathf.Max(0f, row.enemyHpMul),
                enemySpeedMul = Mathf.Max(0f, row.enemySpeedMul),
                enemyDamageMul = Mathf.Max(0f, row.enemyDamageMul),
                eliteEvery = Mathf.Max(0, row.eliteEvery),
                bossWave = row.boss,
                lastConfiguredEnemyId = row.enemyId,
                currentEnemyId = ResolveMonsterIdOrFallback(row.GetMonsterIdOrFallback(), defaultMonsterId),
                spawnTimer = 0f,
                spawnedCount = 0
            };
            waveSessions.Add(session);
        }
    }

    public void BeginWave()
    {
        foreach (var session in waveSessions)
        {
            session.spawnTimer = 0f;
            session.spawnedCount = 0;
        }
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

        if (monsterTable == null)
        {
            if (!loggedMissingMonsterTable)
            {
                Debug.LogError("[EnemySpawner] monsterTable이 할당되지 않았습니다. 'Monster Data' 섹션에 MonsterTable 에셋을 연결하세요.");
                loggedMissingMonsterTable = true;
            }

            return false;
        }

        loggedMissingMonsterTable = false;

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

    private void SpawnEnemy(WaveSession session)
    {
        Transform spawnPoint = GetRandomSpawnPoint();
        if (spawnPoint == null) return;

        MonsterGrade grade = ResolveGrade(session);
        WaveMultipliers multipliers = new WaveMultipliers { hp = session.enemyHpMul, speed = session.enemySpeedMul, damage = session.enemyDamageMul };

        string enemyIdSource = string.IsNullOrWhiteSpace(session.lastConfiguredEnemyId) ? "fallback(defaultMonsterId)" : "waveRow(enemyId/monsterId)";
        Debug.Log($"[EnemySpawner] 이번 스폰 enemyId='{session.currentEnemyId}' (source={enemyIdSource}, waveValue='{session.lastConfiguredEnemyId}', fallback='{defaultMonsterId}')");

        Enemy enemy = enemyPool.Get(spawnPoint.position, Quaternion.identity, arkTarget, monsterTable, session.currentEnemyId, grade, multipliers);
        if (enemy == null)
        {
            Debug.LogError("[EnemySpawner] EnemyPool.Get 실패로 적 스폰에 실패했습니다.");
            return;
        }

        session.spawnedCount++;
    }

    private void SpawnEnemyFallback()
    {
        Transform spawnPoint = GetRandomSpawnPoint();
        if (spawnPoint == null) return;

        MonsterGrade grade = MonsterGrade.Normal;
        WaveMultipliers multipliers = new WaveMultipliers { hp = 1f, speed = 1f, damage = 1f };

        string currentEnemyId = ResolveMonsterIdOrFallback(null, defaultMonsterId);

        Enemy enemy = enemyPool.Get(spawnPoint.position, Quaternion.identity, arkTarget, monsterTable, currentEnemyId, grade, multipliers);
        if (enemy == null)
        {
            Debug.LogError("[EnemySpawner] EnemyPool.Get 실패로 적 스폰에 실패했습니다.");
            return;
        }
    }

    private Transform GetRandomSpawnPoint()
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
            return null;
        }

        int idx = Random.Range(0, validSpawnPoints.Count);
        return validSpawnPoints[idx];
    }


    private string ResolveMonsterIdOrFallback(string waveMonsterId, string fallbackMonsterId)
    {
        string fallback = string.IsNullOrWhiteSpace(fallbackMonsterId) ? DefaultMonsterId : fallbackMonsterId.Trim();

        if (string.IsNullOrWhiteSpace(waveMonsterId))
        {
            return fallback;
        }

        // 숫자/문자 혼합 ID (Monster1, slime_01 등) 모두 허용
        // 공백만 제거하여 그대로 사용
        return waveMonsterId.Trim();
    }

    private MonsterGrade ResolveGrade(WaveSession session)
    {
        if (session.bossWave)
        {
            return MonsterGrade.Boss;
        }

        if (session.eliteEvery > 0 && (session.spawnedCount + 1) % session.eliteEvery == 0)
        {
            return MonsterGrade.Elite;
        }

        return MonsterGrade.Normal;
    }
}
