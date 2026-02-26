using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 씬 비동기 로드를 담당하는 싱글톤 매니저.
/// </summary>
public class AsyncSceneLoader : MonoBehaviour
{
    // 어디서든 접근 가능한 싱글톤 인스턴스
    public static AsyncSceneLoader Instance { get; private set; }

    private void Awake()
    {
        // 이미 인스턴스가 존재하면 중복 오브젝트 제거
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // 씬 전환 시에도 로더를 유지
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 지정한 씬을 비동기로 로드합니다.
    /// 기본 모드는 Additive 입니다.
    /// </summary>
    public void LoadSceneAsync(string sceneName, LoadSceneMode mode = LoadSceneMode.Additive)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning("[AsyncSceneLoader] sceneName 이 비어 있습니다.");
            return;
        }

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, mode);

        if (operation == null)
        {
            Debug.LogError($"[AsyncSceneLoader] '{sceneName}' 씬 로드를 시작하지 못했습니다.");
            return;
        }

        // 로드 완료 시 로그 출력
        operation.completed += _ =>
        {
            Debug.Log($"[AsyncSceneLoader] 씬 로드 완료: {sceneName} ({mode})");
        };
    }
}
