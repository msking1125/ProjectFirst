using System.Collections.Generic;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Project
{
    /// <summary>
    /// 적(Enemy) 객체들을 관리하는 싱글턴 매니저.
    /// </summary>
    public class EnemyManager : MonoBehaviour
    {
        public static EnemyManager Instance;

#if ODIN_INSPECTOR
        [BoxGroup("상태")]
        [ShowInInspector, ReadOnly]
        [LabelText("활성 적 수")]
        [GUIColor(1f, 0.4f, 0.4f)]
#endif
        public int ActiveEnemyCount => activeEnemies?.Count ?? 0;

#if ODIN_INSPECTOR
        [BoxGroup("상태")]
        [ShowInInspector, ReadOnly]
        [LabelText("생존 적 수")]
        [ProgressBar(0, 50, ColorGetter = "GetAliveColor")]
#endif
        public int AliveEnemyCount => GetAliveCount();

#if ODIN_INSPECTOR
        private static Color GetAliveColor() => new Color(1f, 0.4f, 0.4f);
#endif

        [SerializeField]
#if ODIN_INSPECTOR
        [BoxGroup("내부")]
        [ListDrawerSettings(Expanded = false, ShowPaging = true)]
        [LabelText("활성 적 리스트")]
#endif
        private List<Enemy> activeEnemies = new List<Enemy>();

        private readonly List<Enemy> _aliveEnemies = new List<Enemy>();

        public IReadOnlyList<Enemy> AliveEnemies => _aliveEnemies;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogError("[EnemyManager] 중복 Instance가 감지되었습니다.");
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

        /// <summary>적을 등록합니다.</summary>
        public void Register(Enemy enemy)
        {
            if (enemy == null)
            {
                Debug.LogError("[EnemyManager] null Enemy를 Register하려고 했습니다.");
                return;
            }

            if (!activeEnemies.Contains(enemy))
            {
                activeEnemies.Add(enemy);
            }
        }

        /// <summary>적 등록을 해제합니다.</summary>
        public void Unregister(Enemy enemy)
        {
            if (enemy == null)
            {
                Debug.LogError("[EnemyManager] null Enemy를 Unregister하려고 했습니다.");
                return;
            }

            activeEnemies.Remove(enemy);
        }

        /// <summary>생존한 적의 수를 반환합니다.</summary>
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

        /// <summary>주어진 위치에서 가장 가까운 생존 적을 반환합니다.</summary>
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

        /// <summary>모든 생존 적을 반환합니다.</summary>
        public IReadOnlyList<Enemy> GetAliveEnemies()
        {
            RefreshAliveEnemies();
            return _aliveEnemies;
        }

        /// <summary>생존 적을 비할당 리스트에 채웁니다.</summary>
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
