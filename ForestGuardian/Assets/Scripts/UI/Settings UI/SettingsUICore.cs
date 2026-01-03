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

        void OnEnable()
        {
            subSetVolumeSlider = Postmaster.Instance.Subscribe<MsgAudioVolumeChanged>(SetVolumeSlider);

            sliderMusic.onValueChanged.AddListener(RequestNewMusicVolume);
            sliderSFX.onValueChanged.AddListener(RequestNewSFXVolume);

            // The timing on this is potentially strange.
            TryForceUpdateSliderValues();
        }

        private void Start()
        {
            TryForceUpdateSliderValues();
        }

        void TryForceUpdateSliderValues()
        {
            if(Core.Instance == null || Core.Instance.AudioCore == null)
            {
                return;
            }

            SetSliderVolume(sliderMusic, sliderMusicValue, Core.Instance.AudioCore.GetVolume(AudioType.MUSIC));
            SetSliderVolume(sliderSFX, sliderSFXValue, Core.Instance.AudioCore.GetVolume(AudioType.EFFECT_GENERAL));
        }

        void OnDisable()
        {
            sliderSFX.onValueChanged.RemoveListener(RequestNewSFXVolume);
            sliderMusic.onValueChanged.RemoveListener(RequestNewMusicVolume);

            subSetVolumeSlider.Dispose();
        }

        void RequestNewMusicVolume(float newValue)
        {
            float range = (sliderMusic.maxValue - sliderMusic.minValue);
            float targetVolume = newValue / range;
            Postmaster.Instance.Send(new MsgAudioTryChangeVolume
            {
                audioType = AudioType.MUSIC,
                requestedVolume = targetVolume
            });
        }

        void RequestNewSFXVolume(float newValue)
        {
            float range = (sliderSFX.maxValue - sliderSFX.minValue);
            float targetVolume = newValue / range;
            Postmaster.Instance.Send(new MsgAudioTryChangeVolume
            {
                audioType = AudioType.EFFECT_GENERAL,
                requestedVolume = targetVolume
            });
        }

        void SetVolumeSlider(Message raw)
        {
            MsgAudioVolumeChanged msg = raw as MsgAudioVolumeChanged;
            SetVolumeSlider(msg.audioType, msg.volume);
        }

        void SetVolumeSlider(AudioType audioType, float volume01)
        {
            switch (audioType)
            {
                case AudioType.MUSIC:
                    SetSliderVolume(sliderMusic, sliderMusicValue, volume01);
                    break;
                case AudioType.EFFECT_GENERAL:
                    SetSliderVolume(sliderSFX, sliderSFXValue, volume01);
                    break;
            }
        }

        void SetSliderVolume(Slider slider, TMPro.TextMeshProUGUI value, float volume01)
        {
            int newVolume = Mathf.RoundToInt(volume01 * (slider.maxValue - slider.minValue));
            slider.SetValueWithoutNotify(newVolume);

            string volumeDisplay = $"{Mathf.RoundToInt(volume01 * 100)}%";
            value.text = volumeDisplay;
        }
    }
}