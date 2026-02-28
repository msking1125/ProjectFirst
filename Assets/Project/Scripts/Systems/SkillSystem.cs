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

        // VFX 스폰
        SpawnCastVfx(skill);

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
                hitCount = CastBuff(skill);
                break;
            case SkillEffectType.Debuff:
                hitCount = CastDebuff(skill, enemyManager);
                break;
        }

        // 쿨타임 시작
        if (skill.cooldown > 0f)
            cooldownEndTimes[slotIndex] = Time.unscaledTime + skill.cooldown;

        Debug.Log($"[SkillSystem] [{skill.effectType}] {skill.name} 발동, 적중/효과 {hitCount}, 쿨 {skill.cooldown}s");
        return hitCount;
    }

    // ── 효과 타입별 처리 ─────────────────────────────────────────────────────

    /// <summary>범위 공격: 전체/범위 내 모든 적에게 데미지</summary>
    private int CastAllEnemies(SkillRow skill, EnemyManager enemyManager)
    {
        IReadOnlyList<Enemy> aliveEnemies = enemyManager.GetAliveEnemies();
        int atk = Mathf.RoundToInt(GetEffectiveAtk());
        int hitCount = 0;

        for (int i = 0; i < aliveEnemies.Count; i++)
        {
            Enemy enemy = aliveEnemies[i];
            if (enemy == null || !enemy.IsAlive) continue;
            if (skill.range < 9999f)
            {
                float dist = Vector3.Distance(playerAgent.transform.position, enemy.transform.position);
                if (dist > skill.range) continue;
            }

            int dmg = DamageCalculator.ComputeCharacterDamage(
                Mathf.RoundToInt(atk * skill.coefficient),
                Mathf.RoundToInt(enemy.Defense),
                playerAgent.CritChance, playerAgent.CritMultiplier, out bool isCrit);
            enemy.TakeDamage(dmg, isCrit, skill.element);
            hitCount++;
        }
        return hitCount;
    }

    /// <summary>단일 강타: 가장 가까운 적 1명, singleTargetBonus 배율 추가</summary>
    private int CastSingleTarget(SkillRow skill, EnemyManager enemyManager)
    {
        Enemy target = enemyManager.GetClosest(playerAgent.transform.position, skill.range);
        if (target == null || !target.IsAlive) return 0;

        int atk = Mathf.RoundToInt(GetEffectiveAtk());
        float totalCoeff = skill.coefficient * skill.singleTargetBonus;
        int dmg = DamageCalculator.ComputeCharacterDamage(
            Mathf.RoundToInt(atk * totalCoeff),
            Mathf.RoundToInt(target.Defense),
            playerAgent.CritChance, playerAgent.CritMultiplier, out bool isCrit);
        target.TakeDamage(dmg, isCrit, skill.element);
        return 1;
    }

    /// <summary>버프: 플레이어에게 스탯 강화 적용</summary>
    private int CastBuff(SkillRow skill)
    {
        AgentBuffSystem buffSystem = playerAgent.GetComponent<AgentBuffSystem>();
        if (buffSystem == null)
            buffSystem = playerAgent.gameObject.AddComponent<AgentBuffSystem>();

        buffSystem.ApplyBuff(skill.buffStat, skill.buffMultiplier, skill.buffDuration);
        return 1;
    }

    /// <summary>디버프: 범위/전체 적에게 약화 효과</summary>
    private int CastDebuff(SkillRow skill, EnemyManager enemyManager)
    {
        IReadOnlyList<Enemy> aliveEnemies = enemyManager.GetAliveEnemies();
        int count = 0;
        for (int i = 0; i < aliveEnemies.Count; i++)
        {
            Enemy enemy = aliveEnemies[i];
            if (enemy == null || !enemy.IsAlive) continue;
            if (skill.range < 9999f)
            {
                float dist = Vector3.Distance(playerAgent.transform.position, enemy.transform.position);
                if (dist > skill.range) continue;
            }
            enemy.ApplyDebuff(skill.debuffType, skill.debuffValue, skill.debuffDuration);
            count++;
        }
        return count;
    }

    /// <summary>버프가 적용된 실제 공격력 반환</summary>
    private float GetEffectiveAtk()
    {
        AgentBuffSystem buff = playerAgent.GetComponent<AgentBuffSystem>();
        return buff != null ? buff.GetBuffedAttackPower() : playerAgent.AttackPower;
    }

    private void SpawnCastVfx(SkillRow skill)
    {
        if (skill.castVfxPrefab == null) return;
        Vector3 spawnPos = playerAgent.transform.position + playerAgent.transform.forward;
        GameObject vfx = Object.Instantiate(skill.castVfxPrefab, spawnPos, playerAgent.transform.rotation);
        if (vfx != null) StopAndDestroyVfx(vfx);
    }

    // ── VFX 유틸 ─────────────────────────────────────────────────────────────

    /// <summary>
    /// 루프 파티클도 안전하게 소멸시킵니다.
    /// Stop() 없이 Destroy만 하면 looping 파티클이 씬에 영원히 남습니다.
    /// </summary>
    private static void StopAndDestroyVfx(GameObject vfx, float delay = 0f)
    {
        foreach (ParticleSystem ps in vfx.GetComponentsInChildren<ParticleSystem>(true))
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        Object.Destroy(vfx);
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
