using System;

[Serializable]
public class WaveRow
{
    public int wave;
    public int spawnCount;
    public float spawnInterval;
    public float enemyHpMul;
    public float enemySpeedMul;
    public float enemyDamageMul;
    public int eliteEvery;
    public bool boss;
    public int rewardGold;
}