using UnityEngine;

namespace ManagerSystem.SaveLoad
{
    public class PlayerSettings
    {
        public float BGMVolume { get; set; } = 1.0f;
        public float SFXVolume { get; set; } = 1.0f;
        public bool ReverseTouch { get; set; } = false;
        public bool ReverseUI { get; set; } = false;
    }

    public class PlayerSettingsController
    {
        public PlayerSettings Data { get; private set; } = new PlayerSettings();

        public void LoadSettings()
        {
            // PlayerPrefs에서 설정을 불러오는 로직
            Data.BGMVolume = PlayerPrefs.GetFloat("BGMVolume", 1.0f);
            Data.SFXVolume = PlayerPrefs.GetFloat("SFXVolume", 1.0f);
            Data.ReverseTouch = PlayerPrefs.GetInt("ReverseTouch", 0) == 1;
            Data.ReverseUI = PlayerPrefs.GetInt("ReverseUI", 0) == 1;

            SaveSettings();
        }

        public void SaveSettings()
        {
            // PlayerPrefs에 설정을 저장하는 로직
            PlayerPrefs.SetFloat("BGMVolume", Data.BGMVolume);
            PlayerPrefs.SetFloat("SFXVolume", Data.SFXVolume);
            PlayerPrefs.SetInt("ReverseTouch", Data.ReverseTouch ? 1 : 0);
            PlayerPrefs.SetInt("ReverseUI", Data.ReverseUI ? 1 : 0);
            PlayerPrefs.Save();
        }

        public void ChangesBGMVolume(float volume)
        {
            Data.BGMVolume = Mathf.Clamp(volume, 0f, 1f);
            SaveSettings();
        }

        public void ChangesSFXVolume(float volume)
        {
            Data.SFXVolume = Mathf.Clamp(volume, 0f, 1f);
            SaveSettings();
        }

        public void ChangesTouchReverse(bool isReversed)
        {
            Data.ReverseTouch = isReversed;
            SaveSettings();
        }

        public void ChangesUIReverse(bool isReversed)
        {
            Data.ReverseUI = isReversed;
            SaveSettings();

            Managers.InGame?.GameUI?.InGameUI?.SetUIPosition(isReversed);
        }
    }
}