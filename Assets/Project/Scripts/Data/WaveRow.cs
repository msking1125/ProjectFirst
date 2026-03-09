using System;

[Serializable]
public class WaveRow
{
    private const int DefaultMonsterId = 1;

    public int wave;
    public int spawnCount;
    public float spawnInterval;
    public float enemyHpMul;
    public float enemySpeedMul;
    public float enemyDamageMul;
    public int eliteEvery;
    public bool boss;
    public int rewardGold;
    public int enemyId = DefaultMonsterId;
    public int monsterId = 0;

    public int GetMonsterIdOrFallback()
    {
        if (enemyId > 0)
        {
            return enemyId;
        }

        if (monsterId > 0)
        {
            return monsterId;
        }

        return DefaultMonsterId;
    }
}
