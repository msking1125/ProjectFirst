using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 씬 비동기 로드를 담당하는 싱글톤 로더입니다.
/// </summary>
public class AsyncSceneLoader : MonoBehaviour
{
    // 어디서든 접근 가능한 싱글톤 인스턴스
    public static AsyncSceneLoader Instance { get; private set; }

    private void Awake()
    {
        // 중복 인스턴스가 있으면 현재 오브젝트 제거 (Unity 2022.3 안전 패턴)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // 싱글톤 인스턴스 등록 및 씬 전환 간 유지
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    private void OnDestroy()
    {
        // 파괴 시 정적 참조 정리
        if (Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>
    /// 지정한 씬을 비동기로 로드합니다.
    /// 기본 모드는 Additive 입니다.
    /// </summary>
    public void LoadSceneAsync(string sceneName, LoadSceneMode mode = LoadSceneMode.Additive)
    {
        // 잘못된 입력값 방어
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("[AsyncSceneLoader] sceneName이 비어 있어 씬을 로드할 수 없습니다.");
            return;
        }

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName, mode);

        // 비동기 로드 시작 실패 방어
        if (loadOperation == null)
        {
            Debug.LogError("[AsyncSceneLoader] SceneManager.LoadSceneAsync가 null을 반환했습니다: " + sceneName);
            return;
        }

        // 로드 완료 시 로그 출력
        loadOperation.completed += _ =>
        {
            Debug.Log("[AsyncSceneLoader] " + sceneName + " 로드 완료");
        };
    }
}
