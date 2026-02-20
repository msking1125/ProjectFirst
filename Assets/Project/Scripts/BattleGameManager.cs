using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BattleGameManager : MonoBehaviour
{
    public static BattleGameManager Instance { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoCreate()
    {
        if (Instance != null)
        {
            return;
        }

        GameObject managerObject = new GameObject("BattleGameManager");
        managerObject.AddComponent<BattleGameManager>();
    }

    [Header("Base")]
    [SerializeField] private BaseHealth baseHealth;
    [SerializeField] private string baseObjectName = "Ark_Base";

    [Header("Result UI")]
    [SerializeField] private Canvas targetCanvas;
    [SerializeField] private GameObject resultPanel;

    [Header("Scene")]
    [SerializeField] private string titleSceneName = "Title";

    private TMP_Text resultText;
    private bool gameEnded;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Time.timeScale = 1f;

        EnsureBaseHealth();
        EnsureResultUI();
        SetResultUI(false, string.Empty);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void HandleVictory()
    {
        EndGame("Victory");
    }

    public void HandleDefeat()
    {
        EndGame("Defeat");
    }

    public static void EndVictoryFallback()
    {
        BattleGameManager manager = Instance;
        if (manager == null)
        {
            GameObject managerObject = new GameObject("BattleGameManager");
            manager = managerObject.AddComponent<BattleGameManager>();
        }

        manager.HandleVictory();
    }

    public static void ReportBaseDestroyed()
    {
        if (Instance == null)
        {
            GameObject managerObject = new GameObject("BattleGameManager");
            managerObject.AddComponent<BattleGameManager>();
        }

        if (Instance != null)
        {
            Instance.HandleDefeat();
        }
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.name);
    }

    public void BackToTitle()
    {
        Time.timeScale = 1f;
        if (Application.CanStreamedLevelBeLoaded(titleSceneName))
        {
            SceneManager.LoadScene(titleSceneName);
            return;
        }

        SceneManager.LoadScene("Battle_Test");
    }

    private void EndGame(string message)
    {
        if (gameEnded)
        {
            return;
        }

        gameEnded = true;
        Time.timeScale = 0f;
        SetResultUI(true, message);
    }

    private void EnsureBaseHealth()
    {
        if (baseHealth != null)
        {
            baseHealth.BindGameManager(this);
            return;
        }

        GameObject baseObject = GameObject.Find(baseObjectName) ?? GameObject.Find("Base");
        if (baseObject == null)
        {
            return;
        }

        baseHealth = baseObject.GetComponent<BaseHealth>();
        if (baseHealth == null)
        {
            baseHealth = baseObject.AddComponent<BaseHealth>();
        }

        baseHealth.BindGameManager(this);
    }

    private void EnsureResultUI()
    {
        if (targetCanvas == null)
        {
            targetCanvas = FindFirstObjectByType<Canvas>();
        }

        if (targetCanvas == null)
        {
            GameObject canvasObject = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            targetCanvas = canvasObject.GetComponent<Canvas>();
            targetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        if (resultPanel == null)
        {
            resultPanel = new GameObject("ResultPanel", typeof(RectTransform), typeof(Image));
            resultPanel.transform.SetParent(targetCanvas.transform, false);

            RectTransform panelRect = resultPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(560f, 320f);

            Image panelImage = resultPanel.GetComponent<Image>();
            panelImage.color = new Color(0f, 0f, 0f, 0.85f);

            resultText = CreateText("ResultText", resultPanel.transform, 64f, new Vector2(0f, 96f), 56f);
            CreateButton("RestartButton", "Restart", new Vector2(0f, -12f), Restart);
            CreateButton("BackButton", "Back To Title", new Vector2(0f, -102f), BackToTitle);
        }
        else if (resultText == null)
        {
            resultText = resultPanel.GetComponentInChildren<TMP_Text>(true);
        }
    }

    private TMP_Text CreateText(string objectName, Transform parent, float width, Vector2 pos, float fontSize)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.sizeDelta = new Vector2(width * 6f, 90f);
        textRect.anchoredPosition = pos;

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = fontSize;
        text.color = Color.white;

        return text;
    }

    private void CreateButton(string objectName, string label, Vector2 pos, UnityEngine.Events.UnityAction onClick)
    {
        GameObject buttonObject = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(resultPanel.transform, false);

        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(300f, 64f);
        buttonRect.anchoredPosition = pos;

        Image buttonImage = buttonObject.GetComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);

        Button button = buttonObject.GetComponent<Button>();
        button.onClick.AddListener(onClick);

        TMP_Text buttonText = CreateText($"{objectName}_Label", buttonObject.transform, 48f, Vector2.zero, 34f);
        buttonText.text = label;
    }

    private void SetResultUI(bool active, string message)
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(active);
        }

        if (resultText != null)
        {
            resultText.text = message;
        }
    }
}
