using System.Collections.Generic;
using UnityEngine;
using ProjectFirst.Data;

namespace Project
{
    /// <summary>
    /// 적 프리팹을 재사용하기 위한 오브젝트 풀입니다.
    /// 몬스터 테이블에서 프리팹을 찾고, 생성된 Enemy 인스턴스를 재활용합니다.
    /// </summary>
    public class EnemyPool : MonoBehaviour
    {
        public static EnemyPool Instance { get; private set; }

        [Header("적 풀 설정")]
        [Tooltip("비활성 적을 보관할 부모 Transform입니다.")]
        [SerializeField] private Transform poolRoot;

        private readonly Dictionary<Enemy, Queue<Enemy>> prefabPoolMap = new();
        private readonly Dictionary<Enemy, Enemy> enemyToPrefab = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (poolRoot == null)
                poolRoot = transform;
        }

        private Enemy CreateEnemyInstance(Enemy prefab)
        {
            if (prefab == null)
                return null;

            Enemy instance = Instantiate(prefab, poolRoot);
            instance.SetPool(this);
            instance.gameObject.SetActive(false);
            enemyToPrefab[instance] = prefab;
            return instance;
        }

        private Queue<Enemy> GetOrCreatePool(Enemy prefab)
        {
            if (!prefabPoolMap.TryGetValue(prefab, out Queue<Enemy> pool))
            {
                pool = new Queue<Enemy>();
                prefabPoolMap[prefab] = pool;
            }

            return pool;
        }

        /// <summary>
        /// 몬스터 ID와 등급에 맞는 적을 가져와 배치합니다.
        /// 등급 행이 없으면 같은 ID의 기본 행을 한 번 더 시도합니다.
        /// </summary>
        public Enemy Get(Vector3 position, Quaternion rotation, Transform arkTarget, MonsterTable monsterTable, int enemyId, MonsterGrade grade, WaveMultipliers multipliers)
        {
            MonsterRow row = monsterTable != null ? monsterTable.GetByIdAndGrade(enemyId, grade) : null;

            if (row == null && monsterTable != null)
            {
                row = monsterTable.GetById(enemyId);
                if (row != null)
                    Debug.Log("[EnemyPool] 등급에 맞는 데이터가 없어 기본 몬스터 데이터를 사용합니다.");
            }

            Enemy prefab = row != null ? row.prefab?.GetComponent<Enemy>() : null;
            if (prefab == null)
            {
                Debug.LogError($"[EnemyPool] enemyId={enemyId}, grade={grade}에 해당하는 Enemy 프리팹을 찾지 못했습니다.");
                return null;
            }

            Queue<Enemy> pool = GetOrCreatePool(prefab);
            Enemy enemy = null;

#if UNITY_2021_2_OR_NEWER
            if (!pool.TryDequeue(out enemy))
                enemy = CreateEnemyInstance(prefab);
#else
            enemy = pool.Count > 0 ? pool.Dequeue() : CreateEnemyInstance(prefab);
#endif

            if (enemy == null)
            {
                Debug.LogError($"[EnemyPool] enemyId={enemyId}, grade={grade}에 해당하는 Enemy 인스턴스 생성에 실패했습니다.");
                return null;
            }

            Transform enemyTransform = enemy.transform;
            enemyTransform.SetParent(null, false);
            enemyTransform.SetPositionAndRotation(position, rotation);

            enemy.gameObject.SetActive(true);
            enemy.OnSpawnedFromPool(arkTarget, monsterTable, enemyId, grade, multipliers);
            return enemy;
        }

        /// <summary>
        /// 사용이 끝난 적을 풀로 반환합니다.
        /// </summary>
        public void Return(Enemy enemy)
        {
            if (enemy == null)
                return;

            enemy.OnReturnedToPool();
            enemy.gameObject.SetActive(false);
            enemy.transform.SetParent(poolRoot, false);

            if (!enemyToPrefab.TryGetValue(enemy, out Enemy prefab) || prefab == null)
            {
                Debug.LogError("[EnemyPool] 반환 대상 Enemy의 원본 프리팹을 찾지 못했습니다.");
                Destroy(enemy.gameObject);
                return;
            }

            GetOrCreatePool(prefab).Enqueue(enemy);
        }
    }
}
