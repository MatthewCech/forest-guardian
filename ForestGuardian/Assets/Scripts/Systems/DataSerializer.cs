using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace forest
{
    public class DataSerializer
    {
        private const string KEY_GAME_INSTANCE_1 = "forestGameInstance1";

        private float volumeMusic = 1;
        private const string volumeMusicKey = "forestVolumeMusic";
        public float VolumeMusic { get { return volumeMusic; } set { SetValue(volumeMusicKey, value, ref volumeMusic); } }

        private float volumeSFX = 1;
        private const string volumeSFXKey = "forestVolumeSFX";
        public float VolumeSFX { get { return volumeSFX; } set { SetValue(volumeSFXKey, value, ref volumeSFX); } }

        public void Initialize()
        {
            GetValue(volumeMusicKey, ref volumeMusic);
            GetValue(volumeSFXKey, ref volumeSFX);
        }

        private void SetValue(string key, float value, ref float variable)
        {
            if (!PlayerPrefs.HasKey(key))
            {
                PlayerPrefs.SetFloat(key, variable);
            }

            if (variable != value)
            {
                variable = value;
                PlayerPrefs.SetFloat(key, value);
            }
        }

        private void GetValue(string key, ref float variable)
        {
            if(!PlayerPrefs.HasKey(key))
            {
                PlayerPrefs.SetFloat(key, variable);
            }

            variable = PlayerPrefs.GetFloat(key);
        }

        private void SaveGameInstance(GameInstance toSave)
        {
            string data = JsonUtility.ToJson(toSave, false);
            PlayerPrefs.SetString(KEY_GAME_INSTANCE_1, data);
        }

        public GameInstance GetOrCreateGameInstance(VisualLookup lookup)
        {
            string data = PlayerPrefs.GetString(KEY_GAME_INSTANCE_1, null);

            if (!string.IsNullOrEmpty(data))
            {
                GameInstance parsedInstance = JsonUtility.FromJson<GameInstance>(data);
                return parsedInstance;
            }
            else
            {
                GameInstance newInstance = new GameInstance();
                newInstance.PopulateDefaults(lookup);
                SaveGameInstance(newInstance);
                return newInstance;
            }
        }

        public void DestroySavedGameInstance()
        {
            PlayerPrefs.DeleteKey(KEY_GAME_INSTANCE_1);
        }
    }
}