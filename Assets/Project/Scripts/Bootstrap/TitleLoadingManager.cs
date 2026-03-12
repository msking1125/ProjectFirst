using UnityEngine;
using UnityEngine.UIElements;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;

namespace ProjectFirst.Bootstrap
{
    /// <summary>
    /// 타이틀 씬에서 에셋 동적 로딩과 씬 전환 시 진행률을 시각화합니다.
    /// UI Toolkit 배경과 프로그레스 바를 제어합니다.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class TitleLoadingManager : MonoBehaviour
    {
        [Header("Scene Settings")]
        [SerializeField] private string _targetSceneName = "Lobby";
        
        private VisualElement _progressBar;
        private Label _loadingLog;
        private VisualElement _loadingContainer;
        private bool _isLoading;

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            if (root == null) return;

            _loadingContainer = root.Q<VisualElement>("loading-container");
            _progressBar = root.Q<VisualElement>("progress-bar");
            _loadingLog = root.Q<Label>("loading-log");

            // 초기 상태는 로딩 바 0% (로딩 중이 아닐 때 처리)
            if (_loadingContainer != null) 
                _loadingContainer.style.display = DisplayStyle.None;
            
            if (_progressBar != null) 
                _progressBar.style.width = new Length(0, LengthUnit.Percent);
        }

        /// <summary>
        /// 타이틀 UI의 게임 시작 버튼 이벤트가 호출될 때 진행률 표시와 함께 씬 로드를 시작합니다.
        /// </summary>
        public void TriggerLoad()
        {
            if (_isLoading) return;
            
            _isLoading = true;
            if (_loadingContainer != null)
                _loadingContainer.style.display = DisplayStyle.Flex;

            StartLoadingAsync().Forget();
        }

        private async UniTaskVoid StartLoadingAsync()
        {
            if (_loadingLog != null) _loadingLog.text = "자산 로딩 중...";
            if (_progressBar != null) _progressBar.style.width = new Length(0, LengthUnit.Percent);

            // Addressables를 통한 씬 비동기 로드
            var handle = Addressables.LoadSceneAsync(_targetSceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
            
            while (!handle.IsDone)
            {
                float progress = handle.PercentComplete * 100f;
                if (_progressBar != null) _progressBar.style.width = new Length(progress, LengthUnit.Percent);
                if (_loadingLog != null) _loadingLog.text = $"자산 로딩 중... {progress:F0}%";
                await UniTask.Yield();
            }

            if (_loadingLog != null) _loadingLog.text = "로딩 완료! 이동 중...";
            if (_progressBar != null) _progressBar.style.width = new Length(100, LengthUnit.Percent);
            
            await UniTask.Delay(300);
            Debug.Log($"[TitleLoadingManager] {_targetSceneName} 씬으로 성공적으로 이동했습니다.");
        }
    }
}
