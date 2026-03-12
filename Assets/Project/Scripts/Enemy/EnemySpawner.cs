using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using ProjectFirst.Data;
namespace Project
{

/// <summary>
/// EnemySpawner
/// - 湲곕낯?곸쑝濡?spawnInterval(珥?留덈떎 spawnPoints 以??꾩쓽 ?꾩튂?먯꽌 EnemyPool.Get?쇰줈 ???앹꽦
/// - WaveManager媛 ?ㅼ젙?섎㈃ ?대떦 ?⑥씠釉?spawnCount / spawnInterval / ?ㅽ꺈 諛곗닔瑜??ъ슜
/// </summary>
[AddComponentMenu("Enemy/Enemy Spawner")]
public class EnemySpawner : MonoBehaviour
{
    private const int DefaultMonsterId = 1;

    [Header("Enemy Pool (?꾩닔)")]
    [Tooltip("EnemyPool 而댄룷?뚰듃瑜??곌껐?섏꽭??")]
    public EnemyPool enemyPool;

    [Header("?寃?(Ark) (?꾩닔)")]
    [Tooltip("?곸씠 異붽꺽???寃?Ark)??Transform??吏?뺥븯?몄슂.")]
    public Transform arkTarget;

    [Header("Spawn Points (?꾩닔)")]
    [Tooltip("?곸씠 ?앹꽦???꾩튂?ㅼ쓽 Transform 諛곗뿴???깅줉?섏꽭??")]
    public Transform[] spawnPoints;

    [Header("Monster Data")]
    [SerializeField] private MonsterTable monsterTable;

    [Header("Spawn Option")]
    [Tooltip("?ㅽ룿 二쇨린(珥?瑜??ㅼ젙?섏꽭??")]
    [Min(0.01f)]
    public float spawnInterval = 2f;

    private float fallbackSpawnTimer;
    private bool useWaveConfig;
    [SerializeField] private int defaultMonsterId = DefaultMonsterId;

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
        public int currentEnemyId;
        public int lastConfiguredEnemyId;

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
        defaultMonsterId = defaultMonsterId <= 0 ? DefaultMonsterId : defaultMonsterId;

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
                Debug.LogError("[EnemySpawner] enemyPool???좊떦?섏? ?딆븯?듬땲??");
                loggedMissingPool = true;
            }

            return false;
        }

        loggedMissingPool = false;

        if (arkTarget == null)
        {
            if (!loggedMissingTarget)
            {
                Debug.LogError("[EnemySpawner] arkTarget???좊떦?섏? ?딆븯?듬땲??");
                loggedMissingTarget = true;
            }

            return false;
        }

        loggedMissingTarget = false;

        if (monsterTable == null)
        {
            if (!loggedMissingMonsterTable)
            {
                Debug.LogError("[EnemySpawner] monsterTable???좊떦?섏? ?딆븯?듬땲?? 'Monster Data' ?뱀뀡??MonsterTable ?먯뀑???곌껐?섏꽭??");
                loggedMissingMonsterTable = true;
            }

            return false;
        }

        loggedMissingMonsterTable = false;

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            if (!loggedMissingSpawnPoints)
            {
                Debug.LogError("[EnemySpawner] spawnPoints媛 鍮꾩뼱?덉뒿?덈떎. SpawnPoints瑜??곌껐?섏꽭??");
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
            Debug.LogError("[EnemySpawner] spawnPoints???좏슚??Transform???놁뒿?덈떎.");
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

        string enemyIdSource = session.lastConfiguredEnemyId <= 0 ? "fallback(defaultMonsterId)" : "waveRow(enemyId/monsterId)";
        Debug.Log($"[EnemySpawner] ?대쾲 ?ㅽ룿 enemyId='{session.currentEnemyId}' (source={enemyIdSource}, waveValue='{session.lastConfiguredEnemyId}', fallback='{defaultMonsterId}')");

        Enemy enemy = enemyPool.Get(spawnPoint.position, Quaternion.identity, arkTarget, monsterTable, session.currentEnemyId, grade, multipliers);
        if (enemy == null)
        {
            Debug.LogError("[EnemySpawner] EnemyPool.Get ?ㅽ뙣濡????ㅽ룿???ㅽ뙣?덉뒿?덈떎.");
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

        int currentEnemyId = ResolveMonsterIdOrFallback(0, defaultMonsterId);

        Enemy enemy = enemyPool.Get(spawnPoint.position, Quaternion.identity, arkTarget, monsterTable, currentEnemyId, grade, multipliers);
        if (enemy == null)
        {
            Debug.LogError("[EnemySpawner] EnemyPool.Get ?ㅽ뙣濡????ㅽ룿???ㅽ뙣?덉뒿?덈떎.");
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
            Debug.LogError("[EnemySpawner] spawnPoints???좏슚??Transform???놁뒿?덈떎.");
            return null;
        }

        int idx = UnityEngine.Random.Range(0, validSpawnPoints.Count);
        return validSpawnPoints[idx];
    }


    private int ResolveMonsterIdOrFallback(int waveMonsterId, int fallbackMonsterId)
    {
        int fallback = fallbackMonsterId <= 0 ? DefaultMonsterId : fallbackMonsterId;

        if (waveMonsterId <= 0)
        {
            return fallback;
        }

        return waveMonsterId;
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
} // namespace Project



