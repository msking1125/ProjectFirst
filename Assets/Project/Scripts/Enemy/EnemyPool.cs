using System.Collections.Generic;
using UnityEngine;
using ProjectFirst.Data;

namespace Project
{

/// <summary>
/// EnemyPool
/// - ???꾨━??Enemy)??誘몃━ ?щ윭 媛??앹꽦???볤퀬 ?꾩슂????爰쇰궡 ?곌퀬, ?ㅼ떆 諛섑솚?섏뿬 ?ъ궗?⑺븯???ㅻ툕?앺듃 ?
/// - ?꾩슂 媛쒖닔 遺議???利됱떆 異붽? ?앹꽦
/// - ?ㅼ젙?대굹 ?뚯씠釉??ㅻ쪟 ?곹솴?먯꽌 ?붾쾭洹?硫붿떆吏 異쒕젰
/// </summary>
public class EnemyPool : MonoBehaviour
{
    public static EnemyPool Instance { get; private set; }

    [Header("Enemy Pool Settings")]
    [Tooltip("?留?Enemy ?ㅻ툕?앺듃?ㅼ쓽 遺紐④? ???몃옖?ㅽ뤌")]
    [SerializeField] private Transform poolRoot;

    // ?ㅻ툕?앺듃 ?: <???먮낯 ?꾨━?? ?대떦 ?꾨━?뱀쓣 怨듭쑀?섎뒗 Enemy ?몄뒪?댁뒪?ㅼ쓽 ??
    private readonly Dictionary<Enemy, Queue<Enemy>> prefabPoolMap = new();
    // 媛?Enemy ?몄뒪?댁뒪???먮낯 ?꾨━??湲곕줉
    private readonly Dictionary<Enemy, Enemy> enemyToPrefab = new();

    private void Awake()
    {
        // ?깃???以묐났 諛⑹?
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // poolRoot媛 ?ㅼ젙?섏뼱 ?덉? ?딆쑝硫?Pool ?ㅻ툕?앺듃 ?먭린 ?먯떊 ?ъ슜
        if (poolRoot == null)
            poolRoot = transform;
    }

    /// <summary>
    /// Enemy ?꾨━???섎굹瑜??ㅼ젣 ?몄뒪?댁뒪濡??앹꽦?섏뿬 Pool???ｌ쓣 以鍮?
    /// </summary>
    private Enemy CreateEnemyInstance(Enemy prefab)
    {
        if (prefab == null) return null;

        Enemy newEnemy = Instantiate(prefab, poolRoot);
        newEnemy.SetPool(this);
        newEnemy.gameObject.SetActive(false);

        // ?몄뒪?댁뒪 ?먮낯 ?뺣낫 湲곕줉 (諛섑솚 ???꾨━???앸퀎???꾩슂)
        enemyToPrefab[newEnemy] = prefab;
        return newEnemy;
    }

    /// <summary>
    /// ?뱀젙 ?꾨━?뱀뿉 ?대떦?섎뒗 Enemy ???)瑜?媛?몄삤怨? ?놁쑝硫??덈줈 ?앹꽦?댁꽌 ?깅줉
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
    /// Enemy ?ㅻ툕?앺듃瑜???먯꽌 媛?몄삩?? ?먮룞?쇰줈 ?앹꽦&珥덇린?붽퉴吏 泥섎━.
    /// </summary>
    /// <param name="position">???꾩튂</param>
    /// <param name="rotation">???뚯쟾</param>
    /// <param name="arkTarget">怨듦꺽 ????꾪겕) ?몃옖?ㅽ뤌</param>
    /// <param name="monsterTable">紐ъ뒪???곗씠???뚯씠釉?/param>
    /// <param name="enemyId">??ID</param>
    /// <param name="grade">???깃툒</param>
    /// <param name="multipliers">?⑥씠釉?諛곗닔(?λ젰移?</param>
    /// <returns>?ъ슜??Enemy ?몄뒪?댁뒪, ?ㅽ뙣??null</returns>
    public Enemy Get(Vector3 position, Quaternion rotation, Transform arkTarget, MonsterTable monsterTable, int enemyId, MonsterGrade grade, WaveMultipliers multipliers)
    {
        // 紐ъ뒪???뚯씠釉붿뿉?????뺣낫/?꾨━??媛?몄삤湲?
        MonsterRow row = monsterTable != null ? monsterTable.GetByIdAndGrade(enemyId, grade) : null;

        if (row == null && monsterTable != null)
        {
            row = monsterTable.GetById(enemyId); // ?깃툒 臾댁떆 ?⑥씪 留ㅼ묶 ?쒕룄
            if (row != null)
                Debug.Log($"[EnemyPool] Enemy ID='{enemyId}' ?깃툒 臾댁떆 ?쇰컲 留ㅼ묶 (?뚯씠釉? {row.grade}, ?붿껌: {grade})");
        }

        Enemy prefab = row != null ? row.prefab?.GetComponent<Enemy>() : null;

        if (prefab == null)
        {
            Debug.LogError($"[EnemyPool] Enemy ?앹꽦 ?ㅽ뙣: id='{enemyId}', grade='{grade}'???대떦?섎뒗 ?꾨━?뱀씠 議댁옱?섏? ?딆뒿?덈떎.");
            return null;
        }

        // ??먯꽌 ?ъ궗??Enemy 李얘린
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
            Debug.LogError($"[EnemyPool] Enemy ?앹꽦/?留??ㅽ뙣 - ?꾨━?? '{prefab.name}'");
            return null;
        }

        // ?ㅻ툕?앺듃 ?쒖꽦??諛??꾩튂/?뚯쟾/?ㅽ룿 泥섎━
        var t = enemy.transform;
        t.SetParent(null, false); // ? 猷⑦듃?먯꽌 遺꾨━
        t.SetPositionAndRotation(position, rotation);

        enemy.gameObject.SetActive(true);
        enemy.OnSpawnedFromPool(arkTarget, monsterTable, enemyId, grade, multipliers);

        return enemy;
    }

    /// <summary>
    /// ?ъ슜??Enemy瑜?Pool濡?諛섑솚(鍮꾪솢?깊솕 & ?ы걧??
    /// </summary>
    /// <param name="enemy">諛섑솚??Enemy ?몄뒪?댁뒪</param>
    public void Return(Enemy enemy)
    {
        if (enemy == null) return;

        enemy.OnReturnedToPool();
        enemy.gameObject.SetActive(false);
        enemy.transform.SetParent(poolRoot, false);

        // ?먮낯 ?꾨━???뺣낫 ?놁쑝硫?鍮꾩젙???곹깭) 諛붾줈 ?뚭눼
        if (!enemyToPrefab.TryGetValue(enemy, out var prefab) || prefab == null)
        {
            Debug.LogError("[EnemyPool] Enemy 諛섑솚 ?ㅽ뙣: ?먮낯 ?꾨━???뺣낫 ?놁쓬. ?ㅻ툕?앺듃瑜?媛뺤젣 ?뚭눼?⑸땲??");
            Destroy(enemy.gameObject);
            return;
        }

        GetOrCreatePool(prefab).Enqueue(enemy);
    }
}
} // namespace Project



