using System.Collections.Generic;
using UnityEngine;

public class SkillSystem
{
    private readonly RunSession runSession;
    private readonly SkillTable skillTable;
    private readonly Agent playerAgent;
    private readonly Dictionary<string, float> nextReadyTimeBySkillId = new Dictionary<string, float>();

    public SkillSystem(RunSession runSession, SkillTable skillTable, Agent playerAgent)
    {
        this.runSession = runSession;
        this.skillTable = skillTable;
        this.playerAgent = playerAgent;
    }

    public bool TryAcquireSkill(SkillRow skill)
    {
        if (skill == null)
        {
            return false;
        }

        return runSession.TryAddSkill(skill.id);
    }

    public bool IsSkillReady(SkillRow skill)
    {
        if (skill == null || !runSession.HasSkill(skill.id))
        {
            return false;
        }

        return !nextReadyTimeBySkillId.TryGetValue(skill.id, out float nextReadyTime) || Time.time >= nextReadyTime;
    }

    public float GetRemainingCooldown(SkillRow skill)
    {
        if (skill == null)
        {
            return 0f;
        }

        if (!nextReadyTimeBySkillId.TryGetValue(skill.id, out float nextReadyTime))
        {
            return 0f;
        }

        return Mathf.Max(0f, nextReadyTime - Time.time);
    }

    public bool Cast(SkillRow skill)
    {
        if (skill == null || !IsSkillReady(skill) || playerAgent == null)
        {
            return false;
        }

        int baseSkillDamage = Mathf.RoundToInt(playerAgent.AttackPower * skill.coefficient);
        EnemyManager enemyManager = EnemyManager.Instance;
        if (enemyManager == null)
        {
            return false;
        }

        IReadOnlyList<Enemy> aliveEnemies = enemyManager.GetAliveEnemies();
        for (int i = 0; i < aliveEnemies.Count; i++)
        {
            Enemy enemy = aliveEnemies[i];

            float elementMultiplier = ElementRules.GetMultiplier(skill.element, enemy.Element);
            int finalDamage = Mathf.RoundToInt(baseSkillDamage * elementMultiplier);
            enemy.TakeDamage(finalDamage, false);
        }

        nextReadyTimeBySkillId[skill.id] = Time.time + Mathf.Max(0f, skill.cooldown);
        return true;
    }

    public List<SkillRow> GetRandomCandidates(int count)
    {
        List<SkillRow> result = new List<SkillRow>();
        if (skillTable == null || skillTable.Rows == null)
        {
            return result;
        }

        List<SkillRow> pool = new List<SkillRow>();
        for (int i = 0; i < skillTable.Rows.Count; i++)
        {
            SkillRow row = skillTable.Rows[i];
            if (row == null || runSession.HasSkill(row.id))
            {
                continue;
            }

            pool.Add(row);
        }

        while (pool.Count > 0 && result.Count < count)
        {
            int pick = Random.Range(0, pool.Count);
            result.Add(pool[pick]);
            pool.RemoveAt(pick);
        }

        return result;
    }
}
