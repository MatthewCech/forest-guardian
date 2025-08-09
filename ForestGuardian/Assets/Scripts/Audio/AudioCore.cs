using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace forest
{
    /// <summary>
    /// Place to bundle audio
    /// </summary>
    public class AudioCore : MonoBehaviour
    {
        [SerializeField] private AudioLookup audioLookup;

        private List<AudioPlaybackPair> playingSources = new List<AudioPlaybackPair>();
        private static long nextId = 0;

        public void Awake()
        {
            audioLookup.Initialize();
            Core.Instance.TryRegisterAudioCore(this);
        }

        public void Upkeep()
        {
            for(int i = playingSources.Count - 1; i >= 0; --i)
            {
                AudioPlaybackPair cur = playingSources[i];
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
                AudioPlaybackPair cur = playingSources[i];
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

        public bool TryPlay(AudioTag tagToPlay, bool isLooping, out long pairID)
        {
            if (!audioLookup.TryGetAudioWithTag(tagToPlay, out List<AudioDataPair> pairs))
            {
                pairID = 0;
                return false;
            }

            AudioClip toPlay = pairs[Random.Range(0, pairs.Count)].audioClip;

            AudioSource newSource = this.gameObject.AddComponent<AudioSource>();
            newSource.clip = toPlay;
            newSource.Play();
            newSource.loop = isLooping;

            AudioPlaybackPair pair = new AudioPlaybackPair();
            pair.source = newSource;
            pair.id = ++nextId;

            playingSources.Add(pair);
            pairID = pair.id;
            return true;
        }
    }
}