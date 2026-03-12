using UnityEngine;

/// <summary>
/// 캐릭터/몬스터 데미지 계산 유틸리티
/// </summary>
public static class DamageCalculator
{
    public const float MaxCritChance = 0.8f;

    /// <summary>
    /// 공격자의 ATK, 방어자의 DEF, 크리티컬 확률/배수로 최종 피해량 계산
    /// 데미지 = (공격력 - 방어력 × 방어계수) × (치명타? 치명타배율 : 1) × 속성상성계수
    /// </summary>
    public static int ComputeDamage(float attackerAtk, float defenderDef, float critChance, float critMultiplier, 
        ElementType attackerElement, ElementType defenderElement, out bool isCrit, float defCoefficient = 0.5f)
    {
        // 1. 기본 데미지 계산 (방어 계수 적용)
        float baseDmg = Mathf.Max(1f, attackerAtk - (defenderDef * defCoefficient));

        // 2. 크리티컬 판정 (최대 80% 캡 적용)
        float clampedCritChance = Mathf.Clamp(critChance, 0f, MaxCritChance);
        isCrit = Random.value < clampedCritChance;
        float critFactor = isCrit ? Mathf.Max(1f, critMultiplier) : 1f;

        // 3. 속성 상성 적용
        float elementFactor = ElementRules.GetMultiplier(attackerElement, defenderElement);

        // 4. 최종 데미지 합산
        float finalDmg = baseDmg * critFactor * elementFactor;

        return Mathf.Max(1, Mathf.RoundToInt(finalDmg));
    }

    /// <summary>
    /// 레거시 지원을 위한 오버로드
    /// </summary>
    public static int ComputeCharacterDamage(float atk, float def, float critChance, float critMultiplier, out bool isCrit)
    {
        return ComputeDamage(atk, def, critChance, critMultiplier, ElementType.Reason, ElementType.Reason, out isCrit);
    }

    public static int ComputeCharacterDamage(float atk, float def, float critChance, float critMultiplier)
    {
        bool _;
        return ComputeDamage(atk, def, critChance, critMultiplier, ElementType.Reason, ElementType.Reason, out _);
    }
}
