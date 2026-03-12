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

    // ?? 荑⑦??????????????????????????????????????????????????????????????????
    // 媛??щ’??荑⑦???醫낅즺 ?쒓컖 (Time.unscaledTime 湲곗?)
    private readonly float[] cooldownEndTimes = new float[3];

    public IReadOnlyList<SkillRow> EquippedSkills => equippedSkills;

    // ????????????????????????????????????????????????????????????????????????

    public SkillSystem(SkillTable skillTable, Agent playerAgent)
    {
        this.skillTable  = skillTable;
        this.playerAgent = playerAgent;

        for (int i = 0; i < cooldownEndTimes.Length; i++)
            cooldownEndTimes[i] = 0f;
    }

    // ?? 荑⑦???怨듦컻 API ??????????????????????????????????????????????????????

    /// <summary>?щ’???꾩옱 荑⑤떎??以묒씤吏 諛섑솚</summary>
    public bool IsOnCooldown(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= cooldownEndTimes.Length) return false;
        return Time.unscaledTime < cooldownEndTimes[slotIndex];
    }

    /// <summary>?щ’???⑥? 荑⑤떎???쒓컙(珥? 諛섑솚. 荑⑤떎???꾨땲硫?0</summary>
    public float GetRemainingCooldown(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= cooldownEndTimes.Length) return 0f;
        float remaining = cooldownEndTimes[slotIndex] - Time.unscaledTime;
        return remaining > 0f ? remaining : 0f;
    }

    // ?? ?μ갑 ?????????????????????????????????????????????????????????????????

    public bool Equip(SkillRow skill, int slotIndex)
    {
        if (skill == null || slotIndex < 0 || slotIndex >= equippedSkills.Length)
            return false;

        equippedSkills[slotIndex] = skill;
        cooldownEndTimes[slotIndex] = 0f; // ???ㅽ궗 ?μ갑 ??荑⑦???珥덇린??
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

    // ?? ?ъ슜 ?????????????????????????????????????????????????????????????????

    public int Cast(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= equippedSkills.Length || playerAgent == null)
            return 0;

        SkillRow skill = equippedSkills[slotIndex];
        if (skill == null) return 0;

        // 荑⑦???以묒씠硫??ъ슜 遺덇?
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
                // 踰꾪봽???뚮젅?댁뼱 ?꾩튂??VFX ?ㅽ룿
                SpawnVfxAt(skill, playerAgent.transform.position);
                hitCount = CastBuff(skill);
                break;
            case SkillEffectType.Debuff:
                hitCount = CastDebuff(skill, enemyManager);
                break;
        }

        // 荑⑦????쒖옉
        if (skill.cooldown > 0f)
            cooldownEndTimes[slotIndex] = Time.unscaledTime + skill.cooldown;

        Debug.Log($"[SkillSystem] [{skill.effectType}] {skill.name} 諛쒕룞, ?곸쨷/?④낵 {hitCount}, 荑?{skill.cooldown}s");
        return hitCount;
    }

    // ?? ?④낵 ??낅퀎 泥섎━ ?????????????????????????????????????????????????????

    /// <summary>踰붿쐞 怨듦꺽: ?꾩껜/踰붿쐞 ??紐⑤뱺 ?곸뿉寃??곕?吏 + 媛????꾩튂??VFX</summary>
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

            // 媛????꾩튂??VFX ?ㅽ룿
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

    /// <summary>?⑥씪 媛뺥?: 媛??媛源뚯슫 ??1紐? singleTargetBonus 諛곗쑉 異붽? + ?寃??꾩튂??VFX</summary>
    private int CastSingleTarget(SkillRow skill, EnemyManager enemyManager)
    {
        Enemy target = enemyManager.GetClosest(playerAgent.transform.position, skill.range);
        if (target == null || !target.IsAlive) return 0;

        // ?寃??꾩튂??VFX ?ㅽ룿
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

    /// <summary>踰꾪봽: ?뚮젅?댁뼱?먭쾶 ?ㅽ꺈 媛뺥솕 ?곸슜</summary>
    private int CastBuff(SkillRow skill)
    {
        AgentBuffSystem buffSystem = playerAgent.GetComponent<AgentBuffSystem>();
        if (buffSystem == null)
            buffSystem = playerAgent.gameObject.AddComponent<AgentBuffSystem>();

        buffSystem.ApplyBuff(skill.buffStat, skill.buffMultiplier, skill.buffDuration);
        return 1;
    }

    /// <summary>?붾쾭?? 踰붿쐞/?꾩껜 ?곸뿉寃??쏀솕 ?④낵 + 媛????꾩튂??VFX</summary>
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
            // 媛????꾩튂??VFX ?ㅽ룿
            SpawnVfxAt(skill, enemy.transform.position);
            enemy.ApplyDebuff(skill.debuffType, skill.debuffValue, skill.debuffDuration);
            count++;
        }
        return count;
    }

    /// <summary>踰꾪봽媛 ?곸슜???ㅼ젣 怨듦꺽??諛섑솚</summary>
    private float GetEffectiveAtk()
    {
        AgentBuffSystem buff = playerAgent.GetComponent<AgentBuffSystem>();
        return buff != null ? buff.GetBuffedAttackPower() : playerAgent.AttackPower;
    }

    /// <summary>
    /// 吏?뺣맂 ?붾뱶 ?꾩튂???ㅽ궗 VFX瑜??ㅽ룿?⑸땲??
    /// ?寃?以묒떖(Collider 以묒떖 ?먮뒗 +0.5f)??諛곗튂?섍퀬,
    /// ?뚰떚?댁씠 ?먯뿰?ㅻ읇寃??ъ깮?????먮룞 ?뚮㈇?⑸땲??
    /// </summary>
    private void SpawnVfxAt(SkillRow skill, Vector3 worldPos)
    {
        if (skill.castVfxPrefab == null) return;

        // ?寃?以묒떖 ?믪씠 蹂댁젙 (Collider媛 ?놁쓣 ??+0.5f)
        Vector3 spawnPos = new Vector3(worldPos.x, worldPos.y + 0.5f, worldPos.z);

        // ?뚮젅?댁뼱 ???寃?諛⑺뼢?쇰줈 ?뚯쟾
        Vector3 dir = spawnPos - playerAgent.transform.position;
        dir.y = 0;
        Quaternion rot = dir != Vector3.zero
            ? Quaternion.LookRotation(dir.normalized)
            : playerAgent.transform.rotation;

        GameObject vfx = UnityEngine.Object.Instantiate(skill.castVfxPrefab, spawnPos, rot);
        if (vfx == null) return;

        // ?뚰떚??理쒕? ?ъ깮 ?쒓컙 怨꾩궛 ???먮룞 ?뚮㈇
        float lifetime = GetVfxLifetime(vfx);
        AutoDestroyVfx(vfx, lifetime);
    }

    /// <summary>VFX ?ㅻ툕?앺듃 ??紐⑤뱺 ParticleSystem??理쒕? ?ъ깮 ?쒓컙??諛섑솚?⑸땲??</summary>
    private static float GetVfxLifetime(GameObject vfx)
    {
        float maxDuration = 2f; // ?뚰떚?댁씠 ?놁쓣 ??湲곕낯媛?
        foreach (ParticleSystem ps in vfx.GetComponentsInChildren<ParticleSystem>(true))
        {
            ParticleSystem.MainModule main = ps.main;
            float duration = main.duration + main.startLifetime.constantMax;
            if (duration > maxDuration) maxDuration = duration;
        }
        return maxDuration;
    }

    /// <summary>吏???쒓컙 ??VFX瑜??덉쟾?섍쾶 ?뚮㈇?쒗궢?덈떎. (猷⑦봽 ?뚰떚???ы븿)</summary>
    private static void AutoDestroyVfx(GameObject vfx, float delay)
    {
        // 猷⑦봽 ?뚰떚?댁? 媛뺤젣濡?Stop ???뚮㈇
        foreach (ParticleSystem ps in vfx.GetComponentsInChildren<ParticleSystem>(true))
        {
            if (ps.main.loop)
                ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
        UnityEngine.Object.Destroy(vfx, Mathf.Max(0.1f, delay));
    }

    // ?? VFX ?좏떥 ?????????????????????????????????????????????????????????????

    // ?? 吏곸젒 罹먯뒪??(CharUltimate ???щ’ ?녿뒗 ?ㅽ궗?? ??????????????????????????

    /// <summary>
    /// ?щ’ 荑⑦????놁씠 SkillRow瑜?吏곸젒 諛쒕룞?⑸땲??
    /// CharUltimateController?먯꽌 ?몄텧?섎ŉ, 荑⑦??꾩? 而⑦듃濡ㅻ윭媛 愿由ы빀?덈떎.
    /// vfxOverride媛 ?덉쑝硫?SkillRow.castVfxPrefab ????ъ슜?⑸땲??
    /// </summary>
    public int CastDirect(SkillRow skill, GameObject vfxOverride = null)
    {
        if (skill == null || playerAgent == null) return 0;

        EnemyManager enemyManager = EnemyManager.Instance;
        if (enemyManager == null) return 0;

        // VFX ?꾨━???꾩떆 援먯껜
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

        // VFX ?꾨━???먮났
        if (vfxOverride != null)
            skill.castVfxPrefab = originalVfx;

        return hitCount;
    }

    // ?? ?꾨낫 戮묎린 ?????????????????????????????????????????????????????????????

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



