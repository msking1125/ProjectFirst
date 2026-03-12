using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Bootstrap entry manager.
/// It loads the title scene additively on startup and keeps shared bootstrap objects alive.
/// </summary>
public class BootstrapManager : MonoBehaviour
{
    // Globally accessible singleton instance.
    public static BootstrapManager Instance { get; private set; }

    [SerializeField] private string titleSceneName = "Title";

    private void Awake()
    {
        // Destroy duplicate bootstrap instances.
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[BootstrapManager] Duplicate instance detected. Destroying the new object.");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Preserve the bootstrap manager across scene loads.
        DontDestroyOnLoad(gameObject);

        EnsureAsyncSceneLoader();
    }

    private void Start()
    {
        if (string.IsNullOrWhiteSpace(titleSceneName))
        {
            Debug.LogError("[BootstrapManager] titleSceneName is empty. Title scene loading was aborted.");
            return;
        }

        if (AsyncSceneLoader.Instance == null)
        {
            Debug.LogWarning("[BootstrapManager] AsyncSceneLoader.Instance was null on Start. Trying to create or find one.");
            EnsureAsyncSceneLoader();
        }

        if (AsyncSceneLoader.Instance == null)
        {
            Debug.LogError($"[BootstrapManager] Failed to initialize AsyncSceneLoader. Cannot load scene '{titleSceneName}'.");
            return;
        }

        AsyncSceneLoader.Instance.LoadSceneAsync(titleSceneName, LoadSceneMode.Additive);
        Debug.Log($"[BootstrapManager] Requested additive load for title scene: {titleSceneName}");
    }

    private void EnsureAsyncSceneLoader()
    {
        if (AsyncSceneLoader.Instance != null)
        {
            return;
        }

        // Find an existing loader, including inactive objects.
        AsyncSceneLoader existingLoader = FindObjectOfType<AsyncSceneLoader>(true);
        if (existingLoader != null)
        {
            Debug.Log("[BootstrapManager] Found an existing AsyncSceneLoader.");
            return;
        }

        GameObject loaderObject = new GameObject("AsyncSceneLoader");
        loaderObject.AddComponent<AsyncSceneLoader>();
        DontDestroyOnLoad(loaderObject);

        Debug.Log("[BootstrapManager] Created a new AsyncSceneLoader GameObject and initialized it.");
    }
}
