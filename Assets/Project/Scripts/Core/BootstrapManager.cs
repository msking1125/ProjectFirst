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
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // 씬이 바뀌어도 BootstrapManager를 유지
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // BootstrapLoader의 기존 방어 로직을 동일하게 통합
        if (string.IsNullOrEmpty(titleSceneName))
        {
            Debug.LogWarning("[BootstrapManager] titleSceneName 이 비어 있습니다.");
            return;
        }

        // 타이틀 씬을 Additive로 로드
        AsyncSceneLoader.Instance.LoadSceneAsync(titleSceneName, LoadSceneMode.Additive);
    }
}
