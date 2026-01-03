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

    public enum AudioType : byte
    {
        UNSET = 0,

        MUSIC,
        EFFECT_GENERAL
    }

    [System.Serializable]
    public class AudioFileData
    {
        public AudioTag tag;
        public AudioType type;
        public AudioClip audioClip;
        public string artist;
        public string notes;
        public string link;

        public override string ToString()
        {
            return $"name: {audioClip?.name}";
        }
    }

    public class AudioPlaybackData
    {
        public long id;
        public AudioType type;
        public AudioSource source;
    }

    /// <summary>
    /// NOTE: This can be turned into addressables long term.
    /// </summary>
    [CreateAssetMenu(fileName = "Audio Data", menuName = "ScriptableObjects/Audio Lookup Data", order = 2)]
    public class AudioLookup : ScriptableObject
    {
        [SerializeField] private List<AudioFileData> audioPairs = new List<AudioFileData>();

        private Dictionary<AudioTag, List<AudioFileData>> tagLookup = new Dictionary<AudioTag, List<AudioFileData>>();

        public void Initialize()
        {
            tagLookup.Clear();

            foreach (AudioFileData pair in audioPairs)
            {
                if (pair.tag == AudioTag.NONE)
                {
                    Debug.LogWarning($"Untagged pair: {pair}");
                    continue;
                }

                if (!tagLookup.ContainsKey(pair.tag))
                {
                    tagLookup[pair.tag] = new List<AudioFileData>();
                }

                tagLookup[pair.tag].Add(pair);
            }
        }

        public bool TryGetAudioWithTag(AudioTag tag, out List<AudioFileData> audioList)
        {
            return tagLookup.TryGetValue(tag, out audioList);
        }
    }

}