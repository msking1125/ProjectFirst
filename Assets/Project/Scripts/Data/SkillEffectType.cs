/// <summary>스킬 효과 타입</summary>
public enum SkillEffectType
{
    AllEnemies   = 0,  // 범위 공격: 범위 내 모든 적
    SingleTarget = 1,  // 단일 강타: 가장 가까운 적 1명, 높은 계수
    Buff         = 2,  // 버프: 플레이어 스탯 일시 강화
    Debuff       = 3,  // 디버프: 적 스탯 약화 / 슬로우
}

/// <summary>버프 대상 스탯</summary>
public enum BuffStatType
{
    AttackPower = 0,   // 공격력 %
    Defense     = 1,   // 방어력 %
    AttackSpeed = 2,   // 공격 속도 %
}

/// <summary>디버프 종류</summary>
public enum DebuffType
{
    Slow        = 0,   // 이동/공격 슬로우
    WeakenAtk   = 1,   // 공격력 감소
    WeakenDef   = 2,   // 방어력 감소
}
