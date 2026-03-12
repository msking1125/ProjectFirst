using System.Collections.Generic;
using UnityEngine;
using ProjectFirst.Data;

namespace Project
{

/// <summary>
/// EnemyPool
/// Documentation cleaned.
/// Documentation cleaned.
/// Documentation cleaned.
/// </summary>
public class EnemyPool : MonoBehaviour
{
    public static EnemyPool Instance { get; private set; }

    [Header("Enemy Pool Settings")]
    [Tooltip("Configured in inspector.")]
    [SerializeField] private Transform poolRoot;

    // Note: cleaned comment.
    private readonly Dictionary<Enemy, Queue<Enemy>> prefabPoolMap = new();
    // Note: cleaned comment.
    private readonly Dictionary<Enemy, Enemy> enemyToPrefab = new();

    private void Awake()
    {
        // Note: cleaned comment.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Note: cleaned comment.
        if (poolRoot == null)
            poolRoot = transform;
    }

    /// <summary>
    /// Documentation cleaned.
    /// </summary>
    private Enemy CreateEnemyInstance(Enemy prefab)
    {
        if (prefab == null) return null;

        Enemy newEnemy = Instantiate(prefab, poolRoot);
        newEnemy.SetPool(this);
        newEnemy.gameObject.SetActive(false);

        // Note: cleaned comment.
        enemyToPrefab[newEnemy] = prefab;
        return newEnemy;
    }

    /// <summary>
    /// Documentation cleaned.
    /// </summary>
    private Queue<Enemy> GetOrCreatePool(Enemy prefab)
    {
        if (!prefabPoolMap.TryGetValue(prefab, out var pool))
        {
            pool = new Queue<Enemy>();
            prefabPoolMap[prefab] = pool;
        }
        return pool;
    }

    /// <summary>
    /// Documentation cleaned.
    /// </summary>
    /// Documentation cleaned.
    /// Documentation cleaned.
    /// Documentation cleaned.
    /// Documentation cleaned.
    /// Documentation cleaned.
    /// Documentation cleaned.
    /// Documentation cleaned.
    /// Documentation cleaned.
    public Enemy Get(Vector3 position, Quaternion rotation, Transform arkTarget, MonsterTable monsterTable, int enemyId, MonsterGrade grade, WaveMultipliers multipliers)
    {
        // Note: cleaned comment.
        MonsterRow row = monsterTable != null ? monsterTable.GetByIdAndGrade(enemyId, grade) : null;

        if (row == null && monsterTable != null)
        {
            row = monsterTable.GetById(enemyId); // ?깃툒 臾댁떆 ?⑥씪 留ㅼ묶 ?쒕룄
            if (row != null)
                Debug.Log("[Log] Message cleaned.");
        }

        Enemy prefab = row != null ? row.prefab?.GetComponent<Enemy>() : null;

        if (prefab == null)
        {
            Debug.LogError("[Log] Error message cleaned.");
            return null;
        }

        // Note: cleaned comment.
        var pool = GetOrCreatePool(prefab);
        Enemy enemy = null;

#if UNITY_2021_2_OR_NEWER
        if (!pool.TryDequeue(out enemy))
            enemy = CreateEnemyInstance(prefab);
#else
        enemy = pool.Count > 0 ? pool.Dequeue() : CreateEnemyInstance(prefab);
#endif

        if (enemy == null)
        {
            Debug.LogError("[Log] Error message cleaned.");
            return null;
        }

        // Note: cleaned comment.
        var t = enemy.transform;
        t.SetParent(null, false); // ? 猷⑦듃?먯꽌 遺꾨━
        t.SetPositionAndRotation(position, rotation);

        enemy.gameObject.SetActive(true);
        enemy.OnSpawnedFromPool(arkTarget, monsterTable, enemyId, grade, multipliers);

        return enemy;
    }

    /// <summary>
    /// Documentation cleaned.
    /// </summary>
    /// Documentation cleaned.
    public void Return(Enemy enemy)
    {
        if (enemy == null) return;

        enemy.OnReturnedToPool();
        enemy.gameObject.SetActive(false);
        enemy.transform.SetParent(poolRoot, false);

        // Note: cleaned comment.
        if (!enemyToPrefab.TryGetValue(enemy, out var prefab) || prefab == null)
        {
            Debug.LogError("[Log] Error message cleaned.");
            Destroy(enemy.gameObject);
            return;
        }

        GetOrCreatePool(prefab).Enqueue(enemy);
    }
}
} // namespace Project



