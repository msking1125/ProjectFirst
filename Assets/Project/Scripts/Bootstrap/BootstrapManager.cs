using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 프로젝트 부트스트랩을 담당하는 매니저.
/// 최초 실행 시 타이틀 씬을 Additive로 로드하며, 오브젝트를 씬 전환 간 유지합니다.
/// </summary>
public class BootstrapManager : MonoBehaviour
{
    // 전역에서 접근 가능한 싱글톤 인스턴스
    public static BootstrapManager Instance { get; private set; }

    [SerializeField] private string titleSceneName = "Title";

    private void Awake()
    {
        // 이미 인스턴스가 있다면 중복 오브젝트 제거
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[BootstrapManager] Duplicate instance detected. Destroying the new object.");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // 씬이 바뀌어도 BootstrapManager를 유지
        DontDestroyOnLoad(gameObject);

        EnsureAsyncSceneLoader();
    }

    private void Start()
    {
        if (string.IsNullOrWhiteSpace(titleSceneName))
        {
            Debug.LogError("[BootstrapManager] Title Scene Name(titleSceneName)이 비어 있어 타이틀 씬 로드를 중단합니다.");
            return;
        }

        if (AsyncSceneLoader.Instance == null)
        {
            Debug.LogWarning("[BootstrapManager] Start 시점에 AsyncSceneLoader.Instance가 null입니다. 재생성을 시도합니다.");
            EnsureAsyncSceneLoader();
        }

        if (AsyncSceneLoader.Instance == null)
        {
            Debug.LogError($"[BootstrapManager] AsyncSceneLoader 초기화에 실패했습니다. 씬 '{titleSceneName}' 로드를 진행할 수 없습니다.");
            return;
        }

        AsyncSceneLoader.Instance.LoadSceneAsync(titleSceneName, LoadSceneMode.Additive);
        Debug.Log($"[BootstrapManager] 타이틀 씬 Additive 로드 요청: {titleSceneName}");
    }

    private void EnsureAsyncSceneLoader()
    {
        if (AsyncSceneLoader.Instance != null)
        {
            return;
        }

        // 씬 내(비활성 포함) 기존 AsyncSceneLoader 탐색
        AsyncSceneLoader existingLoader = FindObjectOfType<AsyncSceneLoader>(true);
        if (existingLoader != null)
        {
            Debug.Log("[BootstrapManager] 기존 AsyncSceneLoader를 발견했습니다.");
            return;
        }

        GameObject loaderObject = new GameObject("AsyncSceneLoader");
        loaderObject.AddComponent<AsyncSceneLoader>();
        DontDestroyOnLoad(loaderObject);

        Debug.Log("[BootstrapManager] AsyncSceneLoader가 없어 새 GameObject를 생성하고 AddComponent<AsyncSceneLoader>()로 초기화했습니다.");
    }
}
