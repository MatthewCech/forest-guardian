using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Loam;

namespace forest
{
    [MessageMetadata(
    friendlyName: "Request New Audio Volume",
    description: "Issues a request for a new SFX volume, measured 0 to 1",
    isVisible: true)]
    public class MsgAudioTryChangeVolume : Loam.Message
    {
        public AudioType audioType;
        public float requestedVolume;
    }

    [MessageMetadata(
    friendlyName: "Audio Volume Changed",
    description: "Some audio volume was changed! The new volume is measured 0 to 1",
    isVisible: true)]
    public class MsgAudioVolumeChanged : Loam.Message
    {
        public AudioType audioType;
        public float volume;
    }
}