using UnityEngine;

/// <summary>
/// Documentation cleaned.
/// </summary>
public static class DamageCalculator
{
    public const float MaxCritChance = 0.8f;

    /// <summary>
    /// Documentation cleaned.
    /// Documentation cleaned.
    /// </summary>
    public static int ComputeDamage(float attackerAtk, float defenderDef, float critChance, float critMultiplier, 
        ElementType attackerElement, ElementType defenderElement, out bool isCrit, float defCoefficient = 0.5f)
    {
        // Note: cleaned comment.
        float baseDmg = Mathf.Max(1f, attackerAtk - (defenderDef * defCoefficient));

        // Note: cleaned comment.
        float clampedCritChance = Mathf.Clamp(critChance, 0f, MaxCritChance);
        isCrit = Random.value < clampedCritChance;
        float critFactor = isCrit ? Mathf.Max(1f, critMultiplier) : 1f;

        // Note: cleaned comment.
        float elementFactor = ElementRules.GetMultiplier(attackerElement, defenderElement);

        // Note: cleaned comment.
        float finalDmg = baseDmg * critFactor * elementFactor;

        return Mathf.Max(1, Mathf.RoundToInt(finalDmg));
    }

    /// <summary>
    /// Documentation cleaned.
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
