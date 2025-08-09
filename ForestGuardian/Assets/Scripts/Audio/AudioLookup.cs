using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace forest
{
    public enum AudioTag : int
    {
        NONE = 0,

        MUS_Title = 09,
        MUS_Map = 10,
        MUS_Store = 11,

        MUS_Combat = 20,

        SFX_UI = 100,
    }

    [System.Serializable]
    public class AudioDataPair
    {
        public AudioTag tag;
        public AudioClip audioClip;
        public string artist;
        public string notes;
        public string link;

        public override string ToString()
        {
            return $"name: {audioClip?.name}";
        }
    }

    public class AudioPlaybackPair
    {
        public long id;
        public AudioSource source;
    }

    [CreateAssetMenu(fileName = "Audio Data", menuName = "ScriptableObjects/Audio Lookup Data", order = 2)]
    public class AudioLookup : ScriptableObject
    {
        [SerializeField] private List<AudioDataPair> audioPairs = new List<AudioDataPair>();

        private Dictionary<AudioTag, List<AudioDataPair>> tagLookup = new Dictionary<AudioTag, List<AudioDataPair>>();

        public void Initialize()
        {
            tagLookup.Clear();

            foreach (AudioDataPair pair in audioPairs)
            {
                if (pair.tag == AudioTag.NONE)
                {
                    Debug.LogWarning($"Untagged pair: {pair}");
                    continue;
                }

                if (!tagLookup.ContainsKey(pair.tag))
                {
                    tagLookup[pair.tag] = new List<AudioDataPair>();
                }

                tagLookup[pair.tag].Add(pair);
            }
        }

        public bool TryGetAudioWithTag(AudioTag tag, out List<AudioDataPair> audioList)
        {
            return tagLookup.TryGetValue(tag, out audioList);
        }
    }

}