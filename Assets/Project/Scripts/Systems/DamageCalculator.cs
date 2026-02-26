using UnityEngine;

public static class DamageCalculator
{
    private static bool hasLoggedCombatDebug;

    public static int ComputeCharacterDamage(
        int atk,
        int def,
        float critChance,
        float critMultiplier)
    {
        float baseDamage = Mathf.Max(1f, atk - def);

        bool isCrit = Random.value < critChance;
        if (isCrit)
        {
            baseDamage *= critMultiplier;
        }

        int dmg = Mathf.RoundToInt(baseDamage);

        if (!hasLoggedCombatDebug)
        {
            Debug.Log($"[Combat] atk={atk}, def={def}, critChance={critChance}, critMul={critMultiplier}, finalDmg={dmg}");
            hasLoggedCombatDebug = true;
        }

        return dmg;
    }
}
