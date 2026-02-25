using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 부트스트랩 씬에서 타이틀 씬을 최초로 로드하는 로더.
/// </summary>
public class BootstrapLoader : MonoBehaviour
{
    [SerializeField] private string titleSceneName = "Title";

    private void Start()
    {
        if (string.IsNullOrEmpty(titleSceneName))
        {
            Debug.LogWarning("[BootstrapLoader] titleSceneName 이 비어 있습니다.");
            return;
        }

        // 타이틀 씬을 Additive 로 로드하고, 현재 씬은 ActiveScene 으로 남겨둠.
        AsyncSceneLoader.Instance.LoadSceneAsync(titleSceneName, LoadSceneMode.Additive);
    }
}

