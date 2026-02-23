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

    private static string BuildMonsterTableSummary(MonsterTable monsterTable)
    {
        if (monsterTable == null)
        {
            return "monsterTable=null";
        }

        if (monsterTable.rows == null)
        {
            return "rows=null";
        }

        int rowCount = monsterTable.rows.Count;
        if (rowCount == 0)
        {
            return "rows=0";
        }

        HashSet<string> ids = new();
        for (int i = 0; i < monsterTable.rows.Count; i++)
        {
            MonsterRow r = monsterTable.rows[i];
            if (r == null || string.IsNullOrWhiteSpace(r.id))
            {
                continue;
            }

            ids.Add(r.id);
        }

        return $"rows={rowCount}, ids=[{string.Join(", ", ids)}]";
    }

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
        Enemy defaultPrefab = enemyPrefab;
        Enemy sourcePrefab = defaultPrefab;
        MonsterRow row = monsterTable != null ? monsterTable.GetByIdAndGrade(enemyId, grade) : null;
        GameObject prefabOverride = row != null ? row.prefab : null;

        if (row == null)
        {
            Debug.LogWarning($"[EnemyPool] MonsterTable lookup miss. monsterId='{enemyId}', grade='{grade}'. table={BuildMonsterTableSummary(monsterTable)}. Fallback to defaultPrefab='{(defaultPrefab != null ? defaultPrefab.name : "null")}'.");
        }
        else if (prefabOverride == null)
        {
            Debug.LogWarning($"[EnemyPool] Monster row found but prefab is null. monsterId='{enemyId}', grade='{grade}', rowName='{row.name}'. table={BuildMonsterTableSummary(monsterTable)}. Fallback to defaultPrefab='{(defaultPrefab != null ? defaultPrefab.name : "null")}'.");
        }

        if (prefabOverride != null)
        {
            sourcePrefab = prefabOverride.GetComponent<Enemy>();
            if (sourcePrefab == null)
            {
                Debug.LogWarning($"[EnemyPool] prefabOverride has no Enemy component. monsterId='{enemyId}', grade='{grade}', prefab='{prefabOverride.name}'. Fallback to defaultPrefab='{(defaultPrefab != null ? defaultPrefab.name : "null")}'.");
                sourcePrefab = defaultPrefab;
            }
        }

        if (sourcePrefab == null)
        {
            return null;
        }

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
