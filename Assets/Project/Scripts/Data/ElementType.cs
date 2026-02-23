using UnityEngine;

public enum ElementType
{
    Passion,
    Intuition,
    Reason
}

public static class ElementRules
{
    public const float AdvantageMultiplier = 1.5f;
    public const float NeutralMultiplier = 1f;

    public static bool HasAdvantage(ElementType attacker, ElementType defender)
    {
        return (attacker == ElementType.Passion && defender == ElementType.Intuition)
               || (attacker == ElementType.Intuition && defender == ElementType.Reason)
               || (attacker == ElementType.Reason && defender == ElementType.Passion);
    }

    public static float GetMultiplier(ElementType attacker, ElementType defender)
    {
        return HasAdvantage(attacker, defender) ? AdvantageMultiplier : NeutralMultiplier;
    }
}
