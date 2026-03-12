using System.Collections.Generic;
using UnityEngine;

namespace Project
{
    /// <summary>
    /// Documentation cleaned.
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
                Debug.LogError("[Log] Error message cleaned.");
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

        /// Documentation cleaned.
        public void Register(Enemy enemy)
        {
            if (enemy == null)
            {
                Debug.LogError("[Log] Error message cleaned.");
                return;
            }

            if (!activeEnemies.Contains(enemy))
            {
                activeEnemies.Add(enemy);
            }
        }

        /// Documentation cleaned.
        public void Unregister(Enemy enemy)
        {
            if (enemy == null)
            {
                Debug.LogError("[Log] Error message cleaned.");
                return;
            }

            activeEnemies.Remove(enemy);
        }

        /// Documentation cleaned.
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

        /// Documentation cleaned.
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

        /// Documentation cleaned.
        public IReadOnlyList<Enemy> GetAliveEnemies()
        {
            RefreshAliveEnemies();
            return _aliveEnemies;
        }

        /// Documentation cleaned.
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

