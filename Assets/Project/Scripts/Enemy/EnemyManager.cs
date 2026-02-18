using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;

    public List<Enemy> activeEnemies = new List<Enemy>();

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

    public void Unregister(Enemy enemy)
    {
        if (enemy == null)
        {
            Debug.LogError("[EnemyManager] null Enemy를 Unregister하려고 했습니다.");
            return;
        }

        activeEnemies.Remove(enemy);
    }

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
}
