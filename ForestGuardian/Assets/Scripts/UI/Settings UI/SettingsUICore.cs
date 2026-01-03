using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Loam;

namespace forest
{
    public class SettingsUICore : MonoBehaviour
    {
        [Header("Audio")]
        [SerializeField] private Slider sliderMusic;
        [SerializeField] private TMPro.TextMeshProUGUI sliderMusicValue;
        [SerializeField] private Slider sliderSFX;
        [SerializeField] private TMPro.TextMeshProUGUI sliderSFXValue;
        [Space]
        [SerializeField] private Button clearAllData;

        MessageSubscription subSetVolumeSlider;

        // Start is called before the first frame update
        void OnEnable()
        {
            subSetVolumeSlider = Postmaster.Instance.Subscribe<MsgAudioVolumeChanged>(SetVolumeSlider);

            sliderMusic.onValueChanged.AddListener(RequestNewMusicVolume);
            sliderSFX.onValueChanged.AddListener(RequestNewSFXVolume);
        }

        // Update is called once per frame
        void OnDisable()
        {
            sliderSFX.onValueChanged.RemoveListener(RequestNewSFXVolume);
            sliderMusic.onValueChanged.RemoveListener(RequestNewMusicVolume);

            subSetVolumeSlider.Dispose();
        }

        void RequestNewMusicVolume(float newValue)
        {
            float range = (sliderMusic.maxValue - sliderMusic.minValue);
            float newVolume = newValue / range;
            Postmaster.Instance.Send(new MsgAudioTryChangeVolume
            {
                audioType = AudioType.MUSIC,
                requestedVolume = newValue
            });
        }

        void RequestNewSFXVolume(float newValue)
        {
            float range = (sliderSFX.maxValue - sliderSFX.minValue);
            float newVolume = newValue / range;
            Postmaster.Instance.Send(new MsgAudioTryChangeVolume
            {
                audioType = AudioType.EFFECT_GENERAL,
                requestedVolume = newValue
            });
        }

        void SetVolumeSlider(Message raw)
        {
            MsgAudioVolumeChanged msg = raw as MsgAudioVolumeChanged;
            string volumeDisplay = $"{Mathf.RoundToInt(msg.volume * 100)}%";
            switch (msg.audioType)
            {
                case AudioType.MUSIC:
                    int newMusicVolume = Mathf.RoundToInt(msg.volume * (sliderMusic.maxValue - sliderMusic.minValue));
                    sliderMusic.SetValueWithoutNotify(newMusicVolume);
                    sliderMusicValue.text = volumeDisplay;
                    break;
                case AudioType.EFFECT_GENERAL:
                    int newEffectGeneralVolume = Mathf.RoundToInt(msg.volume * (sliderSFX.maxValue - sliderSFX.minValue));
                    sliderSFX.SetValueWithoutNotify(newEffectGeneralVolume);
                    sliderSFXValue.text = volumeDisplay;
                    break;
            }
        }
    }
}