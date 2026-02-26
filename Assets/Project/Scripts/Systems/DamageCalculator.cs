using UnityEngine;

/// <summary>
/// 캐릭터/몬스터 데미지 계산 유틸리티
/// </summary>
public static class DamageCalculator
{
    /// <summary>
    /// 공격자의 ATK, 방어자의 DEF, 크리티컬 확률/배수로 최종 피해량 계산
    /// isCrit: 이번 공격이 크리티컬인지 out 반환
    /// </summary>
    public static int ComputeCharacterDamage(int atk, int def, float critChance, float critMultiplier, out bool isCrit)
    {
        float baseDmg = Mathf.Max(1f, atk - def);
        isCrit = Random.value < critChance;
        float finalDmg = isCrit ? baseDmg * Mathf.Max(1f, critMultiplier) : baseDmg;
        return Mathf.Max(1, Mathf.RoundToInt(finalDmg));
    }

    /// <summary>
    /// isCrit 없이 사용하는 오버로드 (하위 호환)
    /// </summary>
    public static int ComputeCharacterDamage(int atk, int def, float critChance, float critMultiplier)
    {
        bool _ ;
        return ComputeCharacterDamage(atk, def, critChance, critMultiplier, out _);
    }
}
