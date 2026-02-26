using System.Collections.Generic;
using UnityEngine;

public class SkillSystem
{
    private readonly SkillTable skillTable;
    private readonly Agent      playerAgent;
    private readonly SkillRow[] equippedSkills  = new SkillRow[3];

    // ── 쿨타임 ───────────────────────────────────────────────────────────────
    // 각 슬롯의 쿨타임 종료 시각 (Time.unscaledTime 기준)
    private readonly float[] cooldownEndTimes = new float[3];

    public IReadOnlyList<SkillRow> EquippedSkills => equippedSkills;

    // ────────────────────────────────────────────────────────────────────────

    public SkillSystem(SkillTable skillTable, Agent playerAgent)
    {
        this.skillTable  = skillTable;
        this.playerAgent = playerAgent;

        for (int i = 0; i < cooldownEndTimes.Length; i++)
            cooldownEndTimes[i] = 0f;
    }

    // ── 쿨타임 공개 API ──────────────────────────────────────────────────────

    /// <summary>슬롯이 현재 쿨다운 중인지 반환</summary>
    public bool IsOnCooldown(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= cooldownEndTimes.Length) return false;
        return Time.unscaledTime < cooldownEndTimes[slotIndex];
    }

    /// <summary>슬롯의 남은 쿨다운 시간(초) 반환. 쿨다운 아니면 0</summary>
    public float GetRemainingCooldown(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= cooldownEndTimes.Length) return 0f;
        float remaining = cooldownEndTimes[slotIndex] - Time.unscaledTime;
        return remaining > 0f ? remaining : 0f;
    }

    // ── 장착 ─────────────────────────────────────────────────────────────────

    public bool Equip(SkillRow skill, int slotIndex)
    {
        if (skill == null || slotIndex < 0 || slotIndex >= equippedSkills.Length)
            return false;

        equippedSkills[slotIndex] = skill;
        cooldownEndTimes[slotIndex] = 0f; // 새 스킬 장착 시 쿨타임 초기화
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

    // ── 사용 ─────────────────────────────────────────────────────────────────

    public int Cast(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= equippedSkills.Length || playerAgent == null)
            return 0;

        SkillRow skill = equippedSkills[slotIndex];
        if (skill == null) return 0;

        // 쿨타임 중이면 사용 불가
        if (IsOnCooldown(slotIndex))
        {
            Debug.Log($"[SkillSystem] 슬롯 {slotIndex} 쿨타임 중. 남은 시간: {GetRemainingCooldown(slotIndex):F1}초");
            return 0;
        }

        EnemyManager enemyManager = EnemyManager.Instance;
        if (enemyManager == null) return 0;

        IReadOnlyList<Enemy> aliveEnemies = enemyManager.GetAliveEnemies();
        int hitCount = 0;
        int atk = Mathf.RoundToInt(playerAgent.AttackPower);

        for (int i = 0; i < aliveEnemies.Count; i++)
        {
            Enemy enemy = aliveEnemies[i];
            if (enemy == null || !enemy.IsAlive) continue;

            int enemyDef = Mathf.RoundToInt(enemy.Defense);
            int finalDamage = DamageCalculator.ComputeCharacterDamage(
                Mathf.RoundToInt(atk * skill.coefficient),
                enemyDef,
                playerAgent.CritChance,
                playerAgent.CritMultiplier);

            enemy.TakeDamage(finalDamage);
            hitCount++;
        }

        // 쿨타임 시작
        if (skill.cooldown > 0f)
            cooldownEndTimes[slotIndex] = Time.unscaledTime + skill.cooldown;

        Debug.Log($"[SkillSystem] 슬롯 {slotIndex} 스킬 발동: {skill.name}, 적중 {hitCount}마리, 쿨타임 {skill.cooldown}초");
        return hitCount;
    }

    // ── 후보 뽑기 ─────────────────────────────────────────────────────────────

    public List<SkillRow> GetRandomCandidates(int count)
    {
        List<SkillRow> result = new List<SkillRow>();
        if (skillTable == null || skillTable.AllSkills == null) return result;

        HashSet<string> equippedIds = new HashSet<string>();
        for (int i = 0; i < equippedSkills.Length; i++)
        {
            if (equippedSkills[i] != null && !string.IsNullOrWhiteSpace(equippedSkills[i].id))
                equippedIds.Add(equippedSkills[i].id);
        }

        List<SkillRow> pool = new List<SkillRow>();
        for (int i = 0; i < skillTable.AllSkills.Count; i++)
        {
            SkillRow row = skillTable.AllSkills[i];
            if (row == null) continue;
            if (!string.IsNullOrWhiteSpace(row.id) && equippedIds.Contains(row.id)) continue;
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
