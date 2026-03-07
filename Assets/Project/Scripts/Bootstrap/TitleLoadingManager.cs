using UnityEngine;
using UnityEngine.UIElements;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;

[RequireComponent(typeof(UIDocument))]
public class TitleLoadingManager : MonoBehaviour
{
    [SerializeField] private string targetSceneName = "Lobby";
    
    private VisualElement progressBar;
    private Label loadingLog;
    private Button startButton;
    private VisualElement loadingContainer;

    private void OnEnable()
    {
        Debug.Log("[TitleLoadingManager] OnEnable 호출됨 → 스크립트 정상 작동 확인");

        var root = GetComponent<UIDocument>().rootVisualElement;
        if (root == null) return; // UIDocument가 비활성화된 경우 안전하게 종료

        loadingContainer = root.Q<VisualElement>("loading-container");
        progressBar = root.Q<VisualElement>("progress-bar");
        loadingLog = root.Q<Label>("loading-log");
        startButton = root.Q<Button>("start-button");

        // 로딩 영역 강제 표시
        if (loadingContainer != null) loadingContainer.style.display = DisplayStyle.Flex;
        if (progressBar != null) progressBar.style.width = new Length(0, LengthUnit.Percent);

        if (startButton != null)
        {
            startButton.clicked += OnStartButtonClicked;
            Debug.Log("[TitleLoadingManager] 시작 버튼 이벤트 연결 완료");
        }
        else
        {
            Debug.LogError("[TitleLoadingManager] start-button을 찾지 못함! uxml 확인");
        }
    }

    private void OnDisable()
    {
        if (startButton != null) startButton.clicked -= OnStartButtonClicked;
    }

    private void OnStartButtonClicked()
    {
        Debug.Log("[TitleLoadingManager] 게임 시작 버튼 클릭 → 로딩 시작");
        StartLoadingAsync().Forget();
    }

    private async UniTaskVoid StartLoadingAsync()
    {
        loadingLog.text = "자산 로딩 중...";
        progressBar.style.width = new Length(0, LengthUnit.Percent);

        var handle = Addressables.LoadSceneAsync(targetSceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
        
        while (!handle.IsDone)
        {
            float progress = handle.PercentComplete * 100f;
            progressBar.style.width = new Length(progress, LengthUnit.Percent);
            loadingLog.text = $"자산 로딩 중... {progress:F0}%";
            await UniTask.Yield();
        }

        loadingLog.text = "로딩 완료! 이동 중...";
        progressBar.style.width = new Length(100, LengthUnit.Percent);
        
        await UniTask.Delay(300);
        Debug.Log("[TitleLoadingManager] 로딩 완료 → Lobby 씬으로 이동");
    }
}
