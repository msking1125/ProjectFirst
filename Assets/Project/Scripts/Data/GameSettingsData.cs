using UnityEngine;

namespace ProjectFirst.Data
{
    [CreateAssetMenu(menuName = "Game/Settings Data", fileName = "GameSettingsData")]
    public class GameSettingsData : ScriptableObject
    {
        private const string KeyBgm = "setting.sound.bgm";
        private const string KeySfx = "setting.sound.sfx";
        private const string KeyMute = "setting.sound.mute";
        private const string KeyBgmVol = "bgmVol";
        private const string KeySfxVol = "sfxVol";
        private const string KeyBgmMute = "bgmMute";
        private const string KeySfxMute = "sfxMute";
        private const string KeyFrame = "frameQuality";
        private const string KeyShake = "shakeOn";
        private const string KeyBloom = "bloomOn";
        private const string KeyBlur = "blurOn";

        [Range(0f, 100f)]
        public float bgmVolume = 80f;

        [Range(0f, 100f)]
        public float sfxVolume = 80f;

        public bool bgmMute;

        public bool sfxMute;

        public bool globalMute;

        [Range(0, 2)]
        public int frameQuality = 1;

        public bool shake = true;

        public bool bloom = true;

        public bool blur;


        public void LoadLegacyPrefs()
        {
            bgmVolume = PlayerPrefs.GetFloat(KeyBgmVol, PlayerPrefs.GetInt(KeyBgm, Mathf.RoundToInt(bgmVolume)));
            sfxVolume = PlayerPrefs.GetFloat(KeySfxVol, PlayerPrefs.GetInt(KeySfx, Mathf.RoundToInt(sfxVolume)));
            bgmMute = PlayerPrefs.GetInt(KeyBgmMute, bgmMute ? 1 : 0) == 1;
            sfxMute = PlayerPrefs.GetInt(KeySfxMute, sfxMute ? 1 : 0) == 1;
            globalMute = PlayerPrefs.GetInt(KeyMute, globalMute ? 1 : 0) == 1;
            frameQuality = Mathf.Clamp(PlayerPrefs.GetInt(KeyFrame, frameQuality), 0, 2);
            shake = PlayerPrefs.GetInt(KeyShake, shake ? 1 : 0) == 1;
            bloom = PlayerPrefs.GetInt(KeyBloom, bloom ? 1 : 0) == 1;
            blur = PlayerPrefs.GetInt(KeyBlur, blur ? 1 : 0) == 1;
        }

        public void SaveLegacyPrefs()
        {
            PlayerPrefs.SetFloat(KeyBgmVol, bgmVolume);
            PlayerPrefs.SetFloat(KeySfxVol, sfxVolume);
            PlayerPrefs.SetInt(KeyBgm, Mathf.RoundToInt(bgmVolume));
            PlayerPrefs.SetInt(KeySfx, Mathf.RoundToInt(sfxVolume));
            PlayerPrefs.SetInt(KeyBgmMute, bgmMute ? 1 : 0);
            PlayerPrefs.SetInt(KeySfxMute, sfxMute ? 1 : 0);
            PlayerPrefs.SetInt(KeyMute, globalMute ? 1 : 0);
            PlayerPrefs.SetInt(KeyFrame, frameQuality);
            PlayerPrefs.SetInt(KeyShake, shake ? 1 : 0);
            PlayerPrefs.SetInt(KeyBloom, bloom ? 1 : 0);
            PlayerPrefs.SetInt(KeyBlur, blur ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}
