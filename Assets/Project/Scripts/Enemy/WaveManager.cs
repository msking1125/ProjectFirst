using System;
using TMPro;
using UnityEngine;

namespace Project
{

[AddComponentMenu("Enemy/Wave Manager")]
public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    private const string DefaultMonsterId = "1";

    [Header("References")]
    [SerializeField] private WaveTable waveTable;
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private EnemyManager enemyManager;
    [SerializeField] private BattleGameManager gameManager;
    [SerializeField] private bool endVictoryIfGameManagerMissing = true;

    [Header("UI")]
    [SerializeField] private TMP_Text waveText;
    [SerializeField] private TMP_Text aliveEnemyText;

    private int currentWaveNum = 0;
    public int CurrentWave => currentWaveNum;
    private bool waveInProgress;
    private bool gameEnded;
    private bool loggedMissingWaveTable;
    private bool loggedMissingSpawner;
    private int lastAliveEnemyCount = -1;
    private int cachedTotalWaveCount = -1;
    private int cachedTotalWaveSourceCount = -1;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (enemySpawner == null)
            enemySpawner = FindFirstObjectByType<EnemySpawner>();

        if (enemyManager == null)
            enemyManager = EnemyManager.Instance != null ? EnemyManager.Instance : FindFirstObjectByType<EnemyManager>();

        if (gameManager == null)
            gameManager = BattleGameManager.Instance != null ? BattleGameManager.Instance : FindFirstObjectByType<BattleGameManager>();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Start()
    {
        if (!CanStartWaveSystem())
        {
            RefreshUI(GetAliveEnemyCount());
            return;
        }

        StartNextWave();
    }

    private void Update()
    {
        int aliveCount = GetAliveEnemyCount();
        RefreshUI(aliveCount);

        if (gameEnded || !waveInProgress)
            return;

        if (enemySpawner.IsWaveSpawnCompleted && aliveCount <= 0)
            StartNextWave();
    }

    private bool CanStartWaveSystem()
    {
        if (waveTable == null)
        {
            if (!loggedMissingWaveTable)
            {
                Debug.LogError("[WaveManager] waveTable이 할당되지 않았습니다.");
                loggedMissingWaveTable = true;
            }

            return false;
        }

        loggedMissingWaveTable = false;

        if (enemySpawner == null)
        {
            if (!loggedMissingSpawner)
            {
                Debug.LogError("[WaveManager] enemySpawner가 할당되지 않았습니다.");
                loggedMissingSpawner = true;
            }

            return false;
        }

        loggedMissingSpawner = false;
        return true;
    }

    public void StartNextWave()
    {
        int nextWave = int.MaxValue;
        if (waveTable != null && waveTable.wave != null)
        {
            foreach (var r in waveTable.wave)
            {
                if (r != null && r.wave > currentWaveNum && r.wave < nextWave)
                    nextWave = r.wave;
            }
        }

        if (nextWave == int.MaxValue)
        {
            waveInProgress = false;
            gameEnded = true;
            NotifyVictory();
            RefreshUI(GetAliveEnemyCount());
            return;
        }

        currentWaveNum = nextWave;
        System.Collections.Generic.List<WaveRow> currentRows = new System.Collections.Generic.List<WaveRow>();

        if (waveTable != null && waveTable.wave != null)
        {
            foreach (var r in waveTable.wave)
            {
                if (r != null && r.wave == currentWaveNum)
                    currentRows.Add(r);
            }
        }

        enemySpawner.ConfigureWave(currentRows);
        enemySpawner.BeginWave();
        waveInProgress = true;
        RefreshUI(GetAliveEnemyCount());
    }

    private void NotifyVictory()
    {
        if (gameManager == null)
            gameManager = BattleGameManager.Instance != null ? BattleGameManager.Instance : FindFirstObjectByType<BattleGameManager>();

        if (gameManager != null)
        {
            gameManager.HandleVictory();
            return;
        }

        if (endVictoryIfGameManagerMissing)
            BattleGameManager.EndVictoryFallback();
    }

    private int GetAliveEnemyCount()
    {
        if (enemyManager == null)
            enemyManager = EnemyManager.Instance != null ? EnemyManager.Instance : FindFirstObjectByType<EnemyManager>();

        return enemyManager != null ? enemyManager.GetAliveCount() : 0;
    }

    private void RefreshUI(int aliveCount)
    {
        if (waveText != null)
        {
            int displayWave = Mathf.Max(0, currentWaveNum);
            int totalWave = GetTotalWaveCount();
            waveText.text = $"Wave: {displayWave}/{totalWave}";
        }

        if (aliveEnemyText != null && aliveCount != lastAliveEnemyCount)
        {
            aliveEnemyText.text = $"Alive: {aliveCount}";
            lastAliveEnemyCount = aliveCount;
        }
    }

    private int GetTotalWaveCount()
    {
        if (waveTable == null || waveTable.wave == null)
        {
            cachedTotalWaveCount = 0;
            cachedTotalWaveSourceCount = 0;
            return 0;
        }

        if (cachedTotalWaveCount >= 0 && cachedTotalWaveSourceCount == waveTable.wave.Count)
            return cachedTotalWaveCount;

        System.Collections.Generic.HashSet<int> waves = new System.Collections.Generic.HashSet<int>();
        foreach (var r in waveTable.wave)
        {
            if (r != null)
                waves.Add(r.wave);
        }

        cachedTotalWaveCount = waves.Count;
        cachedTotalWaveSourceCount = waveTable.wave.Count;
        return cachedTotalWaveCount;
    }
}
} // namespace Project