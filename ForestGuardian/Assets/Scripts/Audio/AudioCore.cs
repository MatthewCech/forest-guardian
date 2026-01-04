using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Loam;

namespace forest
{
    /// <summary>
    /// Place to bundle audio
    /// </summary>
    public class AudioCore : MonoBehaviour
    {
        [SerializeField] private AudioLookup audioLookup;

        private List<AudioPlaybackData> playingSources = new List<AudioPlaybackData>();
        private static long nextId = 0;

        public void Awake()
        {
            audioLookup.Initialize();
            Core.Instance.TryRegisterAudioCore(this);

            // We don't need to subscribe, we're here forever
            Postmaster.Instance.Subscribe<MsgAudioTryChangeVolume>(TryChangeVolume);
        }

        public float GetVolume(AudioType type)
        {
            switch (type)
            {
                case AudioType.MUSIC:
                    return Core.Instance.DataSerializer.VolumeMusic;
                case AudioType.EFFECT_GENERAL:
                    return Core.Instance.DataSerializer.VolumeSFX;
                case AudioType.UNSET:
                default:
                    throw new System.Exception("Absolutely not. You may not request the volume of an invalid category, fix the method call.");
            }
        }

        private void TryChangeVolume(Message raw)
        {
            MsgAudioTryChangeVolume msg = raw as MsgAudioTryChangeVolume;

            if(msg.audioType == AudioType.UNSET)
            {
                Debug.LogError("Attmepting to change the volume of 'default' audio type, this isn't supported and means something probably wasn't initialized properly.");
                return;
            }

            float newVolume = Mathf.Clamp01(msg.requestedVolume);

            switch (msg.audioType)
            {
                case AudioType.MUSIC:
                    Core.Instance.DataSerializer.VolumeMusic = newVolume;
                    break;
                case AudioType.EFFECT_GENERAL:
                    Core.Instance.DataSerializer.VolumeSFX = newVolume;
                    break;
            }

            SetAllOfCategory(msg.audioType, newVolume);
            Postmaster.Instance.Send(new MsgAudioVolumeChanged { audioType = msg.audioType, volume = newVolume });
        }

        /// <summary>
        /// Go through and find any matching types, and set their volume accordingly.
        /// </summary>
        /// <param name="type">Type to target from existing sources list</param>
        /// <param name="newVolume">Assumed to be 0 to 1</param>
        private void SetAllOfCategory(AudioType type, float newVolume)
        {
            foreach(AudioPlaybackData playing in playingSources)
            {
                if(playing.type == type)
                {
                    playing.source.volume = newVolume;
                }
            }
        }

        public void Upkeep()
        {
            for(int i = playingSources.Count - 1; i >= 0; --i)
            {
                AudioPlaybackData cur = playingSources[i];
                if (!cur.source.isPlaying)
                {
                    Destroy(cur.source);
                    playingSources.RemoveAt(i);
                }
            }
        }

        public bool TryStop(long audioHandle)
        {
            for (int i = playingSources.Count - 1; i >= 0; --i)
            {
                AudioPlaybackData cur = playingSources[i];
                if (cur.id == audioHandle)
                {
                    cur.source.Stop();
                    Destroy(cur.source);
                    playingSources.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        public bool TryPlay(AudioTag tagToPlay, bool isLooping, out long playbackID)
        {
            if (!audioLookup.TryGetAudioWithTag(tagToPlay, out List<AudioFileData> pairs))
            {
                playbackID = 0;
                return false;
            }

            AudioFileData selectedData = pairs[Random.Range(0, pairs.Count)];
            AudioType typeToUse = selectedData.type;

            AudioClip toPlay = selectedData.audioClip;
            AudioSource newSource = this.gameObject.AddComponent<AudioSource>();
            newSource.clip = selectedData.audioClip;
            newSource.Play();
            newSource.loop = isLooping;

            switch (selectedData.type)
            {
                case AudioType.MUSIC:
                    newSource.volume = Core.Instance.DataSerializer.VolumeMusic;
                    break;
                case AudioType.EFFECT_GENERAL:
                    newSource.volume = Core.Instance.DataSerializer.VolumeSFX;
                    break;
                case AudioType.UNSET:
                default:
                    Debug.LogError($"Unexpected audio type found for tag {tagToPlay} with data {selectedData.audioClip?.name}. Treating it as music for category and volume purposes, but this NEEDS to be fixed.");
                    newSource.volume = Core.Instance.DataSerializer.VolumeMusic;
                    typeToUse = AudioType.MUSIC;
                    break;
            }

            AudioPlaybackData playback = new AudioPlaybackData();
            playback.source = newSource;
            playback.id = ++nextId;
            playback.type = typeToUse;

            playingSources.Add(playback);
            playbackID = playback.id;
            return true;
        }
    }
}