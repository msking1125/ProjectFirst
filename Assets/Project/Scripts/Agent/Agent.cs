using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour
{
    public float range = 5f;
    public float attackDamage = 3f;
    public float attackRate = 1f;

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= attackRate)
        {
            Attack();
            timer = 0f;
        }
    }

    void Attack()
    {
        Enemy target = FindClosestEnemy();
        if (target != null)
        {
            target.TakeDamage(attackDamage);
        }
    }

    Enemy FindClosestEnemy()
    {
        Enemy[] enemies = GameObject.FindObjectsOfType<Enemy>();

        float minDist = range;
        Enemy closest = null;

        foreach (var e in enemies)
        {
            float dist = Vector3.Distance(transform.position, e.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = e;
            }
        }

        return closest;
    }
}
