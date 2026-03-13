using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

namespace Project
{
    /// <summary>
    /// UI Toolkit 한글 폰트 관리자.
    ///
    /// 역할:
    /// 1. TextCore FontAsset(HeirofLightBold SDF_2)을 DontDestroyOnLoad 오브젝트에서 참조해
    ///    빌드 중 GC/씬 전환으로 폰트 에셋이 파괴되지 않도록 보장합니다.
    /// 2. 씬 로드 직후 한글 음절(AC00-D7A3) + ASCII 영역을 FontAsset의 동적 아틀라스에
    ///    미리 추가(PreWarm)해, 첫 렌더링 시 글리프 누락 없이 한글이 표시되도록 합니다.
    ///
    /// 사용법:
    /// - 어떤 씬에든 빈 GameObject에 이 컴포넌트를 추가하세요.
    ///   DontDestroyOnLoad이므로 한 씬에만 있어도 전체 씬에서 동작합니다.
    /// - koreanFontAsset 필드에 Assets/Project/Fonts/HeirofLightBold SDF_2.asset 을 할당하세요.
    ///
    /// 주의:
    /// - SDF_2.asset이 1024x1024 아틀라스이면 전체 한글(11,172자)을 담을 수 없습니다.
    ///   TextCore는 아틀라스가 꽉 차면 Overflow 아틀라스를 자동 생성하므로
    ///   메모리가 충분하면 동작하지만, 에디터에서 4096x4096 Static 아틀라스로
    ///   재생성하는 것을 강력히 권장합니다(가이드 파일 참조).
    /// </summary>
    [DefaultExecutionOrder(-200)]
    public class UIFontManager : MonoBehaviour
    {
        private static UIFontManager instance;

        [Header("Korean Font Asset (TextCore)")]
        [Tooltip("Assets/Project/Fonts/HeirofLightBold SDF_2.asset 을 할당하세요.")]
        [SerializeField] private FontAsset koreanFontAsset;

        [Header("PreWarm Settings")]
        [Tooltip("씬 로드 시 한글 음절 전체를 미리 아틀라스에 추가할지 여부.\n" +
                 "true = 첫 화면 진입 시 약간의 딜레이가 생기지만 이후 모든 한글이 즉시 표시됩니다.\n" +
                 "false = 필요할 때마다 동적으로 추가됩니다(일부 글자가 첫 프레임에 누락될 수 있음).")]
        [SerializeField] private bool preWarmOnSceneLoad = true;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);

            if (koreanFontAsset == null)
            {
                Debug.LogWarning("[UIFontManager] koreanFontAsset이 할당되지 않았습니다. " +
                                 "HeirofLightBold SDF_2.asset을 Inspector에서 할당해주세요.");
                return;
            }

            if (preWarmOnSceneLoad)
                StartCoroutine(PreWarmCoroutine());
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (koreanFontAsset == null || !preWarmOnSceneLoad) return;
            StartCoroutine(PreWarmCoroutine());
        }

        /// <summary>
        /// 한글 음절 전체(AC00-D7A3)와 ASCII(20-7E)를 FontAsset 동적 아틀라스에 미리 추가합니다.
        /// 프레임을 1회 양보한 뒤 실행해 UI 초기화가 완료된 이후에 적용됩니다.
        /// </summary>
        private IEnumerator PreWarmCoroutine()
        {
            yield return null; // UI 초기화 완료 대기

            if (koreanFontAsset == null) yield break;

            // 이미 필요한 문자가 충분히 있으면 스킵 (최소 2,000자 이상이면 충분하다고 판단)
            if (koreanFontAsset.characterTable != null && koreanFontAsset.characterTable.Count >= 2000)
                yield break;

            yield return null;

            // ASCII + 한글 음절 전체 구성
            var sb = new StringBuilder(11172 + 95);
            for (int c = 0x20; c <= 0x7E; c++)
                sb.Append((char)c);
            for (int c = 0xAC00; c <= 0xD7A3; c++)
                sb.Append((char)c);

            string chars = sb.ToString();
            bool added = koreanFontAsset.TryAddCharacters(chars, out string missing);

            if (!string.IsNullOrEmpty(missing))
                Debug.LogWarning($"[UIFontManager] 폰트 아틀라스에 추가되지 않은 문자가 있습니다 " +
                                 $"({missing.Length}자). 에디터에서 SDF_2.asset을 " +
                                 "4096x4096 Static으로 재생성하면 해결됩니다.");
        }

        /// <summary>
        /// 외부에서 현재 사용 중인 FontAsset을 가져올 때 사용합니다.
        /// </summary>
        public static FontAsset KoreanFontAsset =>
            instance != null ? instance.koreanFontAsset : null;
    }
}
