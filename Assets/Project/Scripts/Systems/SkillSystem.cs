using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using ProjectFirst.Data;
namespace Project
{

public class SkillSystem
{
    private readonly SkillTable skillTable;
    private readonly Agent      playerAgent;
    private readonly SkillRow[] equippedSkills  = new SkillRow[3];
    private readonly List<Enemy> aliveEnemiesBuffer = new List<Enemy>();
    private readonly float[] cooldownEndTimes = new float[3];

    public IReadOnlyList<SkillRow> EquippedSkills => equippedSkills;

    public SkillSystem(SkillTable skillTable, Agent playerAgent)
    {
        this.skillTable  = skillTable;
        this.playerAgent = playerAgent;

        for (int i = 0; i < cooldownEndTimes.Length; i++)
            cooldownEndTimes[i] = 0f;
    }
    public bool IsOnCooldown(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= cooldownEndTimes.Length) return false;
        return Time.unscaledTime < cooldownEndTimes[slotIndex];
    }
    public float GetRemainingCooldown(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= cooldownEndTimes.Length) return 0f;
        float remaining = cooldownEndTimes[slotIndex] - Time.unscaledTime;
        return remaining > 0f ? remaining : 0f;
    }

    public bool Equip(SkillRow skill, int slotIndex)
    {
        if (skill == null || slotIndex < 0 || slotIndex >= equippedSkills.Length)
            return false;

        equippedSkills[slotIndex] = skill;
        cooldownEndTimes[slotIndex] = 0f; // Reset the slot cooldown timer.
        return true;
    }

    public int EquipToFirstEmpty(SkillRow skill)
    {
        if (skill == null) return -1;

        for (int i = 0; i < equippedSkills.Length; i++)
        {
            if (equippedSkills[i] == null)
            {
                equippedSkills[i] = skill;
                cooldownEndTimes[i] = 0f;
                return i;
            }
        }
        return -1;
    }

    public int Cast(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= equippedSkills.Length || playerAgent == null)
            return 0;

        SkillRow skill = equippedSkills[slotIndex];
        if (skill == null) return 0;
        if (IsOnCooldown(slotIndex))
        {
            Debug.Log($"[SkillSystem] Slot {slotIndex} is on cooldown for {GetRemainingCooldown(slotIndex):F1}s.");
            return 0;
        }

        EnemyManager enemyManager = EnemyManager.Instance;
        if (enemyManager == null) return 0;

        int hitCount = 0;

        switch (skill.effectType)
        {
            case SkillEffectType.AllEnemies:
                hitCount = CastAllEnemies(skill, enemyManager);
                break;
            case SkillEffectType.SingleTarget:
                hitCount = CastSingleTarget(skill, enemyManager);
                break;
            case SkillEffectType.Buff:
                SpawnVfxAt(skill, playerAgent.transform.position);
                hitCount = CastBuff(skill);
                break;
            case SkillEffectType.Debuff:
                hitCount = CastDebuff(skill, enemyManager);
                break;
        }
        if (skill.cooldown > 0f)
            cooldownEndTimes[slotIndex] = Time.unscaledTime + skill.cooldown;

        Debug.Log("[Log] 상태가 갱신되었습니다.");
        return hitCount;
    }
    private int CastAllEnemies(SkillRow skill, EnemyManager enemyManager)
    {
        enemyManager.FillAliveEnemiesNonAlloc(aliveEnemiesBuffer);
        int atk = Mathf.RoundToInt(GetEffectiveAtk());
        int hitCount = 0;

        for (int i = 0; i < aliveEnemiesBuffer.Count; i++)
        {
            Enemy enemy = aliveEnemiesBuffer[i];
            if (enemy == null || !enemy.IsAlive) continue;
            if (skill.range < 9999f)
            {
                float dist = Vector3.Distance(playerAgent.transform.position, enemy.transform.position);
                if (dist > skill.range) continue;
            }
            SpawnVfxAt(skill, enemy.transform.position);

            int dmg = DamageCalculator.ComputeDamage(
                atk * skill.coefficient,
                enemy.Defense,
                playerAgent.CritChance, playerAgent.CritMultiplier,
                skill.element, enemy.Element, out bool isCrit);
            enemy.TakeDamage(dmg, isCrit, skill.element);
            hitCount++;
        }
        return hitCount;
    }
    private int CastSingleTarget(SkillRow skill, EnemyManager enemyManager)
    {
        Enemy target = enemyManager.GetClosest(playerAgent.transform.position, skill.range);
        if (target == null || !target.IsAlive) return 0;
        SpawnVfxAt(skill, target.transform.position);

        int atk = Mathf.RoundToInt(GetEffectiveAtk());
        float totalCoeff = skill.coefficient * skill.singleTargetBonus;
        int dmg = DamageCalculator.ComputeDamage(
            atk * totalCoeff,
            target.Defense,
            playerAgent.CritChance, playerAgent.CritMultiplier,
            skill.element, target.Element, out bool isCrit);
        target.TakeDamage(dmg, isCrit, skill.element);
        return 1;
    }
    private int CastBuff(SkillRow skill)
    {
        AgentBuffSystem buffSystem = playerAgent.GetComponent<AgentBuffSystem>();
        if (buffSystem == null)
            buffSystem = playerAgent.gameObject.AddComponent<AgentBuffSystem>();

        buffSystem.ApplyBuff(skill.buffStat, skill.buffMultiplier, skill.buffDuration);
        return 1;
    }
    private int CastDebuff(SkillRow skill, EnemyManager enemyManager)
    {
        enemyManager.FillAliveEnemiesNonAlloc(aliveEnemiesBuffer);
        int count = 0;
        for (int i = 0; i < aliveEnemiesBuffer.Count; i++)
        {
            Enemy enemy = aliveEnemiesBuffer[i];
            if (enemy == null || !enemy.IsAlive) continue;
            if (skill.range < 9999f)
            {
                float dist = Vector3.Distance(playerAgent.transform.position, enemy.transform.position);
                if (dist > skill.range) continue;
            }
            SpawnVfxAt(skill, enemy.transform.position);
            enemy.ApplyDebuff(skill.debuffType, skill.debuffValue, skill.debuffDuration);
            count++;
        }
        return count;
    }
    private float GetEffectiveAtk()
    {
        AgentBuffSystem buff = playerAgent.GetComponent<AgentBuffSystem>();
        return buff != null ? buff.GetBuffedAttackPower() : playerAgent.AttackPower;
    }
    private void SpawnVfxAt(SkillRow skill, Vector3 worldPos)
    {
        if (skill.castVfxPrefab == null) return;
        Vector3 spawnPos = new Vector3(worldPos.x, worldPos.y + 0.5f, worldPos.z);
        Vector3 dir = spawnPos - playerAgent.transform.position;
        dir.y = 0;
        Quaternion rot = dir != Vector3.zero
            ? Quaternion.LookRotation(dir.normalized)
            : playerAgent.transform.rotation;

        GameObject vfx = UnityEngine.Object.Instantiate(skill.castVfxPrefab, spawnPos, rot);
        if (vfx == null) return;
        float lifetime = GetVfxLifetime(vfx);
        AutoDestroyVfx(vfx, lifetime);
    }
    private static float GetVfxLifetime(GameObject vfx)
    {
        float maxDuration = 2f; // Fallback duration when no particle system is available.
        foreach (ParticleSystem ps in vfx.GetComponentsInChildren<ParticleSystem>(true))
        {
            ParticleSystem.MainModule main = ps.main;
            float duration = main.duration + main.startLifetime.constantMax;
            if (duration > maxDuration) maxDuration = duration;
        }
        return maxDuration;
    }
    private static void AutoDestroyVfx(GameObject vfx, float delay)
    {
        foreach (ParticleSystem ps in vfx.GetComponentsInChildren<ParticleSystem>(true))
        {
            if (ps.main.loop)
                ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
        UnityEngine.Object.Destroy(vfx, Mathf.Max(0.1f, delay));
    }
    public int CastDirect(SkillRow skill, GameObject vfxOverride = null)
    {
        if (skill == null || playerAgent == null) return 0;

        EnemyManager enemyManager = EnemyManager.Instance;
        if (enemyManager == null) return 0;
        GameObject originalVfx = skill.castVfxPrefab;
        if (vfxOverride != null)
            skill.castVfxPrefab = vfxOverride;

        int hitCount = 0;
        switch (skill.effectType)
        {
            case SkillEffectType.AllEnemies:
                hitCount = CastAllEnemies(skill, enemyManager);
                break;
            case SkillEffectType.SingleTarget:
                hitCount = CastSingleTarget(skill, enemyManager);
                break;
            case SkillEffectType.Buff:
                SpawnVfxAt(skill, playerAgent.transform.position);
                hitCount = CastBuff(skill);
                break;
            case SkillEffectType.Debuff:
                hitCount = CastDebuff(skill, enemyManager);
                break;
        }
        if (vfxOverride != null)
            skill.castVfxPrefab = originalVfx;

        return hitCount;
    }

    public List<SkillRow> GetRandomCandidates(int count)
    {
        List<SkillRow> result = new List<SkillRow>();
        if (skillTable == null || skillTable.AllSkills == null) return result;

        HashSet<int> equippedIds = new HashSet<int>();
        for (int i = 0; i < equippedSkills.Length; i++)
        {
            if (equippedSkills[i] != null && equippedSkills[i].id > 0)
                equippedIds.Add(equippedSkills[i].id);
        }

        List<SkillRow> pool = new List<SkillRow>();
        for (int i = 0; i < skillTable.AllSkills.Count; i++)
        {
            SkillRow row = skillTable.AllSkills[i];
            if (row == null) continue;
            if (row.id > 0 && equippedIds.Contains(row.id)) continue;
            pool.Add(row);
        }

        while (pool.Count > 0 && result.Count < count)
        {
            int pick = UnityEngine.Random.Range(0, pool.Count);
            result.Add(pool[pick]);
            pool.RemoveAt(pick);
        }

        return result;
    }
}
} // namespace Project




