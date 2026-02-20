using System;
using TMPro;
using UnityEngine;

[AddComponentMenu("Enemy/Wave Manager")]
public class WaveManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WaveTable waveTable;
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private EnemyManager enemyManager;
    [SerializeField] private BattleGameManager gameManager;
    [SerializeField] private bool endVictoryIfGameManagerMissing = true;

    [Header("UI")]
    [SerializeField] private TMP_Text waveText;
    [SerializeField] private TMP_Text aliveEnemyText;

    private int currentWaveIndex = -1;
    private bool waveInProgress;
    private bool gameEnded;
    private bool loggedMissingWaveTable;
    private bool loggedMissingSpawner;

    private void Awake()
    {
        if (enemySpawner == null)
        {
            enemySpawner = FindFirstObjectByType<EnemySpawner>();
        }

        if (enemyManager == null)
        {
            enemyManager = EnemyManager.Instance != null ? EnemyManager.Instance : FindFirstObjectByType<EnemyManager>();
        }

        if (gameManager == null)
        {
            gameManager = BattleGameManager.Instance != null ? BattleGameManager.Instance : FindFirstObjectByType<BattleGameManager>();
        }
    }

    private void Start()
    {
        if (!CanStartWaveSystem())
        {
            RefreshUI();
            return;
        }

        StartNextWave();
    }

    private void Update()
    {
        RefreshUI();

        if (gameEnded || !waveInProgress)
        {
            return;
        }

        int aliveCount = GetAliveEnemyCount();
        if (enemySpawner.IsWaveSpawnCompleted && aliveCount <= 0)
        {
            StartNextWave();
        }
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

    private void StartNextWave()
    {
        currentWaveIndex++;

        if (waveTable == null || waveTable.wave == null || currentWaveIndex >= waveTable.wave.Count)
        {
            waveInProgress = false;
            gameEnded = true;
            NotifyVictory();
            RefreshUI();
            return;
        }

        WaveRow row = waveTable.wave[currentWaveIndex];
        if (row == null)
        {
            Debug.LogWarning($"[WaveManager] waveTable.wave[{currentWaveIndex}]가 null 입니다. 다음 웨이브로 건너뜁니다.");
            StartNextWave();
            return;
        }

        enemySpawner.ConfigureWave(
            row.spawnCount,
            row.spawnInterval,
            row.enemyHpMul,
            row.enemySpeedMul,
            row.enemyDamageMul,
            row.eliteEvery,
            row.boss,
            string.IsNullOrWhiteSpace(row.enemyId) ? "slime" : row.enemyId);

        enemySpawner.BeginWave();
        waveInProgress = true;
        RefreshUI();
    }

    private void NotifyVictory()
    {
        if (gameManager == null)
        {
            gameManager = BattleGameManager.Instance != null ? BattleGameManager.Instance : FindFirstObjectByType<BattleGameManager>();
        }

        if (gameManager != null)
        {
            gameManager.HandleVictory();
            return;
        }

        if (endVictoryIfGameManagerMissing)
        {
            BattleGameManager.EndVictoryFallback();
        }
    }

    private int GetAliveEnemyCount()
    {
        if (enemyManager == null)
        {
            enemyManager = EnemyManager.Instance != null ? EnemyManager.Instance : FindFirstObjectByType<EnemyManager>();
        }

        return enemyManager != null ? enemyManager.GetAliveCount() : 0;
    }

    private void RefreshUI()
    {
        if (waveText != null)
        {
            int displayWave = Mathf.Max(0, currentWaveIndex + 1);
            int totalWave = waveTable != null && waveTable.wave != null ? waveTable.wave.Count : 0;
            waveText.text = $"Wave: {displayWave}/{totalWave}";
        }

        if (aliveEnemyText != null)
        {
            aliveEnemyText.text = $"Alive: {GetAliveEnemyCount()}";
        }
    }
}
