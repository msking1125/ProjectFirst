using UnityEngine;
using UnityEngine.UIElements;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;

namespace ProjectFirst.Bootstrap
{
    /// <summary>
    /// Documentation cleaned.
    /// Documentation cleaned.
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

            // Note: cleaned comment.
            if (_loadingContainer != null) 
                _loadingContainer.style.display = DisplayStyle.None;
            
            if (_progressBar != null) 
                _progressBar.style.width = new Length(0, LengthUnit.Percent);
        }

        /// <summary>
        /// Documentation cleaned.
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

            // Note: cleaned comment.
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
            Debug.Log("[Log] Message cleaned.");
        }
    }
}
