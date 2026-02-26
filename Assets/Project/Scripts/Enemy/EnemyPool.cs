using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EnemyPool
/// - 적 프리팹을 미리 생성(pooling)하여 재사용함으로써 성능 최적화
/// - MonsterTable의 id/grade 기반으로 프리팹을 가져옴(없을 때 fallback 지원)
/// - EnemySpawner/Return에서 가져가기 위한 싱글턴 구조
/// </summary>
public class EnemyPool : MonoBehaviour
{
    public static EnemyPool Instance { get; private set; }

    [Header("Enemy Pool Settings")]
    [Tooltip("Enemy 기본 프리팹. MonsterTable에서 프리팹 없을 때 사용됨.")]
    [SerializeField] private Enemy enemyPrefab;
    [Tooltip("풀에 미리 생성할 적 개수")]
    [SerializeField] private int initialSize = 30;
    [Tooltip("풀링된 오브젝트의 부모 트랜스폼")]
    [SerializeField] private Transform poolRoot;

    // instanceToSourcePrefab: 각 Enemy 인스턴스가 어떤 프리팹(원본)에서 생성되었는지 연결
    private readonly Dictionary<Enemy, Enemy> instanceToSourcePrefab = new();
    // poolsByPrefab: 각 프리팹별 풀 보관 (동일 프리팹 여러 MonsterRow간 공유용)
    private readonly Dictionary<Enemy, Queue<Enemy>> poolsByPrefab = new();

    /// <summary>
    /// MonsterTable의 요약을 문자열로 반환. 디버그 로그용.
    /// </summary>
    private static string BuildMonsterTableSummary(MonsterTable monsterTable)
    {
        if (monsterTable == null) return "monsterTable=null";
        if (monsterTable.rows == null) return "rows=null";
        int rowCount = monsterTable.rows.Count;
        if (rowCount == 0) return "rows=0";

        HashSet<string> ids = new();
        for (int i = 0; i < rowCount; i++)
        {
            MonsterRow r = monsterTable.rows[i];
            if (r == null || string.IsNullOrWhiteSpace(r.id)) continue;
            ids.Add(r.id);
        }
        return $"rows={rowCount}, ids=[{string.Join(", ", ids)}]";
    }

    // Singleton 초기화 및 풀 미리 생성
    private void Awake()
    {
        // [오류 예방] Duplicate 싱글턴 인스턴스 제거
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // 제대로 오브젝트 제거 (Destroy(this)는 컴포넌트만 제거됨, gameObject로!)
            return;
        }
        Instance = this;

        if (poolRoot == null)
            poolRoot = transform;

        WarmUp(enemyPrefab, initialSize);
    }

    /// <summary>
    /// 지정된 프리팹으로 count만큼 미리 생성하여 풀에 넣는다.
    /// </summary>
    private void WarmUp(Enemy prefab, int count)
    {
        if (prefab == null || count <= 0)
            return;

        for (int i = 0; i < count; i++)
        {
            CreateOne(prefab);
        }
    }

    /// <summary>
    /// 해당 프리팹 기준으로 새 Enemy 인스턴스를 생성하여 풀에 등록
    /// </summary>
    private Enemy CreateOne(Enemy sourcePrefab)
    {
        if (sourcePrefab == null) return null;

        // 오브젝트 생성, poolRoot에 자식으로 둠
        Enemy e = Instantiate(sourcePrefab, poolRoot);
        e.SetPool(this);
        e.gameObject.SetActive(false); // 풀에서 대기

        // 어떤 프리팹에서 왔는지 기록
        instanceToSourcePrefab[e] = sourcePrefab;

        // 풀에 등록
        Queue<Enemy> queue = GetQueue(sourcePrefab);
        queue.Enqueue(e);

        return e;
    }

    /// <summary>
    /// 해당 프리팹의 풀 가져오기(없으면 생성)
    /// </summary>
    private Queue<Enemy> GetQueue(Enemy prefab)
    {
        // [최적화] out variable로 선언할 필요 없이 간결하게 작성
        if (!poolsByPrefab.TryGetValue(prefab, out var queue))
        {
            queue = new Queue<Enemy>();
            poolsByPrefab[prefab] = queue;
        }
        return queue;
    }

    /// <summary>
    /// Enemy 반환
    /// - MonsterTable의 id/grade로 프리팹을 결정, 없으면 defaultPrefab 사용
    /// - 풀에 남은 오브젝트가 없으면 새로 생성
    /// </summary>
    public Enemy Get(Vector3 pos, Quaternion rot, Transform arkTarget, MonsterTable monsterTable, string enemyId, MonsterGrade grade, WaveMultipliers multipliers)
    {
        // 반드시 null체크 처리
        if (enemyPrefab == null)
        {
            Debug.LogError("[EnemyPool] enemyPrefab 미지정! EnemyPool에 Enemy 프리팹 참조 연결 필요.");
            return null;
        }

        Enemy defaultPrefab = enemyPrefab;
        Enemy sourcePrefab = defaultPrefab;

        MonsterRow row = null;
        // bool gradeFallbackUsed = false; // 변수 제거

        if (monsterTable != null)
        {
            // 1순위: ID + Grade 정확 매칭
            row = monsterTable.GetByIdAndGrade(enemyId, grade);

            // 2순위: ID만으로 매칭 (MonsterTable의 Grade와 WaveTable의 Grade가 다를 때 허용)
            if (row == null)
            {
                row = monsterTable.GetById(enemyId);
                if (row != null)
                {
                    // gradeFallbackUsed = true;
                    Debug.Log($"[EnemyPool] ID='{enemyId}'로 Grade 무관 매칭 성공 (MonsterTable Grade='{row.grade}', 요청 Grade='{grade}').");
                }
            }
        }

        GameObject prefabOverride = row != null ? row.prefab : null;

        if (row == null)
        {
            Debug.LogWarning($"[EnemyPool] MonsterTable lookup miss. monsterId='{enemyId}', grade='{grade}'. table={BuildMonsterTableSummary(monsterTable)}. Fallback to defaultPrefab='{(defaultPrefab != null ? defaultPrefab.name : "null")}'.");
        }
        else if (prefabOverride == null)
        {
            Debug.LogWarning($"[EnemyPool] Monster row found but prefab is null. monsterId='{enemyId}', grade='{grade}', rowName='{row.name}'. table={BuildMonsterTableSummary(monsterTable)}. Fallback to defaultPrefab='{(defaultPrefab != null ? defaultPrefab.name : "null")}'.");
        }

        // MonsterTable의 row에 프리팹이 있다면 덮어씀
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
            Debug.LogError("[EnemyPool] Enemy 프리팹이 모두 null입니다. enemyPrefab 연결 필요.");
            return null;
        }

        Queue<Enemy> queue = GetQueue(sourcePrefab);

        // [최적화/오류 예방] Queue에서 바로 TryDequeue 사용
        Enemy e = null;
#if UNITY_2021_2_OR_NEWER // Unity 2021.2부터 지원
        if (!queue.TryDequeue(out e))
            e = CreateOne(sourcePrefab);
#else
        e = queue.Count > 0 ? queue.Dequeue() : CreateOne(sourcePrefab);
#endif

        if (e == null)
        {
            Debug.LogError($"[EnemyPool] Enemy 오브젝트 생성/풀링 모두 실패. sourcePrefab='{sourcePrefab.name}'");
            return null;
        }

        // [오류 예방] 부모를 끊고 위치/회전 세팅
        Transform t = e.transform;
        t.SetParent(null, false); // 부모 미설정(null) + 트랜스폼 보존X
        t.SetPositionAndRotation(pos, rot);

        // 활성화 및 초기화
        e.gameObject.SetActive(true);
        e.OnSpawnedFromPool(arkTarget, monsterTable, enemyId, grade, multipliers);

        return e;
    }

    /// <summary>
    /// Enemy를 풀로 반환
    /// </summary>
    public void Return(Enemy e)
    {
        if (e == null)
            return;

        // 반환시 초기화
        e.OnReturnedToPool();
        e.gameObject.SetActive(false);
        e.transform.SetParent(poolRoot, false); // 부모 poolRoot로 재설정

        // [오류 예방] 원본 프리팹 기록 없으면 enemyPrefab 사용
        Enemy sourcePrefab = null;
        if (!instanceToSourcePrefab.TryGetValue(e, out sourcePrefab) || sourcePrefab == null)
        {
            sourcePrefab = enemyPrefab;
#if UNITY_EDITOR
            Debug.LogWarning("[EnemyPool] 반환된 Enemy의 원본 프리팹 정보를 알 수 없습니다. 기본 enemyPrefab으로 반환.");
#endif
        }

        GetQueue(sourcePrefab).Enqueue(e);
    }
}
