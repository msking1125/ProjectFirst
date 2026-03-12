using UnityEngine;
using UnityEngine.SceneManagement;
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
    public void LoadSceneAsync(string sceneName, LoadSceneMode mode = LoadSceneMode.Additive)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("[Log] 오류가 발생했습니다.");
            return;
        }

        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName, mode);
        if (asyncOperation == null)
        {
            Debug.LogError("[Log] 오류가 발생했습니다.");
            return;
        }

        asyncOperation.completed += _ =>
        {
            Debug.Log("[Log] 상태가 갱신되었습니다.");
        };
    }

    public void LoadScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
    {
        LoadSceneAsync(sceneName, mode);
    }
}

