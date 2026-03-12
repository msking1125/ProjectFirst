using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Bootstrap 씬에서 생성되어 전역으로 사용하는 씬 비동기 로더 싱글톤입니다.
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
    /// 씬을 비동기로 로드합니다.
    /// </summary>
    public void LoadSceneAsync(string sceneName, LoadSceneMode mode = LoadSceneMode.Additive)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("[AsyncSceneLoader] sceneName이 null 또는 빈 문자열입니다.");
            return;
        }

        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName, mode);
        if (asyncOperation == null)
        {
            Debug.LogError("[AsyncSceneLoader] 씬 로드 시작에 실패했습니다: " + sceneName);
            return;
        }

        asyncOperation.completed += _ =>
        {
            Debug.Log("[AsyncSceneLoader] " + sceneName + " 로드 완료");
        };
    }

    public void LoadScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
    {
        LoadSceneAsync(sceneName, mode);
    }
}
