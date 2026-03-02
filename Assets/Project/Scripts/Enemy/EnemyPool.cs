using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EnemyPool
/// - 적 프리팹을 미리 생성하여 재사용, 성능 최적화
/// - 부족 시 즉시 생성
/// - 데이터 테이블에 프리팹 없으면 오류 로그
/// </summary>
public class EnemyPool : MonoBehaviour
{
    public static EnemyPool Instance { get; private set; }

    [Header("Enemy Pool Settings")]
    [Tooltip("풀링된 오브젝트의 부모 트랜스폼")]
    [SerializeField] private Transform poolRoot;

    // Enemy 인스턴스별 원본 프리팹
    private readonly Dictionary<Enemy, Enemy> instanceToPrefab = new();
    // 프리팹별 Enemy 풀
    private readonly Dictionary<Enemy, Queue<Enemy>> prefabPools = new();

    private void Awake()
    {
        // 중복 싱글턴 제거
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        if (poolRoot == null)
            poolRoot = transform;
    }

    // Enemy 생성 및 대기 상태로 풀에 준비
    private Enemy CreateEnemy(Enemy prefab)
    {
        if (prefab == null) return null;

        Enemy enemy = Instantiate(prefab, poolRoot);
        enemy.SetPool(this);
        enemy.gameObject.SetActive(false);
        instanceToPrefab[enemy] = prefab;
        return enemy;
    }

    // 프리팹별 풀 반환 (없으면 생성)
    private Queue<Enemy> GetPool(Enemy prefab)
    {
        if (!prefabPools.TryGetValue(prefab, out var pool))
        {
            pool = new Queue<Enemy>();
            prefabPools[prefab] = pool;
        }
        return pool;
    }

    /// <summary>
    /// Enemy 반환
    /// </summary>
    public Enemy Get(Vector3 pos, Quaternion rot, Transform arkTarget, MonsterTable monsterTable, string enemyId, MonsterGrade grade, WaveMultipliers multipliers)
    {
        // 테이블에서 매칭되는 몬스터 정보 가져오기
        MonsterRow row = monsterTable != null ? monsterTable.GetByIdAndGrade(enemyId, grade) : null;
        if (row == null && monsterTable != null)
        {
            row = monsterTable.GetById(enemyId);
            if (row != null)
                Debug.Log($"[EnemyPool] ID='{enemyId}'로 Grade 무관 매칭 (테이블: {row.grade}, 요청: {grade})");
        }

        Enemy prefab = row != null ? row.prefab?.GetComponent<Enemy>() : null;

        if (prefab == null)
        {
            Debug.LogError($"[EnemyPool] 적 생성 실패: id='{enemyId}', grade='{grade}' 프리팹을 찾지 못함.");
            return null;
        }

        var pool = GetPool(prefab);
        Enemy enemy = null;

#if UNITY_2021_2_OR_NEWER
        if (!pool.TryDequeue(out enemy))
            enemy = CreateEnemy(prefab);
#else
        enemy = pool.Count > 0 ? pool.Dequeue() : CreateEnemy(prefab);
#endif

        if (enemy == null)
        {
            Debug.LogError($"[EnemyPool] Enemy 생성 또는 풀링 실패. 프리팹: '{prefab.name}'");
            return null;
        }

        // 부모 해제 후 위치, 회전 적용
        var t = enemy.transform;
        t.SetParent(null, false);
        t.SetPositionAndRotation(pos, rot);

        enemy.gameObject.SetActive(true);
        enemy.OnSpawnedFromPool(arkTarget, monsterTable, enemyId, grade, multipliers);

        return enemy;
    }

    /// <summary>
    /// Enemy를 풀로 반환
    /// </summary>
    public void Return(Enemy enemy)
    {
        if (enemy == null) return;

        enemy.OnReturnedToPool();
        enemy.gameObject.SetActive(false);
        enemy.transform.SetParent(poolRoot, false);

        if (!instanceToPrefab.TryGetValue(enemy, out var prefab) || prefab == null)
        {
            Debug.LogError("[EnemyPool] 반환 Enemy의 원본 프리팹 정보 없음. 오브젝트 파괴.");
            Destroy(enemy.gameObject);
            return;
        }

        GetPool(prefab).Enqueue(enemy);
    }
}
