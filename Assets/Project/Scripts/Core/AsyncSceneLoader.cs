using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Bootstrap 씬에서 생성되어 전역으로 사용하는 씬 비동기 로더 싱글톤입니다.
/// </summary>
public sealed class AsyncSceneLoader : MonoBehaviour
{
    // 어디서든 접근 가능한 싱글톤 인스턴스
    public static AsyncSceneLoader Instance { get; private set; }

    private void Awake()
    {
        // 이미 다른 인스턴스가 있으면 현재 오브젝트를 제거하여 싱글톤 보장
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        // 요구사항: Awake에서 인스턴스 등록 및 씬 전환 시 유지
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    private void OnDestroy()
    {
        // 파괴 시 정적 참조를 정리해 잘못된 참조를 방지
        if (Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>
    /// 씬을 비동기로 로드합니다.
    /// </summary>
    /// <param name="sceneName">로드할 씬 이름</param>
    /// <param name="mode">씬 로드 모드 (기본: Additive)</param>
    public void LoadSceneAsync(string sceneName, LoadSceneMode mode = LoadSceneMode.Additive)
    {
        // null/빈 문자열 방어 코드
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("[AsyncSceneLoader] sceneName이 null 또는 빈 문자열입니다.");
            return;
        }

        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName, mode);

        // Unity 상황에 따라 null이 반환될 수 있으므로 안전 체크
        if (asyncOperation == null)
        {
            Debug.LogError("[AsyncSceneLoader] 씬 로드 시작에 실패했습니다: " + sceneName);
            return;
        }

        // 요구사항: 로드 완료 로그 출력
        asyncOperation.completed += _ =>
        {
            Debug.Log("[AsyncSceneLoader] " + sceneName + " 로드 완료");
        };
    }
}
