using UnityEngine;

/// <summary>
/// 罹먮┃??紐ъ뒪???곕?吏 怨꾩궛 ?좏떥由ы떚
/// </summary>
public static class DamageCalculator
{
    public const float MaxCritChance = 0.8f;

    /// <summary>
    /// 怨듦꺽?먯쓽 ATK, 諛⑹뼱?먯쓽 DEF, ?щ━?곗뺄 ?뺣쪧/諛곗닔濡?理쒖쥌 ?쇳빐??怨꾩궛
    /// ?곕?吏 = (怨듦꺽??- 諛⑹뼱??횞 諛⑹뼱怨꾩닔) 횞 (移섎챸?? 移섎챸?諛곗쑉 : 1) 횞 ?띿꽦?곸꽦怨꾩닔
    /// </summary>
    public static int ComputeDamage(float attackerAtk, float defenderDef, float critChance, float critMultiplier, 
        ElementType attackerElement, ElementType defenderElement, out bool isCrit, float defCoefficient = 0.5f)
    {
        // 1. 湲곕낯 ?곕?吏 怨꾩궛 (諛⑹뼱 怨꾩닔 ?곸슜)
        float baseDmg = Mathf.Max(1f, attackerAtk - (defenderDef * defCoefficient));

        // 2. ?щ━?곗뺄 ?먯젙 (理쒕? 80% 罹??곸슜)
        float clampedCritChance = Mathf.Clamp(critChance, 0f, MaxCritChance);
        isCrit = Random.value < clampedCritChance;
        float critFactor = isCrit ? Mathf.Max(1f, critMultiplier) : 1f;

        // 3. ?띿꽦 ?곸꽦 ?곸슜
        float elementFactor = ElementRules.GetMultiplier(attackerElement, defenderElement);

        // 4. 理쒖쥌 ?곕?吏 ?⑹궛
        float finalDmg = baseDmg * critFactor * elementFactor;

        return Mathf.Max(1, Mathf.RoundToInt(finalDmg));
    }

    /// <summary>
    /// ?덇굅??吏?먯쓣 ?꾪븳 ?ㅻ쾭濡쒕뱶
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
