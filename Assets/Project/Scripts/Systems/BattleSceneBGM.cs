using UnityEngine;

namespace Project
{
    [RequireComponent(typeof(AudioSource))]
    public class BattleSceneBGM : MonoBehaviour
    {
        [Header("BGM Settings")]
        [SerializeField] private AudioClip bgmClip;
        [Range(0f, 1f)]
        [SerializeField] private float volume = 0.7f;
        [SerializeField] private float fadeInDuration = 1.5f;

        private AudioSource audioSource;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            audioSource.loop = true;
            audioSource.playOnAwake = false;
            audioSource.volume = fadeInDuration > 0f ? 0f : volume;
        }

        private void Start()
        {
            if (bgmClip == null)
            {
                bgmClip = Resources.Load<AudioClip>("Sound/Bgm/battle_bgm");
                if (bgmClip == null)
                {
                    AudioClip[] clips = Resources.LoadAll<AudioClip>("Sound/Bgm");
                    if (clips != null && clips.Length > 0)
                    {
                        bgmClip = clips[0];
                    }
                }
            }

            if (bgmClip == null)
            {
                Debug.LogWarning("[BattleSceneBGM] No BGM clip could be found.");
                return;
            }

            audioSource.clip = bgmClip;
            audioSource.Play();

            if (fadeInDuration > 0f)
            {
                StartCoroutine(FadeIn());
            }
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

        public void SetVolume(float value)
        {
            volume = Mathf.Clamp01(value);
            if (audioSource != null)
            {
                audioSource.volume = volume;
            }
        }

        public void Pause()
        {
            audioSource?.Pause();
        }

        public void Resume()
        {
            audioSource?.UnPause();
        }
    }
}
