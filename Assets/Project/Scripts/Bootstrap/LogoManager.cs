using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 濡쒓퀬 ?ъ쓣 愿由ы빀?덈떎.
/// CompanyLogo ??GameLogo ?쒖꽌濡?FadeIn(0.5珥? ???좎?(2珥? ??FadeOut(0.5珥? ?ъ깮 ??
/// ??댄? ?ъ쑝濡??먮룞 ?대룞?⑸땲??
/// 濡쒓퀬 ?대?吏??Resources/UI/Logos/ 寃쎈줈?먯꽌 濡쒕뱶?⑸땲??
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
        // 濡쒓퀬 ?ъ쓽 諛곌꼍??寃??됱쑝濡?怨좎젙?⑸땲??
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
        // ?몄뒪?숉꽣???대? ?ㅽ봽?쇱씠?멸? ?좊떦?섏뼱 ?덈떎硫?Resources.Load瑜?嫄대꼫?곷땲??
        if (companyLogoImage.sprite == null)
        {
            Sprite companySprite = Resources.Load<Sprite>(LogoResourcePath + "CompanyLogo");
            if (companySprite != null)
            {
                companyLogoImage.sprite = companySprite;
            }
            else
            {
                Debug.LogWarning($"[LogoManager] CompanyLogo ?ㅽ봽?쇱씠?몃? 李얠쓣 ???놁뒿?덈떎: {LogoResourcePath}CompanyLogo\nResources ?대뜑 ?대????먯뀑???덈뒗吏 ?뺤씤?섍굅???몄뒪?숉꽣?먯꽌 吏곸젒 ?좊떦?댁＜?몄슂.");
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
                Debug.LogWarning($"[LogoManager] GameLogo ?ㅽ봽?쇱씠?몃? 李얠쓣 ???놁뒿?덈떎: {LogoResourcePath}GameLogo\nResources ?대뜑 ?대????먯뀑???덈뒗吏 ?뺤씤?섍굅???몄뒪?숉꽣?먯꽌 吏곸젒 ?좊떦?댁＜?몄슂.");
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
            // AsyncSceneLoader媛 ?놁쑝硫??덈줈 ?앹꽦 ???ъ슜
            EnsureAsyncSceneLoader();

            if (AsyncSceneLoader.Instance != null)
            {
                AsyncSceneLoader.Instance.LoadSceneAsync(titleSceneName, LoadSceneMode.Single);
            }
            else
            {
                Debug.LogWarning("[LogoManager] AsyncSceneLoader ?앹꽦???ㅽ뙣?섏뿬 SceneManager.LoadScene?쇰줈 吏곸젒 ?꾪솚?⑸땲??");
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

        // ????鍮꾪솢???ы븿) 湲곗〈 AsyncSceneLoader ?먯깋
        AsyncSceneLoader existingLoader = FindObjectOfType<AsyncSceneLoader>(true);
        if (existingLoader != null)
        {
            Debug.Log("[LogoManager] 湲곗〈 AsyncSceneLoader瑜?諛쒓껄?덉뒿?덈떎.");
            return;
        }

        // ?놁쑝硫??덈줈 ?앹꽦
        GameObject loaderObject = new GameObject("AsyncSceneLoader");
        loaderObject.AddComponent<AsyncSceneLoader>();
        DontDestroyOnLoad(loaderObject);

        Debug.Log("[LogoManager] AsyncSceneLoader媛 ?놁뼱 ??GameObject瑜??앹꽦?섍퀬 AddComponent<AsyncSceneLoader>()濡?珥덇린?뷀뻽?듬땲??");
    }
}
