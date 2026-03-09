using System.Collections.Generic;
using UnityEngine;

namespace Project
{

/// <summary>
/// EnemyPool
/// - 적 프리팹(Enemy)을 미리 여러 개 생성해 놓고 필요할 때 꺼내 쓰고, 다시 반환하여 재사용하는 오브젝트 풀
/// - 필요 개수 부족 시 즉시 추가 생성
/// - 설정이나 테이블 오류 상황에서 디버그 메시지 출력
/// </summary>
public class EnemyPool : MonoBehaviour
{
    public static EnemyPool Instance { get; private set; }

    [Header("Enemy Pool Settings")]
    [Tooltip("풀링 Enemy 오브젝트들의 부모가 될 트랜스폼")]
    [SerializeField] private Transform poolRoot;

    // 오브젝트 풀: <적 원본 프리팹, 해당 프리팹을 공유하는 Enemy 인스턴스들의 큐>
    private readonly Dictionary<Enemy, Queue<Enemy>> prefabPoolMap = new();
    // 각 Enemy 인스턴스의 원본 프리팹 기록
    private readonly Dictionary<Enemy, Enemy> enemyToPrefab = new();

    private void Awake()
    {
        // 싱글턴 중복 방지
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // poolRoot가 설정되어 있지 않으면 Pool 오브젝트 자기 자신 사용
        if (poolRoot == null)
            poolRoot = transform;
    }

    /// <summary>
    /// Enemy 프리팹 하나를 실제 인스턴스로 생성하여 Pool에 넣을 준비
    /// </summary>
    private Enemy CreateEnemyInstance(Enemy prefab)
    {
        if (prefab == null) return null;

        Enemy newEnemy = Instantiate(prefab, poolRoot);
        newEnemy.SetPool(this);
        newEnemy.gameObject.SetActive(false);

        // 인스턴스 원본 정보 기록 (반환 시 프리팹 식별에 필요)
        enemyToPrefab[newEnemy] = prefab;
        return newEnemy;
    }

    /// <summary>
    /// 특정 프리팹에 해당하는 Enemy 큐(풀)를 가져오고, 없으면 새로 생성해서 등록
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
    /// Enemy 오브젝트를 풀에서 가져온다. 자동으로 생성&초기화까지 처리.
    /// </summary>
    /// <param name="position">적 위치</param>
    /// <param name="rotation">적 회전</param>
    /// <param name="arkTarget">공격 대상(아크) 트랜스폼</param>
    /// <param name="monsterTable">몬스터 데이터 테이블</param>
    /// <param name="enemyId">적 ID</param>
    /// <param name="grade">적 등급</param>
    /// <param name="multipliers">웨이브 배수(능력치)</param>
    /// <returns>사용할 Enemy 인스턴스, 실패시 null</returns>
    public Enemy Get(Vector3 position, Quaternion rotation, Transform arkTarget, MonsterTable monsterTable, int enemyId, MonsterGrade grade, WaveMultipliers multipliers)
    {
        // 몬스터 테이블에서 적 정보/프리팹 가져오기
        MonsterRow row = monsterTable != null ? monsterTable.GetByIdAndGrade(enemyId, grade) : null;

        if (row == null && monsterTable != null)
        {
            row = monsterTable.GetById(enemyId); // 등급 무시 단일 매칭 시도
            if (row != null)
                Debug.Log($"[EnemyPool] Enemy ID='{enemyId}' 등급 무시 일반 매칭 (테이블: {row.grade}, 요청: {grade})");
        }

        Enemy prefab = row != null ? row.prefab?.GetComponent<Enemy>() : null;

        if (prefab == null)
        {
            Debug.LogError($"[EnemyPool] Enemy 생성 실패: id='{enemyId}', grade='{grade}'에 해당하는 프리팹이 존재하지 않습니다.");
            return null;
        }

        // 풀에서 재사용 Enemy 찾기
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
            Debug.LogError($"[EnemyPool] Enemy 생성/풀링 실패 - 프리팹: '{prefab.name}'");
            return null;
        }

        // 오브젝트 활성화 및 위치/회전/스폰 처리
        var t = enemy.transform;
        t.SetParent(null, false); // 풀 루트에서 분리
        t.SetPositionAndRotation(position, rotation);

        enemy.gameObject.SetActive(true);
        enemy.OnSpawnedFromPool(arkTarget, monsterTable, enemyId, grade, multipliers);

        return enemy;
    }

    /// <summary>
    /// 사용한 Enemy를 Pool로 반환(비활성화 & 재큐잉)
    /// </summary>
    /// <param name="enemy">반환할 Enemy 인스턴스</param>
    public void Return(Enemy enemy)
    {
        if (enemy == null) return;

        enemy.OnReturnedToPool();
        enemy.gameObject.SetActive(false);
        enemy.transform.SetParent(poolRoot, false);

        // 원본 프리팹 정보 없으면(비정상 상태) 바로 파괴
        if (!enemyToPrefab.TryGetValue(enemy, out var prefab) || prefab == null)
        {
            Debug.LogError("[EnemyPool] Enemy 반환 실패: 원본 프리팹 정보 없음. 오브젝트를 강제 파괴합니다.");
            Destroy(enemy.gameObject);
            return;
        }

        GetOrCreatePool(prefab).Enqueue(enemy);
    }
}
} // namespace Project
