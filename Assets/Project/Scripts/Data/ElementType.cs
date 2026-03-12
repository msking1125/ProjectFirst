public enum ElementType
{
    All = -1,
    Passion,
    Intuition,
    Reason
}

public static class ElementTypeHelper
{
    public const float AdvantageMultiplier = 1.5f;
    public const float NeutralMultiplier = 1f;
    public const float DisadvantageMultiplier = 0.7f;

    public static bool HasAdvantage(ElementType attacker, ElementType defender)
    {
        return (attacker == ElementType.Passion && defender == ElementType.Intuition)
               || (attacker == ElementType.Intuition && defender == ElementType.Reason)
               || (attacker == ElementType.Reason && defender == ElementType.Passion);
    }

    public static bool HasDisadvantage(ElementType attacker, ElementType defender)
    {
        return (attacker == ElementType.Intuition && defender == ElementType.Passion)
               || (attacker == ElementType.Reason && defender == ElementType.Intuition)
               || (attacker == ElementType.Passion && defender == ElementType.Reason);
    }

    public static float GetMultiplier(ElementType attacker, ElementType defender)
    {
        if (HasAdvantage(attacker, defender)) return AdvantageMultiplier;
        if (HasDisadvantage(attacker, defender)) return DisadvantageMultiplier;
        return NeutralMultiplier;
    }
}

public static class ElementRules
{
    public const float AdvantageMultiplier = ElementTypeHelper.AdvantageMultiplier;
    public const float NeutralMultiplier = ElementTypeHelper.NeutralMultiplier;
    public const float DisadvantageMultiplier = ElementTypeHelper.DisadvantageMultiplier;

    public static bool HasAdvantage(ElementType attacker, ElementType defender)
    {
        return ElementTypeHelper.HasAdvantage(attacker, defender);
    }

    public static bool HasDisadvantage(ElementType attacker, ElementType defender)
    {
        return ElementTypeHelper.HasDisadvantage(attacker, defender);
    }

    public static float GetMultiplier(ElementType attacker, ElementType defender)
    {
        return ElementTypeHelper.GetMultiplier(attacker, defender);
    }
}
