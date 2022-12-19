using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
namespace ConnectinnoGames.Managers
{
   
    public class SettingsManager : MonoBehaviour
    {
        private static AudioMixer gameAudioMixer;
        private static ConnectinnoGameData gameData;

        private void Start()
        {
            gameAudioMixer = Resources.Load<AudioMixer>("AudioResources/ConnectinnoAudioMixer");
            DontDestroyOnLoad(this.gameObject);

            gameData = SaveManager.LoadData<ConnectinnoGameData>("SaveData");

            gameAudioMixer.SetFloat("Music", gameData.MusicLevel);
            gameAudioMixer.SetFloat("Sound", gameData.SoundLevel);
        }

        public static void SetMusicSettings(bool value)
        {
            if (value)
            {
                gameData.MusicLevel = 0f;
                gameAudioMixer.SetFloat("Music", 0);
            }
            else
            {
                gameData.MusicLevel = -80f;
                gameAudioMixer.SetFloat("Music", -80);
            }
            SaveData();
        }

        public static void SetSoundSettings(bool value)
        {
            if (value)
            {
                gameData.SoundLevel = 0f;  
                gameAudioMixer.SetFloat("Sound", 0);
            }
            else
            {
                gameAudioMixer.SetFloat("Sound", -80);
                gameData.SoundLevel = -80f;
            }
            SaveData();
        }

        public static void SetHapticSettings(bool value)
        {
            if (value)
            {
                Handheld.Vibrate();
                gameData.HapticIsOn = true;
               
            }
            else
            {
                gameData.HapticIsOn = false;
            }
            SaveData();
        }

        //Get Settings
        public static bool GetMusicSettings()
        {
            
            return gameData.MusicLevel == 0;
        }

        public static bool GetSoundSettings()
        {
            return gameData.SoundLevel == 0;
        }

        public static bool GetHapticSettings()
        {
            return gameData.HapticIsOn;
        }

        private static void SaveData()
        {
            SaveManager.SaveData<ConnectinnoGameData>(gameData, "SaveData");
        }
    }
}
