using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ?꾨줈?앺듃 遺?몄뒪?몃옪???대떦?섎뒗 留ㅻ땲?.
/// 理쒖큹 ?ㅽ뻾 ????댄? ?ъ쓣 Additive濡?濡쒕뱶?섎ŉ, ?ㅻ툕?앺듃瑜????꾪솚 媛??좎??⑸땲??
/// </summary>
public class BootstrapManager : MonoBehaviour
{
    // ?꾩뿭?먯꽌 ?묎렐 媛?ν븳 ?깃????몄뒪?댁뒪
    public static BootstrapManager Instance { get; private set; }

    [SerializeField] private string titleSceneName = "Title";

    private void Awake()
    {
        // ?대? ?몄뒪?댁뒪媛 ?덈떎硫?以묐났 ?ㅻ툕?앺듃 ?쒓굅
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[BootstrapManager] Duplicate instance detected. Destroying the new object.");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // ?ъ씠 諛붾뚯뼱??BootstrapManager瑜??좎?
        DontDestroyOnLoad(gameObject);

        EnsureAsyncSceneLoader();
    }

    private void Start()
    {
        if (string.IsNullOrWhiteSpace(titleSceneName))
        {
            Debug.LogError("[BootstrapManager] Title Scene Name(titleSceneName)??鍮꾩뼱 ?덉뼱 ??댄? ??濡쒕뱶瑜?以묐떒?⑸땲??");
            return;
        }

        if (AsyncSceneLoader.Instance == null)
        {
            Debug.LogWarning("[BootstrapManager] Start ?쒖젏??AsyncSceneLoader.Instance媛 null?낅땲?? ?ъ깮?깆쓣 ?쒕룄?⑸땲??");
            EnsureAsyncSceneLoader();
        }

        if (AsyncSceneLoader.Instance == null)
        {
            Debug.LogError($"[BootstrapManager] AsyncSceneLoader 珥덇린?붿뿉 ?ㅽ뙣?덉뒿?덈떎. ??'{titleSceneName}' 濡쒕뱶瑜?吏꾪뻾?????놁뒿?덈떎.");
            return;
        }

        AsyncSceneLoader.Instance.LoadSceneAsync(titleSceneName, LoadSceneMode.Additive);
        Debug.Log($"[BootstrapManager] ??댄? ??Additive 濡쒕뱶 ?붿껌: {titleSceneName}");
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
            Debug.Log("[BootstrapManager] 湲곗〈 AsyncSceneLoader瑜?諛쒓껄?덉뒿?덈떎.");
            return;
        }

        GameObject loaderObject = new GameObject("AsyncSceneLoader");
        loaderObject.AddComponent<AsyncSceneLoader>();
        DontDestroyOnLoad(loaderObject);

        Debug.Log("[BootstrapManager] AsyncSceneLoader媛 ?놁뼱 ??GameObject瑜??앹꽦?섍퀬 AddComponent<AsyncSceneLoader>()濡?珥덇린?뷀뻽?듬땲??");
    }
}
