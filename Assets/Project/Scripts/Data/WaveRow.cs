using System;

[Serializable]
public class WaveRow
{
    private const string DefaultMonsterId = "1";

    public int wave;
    public int spawnCount;
    public float spawnInterval;
    public float enemyHpMul;
    public float enemySpeedMul;
    public float enemyDamageMul;
    public int eliteEvery;
    public bool boss;
    public int rewardGold;
    public string enemyId = DefaultMonsterId;
    public string monsterId = string.Empty;

    public string GetMonsterIdOrFallback()
    {
        if (!string.IsNullOrWhiteSpace(enemyId))
        {
            return enemyId;
        }

        if (!string.IsNullOrWhiteSpace(monsterId))
        {
            return monsterId;
        }

        return DefaultMonsterId;
    }
}
