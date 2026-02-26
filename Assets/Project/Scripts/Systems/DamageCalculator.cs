using UnityEngine;

public static class DamageCalculator
{
    public static int ComputeBasicDamage(
        int atk,
        int def,
        float critChance,
        float critMultiplier,
        ElementType atkElem,
        ElementType defElem)
    {
        return ComputeBasicDamage(atk, def, critChance, critMultiplier, atkElem, defElem, out _);
    }

    public static int ComputeBasicDamage(
        int atk,
        int def,
        float critChance,
        float critMultiplier,
        ElementType atkElem,
        ElementType defElem,
        out bool isCrit)
    {
        float clampedCritChance = Mathf.Clamp01(critChance);
        float clampedCritMultiplier = Mathf.Max(1f, critMultiplier);

        float damage = Mathf.Max(1f, atk - def);
        isCrit = Random.value < clampedCritChance;

        if (isCrit)
        {
            damage *= clampedCritMultiplier;
        }

        if (ElementTypeHelper.HasAdvantage(atkElem, defElem))
        {
            damage *= ElementTypeHelper.AdvantageMultiplier;
        }

        return Mathf.RoundToInt(damage);
    }

    public static int ComputeSkillDamage(
        int atk,
        int def,
        float coefficient,
        float critChance,
        float critMultiplier,
        ElementType atkElem,
        ElementType defElem)
    {
        float clampedCoefficient = Mathf.Max(0f, coefficient);
        float clampedCritChance = Mathf.Clamp01(critChance);
        float clampedCritMultiplier = Mathf.Max(1f, critMultiplier);

        float damage = Mathf.Max(1f, atk * clampedCoefficient - def);

        if (Random.value < clampedCritChance)
        {
            damage *= clampedCritMultiplier;
        }

        if (ElementTypeHelper.HasAdvantage(atkElem, defElem))
        {
            damage *= ElementTypeHelper.AdvantageMultiplier;
        }

        return Mathf.RoundToInt(damage);
    }
}
