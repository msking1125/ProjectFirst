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
        LoadLogoSprites();

        companyLogoGroup.alpha = 0f;
        gameLogoGroup.alpha = 0f;

        StartCoroutine(PlayLogoSequence());
    }

    private void LoadLogoSprites()
    {
        Sprite companySprite = Resources.Load<Sprite>(LogoResourcePath + "CompanyLogo");
        if (companySprite != null)
        {
            companyLogoImage.sprite = companySprite;
        }
        else
        {
            Debug.LogWarning($"[LogoManager] CompanyLogo 스프라이트를 찾을 수 없습니다: {LogoResourcePath}CompanyLogo");
        }

        Sprite gameSprite = Resources.Load<Sprite>(LogoResourcePath + "GameLogo");
        if (gameSprite != null)
        {
            gameLogoImage.sprite = gameSprite;
        }
        else
        {
            Debug.LogWarning($"[LogoManager] GameLogo 스프라이트를 찾을 수 없습니다: {LogoResourcePath}GameLogo");
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
            Debug.LogWarning("[LogoManager] AsyncSceneLoader 인스턴스가 없어 SceneManager.LoadScene으로 직접 전환합니다.");
            SceneManager.LoadScene(titleSceneName);
        }
    }
}
