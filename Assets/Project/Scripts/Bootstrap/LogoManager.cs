using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 로고 씬을 관리합니다.
/// CompanyLogo → GameLogo 순서로 FadeIn(0.5초) → 유지(2초) → FadeOut(0.5초) 재생 후
/// 타이틀 씬으로 자동 이동합니다.
/// 로고 이미지는 Resources/UI/Logos/ 경로에서 로드합니다.
/// </summary>
public class LogoManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup companyLogoGroup;
    [SerializeField] private Image companyLogoImage;
    [SerializeField] private CanvasGroup gameLogoGroup;
    [SerializeField] private Image gameLogoImage;

    [Header("Scene")]
    [SerializeField] private string titleSceneName = "Title";

    [Header("Timing")]
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float holdDuration = 2f;
    [SerializeField] private float fadeOutDuration = 0.5f;

    private const string LogoResourcePath = "UI/Logos/";

    private void Start()
    {
        // 로고 씬의 배경을 검은색으로 고정합니다.
        if (Camera.main != null)
        {
            Camera.main.clearFlags = CameraClearFlags.SolidColor;
            Camera.main.backgroundColor = Color.black;
        }

        LoadLogoSprites();

        companyLogoGroup.alpha = 0f;
        gameLogoGroup.alpha = 0f;

        StartCoroutine(PlayLogoSequence());
    }

    private void LoadLogoSprites()
    {
        // 인스펙터에 이미 스프라이트가 할당되어 있다면 Resources.Load를 건너뜁니다.
        if (companyLogoImage.sprite == null)
        {
            Sprite companySprite = Resources.Load<Sprite>(LogoResourcePath + "CompanyLogo");
            if (companySprite != null)
            {
                companyLogoImage.sprite = companySprite;
            }
            else
            {
                Debug.LogWarning($"[LogoManager] CompanyLogo 스프라이트를 찾을 수 없습니다: {LogoResourcePath}CompanyLogo\nResources 폴더 내부에 에셋이 있는지 확인하거나 인스펙터에서 직접 할당해주세요.");
            }
        }

        if (gameLogoImage.sprite == null)
        {
            Sprite gameSprite = Resources.Load<Sprite>(LogoResourcePath + "GameLogo");
            if (gameSprite != null)
            {
                gameLogoImage.sprite = gameSprite;
            }
            else
            {
                Debug.LogWarning($"[LogoManager] GameLogo 스프라이트를 찾을 수 없습니다: {LogoResourcePath}GameLogo\nResources 폴더 내부에 에셋이 있는지 확인하거나 인스펙터에서 직접 할당해주세요.");
            }
        }
    }

    private IEnumerator PlayLogoSequence()
    {
        yield return StartCoroutine(PlayLogoAnimation(companyLogoGroup));
        yield return StartCoroutine(PlayLogoAnimation(gameLogoGroup));
        LoadTitleScene();
    }

    private IEnumerator PlayLogoAnimation(CanvasGroup group)
    {
        yield return StartCoroutine(FadeCanvasGroup(group, 0f, 1f, fadeInDuration));
        yield return new WaitForSeconds(holdDuration);
        yield return StartCoroutine(FadeCanvasGroup(group, 1f, 0f, fadeOutDuration));
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup group, float from, float to, float duration)
    {
        float elapsed = 0f;
        group.alpha = from;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            group.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }

        group.alpha = to;
    }

    private void LoadTitleScene()
    {
        if (AsyncSceneLoader.Instance != null)
        {
            AsyncSceneLoader.Instance.LoadSceneAsync(titleSceneName, LoadSceneMode.Single);
        }
        else
        {
            // AsyncSceneLoader가 없으면 새로 생성 후 사용
            EnsureAsyncSceneLoader();

            if (AsyncSceneLoader.Instance != null)
            {
                AsyncSceneLoader.Instance.LoadSceneAsync(titleSceneName, LoadSceneMode.Single);
            }
            else
            {
                Debug.LogWarning("[LogoManager] AsyncSceneLoader 생성에 실패하여 SceneManager.LoadScene으로 직접 전환합니다.");
                SceneManager.LoadScene(titleSceneName);
            }
        }
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
            Debug.Log("[LogoManager] 기존 AsyncSceneLoader를 발견했습니다.");
            return;
        }

        // 없으면 새로 생성
        GameObject loaderObject = new GameObject("AsyncSceneLoader");
        loaderObject.AddComponent<AsyncSceneLoader>();
        DontDestroyOnLoad(loaderObject);

        Debug.Log("[LogoManager] AsyncSceneLoader가 없어 새 GameObject를 생성하고 AddComponent<AsyncSceneLoader>()로 초기화했습니다.");
    }
}
