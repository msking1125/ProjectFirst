using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Plays the boot logo sequence.
/// CompanyLogo and GameLogo fade in and out in order, then the title scene is loaded.
/// Logo sprites are loaded from Resources/UI/Logos/ when not assigned in the inspector.
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
        // Keep the camera background black while logos are shown.
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
        // Skip Resources.Load when sprites are already assigned in the inspector.
        if (companyLogoImage.sprite == null)
        {
            Sprite companySprite = Resources.Load<Sprite>(LogoResourcePath + "CompanyLogo");
            if (companySprite != null)
            {
                companyLogoImage.sprite = companySprite;
            }
            else
            {
                Debug.LogWarning($"[LogoManager] Could not find CompanyLogo at {LogoResourcePath}CompanyLogo. Check the Resources folder or assign the sprite directly in the inspector.");
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
                Debug.LogWarning($"[LogoManager] Could not find GameLogo at {LogoResourcePath}GameLogo. Check the Resources folder or assign the sprite directly in the inspector.");
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
            // Create or reuse AsyncSceneLoader before falling back to SceneManager.
            EnsureAsyncSceneLoader();

            if (AsyncSceneLoader.Instance != null)
            {
                AsyncSceneLoader.Instance.LoadSceneAsync(titleSceneName, LoadSceneMode.Single);
            }
            else
            {
                Debug.LogWarning("[LogoManager] Failed to prepare AsyncSceneLoader. Falling back to SceneManager.LoadScene.");
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

        // Find an existing loader, including inactive objects.
        AsyncSceneLoader existingLoader = FindObjectOfType<AsyncSceneLoader>(true);
        if (existingLoader != null)
        {
            Debug.Log("[LogoManager] Found an existing AsyncSceneLoader.");
            return;
        }

        // Create one if no loader exists.
        GameObject loaderObject = new GameObject("AsyncSceneLoader");
        loaderObject.AddComponent<AsyncSceneLoader>();
        DontDestroyOnLoad(loaderObject);

        Debug.Log("[LogoManager] Created a new AsyncSceneLoader GameObject and initialized it.");
    }
}

