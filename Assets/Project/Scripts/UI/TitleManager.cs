using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// 타이틀 씬 UI 관리자 (Cyberpunk Dark Neon Style - 9:16 모바일 최적화)
/// 기존 UI Toolkit 방식에서 uGUI 방식으로 대대적으로 개편되었습니다.
/// 시작 시 Canvas가 없으면 코드로 자동 생성합니다.
/// </summary>
public class TitleManager : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string gameSceneName = "Battle_Test";

    [Header("UI Texts")]
    [SerializeField] private TMP_FontAsset customFont;
    [SerializeField] private string startButtonText = "게임 시작";
    [SerializeField] private string settingsButtonText = "설정";
    [SerializeField] private string quitButtonText = "종료";

    [Header("Events (Optional)")]
    [SerializeField] private VoidEventChannelSO startButtonEvent;
    [SerializeField] private VoidEventChannelSO settingsButtonEvent;
    [SerializeField] private VoidEventChannelSO quitButtonEvent;

    private Canvas targetCanvas;

    private void Awake()
    {
        EnsureCanvasAndUI();
    }

    private void EnsureCanvasAndUI()
    {
        // 최상단 Canvas 찾기
        targetCanvas = FindObjectOfType<Canvas>();
        
        if (targetCanvas == null)
        {
            GameObject canvasObj = new GameObject("TitleCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            targetCanvas = canvasObj.GetComponent<Canvas>();
            targetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f); // 9:16 모바일 기준
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f; // 너비/높이 중간 매칭
            
            // UI 생성
            BuildUI();
        }
        else
        {
            // 만약 이미 Canvas가 있다면 내부 구조 체크 후 없으면 생성
            if (targetCanvas.transform.Find("CyberpunkOverlay") == null)
            {
                CanvasScaler scaler = targetCanvas.GetComponent<CanvasScaler>();
                if (scaler != null)
                {
                    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    scaler.referenceResolution = new Vector2(1080f, 1920f);
                    scaler.matchWidthOrHeight = 0.5f;
                }
                BuildUI();
            }
        }
        
        // Hide existing UIDocument if present to remove the old UI at the bottom
        var uiDoc = FindObjectOfType<UnityEngine.UIElements.UIDocument>();
        if (uiDoc != null && uiDoc.rootVisualElement != null)
        {
            uiDoc.rootVisualElement.style.display = UnityEngine.UIElements.DisplayStyle.None;
        }
    }

    private void BuildUI()
    {
        // 1. 다크 오버레이 (기존 포스터 이미지를 덮어 로고/버튼 가시성 확보)
        GameObject overlay = new GameObject("CyberpunkOverlay", typeof(RectTransform), typeof(Image));
        overlay.transform.SetParent(targetCanvas.transform, false);
        SetFullScreenStretch(overlay.GetComponent<RectTransform>());
        Image overlayImg = overlay.GetComponent<Image>();
        overlayImg.color = new Color(0.02f, 0.02f, 0.05f, 0.75f); // 딥 다크 블루 + 반투명

        // 2. 로고 영역 생성 (상단 중심)
        GameObject logoRoot = new GameObject("LogoRoot", typeof(RectTransform));
        logoRoot.transform.SetParent(targetCanvas.transform, false);
        RectTransform logoRt = logoRoot.GetComponent<RectTransform>();
        logoRt.anchorMin = new Vector2(0.5f, 0.7f);
        logoRt.anchorMax = new Vector2(0.5f, 0.7f);
        logoRt.anchoredPosition = Vector2.zero;

        // 로고 텍스트는 제거되었습니다 (포스터 이미지 내 로고 사용)

        // 3. 버튼 레이아웃 컨테이너 (세로 정렬)
        GameObject buttonGroup = new GameObject("ButtonGroup", typeof(RectTransform));
        buttonGroup.transform.SetParent(targetCanvas.transform, false);
        RectTransform groupRt = buttonGroup.GetComponent<RectTransform>();
        groupRt.anchorMin = new Vector2(0.5f, 0.2f);
        groupRt.anchorMax = new Vector2(0.5f, 0.2f);
        groupRt.anchoredPosition = Vector2.zero;

        // 버튼 3개 배치 (너비 70%인 약 760px, 높이 110px. 여백은 160px 간격)
        CreateCyberpunkButton("Btn_Start", buttonGroup.transform, startButtonText, new Vector2(0f, 160f), new Color(0.1f, 0.5f, 0.9f, 0.9f), OnStartClicked);
        CreateCyberpunkButton("Btn_Settings", buttonGroup.transform, settingsButtonText, new Vector2(0f, 0f), new Color(0.3f, 0.3f, 0.35f, 0.9f), OnSettingsClicked);
        CreateCyberpunkButton("Btn_Quit", buttonGroup.transform, quitButtonText, new Vector2(0f, -160f), new Color(0.8f, 0.2f, 0.3f, 0.9f), OnQuitClicked);
    }

    private void CreateCyberpunkButton(string objName, Transform parent, string label, Vector2 pos, Color neonColor, UnityEngine.Events.UnityAction onClick)
    {
        // 버튼 본체
        GameObject btnGo = new GameObject(objName, typeof(RectTransform), typeof(Image), typeof(Button), typeof(TitleUIButton), typeof(Shadow));
        btnGo.transform.SetParent(parent, false);

        RectTransform btnRt = btnGo.GetComponent<RectTransform>();
        btnRt.sizeDelta = new Vector2(760f, 110f); // 넓고 큰 터치 영역
        btnRt.anchoredPosition = pos;

        Image btnImg = btnGo.GetComponent<Image>();
        // Assign a default sprite so the color tinting works correctly instead of appearing as a solid white block
        btnImg.sprite = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        btnImg.type = Image.Type.Sliced;
        btnImg.color = neonColor;

        // 글로우 효과용 그림자
        Shadow cyberShadow = btnGo.GetComponent<Shadow>();
        cyberShadow.effectColor = new Color(neonColor.r, neonColor.g, neonColor.b, 0.5f);
        cyberShadow.effectDistance = new Vector2(4f, -4f);

        Button btn = btnGo.GetComponent<Button>();
        btn.onClick.AddListener(onClick);
        // TitleUIButton이 호버 효과 등 처리함

        // 데코 바 (사이버펑크 느낌의 왼쪽 포인트 라인)
        GameObject decoLine = new GameObject("DecoLine", typeof(RectTransform), typeof(Image));
        decoLine.transform.SetParent(btnGo.transform, false);
        RectTransform decoRt = decoLine.GetComponent<RectTransform>();
        decoRt.anchorMin = new Vector2(0f, 0f);
        decoRt.anchorMax = new Vector2(0f, 1f);
        decoRt.pivot = new Vector2(0f, 0.5f);
        decoRt.sizeDelta = new Vector2(10f, 0f); // 10px 굵기
        decoLine.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.8f);

        // 버튼 텍스트
        GameObject txtGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtGo.transform.SetParent(btnGo.transform, false);
        SetFullScreenStretch(txtGo.GetComponent<RectTransform>());

        TextMeshProUGUI tmp = txtGo.GetComponent<TextMeshProUGUI>();
        
        // Workaround: TextMeshPro replaces missing \u0020 space with \u0003 (End of Text) which clips the string.
        // We use rich text <space=xx> instead to force a visual gap without needing the actual space glyph.
        tmp.text = label.Replace(" ", "<space=0.4em>");
        
        if (customFont != null) tmp.font = customFont;
        tmp.fontSize = 40f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.verticalAlignment = VerticalAlignmentOptions.Middle;
        tmp.enableWordWrapping = false;
        tmp.enableAutoSizing = false;
        tmp.overflowMode = TextOverflowModes.Overflow;
    }

    private void SetFullScreenStretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    // ── 버튼 핸들러 ──────────────────────────────────────────

    private void OnStartClicked()
    {
        Debug.Log("[TitleManager] 게임 시작 클릭 -> AsyncSceneLoader 실행");
        startButtonEvent?.RaiseEvent();
        LoadGameScene();
    }

    private void OnSettingsClicked()
    {
        Debug.Log("[TitleManager] 설정 클릭 (현재 더미 로그)");
        settingsButtonEvent?.RaiseEvent();
    }

    private void OnQuitClicked()
    {
        Debug.Log("[TitleManager] 종료 클릭");
        quitButtonEvent?.RaiseEvent();
        QuitGame();
    }

    // ── Public 메서드 ─────────────────────────────────────────

    public void LoadGameScene()
    {
        if (string.IsNullOrEmpty(gameSceneName))
        {
            Debug.LogError("[TitleManager] gameSceneName이 비어 있습니다.");
            return;
        }

        // 비동기 씬 로드로 권장된 방식 적용 (AsyncSceneLoader.Instance.LoadSceneAsync)
        // AsyncSceneLoader가 없으면 SceneManager 사용
        var sceneLoaderType = System.Type.GetType("AsyncSceneLoader");
        if (sceneLoaderType != null)
        {
            var instanceProp = sceneLoaderType.GetProperty("Instance");
            if (instanceProp != null)
            {
                var instance = instanceProp.GetValue(null);
                if (instance != null)
                {
                    var loadMethod = sceneLoaderType.GetMethod("LoadSceneAsync", new System.Type[] { typeof(string), typeof(LoadSceneMode) });
                    if (loadMethod != null)
                    {
                        loadMethod.Invoke(instance, new object[] { gameSceneName, LoadSceneMode.Single });
                        return; // 성공적으로 로드됨
                    }
                }
            }
        }

        // 폴백
        Debug.LogWarning("[TitleManager] AsyncSceneLoader.Instance를 찾지 못해 동기식으로 전환 (SceneManager 사용)");
        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
