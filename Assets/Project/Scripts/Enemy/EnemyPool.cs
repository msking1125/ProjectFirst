using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Enemy 오브젝트 풀.
/// 초기 용량만큼 미리 생성해 비활성화 상태로 보관하고,
/// 필요 시 Get/Return으로 재사용한다.
/// </summary>
[AddComponentMenu("Enemy/Enemy Pool")]
public class EnemyPool : MonoBehaviour
{
    public static EnemyPool Instance;

    [Header("Enemy 프리팹 (필수)")]
    [Tooltip("풀에서 생성/재사용할 Enemy 프리팹")]
    public Enemy enemyPrefab;

    [Header("Pool Option")]
    [Tooltip("초기 미리 생성할 Enemy 수량")]
    [Min(1)]
    public int initialCapacity = 20;

    [Tooltip("풀에서 확장 생성이 허용되는지 여부")]
    public bool allowExpand = true;

    private readonly Queue<Enemy> pool = new Queue<Enemy>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("[EnemyPool] 중복 EnemyPool이 감지되었습니다. 기존 Instance를 사용합니다.");
            return;
        }

        Instance = this;

        if (enemyPrefab == null)
        {
            Debug.LogError("[EnemyPool] enemyPrefab이 할당되지 않았습니다.");
            return;
        }

        Prewarm();
    }

    private void Prewarm()
    {
        for (int i = 0; i < initialCapacity; i++)
        {
            Enemy enemy = CreateEnemy();
            if (enemy == null)
            {
                Debug.LogError("[EnemyPool] 초기화 중 Enemy 생성에 실패했습니다.");
                return;
            }

            Return(enemy);
        }
    }

    private Enemy CreateEnemy()
    {
        Enemy enemy = Instantiate(enemyPrefab, transform);
        if (enemy == null)
        {
            Debug.LogError("[EnemyPool] Enemy Instantiate에 실패했습니다.");
            return null;
        }

        enemy.SetPool(this);
        return enemy;
    }

    public Enemy Get(Vector3 position, Quaternion rotation, Transform target)
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("[EnemyPool] enemyPrefab이 없어 Get을 수행할 수 없습니다.");
            return null;
        }

        Enemy enemy = null;
        while (pool.Count > 0 && enemy == null)
        {
            enemy = pool.Dequeue();
        }

        if (enemy == null)
        {
            if (!allowExpand)
            {
                Debug.LogError("[EnemyPool] 풀이 비어 있고 allowExpand=false 입니다.");
                return null;
            }

            enemy = CreateEnemy();
            if (enemy == null)
            {
                Debug.LogError("[EnemyPool] 확장 생성에 실패해 Enemy를 가져올 수 없습니다.");
                return null;
            }
        }

        enemy.transform.SetPositionAndRotation(position, rotation);
        enemy.gameObject.SetActive(true);
        enemy.Init(target);
        return enemy;
    }

    public void Return(Enemy enemy)
    {
        if (enemy == null)
        {
            Debug.LogError("[EnemyPool] null Enemy를 Return하려고 했습니다.");
            return;
        }

        enemy.ResetForPool();
        enemy.transform.SetParent(transform);

        if (enemy.gameObject.activeSelf)
        {
            enemy.gameObject.SetActive(false);
        }

        pool.Enqueue(enemy);
    }
}
