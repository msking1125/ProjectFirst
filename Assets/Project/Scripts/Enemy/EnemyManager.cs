using System.Collections.Generic;
using UnityEngine;

namespace Project
{
    /// <summary>
    /// ??Enemy) 媛앹껜?ㅼ쓣 愿由ы븯???깃???留ㅻ땲?.
    /// </summary>
    public class EnemyManager : MonoBehaviour
    {
        public static EnemyManager Instance;
        public int ActiveEnemyCount => activeEnemies?.Count ?? 0;
        public int AliveEnemyCount => GetAliveCount();

        [SerializeField]
        private List<Enemy> activeEnemies = new List<Enemy>();

        private readonly List<Enemy> _aliveEnemies = new List<Enemy>();

        public IReadOnlyList<Enemy> AliveEnemies => _aliveEnemies;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogError("[EnemyManager] 以묐났 Instance媛 媛먯??섏뿀?듬땲??");
                return;
            }

            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>?곸쓣 ?깅줉?⑸땲??</summary>
        public void Register(Enemy enemy)
        {
            if (enemy == null)
            {
                Debug.LogError("[EnemyManager] null Enemy瑜?Register?섎젮怨??덉뒿?덈떎.");
                return;
            }

            if (!activeEnemies.Contains(enemy))
            {
                activeEnemies.Add(enemy);
            }
        }

        /// <summary>???깅줉???댁젣?⑸땲??</summary>
        public void Unregister(Enemy enemy)
        {
            if (enemy == null)
            {
                Debug.LogError("[EnemyManager] null Enemy瑜?Unregister?섎젮怨??덉뒿?덈떎.");
                return;
            }

            activeEnemies.Remove(enemy);
        }

        /// <summary>?앹〈???곸쓽 ?섎? 諛섑솚?⑸땲??</summary>
        public int GetAliveCount()
        {
            RefreshAliveEnemies();
            return _aliveEnemies.Count;
        }

        private void RefreshAliveEnemies()
        {
            _aliveEnemies.Clear();

            for (int i = activeEnemies.Count - 1; i >= 0; i--)
            {
                Enemy e = activeEnemies[i];
                if (e == null || !e.gameObject.activeInHierarchy)
                {
                    activeEnemies.RemoveAt(i);
                    continue;
                }

                if (!e.IsAlive)
                {
                    continue;
                }

                _aliveEnemies.Add(e);
            }
        }

        /// <summary>二쇱뼱吏??꾩튂?먯꽌 媛??媛源뚯슫 ?앹〈 ?곸쓣 諛섑솚?⑸땲??</summary>
        public Enemy GetClosest(Vector3 pos, float range)
        {
            float min = range;
            Enemy closest = null;

            for (int i = activeEnemies.Count - 1; i >= 0; i--)
            {
                Enemy e = activeEnemies[i];
                if (e == null || !e.gameObject.activeInHierarchy)
                {
                    activeEnemies.RemoveAt(i);
                    continue;
                }

                float d = Vector3.Distance(pos, e.transform.position);
                if (d < min)
                {
                    min = d;
                    closest = e;
                }
            }

            return closest;
        }

        /// <summary>紐⑤뱺 ?앹〈 ?곸쓣 諛섑솚?⑸땲??</summary>
        public IReadOnlyList<Enemy> GetAliveEnemies()
        {
            RefreshAliveEnemies();
            return _aliveEnemies;
        }

        /// <summary>?앹〈 ?곸쓣 鍮꾪븷??由ъ뒪?몄뿉 梨꾩썎?덈떎.</summary>
        public int FillAliveEnemiesNonAlloc(List<Enemy> buffer)
        {
            if (buffer == null)
            {
                return 0;
            }

            RefreshAliveEnemies();
            buffer.Clear();
            buffer.AddRange(_aliveEnemies);
            return buffer.Count;
        }
    }
}

