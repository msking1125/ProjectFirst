using System.Collections.Generic;
using UnityEngine;

public class EnemyPool : MonoBehaviour
{
    public static EnemyPool Instance { get; private set; }

    [SerializeField] private Enemy enemyPrefab;
    [SerializeField] private int initialSize = 30;
    [SerializeField] private Transform poolRoot;

    private readonly Queue<Enemy> pool = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("EnemyPool: 중복 Instance가 감지되었습니다. 기존 Instance를 유지합니다.");
            Destroy(this);
            return;
        }
        Instance = this;

        if (poolRoot == null) poolRoot = transform;

        WarmUp();
    }

    private void WarmUp()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("EnemyPool: enemyPrefab is null.", this);
            return;
        }

        for (int i = 0; i < initialSize; i++)
        {
            CreateOne();
        }
    }

    private Enemy CreateOne()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("EnemyPool: enemyPrefab is null.", this);
            return null;
        }

        Enemy e = Instantiate(enemyPrefab, poolRoot);
        e.SetPool(this);
        e.gameObject.SetActive(false);
        pool.Enqueue(e);
        return e;
    }

    public Enemy Get(Vector3 pos, Quaternion rot, Transform arkTarget)
    {
        return Get(pos, rot, arkTarget, 1f, 1f, 1f);
    }

    public Enemy Get(Vector3 pos, Quaternion rot, Transform arkTarget, float hpMul, float speedMul, float damageMul)
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("EnemyPool: enemyPrefab is null.", this);
            return null;
        }

        Enemy e = pool.Count > 0 ? pool.Dequeue() : CreateOne();
        if (e == null)
        {
            Debug.LogError("EnemyPool: Enemy 생성 실패.", this);
            return null;
        }

        Transform t = e.transform;
        t.SetParent(null);
        t.SetPositionAndRotation(pos, rot);
        e.gameObject.SetActive(true);
        e.OnSpawnedFromPool(arkTarget, hpMul, speedMul, damageMul);
        return e;
    }

    public void Return(Enemy e)
    {
        if (e == null) return;
        e.OnReturnedToPool();
        e.gameObject.SetActive(false);
        e.transform.SetParent(poolRoot);
        pool.Enqueue(e);
    }
}
