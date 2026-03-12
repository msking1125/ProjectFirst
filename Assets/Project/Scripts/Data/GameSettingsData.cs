using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace ProjectFirst.Data
{
#if ODIN_INSPECTOR
    [CreateAssetMenu(menuName = "Soul Ark/Settings Data", fileName = "GameSettingsData")]
#else
    [CreateAssetMenu(menuName = "Game/Settings Data", fileName = "GameSettingsData")]
#endif
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

#if ODIN_INSPECTOR
        [Title("사운드 설정", TitleAlignment = TitleAlignments.Left)]
        [HorizontalGroup("볼륨", 0.5f)]
        [BoxGroup("볼륨/BGM")]
        [LabelText("BGM 볼륨")]
        [ProgressBar(0, 100, ColorGetter = "GetBgmColor")]
        [SuffixLabel("%", true)]
#endif
        [Range(0f, 100f)]
        public float bgmVolume = 80f;

#if ODIN_INSPECTOR
        [HorizontalGroup("볼륨", 0.5f)]
        [BoxGroup("볼륨/SFX")]
        [LabelText("SFX 볼륨")]
        [ProgressBar(0, 100, ColorGetter = "GetSfxColor")]
        [SuffixLabel("%", true)]
#endif
        [Range(0f, 100f)]
        public float sfxVolume = 80f;

#if ODIN_INSPECTOR
        [HorizontalGroup("뮤트", 0.33f)]
        [BoxGroup("뮤트/BGM")]
        [LabelText("BGM 뮤트")]
        [ToggleLeft]
#endif
        public bool bgmMute;

#if ODIN_INSPECTOR
        [HorizontalGroup("뮤트", 0.33f)]
        [BoxGroup("뮤트/SFX")]
        [LabelText("SFX 뮤트")]
        [ToggleLeft]
#endif
        public bool sfxMute;

#if ODIN_INSPECTOR
        [HorizontalGroup("뮤트", 0.34f)]
        [BoxGroup("뮤트/전체")]
        [LabelText("전체 뮤트")]
        [ToggleLeft]
        [GUIColor(1f, 0.4f, 0.4f)]
#endif
        public bool globalMute;

#if ODIN_INSPECTOR
        [Title("그래픽 설정", TitleAlignment = TitleAlignments.Left)]
        [HorizontalGroup("그래픽", 0.5f)]
        [BoxGroup("그래픽/품질")]
        [LabelText("프레임 품질")]
        [EnumToggleButtons]
#endif
        [Range(0, 2)]
        public int frameQuality = 1;

#if ODIN_INSPECTOR
        [HorizontalGroup("효과", 0.33f)]
        [BoxGroup("효과/흔들림")]
        [LabelText("화면 흔들림")]
        [ToggleLeft]
#endif
        public bool shake = true;

#if ODIN_INSPECTOR
        [HorizontalGroup("효과", 0.33f)]
        [BoxGroup("효과/블룸")]
        [LabelText("블룸")]
        [ToggleLeft]
#endif
        public bool bloom = true;

#if ODIN_INSPECTOR
        [HorizontalGroup("효과", 0.34f)]
        [BoxGroup("효과/블러")]
        [LabelText("블러")]
        [ToggleLeft]
#endif
        public bool blur;

#if ODIN_INSPECTOR
        private static Color GetBgmColor() => new Color(0.3f, 0.7f, 1f);
        private static Color GetSfxColor() => new Color(1f, 0.6f, 0.2f);
#endif

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