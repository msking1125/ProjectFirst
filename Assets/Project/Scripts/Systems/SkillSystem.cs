using System.Collections.Generic;
using UnityEngine;

public class SkillSystem
{
    private readonly SkillTable skillTable;
    private readonly Agent      playerAgent;
    private readonly SkillRow[] equippedSkills  = new SkillRow[3];
    private readonly List<Enemy> aliveEnemiesBuffer = new List<Enemy>();

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
                // 버프는 플레이어 위치에 VFX 스폰
                SpawnVfxAt(skill, playerAgent.transform.position);
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

    /// <summary>범위 공격: 전체/범위 내 모든 적에게 데미지 + 각 적 위치에 VFX</summary>
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

            // 각 적 위치에 VFX 스폰
            SpawnVfxAt(skill, enemy.transform.position);

            int dmg = DamageCalculator.ComputeCharacterDamage(
                Mathf.RoundToInt(atk * skill.coefficient),
                Mathf.RoundToInt(enemy.Defense),
                playerAgent.CritChance, playerAgent.CritMultiplier, out bool isCrit);
            enemy.TakeDamage(dmg, isCrit, skill.element);
            hitCount++;
        }
        return hitCount;
    }

    /// <summary>단일 강타: 가장 가까운 적 1명, singleTargetBonus 배율 추가 + 타겟 위치에 VFX</summary>
    private int CastSingleTarget(SkillRow skill, EnemyManager enemyManager)
    {
        Enemy target = enemyManager.GetClosest(playerAgent.transform.position, skill.range);
        if (target == null || !target.IsAlive) return 0;

        // 타겟 위치에 VFX 스폰
        SpawnVfxAt(skill, target.transform.position);

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

    /// <summary>디버프: 범위/전체 적에게 약화 효과 + 각 적 위치에 VFX</summary>
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
            // 각 적 위치에 VFX 스폰
            SpawnVfxAt(skill, enemy.transform.position);
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

    /// <summary>
    /// 지정된 월드 위치에 스킬 VFX를 스폰합니다.
    /// 타겟 중심(Collider 중심 또는 +0.5f)에 배치하고,
    /// 파티클이 자연스럽게 재생된 뒤 자동 소멸합니다.
    /// </summary>
    private void SpawnVfxAt(SkillRow skill, Vector3 worldPos)
    {
        if (skill.castVfxPrefab == null) return;

        // 타겟 중심 높이 보정 (Collider가 없을 때 +0.5f)
        Vector3 spawnPos = new Vector3(worldPos.x, worldPos.y + 0.5f, worldPos.z);

        // 플레이어 → 타겟 방향으로 회전
        Vector3 dir = spawnPos - playerAgent.transform.position;
        dir.y = 0;
        Quaternion rot = dir != Vector3.zero
            ? Quaternion.LookRotation(dir.normalized)
            : playerAgent.transform.rotation;

        GameObject vfx = Object.Instantiate(skill.castVfxPrefab, spawnPos, rot);
        if (vfx == null) return;

        // 파티클 최대 재생 시간 계산 후 자동 소멸
        float lifetime = GetVfxLifetime(vfx);
        AutoDestroyVfx(vfx, lifetime);
    }

    /// <summary>VFX 오브젝트 내 모든 ParticleSystem의 최대 재생 시간을 반환합니다.</summary>
    private static float GetVfxLifetime(GameObject vfx)
    {
        float maxDuration = 2f; // 파티클이 없을 때 기본값
        foreach (ParticleSystem ps in vfx.GetComponentsInChildren<ParticleSystem>(true))
        {
            ParticleSystem.MainModule main = ps.main;
            float duration = main.duration + main.startLifetime.constantMax;
            if (duration > maxDuration) maxDuration = duration;
        }
        return maxDuration;
    }

    /// <summary>지정 시간 후 VFX를 안전하게 소멸시킵니다. (루프 파티클 포함)</summary>
    private static void AutoDestroyVfx(GameObject vfx, float delay)
    {
        // 루프 파티클은 강제로 Stop 후 소멸
        foreach (ParticleSystem ps in vfx.GetComponentsInChildren<ParticleSystem>(true))
        {
            if (ps.main.loop)
                ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
        Object.Destroy(vfx, Mathf.Max(0.1f, delay));
    }

    // ── VFX 유틸 ─────────────────────────────────────────────────────────────

    // ── 직접 캐스트 (CharUltimate 등 슬롯 없는 스킬용) ──────────────────────────

    /// <summary>
    /// 슬롯 쿨타임 없이 SkillRow를 직접 발동합니다.
    /// CharUltimateController에서 호출하며, 쿨타임은 컨트롤러가 관리합니다.
    /// vfxOverride가 있으면 SkillRow.castVfxPrefab 대신 사용합니다.
    /// </summary>
    public int CastDirect(SkillRow skill, GameObject vfxOverride = null)
    {
        if (skill == null || playerAgent == null) return 0;

        EnemyManager enemyManager = EnemyManager.Instance;
        if (enemyManager == null) return 0;

        // VFX 프리팹 임시 교체
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

        // VFX 프리팹 원복
        if (vfxOverride != null)
            skill.castVfxPrefab = originalVfx;

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
