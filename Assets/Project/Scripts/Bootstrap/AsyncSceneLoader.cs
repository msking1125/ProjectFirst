using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Documentation cleaned.
/// </summary>
public sealed class AsyncSceneLoader : MonoBehaviour
{
    public static AsyncSceneLoader Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>
    /// Documentation cleaned.
    /// </summary>
    public void LoadSceneAsync(string sceneName, LoadSceneMode mode = LoadSceneMode.Additive)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("[Log] Error message cleaned.");
            return;
        }

        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName, mode);
        if (asyncOperation == null)
        {
            Debug.LogError("[Log] Error message cleaned.");
            return;
        }

        asyncOperation.completed += _ =>
        {
            Debug.Log("[Log] Message cleaned.");
        };
    }

    public void LoadScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
    {
        LoadSceneAsync(sceneName, mode);
    }
}
