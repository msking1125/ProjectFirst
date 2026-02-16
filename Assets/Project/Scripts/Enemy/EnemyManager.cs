using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;

    public List<Enemy> activeEnemies = new List<Enemy>();

    void Awake()
    {
        Instance = this;
    }

    public void Register(Enemy enemy)
    {
        activeEnemies.Add(enemy);
    }

    public void Unregister(Enemy enemy)
    {
        activeEnemies.Remove(enemy);
    }

    public Enemy GetClosest(Vector3 pos, float range)
    {
        float min = range;
        Enemy closest = null;

        foreach (var e in activeEnemies)
        {
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
