using System.IO;
using UnityEngine;

/// <summary>
/// Battle_Test 씬 진입 시 Assets/Project/Sound/Bgm 폴더에서
/// BGM을 자동으로 찾아 Loop 재생합니다.
///
/// 사용법:
/// 1. 씬 Hierarchy에 빈 오브젝트 생성 → 이름: "BattleSceneBGM"
/// 2. 이 스크립트를 부착
/// 3. Inspector에서 Bgm Clip에 음악 파일을 드래그 연결 (권장)
///    또는 비워두면 Resources/Sound/Bgm/ 에서 자동 로드 시도
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class BattleSceneBGM : MonoBehaviour
{
    [Header("BGM 설정")]
    [Tooltip("재생할 BGM 클립을 직접 연결하세요. (비워두면 Resources에서 자동 탐색)")]
    [SerializeField] private AudioClip bgmClip;

    [Tooltip("볼륨 (0~1)")]
    [Range(0f, 1f)]
    [SerializeField] private float volume = 0.7f;

    [Tooltip("페이드 인 시간 (초). 0이면 즉시 재생.")]
    [SerializeField] private float fadeInDuration = 1.5f;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.loop        = true;
        audioSource.playOnAwake = false;
        audioSource.volume      = fadeInDuration > 0f ? 0f : volume;
    }

    private void Start()
    {
        // bgmClip이 없으면 Resources에서 자동 탐색
        if (bgmClip == null)
        {
            bgmClip = Resources.Load<AudioClip>("Sound/Bgm/battle_bgm");
            if (bgmClip == null)
            {
                // Resources/Sound/Bgm/ 폴더 전체 탐색
                AudioClip[] clips = Resources.LoadAll<AudioClip>("Sound/Bgm");
                if (clips != null && clips.Length > 0)
                    bgmClip = clips[0];
            }
        }

        if (bgmClip == null)
        {
            Debug.LogWarning("[BattleSceneBGM] BGM 클립을 찾지 못했습니다.\n" +
                             "방법1: Inspector → Bgm Clip에 음악 파일 드래그\n" +
                             "방법2: 음악 파일을 Assets/Resources/Sound/Bgm/ 에 배치");
            return;
        }

        audioSource.clip = bgmClip;
        audioSource.Play();

        if (fadeInDuration > 0f)
            StartCoroutine(FadeIn());

        Debug.Log($"[BattleSceneBGM] BGM 재생 시작: {bgmClip.name}");
    }

    private System.Collections.IEnumerator FadeIn()
    {
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            audioSource.volume = Mathf.Lerp(0f, volume, elapsed / fadeInDuration);
            yield return null;
        }
        audioSource.volume = volume;
    }

    /// <summary>런타임에서 볼륨 변경</summary>
    public void SetVolume(float v)
    {
        volume = Mathf.Clamp01(v);
        if (audioSource != null)
            audioSource.volume = volume;
    }

    /// <summary>BGM 일시정지</summary>
    public void Pause() => audioSource?.Pause();

    /// <summary>BGM 재개</summary>
    public void Resume() => audioSource?.UnPause();
}
