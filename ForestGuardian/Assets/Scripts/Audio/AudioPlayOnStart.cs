using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace forest
{
    public class AudioPlayOnStart : MonoBehaviour
    {
        [SerializeField] private AudioTag tagToPlay;
        [SerializeField] private bool isLooping = true;
        [SerializeField] private bool stopOnDestroy = true;

        private long audioHandle;

        void Start()
        {
            if (Core.HasInstance && Core.Instance.audioCore != null)
            {
                Core.Instance.audioCore.TryPlay(tagToPlay, isLooping, out audioHandle);
            }
        }

        private void OnDestroy()
        {
            if (stopOnDestroy && Core.HasInstance && Core.Instance.audioCore != null)
            {
                Core.Instance.audioCore.TryStop(audioHandle);
            }
        }
    }
}