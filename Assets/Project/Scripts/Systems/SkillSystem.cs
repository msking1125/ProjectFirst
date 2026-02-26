using System.Collections.Generic;
using UnityEngine;

public class SkillSystem
{
    private readonly SkillTable skillTable;
    private readonly Agent playerAgent;
    private readonly SkillRow[] equippedSkills = new SkillRow[3];

    public IReadOnlyList<SkillRow> EquippedSkills => equippedSkills;

    public SkillSystem(SkillTable skillTable, Agent playerAgent)
    {
        this.skillTable = skillTable;
        this.playerAgent = playerAgent;
    }

    public bool Equip(SkillRow skill, int slotIndex)
    {
        if (skill == null || slotIndex < 0 || slotIndex >= equippedSkills.Length)
        {
            return false;
        }

        equippedSkills[slotIndex] = skill;
        return true;
    }

    public int EquipToFirstEmpty(SkillRow skill)
    {
        if (skill == null)
        {
            return -1;
        }

        for (int i = 0; i < equippedSkills.Length; i++)
        {
            if (equippedSkills[i] == null)
            {
                equippedSkills[i] = skill;
                return i;
            }
        }

        return -1;
    }

    public int Cast(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= equippedSkills.Length || playerAgent == null)
        {
            return 0;
        }

        SkillRow skill = equippedSkills[slotIndex];
        if (skill == null)
        {
            return 0;
        }

        EnemyManager enemyManager = EnemyManager.Instance;
        if (enemyManager == null)
        {
            return 0;
        }

        IReadOnlyList<Enemy> aliveEnemies = enemyManager.GetAliveEnemies();
        int hitCount = 0;
        int atk = Mathf.RoundToInt(playerAgent.AttackPower);

        for (int i = 0; i < aliveEnemies.Count; i++)
        {
            Enemy enemy = aliveEnemies[i];
            if (enemy == null || !enemy.IsAlive)
            {
                continue;
            }

            int enemyDef = Mathf.RoundToInt(enemy.Defense);
            int finalDamage = DamageCalculator.ComputeCharacterDamage(
                atk,
                enemyDef,
                playerAgent.CritChance,
                playerAgent.CritMultiplier);

            enemy.TakeDamage(finalDamage);
            hitCount++;
        }

        return hitCount;
    }

    public List<SkillRow> GetRandomCandidates(int count)
    {
        List<SkillRow> result = new List<SkillRow>();
        if (skillTable == null || skillTable.AllSkills == null)
        {
            return result;
        }

        List<SkillRow> pool = new List<SkillRow>();
        HashSet<string> equippedIds = new HashSet<string>();
        for (int i = 0; i < equippedSkills.Length; i++)
        {
            if (equippedSkills[i] != null && !string.IsNullOrWhiteSpace(equippedSkills[i].id))
            {
                equippedIds.Add(equippedSkills[i].id);
            }
        }

        for (int i = 0; i < skillTable.AllSkills.Count; i++)
        {
            SkillRow row = skillTable.AllSkills[i];
            if (row == null || (!string.IsNullOrWhiteSpace(row.id) && equippedIds.Contains(row.id)))
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
