using System.Collections.Generic;
using UnityEngine;

public class EnemyPool : MonoBehaviour
{
    public static EnemyPool Instance { get; private set; }

    [SerializeField] private Enemy enemyPrefab;
    [SerializeField] private int initialSize = 30;
    [SerializeField] private Transform poolRoot;

    private readonly Dictionary<Enemy, Enemy> instanceToSourcePrefab = new();
    private readonly Dictionary<Enemy, Queue<Enemy>> poolsByPrefab = new();

    public Enemy DefaultEnemyPrefab => enemyPrefab;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        if (poolRoot == null)
        {
            poolRoot = transform;
        }

        WarmUp(enemyPrefab, initialSize);
    }

    private void WarmUp(Enemy prefab, int count)
    {
        if (prefab == null)
        {
            return;
        }

        for (int i = 0; i < count; i++)
        {
            CreateOne(prefab);
        }
    }

    private Enemy CreateOne(Enemy sourcePrefab)
    {
        if (sourcePrefab == null)
        {
            return null;
        }

        Enemy e = Instantiate(sourcePrefab, poolRoot);
        e.SetPool(this);
        e.gameObject.SetActive(false);
        instanceToSourcePrefab[e] = sourcePrefab;

        Queue<Enemy> queue = GetQueue(sourcePrefab);
        queue.Enqueue(e);
        return e;
    }

    private Queue<Enemy> GetQueue(Enemy prefab)
    {
        if (!poolsByPrefab.TryGetValue(prefab, out Queue<Enemy> queue))
        {
            queue = new Queue<Enemy>();
            poolsByPrefab[prefab] = queue;
        }

        return queue;
    }

    public Enemy Get(Vector3 pos, Quaternion rot, Transform arkTarget, MonsterTable monsterTable, string enemyId, MonsterGrade grade, WaveMultipliers multipliers)
    {
        Enemy sourcePrefab = enemyPrefab;
        MonsterRow row = monsterTable != null ? monsterTable.GetByIdAndGrade(enemyId, grade) : null;
        if (row != null && row.prefab != null)
        {
            sourcePrefab = row.prefab.GetComponent<Enemy>();
        }

        if (sourcePrefab == null)
        {
            return null;
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        string prefabOverrideName = row != null && row.prefab != null ? row.prefab.name : "null";
        string defaultPrefabName = enemyPrefab != null ? enemyPrefab.name : "null";
        Debug.Log($"[EnemyPool] Spawning. monsterId='{enemyId}', grade='{grade}', prefabOverride='{prefabOverrideName}', defaultPrefab='{defaultPrefabName}', sourcePrefab='{sourcePrefab.name}'");
#endif

        Queue<Enemy> queue = GetQueue(sourcePrefab);
        Enemy e = queue.Count > 0 ? queue.Dequeue() : CreateOne(sourcePrefab);
        if (e == null)
        {
            return null;
        }

        Transform t = e.transform;
        t.SetParent(null);
        t.SetPositionAndRotation(pos, rot);
        e.gameObject.SetActive(true);
        e.OnSpawnedFromPool(arkTarget, monsterTable, enemyId, grade, multipliers);
        return e;
    }

    public void Return(Enemy e)
    {
        if (e == null)
        {
            return;
        }

        e.OnReturnedToPool();
        e.gameObject.SetActive(false);
        e.transform.SetParent(poolRoot);

        Enemy sourcePrefab = instanceToSourcePrefab.ContainsKey(e) ? instanceToSourcePrefab[e] : enemyPrefab;
        GetQueue(sourcePrefab).Enqueue(e);
    }
}
